using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace WOLManager
{
    public partial class Form1 : Form
    {
        private List<Computer> computers = new();
        private BindingSource bindingSource = new();
        // Config stored in user profile (%APPDATA%), not in the working directory or Program Files
        private static readonly string CONFIG_FILE = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WOLManager", "computers.json");
        // Legacy path in %APPDATA% (before rename WOLMenager->WOLManager) — migrated once on first run
        private static readonly string LEGACY_APPDATA_CONFIG = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WOLMenager", "computers.json");
        // Oldest legacy path (working directory) — also migrated once
        private const string LEGACY_CONFIG_FILE = "computers.json";
        private System.Windows.Forms.Timer statusTimer;

        public Form1()
        {
            InitializeComponent();
            LoadComputers();
            InitializeComputersList();
            dgvComputers.DataSource = bindingSource;
            dgvComputers.AutoGenerateColumns = true;
            ConfigureDataGridView();

            // Power
            btnWake.Click     += BtnWake_Click;
            btnRestart.Click  += BtnRestart_Click;
            btnShutdown.Click += BtnShutdown_Click;
            // Remote Access
            btnRDP.Click           += BtnRDP_Click;
            btnSSH.Click           += BtnSSH_Click;
            btnNetworkShare.Click  += BtnNetworkShare_Click;
            btnInfo.Click          += BtnInfo_Click;
            // Computers
            btnAdd.Click     += BtnAdd_Click;
            btnAddAuto.Click += BtnAddAuto_Click;
            btnEdit.Click    += BtnEdit_Click;
            btnRemove.Click  += BtnRemove_Click;
            // Misc
            btnHelp.Click    += BtnHelp_Click;
            btnSetName.Click += StatusTimer_Tick;

            dgvComputers.CellDoubleClick += DgvComputers_CellDoubleClick;

            // System tray
            notifyIcon1.Icon = this.Icon;
            menuItemOpen.Font = new System.Drawing.Font(menuItemOpen.Font, System.Drawing.FontStyle.Bold);
            menuItemOpen.Click += (s, e) => RestoreFromTray();
            menuItemExit.Click += (s, e) => { notifyIcon1.Visible = false; Application.Exit(); };
            notifyIcon1.DoubleClick += (s, e) => RestoreFromTray();
            this.Resize += Form1_Resize;

            // Timer auto-refresh
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 10000;
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            FormClosed += (s, e) =>
            {
                statusTimer.Stop();
                statusTimer.Dispose();
                notifyIcon1.Visible = false;
            };

            Task.Run(async () => await CheckAllComputersStatus());
            UpdateStatusBar("Ready - Double-click on computer to wake up, or use buttons on the right");
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(2000, "WOL Manager", "Minimized to tray — double-click to restore", ToolTipIcon.Info);
            }
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
            notifyIcon1.Visible = false;
        }

        // 0 = idle, 1 = running. Prevents concurrent status-check runs
        // (timer + startup + Refresh button) that caused status flickering in the grid.
        private int _statusChecking;

        private async Task CheckAllComputersStatus()
        {
            // Skip if a previous check is still in progress
            if (Interlocked.CompareExchange(ref _statusChecking, 1, 0) != 0)
                return;

            try
            {
                UpdateStatusBar("Checking computers status...");

                // Snapshot the list — it may change while the check is running
                var snapshot = computers.ToList();

                var tasks = snapshot.Select(async computer =>
                {
                    computer.IsOnline = await IsComputerOnline(computer);
                    return computer;
                });

                await Task.WhenAll(tasks);

                // Refresh the grid on the UI thread
                if (InvokeRequired)
                {
                    Invoke(new Action(() => bindingSource.ResetBindings(false)));
                }
                else
                {
                    bindingSource.ResetBindings(false);
                }

                // Podsumowanie statusu
                var onlineCount = snapshot.Count(c => c.IsOnline);
                var totalCount = snapshot.Count;
                UpdateStatusBar($"Status updated: {onlineCount}/{totalCount} computers online");
            }
            finally
            {
                Interlocked.Exchange(ref _statusChecking, 0);
            }
        }

        private void ConfigureDataGridView()
        {
            dgvComputers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvComputers.AllowUserToResizeColumns = true;

            var toolTip = new ToolTip();
            toolTip.SetToolTip(dgvComputers, "Double-click on any computer to wake it up, or use the buttons on the right.");

            if (dgvComputers.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvComputers.Columns)
                {
                    switch (column.Name)
                    {
                        case "Name":        column.FillWeight = 20; break;
                        case "IP":          column.FillWeight = 16; break;
                        case "MacAddress":  column.FillWeight = 20; column.HeaderText = "MAC"; break;
                        case "Broadcast":   column.FillWeight = 14; break;
                        case "Port":        column.FillWeight = 6;  break;
                        case "PingType":    column.FillWeight = 7;  column.HeaderText = "Ping"; break;
                        case "Notes":       column.FillWeight = 22; break;
                        case "IsOnline":    column.FillWeight = 9;  column.HeaderText = "Status"; break;
                        default:            column.FillWeight = 6;  break;
                    }
                }
            }
        }

        private void InitializeComputersList()
        {
            bindingSource.DataSource = computers;
            // Configure columns after data source is bound
            if (computers.Count > 0)
            {
                ConfigureDataGridView();
            }
        }

        private void LoadComputers()
        {
            try
            {
                var path = CONFIG_FILE;
                // Jednorazowa migracja: najpierw stary folder %APPDATA%\WOLMenager, potem katalog roboczy
                if (!File.Exists(path) && File.Exists(LEGACY_APPDATA_CONFIG))
                {
                    path = LEGACY_APPDATA_CONFIG;
                }
                else if (!File.Exists(path) && File.Exists(LEGACY_CONFIG_FILE))
                {
                    path = LEGACY_CONFIG_FILE;
                }

                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    computers = JsonSerializer.Deserialize<List<Computer>>(json) ?? new List<Computer>();
                }
            }
            catch
            {
                computers = new List<Computer>();
            }
        }

        private void SaveComputers()
        {
            try
            {
                var dir = Path.GetDirectoryName(CONFIG_FILE);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var json = JsonSerializer.Serialize(computers, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch (Exception ex)
            {
                // Do not swallow the error silently — user would lose data without knowing
                MessageBox.Show($"Failed to save configuration:\n{ex.Message}", "Save error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Wake-on-LAN: sends the magic packet through EVERY active IPv4 interface separately,
        // targeting both limited broadcast (255.255.255.255) and the configured directed broadcast.
        // This ensures the packet leaves via the real LAN NIC, not a virtual one (Hyper-V/VPN),
        // and does not rely on the network hardware flooding directed broadcasts.
        private void WakeOnLan(Computer computer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(computer.MacAddress))
                    throw new ArgumentException("Brak adresu MAC – nie można wysłać pakietu WOL");

                var mac = computer.MacAddress.Replace(":", "").Replace("-", "").Replace(" ", "");
                if (mac.Length != 12)
                    throw new ArgumentException("Nieprawidłowy format adresu MAC");

                byte[] macBytes = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    macBytes[i] = Convert.ToByte(mac.Substring(i * 2, 2), 16);
                }

                // Magic packet: 6x 0xFF + 16x adres MAC = 102 bajty
                byte[] packet = new byte[102];
                for (int i = 0; i < 6; i++)
                    packet[i] = 0xFF;
                for (int i = 1; i <= 16; i++)
                    Buffer.BlockCopy(macBytes, 0, packet, i * 6, 6);

                int port = computer.Port > 0 ? computer.Port : 9;

                // Adresy docelowe: zawsze limited broadcast, plus skonfigurowany directed broadcast
                var targets = new List<IPEndPoint> { new IPEndPoint(IPAddress.Broadcast, port) };
                if (!string.IsNullOrWhiteSpace(computer.Broadcast)
                    && IPAddress.TryParse(computer.Broadcast, out var dir)
                    && !dir.Equals(IPAddress.Broadcast))
                {
                    targets.Add(new IPEndPoint(dir, port));
                }

                bool sentAny = false;
                foreach (var localIp in GetActiveIPv4Addresses())
                {
                    try
                    {
                        // Bind to a specific NIC so the packet exits through that interface
                        using var client = new UdpClient(new IPEndPoint(localIp, 0)) { EnableBroadcast = true };
                        foreach (var ep in targets)
                            client.Send(packet, packet.Length, ep);
                        sentAny = true;
                    }
                    catch { /* spróbuj kolejną kartę */ }
                }

                // Fallback if binding to any specific NIC failed
                if (!sentAny)
                {
                    using var client = new UdpClient { EnableBroadcast = true };
                    foreach (var ep in targets)
                        client.Send(packet, packet.Length, ep);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WOL packet sending error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Aktywne adresy IPv4 wszystkich kart (bez loopback i APIPA 169.254.x.x)
        private static IEnumerable<IPAddress> GetActiveIPv4Addresses()
        {
            var list = new List<IPAddress>();
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork
                            && !ua.Address.ToString().StartsWith("169.254"))
                        {
                            list.Add(ua.Address);
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        private async Task<bool> IsComputerOnline(Computer computer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(computer.IP)) return false;

                if (string.Equals(computer.PingType, "ICMP", StringComparison.OrdinalIgnoreCase))
                {
                    // Retry up to 2 times — a single dropped ICMP packet should not
                    // flip the host to "offline" (this was causing status flickering)
                    using Ping ping = new();
                    for (int attempt = 0; attempt < 2; attempt++)
                    {
                        try
                        {
                            var reply = await ping.SendPingAsync(computer.IP, 1500);
                            if (reply.Status == IPStatus.Success) return true;
                        }
                        catch { /* spróbuj ponownie */ }
                    }
                    return false;
                }
                else if (string.Equals(computer.PingType, "TCP", StringComparison.OrdinalIgnoreCase))
                {
                    var (host, port) = ParseHostPort(computer.IP, 3389); // default to RDP port
                    return await IsTcpOpen(host, port, 1500);
                }
            }
            catch { }
            
            return false;
        }

        private (string host, int port) ParseHostPort(string hostPort, int defaultPort)
        {
            if (string.IsNullOrWhiteSpace(hostPort)) return ("", defaultPort);

            var lastColon = hostPort.LastIndexOf(':');
            if (lastColon > 0 && lastColon < hostPort.Length - 1)
            {
                var host = hostPort.Substring(0, lastColon);
                var portPart = hostPort.Substring(lastColon + 1);
                if (int.TryParse(portPart, out var port) && port > 0 && port <= 65535)
                {
                    return (host, port);
                }
            }
            return (hostPort, defaultPort);
        }

        private async Task<bool> IsTcpOpen(string host, int port, int timeoutMs)
        {
            try
            {
                using var client = new System.Net.Sockets.TcpClient();
                using var cts = new CancellationTokenSource(timeoutMs);
                await client.ConnectAsync(host, port, cts.Token);
                return client.Connected;
            }
            catch
            {
                return false;
            }
        }

        private async void BtnWake_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is Computer computer)
            {
                await WakeUpComputer(computer);
            }
        }

        private async Task WakeUpComputer(Computer computer)
        {
            try
            {
                WakeOnLan(computer);
                UpdateStatusBar($"WOL packet sent to {computer.Name}");

                MessageBox.Show($"⚡ Wybudzono > {computer.Name}",
                    "Wake-on-LAN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await Task.Delay(3000); // wait 3 s then check if the machine responded
                computer.IsOnline = await IsComputerOnline(computer);
                bindingSource.ResetBindings(false);
                
                if (computer.IsOnline)
                {
                    UpdateStatusBar($"{computer.Name} is online");
                    // Tray balloon — useful when the app is minimized
                    notifyIcon1.BalloonTipTitle = "WOL Manager";
                    notifyIcon1.BalloonTipText = $"{computer.Name} is now online!";
                    notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon1.ShowBalloonTip(4000);
                }
                else
                {
                    UpdateStatusBar($"{computer.Name} is not responding yet");
                }
            }
            catch (Exception ex)
            {
                UpdateStatusBar($"Error waking up {computer.Name}: {ex.Message}");
            }
        }

        private async void DgvComputers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvComputers.Rows[e.RowIndex].DataBoundItem is Computer computer)
            {
                await WakeUpComputer(computer);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using var dialog = new ComputerEditForm();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                computers.Add(dialog.Computer);
                bindingSource.ResetBindings(false);
                SaveComputers();
                UpdateStatusBar($"Computer added: {dialog.Computer.Name}");
            }
        }

        private async void BtnAddAuto_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateStatusBar("Opening network scanner...");
                
                using var scannerForm = new MultipleComputerSelectForm();
                if (scannerForm.ShowDialog(this) == DialogResult.OK)
                {
                    var selectedComputers = scannerForm.SelectedComputers;
                    
                    if (selectedComputers.Count == 0)
                    {
                        UpdateStatusBar("No computers selected");
                        return;
                    }

                    // Show progress form
                    using var progressForm = new BatchAddProgressForm();
                    progressForm.Show(this);
                    
                    UpdateStatusBar($"Adding {selectedComputers.Count} computers...");
                    
                    int addedCount = 0;
                    var errors = new List<string>();
                    var skippedCount = 0;
                    
                    for (int i = 0; i < selectedComputers.Count; i++)
                    {
                        var networkComputer = selectedComputers[i];
                        
                        try
                        {
                            progressForm.UpdateProgress(i + 1, selectedComputers.Count, networkComputer.Name);

                            // Skip if already in list
                            if (computers.Any(c => c.Name.Equals(networkComputer.Name, StringComparison.OrdinalIgnoreCase) ||
                                                 (!string.IsNullOrEmpty(c.IP) && !string.IsNullOrEmpty(networkComputer.IP) && 
                                                  c.IP.Equals(networkComputer.IP, StringComparison.OrdinalIgnoreCase))))
                            {
                                skippedCount++;
                                UpdateStatusBar($"Skipping {networkComputer.Name} - already exists");
                                continue;
                            }

                            var computer = new Computer
                            {
                                Name = networkComputer.Name,
                                IP = networkComputer.IP,
                                Port = 9, // default WOL port
                                MacAddress = networkComputer.MacAddress ?? "",
                                Broadcast = networkComputer.Broadcast ?? CalculateBroadcast(networkComputer.IP),
                                PingType = "ICMP",
                                IsOnline = false
                            };

                            computers.Add(computer);
                            addedCount++;
                            
                            UpdateStatusBar($"Added {computer.Name} ({addedCount}/{selectedComputers.Count})");
                            
                            // Brief delay so the user can see progress
                            await Task.Delay(200);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{networkComputer.Name}: {ex.Message}");
                        }
                    }

                    progressForm.SetCompleted(addedCount, selectedComputers.Count);

                    bindingSource.ResetBindings(false);
                    SaveComputers();

                    // Check status of newly added computers in the background
                    UpdateStatusBar("Checking status of new computers...");
                    _ = Task.Run(async () => await CheckAllComputersStatus());
                    
                    // Podsumowanie
                    var summary = $"✅ Successfully added: {addedCount} computers";
                    if (skippedCount > 0)
                    {
                        summary += $"\n⚠️ Skipped (already exist): {skippedCount} computers";
                    }
                    if (errors.Count > 0)
                    {
                        summary += $"\n❌ Errors: {errors.Count} computers";
                        if (errors.Count <= 3) // show only the first few errors
                        {
                            summary += "\nErrors:\n" + string.Join("\n", errors.Take(3));
                            if (errors.Count > 3)
                            {
                                summary += $"\n... and {errors.Count - 3} more errors";
                            }
                        }
                    }
                    
                    summary += $"\n\nTotal processed: {selectedComputers.Count} computers";
                    
                    MessageBox.Show(summary, "Batch Add Complete", MessageBoxButtons.OK, 
                        errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    
                    UpdateStatusBar($"Batch add completed: {addedCount} added, {skippedCount} skipped, {errors.Count} errors");
                }
                else
                {
                    UpdateStatusBar("Network scan cancelled");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during batch add operation:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatusBar($"Batch add error: {ex.Message}");
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is Computer computer)
            {
                using var dialog = new ComputerEditForm(computer);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int idx = computers.IndexOf(computer);
                    computers[idx] = dialog.Computer;
                    bindingSource.ResetBindings(false);
                    SaveComputers();
                    UpdateStatusBar($"Computer updated: {dialog.Computer.Name}");
                }
            }
            else
            {
                UpdateStatusBar("Select a computer to edit!");
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is Computer computer)
            {
                computers.Remove(computer);
                bindingSource.ResetBindings(false);
                SaveComputers();
                UpdateStatusBar($"Computer removed: {computer.Name}");
            }
            else
            {
                UpdateStatusBar("Select a computer to remove!");
            }
        }

        private void BtnRDP_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is Computer computer)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(computer.IP))
                    {
                        UpdateStatusBar("No IP address for this computer!");
                        return;
                    }

                    // Uruchom  Desktop Connection z parametrem IP
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "mstsc",
                        UseShellExecute = true
                    };
                    processInfo.ArgumentList.Add($"/v:{computer.IP}");

                    using var rdp = Process.Start(processInfo);
                    UpdateStatusBar($"RDP launched for {computer.Name} ({computer.IP})");
                }
                catch (Exception ex)
                {
                    UpdateStatusBar($"Error launching RDP for {computer.Name}: {ex.Message}");
                }
            }
            else
            {
                UpdateStatusBar("Select a computer from the list!");
            }
        }

        // Allows letters, digits and safe SSH target characters (hostname + username).
        // Excludes all shell metacharacters — guards against injection in the cmd.exe fallback
        // (hostname may originate from attacker-controlled DNS).
        private static readonly System.Text.RegularExpressions.Regex _sshTargetRegex =
            new(@"^[a-zA-Z0-9._@%:-]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

        private void BtnSSH_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is not Computer computer)
            {
                UpdateStatusBar("Select a computer from the list!");
                return;
            }

            var host = !string.IsNullOrWhiteSpace(computer.IP) ? computer.IP : computer.Name;
            if (string.IsNullOrWhiteSpace(host))
            {
                UpdateStatusBar("No IP address or hostname for SSH!");
                return;
            }

            var user = computer.SshUser;
            var target = string.IsNullOrWhiteSpace(user) ? host : $"{user}@{host}";

            if (!_sshTargetRegex.IsMatch(target))
            {
                MessageBox.Show(
                    $"Invalid SSH target: \"{target}\"\n\n" +
                    "Host and username may only contain: letters, digits, . - _ @ % :\n" +
                    "(shell metacharacters are blocked for security)",
                    "SSH — Invalid Target", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Attempt 1: Windows Terminal — `wt -- ssh user@host`
                // `--` signals end of wt arguments and start of the terminal command.
                SshLaunchWT(target);
                UpdateStatusBar($"SSH → {target}");
            }
            catch
            {
                try
                {
                    // Attempt 2: cmd.exe /k ssh — classic console window.
                    // target is validated (no cmd metacharacters), interpolation is safe.
                    SshLaunchCmd(target);
                    UpdateStatusBar($"SSH (cmd) → {target}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Cannot launch SSH client.\n\n{ex.Message}\n\n" +
                        "Make sure OpenSSH client is installed:\n" +
                        "Settings → Apps → Optional features → OpenSSH Client",
                        "SSH Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void SshLaunchWT(string target)
        {
            var psi = new ProcessStartInfo { FileName = "wt.exe", UseShellExecute = true };
            psi.ArgumentList.Add("--");
            psi.ArgumentList.Add("ssh");
            psi.ArgumentList.Add(target);
            var proc = Process.Start(psi);
            if (proc == null) throw new InvalidOperationException("wt.exe did not start");
        }

        private static void SshLaunchCmd(string target)
        {
            var psi = new ProcessStartInfo { FileName = "cmd.exe", UseShellExecute = true };
            psi.ArgumentList.Add("/k");
            psi.ArgumentList.Add($"ssh {target}");
            var proc = Process.Start(psi);
            if (proc == null) throw new InvalidOperationException("cmd.exe did not start");
        }

        private void BtnInfo_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is Computer computer)
            {
                if (string.IsNullOrWhiteSpace(computer.IP) && string.IsNullOrWhiteSpace(computer.Name))
                {
                    UpdateStatusBar("No IP address or hostname for this computer!");
                    return;
                }
                UpdateStatusBar($"Opening Info for {computer.Name}...");
                using var infoForm = new ComputerInfoForm(computer);
                infoForm.Icon = this.Icon;
                infoForm.ShowDialog(this);
            }
            else
            {
                UpdateStatusBar("Select a computer from the list!");
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is Computer computer)
                SendPowerCommand(computer, "/r", "Restart");
            else
                UpdateStatusBar("Select a computer from the list!");
        }

        private void BtnShutdown_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is Computer computer)
                SendPowerCommand(computer, "/s", "Shutdown");
            else
                UpdateStatusBar("Select a computer from the list!");
        }

        private void SendPowerCommand(Computer computer, string flag, string label)
        {
            var confirm = MessageBox.Show(
                $"{label} computer \"{computer.Name}\"?\n\nIP: {computer.IP}",
                $"Confirm {label}",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                // Dla zdalnego shutdown Windows wymaga: File and Printer Sharing + admin$ share + prawa admina
                var target = !string.IsNullOrWhiteSpace(computer.Name) ? computer.Name : computer.IP;
                if (string.IsNullOrWhiteSpace(target))
                {
                    MessageBox.Show("No computer name or IP to connect to.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };
                psi.ArgumentList.Add(flag);
                psi.ArgumentList.Add("/m");
                psi.ArgumentList.Add($"\\\\{target}");
                psi.ArgumentList.Add("/t");
                psi.ArgumentList.Add("0");
                psi.ArgumentList.Add("/c");
                psi.ArgumentList.Add($"{label} by WOL Manager");

                using var p = Process.Start(psi);
                var err = p?.StandardError.ReadToEnd() ?? "";
                p?.WaitForExit(3000);

                if (!string.IsNullOrWhiteSpace(err))
                {
                    MessageBox.Show(
                        $"shutdown.exe returned an error:\n{err}\n\n" +
                        "Tip: Remote shutdown requires:\n" +
                        "• File and Printer Sharing enabled on target\n" +
                        "• Target firewall rule \"Remote Shutdown\" active\n" +
                        "• Admin rights on the target machine",
                        $"{label} error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    UpdateStatusBar($"{label} command sent to {computer.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending {label} command:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool OpenNetworkPath(string path)
        {
            try
            {
                // Method 1: direct ShellExecute on UNC path
                var psi = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    ErrorDialog = false,
                    Verb = "open"
                };
                Process.Start(psi);
                return true;
            }
            catch
            {
                // Metoda 2: Explorer z parametrem /select (ArgumentList — brak injection)
                try
                {
                    var explorerProcess = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        UseShellExecute = true
                    };
                    explorerProcess.ArgumentList.Add(path);
                    Process.Start(explorerProcess);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private async void BtnNetworkShare_Click(object sender, EventArgs e)
        {
            if (dgvComputers.CurrentRow?.DataBoundItem is Computer computer)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(computer.IP) && string.IsNullOrWhiteSpace(computer.Name))
                    {
                        UpdateStatusBar("No IP address or computer name!");
                        return;
                    }

                    // Resolve the UNC host (strip port) and determine if it is private (LAN) or public (WAN)
                    string hostForUnc = GetUncTargetHost(computer);
                    bool isPrivate = await IsPrivateHostAsync(hostForUnc);

                    // On LAN with no computer name, prompt the user to enter one
                    string overrideName = null;
                    if (isPrivate && string.IsNullOrWhiteSpace(computer.Name))
                    {
                        using var nameDlg = new ComputerNameInputForm();
                        if (nameDlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(nameDlg.ComputerName))
                        {
                            overrideName = nameDlg.ComputerName.Trim();
                            UpdateStatusBar($"Using entered computer name: {overrideName}");
                        }
                    }

                    // On LAN prefer name (easier SMB auth); on WAN prefer IP/host
                    string preferredName = overrideName ?? computer.Name;
                    string targetAddress = isPrivate
                        ? (!string.IsNullOrWhiteSpace(preferredName) ? preferredName : hostForUnc)
                        : hostForUnc;

                    string networkPath = $"\\\\{targetAddress}";

                    UpdateStatusBar($"Opening resource: {networkPath} for {computer.Name} ({(isPrivate ? "LAN" : "WAN")})");

                    // Try direct connection first
                    if (OpenNetworkPath(networkPath))
                    {
                        UpdateStatusBar($"Network resource opened for {computer.Name}");
                        return;
                    }

                    // Prepare alternative path (swap name and IP/host)
                    string alternativePath = null;
                    if (targetAddress == hostForUnc && !string.IsNullOrWhiteSpace(preferredName))
                    {
                        alternativePath = $"\\\\{preferredName}";
                        UpdateStatusBar($"Trying with computer name: {alternativePath}");

                        if (OpenNetworkPath(alternativePath))
                        {
                            UpdateStatusBar($"Network resource opened via name for {computer.Name}");
                            return;
                        }
                    }
                    else if (targetAddress == preferredName && !string.IsNullOrWhiteSpace(hostForUnc))
                    {
                        alternativePath = $"\\\\{hostForUnc}";
                        UpdateStatusBar($"Trying with host/IP: {alternativePath}");

                        if (OpenNetworkPath(alternativePath))
                        {
                            UpdateStatusBar($"Network resource opened via host/IP for {computer.Name}");
                            return;
                        }
                    }

                    // On LAN, if all attempts failed, let the user enter/correct the name and retry
                    if (isPrivate)
                    {
                        using var retryNameDlg = new ComputerNameInputForm();
                        if (retryNameDlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(retryNameDlg.ComputerName))
                        {
                            var manualNamePath = $"\\\\{retryNameDlg.ComputerName.Trim()}";
                            UpdateStatusBar($"Trying with entered name: {manualNamePath}");
                            if (OpenNetworkPath(manualNamePath))
                            {
                                UpdateStatusBar($"Network resource opened with entered name for {computer.Name}");
                                return;
                            }
                        }
                    }

                    // Still failing — show extended options dialog
                    var ask = MessageBox.Show(
                        $"Cannot open resource automatically.\n" +
                        $"Tried: {networkPath}" +
                        (alternativePath != null ? $" and {alternativePath}" : "") + "\n\n" +
                        "Choose option:\n" +
                        "• YES - Try with login (NET USE)\n" +
                        "• NO - Show manual access instructions\n" +
                        "• CANCEL - Open Network Explorer (check availability)",
                        "Network resource access",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (ask == DialogResult.Cancel)
                    {
                        // Open Explorer at Network section
                        OpenNetworkExplorer();
                        UpdateStatusBar("Network Explorer opened - check available computers");
                        return;
                    }

                    // Build path variants for the manual instructions message
                    var nameOption = !string.IsNullOrWhiteSpace(preferredName) ? $"\\\\{preferredName}" : "no name";
                    var ipOption = $"\\\\{hostForUnc}";

                    if (ask == DialogResult.No)
                    {
                        var instructions = $"MANUAL ACCESS INSTRUCTIONS:\n\n" +
                            $"ADDRESS OPTIONS:\n" +
                            $"• By name (LAN): {nameOption}\n" +
                            $"• By host/IP (WAN): {ipOption}\n\n" +
                            $"1. FASTEST (Win+R):\n" +
                            $"   • Press Win+R\n" +
                            $"   • Type: {(isPrivate ? nameOption : ipOption)}\n" +
                            $"   • Press Enter\n\n" +
                            $"2. VIA EXPLORER:\n" +
                            $"   • Open Explorer (Win+E)\n" +
                            $"   • In address bar type: {(isPrivate ? nameOption : ipOption)}\n" +
                            $"   • Press Enter\n\n" +
                            $"3. VIA NETWORK EXPLORER (LAN):\n" +
                            $"   • Open Explorer → Network (left panel)\n" +
                            $"   • Double-click on the computer name if visible\n\n" +
                            $"4. IF NAME DOESN'T WORK, USE HOST/IP:\n" +
                            $"   • Win+R → {ipOption}\n" +
                            $"   • Provide login credentials when prompted\n\n" +
                            $"5. MAP NETWORK DRIVE:\n" +
                            $"   • Explorer → Right-click on 'This PC'\n" +
                            $"   • 'Map network drive'\n" +
                            $"   • Folder: {(isPrivate ? nameOption : ipOption)}\n" +
                            $"   • Check 'Use different credentials' if needed";

                        MessageBox.Show(instructions, "Manual access instructions", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Copy preferred path to clipboard (LAN = name, WAN = IP)
                        var preferredPath = isPrivate && !string.IsNullOrWhiteSpace(preferredName) ? nameOption : ipOption;
                        try
                        {
                            Clipboard.SetText(preferredPath);
                            UpdateStatusBar($"Copied to clipboard: {preferredPath}");
                        }
                        catch
                        {
                            UpdateStatusBar("Failed to copy to clipboard");
                        }
                        return;
                    }

                    if (ask == DialogResult.Yes)
                    {
                        try
                        {
                            using var credDlg = new NetworkCredentialForm(targetAddress);
                            if (credDlg.ShowDialog(this) == DialogResult.OK)
                            {
                                // Choose NET USE target based on LAN/WAN
                                var connectTarget = isPrivate && !string.IsNullOrWhiteSpace(preferredName) ? nameOption : ipOption;
                                var netProcess = new ProcessStartInfo
                                {
                                    FileName = "net.exe",
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                };
                                // ArgumentList quotes each argument separately — no injection via
                                // special characters in password or username (e.g. quotes)
                                netProcess.ArgumentList.Add("use");
                                netProcess.ArgumentList.Add(connectTarget);
                                netProcess.ArgumentList.Add(credDlg.Password);
                                netProcess.ArgumentList.Add("/user:" + credDlg.Username);
                                netProcess.ArgumentList.Add("/persistent:no");

                                using var p = Process.Start(netProcess);

                                // Read streams BEFORE WaitForExit — a full pipe buffer
                                // would otherwise deadlock the child process
                                var output = p?.StandardOutput.ReadToEnd() ?? "";
                                var error = p?.StandardError.ReadToEnd() ?? "";
                                p?.WaitForExit(5000);

                                // After NET USE attempt, try opening Explorer
                                if (OpenNetworkPath(connectTarget))
                                {
                                    UpdateStatusBar($"Connected to {computer.Name}");
                                }
                                else
                                {
                                    UpdateStatusBar("NET USE executed - check manually");

                                    var result = $"NET USE executed - result:\n\n";
                                    if (!string.IsNullOrEmpty(output)) result += $"OUTPUT: {output}\n";
                                    if (!string.IsNullOrEmpty(error)) result += $"ERROR: {error}\n";
                                    result += $"\nTry now manually via {(isPrivate ? "name" : "host/IP")}:\nWin+R → {(isPrivate && !string.IsNullOrWhiteSpace(preferredName) ? nameOption : ipOption)}";

                                    MessageBox.Show(result, "NET USE result", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // Copy preferred path to clipboard
                                    var preferredPath = isPrivate && !string.IsNullOrWhiteSpace(preferredName) ? nameOption : ipOption;
                                    try
                                    {
                                        Clipboard.SetText(preferredPath);
                                        UpdateStatusBar($"Copied to clipboard: {preferredPath}");
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatusBar($"Error: {ex.Message}");
                }
            }
            else
            {
                UpdateStatusBar("Select a computer from the list!");
            }
        }

        // Returns the host (port stripped) suitable for UNC path, from IP or name
        private string GetUncTargetHost(Computer computer)
        {
            if (!string.IsNullOrWhiteSpace(computer.IP))
            {
                var (host, _) = ParseHostPort(computer.IP, 445);
                return host;
            }
            return computer.Name;
        }

        // Returns true if the host is a private (LAN) address. Resolves names via DNS if needed.
        // Async — synchronous DNS can block the UI for several seconds on a slow/unreachable server.
        private async Task<bool> IsPrivateHostAsync(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) return false;

            if (IPAddress.TryParse(host, out var ip))
            {
                return IsPrivateIpAddress(ip);
            }

            try
            {
                var addrs = await Dns.GetHostAddressesAsync(host);
                return addrs.Any(a => a.AddressFamily == AddressFamily.InterNetwork && IsPrivateIpAddress(a));
            }
            catch
            {
                return false;
            }
        }

        private bool IsPrivateIpAddress(IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork) return false;
            var b = ip.GetAddressBytes();
            // 10.0.0.0/8
            if (b[0] == 10) return true;
            // 172.16.0.0 – 172.31.255.255
            if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true;
            // 192.168.0.0/16
            if (b[0] == 192 && b[1] == 168) return true;
            // Link-local 169.254.0.0/16
            if (b[0] == 169 && b[1] == 254) return true;
            return false;
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            using var helpForm = new HelpForm();
            helpForm.ShowDialog(this);
        }

        private void OpenNetworkExplorer()
        {
            try
            {
                var explorerProcess = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "shell:NetworkPlacesFolder",
                    UseShellExecute = true
                };
                Process.Start(explorerProcess);

                UpdateStatusBar("Network Explorer opened - check available computers");
            }
            catch (Exception ex)
            {
                try
                {
                    var fallbackProcess = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = "::{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}",
                        UseShellExecute = true
                    };
                    Process.Start(fallbackProcess);
                    UpdateStatusBar("Explorer opened (fallback) - find Network section");
                }
                catch
                {
                    try
                    {
                        Process.Start("explorer.exe");
                        UpdateStatusBar("Explorer opened - manually navigate to Network section");
                    }
                    catch
                    {
                        UpdateStatusBar($"Cannot open Explorer: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateStatusBar(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatusBar), message);
            }
            else
            {
                statusLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
            }
        }

        private async void StatusTimer_Tick(object sender, EventArgs e)
        {
            await CheckAllComputersStatus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Reserved for future initialization
        }

        private string CalculateBroadcast(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return "192.168.1.255";
            try
            {
                var parts = ip.Split('.');
                if (parts.Length == 4)
                {
                    return $"{parts[0]}.{parts[1]}.{parts[2]}.255";
                }
            }
            catch { }
            return "192.168.1.255";
        }
    }

    public class Computer
    {
        public string Name { get; set; } = "";
        public string IP { get; set; } = "";
        public int Port { get; set; } = 9;
        public string MacAddress { get; set; } = "";
        public string Broadcast { get; set; } = "";
        public string PingType { get; set; } = "ICMP";
        public string SshUser { get; set; } = "";
        public string Notes { get; set; } = "";
        public bool IsOnline { get; set; }
    }
}
