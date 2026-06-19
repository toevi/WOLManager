namespace WOLManager
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            components = new System.ComponentModel.Container();

            dgvComputers      = new DataGridView();
            statusStrip       = new StatusStrip();
            statusLabel       = new ToolStripStatusLabel();

            grpPower          = new GroupBox();
            grpRemote         = new GroupBox();
            grpComputers      = new GroupBox();

            btnWake           = new Button();
            btnRestart        = new Button();
            btnShutdown       = new Button();
            btnRDP            = new Button();
            btnNetworkShare   = new Button();
            btnSSH            = new Button();
            btnInfo           = new Button();
            btnAdd            = new Button();
            btnAddAuto        = new Button();
            btnEdit           = new Button();
            btnRemove         = new Button();
            btnSetName        = new Button();
            btnHelp           = new Button();

            notifyIcon1       = new NotifyIcon(components);
            contextMenuTray   = new ContextMenuStrip(components);
            menuItemOpen      = new ToolStripMenuItem();
            menuItemSep       = new ToolStripSeparator();
            menuItemExit      = new ToolStripMenuItem();

            ((System.ComponentModel.ISupportInitialize)dgvComputers).BeginInit();
            statusStrip.SuspendLayout();
            grpPower.SuspendLayout();
            grpRemote.SuspendLayout();
            grpComputers.SuspendLayout();
            SuspendLayout();

            // contextMenuTray
            contextMenuTray.Items.AddRange(new ToolStripItem[] { menuItemOpen, menuItemSep, menuItemExit });
            menuItemOpen.Text = "Open WOL Manager";
            menuItemExit.Text = "Exit";

            // notifyIcon1
            notifyIcon1.ContextMenuStrip = contextMenuTray;
            notifyIcon1.Text = "WOL Manager";
            notifyIcon1.Visible = false;

            // dgvComputers
            dgvComputers.AllowUserToAddRows = false;
            dgvComputers.AllowUserToDeleteRows = false;
            dgvComputers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvComputers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvComputers.Location = new Point(12, 12);
            dgvComputers.Name = "dgvComputers";
            dgvComputers.ReadOnly = true;
            dgvComputers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvComputers.Size = new Size(736, 474);
            dgvComputers.TabIndex = 0;

            // ── GroupBox: Power ──────────────────────────────────
            grpPower.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpPower.Location = new Point(758, 12);
            grpPower.Name = "grpPower";
            grpPower.Size = new Size(210, 112);
            grpPower.TabIndex = 10;
            grpPower.TabStop = false;
            grpPower.Text = "Power";

            btnWake.Location = new Point(8, 22);
            btnWake.Name = "btnWake";
            btnWake.Size = new Size(194, 28);
            btnWake.TabIndex = 1;
            btnWake.Text = "Wake Up";
            btnWake.UseVisualStyleBackColor = true;

            btnRestart.Location = new Point(8, 55);
            btnRestart.Name = "btnRestart";
            btnRestart.Size = new Size(93, 28);
            btnRestart.TabIndex = 2;
            btnRestart.Text = "Restart";
            btnRestart.UseVisualStyleBackColor = true;

            btnShutdown.Location = new Point(109, 55);
            btnShutdown.Name = "btnShutdown";
            btnShutdown.Size = new Size(93, 28);
            btnShutdown.TabIndex = 3;
            btnShutdown.Text = "Shutdown";
            btnShutdown.UseVisualStyleBackColor = true;

            grpPower.Controls.Add(btnWake);
            grpPower.Controls.Add(btnRestart);
            grpPower.Controls.Add(btnShutdown);

            // ── GroupBox: Remote Access ──────────────────────────
            grpRemote.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpRemote.Location = new Point(758, 132);
            grpRemote.Name = "grpRemote";
            grpRemote.Size = new Size(210, 163);
            grpRemote.TabIndex = 11;
            grpRemote.TabStop = false;
            grpRemote.Text = "Remote Access";

            btnRDP.Location = new Point(8, 22);
            btnRDP.Name = "btnRDP";
            btnRDP.Size = new Size(194, 28);
            btnRDP.TabIndex = 6;
            btnRDP.Text = "RDP";
            btnRDP.UseVisualStyleBackColor = true;

            btnSSH.Location = new Point(8, 55);
            btnSSH.Name = "btnSSH";
            btnSSH.Size = new Size(194, 28);
            btnSSH.TabIndex = 7;
            btnSSH.Text = "SSH";
            btnSSH.UseVisualStyleBackColor = true;

            btnNetworkShare.Location = new Point(8, 88);
            btnNetworkShare.Name = "btnNetworkShare";
            btnNetworkShare.Size = new Size(194, 28);
            btnNetworkShare.TabIndex = 8;
            btnNetworkShare.Text = "Network Share";
            btnNetworkShare.UseVisualStyleBackColor = true;

            btnInfo.Location = new Point(8, 121);
            btnInfo.Name = "btnInfo";
            btnInfo.Size = new Size(194, 28);
            btnInfo.TabIndex = 9;
            btnInfo.Text = "Info / Port Scan";
            btnInfo.UseVisualStyleBackColor = true;

            grpRemote.Controls.Add(btnRDP);
            grpRemote.Controls.Add(btnSSH);
            grpRemote.Controls.Add(btnNetworkShare);
            grpRemote.Controls.Add(btnInfo);

            // ── GroupBox: Computers ──────────────────────────────
            grpComputers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpComputers.Location = new Point(758, 303);
            grpComputers.Name = "grpComputers";
            grpComputers.Size = new Size(210, 112);
            grpComputers.TabIndex = 12;
            grpComputers.TabStop = false;
            grpComputers.Text = "Computers";

            btnAdd.Location = new Point(8, 22);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(93, 28);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;

            btnAddAuto.Location = new Point(109, 22);
            btnAddAuto.Name = "btnAddAuto";
            btnAddAuto.Size = new Size(93, 28);
            btnAddAuto.TabIndex = 3;
            btnAddAuto.Text = "Add Scan";
            btnAddAuto.UseVisualStyleBackColor = true;

            btnEdit.Location = new Point(8, 55);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(93, 28);
            btnEdit.TabIndex = 4;
            btnEdit.Text = "Edit";
            btnEdit.UseVisualStyleBackColor = true;

            btnRemove.Location = new Point(109, 55);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(93, 28);
            btnRemove.TabIndex = 5;
            btnRemove.Text = "Remove";
            btnRemove.UseVisualStyleBackColor = true;

            grpComputers.Controls.Add(btnAdd);
            grpComputers.Controls.Add(btnAddAuto);
            grpComputers.Controls.Add(btnEdit);
            grpComputers.Controls.Add(btnRemove);

            // Standalone: Refresh / Help (anchored to bottom)
            btnSetName.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSetName.Location = new Point(758, 430);
            btnSetName.Name = "btnSetName";
            btnSetName.Size = new Size(210, 28);
            btnSetName.TabIndex = 9;
            btnSetName.Text = "Refresh";
            btnSetName.UseVisualStyleBackColor = true;

            btnHelp.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnHelp.Location = new Point(758, 462);
            btnHelp.Name = "btnHelp";
            btnHelp.Size = new Size(210, 28);
            btnHelp.TabIndex = 8;
            btnHelp.Text = "Help";
            btnHelp.UseVisualStyleBackColor = true;

            // statusStrip
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 498);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(980, 22);
            statusStrip.TabIndex = 9;
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(48, 17);
            statusLabel.Text = "Ready";

            // Form1
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 520);
            MinimumSize = new Size(820, 500);
            Controls.Add(dgvComputers);
            Controls.Add(grpPower);
            Controls.Add(grpRemote);
            Controls.Add(grpComputers);
            Controls.Add(btnSetName);
            Controls.Add(btnHelp);
            Controls.Add(statusStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "WOL MANAGER tmfgroup";
            Load += Form1_Load;

            ((System.ComponentModel.ISupportInitialize)dgvComputers).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            grpPower.ResumeLayout(false);
            grpRemote.ResumeLayout(false);
            grpComputers.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvComputers;
        private System.Windows.Forms.Button btnWake;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnShutdown;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAddAuto;
        private System.Windows.Forms.Button btnRDP;
        private System.Windows.Forms.Button btnSSH;
        private System.Windows.Forms.Button btnNetworkShare;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnSetName;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.GroupBox grpPower;
        private System.Windows.Forms.GroupBox grpRemote;
        private System.Windows.Forms.GroupBox grpComputers;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuTray;
        private System.Windows.Forms.ToolStripMenuItem menuItemOpen;
        private System.Windows.Forms.ToolStripSeparator menuItemSep;
        private System.Windows.Forms.ToolStripMenuItem menuItemExit;
    }
}
