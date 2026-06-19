namespace WOLManager
{
    partial class ComputerNameInputForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtComputerName;
        private Button btnOK;
        private Button btnCancel;
        private Button btnCheckClipboard;
        private Label lblInstruction;
        private Label lblExample;

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
            this.txtComputerName = new TextBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.lblInstruction = new Label();
            this.lblExample = new Label();
            this.btnCheckClipboard = new Button();
            this.SuspendLayout();

            // lblInstruction
            this.lblInstruction.Location = new Point(12, 12);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new Size(360, 40);
            this.lblInstruction.Text = "Wpisz dok³adn¹ NAZWÊ komputera z Explorer - Sieæ:\n(Nazwa musi byæ identyczna jak w sieci!)";
            this.lblInstruction.ForeColor = Color.DarkBlue;
            this.lblInstruction.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);

            // lblExample
            this.lblExample.Location = new Point(12, 55);
            this.lblExample.Name = "lblExample";
            this.lblExample.Size = new Size(360, 20);
            this.lblExample.Text = "Przyk³ad: DESKTOP-ABC123, LAPTOP-USER, SERWER01";
            this.lblExample.ForeColor = Color.Gray;
            this.lblExample.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Italic);

            // txtComputerName
            this.txtComputerName.Location = new Point(12, 80);
            this.txtComputerName.Name = "txtComputerName";
            this.txtComputerName.Size = new Size(280, 23);
            this.txtComputerName.PlaceholderText = "Wpisz nazwê komputera (np. DESKTOP-ABC123)";
            this.txtComputerName.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular);
            this.txtComputerName.KeyPress += txtComputerName_KeyPress;

            // btnCheckClipboard
            this.btnCheckClipboard.Location = new Point(297, 80);
            this.btnCheckClipboard.Name = "btnCheckClipboard";
            this.btnCheckClipboard.Size = new Size(75, 23);
            this.btnCheckClipboard.Text = "Schowek";
            this.btnCheckClipboard.UseVisualStyleBackColor = true;
            this.btnCheckClipboard.Click += btnCheckClipboard_Click;
            this.btnCheckClipboard.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular);

            // btnOK
            this.btnOK.Location = new Point(217, 115);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(75, 30);
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += btnOK_Click;

            // btnCancel
            this.btnCancel.Location = new Point(297, 115);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(75, 30);
            this.btnCancel.Text = "Anuluj";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += btnCancel_Click;

            // ComputerNameInputForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 157);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.lblExample);
            this.Controls.Add(this.txtComputerName);
            this.Controls.Add(this.btnCheckClipboard);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ComputerNameInputForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Wpisz nazwê komputera";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}