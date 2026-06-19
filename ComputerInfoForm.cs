using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WOLManager
{
    public class ComputerInfoForm : Form
    {
        private readonly Computer _computer;
        private CancellationTokenSource? _cts;

        private Label lblPingVal;
        private Label lblOsVal;
        private ListView lvPorts;
        private Button btnScan;
        private Label lblStatus;
        private ProgressBar progressBar;

        private static readonly (int Port, string Service)[] WellKnownPorts =
        {
            (21,    "FTP"),
            (22,    "SSH"),
            (23,    "Telnet"),
            (25,    "SMTP"),
            (53,    "DNS"),
            (80,    "HTTP"),
            (110,   "POP3"),
            (135,   "RPC / DCOM"),
            (139,   "NetBIOS"),
            (143,   "IMAP"),
            (443,   "HTTPS"),
            (445,   "SMB"),
            (1433,  "MS SQL Server"),
            (3306,  "MySQL"),
            (3389,  "RDP"),
            (5900,  "VNC"),
            (5985,  "WinRM (HTTP)"),
            (5986,  "WinRM (HTTPS)"),
            (8080,  "HTTP Alt"),
            (8443,  "HTTPS Alt"),
        };

        public ComputerInfoForm(Computer computer)
        {
            _computer = computer;
            BuildUI();
            this.FormClosing += (s, e) => _cts?.Cancel();
            this.Load += async (s, e) => await RunScanAsync();
        }

        private void BuildUI()
        {
            Text = $"Info — {_computer.Name}  ({_computer.IP})";
            Size = new Size(500, 570);
            MinimumSize = new Size(420, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            // ── Network Info ─────────────────────────────────────
            var grpInfo = new GroupBox
            {
                Text = "Network Info",
                Location = new Point(12, 8),
                Size = new Size(460, 112),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int y = 20;
            AddRow(grpInfo, "Hostname:",   _computer.Name,        ref y);
            AddRow(grpInfo, "IP:",         _computer.IP,          ref y);
            AddRow(grpInfo, "MAC:",        _computer.MacAddress,  ref y);
            lblPingVal = AddRow(grpInfo, "Ping:",     "—", ref y);
            lblOsVal   = AddRow(grpInfo, "OS hint:",  "—", ref y);

            // ── Port Scan ─────────────────────────────────────────
            var grpPorts = new GroupBox
            {
                Text = "Port Scan",
                Location = new Point(12, 128),
                Size = new Size(460, 350),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            lvPorts = new ListView
            {
                Location = new Point(8, 20),
                Size = new Size(444, 322),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            lvPorts.Columns.Add("Port", 58);
            lvPorts.Columns.Add("Service", 175);
            lvPorts.Columns.Add("Status", 195);
            grpPorts.Controls.Add(lvPorts);

            // Pre-populate with placeholders
            foreach (var (port, service) in WellKnownPorts)
            {
                var item = new ListViewItem(port.ToString());
                item.SubItems.Add(service);
                item.SubItems.Add("—");
                item.ForeColor = Color.DarkGray;
                lvPorts.Items.Add(item);
            }

            // ── Bottom strip ──────────────────────────────────────
            lblStatus = new Label
            {
                Text = "Scanning...",
                Location = new Point(12, 490),
                Size = new Size(300, 18),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            progressBar = new ProgressBar
            {
                Location = new Point(12, 512),
                Size = new Size(460, 7),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            btnScan = new Button
            {
                Text = "Scan Again",
                Location = new Point(316, 524),
                Size = new Size(90, 28),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Enabled = false
            };
            btnScan.Click += async (s, e) => await RunScanAsync();

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(412, 524),
                Size = new Size(60, 28),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => { _cts?.Cancel(); Close(); };

            Controls.Add(grpInfo);
            Controls.Add(grpPorts);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btnScan);
            Controls.Add(btnClose);
        }

        private Label AddRow(GroupBox parent, string caption, string value, ref int y)
        {
            parent.Controls.Add(new Label
            {
                Text = caption,
                Location = new Point(8, y),
                Size = new Size(72, 18),
                ForeColor = Color.DimGray
            });
            var val = new Label
            {
                Text = value ?? "—",
                Location = new Point(84, y),
                Size = new Size(368, 18)
            };
            parent.Controls.Add(val);
            y += 19;
            return val;
        }

        private async Task RunScanAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            btnScan.Enabled = false;
            progressBar.Visible = true;

            // Reset list
            foreach (ListViewItem item in lvPorts.Items)
            {
                item.SubItems[2].Text = "—";
                item.ForeColor = Color.DarkGray;
                item.Font = lvPorts.Font;
            }

            var host = !string.IsNullOrWhiteSpace(_computer.IP) ? _computer.IP : _computer.Name;
            if (string.IsNullOrWhiteSpace(host))
            {
                SetStatus("No IP or hostname.");
                progressBar.Visible = false;
                btnScan.Enabled = true;
                return;
            }

            try
            {
                // ── Ping ─────────────────────────────────────────
                SetStatus("Pinging...");
                try
                {
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync(host, 2000);
                    if (reply.Status == IPStatus.Success)
                    {
                        var ttl = reply.Options?.Ttl ?? 0;
                        lblPingVal.Text = $"{reply.RoundtripTime} ms   (TTL={ttl})";
                        lblOsVal.Text = ttl >= 120 && ttl <= 130 ? "Windows  (TTL ~128)"
                                      : ttl >= 58  && ttl <= 65  ? "Linux / macOS  (TTL ~64)"
                                      : ttl >= 248               ? "Network device  (TTL ~255)"
                                      :                            $"Unknown  (TTL={ttl})";
                    }
                    else
                    {
                        lblPingVal.Text = $"No response  ({reply.Status})";
                        lblOsVal.Text = "—";
                    }
                }
                catch
                {
                    lblPingVal.Text = "Ping failed";
                    lblOsVal.Text = "—";
                }

                if (token.IsCancellationRequested) return;

                // ── Port scan — wszystkie równolegle, 1500ms timeout ──
                SetStatus($"Scanning {WellKnownPorts.Length} ports in parallel...");

                var tasks = WellKnownPorts.Select(async (entry, idx) =>
                {
                    bool open = await IsPortOpenAsync(host, entry.Port, 1500, token);
                    return (idx, open);
                });

                var results = await Task.WhenAll(tasks);

                int openCount = 0;
                foreach (var (idx, open) in results)
                {
                    if (token.IsCancellationRequested) break;
                    var item = lvPorts.Items[idx];
                    if (open)
                    {
                        item.SubItems[2].Text = "OPEN";
                        item.ForeColor = Color.DarkGreen;
                        item.Font = new Font(lvPorts.Font, FontStyle.Bold);
                        openCount++;
                    }
                    else
                    {
                        item.SubItems[2].Text = "closed";
                        item.ForeColor = Color.LightGray;
                    }
                }

                if (!token.IsCancellationRequested)
                    SetStatus($"Done — {openCount} open port(s) out of {WellKnownPorts.Length} scanned");
            }
            catch (OperationCanceledException)
            {
                SetStatus("Scan cancelled.");
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (!IsDisposed)
                    {
                        btnScan.Enabled = true;
                        progressBar.Visible = false;
                    }
                }
                catch (ObjectDisposedException) { }
            }
        }

        private void SetStatus(string msg)
        {
            if (InvokeRequired)
                Invoke(new Action<string>(SetStatus), msg);
            else
                lblStatus.Text = msg;
        }

        private static async Task<bool> IsPortOpenAsync(string host, int port, int timeoutMs, CancellationToken cancel)
        {
            try
            {
                using var client = new TcpClient();
                using var timeout = new CancellationTokenSource(timeoutMs);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeout.Token);
                await client.ConnectAsync(host, port, linked.Token);
                return true;
            }
            catch { return false; }
        }
    }
}
