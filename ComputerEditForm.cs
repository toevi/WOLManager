using System;
using System.Windows.Forms;

namespace WOLMenager
{
    public partial class ComputerEditForm : Form
    {
        public Computer Computer { get; private set; }

        public ComputerEditForm()
        {
            InitializeComponent();
            Computer = new Computer();
        }

        public ComputerEditForm(Computer computer)
        {
            InitializeComponent();
            Computer = new Computer
            {
                Name = computer.Name,
                IP = computer.IP,
                Port = computer.Port,
                MacAddress = computer.MacAddress,
                Broadcast = computer.Broadcast,
                PingType = computer.PingType
            };
            txtName.Text = Computer.Name;
            txtIP.Text = Computer.IP;
            numPort.Value = Computer.Port;
            txtMac.Text = Computer.MacAddress;
            txtBroadcast.Text = Computer.Broadcast;
            cmbPingType.SelectedItem = Computer.PingType;
        }

        private bool IsValidMacAddress(string mac)
        {
            // Accepts formats: XX:XX:XX:XX:XX:XX or XX-XX-XX-XX-XX-XX or XXXXXXXXXXXX
            mac = mac.Replace(":", "").Replace("-", "").Replace(" ", "");
            if (mac.Length != 12) return false;
            for (int i = 0; i < 12; i++)
            {
                if (!Uri.IsHexDigit(mac[i])) return false;
            }
            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Walidacja nazwy komputera
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(
                    "COMPUTER NAME is required!\n\n" +
                    "IMPORTANT: Name must be identical to the network computer name!\n\n" +
                    "TIP: Use \"Add Scan\" button in main window for automatic discovery!\n\n" +
                    "WAYS TO CHECK THE NAME:\n" +
                    "� Check names in Explorer - Network\n" +
                    "� CMD: hostname\n" +
                    "� PowerShell: $env:COMPUTERNAME\n\n" +
                    "Computer names work better than IP addresses for network resource access!",
                    "Computer name required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtMac.Text) && !IsValidMacAddress(txtMac.Text))
            {
                MessageBox.Show("Invalid MAC address format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Computer.Name = txtName.Text.Trim();
            Computer.IP = txtIP.Text.Trim();
            Computer.Port = (int)numPort.Value;
            Computer.MacAddress = txtMac.Text.Trim();
            Computer.Broadcast = txtBroadcast.Text.Trim();
            Computer.PingType = cmbPingType.SelectedItem?.ToString() ?? "ICMP";
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
