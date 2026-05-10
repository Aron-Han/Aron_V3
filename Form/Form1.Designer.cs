namespace Aron_V3
{
	partial class Form1
	{
		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.TableLayoutPanel rootLayout;
		private System.Windows.Forms.Panel titlePanel;
		private System.Windows.Forms.Label lblLogo;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Button btnExit;

		private System.Windows.Forms.Panel toolbarPanel;
		private System.Windows.Forms.Button btnLogin;
		private System.Windows.Forms.Button btnAlgorithmConfig;
		private System.Windows.Forms.Button btnDatabase;
		private System.Windows.Forms.Button btnSystemSetting;
		private System.Windows.Forms.Button btnStop;

		private System.Windows.Forms.TableLayoutPanel mainLayout;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCameras;

		private System.Windows.Forms.Panel resultPanel;
		private System.Windows.Forms.Label lblResultTitle;
		private System.Windows.Forms.DataGridView dgvResults;
		private System.Windows.Forms.Label lblPageInfo;

		private System.Windows.Forms.Panel logPanel;
		private System.Windows.Forms.Label lblLogTitle;
		private System.Windows.Forms.ComboBox cmbLogLevel;
		private System.Windows.Forms.Button btnClearLog;
		private System.Windows.Forms.ListBox lstLog;

		private System.Windows.Forms.Panel statusPanel;
		private System.Windows.Forms.Label lblCameraStatus;
		private System.Windows.Forms.Label lblPlcStatus;
		private System.Windows.Forms.Label lblVersion;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();

			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
			this.titlePanel = new System.Windows.Forms.Panel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.btnExit = new System.Windows.Forms.Button();
			this.lblLogo = new System.Windows.Forms.Label();
			this.toolbarPanel = new System.Windows.Forms.Panel();
			this.btnLogin = new System.Windows.Forms.Button();
			this.btnAlgorithmConfig = new System.Windows.Forms.Button();
			this.btnDatabase = new System.Windows.Forms.Button();
			this.btnSystemSetting = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelCameras = new System.Windows.Forms.TableLayoutPanel();
			this.resultPanel = new System.Windows.Forms.Panel();
			this.dgvResults = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lblPageInfo = new System.Windows.Forms.Label();
			this.lblResultTitle = new System.Windows.Forms.Label();
			this.logPanel = new System.Windows.Forms.Panel();
			this.lstLog = new System.Windows.Forms.ListBox();
			this.cmbLogLevel = new System.Windows.Forms.ComboBox();
			this.btnClearLog = new System.Windows.Forms.Button();
			this.lblLogTitle = new System.Windows.Forms.Label();
			this.statusPanel = new System.Windows.Forms.Panel();
			this.lblVersion = new System.Windows.Forms.Label();
			this.lblPlcStatus = new System.Windows.Forms.Label();
			this.lblCameraStatus = new System.Windows.Forms.Label();
			this.rootLayout.SuspendLayout();
			this.titlePanel.SuspendLayout();
			this.toolbarPanel.SuspendLayout();
			this.mainLayout.SuspendLayout();
			this.resultPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
			this.logPanel.SuspendLayout();
			this.statusPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// rootLayout
			// 
			this.rootLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(14)))), ((int)(((byte)(25)))));
			this.rootLayout.ColumnCount = 1;
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Controls.Add(this.titlePanel, 0, 0);
			this.rootLayout.Controls.Add(this.toolbarPanel, 0, 1);
			this.rootLayout.Controls.Add(this.mainLayout, 0, 2);
			this.rootLayout.Controls.Add(this.statusPanel, 0, 3);
			this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rootLayout.Location = new System.Drawing.Point(0, 0);
			this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
			this.rootLayout.Name = "rootLayout";
			this.rootLayout.RowCount = 4;
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.rootLayout.Size = new System.Drawing.Size(1500, 850);
			this.rootLayout.TabIndex = 0;
			// 
			// titlePanel
			// 
			this.titlePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(18)))));
			this.titlePanel.Controls.Add(this.lblTitle);
			this.titlePanel.Controls.Add(this.btnExit);
			this.titlePanel.Controls.Add(this.lblLogo);
			this.titlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.titlePanel.Location = new System.Drawing.Point(0, 0);
			this.titlePanel.Margin = new System.Windows.Forms.Padding(0);
			this.titlePanel.Name = "titlePanel";
			this.titlePanel.Size = new System.Drawing.Size(1500, 44);
			this.titlePanel.TabIndex = 0;
			// 
			// lblTitle
			// 
			this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 13.5F, System.Drawing.FontStyle.Bold);
			this.lblTitle.ForeColor = System.Drawing.Color.White;
			this.lblTitle.Location = new System.Drawing.Point(54, 0);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
			this.lblTitle.Size = new System.Drawing.Size(1358, 44);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Betterway Vision-Base  |  工业视觉检测平台";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnExit
			// 
			this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
			this.btnExit.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(55)))), ((int)(((byte)(65)))));
			this.btnExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(40)))), ((int)(((byte)(50)))));
			this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(35)))), ((int)(((byte)(45)))));
			this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnExit.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnExit.ForeColor = System.Drawing.Color.White;
			this.btnExit.Location = new System.Drawing.Point(1412, 0);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(88, 44);
			this.btnExit.TabIndex = 1;
			this.btnExit.Text = "Exit";
			this.btnExit.UseVisualStyleBackColor = false;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// lblLogo
			// 
			this.lblLogo.Dock = System.Windows.Forms.DockStyle.Left;
			this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
			this.lblLogo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(135)))), ((int)(((byte)(255)))));
			this.lblLogo.Location = new System.Drawing.Point(0, 0);
			this.lblLogo.Name = "lblLogo";
			this.lblLogo.Size = new System.Drawing.Size(54, 44);
			this.lblLogo.TabIndex = 2;
			this.lblLogo.Text = "V";
			this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// toolbarPanel
			// 
			this.toolbarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(20)))), ((int)(((byte)(36)))));
			this.toolbarPanel.Controls.Add(this.btnLogin);
			this.toolbarPanel.Controls.Add(this.btnAlgorithmConfig);
			this.toolbarPanel.Controls.Add(this.btnDatabase);
			this.toolbarPanel.Controls.Add(this.btnSystemSetting);
			this.toolbarPanel.Controls.Add(this.btnStop);
			this.toolbarPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolbarPanel.Location = new System.Drawing.Point(0, 44);
			this.toolbarPanel.Margin = new System.Windows.Forms.Padding(0);
			this.toolbarPanel.Name = "toolbarPanel";
			this.toolbarPanel.Padding = new System.Windows.Forms.Padding(18, 8, 18, 8);
			this.toolbarPanel.Size = new System.Drawing.Size(1500, 72);
			this.toolbarPanel.TabIndex = 1;
			// 
			// btnLogin
			// 
			this.btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(20)))), ((int)(((byte)(36)))));
			this.btnLogin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(58)))), ((int)(((byte)(82)))));
			this.btnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLogin.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnLogin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(245)))));
			this.btnLogin.Location = new System.Drawing.Point(18, 10);
			this.btnLogin.Name = "btnLogin";
			this.btnLogin.Size = new System.Drawing.Size(130, 50);
			this.btnLogin.TabIndex = 0;
			this.btnLogin.Text = "◎  登录";
			this.btnLogin.UseVisualStyleBackColor = false;
			this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
			// 
			// btnAlgorithmConfig
			// 
			this.btnAlgorithmConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(20)))), ((int)(((byte)(36)))));
			this.btnAlgorithmConfig.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(58)))), ((int)(((byte)(82)))));
			this.btnAlgorithmConfig.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnAlgorithmConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnAlgorithmConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAlgorithmConfig.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnAlgorithmConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(245)))));
			this.btnAlgorithmConfig.Location = new System.Drawing.Point(156, 10);
			this.btnAlgorithmConfig.Name = "btnAlgorithmConfig";
			this.btnAlgorithmConfig.Size = new System.Drawing.Size(150, 50);
			this.btnAlgorithmConfig.TabIndex = 1;
			this.btnAlgorithmConfig.Text = "◇  算法配置";
			this.btnAlgorithmConfig.UseVisualStyleBackColor = false;
			this.btnAlgorithmConfig.Click += new System.EventHandler(this.btnAlgorithmConfig_Click);
			// 
			// btnDatabase
			// 
			this.btnDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(20)))), ((int)(((byte)(36)))));
			this.btnDatabase.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(58)))), ((int)(((byte)(82)))));
			this.btnDatabase.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnDatabase.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDatabase.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnDatabase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(245)))));
			this.btnDatabase.Location = new System.Drawing.Point(314, 10);
			this.btnDatabase.Name = "btnDatabase";
			this.btnDatabase.Size = new System.Drawing.Size(130, 50);
			this.btnDatabase.TabIndex = 2;
			this.btnDatabase.Text = "▤  数据库";
			this.btnDatabase.UseVisualStyleBackColor = false;
			this.btnDatabase.Click += new System.EventHandler(this.btnDatabase_Click);
			// 
			// btnSystemSetting
			// 
			this.btnSystemSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(20)))), ((int)(((byte)(36)))));
			this.btnSystemSetting.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(58)))), ((int)(((byte)(82)))));
			this.btnSystemSetting.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnSystemSetting.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnSystemSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSystemSetting.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnSystemSetting.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(235)))), ((int)(((byte)(245)))));
			this.btnSystemSetting.Location = new System.Drawing.Point(452, 10);
			this.btnSystemSetting.Name = "btnSystemSetting";
			this.btnSystemSetting.Size = new System.Drawing.Size(145, 50);
			this.btnSystemSetting.TabIndex = 3;
			this.btnSystemSetting.Text = "⚙  系统设置";
			this.btnSystemSetting.UseVisualStyleBackColor = false;
			this.btnSystemSetting.Click += new System.EventHandler(this.btnSystemSetting_Click);
			// 
			// btnStop
			// 
			this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(20)))), ((int)(((byte)(36)))));
			this.btnStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(58)))), ((int)(((byte)(82)))));
			this.btnStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnStop.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(54)))), ((int)(((byte)(65)))));
			this.btnStop.Location = new System.Drawing.Point(605, 10);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(120, 50);
			this.btnStop.TabIndex = 4;
			this.btnStop.Text = "□  停止";
			this.btnStop.UseVisualStyleBackColor = false;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// mainLayout
			// 
			this.mainLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(14)))), ((int)(((byte)(25)))));
			this.mainLayout.ColumnCount = 2;
			this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76F));
			this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
			this.mainLayout.Controls.Add(this.tableLayoutPanelCameras, 0, 0);
			this.mainLayout.Controls.Add(this.resultPanel, 1, 0);
			this.mainLayout.Controls.Add(this.logPanel, 0, 1);
			this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainLayout.Location = new System.Drawing.Point(0, 116);
			this.mainLayout.Margin = new System.Windows.Forms.Padding(0);
			this.mainLayout.Name = "mainLayout";
			this.mainLayout.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
			this.mainLayout.RowCount = 2;
			this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 78F));
			this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22F));
			this.mainLayout.Size = new System.Drawing.Size(1500, 694);
			this.mainLayout.TabIndex = 2;
			// 
			// tableLayoutPanelCameras
			// 
			this.tableLayoutPanelCameras.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(14)))), ((int)(((byte)(25)))));
			this.tableLayoutPanelCameras.ColumnCount = 1;
			this.tableLayoutPanelCameras.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCameras.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelCameras.Location = new System.Drawing.Point(8, 8);
			this.tableLayoutPanelCameras.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelCameras.Name = "tableLayoutPanelCameras";
			this.tableLayoutPanelCameras.RowCount = 1;
			this.tableLayoutPanelCameras.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCameras.Size = new System.Drawing.Size(1127, 535);
			this.tableLayoutPanelCameras.TabIndex = 0;
			// 
			// resultPanel
			// 
			this.resultPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.resultPanel.Controls.Add(this.dgvResults);
			this.resultPanel.Controls.Add(this.lblPageInfo);
			this.resultPanel.Controls.Add(this.lblResultTitle);
			this.resultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultPanel.Location = new System.Drawing.Point(1143, 8);
			this.resultPanel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 8);
			this.resultPanel.Name = "resultPanel";
			this.resultPanel.Padding = new System.Windows.Forms.Padding(10);
			this.resultPanel.Size = new System.Drawing.Size(349, 527);
			this.resultPanel.TabIndex = 1;
			// 
			// dgvResults
			// 
			this.dgvResults.AllowUserToAddRows = false;
			this.dgvResults.AllowUserToDeleteRows = false;
			this.dgvResults.AllowUserToResizeRows = false;
			this.dgvResults.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.dgvResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(29)))), ((int)(((byte)(50)))));
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(205)))), ((int)(((byte)(220)))));
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvResults.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dgvResults.ColumnHeadersHeight = 32;
			this.dgvResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.WhiteSmoke;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(78)))), ((int)(((byte)(145)))));
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvResults.DefaultCellStyle = dataGridViewCellStyle2;
			this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvResults.EnableHeadersVisualStyles = false;
			this.dgvResults.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.dgvResults.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(48)))), ((int)(((byte)(70)))));
			this.dgvResults.Location = new System.Drawing.Point(10, 46);
			this.dgvResults.MultiSelect = false;
			this.dgvResults.Name = "dgvResults";
			this.dgvResults.ReadOnly = true;
			this.dgvResults.RowHeadersVisible = false;
			this.dgvResults.RowHeadersWidth = 62;
			this.dgvResults.RowTemplate.Height = 30;
			this.dgvResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvResults.Size = new System.Drawing.Size(329, 433);
			this.dgvResults.TabIndex = 0;
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.HeaderText = "工位/产品ID";
			this.dataGridViewTextBoxColumn1.MinimumWidth = 8;
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			this.dataGridViewTextBoxColumn1.Width = 150;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.HeaderText = "结果";
			this.dataGridViewTextBoxColumn2.MinimumWidth = 8;
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			this.dataGridViewTextBoxColumn2.Width = 150;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.HeaderText = "状态";
			this.dataGridViewTextBoxColumn3.MinimumWidth = 8;
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			this.dataGridViewTextBoxColumn3.Width = 150;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.HeaderText = "时间";
			this.dataGridViewTextBoxColumn4.MinimumWidth = 8;
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			this.dataGridViewTextBoxColumn4.Width = 150;
			// 
			// lblPageInfo
			// 
			this.lblPageInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lblPageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(185)))), ((int)(((byte)(200)))));
			this.lblPageInfo.Location = new System.Drawing.Point(10, 479);
			this.lblPageInfo.Name = "lblPageInfo";
			this.lblPageInfo.Size = new System.Drawing.Size(329, 38);
			this.lblPageInfo.TabIndex = 1;
			this.lblPageInfo.Text = "共 1286 条                         <   1   2   3   4   5   ...   86   >";
			this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblResultTitle
			// 
			this.lblResultTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblResultTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblResultTitle.ForeColor = System.Drawing.Color.WhiteSmoke;
			this.lblResultTitle.Location = new System.Drawing.Point(10, 10);
			this.lblResultTitle.Name = "lblResultTitle";
			this.lblResultTitle.Size = new System.Drawing.Size(329, 36);
			this.lblResultTitle.TabIndex = 2;
			this.lblResultTitle.Text = "检测结果                                      ⌕    ⚙";
			this.lblResultTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// logPanel
			// 
			this.logPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.mainLayout.SetColumnSpan(this.logPanel, 2);
			this.logPanel.Controls.Add(this.lstLog);
			this.logPanel.Controls.Add(this.cmbLogLevel);
			this.logPanel.Controls.Add(this.btnClearLog);
			this.logPanel.Controls.Add(this.lblLogTitle);
			this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logPanel.Location = new System.Drawing.Point(8, 543);
			this.logPanel.Margin = new System.Windows.Forms.Padding(0);
			this.logPanel.Name = "logPanel";
			this.logPanel.Padding = new System.Windows.Forms.Padding(12, 6, 12, 8);
			this.logPanel.Size = new System.Drawing.Size(1484, 151);
			this.logPanel.TabIndex = 2;
			// 
			// lstLog
			// 
			this.lstLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(15)))), ((int)(((byte)(28)))));
			this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstLog.Font = new System.Drawing.Font("Consolas", 9F);
			this.lstLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(215)))), ((int)(((byte)(225)))));
			this.lstLog.FormattingEnabled = true;
			this.lstLog.ItemHeight = 22;
			this.lstLog.Location = new System.Drawing.Point(12, 36);
			this.lstLog.Name = "lstLog";
			this.lstLog.Size = new System.Drawing.Size(1460, 107);
			this.lstLog.TabIndex = 0;
			// 
			// cmbLogLevel
			// 
			this.cmbLogLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbLogLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(19)))), ((int)(((byte)(34)))));
			this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbLogLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmbLogLevel.ForeColor = System.Drawing.Color.WhiteSmoke;
			this.cmbLogLevel.Items.AddRange(new object[] {
            "全部级别",
            "INFO",
            "OK",
            "NG",
            "ERROR"});
			this.cmbLogLevel.Location = new System.Drawing.Point(2314, 8);
			this.cmbLogLevel.Name = "cmbLogLevel";
			this.cmbLogLevel.Size = new System.Drawing.Size(120, 32);
			this.cmbLogLevel.TabIndex = 1;
			// 
			// btnClearLog
			// 
			this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClearLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(19)))), ((int)(((byte)(34)))));
			this.btnClearLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClearLog.ForeColor = System.Drawing.Color.WhiteSmoke;
			this.btnClearLog.Location = new System.Drawing.Point(2444, 7);
			this.btnClearLog.Name = "btnClearLog";
			this.btnClearLog.Size = new System.Drawing.Size(70, 26);
			this.btnClearLog.TabIndex = 2;
			this.btnClearLog.Text = "清空";
			this.btnClearLog.UseVisualStyleBackColor = false;
			// 
			// lblLogTitle
			// 
			this.lblLogTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblLogTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblLogTitle.ForeColor = System.Drawing.Color.WhiteSmoke;
			this.lblLogTitle.Location = new System.Drawing.Point(12, 6);
			this.lblLogTitle.Name = "lblLogTitle";
			this.lblLogTitle.Size = new System.Drawing.Size(1460, 30);
			this.lblLogTitle.TabIndex = 3;
			this.lblLogTitle.Text = "Log日志";
			this.lblLogTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// statusPanel
			// 
			this.statusPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(15)))), ((int)(((byte)(27)))));
			this.statusPanel.Controls.Add(this.lblVersion);
			this.statusPanel.Controls.Add(this.lblPlcStatus);
			this.statusPanel.Controls.Add(this.lblCameraStatus);
			this.statusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusPanel.Location = new System.Drawing.Point(0, 810);
			this.statusPanel.Margin = new System.Windows.Forms.Padding(0);
			this.statusPanel.Name = "statusPanel";
			this.statusPanel.Size = new System.Drawing.Size(1500, 40);
			this.statusPanel.TabIndex = 3;
			// 
			// lblVersion
			// 
			this.lblVersion.Dock = System.Windows.Forms.DockStyle.Right;
			this.lblVersion.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
			this.lblVersion.Location = new System.Drawing.Point(1320, 0);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(180, 40);
			this.lblVersion.TabIndex = 0;
			this.lblVersion.Text = "版本号:  1.0.0.0";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblPlcStatus
			// 
			this.lblPlcStatus.Dock = System.Windows.Forms.DockStyle.Left;
			this.lblPlcStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.lblPlcStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(210)))), ((int)(((byte)(70)))));
			this.lblPlcStatus.Location = new System.Drawing.Point(170, 0);
			this.lblPlcStatus.Name = "lblPlcStatus";
			this.lblPlcStatus.Size = new System.Drawing.Size(160, 40);
			this.lblPlcStatus.TabIndex = 1;
			this.lblPlcStatus.Text = "▦  PLC:  已连接";
			this.lblPlcStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblCameraStatus
			// 
			this.lblCameraStatus.Dock = System.Windows.Forms.DockStyle.Left;
			this.lblCameraStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.lblCameraStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(210)))), ((int)(((byte)(70)))));
			this.lblCameraStatus.Location = new System.Drawing.Point(0, 0);
			this.lblCameraStatus.Name = "lblCameraStatus";
			this.lblCameraStatus.Padding = new System.Windows.Forms.Padding(18, 0, 0, 0);
			this.lblCameraStatus.Size = new System.Drawing.Size(170, 40);
			this.lblCameraStatus.TabIndex = 2;
			this.lblCameraStatus.Text = "▣  相机:  已连接";
			this.lblCameraStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Form1
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(14)))), ((int)(((byte)(25)))));
			this.ClientSize = new System.Drawing.Size(1500, 850);
			this.Controls.Add(this.rootLayout);
			this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MinimumSize = new System.Drawing.Size(1280, 720);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Betterway Vision-Base  |  工业视觉检测平台";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.Form1_Load);
			this.rootLayout.ResumeLayout(false);
			this.titlePanel.ResumeLayout(false);
			this.toolbarPanel.ResumeLayout(false);
			this.mainLayout.ResumeLayout(false);
			this.resultPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
			this.logPanel.ResumeLayout(false);
			this.statusPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
	}
}
