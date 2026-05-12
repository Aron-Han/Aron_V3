namespace Aron_V3
{
	partial class CommunicationConfigControl
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

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
			this.panelType = new System.Windows.Forms.Panel();
			this.btnS7 = new System.Windows.Forms.Button();
			this.btnProfinet = new System.Windows.Forms.Button();
			this.btnTcpIp = new System.Windows.Forms.Button();
			this.lblTypeTitle = new System.Windows.Forms.Label();
			this.panelParams = new System.Windows.Forms.Panel();
			this.grpTest = new System.Windows.Forms.GroupBox();
			this.btnClearTest = new System.Windows.Forms.Button();
			this.btnSendTest = new System.Windows.Forms.Button();
			this.txtReceive = new System.Windows.Forms.TextBox();
			this.lblReceive = new System.Windows.Forms.Label();
			this.txtSend = new System.Windows.Forms.TextBox();
			this.lblSend = new System.Windows.Forms.Label();
			this.grpParams = new System.Windows.Forms.GroupBox();
			this.paramLayout = new System.Windows.Forms.TableLayoutPanel();
			this.lblP1 = new System.Windows.Forms.Label();
			this.txtP1 = new System.Windows.Forms.TextBox();
			this.lblP2 = new System.Windows.Forms.Label();
			this.txtP2 = new System.Windows.Forms.TextBox();
			this.lblP3 = new System.Windows.Forms.Label();
			this.txtP3 = new System.Windows.Forms.TextBox();
			this.lblP4 = new System.Windows.Forms.Label();
			this.txtP4 = new System.Windows.Forms.TextBox();
			this.lblP5 = new System.Windows.Forms.Label();
			this.txtP5 = new System.Windows.Forms.TextBox();
			this.lblP6 = new System.Windows.Forms.Label();
			this.txtP6 = new System.Windows.Forms.TextBox();
			this.cmbMode = new System.Windows.Forms.ComboBox();
			this.lblParamTitle = new System.Windows.Forms.Label();
			this.rightLayout = new System.Windows.Forms.TableLayoutPanel();
			this.panelInput = new System.Windows.Forms.Panel();
			this.dgvInput = new System.Windows.Forms.DataGridView();
			this.colInputName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colInputTrigger = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colInputType = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.colInputByteOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colInputBitOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colInputLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colInputRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panelInputButtons = new System.Windows.Forms.Panel();
			this.btnDeleteInput = new System.Windows.Forms.Button();
			this.btnAddInput = new System.Windows.Forms.Button();
			this.lblInputTitle = new System.Windows.Forms.Label();
			this.panelOutput = new System.Windows.Forms.Panel();
			this.dgvOutput = new System.Windows.Forms.DataGridView();
			this.colOutputName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colOutputType = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.colOutputByteOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colOutputBitOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colOutputLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colOutputRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panelOutputButtons = new System.Windows.Forms.Panel();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnDeleteOutput = new System.Windows.Forms.Button();
			this.btnAddOutput = new System.Windows.Forms.Button();
			this.lblOutputTitle = new System.Windows.Forms.Label();
			this.mainLayout.SuspendLayout();
			this.panelType.SuspendLayout();
			this.panelParams.SuspendLayout();
			this.grpTest.SuspendLayout();
			this.grpParams.SuspendLayout();
			this.paramLayout.SuspendLayout();
			this.rightLayout.SuspendLayout();
			this.panelInput.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvInput)).BeginInit();
			this.panelInputButtons.SuspendLayout();
			this.panelOutput.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvOutput)).BeginInit();
			this.panelOutputButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainLayout
			// 
			this.mainLayout.ColumnCount = 3;
			this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 230F));
			this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 360F));
			this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainLayout.Controls.Add(this.panelType, 0, 0);
			this.mainLayout.Controls.Add(this.panelParams, 1, 0);
			this.mainLayout.Controls.Add(this.rightLayout, 2, 0);
			this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainLayout.Location = new System.Drawing.Point(0, 0);
			this.mainLayout.Name = "mainLayout";
			this.mainLayout.Padding = new System.Windows.Forms.Padding(8);
			this.mainLayout.RowCount = 1;
			this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainLayout.Size = new System.Drawing.Size(1200, 720);
			this.mainLayout.TabIndex = 0;
			// 
			// panelType
			// 
			this.panelType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.panelType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelType.Controls.Add(this.btnS7);
			this.panelType.Controls.Add(this.btnProfinet);
			this.panelType.Controls.Add(this.btnTcpIp);
			this.panelType.Controls.Add(this.lblTypeTitle);
			this.panelType.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelType.Location = new System.Drawing.Point(11, 11);
			this.panelType.Name = "panelType";
			this.panelType.Size = new System.Drawing.Size(224, 698);
			this.panelType.TabIndex = 0;
			// 
			// btnS7
			// 
			this.btnS7.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnS7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnS7.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnS7.ForeColor = System.Drawing.Color.White;
			this.btnS7.Location = new System.Drawing.Point(18, 182);
			this.btnS7.Name = "btnS7";
			this.btnS7.Size = new System.Drawing.Size(188, 44);
			this.btnS7.TabIndex = 3;
			this.btnS7.Text = "S7";
			this.btnS7.UseVisualStyleBackColor = true;
			this.btnS7.Click += new System.EventHandler(this.btnS7_Click);
			// 
			// btnProfinet
			// 
			this.btnProfinet.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnProfinet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnProfinet.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnProfinet.ForeColor = System.Drawing.Color.White;
			this.btnProfinet.Location = new System.Drawing.Point(18, 120);
			this.btnProfinet.Name = "btnProfinet";
			this.btnProfinet.Size = new System.Drawing.Size(188, 44);
			this.btnProfinet.TabIndex = 2;
			this.btnProfinet.Text = "Profinet";
			this.btnProfinet.UseVisualStyleBackColor = true;
			this.btnProfinet.Click += new System.EventHandler(this.btnProfinet_Click);
			// 
			// btnTcpIp
			// 
			this.btnTcpIp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnTcpIp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTcpIp.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnTcpIp.ForeColor = System.Drawing.Color.White;
			this.btnTcpIp.Location = new System.Drawing.Point(18, 58);
			this.btnTcpIp.Name = "btnTcpIp";
			this.btnTcpIp.Size = new System.Drawing.Size(188, 44);
			this.btnTcpIp.TabIndex = 1;
			this.btnTcpIp.Text = "TCP/IP";
			this.btnTcpIp.UseVisualStyleBackColor = true;
			this.btnTcpIp.Click += new System.EventHandler(this.btnTcpIp_Click);
			// 
			// lblTypeTitle
			// 
			this.lblTypeTitle.AutoSize = true;
			this.lblTypeTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblTypeTitle.ForeColor = System.Drawing.Color.White;
			this.lblTypeTitle.Location = new System.Drawing.Point(18, 22);
			this.lblTypeTitle.Name = "lblTypeTitle";
			this.lblTypeTitle.Size = new System.Drawing.Size(101, 30);
			this.lblTypeTitle.TabIndex = 0;
			this.lblTypeTitle.Text = "通讯类型";
			// 
			// panelParams
			// 
			this.panelParams.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.panelParams.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelParams.Controls.Add(this.grpTest);
			this.panelParams.Controls.Add(this.grpParams);
			this.panelParams.Controls.Add(this.lblParamTitle);
			this.panelParams.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelParams.Location = new System.Drawing.Point(241, 11);
			this.panelParams.Name = "panelParams";
			this.panelParams.Size = new System.Drawing.Size(354, 698);
			this.panelParams.TabIndex = 1;
			// 
			// grpTest
			// 
			this.grpTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.grpTest.Controls.Add(this.btnClearTest);
			this.grpTest.Controls.Add(this.btnSendTest);
			this.grpTest.Controls.Add(this.txtReceive);
			this.grpTest.Controls.Add(this.lblReceive);
			this.grpTest.Controls.Add(this.txtSend);
			this.grpTest.Controls.Add(this.lblSend);
			this.grpTest.ForeColor = System.Drawing.Color.White;
			this.grpTest.Location = new System.Drawing.Point(18, 352);
			this.grpTest.Name = "grpTest";
			this.grpTest.Size = new System.Drawing.Size(316, 326);
			this.grpTest.TabIndex = 2;
			this.grpTest.TabStop = false;
			this.grpTest.Text = "测试收发数据报文";
			// 
			// btnClearTest
			// 
			this.btnClearTest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnClearTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClearTest.ForeColor = System.Drawing.Color.White;
			this.btnClearTest.Location = new System.Drawing.Point(172, 280);
			this.btnClearTest.Name = "btnClearTest";
			this.btnClearTest.Size = new System.Drawing.Size(120, 32);
			this.btnClearTest.TabIndex = 5;
			this.btnClearTest.Text = "清空";
			this.btnClearTest.UseVisualStyleBackColor = true;
			this.btnClearTest.Click += new System.EventHandler(this.btnClearTest_Click);
			// 
			// btnSendTest
			// 
			this.btnSendTest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnSendTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSendTest.ForeColor = System.Drawing.Color.White;
			this.btnSendTest.Location = new System.Drawing.Point(28, 280);
			this.btnSendTest.Name = "btnSendTest";
			this.btnSendTest.Size = new System.Drawing.Size(120, 32);
			this.btnSendTest.TabIndex = 4;
			this.btnSendTest.Text = "发送测试";
			this.btnSendTest.UseVisualStyleBackColor = true;
			this.btnSendTest.Click += new System.EventHandler(this.btnSendTest_Click);
			// 
			// txtReceive
			// 
			this.txtReceive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtReceive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.txtReceive.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtReceive.ForeColor = System.Drawing.Color.White;
			this.txtReceive.Location = new System.Drawing.Point(28, 170);
			this.txtReceive.Multiline = true;
			this.txtReceive.Name = "txtReceive";
			this.txtReceive.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtReceive.Size = new System.Drawing.Size(264, 94);
			this.txtReceive.TabIndex = 3;
			// 
			// lblReceive
			// 
			this.lblReceive.AutoSize = true;
			this.lblReceive.ForeColor = System.Drawing.Color.White;
			this.lblReceive.Location = new System.Drawing.Point(25, 148);
			this.lblReceive.Name = "lblReceive";
			this.lblReceive.Size = new System.Drawing.Size(82, 24);
			this.lblReceive.TabIndex = 2;
			this.lblReceive.Text = "接收报文";
			// 
			// txtSend
			// 
			this.txtSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.txtSend.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSend.ForeColor = System.Drawing.Color.White;
			this.txtSend.Location = new System.Drawing.Point(28, 48);
			this.txtSend.Multiline = true;
			this.txtSend.Name = "txtSend";
			this.txtSend.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtSend.Size = new System.Drawing.Size(264, 84);
			this.txtSend.TabIndex = 1;
			// 
			// lblSend
			// 
			this.lblSend.AutoSize = true;
			this.lblSend.ForeColor = System.Drawing.Color.White;
			this.lblSend.Location = new System.Drawing.Point(25, 26);
			this.lblSend.Name = "lblSend";
			this.lblSend.Size = new System.Drawing.Size(82, 24);
			this.lblSend.TabIndex = 0;
			this.lblSend.Text = "发送报文";
			// 
			// grpParams
			// 
			this.grpParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.grpParams.Controls.Add(this.paramLayout);
			this.grpParams.ForeColor = System.Drawing.Color.White;
			this.grpParams.Location = new System.Drawing.Point(18, 58);
			this.grpParams.Name = "grpParams";
			this.grpParams.Size = new System.Drawing.Size(316, 270);
			this.grpParams.TabIndex = 1;
			this.grpParams.TabStop = false;
			this.grpParams.Text = "通讯必要参数";
			// 
			// paramLayout
			// 
			this.paramLayout.ColumnCount = 2;
			this.paramLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.paramLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.paramLayout.Controls.Add(this.lblP1, 0, 0);
			this.paramLayout.Controls.Add(this.txtP1, 1, 0);
			this.paramLayout.Controls.Add(this.lblP2, 0, 1);
			this.paramLayout.Controls.Add(this.txtP2, 1, 1);
			this.paramLayout.Controls.Add(this.lblP3, 0, 2);
			this.paramLayout.Controls.Add(this.txtP3, 1, 2);
			this.paramLayout.Controls.Add(this.lblP4, 0, 3);
			this.paramLayout.Controls.Add(this.txtP4, 1, 3);
			this.paramLayout.Controls.Add(this.lblP5, 0, 4);
			this.paramLayout.Controls.Add(this.txtP5, 1, 4);
			this.paramLayout.Controls.Add(this.lblP6, 0, 5);
			this.paramLayout.Controls.Add(this.txtP6, 1, 5);
			this.paramLayout.Controls.Add(this.cmbMode, 1, 6);
			this.paramLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.paramLayout.Location = new System.Drawing.Point(3, 26);
			this.paramLayout.Name = "paramLayout";
			this.paramLayout.Padding = new System.Windows.Forms.Padding(14);
			this.paramLayout.RowCount = 7;
			this.paramLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.paramLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.paramLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.paramLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.paramLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.paramLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.paramLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.paramLayout.Size = new System.Drawing.Size(310, 241);
			this.paramLayout.TabIndex = 0;
			// 
			// lblP1
			// 
			this.lblP1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblP1.ForeColor = System.Drawing.Color.White;
			this.lblP1.Location = new System.Drawing.Point(17, 14);
			this.lblP1.Name = "lblP1";
			this.lblP1.Size = new System.Drawing.Size(114, 32);
			this.lblP1.TabIndex = 0;
			this.lblP1.Text = "本地IP";
			this.lblP1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtP1
			// 
			this.txtP1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.txtP1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtP1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtP1.ForeColor = System.Drawing.Color.White;
			this.txtP1.Location = new System.Drawing.Point(137, 17);
			this.txtP1.Name = "txtP1";
			this.txtP1.Size = new System.Drawing.Size(156, 30);
			this.txtP1.TabIndex = 1;
			// 
			// lblP2
			// 
			this.lblP2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblP2.ForeColor = System.Drawing.Color.White;
			this.lblP2.Location = new System.Drawing.Point(17, 46);
			this.lblP2.Name = "lblP2";
			this.lblP2.Size = new System.Drawing.Size(114, 32);
			this.lblP2.TabIndex = 2;
			this.lblP2.Text = "本地端口";
			this.lblP2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtP2
			// 
			this.txtP2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.txtP2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtP2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtP2.ForeColor = System.Drawing.Color.White;
			this.txtP2.Location = new System.Drawing.Point(137, 49);
			this.txtP2.Name = "txtP2";
			this.txtP2.Size = new System.Drawing.Size(156, 30);
			this.txtP2.TabIndex = 3;
			// 
			// lblP3
			// 
			this.lblP3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblP3.ForeColor = System.Drawing.Color.White;
			this.lblP3.Location = new System.Drawing.Point(17, 78);
			this.lblP3.Name = "lblP3";
			this.lblP3.Size = new System.Drawing.Size(114, 32);
			this.lblP3.TabIndex = 4;
			this.lblP3.Text = "远程IP";
			this.lblP3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtP3
			// 
			this.txtP3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.txtP3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtP3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtP3.ForeColor = System.Drawing.Color.White;
			this.txtP3.Location = new System.Drawing.Point(137, 81);
			this.txtP3.Name = "txtP3";
			this.txtP3.Size = new System.Drawing.Size(156, 30);
			this.txtP3.TabIndex = 5;
			// 
			// lblP4
			// 
			this.lblP4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblP4.ForeColor = System.Drawing.Color.White;
			this.lblP4.Location = new System.Drawing.Point(17, 110);
			this.lblP4.Name = "lblP4";
			this.lblP4.Size = new System.Drawing.Size(114, 32);
			this.lblP4.TabIndex = 6;
			this.lblP4.Text = "远程端口";
			this.lblP4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtP4
			// 
			this.txtP4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.txtP4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtP4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtP4.ForeColor = System.Drawing.Color.White;
			this.txtP4.Location = new System.Drawing.Point(137, 113);
			this.txtP4.Name = "txtP4";
			this.txtP4.Size = new System.Drawing.Size(156, 30);
			this.txtP4.TabIndex = 7;
			// 
			// lblP5
			// 
			this.lblP5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblP5.ForeColor = System.Drawing.Color.White;
			this.lblP5.Location = new System.Drawing.Point(17, 142);
			this.lblP5.Name = "lblP5";
			this.lblP5.Size = new System.Drawing.Size(114, 32);
			this.lblP5.TabIndex = 8;
			this.lblP5.Text = "参数5";
			this.lblP5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtP5
			// 
			this.txtP5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.txtP5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtP5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtP5.ForeColor = System.Drawing.Color.White;
			this.txtP5.Location = new System.Drawing.Point(137, 145);
			this.txtP5.Name = "txtP5";
			this.txtP5.Size = new System.Drawing.Size(156, 30);
			this.txtP5.TabIndex = 9;
			// 
			// lblP6
			// 
			this.lblP6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblP6.ForeColor = System.Drawing.Color.White;
			this.lblP6.Location = new System.Drawing.Point(17, 174);
			this.lblP6.Name = "lblP6";
			this.lblP6.Size = new System.Drawing.Size(114, 32);
			this.lblP6.TabIndex = 10;
			this.lblP6.Text = "参数6";
			this.lblP6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtP6
			// 
			this.txtP6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.txtP6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtP6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtP6.ForeColor = System.Drawing.Color.White;
			this.txtP6.Location = new System.Drawing.Point(137, 177);
			this.txtP6.Name = "txtP6";
			this.txtP6.Size = new System.Drawing.Size(156, 30);
			this.txtP6.TabIndex = 11;
			// 
			// cmbMode
			// 
			this.cmbMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.cmbMode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmbMode.ForeColor = System.Drawing.Color.White;
			this.cmbMode.Location = new System.Drawing.Point(137, 209);
			this.cmbMode.Name = "cmbMode";
			this.cmbMode.Size = new System.Drawing.Size(156, 32);
			this.cmbMode.TabIndex = 12;
			// 
			// lblParamTitle
			// 
			this.lblParamTitle.AutoSize = true;
			this.lblParamTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblParamTitle.ForeColor = System.Drawing.Color.White;
			this.lblParamTitle.Location = new System.Drawing.Point(18, 22);
			this.lblParamTitle.Name = "lblParamTitle";
			this.lblParamTitle.Size = new System.Drawing.Size(101, 30);
			this.lblParamTitle.TabIndex = 0;
			this.lblParamTitle.Text = "通讯设置";
			// 
			// rightLayout
			// 
			this.rightLayout.ColumnCount = 1;
			this.rightLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rightLayout.Controls.Add(this.panelInput, 0, 0);
			this.rightLayout.Controls.Add(this.panelOutput, 0, 1);
			this.rightLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightLayout.Location = new System.Drawing.Point(601, 11);
			this.rightLayout.Name = "rightLayout";
			this.rightLayout.RowCount = 2;
			this.rightLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.rightLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.rightLayout.Size = new System.Drawing.Size(588, 698);
			this.rightLayout.TabIndex = 2;
			// 
			// panelInput
			// 
			this.panelInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelInput.Controls.Add(this.dgvInput);
			this.panelInput.Controls.Add(this.panelInputButtons);
			this.panelInput.Controls.Add(this.lblInputTitle);
			this.panelInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelInput.Location = new System.Drawing.Point(3, 3);
			this.panelInput.Name = "panelInput";
			this.panelInput.Size = new System.Drawing.Size(582, 343);
			this.panelInput.TabIndex = 0;
			// 
			// dgvInput
			// 
			this.dgvInput.AllowUserToAddRows = false;
			this.dgvInput.AllowUserToDeleteRows = false;
			this.dgvInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvInput.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.dgvInput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvInput.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colInputName,
            this.colInputTrigger,
            this.colInputType,
            this.colInputByteOffset,
            this.colInputBitOffset,
            this.colInputLength,
            this.colInputRemark});
			this.dgvInput.Location = new System.Drawing.Point(18, 48);
			this.dgvInput.Name = "dgvInput";
			this.dgvInput.RowHeadersWidth = 62;
			this.dgvInput.RowTemplate.Height = 28;
			this.dgvInput.Size = new System.Drawing.Size(544, 238);
			this.dgvInput.TabIndex = 2;
			// 
			// colInputName
			// 
			this.colInputName.HeaderText = "输入变量名称";
			this.colInputName.MinimumWidth = 8;
			this.colInputName.Name = "colInputName";
			this.colInputName.Width = 130;
			// 
			// colInputTrigger
			// 
			this.colInputTrigger.HeaderText = "作为触发源";
			this.colInputTrigger.MinimumWidth = 8;
			this.colInputTrigger.Name = "colInputTrigger";
			this.colInputTrigger.Width = 90;
			// 
			// colInputType
			// 
			this.colInputType.HeaderText = "类型";
			this.colInputType.MinimumWidth = 8;
			this.colInputType.Name = "colInputType";
			this.colInputType.Width = 90;
			// 
			// colInputByteOffset
			// 
			this.colInputByteOffset.HeaderText = "偏移字节";
			this.colInputByteOffset.MinimumWidth = 8;
			this.colInputByteOffset.Name = "colInputByteOffset";
			this.colInputByteOffset.Width = 80;
			// 
			// colInputBitOffset
			// 
			this.colInputBitOffset.HeaderText = "Bit";
			this.colInputBitOffset.MinimumWidth = 8;
			this.colInputBitOffset.Name = "colInputBitOffset";
			this.colInputBitOffset.Width = 50;
			// 
			// colInputLength
			// 
			this.colInputLength.HeaderText = "长度";
			this.colInputLength.MinimumWidth = 8;
			this.colInputLength.Name = "colInputLength";
			this.colInputLength.Width = 70;
			// 
			// colInputRemark
			// 
			this.colInputRemark.HeaderText = "备注";
			this.colInputRemark.MinimumWidth = 8;
			this.colInputRemark.Name = "colInputRemark";
			this.colInputRemark.Width = 180;
			// 
			// panelInputButtons
			// 
			this.panelInputButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelInputButtons.Controls.Add(this.btnDeleteInput);
			this.panelInputButtons.Controls.Add(this.btnAddInput);
			this.panelInputButtons.Location = new System.Drawing.Point(18, 294);
			this.panelInputButtons.Name = "panelInputButtons";
			this.panelInputButtons.Size = new System.Drawing.Size(544, 38);
			this.panelInputButtons.TabIndex = 1;
			// 
			// btnDeleteInput
			// 
			this.btnDeleteInput.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnDeleteInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDeleteInput.ForeColor = System.Drawing.Color.White;
			this.btnDeleteInput.Location = new System.Drawing.Point(132, 4);
			this.btnDeleteInput.Name = "btnDeleteInput";
			this.btnDeleteInput.Size = new System.Drawing.Size(110, 30);
			this.btnDeleteInput.TabIndex = 1;
			this.btnDeleteInput.Text = "删除选中";
			this.btnDeleteInput.UseVisualStyleBackColor = true;
			this.btnDeleteInput.Click += new System.EventHandler(this.btnDeleteInput_Click);
			// 
			// btnAddInput
			// 
			this.btnAddInput.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnAddInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddInput.ForeColor = System.Drawing.Color.White;
			this.btnAddInput.Location = new System.Drawing.Point(0, 4);
			this.btnAddInput.Name = "btnAddInput";
			this.btnAddInput.Size = new System.Drawing.Size(110, 30);
			this.btnAddInput.TabIndex = 0;
			this.btnAddInput.Text = "+ 新增输入";
			this.btnAddInput.UseVisualStyleBackColor = true;
			this.btnAddInput.Click += new System.EventHandler(this.btnAddInput_Click);
			// 
			// lblInputTitle
			// 
			this.lblInputTitle.AutoSize = true;
			this.lblInputTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblInputTitle.ForeColor = System.Drawing.Color.White;
			this.lblInputTitle.Location = new System.Drawing.Point(18, 18);
			this.lblInputTitle.Name = "lblInputTitle";
			this.lblInputTitle.Size = new System.Drawing.Size(101, 30);
			this.lblInputTitle.TabIndex = 0;
			this.lblInputTitle.Text = "输入参数";
			// 
			// panelOutput
			// 
			this.panelOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelOutput.Controls.Add(this.dgvOutput);
			this.panelOutput.Controls.Add(this.panelOutputButtons);
			this.panelOutput.Controls.Add(this.lblOutputTitle);
			this.panelOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelOutput.Location = new System.Drawing.Point(3, 352);
			this.panelOutput.Name = "panelOutput";
			this.panelOutput.Size = new System.Drawing.Size(582, 343);
			this.panelOutput.TabIndex = 1;
			// 
			// dgvOutput
			// 
			this.dgvOutput.AllowUserToAddRows = false;
			this.dgvOutput.AllowUserToDeleteRows = false;
			this.dgvOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvOutput.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.dgvOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvOutput.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colOutputName,
            this.colOutputType,
            this.colOutputByteOffset,
            this.colOutputBitOffset,
            this.colOutputLength,
            this.colOutputRemark});
			this.dgvOutput.Location = new System.Drawing.Point(18, 48);
			this.dgvOutput.Name = "dgvOutput";
			this.dgvOutput.RowHeadersWidth = 62;
			this.dgvOutput.RowTemplate.Height = 28;
			this.dgvOutput.Size = new System.Drawing.Size(544, 238);
			this.dgvOutput.TabIndex = 2;
			// 
			// colOutputName
			// 
			this.colOutputName.HeaderText = "输出变量名称";
			this.colOutputName.MinimumWidth = 8;
			this.colOutputName.Name = "colOutputName";
			this.colOutputName.Width = 130;
			// 
			// colOutputType
			// 
			this.colOutputType.HeaderText = "类型";
			this.colOutputType.MinimumWidth = 8;
			this.colOutputType.Name = "colOutputType";
			this.colOutputType.Width = 90;
			// 
			// colOutputByteOffset
			// 
			this.colOutputByteOffset.HeaderText = "偏移字节";
			this.colOutputByteOffset.MinimumWidth = 8;
			this.colOutputByteOffset.Name = "colOutputByteOffset";
			this.colOutputByteOffset.Width = 80;
			// 
			// colOutputBitOffset
			// 
			this.colOutputBitOffset.HeaderText = "Bit";
			this.colOutputBitOffset.MinimumWidth = 8;
			this.colOutputBitOffset.Name = "colOutputBitOffset";
			this.colOutputBitOffset.Width = 50;
			// 
			// colOutputLength
			// 
			this.colOutputLength.HeaderText = "长度";
			this.colOutputLength.MinimumWidth = 8;
			this.colOutputLength.Name = "colOutputLength";
			this.colOutputLength.Width = 70;
			// 
			// colOutputRemark
			// 
			this.colOutputRemark.HeaderText = "备注";
			this.colOutputRemark.MinimumWidth = 8;
			this.colOutputRemark.Name = "colOutputRemark";
			this.colOutputRemark.Width = 180;
			// 
			// panelOutputButtons
			// 
			this.panelOutputButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelOutputButtons.Controls.Add(this.btnSave);
			this.panelOutputButtons.Controls.Add(this.btnDeleteOutput);
			this.panelOutputButtons.Controls.Add(this.btnAddOutput);
			this.panelOutputButtons.Location = new System.Drawing.Point(18, 294);
			this.panelOutputButtons.Name = "panelOutputButtons";
			this.panelOutputButtons.Size = new System.Drawing.Size(544, 38);
			this.panelOutputButtons.TabIndex = 1;
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(220)))));
			this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSave.ForeColor = System.Drawing.Color.White;
			this.btnSave.Location = new System.Drawing.Point(414, 4);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(110, 30);
			this.btnSave.TabIndex = 2;
			this.btnSave.Text = "保存";
			this.btnSave.UseVisualStyleBackColor = false;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnDeleteOutput
			// 
			this.btnDeleteOutput.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnDeleteOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDeleteOutput.ForeColor = System.Drawing.Color.White;
			this.btnDeleteOutput.Location = new System.Drawing.Point(132, 4);
			this.btnDeleteOutput.Name = "btnDeleteOutput";
			this.btnDeleteOutput.Size = new System.Drawing.Size(110, 30);
			this.btnDeleteOutput.TabIndex = 1;
			this.btnDeleteOutput.Text = "删除选中";
			this.btnDeleteOutput.UseVisualStyleBackColor = true;
			this.btnDeleteOutput.Click += new System.EventHandler(this.btnDeleteOutput_Click);
			// 
			// btnAddOutput
			// 
			this.btnAddOutput.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnAddOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddOutput.ForeColor = System.Drawing.Color.White;
			this.btnAddOutput.Location = new System.Drawing.Point(0, 4);
			this.btnAddOutput.Name = "btnAddOutput";
			this.btnAddOutput.Size = new System.Drawing.Size(110, 30);
			this.btnAddOutput.TabIndex = 0;
			this.btnAddOutput.Text = "+ 新增输出";
			this.btnAddOutput.UseVisualStyleBackColor = true;
			this.btnAddOutput.Click += new System.EventHandler(this.btnAddOutput_Click);
			// 
			// lblOutputTitle
			// 
			this.lblOutputTitle.AutoSize = true;
			this.lblOutputTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblOutputTitle.ForeColor = System.Drawing.Color.White;
			this.lblOutputTitle.Location = new System.Drawing.Point(18, 18);
			this.lblOutputTitle.Name = "lblOutputTitle";
			this.lblOutputTitle.Size = new System.Drawing.Size(101, 30);
			this.lblOutputTitle.TabIndex = 0;
			this.lblOutputTitle.Text = "输出参数";
			// 
			// CommunicationConfigControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.Controls.Add(this.mainLayout);
			this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.Name = "CommunicationConfigControl";
			this.Size = new System.Drawing.Size(1200, 720);
			this.mainLayout.ResumeLayout(false);
			this.panelType.ResumeLayout(false);
			this.panelType.PerformLayout();
			this.panelParams.ResumeLayout(false);
			this.panelParams.PerformLayout();
			this.grpTest.ResumeLayout(false);
			this.grpTest.PerformLayout();
			this.grpParams.ResumeLayout(false);
			this.paramLayout.ResumeLayout(false);
			this.paramLayout.PerformLayout();
			this.rightLayout.ResumeLayout(false);
			this.panelInput.ResumeLayout(false);
			this.panelInput.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvInput)).EndInit();
			this.panelInputButtons.ResumeLayout(false);
			this.panelOutput.ResumeLayout(false);
			this.panelOutput.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvOutput)).EndInit();
			this.panelOutputButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel mainLayout;
		private System.Windows.Forms.Panel panelType;
		private System.Windows.Forms.Button btnS7;
		private System.Windows.Forms.Button btnProfinet;
		private System.Windows.Forms.Button btnTcpIp;
		private System.Windows.Forms.Label lblTypeTitle;
		private System.Windows.Forms.Panel panelParams;
		private System.Windows.Forms.GroupBox grpTest;
		private System.Windows.Forms.Button btnClearTest;
		private System.Windows.Forms.Button btnSendTest;
		private System.Windows.Forms.TextBox txtReceive;
		private System.Windows.Forms.Label lblReceive;
		private System.Windows.Forms.TextBox txtSend;
		private System.Windows.Forms.Label lblSend;
		private System.Windows.Forms.GroupBox grpParams;
		private System.Windows.Forms.TableLayoutPanel paramLayout;
		private System.Windows.Forms.Label lblP1;
		private System.Windows.Forms.TextBox txtP1;
		private System.Windows.Forms.Label lblP2;
		private System.Windows.Forms.TextBox txtP2;
		private System.Windows.Forms.Label lblP3;
		private System.Windows.Forms.TextBox txtP3;
		private System.Windows.Forms.Label lblP4;
		private System.Windows.Forms.TextBox txtP4;
		private System.Windows.Forms.Label lblP5;
		private System.Windows.Forms.TextBox txtP5;
		private System.Windows.Forms.Label lblP6;
		private System.Windows.Forms.TextBox txtP6;
		private System.Windows.Forms.ComboBox cmbMode;
		private System.Windows.Forms.Label lblParamTitle;
		private System.Windows.Forms.TableLayoutPanel rightLayout;
		private System.Windows.Forms.Panel panelInput;
		private System.Windows.Forms.DataGridView dgvInput;
		private System.Windows.Forms.DataGridViewTextBoxColumn colInputName;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colInputTrigger;
		private System.Windows.Forms.DataGridViewComboBoxColumn colInputType;
		private System.Windows.Forms.DataGridViewTextBoxColumn colInputByteOffset;
		private System.Windows.Forms.DataGridViewTextBoxColumn colInputBitOffset;
		private System.Windows.Forms.DataGridViewTextBoxColumn colInputLength;
		private System.Windows.Forms.DataGridViewTextBoxColumn colInputRemark;
		private System.Windows.Forms.Panel panelInputButtons;
		private System.Windows.Forms.Button btnDeleteInput;
		private System.Windows.Forms.Button btnAddInput;
		private System.Windows.Forms.Label lblInputTitle;
		private System.Windows.Forms.Panel panelOutput;
		private System.Windows.Forms.DataGridView dgvOutput;
		private System.Windows.Forms.DataGridViewTextBoxColumn colOutputName;
		private System.Windows.Forms.DataGridViewComboBoxColumn colOutputType;
		private System.Windows.Forms.DataGridViewTextBoxColumn colOutputByteOffset;
		private System.Windows.Forms.DataGridViewTextBoxColumn colOutputBitOffset;
		private System.Windows.Forms.DataGridViewTextBoxColumn colOutputLength;
		private System.Windows.Forms.DataGridViewTextBoxColumn colOutputRemark;
		private System.Windows.Forms.Panel panelOutputButtons;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnDeleteOutput;
		private System.Windows.Forms.Button btnAddOutput;
		private System.Windows.Forms.Label lblOutputTitle;
	}
}
