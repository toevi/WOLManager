using System;
using System.Drawing;
using System.Windows.Forms;

namespace WOLManager
{
    public partial class ComputerNameInputForm : Form
    {
        public string ComputerName { get; private set; }

        public ComputerNameInputForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtComputerName.Text))
            {
                MessageBox.Show("Wpisz nazwê komputera!", "Wymagana nazwa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtComputerName.Focus();
                return;
            }

            ComputerName = txtComputerName.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtComputerName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnOK_Click(sender, e);
            }
        }

        private void btnCheckClipboard_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    var clipboardContent = Clipboard.GetText().Trim();
                    
                    if (string.IsNullOrEmpty(clipboardContent))
                    {
                        MessageBox.Show("Schowek jest pusty!", "Schowek", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // SprawdŸ czy to œcie¿ka UNC
                    if (clipboardContent.StartsWith("\\\\"))
                    {
                        var uncPath = clipboardContent.TrimStart('\\');
                        var firstSlash = uncPath.IndexOf('\\');
                        var computerName = firstSlash > 0 ? uncPath.Substring(0, firstSlash) : uncPath;
                        
                        if (!string.IsNullOrEmpty(computerName))
                        {
                            txtComputerName.Text = computerName.ToUpper();
                            MessageBox.Show(
                                $"Znaleziono œcie¿kê UNC: {clipboardContent}\n" +
                                $"Wyci¹gniêto nazwê: {computerName.ToUpper()}",
                                "Sukces",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            return;
                        }
                    }

                    // SprawdŸ czy to mo¿e byæ sama nazwa komputera
                    if (!clipboardContent.Contains("\\") && !clipboardContent.Contains("/") && 
                        !clipboardContent.Contains(" ") && clipboardContent.Length > 3)
                    {
                        txtComputerName.Text = clipboardContent.ToUpper();
                        MessageBox.Show(
                            $"U¿yto zawartoœci schowka jako nazwy komputera:\n{clipboardContent.ToUpper()}",
                            "Sukces",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    // Jeœli nic nie pasuje, poka¿ zawartoœæ
                    MessageBox.Show(
                        $"Zawartoœæ schowka:\n{clipboardContent}\n\n" +
                        "To nie wygl¹da na nazwê komputera ani œcie¿kê UNC.\n" +
                        "Oczekiwane formaty:\n" +
                        "• \\\\NAZWA-KOMPUTERA\n" +
                        "• NAZWA-KOMPUTERA",
                        "Nierozpoznany format",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Schowek nie zawiera tekstu!", "Schowek", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"B³¹d odczytu schowka: {ex.Message}", "B³¹d", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}