namespace WOLManager
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            dgvComputers = new DataGridView();
            btnWake = new Button();
            btnAdd = new Button();
            btnEdit = new Button();
            btnRemove = new Button();
            btnRDP = new Button();
            btnNetworkShare = new Button();
            btnAddAuto = new Button();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            btnHelp = new Button();
            btnSetName = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvComputers).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // dgvComputers
            // 
            dgvComputers.AllowUserToAddRows = false;
            dgvComputers.AllowUserToDeleteRows = false;
            dgvComputers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvComputers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvComputers.Location = new Point(12, 12);
            dgvComputers.Name = "dgvComputers";
            dgvComputers.ReadOnly = true;
            dgvComputers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvComputers.Size = new Size(650, 400);
            dgvComputers.TabIndex = 0;
            // 
            // btnWake
            // 
            btnWake.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnWake.Location = new Point(680, 12);
            btnWake.Name = "btnWake";
            btnWake.Size = new Size(100, 30);
            btnWake.TabIndex = 1;
            btnWake.Text = "Wake Up";
            btnWake.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(680, 52);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(100, 30);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnAddAuto
            // 
            btnAddAuto.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddAuto.Location = new Point(680, 92);
            btnAddAuto.Name = "btnAddAuto";
            btnAddAuto.Size = new Size(100, 30);
            btnAddAuto.TabIndex = 3;
            btnAddAuto.Text = "Add Scan";
            btnAddAuto.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.Location = new Point(680, 132);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(100, 30);
            btnEdit.TabIndex = 4;
            btnEdit.Text = "Edit";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnRemove
            // 
            btnRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRemove.Location = new Point(680, 172);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(100, 30);
            btnRemove.TabIndex = 5;
            btnRemove.Text = "Remove";
            btnRemove.UseVisualStyleBackColor = true;
            // 
            // btnRDP
            // 
            btnRDP.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRDP.Location = new Point(680, 212);
            btnRDP.Name = "btnRDP";
            btnRDP.Size = new Size(100, 30);
            btnRDP.TabIndex = 6;
            btnRDP.Text = "RDP";
            btnRDP.UseVisualStyleBackColor = true;
            // 
            // btnNetworkShare
            // 
            btnNetworkShare.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNetworkShare.Location = new Point(680, 252);
            btnNetworkShare.Name = "btnNetworkShare";
            btnNetworkShare.Size = new Size(100, 30);
            btnNetworkShare.TabIndex = 7;
            btnNetworkShare.Text = "Network Share";
            btnNetworkShare.UseVisualStyleBackColor = true;
            // 
            // btnHelp
            // 
            btnHelp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHelp.Location = new Point(680, 332);
            btnHelp.Name = "btnHelp";
            btnHelp.Size = new Size(100, 30);
            btnHelp.TabIndex = 8;
            btnHelp.Text = "Help";
            btnHelp.UseVisualStyleBackColor = true;
            // 
            // btnSetName (now Refresh)
            // 
            btnSetName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSetName.Location = new Point(680, 292);
            btnSetName.Name = "btnSetName";
            btnSetName.Size = new Size(100, 30);
            btnSetName.TabIndex = 9;
            btnSetName.Text = "Refresh";
            btnSetName.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 428);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(800, 22);
            statusStrip.TabIndex = 9;
            statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(48, 17);
            statusLabel.Text = "Ready";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvComputers);
            Controls.Add(btnWake);
            Controls.Add(btnAdd);
            Controls.Add(btnAddAuto);
            Controls.Add(btnEdit);
            Controls.Add(btnRemove);
            Controls.Add(btnRDP);
            Controls.Add(btnNetworkShare);
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
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvComputers;
        private System.Windows.Forms.Button btnWake;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAddAuto;
        private System.Windows.Forms.Button btnRDP;
        private System.Windows.Forms.Button btnNetworkShare;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnSetName;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
