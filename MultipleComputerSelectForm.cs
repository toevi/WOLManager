using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WOLManager
{
    public partial class MultipleComputerSelectForm : Form
    {
        public List<NetworkComputer> SelectedComputers { get; private set; } = new List<NetworkComputer>();
        private List<NetworkComputer> allComputers = new List<NetworkComputer>();
        private bool isScanning = false;

        public MultipleComputerSelectForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Network Scanner - Select Multiple Computers";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private async void btnScanNetwork_Click(object sender, EventArgs e)
        {
            if (isScanning)
            {
                MessageBox.Show("Scanning is already in progress!", "Scan in progress", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                isScanning = true;
                btnScanNetwork.Text = "Scanning... Please wait";
                btnScanNetwork.Enabled = false;
                btnAddSelected.Enabled = false;
                listViewComputers.Items.Clear();
                
                progressBar.Visible = true;
                lblStatus.Visible = true;
                lblStatus.Text = "Initializing network scan...";
                lblStatus.ForeColor = Color.DarkBlue;
                
                Application.DoEvents();

                // Uruchom skanowanie sieci
                allComputers = await Task.Run(() => ScanNetworkForComputers());

                progressBar.Visible = false;

                if (allComputers.Count == 0)
                {
                    lblStatus.Text = "No active computers found on the network";
                    lblStatus.ForeColor = Color.DarkRed;
                    
                    MessageBox.Show(
                        "No active computers found on the network.\n\n" +
                        "Possible causes:\n" +
                        "� All computers are turned off\n" +
                        "� Firewall blocks ping/communication\n" +
                        "� You are on an isolated network\n" +
                        "� ICMP is blocked on the router",
                        "No active computers",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Wype�nij list� z checkboxami
                LoadComputersToList();
                
                lblStatus.Text = $"Found {allComputers.Count} computers. Select computers to add:";
                lblStatus.ForeColor = Color.DarkGreen;
                
                btnAddSelected.Enabled = true;
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                lblStatus.Text = $"Scan error: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
                
                MessageBox.Show(
                    $"Error during network scan:\n{ex.Message}",
                    "Scan error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isScanning = false;
                btnScanNetwork.Text = "SCAN NETWORK";
                btnScanNetwork.Enabled = true;
            }
        }

        private void LoadComputersToList()
        {
            listViewComputers.Items.Clear();
            
            foreach (var computer in allComputers.OrderBy(c => c.Name))
            {
                var item = new ListViewItem();
                item.Text = ""; // Pierwsza kolumna dla checkbox
                item.SubItems.Add(computer.Name);
                item.SubItems.Add(computer.IP);
                
                // Poka� lepszy status z informacj� o MAC
                var status = !string.IsNullOrEmpty(computer.MacAddress) ? 
                    "Windows (MAC found)" : "Windows (no MAC)";
                item.SubItems.Add(status);
                item.Tag = computer;
                
                listViewComputers.Items.Add(item);
            }
            
            // Lepsze wyja�nienie dla u�ytkownika
            lblInstructions.Text = $"Found {allComputers.Count} Windows computers. Non-Windows devices (routers, printers) were filtered out. Select computers to add:";
            lblInstructions.Visible = true;
        }

        private List<NetworkComputer> ScanNetworkForComputers()
        {
            var activeComputers = new List<NetworkComputer>();
            
            try
            {
                var localNetworks = GetLocalNetworkRanges();
                UpdateScanStatus($"Found {localNetworks.Count} networks to scan...");
                
                foreach (var network in localNetworks)
                {
                    UpdateScanStatus($"Scanning network {network.BaseAddress}.x...");
                    
                    // Skanuj w mniejszych grupach dla lepszej dok�adno�ci
                    var allIPs = new List<string>();
                    for (int i = 1; i <= 254; i++)
                    {
                        allIPs.Add($"{network.BaseAddress}.{i}");
                    }
                    
                    // Przetwarzaj w grupach po 25 IP dla lepszej kontroli
                    var batchSize = 25;
                    var batches = allIPs
                        .Select((ip, index) => new { ip, index })
                        .GroupBy(x => x.index / batchSize)
                        .Select(g => g.Select(x => x.ip).ToList())
                        .ToList();
                    
                    int processedBatches = 0;
                    
                    foreach (var batch in batches)
                    {
                        processedBatches++;
                        UpdateScanStatus($"Processing batch {processedBatches}/{batches.Count} in {network.BaseAddress}.x...");
                        
                        // Uruchom wszystkie ping'i w batchu r�wnolegle
                        var batchTasks = batch.Select(ip => Task.Run(() => PingAndResolveComputer(ip))).ToArray();
                        
                        // Poczekaj na wszystkie wyniki z timeout
                        var batchResults = Task.WhenAll(batchTasks).Result;
                        var foundInBatch = batchResults.Where(c => c != null).ToList();
                        
                        activeComputers.AddRange(foundInBatch);
                        
                        if (foundInBatch.Count > 0)
                        {
                            UpdateScanStatus($"Found {foundInBatch.Count} Windows computers in batch {processedBatches}");
                        }
                        
                        // Kr�tka pauza mi�dzy batches
                        Task.Delay(100).Wait();
                    }
                }
                
                UpdateScanStatus($"Scan complete. Found {activeComputers.Count} Windows computers.");
                
                // Usu� duplikaty na podstawie IP
                var uniqueComputers = activeComputers
                    .GroupBy(c => c.IP)
                    .Select(g => g.First())
                    .OrderBy(c => c.Name)
                    .ToList();
                
                return uniqueComputers;
            }
            catch (Exception ex)
            {
                UpdateScanStatus($"Error: {ex.Message}");
                return activeComputers;
            }
        }

        private void UpdateScanStatus(string message)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<string>(UpdateScanStatus), message);
                    return;
                }
                
                lblStatus.Text = message;
                lblStatus.ForeColor = Color.DarkBlue;
                Application.DoEvents();
            }
            catch { }
        }

        private NetworkComputer PingAndResolveComputer(string ip)
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send(ip, 2000); // Zwi�kszony timeout do 2 sekund
                
                if (reply.Status == IPStatus.Success)
                {
                    var computer = new NetworkComputer { IP = ip };
                    
                    // Spr�buj znale�� nazw� komputera
                    try
                    {
                        var hostEntry = System.Net.Dns.GetHostEntry(ip);
                        var hostName = hostEntry.HostName;
                        var shortName = hostName.Split('.')[0].ToUpper();
                        computer.Name = shortName;
                    }
                    catch
                    {
                        computer.Name = $"PC-{ip.Replace(".", "-")}{Random.Shared.Next(1, 10000)}";
                    }
                    
                    computer.Broadcast = CalculateBroadcast(ip);
                    
                    // Sprawd� czy to Windows computer
                    if (!IsLikelyWindowsComputer(ip, computer.Name))
                    {
                        return null; // Pomijaj urz�dzenia nie-Windows
                    }
                    
                    // Spr�buj znale�� MAC address (z wi�kszym timeout)
                    try
                    {
                        computer.MacAddress = GetMacAddressByIP(ip);
                    }
                    catch { }
                    
                    return computer;
                }
            }
            catch { }
            
            return null;
        }

        private bool IsLikelyWindowsComputer(string ip, string name)
        {
            try
            {
                // Test 1: Sprawd� czy nazwa komputera wygl�da jak Windows
                if (!string.IsNullOrEmpty(name) && !name.StartsWith("PC-"))
                {
                    // Typowe nazwy Windows computers (nie routery/printery)
                    var windowsPatterns = new[] { "DESKTOP-", "LAPTOP-", "PC-", "WIN-", "WORKSTATION-" };
                    var nonWindowsPatterns = new[] { "ROUTER", "SWITCH", "ACCESS-POINT", "AP-", "PRINTER", "CANON", "HP-", "EPSON", "SAMSUNG", "BROTHER" };
                    
                    var upperName = name.ToUpper();
                    
                    // Wykluczaj typowe urz�dzenia sieciowe
                    foreach (var pattern in nonWindowsPatterns)
                    {
                        if (upperName.Contains(pattern))
                            return false;
                    }
                }
                
                // Test 2: Sprawd� czy odpowiada na typowe porty Windows
                if (IsWindowsPortOpen(ip))
                {
                    return true;
                }
                
                // Test 3: Sprawd� TTL (Windows zazwyczaj ma TTL 128)
                if (CheckWindowsTTL(ip))
                {
                    return true;
                }
                
                // Je�li nazwa nie wygl�da na router/printer, prawdopodobnie to komputer
                if (!string.IsNullOrEmpty(name) && !name.StartsWith("PC-"))
                {
                    var upperName = name.ToUpper();
                    return !upperName.Contains("ROUTER") && 
                           !upperName.Contains("SWITCH") && 
                           !upperName.Contains("AP-") &&
                           !upperName.Contains("PRINTER");
                }
                
                return true; // Domy�lnie akceptuj
            }
            catch
            {
                return true; // W przypadku b��du, akceptuj
            }
        }

        private bool IsWindowsPortOpen(string ip)
        {
            try
            {
                // Sprawd� port 445 (SMB) - typowy dla Windows
                using var client = new System.Net.Sockets.TcpClient();
                var result = client.BeginConnect(ip, 445, null, null);
                var success = result.AsyncWaitHandle.WaitOne(1000); // 1 sekunda timeout
                
                if (success)
                {
                    try { client.EndConnect(result); return true; }
                    catch { return false; }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckWindowsTTL(string ip)
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send(ip, 1000);
                
                if (reply.Status == IPStatus.Success)
                {
                    // Windows TTL jest zazwyczaj 128, Linux/Unix 64
                    // Router/embedded devices cz�sto maj� inne warto�ci
                    return reply.Options?.Ttl >= 120 && reply.Options?.Ttl <= 128;
                }
            }
            catch { }
            
            return false;
        }

        private List<NetworkRange> GetLocalNetworkRanges()
        {
            var ranges = new List<NetworkRange>();
            
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var ni in networkInterfaces)
                {
                    var properties = ni.GetIPProperties();
                    var addresses = properties.UnicastAddresses
                        .Where(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    foreach (var addr in addresses)
                    {
                        var ip = addr.Address.ToString();
                        if (!ip.StartsWith("169.254"))
                        {
                            var parts = ip.Split('.');
                            if (parts.Length == 4)
                            {
                                ranges.Add(new NetworkRange
                                {
                                    BaseAddress = $"{parts[0]}.{parts[1]}.{parts[2]}",
                                    NetworkInterface = ni.Name
                                });
                            }
                        }
                    }
                }
            }
            catch { }
            
            if (ranges.Count == 0)
            {
                ranges.Add(new NetworkRange { BaseAddress = "192.168.1" });
                ranges.Add(new NetworkRange { BaseAddress = "192.168.0" });
                ranges.Add(new NetworkRange { BaseAddress = "10.0.0" });
            }
            
            return ranges.Distinct().ToList();
        }

        private string GetMacAddressByIP(string ip)
        {
            try
            {
                // Najpierw wy�lij ping �eby od�wie�y� ARP cache
                using (var ping = new Ping())
                {
                    ping.Send(ip, 500);
                }
                
                // Poczekaj chwil� na aktualizacj� ARP cache
                Task.Delay(200).Wait();
                
                var process = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = $"-a {ip}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                using var p = Process.Start(process);
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(3000);
                
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains(ip))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var mac = parts[1].Trim();
                            if (mac.Contains("-") && mac.Length == 17)
                            {
                                return mac.Replace("-", ":");
                            }
                        }
                    }
                }
            }
            catch { }
            
            return null;
        }

        private string CalculateBroadcast(string ip)
        {
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

        private void listViewComputers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Update counter after UI update
            BeginInvoke(new Action(UpdateSelectedCounter));
        }

        private void UpdateSelectedCounter()
        {
            var checkedCount = listViewComputers.CheckedItems.Count;
            btnAddSelected.Text = checkedCount > 0 
                ? $"ADD SELECTED ({checkedCount})" 
                : "ADD SELECTED";
            
            btnAddSelected.Enabled = checkedCount > 0 && !isScanning;
        }

        private void btnAddSelected_Click(object sender, EventArgs e)
        {
            var checkedItems = listViewComputers.CheckedItems;
            
            if (checkedItems.Count == 0)
            {
                MessageBox.Show("Select at least one computer to add!", "No selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedComputers.Clear();
            
            foreach (ListViewItem item in checkedItems)
            {
                if (item.Tag is NetworkComputer computer)
                {
                    SelectedComputers.Add(computer);
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewComputers.Items)
            {
                item.Checked = true;
            }
        }

        private void btnDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewComputers.Items)
            {
                item.Checked = false;
            }
        }
    }

    // Klasy pomocnicze
    public class NetworkComputer
    {
        public string Name { get; set; } = "";
        public string IP { get; set; } = "";
        public string Broadcast { get; set; } = "";
        public string MacAddress { get; set; } = "";
    }

    public class NetworkRange
    {
        public string BaseAddress { get; set; } = "";
        public string NetworkInterface { get; set; } = "";
        
        public override bool Equals(object? obj)
        {
            return obj is NetworkRange range && BaseAddress == range.BaseAddress;
        }
        
        public override int GetHashCode()
        {
            return BaseAddress?.GetHashCode() ?? 0;
        }
    }
}