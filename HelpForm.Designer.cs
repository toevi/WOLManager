namespace WOLMenager
{
    partial class HelpForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtHelp;
        private Button btnClose;
        private Label lblTitle;

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
            this.txtHelp = new TextBox();
            this.btnClose = new Button();
            this.lblTitle = new Label();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.Location = new Point(12, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(660, 30);
            this.lblTitle.Text = "?? WOL MANAGER - Help & Information";
            this.lblTitle.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.DarkBlue;
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // txtHelp
            this.txtHelp.Location = new Point(12, 50);
            this.txtHelp.Name = "txtHelp";
            this.txtHelp.Size = new Size(660, 480);
            this.txtHelp.Multiline = true;
            this.txtHelp.ScrollBars = ScrollBars.Vertical;
            this.txtHelp.ReadOnly = true;
            this.txtHelp.Font = new Font("Consolas", 9F, FontStyle.Regular);
            this.txtHelp.BackColor = Color.White;
            this.txtHelp.BorderStyle = BorderStyle.FixedSingle;

            // btnClose
            this.btnClose.Location = new Point(597, 540);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(75, 30);
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);

            // HelpForm
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(684, 582);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtHelp);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HelpForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "WOL Manager - Help";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}