namespace Aron_V3
{
	partial class FlowConfigForm
	{
		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.Panel panelRoot;

		private System.Windows.Forms.Panel panelSide;
		private System.Windows.Forms.Button btnTriggerManager;
		private System.Windows.Forms.Button btnTaskScheduler;
		private System.Windows.Forms.Panel panelSidePattern;

		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.Panel panelContent;

		private System.Windows.Forms.Panel panelJobList;
		private System.Windows.Forms.Label lblJobTitle;
		private System.Windows.Forms.TextBox txtSearchJob;
		private System.Windows.Forms.ListBox listJobs;

		private System.Windows.Forms.Panel panelTaskList;
		private System.Windows.Forms.Label lblTaskTitle;
		private System.Windows.Forms.TextBox txtSearchTask;
		private System.Windows.Forms.ListBox listTasks;

		private System.Windows.Forms.Panel panelConfig;
		private System.Windows.Forms.Label lblMainSectionTitle;
		private System.Windows.Forms.Panel panelTableBody;
		private System.Windows.Forms.DataGridView dgvConfig;
		private System.Windows.Forms.Label lblEmpty;

		private System.Windows.Forms.Panel panelAction;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnMoveUp;
		private System.Windows.Forms.Button btnMoveDown;
		private System.Windows.Forms.Button btnSave;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();

			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			this.panelRoot = new System.Windows.Forms.Panel();
			this.panelMain = new System.Windows.Forms.Panel();
			this.panelContent = new System.Windows.Forms.Panel();
			this.panelJobList = new System.Windows.Forms.Panel();
			this.lblJobTitle = new System.Windows.Forms.Label();
			this.txtSearchJob = new System.Windows.Forms.TextBox();
			this.listJobs = new System.Windows.Forms.ListBox();
			this.panelTaskList = new System.Windows.Forms.Panel();
			this.lblTaskTitle = new System.Windows.Forms.Label();
			this.txtSearchTask = new System.Windows.Forms.TextBox();
			this.listTasks = new System.Windows.Forms.ListBox();
			this.panelConfig = new System.Windows.Forms.Panel();
			this.lblMainSectionTitle = new System.Windows.Forms.Label();
			this.panelTableBody = new System.Windows.Forms.Panel();
			this.lblEmpty = new System.Windows.Forms.Label();
			this.dgvConfig = new System.Windows.Forms.DataGridView();
			this.panelAction = new System.Windows.Forms.Panel();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnMoveUp = new System.Windows.Forms.Button();
			this.btnMoveDown = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.panelSide = new System.Windows.Forms.Panel();
			this.btnTriggerManager = new System.Windows.Forms.Button();
			this.btnTaskScheduler = new System.Windows.Forms.Button();
			this.panelSidePattern = new System.Windows.Forms.Panel();
			this.panelRoot.SuspendLayout();
			this.panelMain.SuspendLayout();
			this.panelContent.SuspendLayout();
			this.panelJobList.SuspendLayout();
			this.panelTaskList.SuspendLayout();
			this.panelConfig.SuspendLayout();
			this.panelTableBody.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvConfig)).BeginInit();
			this.panelAction.SuspendLayout();
			this.panelSide.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelRoot
			// 
			this.panelRoot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(12)))), ((int)(((byte)(24)))));
			this.panelRoot.Controls.Add(this.panelMain);
			this.panelRoot.Controls.Add(this.panelSide);
			this.panelRoot.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelRoot.Location = new System.Drawing.Point(0, 0);
			this.panelRoot.Name = "panelRoot";
			this.panelRoot.Size = new System.Drawing.Size(1600, 900);
			this.panelRoot.TabIndex = 0;
			// 
			// panelMain
			// 
			this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.panelMain.Controls.Add(this.panelContent);
			this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMain.Location = new System.Drawing.Point(285, 0);
			this.panelMain.Name = "panelMain";
			this.panelMain.Padding = new System.Windows.Forms.Padding(28, 24, 28, 28);
			this.panelMain.Size = new System.Drawing.Size(1315, 900);
			this.panelMain.TabIndex = 0;
			// 
			// panelContent
			// 
			this.panelContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelContent.Controls.Add(this.panelJobList);
			this.panelContent.Controls.Add(this.panelTaskList);
			this.panelContent.Controls.Add(this.panelConfig);
			this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContent.Location = new System.Drawing.Point(28, 24);
			this.panelContent.Name = "panelContent";
			this.panelContent.Size = new System.Drawing.Size(1259, 848);
			this.panelContent.TabIndex = 2;
			// 
			// panelJobList
			// 
			this.panelJobList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelJobList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelJobList.Controls.Add(this.lblJobTitle);
			this.panelJobList.Controls.Add(this.txtSearchJob);
			this.panelJobList.Controls.Add(this.listJobs);
			this.panelJobList.Location = new System.Drawing.Point(22, 22);
			this.panelJobList.Name = "panelJobList";
			this.panelJobList.Size = new System.Drawing.Size(250, 666);
			this.panelJobList.TabIndex = 0;
			// 
			// lblJobTitle
			// 
			this.lblJobTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblJobTitle.ForeColor = System.Drawing.Color.White;
			this.lblJobTitle.Location = new System.Drawing.Point(18, 18);
			this.lblJobTitle.Name = "lblJobTitle";
			this.lblJobTitle.Size = new System.Drawing.Size(200, 28);
			this.lblJobTitle.TabIndex = 0;
			this.lblJobTitle.Text = "所有 JobID";
			// 
			// txtSearchJob
			// 
			this.txtSearchJob.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.txtSearchJob.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSearchJob.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
			this.txtSearchJob.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(150)))), ((int)(((byte)(170)))));
			this.txtSearchJob.Location = new System.Drawing.Point(18, 58);
			this.txtSearchJob.Name = "txtSearchJob";
			this.txtSearchJob.Size = new System.Drawing.Size(210, 32);
			this.txtSearchJob.TabIndex = 1;
			this.txtSearchJob.Text = "搜索 JobID";
			// 
			// listJobs
			// 
			this.listJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(16)))), ((int)(((byte)(30)))));
			this.listJobs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listJobs.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
			this.listJobs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.listJobs.FormattingEnabled = true;
			this.listJobs.ItemHeight = 27;
			this.listJobs.Location = new System.Drawing.Point(18, 108);
			this.listJobs.Name = "listJobs";
			this.listJobs.Size = new System.Drawing.Size(210, 461);
			this.listJobs.TabIndex = 2;
			// 
			// panelTaskList
			// 
			this.panelTaskList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelTaskList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTaskList.Controls.Add(this.lblTaskTitle);
			this.panelTaskList.Controls.Add(this.txtSearchTask);
			this.panelTaskList.Controls.Add(this.listTasks);
			this.panelTaskList.Location = new System.Drawing.Point(286, 22);
			this.panelTaskList.Name = "panelTaskList";
			this.panelTaskList.Size = new System.Drawing.Size(250, 666);
			this.panelTaskList.TabIndex = 1;
			// 
			// lblTaskTitle
			// 
			this.lblTaskTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblTaskTitle.ForeColor = System.Drawing.Color.White;
			this.lblTaskTitle.Location = new System.Drawing.Point(18, 18);
			this.lblTaskTitle.Name = "lblTaskTitle";
			this.lblTaskTitle.Size = new System.Drawing.Size(200, 28);
			this.lblTaskTitle.TabIndex = 0;
			this.lblTaskTitle.Text = "所有 task";
			// 
			// txtSearchTask
			// 
			this.txtSearchTask.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.txtSearchTask.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSearchTask.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
			this.txtSearchTask.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(150)))), ((int)(((byte)(170)))));
			this.txtSearchTask.Location = new System.Drawing.Point(18, 58);
			this.txtSearchTask.Name = "txtSearchTask";
			this.txtSearchTask.Size = new System.Drawing.Size(210, 32);
			this.txtSearchTask.TabIndex = 1;
			this.txtSearchTask.Text = "搜索 task";
			// 
			// listTasks
			// 
			this.listTasks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(16)))), ((int)(((byte)(30)))));
			this.listTasks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listTasks.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
			this.listTasks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.listTasks.FormattingEnabled = true;
			this.listTasks.ItemHeight = 27;
			this.listTasks.Location = new System.Drawing.Point(18, 108);
			this.listTasks.Name = "listTasks";
			this.listTasks.Size = new System.Drawing.Size(210, 461);
			this.listTasks.TabIndex = 2;
			// 
			// panelConfig
			// 
			this.panelConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelConfig.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelConfig.Controls.Add(this.lblMainSectionTitle);
			this.panelConfig.Controls.Add(this.panelTableBody);
			this.panelConfig.Controls.Add(this.panelAction);
			this.panelConfig.Location = new System.Drawing.Point(550, 22);
			this.panelConfig.Name = "panelConfig";
			this.panelConfig.Size = new System.Drawing.Size(680, 666);
			this.panelConfig.TabIndex = 2;
			// 
			// lblMainSectionTitle
			// 
			this.lblMainSectionTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblMainSectionTitle.ForeColor = System.Drawing.Color.White;
			this.lblMainSectionTitle.Location = new System.Drawing.Point(18, 18);
			this.lblMainSectionTitle.Name = "lblMainSectionTitle";
			this.lblMainSectionTitle.Size = new System.Drawing.Size(350, 28);
			this.lblMainSectionTitle.TabIndex = 0;
			this.lblMainSectionTitle.Text = "当前 task 中的 step";
			// 
			// panelTableBody
			// 
			this.panelTableBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(18)))), ((int)(((byte)(34)))));
			this.panelTableBody.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTableBody.Controls.Add(this.lblEmpty);
			this.panelTableBody.Controls.Add(this.dgvConfig);
			this.panelTableBody.Location = new System.Drawing.Point(18, 58);
			this.panelTableBody.Name = "panelTableBody";
			this.panelTableBody.Size = new System.Drawing.Size(640, 510);
			this.panelTableBody.TabIndex = 1;
			// 
			// lblEmpty
			// 
			this.lblEmpty.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
			this.lblEmpty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(120)))), ((int)(((byte)(145)))));
			this.lblEmpty.Location = new System.Drawing.Point(235, 215);
			this.lblEmpty.Name = "lblEmpty";
			this.lblEmpty.Size = new System.Drawing.Size(170, 80);
			this.lblEmpty.TabIndex = 0;
			this.lblEmpty.Text = "▱\r\n暂无数据";
			this.lblEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// dgvConfig
			// 
			this.dgvConfig.AllowUserToAddRows = false;
			this.dgvConfig.AllowUserToDeleteRows = false;
			this.dgvConfig.AllowUserToResizeRows = false;
			this.dgvConfig.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(18)))), ((int)(((byte)(34)))));
			this.dgvConfig.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(34)))), ((int)(((byte)(60)))));
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(34)))), ((int)(((byte)(60)))));
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
			this.dgvConfig.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.dgvConfig.ColumnHeadersHeight = 44;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(18)))), ((int)(((byte)(34)))));
			dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(61)))), ((int)(((byte)(135)))));
			dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvConfig.DefaultCellStyle = dataGridViewCellStyle4;
			this.dgvConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvConfig.EnableHeadersVisualStyles = false;
			this.dgvConfig.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(58)))), ((int)(((byte)(82)))));
			this.dgvConfig.Location = new System.Drawing.Point(0, 0);
			this.dgvConfig.Name = "dgvConfig";
			this.dgvConfig.RowHeadersVisible = false;
			this.dgvConfig.RowHeadersWidth = 62;
			this.dgvConfig.RowTemplate.Height = 32;
			this.dgvConfig.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvConfig.Size = new System.Drawing.Size(638, 508);
			this.dgvConfig.TabIndex = 1;
			// 
			// panelAction
			// 
			this.panelAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelAction.Controls.Add(this.btnAdd);
			this.panelAction.Controls.Add(this.btnDelete);
			this.panelAction.Controls.Add(this.btnMoveUp);
			this.panelAction.Controls.Add(this.btnSave);
			this.panelAction.Controls.Add(this.btnMoveDown);
			this.panelAction.Location = new System.Drawing.Point(18, 584);
			this.panelAction.Name = "panelAction";
			this.panelAction.Size = new System.Drawing.Size(640, 60);
			this.panelAction.TabIndex = 2;
			// 
			// btnAdd
			// 
			this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.btnAdd.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnAdd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnAdd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAdd.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnAdd.ForeColor = System.Drawing.Color.White;
			this.btnAdd.Location = new System.Drawing.Point(5, 6);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(135, 40);
			this.btnAdd.TabIndex = 0;
			this.btnAdd.Text = "＋  新增算子";
			this.btnAdd.UseVisualStyleBackColor = false;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.btnDelete.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnDelete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnDelete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDelete.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnDelete.ForeColor = System.Drawing.Color.White;
			this.btnDelete.Location = new System.Drawing.Point(146, 6);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(135, 40);
			this.btnDelete.TabIndex = 1;
			this.btnDelete.Text = "▥  删除选中";
			this.btnDelete.UseVisualStyleBackColor = false;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// btnMoveUp
			// 
			this.btnMoveUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.btnMoveUp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnMoveUp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnMoveUp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnMoveUp.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnMoveUp.ForeColor = System.Drawing.Color.White;
			this.btnMoveUp.Location = new System.Drawing.Point(287, 6);
			this.btnMoveUp.Name = "btnMoveUp";
			this.btnMoveUp.Size = new System.Drawing.Size(135, 40);
			this.btnMoveUp.TabIndex = 2;
			this.btnMoveUp.Text = "▲  上移选中";
			this.btnMoveUp.UseVisualStyleBackColor = false;
			this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
			// 
			// btnMoveDown
			// 
			this.btnMoveDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.btnMoveDown.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnMoveDown.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnMoveDown.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnMoveDown.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnMoveDown.ForeColor = System.Drawing.Color.White;
			this.btnMoveDown.Location = new System.Drawing.Point(428, 6);
			this.btnMoveDown.Name = "btnMoveDown";
			this.btnMoveDown.Size = new System.Drawing.Size(135, 40);
			this.btnMoveDown.TabIndex = 3;
			this.btnMoveDown.Text = "▼  下移选中";
			this.btnMoveDown.UseVisualStyleBackColor = false;
			this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
			// 
			// btnSave
			// 
			this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(75)))), ((int)(((byte)(190)))));
			this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(135)))), ((int)(((byte)(255)))));
			this.btnSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(60)))), ((int)(((byte)(160)))));
			this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(95)))), ((int)(((byte)(220)))));
			this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnSave.ForeColor = System.Drawing.Color.White;
			this.btnSave.Location = new System.Drawing.Point(569, 6);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(115, 40);
			this.btnSave.TabIndex = 4;
			this.btnSave.Text = "▣  保存";
			this.btnSave.UseVisualStyleBackColor = false;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// panelSide
			// 
			this.panelSide.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(18)))), ((int)(((byte)(34)))));
			this.panelSide.Controls.Add(this.btnTriggerManager);
			this.panelSide.Controls.Add(this.btnTaskScheduler);
			this.panelSide.Controls.Add(this.panelSidePattern);
			this.panelSide.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelSide.Location = new System.Drawing.Point(0, 0);
			this.panelSide.Name = "panelSide";
			this.panelSide.Padding = new System.Windows.Forms.Padding(20, 36, 20, 20);
			this.panelSide.Size = new System.Drawing.Size(285, 900);
			this.panelSide.TabIndex = 1;
			// 
			// btnTriggerManager
			// 
			this.btnTriggerManager.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(22)))), ((int)(((byte)(39)))));
			this.btnTriggerManager.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(63)))), ((int)(((byte)(88)))));
			this.btnTriggerManager.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(61)))), ((int)(((byte)(135)))));
			this.btnTriggerManager.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnTriggerManager.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTriggerManager.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
			this.btnTriggerManager.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.btnTriggerManager.Location = new System.Drawing.Point(22, 36);
			this.btnTriggerManager.Name = "btnTriggerManager";
			this.btnTriggerManager.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
			this.btnTriggerManager.Size = new System.Drawing.Size(240, 64);
			this.btnTriggerManager.TabIndex = 0;
			this.btnTriggerManager.Text = "⚡   触发管理";
			this.btnTriggerManager.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnTriggerManager.UseVisualStyleBackColor = false;
			this.btnTriggerManager.Click += new System.EventHandler(this.btnTriggerManager_Click);
			// 
			// btnTaskScheduler
			// 
			this.btnTaskScheduler.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(22)))), ((int)(((byte)(39)))));
			this.btnTaskScheduler.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(63)))), ((int)(((byte)(88)))));
			this.btnTaskScheduler.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(61)))), ((int)(((byte)(135)))));
			this.btnTaskScheduler.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnTaskScheduler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTaskScheduler.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
			this.btnTaskScheduler.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.btnTaskScheduler.Location = new System.Drawing.Point(22, 122);
			this.btnTaskScheduler.Name = "btnTaskScheduler";
			this.btnTaskScheduler.Padding = new System.Windows.Forms.Padding(30, 0, 0, 0);
			this.btnTaskScheduler.Size = new System.Drawing.Size(240, 64);
			this.btnTaskScheduler.TabIndex = 1;
			this.btnTaskScheduler.Text = "▣   任务调度";
			this.btnTaskScheduler.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnTaskScheduler.UseVisualStyleBackColor = false;
			this.btnTaskScheduler.Click += new System.EventHandler(this.btnTaskScheduler_Click);
			// 
			// panelSidePattern
			// 
			this.panelSidePattern.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(18)))), ((int)(((byte)(34)))));
			this.panelSidePattern.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelSidePattern.Location = new System.Drawing.Point(20, 720);
			this.panelSidePattern.Name = "panelSidePattern";
			this.panelSidePattern.Size = new System.Drawing.Size(245, 160);
			this.panelSidePattern.TabIndex = 2;
			// 
			// FlowConfigForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(12)))), ((int)(((byte)(24)))));
			this.ClientSize = new System.Drawing.Size(1600, 900);
			this.Controls.Add(this.panelRoot);
			this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MinimumSize = new System.Drawing.Size(1280, 720);
			this.Name = "FlowConfigForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Flow Configuration";
			this.Load += new System.EventHandler(this.FlowConfigForm_Load);
			this.panelRoot.ResumeLayout(false);
			this.panelMain.ResumeLayout(false);
			this.panelContent.ResumeLayout(false);
			this.panelJobList.ResumeLayout(false);
			this.panelJobList.PerformLayout();
			this.panelTaskList.ResumeLayout(false);
			this.panelTaskList.PerformLayout();
			this.panelConfig.ResumeLayout(false);
			this.panelTableBody.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvConfig)).EndInit();
			this.panelAction.ResumeLayout(false);
			this.panelSide.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}
}
