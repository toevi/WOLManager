using System;
using System.Drawing;
using System.Windows.Forms;

namespace WOLManager
{
    public partial class BatchAddProgressForm : Form
    {
        public BatchAddProgressForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false; // Disable close button during operation
        }

        public void UpdateProgress(int current, int total, string currentItem)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int, string>(UpdateProgress), current, total, currentItem);
                return;
            }

            progressBar.Value = Math.Min(current, total);
            progressBar.Maximum = total;
            
            lblStatus.Text = $"Adding computer: {currentItem}";
            lblProgress.Text = $"Progress: {current}/{total}";
            
            Application.DoEvents();
        }

        public void SetCompleted(int successCount, int totalCount)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int>(SetCompleted), successCount, totalCount);
                return;
            }

            progressBar.Value = progressBar.Maximum;
            lblStatus.Text = "Completed!";
            lblProgress.Text = $"Added {successCount} out of {totalCount} computers";
            
            btnClose.Enabled = true;
            btnClose.Visible = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}