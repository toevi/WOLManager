namespace WOLManager
{
    partial class ComputerEditForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.TextBox txtMac;
        private System.Windows.Forms.TextBox txtBroadcast;
        private System.Windows.Forms.ComboBox cmbPingType;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblNameInfo;

        private void InitializeComponent()
        {
            txtName = new TextBox();
            txtIP = new TextBox();
            numPort = new NumericUpDown();
            txtMac = new TextBox();
            txtBroadcast = new TextBox();
            cmbPingType = new ComboBox();
            btnOK = new Button();
            btnCancel = new Button();
            lblNameInfo = new Label();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            SuspendLayout();
            // 
            // txtName
            // 
            txtName.Location = new Point(12, 55);
            txtName.Name = "txtName";
            txtName.PlaceholderText = "Enter network computer name";
            txtName.Size = new Size(400, 23);
            txtName.TabIndex = 1;
            // 
            // txtIP
            // 
            txtIP.Location = new Point(12, 84);
            txtIP.Name = "txtIP";
            txtIP.PlaceholderText = "IP address (optional, but name is more important)";
            txtIP.Size = new Size(400, 23);
            txtIP.TabIndex = 2;
            // 
            // numPort
            // 
            numPort.Location = new Point(12, 113);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(400, 23);
            numPort.TabIndex = 3;
            numPort.Value = new decimal(new int[] { 9, 0, 0, 0 });
            // 
            // txtMac
            // 
            txtMac.Location = new Point(12, 142);
            txtMac.Name = "txtMac";
            txtMac.PlaceholderText = "MAC address (XX:XX:XX:XX:XX:XX)";
            txtMac.Size = new Size(400, 23);
            txtMac.TabIndex = 4;
            // 
            // txtBroadcast
            // 
            txtBroadcast.Location = new Point(12, 171);
            txtBroadcast.Name = "txtBroadcast";
            txtBroadcast.PlaceholderText = "Broadcast address (e.g. 192.168.1.255)";
            txtBroadcast.Size = new Size(400, 23);
            txtBroadcast.TabIndex = 5;
            // 
            // cmbPingType
            // 
            cmbPingType.Items.AddRange(new object[] { "ICMP", "TCP" });
            cmbPingType.Location = new Point(12, 200);
            cmbPingType.Name = "cmbPingType";
            cmbPingType.Size = new Size(400, 23);
            cmbPingType.TabIndex = 6;
            //
            // btnOK
            //
            btnOK.Location = new Point(120, 239);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(80, 30);
            btnOK.TabIndex = 7;
            btnOK.Text = "OK";
            btnOK.Click += btnOK_Click;
            //
            // btnCancel
            //
            btnCancel.Location = new Point(220, 239);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            // 
            // lblNameInfo
            // 
            lblNameInfo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            lblNameInfo.ForeColor = Color.DarkBlue;
            lblNameInfo.Location = new Point(12, 12);
            lblNameInfo.Name = "lblNameInfo";
            lblNameInfo.Size = new Size(400, 40);
            lblNameInfo.TabIndex = 0;
            lblNameInfo.Text = "COMPUTER NAME (REQUIRED):\r\nMust be identical to network name!\r\nor use Add Scan in the main window";
            // 
            // ComputerEditForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(424, 284);
            Controls.Add(lblNameInfo);
            Controls.Add(txtName);
            Controls.Add(txtIP);
            Controls.Add(numPort);
            Controls.Add(txtMac);
            Controls.Add(txtBroadcast);
            Controls.Add(cmbPingType);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ComputerEditForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add/Edit Computer";
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
