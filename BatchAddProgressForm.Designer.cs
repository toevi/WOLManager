namespace WOLMenager
{
    partial class BatchAddProgressForm
    {
        private System.ComponentModel.IContainer components = null;
        private ProgressBar progressBar;
        private Label lblTitle;
        private Label lblStatus;
        private Label lblProgress;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.progressBar = new ProgressBar();
            this.lblTitle = new Label();
            this.lblStatus = new Label();
            this.lblProgress = new Label();
            this.btnClose = new Button();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.Location = new Point(12, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(360, 25);
            this.lblTitle.Text = "? Adding Multiple Computers";
            this.lblTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.DarkBlue;
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // lblStatus
            this.lblStatus.Location = new Point(12, 45);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(360, 20);
            this.lblStatus.Text = "Preparing to add computers...";
            this.lblStatus.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
            this.lblStatus.ForeColor = Color.DarkGreen;

            // progressBar
            this.progressBar.Location = new Point(12, 75);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(360, 25);
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 100;
            this.progressBar.Value = 0;

            // lblProgress
            this.lblProgress.Location = new Point(12, 110);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new Size(360, 20);
            this.lblProgress.Text = "Progress: 0/0";
            this.lblProgress.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular);
            this.lblProgress.ForeColor = Color.Gray;
            this.lblProgress.TextAlign = ContentAlignment.MiddleCenter;

            // btnClose
            this.btnClose.Location = new Point(150, 140);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(84, 30);
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Enabled = false;
            this.btnClose.Visible = false;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);

            // BatchAddProgressForm
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 185);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BatchAddProgressForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Adding Computers...";
            this.ResumeLayout(false);
        }
    }
}