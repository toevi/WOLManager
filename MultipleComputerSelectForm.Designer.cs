namespace WOLManager
{
    partial class MultipleComputerSelectForm
    {
        private System.ComponentModel.IContainer components = null;
        private ListView listViewComputers;
        private Button btnScanNetwork;
        private Button btnAddSelected;
        private Button btnCancel;
        private Button btnSelectAll;
        private Button btnDeselectAll;
        private Label lblTitle;
        private Label lblInstructions;
        private Label lblStatus;
        private ProgressBar progressBar;

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
            this.listViewComputers = new ListView();
            this.btnScanNetwork = new Button();
            this.btnAddSelected = new Button();
            this.btnCancel = new Button();
            this.btnSelectAll = new Button();
            this.btnDeselectAll = new Button();
            this.lblTitle = new Label();
            this.lblInstructions = new Label();
            this.lblStatus = new Label();
            this.progressBar = new ProgressBar();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.Location = new Point(12, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(560, 30);
            this.lblTitle.Text = "NETWORK SCANNER - Multiple Computer Selection";
            this.lblTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.DarkBlue;
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // btnScanNetwork
            this.btnScanNetwork.Location = new Point(12, 50);
            this.btnScanNetwork.Name = "btnScanNetwork";
            this.btnScanNetwork.Size = new Size(560, 35);
            this.btnScanNetwork.Text = "SCAN NETWORK";
            this.btnScanNetwork.UseVisualStyleBackColor = false;
            this.btnScanNetwork.BackColor = Color.FromArgb(0, 120, 215);
            this.btnScanNetwork.ForeColor = Color.White;
            this.btnScanNetwork.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            this.btnScanNetwork.FlatStyle = FlatStyle.Flat;
            this.btnScanNetwork.FlatAppearance.BorderSize = 0;
            this.btnScanNetwork.Click += new EventHandler(this.btnScanNetwork_Click);

            // progressBar
            this.progressBar.Location = new Point(12, 95);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(560, 20);
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;
            this.progressBar.Visible = false;

            // lblStatus
            this.lblStatus.Location = new Point(12, 120);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(560, 20);
            this.lblStatus.Text = "Click 'SCAN NETWORK' to start";
            this.lblStatus.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Italic);
            this.lblStatus.ForeColor = Color.Gray;
            this.lblStatus.TextAlign = ContentAlignment.MiddleCenter;

            // lblInstructions
            this.lblInstructions.Location = new Point(12, 145);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new Size(560, 20);
            this.lblInstructions.Text = "Instructions will appear here after scanning";
            this.lblInstructions.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            this.lblInstructions.ForeColor = Color.DarkGreen;
            this.lblInstructions.Visible = false;

            // listViewComputers
            this.listViewComputers.Location = new Point(12, 170);
            this.listViewComputers.Name = "listViewComputers";
            this.listViewComputers.Size = new Size(560, 220);
            this.listViewComputers.UseCompatibleStateImageBehavior = false;
            this.listViewComputers.View = View.Details;
            this.listViewComputers.FullRowSelect = true;
            this.listViewComputers.GridLines = true;
            this.listViewComputers.CheckBoxes = true; // Enable checkboxes
            this.listViewComputers.MultiSelect = true;
            this.listViewComputers.ItemCheck += new ItemCheckEventHandler(this.listViewComputers_ItemCheck);

            // ListView columns
            this.listViewComputers.Columns.Add("Select", 30); // Checkbox column
            this.listViewComputers.Columns.Add("Computer Name", 250);
            this.listViewComputers.Columns.Add("IP Address", 150);
            this.listViewComputers.Columns.Add("Status", 120);

            // btnSelectAll
            this.btnSelectAll.Location = new Point(12, 400);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new Size(100, 30);
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new EventHandler(this.btnSelectAll_Click);

            // btnDeselectAll
            this.btnDeselectAll.Location = new Point(120, 400);
            this.btnDeselectAll.Name = "btnDeselectAll";
            this.btnDeselectAll.Size = new Size(100, 30);
            this.btnDeselectAll.Text = "Deselect All";
            this.btnDeselectAll.UseVisualStyleBackColor = true;
            this.btnDeselectAll.Click += new EventHandler(this.btnDeselectAll_Click);

            // btnAddSelected
            this.btnAddSelected.Location = new Point(350, 400);
            this.btnAddSelected.Name = "btnAddSelected";
            this.btnAddSelected.Size = new Size(130, 30);
            this.btnAddSelected.Text = "ADD SELECTED";
            this.btnAddSelected.UseVisualStyleBackColor = false;
            this.btnAddSelected.BackColor = Color.FromArgb(0, 150, 0);
            this.btnAddSelected.ForeColor = Color.White;
            this.btnAddSelected.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            this.btnAddSelected.FlatStyle = FlatStyle.Flat;
            this.btnAddSelected.FlatAppearance.BorderSize = 0;
            this.btnAddSelected.Enabled = false;
            this.btnAddSelected.Click += new EventHandler(this.btnAddSelected_Click);

            // btnCancel
            this.btnCancel.Location = new Point(490, 400);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(82, 30);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // MultipleComputerSelectForm
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(584, 450);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnScanNetwork);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblInstructions);
            this.Controls.Add(this.listViewComputers);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.btnDeselectAll);
            this.Controls.Add(this.btnAddSelected);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MultipleComputerSelectForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Network Scanner - Multiple Selection";
            this.ResumeLayout(false);
        }
    }
}