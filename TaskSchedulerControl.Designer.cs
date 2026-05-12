namespace Aron_V3
{
	partial class TaskSchedulerControl
	{
		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.TableLayoutPanel rootLayout;

		private System.Windows.Forms.TableLayoutPanel leftLayout;
		private System.Windows.Forms.Panel panelJobs;
		private System.Windows.Forms.Panel panelTasks;

		private System.Windows.Forms.Panel panelJobHeader;
		private System.Windows.Forms.Panel panelTaskHeader;
		private System.Windows.Forms.Label lblJobsTitle;
		private System.Windows.Forms.Label lblTasksTitle;

		private System.Windows.Forms.ListBox listJobs;
		private System.Windows.Forms.ListBox listTasks;

		private System.Windows.Forms.Panel panelStepList;
		private System.Windows.Forms.Panel panelStepListHeader;
		private System.Windows.Forms.FlowLayoutPanel panelStepIconBar;
		private System.Windows.Forms.Label lblStepListTitle;
		private System.Windows.Forms.Button btnAddStepItem;
		private System.Windows.Forms.Button btnBatchAddStepItem;
		private System.Windows.Forms.Button btnDeleteStepItem;
		private System.Windows.Forms.Button btnRefreshStepItem;
		private System.Windows.Forms.ListBox listSteps;

		private System.Windows.Forms.Panel panelSteps;
		private System.Windows.Forms.Label lblStepsTitle;
		private System.Windows.Forms.DataGridView dgvSteps;
		private System.Windows.Forms.TableLayoutPanel panelButtons;

		private System.Windows.Forms.Button btnAddStep;
		private System.Windows.Forms.Button btnDeleteSelected;
		private System.Windows.Forms.Button btnMoveUp;
		private System.Windows.Forms.Button btnMoveDown;
		private System.Windows.Forms.Button btnSave;

		private System.Windows.Forms.DataGridViewTextBoxColumn colStep;
		private System.Windows.Forms.DataGridViewTextBoxColumn colImageSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn colRunOrder;
		private System.Windows.Forms.DataGridViewTextBoxColumn colRemark;

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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
			this.leftLayout = new System.Windows.Forms.TableLayoutPanel();
			this.panelJobs = new System.Windows.Forms.Panel();
			this.listJobs = new System.Windows.Forms.ListBox();
			this.panelJobHeader = new System.Windows.Forms.Panel();
			this.lblJobsTitle = new System.Windows.Forms.Label();
			this.panelTasks = new System.Windows.Forms.Panel();
			this.listTasks = new System.Windows.Forms.ListBox();
			this.panelTaskHeader = new System.Windows.Forms.Panel();
			this.lblTasksTitle = new System.Windows.Forms.Label();
			this.panelStepList = new System.Windows.Forms.Panel();
			this.listSteps = new System.Windows.Forms.ListBox();
			this.panelStepIconBar = new System.Windows.Forms.FlowLayoutPanel();
			this.btnAddStepItem = new System.Windows.Forms.Button();
			this.btnBatchAddStepItem = new System.Windows.Forms.Button();
			this.btnDeleteStepItem = new System.Windows.Forms.Button();
			this.btnRefreshStepItem = new System.Windows.Forms.Button();
			this.panelStepListHeader = new System.Windows.Forms.Panel();
			this.lblStepListTitle = new System.Windows.Forms.Label();
			this.panelSteps = new System.Windows.Forms.Panel();
			this.dgvSteps = new System.Windows.Forms.DataGridView();
			this.colStep = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colImageSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colRunOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panelButtons = new System.Windows.Forms.TableLayoutPanel();
			this.btnAddStep = new System.Windows.Forms.Button();
			this.btnDeleteSelected = new System.Windows.Forms.Button();
			this.btnMoveUp = new System.Windows.Forms.Button();
			this.btnMoveDown = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.lblStepsTitle = new System.Windows.Forms.Label();
			this.rootLayout.SuspendLayout();
			this.leftLayout.SuspendLayout();
			this.panelJobs.SuspendLayout();
			this.panelJobHeader.SuspendLayout();
			this.panelTasks.SuspendLayout();
			this.panelTaskHeader.SuspendLayout();
			this.panelStepList.SuspendLayout();
			this.panelStepIconBar.SuspendLayout();
			this.panelStepListHeader.SuspendLayout();
			this.panelSteps.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvSteps)).BeginInit();
			this.panelButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// rootLayout
			// 
			this.rootLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.rootLayout.ColumnCount = 3;
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 330F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Controls.Add(this.leftLayout, 0, 0);
			this.rootLayout.Controls.Add(this.panelStepList, 1, 0);
			this.rootLayout.Controls.Add(this.panelSteps, 2, 0);
			this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rootLayout.Location = new System.Drawing.Point(0, 0);
			this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
			this.rootLayout.Name = "rootLayout";
			this.rootLayout.RowCount = 1;
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Size = new System.Drawing.Size(1200, 700);
			this.rootLayout.TabIndex = 0;
			// 
			// leftLayout
			// 
			this.leftLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.leftLayout.ColumnCount = 1;
			this.leftLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.leftLayout.Controls.Add(this.panelJobs, 0, 0);
			this.leftLayout.Controls.Add(this.panelTasks, 0, 1);
			this.leftLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.leftLayout.Location = new System.Drawing.Point(0, 0);
			this.leftLayout.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
			this.leftLayout.Name = "leftLayout";
			this.leftLayout.RowCount = 2;
			this.leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.leftLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.leftLayout.Size = new System.Drawing.Size(316, 700);
			this.leftLayout.TabIndex = 0;
			// 
			// panelJobs
			// 
			this.panelJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelJobs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelJobs.Controls.Add(this.listJobs);
			this.panelJobs.Controls.Add(this.panelJobHeader);
			this.panelJobs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelJobs.Location = new System.Drawing.Point(0, 0);
			this.panelJobs.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
			this.panelJobs.Name = "panelJobs";
			this.panelJobs.Padding = new System.Windows.Forms.Padding(16, 14, 16, 16);
			this.panelJobs.Size = new System.Drawing.Size(316, 336);
			this.panelJobs.TabIndex = 0;
			// 
			// listJobs
			// 
			this.listJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.listJobs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listJobs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listJobs.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
			this.listJobs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.listJobs.FormattingEnabled = true;
			this.listJobs.ItemHeight = 25;
			this.listJobs.Location = new System.Drawing.Point(16, 56);
			this.listJobs.Name = "listJobs";
			this.listJobs.Size = new System.Drawing.Size(282, 262);
			this.listJobs.TabIndex = 0;
			this.listJobs.DoubleClick += new System.EventHandler(this.listJobs_DoubleClick);
			// 
			// panelJobHeader
			// 
			this.panelJobHeader.Controls.Add(this.lblJobsTitle);
			this.panelJobHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelJobHeader.Location = new System.Drawing.Point(16, 14);
			this.panelJobHeader.Margin = new System.Windows.Forms.Padding(0);
			this.panelJobHeader.Name = "panelJobHeader";
			this.panelJobHeader.Size = new System.Drawing.Size(282, 42);
			this.panelJobHeader.TabIndex = 1;
			// 
			// lblJobsTitle
			// 
			this.lblJobsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblJobsTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblJobsTitle.ForeColor = System.Drawing.Color.White;
			this.lblJobsTitle.Location = new System.Drawing.Point(0, 0);
			this.lblJobsTitle.Name = "lblJobsTitle";
			this.lblJobsTitle.Size = new System.Drawing.Size(282, 42);
			this.lblJobsTitle.TabIndex = 0;
			this.lblJobsTitle.Text = "所有 JobID";
			this.lblJobsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelTasks
			// 
			this.panelTasks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelTasks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTasks.Controls.Add(this.listTasks);
			this.panelTasks.Controls.Add(this.panelTaskHeader);
			this.panelTasks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTasks.Location = new System.Drawing.Point(0, 350);
			this.panelTasks.Margin = new System.Windows.Forms.Padding(0);
			this.panelTasks.Name = "panelTasks";
			this.panelTasks.Padding = new System.Windows.Forms.Padding(16, 14, 16, 16);
			this.panelTasks.Size = new System.Drawing.Size(316, 350);
			this.panelTasks.TabIndex = 1;
			// 
			// listTasks
			// 
			this.listTasks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.listTasks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listTasks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listTasks.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
			this.listTasks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.listTasks.FormattingEnabled = true;
			this.listTasks.ItemHeight = 25;
			this.listTasks.Location = new System.Drawing.Point(16, 56);
			this.listTasks.Name = "listTasks";
			this.listTasks.Size = new System.Drawing.Size(282, 276);
			this.listTasks.TabIndex = 0;
			this.listTasks.DoubleClick += new System.EventHandler(this.listTasks_DoubleClick);
			// 
			// panelTaskHeader
			// 
			this.panelTaskHeader.Controls.Add(this.lblTasksTitle);
			this.panelTaskHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTaskHeader.Location = new System.Drawing.Point(16, 14);
			this.panelTaskHeader.Margin = new System.Windows.Forms.Padding(0);
			this.panelTaskHeader.Name = "panelTaskHeader";
			this.panelTaskHeader.Size = new System.Drawing.Size(282, 42);
			this.panelTaskHeader.TabIndex = 1;
			// 
			// lblTasksTitle
			// 
			this.lblTasksTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTasksTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblTasksTitle.ForeColor = System.Drawing.Color.White;
			this.lblTasksTitle.Location = new System.Drawing.Point(0, 0);
			this.lblTasksTitle.Name = "lblTasksTitle";
			this.lblTasksTitle.Size = new System.Drawing.Size(282, 42);
			this.lblTasksTitle.TabIndex = 0;
			this.lblTasksTitle.Text = "所有 task";
			this.lblTasksTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelStepList
			// 
			this.panelStepList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelStepList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelStepList.Controls.Add(this.listSteps);
			this.panelStepList.Controls.Add(this.panelStepIconBar);
			this.panelStepList.Controls.Add(this.panelStepListHeader);
			this.panelStepList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelStepList.Location = new System.Drawing.Point(330, 0);
			this.panelStepList.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
			this.panelStepList.Name = "panelStepList";
			this.panelStepList.Padding = new System.Windows.Forms.Padding(16, 14, 16, 16);
			this.panelStepList.Size = new System.Drawing.Size(286, 700);
			this.panelStepList.TabIndex = 2;
			// 
			// listSteps
			// 
			this.listSteps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.listSteps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listSteps.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listSteps.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
			this.listSteps.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.listSteps.FormattingEnabled = true;
			this.listSteps.ItemHeight = 25;
			this.listSteps.Location = new System.Drawing.Point(16, 110);
			this.listSteps.Name = "listSteps";
			this.listSteps.Size = new System.Drawing.Size(252, 572);
			this.listSteps.TabIndex = 0;
			this.listSteps.DoubleClick += new System.EventHandler(this.listSteps_DoubleClick);
			// 
			// panelStepIconBar
			// 
			this.panelStepIconBar.Controls.Add(this.btnAddStepItem);
			this.panelStepIconBar.Controls.Add(this.btnBatchAddStepItem);
			this.panelStepIconBar.Controls.Add(this.btnDeleteStepItem);
			this.panelStepIconBar.Controls.Add(this.btnRefreshStepItem);
			this.panelStepIconBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelStepIconBar.Location = new System.Drawing.Point(16, 56);
			this.panelStepIconBar.Margin = new System.Windows.Forms.Padding(0);
			this.panelStepIconBar.Name = "panelStepIconBar";
			this.panelStepIconBar.Padding = new System.Windows.Forms.Padding(0, 4, 0, 6);
			this.panelStepIconBar.Size = new System.Drawing.Size(252, 54);
			this.panelStepIconBar.TabIndex = 1;
			this.panelStepIconBar.WrapContents = false;
			// 
			// btnAddStepItem
			// 
			this.btnAddStepItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnAddStepItem.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(145)))), ((int)(((byte)(205)))));
			this.btnAddStepItem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnAddStepItem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnAddStepItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddStepItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 14F, System.Drawing.FontStyle.Bold);
			this.btnAddStepItem.ForeColor = System.Drawing.Color.White;
			this.btnAddStepItem.Location = new System.Drawing.Point(0, 4);
			this.btnAddStepItem.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
			this.btnAddStepItem.Name = "btnAddStepItem";
			this.btnAddStepItem.Size = new System.Drawing.Size(42, 42);
			this.btnAddStepItem.TabIndex = 0;
			this.btnAddStepItem.Text = "+";
			this.btnAddStepItem.UseVisualStyleBackColor = false;
			this.btnAddStepItem.Click += new System.EventHandler(this.btnAddStepItem_Click);
			// 
			// btnBatchAddStepItem
			// 
			this.btnBatchAddStepItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnBatchAddStepItem.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(145)))), ((int)(((byte)(205)))));
			this.btnBatchAddStepItem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnBatchAddStepItem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnBatchAddStepItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBatchAddStepItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
			this.btnBatchAddStepItem.ForeColor = System.Drawing.Color.White;
			this.btnBatchAddStepItem.Location = new System.Drawing.Point(50, 4);
			this.btnBatchAddStepItem.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
			this.btnBatchAddStepItem.Name = "btnBatchAddStepItem";
			this.btnBatchAddStepItem.Size = new System.Drawing.Size(42, 42);
			this.btnBatchAddStepItem.TabIndex = 1;
			this.btnBatchAddStepItem.Text = "▦";
			this.btnBatchAddStepItem.UseVisualStyleBackColor = false;
			this.btnBatchAddStepItem.Click += new System.EventHandler(this.btnBatchAddStepItem_Click);
			// 
			// btnDeleteStepItem
			// 
			this.btnDeleteStepItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnDeleteStepItem.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(145)))), ((int)(((byte)(205)))));
			this.btnDeleteStepItem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnDeleteStepItem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnDeleteStepItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDeleteStepItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 14F, System.Drawing.FontStyle.Bold);
			this.btnDeleteStepItem.ForeColor = System.Drawing.Color.White;
			this.btnDeleteStepItem.Location = new System.Drawing.Point(100, 4);
			this.btnDeleteStepItem.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
			this.btnDeleteStepItem.Name = "btnDeleteStepItem";
			this.btnDeleteStepItem.Size = new System.Drawing.Size(42, 42);
			this.btnDeleteStepItem.TabIndex = 2;
			this.btnDeleteStepItem.Text = "-";
			this.btnDeleteStepItem.UseVisualStyleBackColor = false;
			this.btnDeleteStepItem.Click += new System.EventHandler(this.btnDeleteStepItem_Click);
			// 
			// btnRefreshStepItem
			// 
			this.btnRefreshStepItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnRefreshStepItem.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(145)))), ((int)(((byte)(205)))));
			this.btnRefreshStepItem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnRefreshStepItem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnRefreshStepItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRefreshStepItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold);
			this.btnRefreshStepItem.ForeColor = System.Drawing.Color.White;
			this.btnRefreshStepItem.Location = new System.Drawing.Point(150, 4);
			this.btnRefreshStepItem.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
			this.btnRefreshStepItem.Name = "btnRefreshStepItem";
			this.btnRefreshStepItem.Size = new System.Drawing.Size(42, 42);
			this.btnRefreshStepItem.TabIndex = 3;
			this.btnRefreshStepItem.Text = "↻";
			this.btnRefreshStepItem.UseVisualStyleBackColor = false;
			this.btnRefreshStepItem.Click += new System.EventHandler(this.btnRefreshStepItem_Click);
			// 
			// panelStepListHeader
			// 
			this.panelStepListHeader.Controls.Add(this.lblStepListTitle);
			this.panelStepListHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelStepListHeader.Location = new System.Drawing.Point(16, 14);
			this.panelStepListHeader.Margin = new System.Windows.Forms.Padding(0);
			this.panelStepListHeader.Name = "panelStepListHeader";
			this.panelStepListHeader.Size = new System.Drawing.Size(252, 42);
			this.panelStepListHeader.TabIndex = 2;
			// 
			// lblStepListTitle
			// 
			this.lblStepListTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStepListTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblStepListTitle.ForeColor = System.Drawing.Color.White;
			this.lblStepListTitle.Location = new System.Drawing.Point(0, 0);
			this.lblStepListTitle.Name = "lblStepListTitle";
			this.lblStepListTitle.Size = new System.Drawing.Size(252, 42);
			this.lblStepListTitle.TabIndex = 0;
			this.lblStepListTitle.Text = "所有 step";
			this.lblStepListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelSteps
			// 
			this.panelSteps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelSteps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSteps.Controls.Add(this.dgvSteps);
			this.panelSteps.Controls.Add(this.panelButtons);
			this.panelSteps.Controls.Add(this.lblStepsTitle);
			this.panelSteps.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelSteps.Location = new System.Drawing.Point(630, 0);
			this.panelSteps.Margin = new System.Windows.Forms.Padding(0);
			this.panelSteps.Name = "panelSteps";
			this.panelSteps.Padding = new System.Windows.Forms.Padding(18, 16, 18, 18);
			this.panelSteps.Size = new System.Drawing.Size(570, 700);
			this.panelSteps.TabIndex = 3;
			// 
			// dgvSteps
			// 
			this.dgvSteps.AllowUserToAddRows = false;
			this.dgvSteps.AllowUserToDeleteRows = false;
			this.dgvSteps.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvSteps.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(29)))), ((int)(((byte)(50)))));
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(29)))), ((int)(((byte)(50)))));
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
			this.dgvSteps.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dgvSteps.ColumnHeadersHeight = 42;
			this.dgvSteps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStep,
            this.colImageSource,
            this.colRunOrder,
            this.colRemark});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(210)))));
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvSteps.DefaultCellStyle = dataGridViewCellStyle2;
			this.dgvSteps.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvSteps.EnableHeadersVisualStyles = false;
			this.dgvSteps.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(80)))), ((int)(((byte)(105)))));
			this.dgvSteps.Location = new System.Drawing.Point(18, 58);
			this.dgvSteps.Name = "dgvSteps";
			this.dgvSteps.RowHeadersVisible = false;
			this.dgvSteps.RowHeadersWidth = 62;
			this.dgvSteps.RowTemplate.Height = 30;
			this.dgvSteps.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvSteps.Size = new System.Drawing.Size(532, 564);
			this.dgvSteps.TabIndex = 0;
			// 
			// colStep
			// 
			this.colStep.HeaderText = "step";
			this.colStep.MinimumWidth = 8;
			this.colStep.Name = "colStep";
			// 
			// colImageSource
			// 
			this.colImageSource.HeaderText = "图像源";
			this.colImageSource.MinimumWidth = 8;
			this.colImageSource.Name = "colImageSource";
			// 
			// colRunOrder
			// 
			this.colRunOrder.HeaderText = "执行步序";
			this.colRunOrder.MinimumWidth = 8;
			this.colRunOrder.Name = "colRunOrder";
			// 
			// colRemark
			// 
			this.colRemark.HeaderText = "备注";
			this.colRemark.MinimumWidth = 8;
			this.colRemark.Name = "colRemark";
			// 
			// panelButtons
			// 
			this.panelButtons.ColumnCount = 6;
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
			this.panelButtons.Controls.Add(this.btnAddStep, 0, 0);
			this.panelButtons.Controls.Add(this.btnDeleteSelected, 1, 0);
			this.panelButtons.Controls.Add(this.btnMoveUp, 2, 0);
			this.panelButtons.Controls.Add(this.btnMoveDown, 3, 0);
			this.panelButtons.Controls.Add(this.btnSave, 5, 0);
			this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelButtons.Location = new System.Drawing.Point(18, 622);
			this.panelButtons.Margin = new System.Windows.Forms.Padding(0);
			this.panelButtons.Name = "panelButtons";
			this.panelButtons.Padding = new System.Windows.Forms.Padding(0, 10, 0, 8);
			this.panelButtons.RowCount = 1;
			this.panelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.panelButtons.Size = new System.Drawing.Size(532, 58);
			this.panelButtons.TabIndex = 1;
			// 
			// btnAddStep
			// 
			this.btnAddStep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnAddStep.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnAddStep.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnAddStep.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnAddStep.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnAddStep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddStep.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnAddStep.ForeColor = System.Drawing.Color.White;
			this.btnAddStep.Location = new System.Drawing.Point(0, 10);
			this.btnAddStep.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this.btnAddStep.Name = "btnAddStep";
			this.btnAddStep.Size = new System.Drawing.Size(130, 40);
			this.btnAddStep.TabIndex = 0;
			this.btnAddStep.Text = "+  新增算子";
			this.btnAddStep.UseVisualStyleBackColor = false;
			this.btnAddStep.Click += new System.EventHandler(this.btnAddStep_Click);
			// 
			// btnDeleteSelected
			// 
			this.btnDeleteSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnDeleteSelected.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnDeleteSelected.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnDeleteSelected.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnDeleteSelected.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnDeleteSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDeleteSelected.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnDeleteSelected.ForeColor = System.Drawing.Color.White;
			this.btnDeleteSelected.Location = new System.Drawing.Point(140, 10);
			this.btnDeleteSelected.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this.btnDeleteSelected.Name = "btnDeleteSelected";
			this.btnDeleteSelected.Size = new System.Drawing.Size(140, 40);
			this.btnDeleteSelected.TabIndex = 1;
			this.btnDeleteSelected.Text = "▦  删除选中";
			this.btnDeleteSelected.UseVisualStyleBackColor = false;
			this.btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);
			// 
			// btnMoveUp
			// 
			this.btnMoveUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnMoveUp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnMoveUp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnMoveUp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnMoveUp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnMoveUp.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnMoveUp.ForeColor = System.Drawing.Color.White;
			this.btnMoveUp.Location = new System.Drawing.Point(290, 10);
			this.btnMoveUp.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this.btnMoveUp.Name = "btnMoveUp";
			this.btnMoveUp.Size = new System.Drawing.Size(140, 40);
			this.btnMoveUp.TabIndex = 2;
			this.btnMoveUp.Text = "▲  上移选中";
			this.btnMoveUp.UseVisualStyleBackColor = false;
			this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
			// 
			// btnMoveDown
			// 
			this.btnMoveDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnMoveDown.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnMoveDown.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnMoveDown.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnMoveDown.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnMoveDown.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnMoveDown.ForeColor = System.Drawing.Color.White;
			this.btnMoveDown.Location = new System.Drawing.Point(440, 10);
			this.btnMoveDown.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this.btnMoveDown.Name = "btnMoveDown";
			this.btnMoveDown.Size = new System.Drawing.Size(140, 40);
			this.btnMoveDown.TabIndex = 3;
			this.btnMoveDown.Text = "▼  下移选中";
			this.btnMoveDown.UseVisualStyleBackColor = false;
			this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
			// 
			// btnSave
			// 
			this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(90)))), ((int)(((byte)(215)))));
			this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
			this.btnSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(70)))), ((int)(((byte)(190)))));
			this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(110)))), ((int)(((byte)(245)))));
			this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnSave.ForeColor = System.Drawing.Color.White;
			this.btnSave.Location = new System.Drawing.Point(412, 10);
			this.btnSave.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(120, 40);
			this.btnSave.TabIndex = 4;
			this.btnSave.Text = "▣  保存";
			this.btnSave.UseVisualStyleBackColor = false;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// lblStepsTitle
			// 
			this.lblStepsTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblStepsTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblStepsTitle.ForeColor = System.Drawing.Color.White;
			this.lblStepsTitle.Location = new System.Drawing.Point(18, 16);
			this.lblStepsTitle.Name = "lblStepsTitle";
			this.lblStepsTitle.Size = new System.Drawing.Size(532, 42);
			this.lblStepsTitle.TabIndex = 2;
			this.lblStepsTitle.Text = "当前 step 的详细信息";
			this.lblStepsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TaskSchedulerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.Controls.Add(this.rootLayout);
			this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.Name = "TaskSchedulerControl";
			this.Size = new System.Drawing.Size(1200, 700);
			this.rootLayout.ResumeLayout(false);
			this.leftLayout.ResumeLayout(false);
			this.panelJobs.ResumeLayout(false);
			this.panelJobHeader.ResumeLayout(false);
			this.panelTasks.ResumeLayout(false);
			this.panelTaskHeader.ResumeLayout(false);
			this.panelStepList.ResumeLayout(false);
			this.panelStepIconBar.ResumeLayout(false);
			this.panelStepListHeader.ResumeLayout(false);
			this.panelSteps.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvSteps)).EndInit();
			this.panelButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}
}
