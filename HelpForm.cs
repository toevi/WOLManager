using System;
using System.Drawing;
using System.Windows.Forms;

namespace WOLManager
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
            SetupForm();
            LoadHelpContent();
        }

        private void SetupForm()
        {
            this.Text = "WOL Manager - Help & Information";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
        }

        private void LoadHelpContent()
        {
            string helpText =
"WOL MANAGER 1.1  -  Help & User Guide\r\n" +
"=======================================\r\n" +
"\r\n" +
"OVERVIEW\r\n" +
"--------\r\n" +
"WOL Manager is an admin tool for managing computers on your local network:\r\n" +
"  - Wake up computers remotely (Wake-on-LAN magic packet)\r\n" +
"  - Remote Restart / Shutdown\r\n" +
"  - Remote Desktop (RDP), SSH, Network Share access\r\n" +
"  - Scan open ports and detect OS on any host\r\n" +
"  - Auto-discover computers on the LAN\r\n" +
"  - System tray with online notifications\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"BUTTON REFERENCE\r\n" +
"\r\n" +
"  POWER GROUP\r\n" +
"    Wake Up    -- sends WOL magic packet (requires MAC address)\r\n" +
"    Restart    -- remote restart via shutdown.exe (requires admin share)\r\n" +
"    Shutdown   -- remote shutdown via shutdown.exe (requires admin share)\r\n" +
"\r\n" +
"  REMOTE ACCESS GROUP\r\n" +
"    RDP              -- opens mstsc.exe connected to selected computer\r\n" +
"    SSH              -- opens Windows Terminal (or cmd) with ssh [user@]host\r\n" +
"                        Set SSH Username in Edit dialog to skip the prompt.\r\n" +
"    Network Share    -- opens \\\\hostname in Explorer; prompts for credentials\r\n" +
"                        if the direct connection fails\r\n" +
"    Info / Port Scan -- pings host, shows TTL-based OS hint, scans 20 common\r\n" +
"                        TCP ports in parallel (FTP, HTTP, RDP, SMB, SSH, ...)\r\n" +
"\r\n" +
"  COMPUTERS GROUP\r\n" +
"    Add       -- manually add a computer with all details\r\n" +
"    Add Scan  -- scan LAN, pick computers from the discovered list\r\n" +
"    Edit      -- change name, IP, MAC, port, ping type, SSH user, notes\r\n" +
"    Remove    -- delete computer from list\r\n" +
"\r\n" +
"  BOTTOM\r\n" +
"    Refresh   -- re-check online/offline status of all computers now\r\n" +
"    Help      -- this window\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"QUICK START\r\n" +
"\r\n" +
"  1. Add computers\r\n" +
"       Manual : Add -> fill form\r\n" +
"       Auto   : Add Scan -> wait -> tick checkboxes -> Add Selected\r\n" +
"\r\n" +
"  2. Wake up a computer\r\n" +
"       Double-click the row  OR  select + click Wake Up\r\n" +
"       Status updates automatically after 3 s\r\n" +
"\r\n" +
"  3. Remote access\r\n" +
"       Select computer -> RDP / SSH / Network Share\r\n" +
"\r\n" +
"  4. Port scan / OS info\r\n" +
"       Select computer -> Info / Port Scan\r\n" +
"       Scan runs automatically on open; click Scan Again to repeat\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"SYSTEM TRAY\r\n" +
"\r\n" +
"  Minimizing the window sends WOL Manager to the notification area.\r\n" +
"  Double-click the tray icon (or right-click -> Open) to restore.\r\n" +
"  A balloon notification appears when a woken computer comes online.\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"COMPUTER LIST - COLUMNS\r\n" +
"\r\n" +
"  Name     -- network hostname (used for RDP, share, shutdown)\r\n" +
"  IP       -- IPv4 address (used for WOL, SSH, ping, port scan)\r\n" +
"  MAC      -- required for Wake-on-LAN (XX:XX:XX:XX:XX:XX)\r\n" +
"  Broadcast-- directed broadcast for WOL (e.g. 192.168.1.255)\r\n" +
"  Port     -- UDP port for WOL packet (default 9)\r\n" +
"  Ping     -- ICMP or TCP (use TCP if ICMP is blocked by firewall)\r\n" +
"  SshU     -- SSH username saved per computer\r\n" +
"  Notes    -- free-text notes (role, location, owner)\r\n" +
"  Status   -- online/offline, refreshed every 10 s\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"WAKE-ON-LAN REQUIREMENTS\r\n" +
"\r\n" +
"  - MAC address must be set\r\n" +
"  - WOL enabled in BIOS/UEFI and network adapter properties\r\n" +
"  - Ethernet connection recommended (WiFi WOL is unreliable)\r\n" +
"  - Fast Startup disabled (Control Panel -> Power Options -> Choose what\r\n" +
"    the power buttons do -> Turn on fast startup: OFF)\r\n" +
"  - Packet is sent through every active IPv4 network interface\r\n" +
"    (supports multi-homed PCs with Hyper-V / VPN adapters)\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"SSH NOTES\r\n" +
"\r\n" +
"  - Requires OpenSSH Client: Settings -> Apps -> Optional features\r\n" +
"  - Preferred terminal: Windows Terminal (wt.exe)\r\n" +
"  - Fallback: cmd.exe /k ssh ...\r\n" +
"  - SSH Username can be saved per computer in Edit dialog\r\n" +
"  - If left empty, the SSH client will prompt for credentials\r\n" +
"  - Host and username are validated -- shell metacharacters blocked\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"REMOTE RESTART / SHUTDOWN REQUIREMENTS\r\n" +
"\r\n" +
"  - File and Printer Sharing must be enabled on the target\r\n" +
"  - Firewall rule 'Remote Shutdown' must be active on the target\r\n" +
"  - You must have administrator rights on the target machine\r\n" +
"  - Works best with computers joined to the same domain or workgroup\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"TROUBLESHOOTING\r\n" +
"\r\n" +
"  WOL packet sent but computer does not wake:\r\n" +
"    -> Check BIOS WOL setting ('Resume by LAN' or similar)\r\n" +
"    -> Check NIC power management: allow the device to wake the computer\r\n" +
"    -> Check Windows Fast Startup is OFF\r\n" +
"    -> Make sure MAC address in the list is correct\r\n" +
"\r\n" +
"  RDP fails:\r\n" +
"    -> Confirm Remote Desktop is enabled on the target\r\n" +
"    -> Check Windows Firewall allows Remote Desktop (port 3389)\r\n" +
"\r\n" +
"  Network Share fails:\r\n" +
"    -> Use computer Name, not just IP (SMB auth works better with names)\r\n" +
"    -> Enable File and Printer Sharing in target's firewall settings\r\n" +
"\r\n" +
"  SSH fails:\r\n" +
"    -> Install OpenSSH Client (Settings -> Apps -> Optional features)\r\n" +
"    -> Confirm SSH service is running on the target (port 22)\r\n" +
"\r\n" +
"  Port scan shows all ports closed:\r\n" +
"    -> Target may be offline or firewall blocks all inbound connections\r\n" +
"    -> Try 'Scan Again' -- first scan after wake may time out\r\n" +
"\r\n" +
"=======================================\r\n" +
"\r\n" +
"ABOUT\r\n" +
"\r\n" +
"  WOL Manager 1.1\r\n" +
"  Author  : Tomek Maselko\r\n" +
"  Company : tmfgroup\r\n" +
"  License : MIT\r\n" +
"  GitHub  : https://github.com/toevi/WOLManager\r\n" +
"\r\n" +
"  Config  : %APPDATA%\\WOLManager\\computers.json\r\n" +
"\r\n" +
"  (c) 2025 tmfgroup\r\n" +
"=======================================";

            if (txtHelp != null)
            {
                txtHelp.Text = helpText;
                txtHelp.SelectionStart = 0;
                txtHelp.SelectionLength = 0;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
