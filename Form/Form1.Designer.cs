namespace Aron_V3
{
	partial class Form1
	{
		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.TableLayoutPanel rootLayout;

		private System.Windows.Forms.Panel titlePanel;
		private System.Windows.Forms.Panel panelBrand;
		private System.Windows.Forms.Label lblLogo;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.FlowLayoutPanel navFlowPanel;
		private System.Windows.Forms.Panel panelNavLogin;
		private System.Windows.Forms.Panel panelNavAlgorithm;
		private System.Windows.Forms.Panel panelNavProcess;
		private System.Windows.Forms.Panel panelNavCommunication;
		private System.Windows.Forms.Panel panelNavDatabase;
		private System.Windows.Forms.Panel panelNavSystem;
		private System.Windows.Forms.Panel panelNavStop;

		private System.Windows.Forms.Button btnLogin;
		private System.Windows.Forms.Button btnAlgorithmConfig;
		private System.Windows.Forms.Button btnProcessConfig;
		private System.Windows.Forms.Button btnCommunicateConfig;
		private System.Windows.Forms.Button btnDatabase;
		private System.Windows.Forms.Button btnSystemSetting;
		private System.Windows.Forms.Button btnStop;

		private System.Windows.Forms.Label lblRunStatus;
		private System.Windows.Forms.Label lblUser;
		private System.Windows.Forms.Label lblRightDivider;
		private System.Windows.Forms.Button btnMinimize;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Panel panelRightStatus;

		private System.Windows.Forms.Panel underlineLogin;
		private System.Windows.Forms.Panel underlineAlgorithmConfig;
		private System.Windows.Forms.Panel underlineProcessConfig;
		private System.Windows.Forms.Panel underlineCommunicateConfig;
		private System.Windows.Forms.Panel underlineDatabase;
		private System.Windows.Forms.Panel underlineSystemSetting;
		private System.Windows.Forms.Panel underlineStop;

		private System.Windows.Forms.Panel pageHost;

		private System.Windows.Forms.TableLayoutPanel mainLayout;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCameras;

		private System.Windows.Forms.Panel resultPanel;
		private System.Windows.Forms.DataGridView dgvResults;

		private System.Windows.Forms.Panel logPanel;
		private System.Windows.Forms.Label lblLogTitle;
		private System.Windows.Forms.ComboBox cmbLogLevel;
		private System.Windows.Forms.Button btnClearLog;
		private System.Windows.Forms.ListBox lstLog;

		private System.Windows.Forms.Panel statusPanel;
		private System.Windows.Forms.Label lblCameraStatus;
		private System.Windows.Forms.Label lblPlcStatus;
		private System.Windows.Forms.Label lblVersion;
		private System.Windows.Forms.Button btnLanguage;

		private System.Windows.Forms.DataGridViewTextBoxColumn colCamera;
		private System.Windows.Forms.DataGridViewTextBoxColumn colItem;
		private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTime;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();

			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
			this.titlePanel = new System.Windows.Forms.Panel();
			this.navFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.panelNavLogin = new System.Windows.Forms.Panel();
			this.btnLogin = new System.Windows.Forms.Button();
			this.underlineLogin = new System.Windows.Forms.Panel();
			this.panelNavAlgorithm = new System.Windows.Forms.Panel();
			this.btnAlgorithmConfig = new System.Windows.Forms.Button();
			this.underlineAlgorithmConfig = new System.Windows.Forms.Panel();
			this.panelNavProcess = new System.Windows.Forms.Panel();
			this.btnProcessConfig = new System.Windows.Forms.Button();
			this.underlineProcessConfig = new System.Windows.Forms.Panel();
			this.panelNavCommunication = new System.Windows.Forms.Panel();
			this.btnCommunicateConfig = new System.Windows.Forms.Button();
			this.underlineCommunicateConfig = new System.Windows.Forms.Panel();
			this.panelNavDatabase = new System.Windows.Forms.Panel();
			this.btnDatabase = new System.Windows.Forms.Button();
			this.underlineDatabase = new System.Windows.Forms.Panel();
			this.panelNavSystem = new System.Windows.Forms.Panel();
			this.btnSystemSetting = new System.Windows.Forms.Button();
			this.underlineSystemSetting = new System.Windows.Forms.Panel();
			this.panelNavStop = new System.Windows.Forms.Panel();
			this.btnStop = new System.Windows.Forms.Button();
			this.underlineStop = new System.Windows.Forms.Panel();
			this.panelRightStatus = new System.Windows.Forms.Panel();
			this.btnClose = new System.Windows.Forms.Button();
			this.btnMinimize = new System.Windows.Forms.Button();
			this.lblUser = new System.Windows.Forms.Label();
			this.lblRightDivider = new System.Windows.Forms.Label();
			this.lblRunStatus = new System.Windows.Forms.Label();
			this.panelBrand = new System.Windows.Forms.Panel();
			this.lblLogo = new System.Windows.Forms.Label();
			this.lblTitle = new System.Windows.Forms.Label();
			this.pageHost = new System.Windows.Forms.Panel();
			this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelCameras = new System.Windows.Forms.TableLayoutPanel();
			this.resultPanel = new System.Windows.Forms.Panel();
			this.dgvResults = new System.Windows.Forms.DataGridView();
			this.colCamera = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.logPanel = new System.Windows.Forms.Panel();
			this.lstLog = new System.Windows.Forms.ListBox();
			this.cmbLogLevel = new System.Windows.Forms.ComboBox();
			this.btnClearLog = new System.Windows.Forms.Button();
			this.lblLogTitle = new System.Windows.Forms.Label();
			this.statusPanel = new System.Windows.Forms.Panel();
			this.btnLanguage = new System.Windows.Forms.Button();
			this.lblVersion = new System.Windows.Forms.Label();
			this.lblPlcStatus = new System.Windows.Forms.Label();
			this.lblCameraStatus = new System.Windows.Forms.Label();
			this.rootLayout.SuspendLayout();
			this.titlePanel.SuspendLayout();
			this.navFlowPanel.SuspendLayout();
			this.panelNavLogin.SuspendLayout();
			this.panelNavAlgorithm.SuspendLayout();
			this.panelNavProcess.SuspendLayout();
			this.panelNavCommunication.SuspendLayout();
			this.panelNavDatabase.SuspendLayout();
			this.panelNavSystem.SuspendLayout();
			this.panelNavStop.SuspendLayout();
			this.panelRightStatus.SuspendLayout();
			this.panelBrand.SuspendLayout();
			this.pageHost.SuspendLayout();
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
			this.rootLayout.Controls.Add(this.pageHost, 0, 1);
			this.rootLayout.Controls.Add(this.statusPanel, 0, 2);
			this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rootLayout.Location = new System.Drawing.Point(0, 0);
			this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
			this.rootLayout.Name = "rootLayout";
			this.rootLayout.RowCount = 3;
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92F));
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.rootLayout.Size = new System.Drawing.Size(1500, 850);
			this.rootLayout.TabIndex = 0;
			// 
			// titlePanel
			// 
			this.titlePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.titlePanel.Controls.Add(this.navFlowPanel);
			this.titlePanel.Controls.Add(this.panelRightStatus);
			this.titlePanel.Controls.Add(this.panelBrand);
			this.titlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.titlePanel.Location = new System.Drawing.Point(0, 0);
			this.titlePanel.Margin = new System.Windows.Forms.Padding(0);
			this.titlePanel.Name = "titlePanel";
			this.titlePanel.Size = new System.Drawing.Size(1500, 92);
			this.titlePanel.TabIndex = 0;
			// 
			// navFlowPanel
			// 
			this.navFlowPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.navFlowPanel.Controls.Add(this.panelNavLogin);
			this.navFlowPanel.Controls.Add(this.panelNavAlgorithm);
			this.navFlowPanel.Controls.Add(this.panelNavProcess);
			this.navFlowPanel.Controls.Add(this.panelNavCommunication);
			this.navFlowPanel.Controls.Add(this.panelNavDatabase);
			this.navFlowPanel.Controls.Add(this.panelNavSystem);
			this.navFlowPanel.Controls.Add(this.panelNavStop);
			this.navFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.navFlowPanel.Location = new System.Drawing.Point(230, 0);
			this.navFlowPanel.Margin = new System.Windows.Forms.Padding(0);
			this.navFlowPanel.Name = "navFlowPanel";
			this.navFlowPanel.Size = new System.Drawing.Size(910, 92);
			this.navFlowPanel.TabIndex = 0;
			this.navFlowPanel.WrapContents = false;
			// 
			// panelNavLogin
			// 
			this.panelNavLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelNavLogin.Controls.Add(this.btnLogin);
			this.panelNavLogin.Controls.Add(this.underlineLogin);
			this.panelNavLogin.Location = new System.Drawing.Point(0, 0);
			this.panelNavLogin.Margin = new System.Windows.Forms.Padding(0);
			this.panelNavLogin.Name = "panelNavLogin";
			this.panelNavLogin.Size = new System.Drawing.Size(120, 92);
			this.panelNavLogin.TabIndex = 0;
			// 
			// btnLogin
			// 
			this.btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnLogin.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnLogin.FlatAppearance.BorderSize = 0;
			this.btnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(50)))), ((int)(((byte)(82)))));
			this.btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(36)))), ((int)(((byte)(62)))));
			this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLogin.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnLogin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(170)))), ((int)(((byte)(195)))));
			this.btnLogin.Location = new System.Drawing.Point(0, 0);
			this.btnLogin.Name = "btnLogin";
			this.btnLogin.Size = new System.Drawing.Size(120, 89);
			this.btnLogin.TabIndex = 0;
			this.btnLogin.Text = "⌂  主页";
			this.btnLogin.UseVisualStyleBackColor = false;
			this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
			// 
			// underlineLogin
			// 
			this.underlineLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
			this.underlineLogin.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.underlineLogin.Location = new System.Drawing.Point(0, 89);
			this.underlineLogin.Name = "underlineLogin";
			this.underlineLogin.Size = new System.Drawing.Size(120, 3);
			this.underlineLogin.TabIndex = 1;
			this.underlineLogin.Visible = false;
			// 
			// panelNavAlgorithm
			// 
			this.panelNavAlgorithm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelNavAlgorithm.Controls.Add(this.btnAlgorithmConfig);
			this.panelNavAlgorithm.Controls.Add(this.underlineAlgorithmConfig);
			this.panelNavAlgorithm.Location = new System.Drawing.Point(120, 0);
			this.panelNavAlgorithm.Margin = new System.Windows.Forms.Padding(0);
			this.panelNavAlgorithm.Name = "panelNavAlgorithm";
			this.panelNavAlgorithm.Size = new System.Drawing.Size(135, 92);
			this.panelNavAlgorithm.TabIndex = 1;
			// 
			// btnAlgorithmConfig
			// 
			this.btnAlgorithmConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnAlgorithmConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnAlgorithmConfig.FlatAppearance.BorderSize = 0;
			this.btnAlgorithmConfig.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(50)))), ((int)(((byte)(82)))));
			this.btnAlgorithmConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(36)))), ((int)(((byte)(62)))));
			this.btnAlgorithmConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAlgorithmConfig.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnAlgorithmConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(170)))), ((int)(((byte)(195)))));
			this.btnAlgorithmConfig.Location = new System.Drawing.Point(0, 0);
			this.btnAlgorithmConfig.Name = "btnAlgorithmConfig";
			this.btnAlgorithmConfig.Size = new System.Drawing.Size(135, 89);
			this.btnAlgorithmConfig.TabIndex = 0;
			this.btnAlgorithmConfig.Text = "▣  算法模块";
			this.btnAlgorithmConfig.UseVisualStyleBackColor = false;
			this.btnAlgorithmConfig.Click += new System.EventHandler(this.btnAlgorithmConfig_Click);
			// 
			// underlineAlgorithmConfig
			// 
			this.underlineAlgorithmConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
			this.underlineAlgorithmConfig.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.underlineAlgorithmConfig.Location = new System.Drawing.Point(0, 89);
			this.underlineAlgorithmConfig.Name = "underlineAlgorithmConfig";
			this.underlineAlgorithmConfig.Size = new System.Drawing.Size(135, 3);
			this.underlineAlgorithmConfig.TabIndex = 1;
			this.underlineAlgorithmConfig.Visible = false;
			// 
			// panelNavProcess
			// 
			this.panelNavProcess.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelNavProcess.Controls.Add(this.btnProcessConfig);
			this.panelNavProcess.Controls.Add(this.underlineProcessConfig);
			this.panelNavProcess.Location = new System.Drawing.Point(255, 0);
			this.panelNavProcess.Margin = new System.Windows.Forms.Padding(0);
			this.panelNavProcess.Name = "panelNavProcess";
			this.panelNavProcess.Size = new System.Drawing.Size(135, 92);
			this.panelNavProcess.TabIndex = 2;
			// 
			// btnProcessConfig
			// 
			this.btnProcessConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnProcessConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnProcessConfig.FlatAppearance.BorderSize = 0;
			this.btnProcessConfig.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(50)))), ((int)(((byte)(82)))));
			this.btnProcessConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(36)))), ((int)(((byte)(62)))));
			this.btnProcessConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnProcessConfig.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnProcessConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(170)))), ((int)(((byte)(195)))));
			this.btnProcessConfig.Location = new System.Drawing.Point(0, 0);
			this.btnProcessConfig.Name = "btnProcessConfig";
			this.btnProcessConfig.Size = new System.Drawing.Size(135, 89);
			this.btnProcessConfig.TabIndex = 0;
			this.btnProcessConfig.Text = "⚙  配置管理";
			this.btnProcessConfig.UseVisualStyleBackColor = false;
			this.btnProcessConfig.Click += new System.EventHandler(this.btnProcessConfig_Click);
			// 
			// underlineProcessConfig
			// 
			this.underlineProcessConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
			this.underlineProcessConfig.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.underlineProcessConfig.Location = new System.Drawing.Point(0, 89);
			this.underlineProcessConfig.Name = "underlineProcessConfig";
			this.underlineProcessConfig.Size = new System.Drawing.Size(135, 3);
			this.underlineProcessConfig.TabIndex = 1;
			this.underlineProcessConfig.Visible = false;
			// 
			// panelNavCommunication
			// 
			this.panelNavCommunication.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelNavCommunication.Controls.Add(this.btnCommunicateConfig);
			this.panelNavCommunication.Controls.Add(this.underlineCommunicateConfig);
			this.panelNavCommunication.Location = new System.Drawing.Point(390, 0);
			this.panelNavCommunication.Margin = new System.Windows.Forms.Padding(0);
			this.panelNavCommunication.Name = "panelNavCommunication";
			this.panelNavCommunication.Size = new System.Drawing.Size(135, 92);
			this.panelNavCommunication.TabIndex = 3;
			// 
			// btnCommunicateConfig
			// 
			this.btnCommunicateConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnCommunicateConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnCommunicateConfig.FlatAppearance.BorderSize = 0;
			this.btnCommunicateConfig.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(50)))), ((int)(((byte)(82)))));
			this.btnCommunicateConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(36)))), ((int)(((byte)(62)))));
			this.btnCommunicateConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCommunicateConfig.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnCommunicateConfig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(170)))), ((int)(((byte)(195)))));
			this.btnCommunicateConfig.Location = new System.Drawing.Point(0, 0);
			this.btnCommunicateConfig.Name = "btnCommunicateConfig";
			this.btnCommunicateConfig.Size = new System.Drawing.Size(135, 89);
			this.btnCommunicateConfig.TabIndex = 0;
			this.btnCommunicateConfig.Text = "◇  通讯配置";
			this.btnCommunicateConfig.UseVisualStyleBackColor = false;
			this.btnCommunicateConfig.Click += new System.EventHandler(this.btnCommunicateConfig_Click);
			// 
			// underlineCommunicateConfig
			// 
			this.underlineCommunicateConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
			this.underlineCommunicateConfig.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.underlineCommunicateConfig.Location = new System.Drawing.Point(0, 89);
			this.underlineCommunicateConfig.Name = "underlineCommunicateConfig";
			this.underlineCommunicateConfig.Size = new System.Drawing.Size(135, 3);
			this.underlineCommunicateConfig.TabIndex = 1;
			this.underlineCommunicateConfig.Visible = false;
			// 
			// panelNavDatabase
			// 
			this.panelNavDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelNavDatabase.Controls.Add(this.btnDatabase);
			this.panelNavDatabase.Controls.Add(this.underlineDatabase);
			this.panelNavDatabase.Location = new System.Drawing.Point(525, 0);
			this.panelNavDatabase.Margin = new System.Windows.Forms.Padding(0);
			this.panelNavDatabase.Name = "panelNavDatabase";
			this.panelNavDatabase.Size = new System.Drawing.Size(120, 92);
			this.panelNavDatabase.TabIndex = 4;
			// 
			// btnDatabase
			// 
			this.btnDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnDatabase.FlatAppearance.BorderSize = 0;
			this.btnDatabase.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(50)))), ((int)(((byte)(82)))));
			this.btnDatabase.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(36)))), ((int)(((byte)(62)))));
			this.btnDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDatabase.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnDatabase.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(170)))), ((int)(((byte)(195)))));
			this.btnDatabase.Location = new System.Drawing.Point(0, 0);
			this.btnDatabase.Name = "btnDatabase";
			this.btnDatabase.Size = new System.Drawing.Size(120, 89);
			this.btnDatabase.TabIndex = 0;
			this.btnDatabase.Text = "▤  数据库";
			this.btnDatabase.UseVisualStyleBackColor = false;
			this.btnDatabase.Click += new System.EventHandler(this.btnDatabase_Click);
			// 
			// underlineDatabase
			// 
			this.underlineDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
			this.underlineDatabase.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.underlineDatabase.Location = new System.Drawing.Point(0, 89);
			this.underlineDatabase.Name = "underlineDatabase";
			this.underlineDatabase.Size = new System.Drawing.Size(120, 3);
			this.underlineDatabase.TabIndex = 1;
			this.underlineDatabase.Visible = false;
			// 
			// panelNavSystem
			// 
			this.panelNavSystem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelNavSystem.Controls.Add(this.btnSystemSetting);
			this.panelNavSystem.Controls.Add(this.underlineSystemSetting);
			this.panelNavSystem.Location = new System.Drawing.Point(645, 0);
			this.panelNavSystem.Margin = new System.Windows.Forms.Padding(0);
			this.panelNavSystem.Name = "panelNavSystem";
			this.panelNavSystem.Size = new System.Drawing.Size(135, 92);
			this.panelNavSystem.TabIndex = 5;
			// 
			// btnSystemSetting
			// 
			this.btnSystemSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnSystemSetting.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnSystemSetting.FlatAppearance.BorderSize = 0;
			this.btnSystemSetting.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(50)))), ((int)(((byte)(82)))));
			this.btnSystemSetting.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(36)))), ((int)(((byte)(62)))));
			this.btnSystemSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSystemSetting.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnSystemSetting.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(170)))), ((int)(((byte)(195)))));
			this.btnSystemSetting.Location = new System.Drawing.Point(0, 0);
			this.btnSystemSetting.Name = "btnSystemSetting";
			this.btnSystemSetting.Size = new System.Drawing.Size(135, 89);
			this.btnSystemSetting.TabIndex = 0;
			this.btnSystemSetting.Text = "⚙  系统管理";
			this.btnSystemSetting.UseVisualStyleBackColor = false;
			this.btnSystemSetting.Click += new System.EventHandler(this.btnSystemSetting_Click);
			// 
			// underlineSystemSetting
			// 
			this.underlineSystemSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
			this.underlineSystemSetting.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.underlineSystemSetting.Location = new System.Drawing.Point(0, 89);
			this.underlineSystemSetting.Name = "underlineSystemSetting";
			this.underlineSystemSetting.Size = new System.Drawing.Size(135, 3);
			this.underlineSystemSetting.TabIndex = 1;
			this.underlineSystemSetting.Visible = false;
			// 
			// panelNavStop
			// 
			this.panelNavStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelNavStop.Controls.Add(this.btnStop);
			this.panelNavStop.Controls.Add(this.underlineStop);
			this.panelNavStop.Location = new System.Drawing.Point(780, 0);
			this.panelNavStop.Margin = new System.Windows.Forms.Padding(0);
			this.panelNavStop.Name = "panelNavStop";
			this.panelNavStop.Size = new System.Drawing.Size(110, 92);
			this.panelNavStop.TabIndex = 6;
			// 
			// btnStop
			// 
			this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnStop.FlatAppearance.BorderSize = 0;
			this.btnStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(50)))), ((int)(((byte)(82)))));
			this.btnStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(36)))), ((int)(((byte)(62)))));
			this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnStop.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnStop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(54)))), ((int)(((byte)(65)))));
			this.btnStop.Location = new System.Drawing.Point(0, 0);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(110, 89);
			this.btnStop.TabIndex = 0;
			this.btnStop.Text = "□  停止";
			this.btnStop.UseVisualStyleBackColor = false;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// underlineStop
			// 
			this.underlineStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
			this.underlineStop.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.underlineStop.Location = new System.Drawing.Point(0, 89);
			this.underlineStop.Name = "underlineStop";
			this.underlineStop.Size = new System.Drawing.Size(110, 3);
			this.underlineStop.TabIndex = 1;
			this.underlineStop.Visible = false;
			// 
			// panelRightStatus
			// 
			this.panelRightStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelRightStatus.Controls.Add(this.btnClose);
			this.panelRightStatus.Controls.Add(this.btnMinimize);
			this.panelRightStatus.Controls.Add(this.lblUser);
			this.panelRightStatus.Controls.Add(this.lblRightDivider);
			this.panelRightStatus.Controls.Add(this.lblRunStatus);
			this.panelRightStatus.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelRightStatus.Location = new System.Drawing.Point(1140, 0);
			this.panelRightStatus.Name = "panelRightStatus";
			this.panelRightStatus.Size = new System.Drawing.Size(360, 92);
			this.panelRightStatus.TabIndex = 1;
			// 
			// btnClose
			// 
			this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnClose.FlatAppearance.BorderSize = 0;
			this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(30)))), ((int)(((byte)(42)))));
			this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(20)))), ((int)(((byte)(32)))));
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
			this.btnClose.ForeColor = System.Drawing.Color.White;
			this.btnClose.Location = new System.Drawing.Point(328, 0);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(32, 30);
			this.btnClose.TabIndex = 0;
			this.btnClose.Text = "×";
			this.btnClose.UseVisualStyleBackColor = false;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// btnMinimize
			// 
			this.btnMinimize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.btnMinimize.FlatAppearance.BorderSize = 0;
			this.btnMinimize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(35)))), ((int)(((byte)(58)))));
			this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(28)))), ((int)(((byte)(48)))));
			this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
			this.btnMinimize.ForeColor = System.Drawing.Color.White;
			this.btnMinimize.Location = new System.Drawing.Point(295, 0);
			this.btnMinimize.Name = "btnMinimize";
			this.btnMinimize.Size = new System.Drawing.Size(34, 30);
			this.btnMinimize.TabIndex = 2;
			this.btnMinimize.Text = "—";
			this.btnMinimize.UseVisualStyleBackColor = false;
			this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
			// 
			// lblUser
			// 
			this.lblUser.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold);
			this.lblUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(220)))), ((int)(((byte)(235)))));
			this.lblUser.Location = new System.Drawing.Point(152, 45);
			this.lblUser.Name = "lblUser";
			this.lblUser.Size = new System.Drawing.Size(115, 34);
			this.lblUser.TabIndex = 3;
			this.lblUser.Text = "♟  admin ▾";
			this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblRightDivider
			// 
			this.lblRightDivider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(42)))), ((int)(((byte)(70)))));
			this.lblRightDivider.Location = new System.Drawing.Point(142, 47);
			this.lblRightDivider.Name = "lblRightDivider";
			this.lblRightDivider.Size = new System.Drawing.Size(1, 30);
			this.lblRightDivider.TabIndex = 4;
			// 
			// lblRunStatus
			// 
			this.lblRunStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold);
			this.lblRunStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(220)))), ((int)(((byte)(235)))));
			this.lblRunStatus.Location = new System.Drawing.Point(16, 45);
			this.lblRunStatus.Name = "lblRunStatus";
			this.lblRunStatus.Size = new System.Drawing.Size(120, 34);
			this.lblRunStatus.TabIndex = 5;
			this.lblRunStatus.Text = "●  运行中";
			this.lblRunStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panelBrand
			// 
			this.panelBrand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.panelBrand.Controls.Add(this.lblLogo);
			this.panelBrand.Controls.Add(this.lblTitle);
			this.panelBrand.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelBrand.Location = new System.Drawing.Point(0, 0);
			this.panelBrand.Name = "panelBrand";
			this.panelBrand.Size = new System.Drawing.Size(230, 92);
			this.panelBrand.TabIndex = 2;
			// 
			// lblLogo
			// 
			this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 34F, System.Drawing.FontStyle.Bold);
			this.lblLogo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
			this.lblLogo.Location = new System.Drawing.Point(18, 4);
			this.lblLogo.Name = "lblLogo";
			this.lblLogo.Size = new System.Drawing.Size(60, 78);
			this.lblLogo.TabIndex = 0;
			this.lblLogo.Text = "B";
			this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblTitle
			// 
			this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 13.5F, System.Drawing.FontStyle.Bold);
			this.lblTitle.ForeColor = System.Drawing.Color.White;
			this.lblTitle.Location = new System.Drawing.Point(78, 8);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(145, 72);
			this.lblTitle.TabIndex = 1;
			this.lblTitle.Text = "Betterway\r\nVision-Base";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pageHost
			// 
			this.pageHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(14)))), ((int)(((byte)(25)))));
			this.pageHost.Controls.Add(this.mainLayout);
			this.pageHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageHost.Location = new System.Drawing.Point(0, 92);
			this.pageHost.Margin = new System.Windows.Forms.Padding(0);
			this.pageHost.Name = "pageHost";
			this.pageHost.Size = new System.Drawing.Size(1500, 718);
			this.pageHost.TabIndex = 1;
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
			this.mainLayout.Location = new System.Drawing.Point(0, 0);
			this.mainLayout.Margin = new System.Windows.Forms.Padding(0);
			this.mainLayout.Name = "mainLayout";
			this.mainLayout.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
			this.mainLayout.RowCount = 2;
			this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 78F));
			this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22F));
			this.mainLayout.Size = new System.Drawing.Size(1500, 718);
			this.mainLayout.TabIndex = 0;
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
			this.tableLayoutPanelCameras.Size = new System.Drawing.Size(1127, 553);
			this.tableLayoutPanelCameras.TabIndex = 0;
			// 
			// resultPanel
			// 
			this.resultPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.resultPanel.Controls.Add(this.dgvResults);
			this.resultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultPanel.Location = new System.Drawing.Point(1143, 8);
			this.resultPanel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 8);
			this.resultPanel.Name = "resultPanel";
			this.resultPanel.Padding = new System.Windows.Forms.Padding(10);
			this.resultPanel.Size = new System.Drawing.Size(349, 545);
			this.resultPanel.TabIndex = 1;
			// 
			// dgvResults
			// 
			this.dgvResults.AllowUserToAddRows = false;
			this.dgvResults.AllowUserToDeleteRows = false;
			this.dgvResults.AllowUserToResizeRows = false;
			this.dgvResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvResults.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			this.dgvResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(29)))), ((int)(((byte)(50)))));
			dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(205)))), ((int)(((byte)(220)))));
			dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(29)))), ((int)(((byte)(50)))));
			dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvResults.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
			this.dgvResults.ColumnHeadersHeight = 32;
			this.dgvResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCamera,
            this.colItem,
            this.colValue,
            this.colTime});
			dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(24)))), ((int)(((byte)(42)))));
			dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle6.ForeColor = System.Drawing.Color.WhiteSmoke;
			dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(78)))), ((int)(((byte)(145)))));
			dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvResults.DefaultCellStyle = dataGridViewCellStyle6;
			this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvResults.EnableHeadersVisualStyles = false;
			this.dgvResults.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.dgvResults.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(48)))), ((int)(((byte)(70)))));
			this.dgvResults.Location = new System.Drawing.Point(10, 10);
			this.dgvResults.MultiSelect = false;
			this.dgvResults.Name = "dgvResults";
			this.dgvResults.ReadOnly = true;
			this.dgvResults.RowHeadersVisible = false;
			this.dgvResults.RowHeadersWidth = 62;
			this.dgvResults.RowTemplate.Height = 30;
			this.dgvResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvResults.Size = new System.Drawing.Size(329, 525);
			this.dgvResults.TabIndex = 0;
			// 
			// colCamera
			// 
			this.colCamera.HeaderText = "Camera";
			this.colCamera.MinimumWidth = 8;
			this.colCamera.Name = "colCamera";
			this.colCamera.ReadOnly = true;
			// 
			// colItem
			// 
			this.colItem.HeaderText = "Item";
			this.colItem.MinimumWidth = 8;
			this.colItem.Name = "colItem";
			this.colItem.ReadOnly = true;
			// 
			// colValue
			// 
			this.colValue.HeaderText = "Value";
			this.colValue.MinimumWidth = 8;
			this.colValue.Name = "colValue";
			this.colValue.ReadOnly = true;
			// 
			// colTime
			// 
			this.colTime.HeaderText = "Time";
			this.colTime.MinimumWidth = 8;
			this.colTime.Name = "colTime";
			this.colTime.ReadOnly = true;
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
			this.logPanel.Location = new System.Drawing.Point(8, 561);
			this.logPanel.Margin = new System.Windows.Forms.Padding(0);
			this.logPanel.Name = "logPanel";
			this.logPanel.Padding = new System.Windows.Forms.Padding(12, 6, 12, 8);
			this.logPanel.Size = new System.Drawing.Size(1484, 157);
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
			this.lstLog.Size = new System.Drawing.Size(1460, 113);
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
			this.cmbLogLevel.Location = new System.Drawing.Point(3598, 8);
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
			this.btnClearLog.Location = new System.Drawing.Point(3728, 7);
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
			this.statusPanel.Controls.Add(this.btnLanguage);
			this.statusPanel.Controls.Add(this.lblVersion);
			this.statusPanel.Controls.Add(this.lblPlcStatus);
			this.statusPanel.Controls.Add(this.lblCameraStatus);
			this.statusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusPanel.Location = new System.Drawing.Point(0, 810);
			this.statusPanel.Margin = new System.Windows.Forms.Padding(0);
			this.statusPanel.Name = "statusPanel";
			this.statusPanel.Size = new System.Drawing.Size(1500, 40);
			this.statusPanel.TabIndex = 2;
			// 
			// btnLanguage
			// 
			this.btnLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(24)))), ((int)(((byte)(38)))));
			this.btnLanguage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnLanguage.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(70)))), ((int)(((byte)(105)))));
			this.btnLanguage.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(48)))), ((int)(((byte)(78)))));
			this.btnLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLanguage.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
			this.btnLanguage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.btnLanguage.Location = new System.Drawing.Point(2560, 7);
			this.btnLanguage.Name = "btnLanguage";
			this.btnLanguage.Size = new System.Drawing.Size(100, 26);
			this.btnLanguage.TabIndex = 0;
			this.btnLanguage.Text = "中文 / EN";
			this.btnLanguage.UseVisualStyleBackColor = false;
			this.btnLanguage.Click += new System.EventHandler(this.btnLanguage_Click);
			// 
			// lblVersion
			// 
			this.lblVersion.Dock = System.Windows.Forms.DockStyle.Right;
			this.lblVersion.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
			this.lblVersion.Location = new System.Drawing.Point(1320, 0);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(180, 40);
			this.lblVersion.TabIndex = 1;
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
			this.lblPlcStatus.TabIndex = 2;
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
			this.lblCameraStatus.TabIndex = 3;
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
			this.navFlowPanel.ResumeLayout(false);
			this.panelNavLogin.ResumeLayout(false);
			this.panelNavAlgorithm.ResumeLayout(false);
			this.panelNavProcess.ResumeLayout(false);
			this.panelNavCommunication.ResumeLayout(false);
			this.panelNavDatabase.ResumeLayout(false);
			this.panelNavSystem.ResumeLayout(false);
			this.panelNavStop.ResumeLayout(false);
			this.panelRightStatus.ResumeLayout(false);
			this.panelBrand.ResumeLayout(false);
			this.pageHost.ResumeLayout(false);
			this.mainLayout.ResumeLayout(false);
			this.resultPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
			this.logPanel.ResumeLayout(false);
			this.statusPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	}
}
