using System;
using System.Windows.Forms;

namespace WOLMenager
{
    public class NetworkCredentialForm : Form
    {
        private readonly string _host;
        private TextBox txtUser;
        private TextBox txtPassword;
        private CheckBox chkUseLocalAccount;
        private Button btnOk;
        private Button btnCancel;

        public string Username => GetFormattedUsername();
        public string Password => txtPassword.Text;

        private string GetFormattedUsername()
        {
            var user = txtUser.Text.Trim();
            if (string.IsNullOrWhiteSpace(user)) return string.Empty;
            
            // Jeœli zaznaczone konto lokalne i nie ma ju¿ formatu host\user
            if (chkUseLocalAccount.Checked && !user.Contains("\\") && !user.Contains("@"))
            {
                return _host + "\\" + user;
            }
            
            return user;
        }

        public NetworkCredentialForm(string host)
        {
            _host = host;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Logowanie do " + _host;
            this.Width = 420;
            this.Height = 220;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblUser = new Label { Left = 12, Top = 15, Width = 90, Text = "U¿ytkownik:" };
            txtUser = new TextBox { Left = 110, Top = 12, Width = 280, PlaceholderText = "Administrator lub nazwa u¿ytkownika" };

            var lblPass = new Label { Left = 12, Top = 50, Width = 90, Text = "Has³o:" };
            txtPassword = new TextBox { Left = 110, Top = 47, Width = 280, UseSystemPasswordChar = true };

            chkUseLocalAccount = new CheckBox 
            { 
                Left = 110, 
                Top = 80, 
                Width = 280, 
                Text = "Konto lokalne serwera (dodaje nazwê hosta automatycznie)",
                Checked = true
            };

            var lblInfo = new Label 
            { 
                Left = 12, 
                Top = 110, 
                Width = 380, 
                Height = 40,
                Text = "Dla konta lokalnego: wpisz tylko nazwê u¿ytkownika (np. Administrator)\nDla konta domenowego: odznacz powy¿sze i wpisz pe³n¹ nazwê",
                ForeColor = System.Drawing.Color.DarkBlue
            };

            btnOk = new Button { Left = 230, Top = 160, Width = 75, Text = "OK" };
            btnCancel = new Button { Left = 315, Top = 160, Width = 75, Text = "Anuluj" };

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUser.Text.Trim()))
                {
                    MessageBox.Show("Podaj nazwê u¿ytkownika", "B³¹d", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Podaj has³o", "B³¹d", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblUser);
            this.Controls.Add(txtUser);
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPassword);
            this.Controls.Add(chkUseLocalAccount);
            this.Controls.Add(lblInfo);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
        }
    }
}
