using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace WOLMenager
{
    public partial class Form1 : Form
    {
        private List<Computer> computers = new();
        private BindingSource bindingSource = new();
        // Konfiguracja przechowywana w profilu użytkownika (%APPDATA%), nie w katalogu roboczym/Program Files
        private static readonly string CONFIG_FILE = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WOLMenager", "computers.json");
        // Stara lokalizacja (katalog roboczy) – używana tylko do jednorazowej migracji
        private const string LEGACY_CONFIG_FILE = "computers.json";
        private System.Windows.Forms.Timer statusTimer;

        public Form1()
        {
            InitializeComponent();
            LoadComputers();
            InitializeComputersList();
            dgvComputers.DataSource = bindingSource;
            dgvComputers.AutoGenerateColumns = true;
            
            // Konfiguracja kolumn DataGridView
            ConfigureDataGridView();
            
            btnWake.Click += BtnWake_Click;
            btnAdd.Click += BtnAdd_Click;
            btnAddAuto.Click += BtnAddAuto_Click;
            btnEdit.Click += BtnEdit_Click;
            btnRemove.Click += BtnRemove_Click;
            btnRDP.Click += BtnRDP_Click;
            btnNetworkShare.Click += BtnNetworkShare_Click;
            btnHelp.Click += BtnHelp_Click;
            // Przypnij przycisk Refresh (dawniej Set Name) do istniejącej metody odświeżania
            btnSetName.Click += StatusTimer_Tick;
            
            // Dodaj obsługę podwójnego kliknięcia na DataGridView
            dgvComputers.CellDoubleClick += DgvComputers_CellDoubleClick;
            
            // Natychmiastowe sprawdzenie stanu przy starcie
            Task.Run(async () => await CheckAllComputersStatus());
            
            // Timer for auto-refresh
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 10000; // 10 seconds
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            // Zwolnij timer przy zamykaniu formularza (unikamy wycieku zasobu)
            FormClosed += (s, e) =>
            {
                statusTimer.Stop();
                statusTimer.Dispose();
            };
            
            // Informacja o podwójnym kliknięciu
            UpdateStatusBar("Ready - Double-click on computer to wake up, or use buttons on the right");
        }

        // 0 = brak sprawdzania, 1 = trwa. Zapobiega nakładaniu się przebiegów
        // (timer + start + przycisk Refresh), które migotały statusem w siatce.
        private int _statusChecking;

        private async Task CheckAllComputersStatus()
        {
            // Pomiń, jeśli poprzednie sprawdzanie jeszcze trwa
            if (Interlocked.CompareExchange(ref _statusChecking, 1, 0) != 0)
                return;

            try
            {
                UpdateStatusBar("Checking computers status...");

                // Zrzut listy na czas przebiegu (lista może się zmienić w trakcie)
                var snapshot = computers.ToList();

                var tasks = snapshot.Select(async computer =>
                {
                    computer.IsOnline = await IsComputerOnline(computer);
                    return computer;
                });

                await Task.WhenAll(tasks);

                // Aktualizuj UI w głównym wątku
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
            
            // Dodaj tooltip informujący o podwójnym kliknięciu
            var toolTip = new ToolTip();
            toolTip.SetToolTip(dgvComputers, "💡 Double-click on any computer to wake it up!\nOr use the 'Wake Up' button on the right.");
            
            // Po załadowaniu danych, ustaw szerokości kolumn
            if (dgvComputers.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvComputers.Columns)
                {
                    switch (column.Name)
                    {
                        case "Name":
                            column.FillWeight = 25;
                            break;
                        case "IP":
                            column.FillWeight = 20;
                            break;
                        case "MacAddress":
                            column.FillWeight = 25;
                            break;
                        case "Broadcast":
                            column.FillWeight = 20;
                            break;
                        case "IsOnline":
                            column.FillWeight = 10;
                            column.HeaderText = "Status";
                            break;
                        default:
                            column.FillWeight = 10;
                            break;
                    }
                }
            }
        }

        private void InitializeComputersList()
        {
            bindingSource.DataSource = computers;
            // Konfiguruj kolumny po przypisaniu danych
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
                // Jednorazowa migracja ze starej lokalizacji (katalog roboczy)
                if (!File.Exists(path) && File.Exists(LEGACY_CONFIG_FILE))
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
                // Nie połykaj błędu po cichu – użytkownik traciłby dane bez ostrzeżenia
                MessageBox.Show($"Failed to save configuration:\n{ex.Message}", "Save error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Wake-on-LAN: wysyła magic packet przez KAŻDĄ aktywną kartę IPv4 osobno,
        // na limited broadcast (255.255.255.255) i skonfigurowany directed broadcast.
        // Dzięki temu pakiet wychodzi realną kartą LAN, a nie np. wirtualną (Hyper-V/VPN),
        // i nie zależy od tego, czy sprzęt sieciowy flooduje directed broadcast.
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
                        // Bind do konkretnej karty => pakiet wychodzi właśnie tą kartą
                        using var client = new UdpClient(new IPEndPoint(localIp, 0)) { EnableBroadcast = true };
                        foreach (var ep in targets)
                            client.Send(packet, packet.Length, ep);
                        sentAny = true;
                    }
                    catch { /* spróbuj kolejną kartę */ }
                }

                // Fallback, gdyby nie udało się powiązać z żadną kartą
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
                    // Ponów do 2 razy – pojedynczy zgubiony pakiet ICMP nie powinien
                    // przełączać hosta na "offline" (to powodowało migotanie statusu)
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
                    var (host, port) = ParseHostPort(computer.IP, 3389); // domyślnie RDP
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

                // Pokaż małe okno informacyjne o wybudzeniu
                MessageBox.Show($"⚡ Wybudzono > {computer.Name}", 
                    "Wake-on-LAN", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);

                // Opcjonalnie: sprawdź czy komputer się włączył po chwili
                await Task.Delay(3000); // czekaj 3 sekundy
                computer.IsOnline = await IsComputerOnline(computer);
                bindingSource.ResetBindings(false);
                
                if (computer.IsOnline)
                {
                    UpdateStatusBar($"{computer.Name} is online");
                }
                else
                {
                    UpdateStatusBar($"{computer.Name} is not responding");
                }
            }
            catch (Exception ex)
            {
                UpdateStatusBar($"Error waking up {computer.Name}: {ex.Message}");
            }
        }

        private async void DgvComputers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Sprawdź czy kliknięto na prawidłowy wiersz (nie na nagłówek)
            if (e.RowIndex >= 0 && dgvComputers.Rows[e.RowIndex].DataBoundItem is Computer computer)
            {
                // Wybudź komputer
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

                    // Pokaż formularz postępu
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
                            // Aktualizuj postęp
                            progressForm.UpdateProgress(i + 1, selectedComputers.Count, networkComputer.Name);
                            
                            // Sprawdź czy komputer już nie istnieje
                            if (computers.Any(c => c.Name.Equals(networkComputer.Name, StringComparison.OrdinalIgnoreCase) ||
                                                 (!string.IsNullOrEmpty(c.IP) && !string.IsNullOrEmpty(networkComputer.IP) && 
                                                  c.IP.Equals(networkComputer.IP, StringComparison.OrdinalIgnoreCase))))
                            {
                                skippedCount++;
                                UpdateStatusBar($"Skipping {networkComputer.Name} - already exists");
                                continue;
                            }

                            // Konwertuj NetworkComputer na Computer
                            var computer = new Computer
                            {
                                Name = networkComputer.Name,
                                IP = networkComputer.IP,
                                Port = 9, // domyślny port WOL
                                MacAddress = networkComputer.MacAddress ?? "",
                                Broadcast = networkComputer.Broadcast ?? CalculateBroadcast(networkComputer.IP),
                                PingType = "ICMP",
                                IsOnline = false
                            };

                            computers.Add(computer);
                            addedCount++;
                            
                            UpdateStatusBar($"Added {computer.Name} ({addedCount}/{selectedComputers.Count})");
                            
                            // Krótka pauza żeby użytkownik widział postęp
                            await Task.Delay(200);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{networkComputer.Name}: {ex.Message}");
                        }
                    }

                    // Zakończ postęp
                    progressForm.SetCompleted(addedCount, selectedComputers.Count);
                    
                    // Odśwież UI i zapisz
                    bindingSource.ResetBindings(false);
                    SaveComputers();
                    
                    // Sprawdź status nowo dodanych komputerów w tle
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
                        if (errors.Count <= 3) // Pokaż tylko pierwsze kilka błędów
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

        private bool OpenNetworkPath(string path)
        {
            try
            {
                // Metoda 1: Bezpośredni ShellExecute na ścieżce UNC (najprostsze)
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
                // Metoda 2: Explorer z parametrem /n (nowe okno)
                try
                {
                    var explorerProcess = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/n,\"{path}\"",
                        UseShellExecute = true
                    };
                    Process.Start(explorerProcess);
                    return true;
                }
                catch
                {
                    // Metoda 3: CMD start z /D (ustawienie katalogu roboczego)
                    try
                    {
                        var cmdProcess = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c start \"Zasób sieciowy\" /D \"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.System)}\" explorer.exe \"{path}\"",
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true
                        };
                        Process.Start(cmdProcess);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        private void BtnNetworkShare_Click(object sender, EventArgs e)
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

                    // Ustal host do UNC (bez portu) i oceń czy to adres prywatny (LAN) czy publiczny (WAN)
                    string hostForUnc = GetUncTargetHost(computer);
                    bool isPrivate = IsPrivateHost(hostForUnc);

                    // Jeśli LAN i brak nazwy komputera, poproś o wpisanie nazwy (ComputerNameInputForm)
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

                    // Dla LAN preferuj nazwę (łatwiejsza autoryzacja SMB), dla WAN preferuj IP/host
                    string preferredName = overrideName ?? computer.Name;
                    string targetAddress = isPrivate
                        ? (!string.IsNullOrWhiteSpace(preferredName) ? preferredName : hostForUnc)
                        : hostForUnc;

                    string networkPath = $"\\\\{targetAddress}";

                    UpdateStatusBar($"Opening resource: {networkPath} for {computer.Name} ({(isPrivate ? "LAN" : "WAN")})");

                    // Najpierw spróbuj bezpośrednio
                    if (OpenNetworkPath(networkPath))
                    {
                        UpdateStatusBar($"Network resource opened for {computer.Name}");
                        return;
                    }

                    // Przygotuj ścieżkę alternatywną (zamiana nazwy i IP/host)
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

                    // Jeżeli LAN i wcześniejsze próby nie powiodły się, pozwól użytkownikowi podać/zmienić nazwę i spróbuj ponownie
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

                    // Jeśli nadal nie działa, pokaż rozszerzone opcje
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
                        // Otwórz Explorer w sekcji Sieć
                        OpenNetworkExplorer();
                        UpdateStatusBar("Network Explorer opened - check available computers");
                        return;
                    }

                    // Przygotuj warianty ścieżek do instrukcji
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

                        // Skopiuj preferowaną ścieżkę do schowka (zależnie od LAN/WAN)
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
                                // Wybierz cel do NET USE zależnie od LAN/WAN
                                var connectTarget = isPrivate && !string.IsNullOrWhiteSpace(preferredName) ? nameOption : ipOption;
                                var netProcess = new ProcessStartInfo
                                {
                                    FileName = "net.exe",
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true
                                };
                                // ArgumentList escapuje każdy argument osobno – brak wstrzyknięcia argumentów
                                // przez znaki specjalne w haśle/nazwie użytkownika (np. cudzysłów)
                                netProcess.ArgumentList.Add("use");
                                netProcess.ArgumentList.Add(connectTarget);
                                netProcess.ArgumentList.Add(credDlg.Password);
                                netProcess.ArgumentList.Add("/user:" + credDlg.Username);
                                netProcess.ArgumentList.Add("/persistent:no");

                                using var p = Process.Start(netProcess);

                                // Czytaj strumienie PRZED WaitForExit – inaczej pełny bufor pipe
                                // może zakleszczyć proces potomny
                                var output = p?.StandardOutput.ReadToEnd() ?? "";
                                var error = p?.StandardError.ReadToEnd() ?? "";
                                p?.WaitForExit(5000);

                                // Po próbie logowania otwórz Explorer
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

                                    // Skopiuj preferowaną ścieżkę do schowka
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

        // Zwraca host (bez portu) odpowiedni do UNC, na podstawie IP lub nazwy
        private string GetUncTargetHost(Computer computer)
        {
            if (!string.IsNullOrWhiteSpace(computer.IP))
            {
                var (host, _) = ParseHostPort(computer.IP, 445);
                return host;
            }
            return computer.Name;
        }

        // Określa czy host to adres prywatny (LAN). Jeśli host to nazwa, sprawdza zresolvowane adresy.
        private bool IsPrivateHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) return false;

            if (IPAddress.TryParse(host, out var ip))
            {
                return IsPrivateIpAddress(ip);
            }

            try
            {
                var addrs = Dns.GetHostAddresses(host);
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
        public string PingType { get; set; } = "ICMP"; // "ICMP", "TCP", etc.
        public bool IsOnline { get; set; }
    }
}
