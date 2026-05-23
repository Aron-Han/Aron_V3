namespace Aron_V3
{
	partial class CSharpScriptStepEditorControl
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
			this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
			this.topPanel = new System.Windows.Forms.Panel();
			this.topLayout = new System.Windows.Forms.TableLayoutPanel();
			this.lblStepName = new System.Windows.Forms.Label();
			this.txtStepName = new System.Windows.Forms.TextBox();
			this.chkEnable = new System.Windows.Forms.CheckBox();
			this.lblStatusTitle = new System.Windows.Forms.Label();
			this.statusPanel = new System.Windows.Forms.Panel();
			this.lblStatusLight = new System.Windows.Forms.Label();
			this.lblStatusText = new System.Windows.Forms.Label();
			this.btnReferenceDll = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnCompile = new System.Windows.Forms.Button();
			this.btnRun = new System.Windows.Forms.Button();
			this.mainSplit = new System.Windows.Forms.SplitContainer();
			this.leftSplit = new System.Windows.Forms.SplitContainer();
			this.pinHostPanel = new System.Windows.Forms.Panel();
			this.pinContentPanel = new System.Windows.Forms.Panel();
			this.inputPanel = new System.Windows.Forms.Panel();
			this.gridInputs = new System.Windows.Forms.DataGridView();
			this.outputPanel = new System.Windows.Forms.Panel();
			this.gridOutputs = new System.Windows.Forms.DataGridView();
			this.pinToolPanel = new System.Windows.Forms.Panel();
			this.btnShowInputs = new System.Windows.Forms.Button();
			this.btnShowOutputs = new System.Windows.Forms.Button();
			this.lblInputTitle = new System.Windows.Forms.Label();
			this.lblOutputTitle = new System.Windows.Forms.Label();
			this.btnInputAdd = new System.Windows.Forms.Button();
			this.btnInputDelete = new System.Windows.Forms.Button();
			this.btnOutputAdd = new System.Windows.Forms.Button();
			this.btnOutputDelete = new System.Windows.Forms.Button();
			this.codePanel = new System.Windows.Forms.Panel();
			this.codeEditorHost = new System.Windows.Forms.Panel();
			this.txtCode = new System.Windows.Forms.RichTextBox();
			this.panelLineNumbers = new System.Windows.Forms.Panel();
			this.lblCodeTitle = new System.Windows.Forms.Label();
			this.logPanel = new System.Windows.Forms.Panel();
			this.gridLogs = new System.Windows.Forms.DataGridView();
			this.lblLogTitle = new System.Windows.Forms.Label();
			this.lblScriptFile = new System.Windows.Forms.Label();
			this.txtScriptPath = new System.Windows.Forms.TextBox();
			this.btnBrowseScript = new System.Windows.Forms.Button();
			this.rootLayout.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.topLayout.SuspendLayout();
			this.statusPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplit)).BeginInit();
			this.mainSplit.Panel1.SuspendLayout();
			this.mainSplit.Panel2.SuspendLayout();
			this.mainSplit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.leftSplit)).BeginInit();
			this.leftSplit.Panel1.SuspendLayout();
			this.leftSplit.SuspendLayout();
			this.pinHostPanel.SuspendLayout();
			this.pinContentPanel.SuspendLayout();
			this.inputPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridInputs)).BeginInit();
			this.outputPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridOutputs)).BeginInit();
			this.pinToolPanel.SuspendLayout();
			this.codePanel.SuspendLayout();
			this.codeEditorHost.SuspendLayout();
			this.logPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridLogs)).BeginInit();
			this.SuspendLayout();
			// 
			// rootLayout
			// 
			this.rootLayout.ColumnCount = 1;
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Controls.Add(this.topPanel, 0, 0);
			this.rootLayout.Controls.Add(this.mainSplit, 0, 1);
			this.rootLayout.Controls.Add(this.logPanel, 0, 2);
			this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rootLayout.Location = new System.Drawing.Point(0, 0);
			this.rootLayout.Margin = new System.Windows.Forms.Padding(4);
			this.rootLayout.Name = "rootLayout";
			this.rootLayout.Padding = new System.Windows.Forms.Padding(12);
			this.rootLayout.RowCount = 3;
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 190F));
			this.rootLayout.Size = new System.Drawing.Size(1650, 1080);
			this.rootLayout.TabIndex = 0;
			// 
			// topPanel
			// 
			this.topPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.topPanel.Controls.Add(this.topLayout);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.Location = new System.Drawing.Point(12, 12);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(10);
			this.topPanel.Size = new System.Drawing.Size(1626, 62);
			this.topPanel.TabIndex = 0;
			// 
			// topLayout
			// 
			this.topLayout.ColumnCount = 10;
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 330F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
			this.topLayout.Controls.Add(this.lblStepName, 0, 0);
			this.topLayout.Controls.Add(this.txtStepName, 1, 0);
			this.topLayout.Controls.Add(this.chkEnable, 2, 0);
			this.topLayout.Controls.Add(this.lblStatusTitle, 3, 0);
			this.topLayout.Controls.Add(this.statusPanel, 4, 0);
			this.topLayout.Controls.Add(this.btnReferenceDll, 6, 0);
			this.topLayout.Controls.Add(this.btnSave, 7, 0);
			this.topLayout.Controls.Add(this.btnCompile, 8, 0);
			this.topLayout.Controls.Add(this.btnRun, 9, 0);
			this.topLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topLayout.Location = new System.Drawing.Point(10, 10);
			this.topLayout.Margin = new System.Windows.Forms.Padding(0);
			this.topLayout.Name = "topLayout";
			this.topLayout.RowCount = 1;
			this.topLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.topLayout.Size = new System.Drawing.Size(1604, 40);
			this.topLayout.TabIndex = 0;
			// 
			// lblStepName
			// 
			this.lblStepName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStepName.Location = new System.Drawing.Point(4, 0);
			this.lblStepName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStepName.Name = "lblStepName";
			this.lblStepName.Size = new System.Drawing.Size(82, 40);
			this.lblStepName.TabIndex = 0;
			this.lblStepName.Text = "当前脚本";
			this.lblStepName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtStepName
			// 
			this.txtStepName.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtStepName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtStepName.Location = new System.Drawing.Point(96, 10);
			this.txtStepName.Margin = new System.Windows.Forms.Padding(6, 10, 6, 4);
			this.txtStepName.Name = "txtStepName";
			this.txtStepName.ReadOnly = true;
			this.txtStepName.Size = new System.Drawing.Size(318, 21);
			this.txtStepName.TabIndex = 1;
			// 
			// chkEnable
			// 
			this.chkEnable.Checked = true;
			this.chkEnable.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkEnable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkEnable.Location = new System.Drawing.Point(424, 4);
			this.chkEnable.Margin = new System.Windows.Forms.Padding(4);
			this.chkEnable.Name = "chkEnable";
			this.chkEnable.Size = new System.Drawing.Size(1, 32);
			this.chkEnable.TabIndex = 2;
			this.chkEnable.Text = "启用";
			this.chkEnable.Visible = false;
			// 
			// lblStatusTitle
			// 
			this.lblStatusTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStatusTitle.Location = new System.Drawing.Point(424, 0);
			this.lblStatusTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatusTitle.Name = "lblStatusTitle";
			this.lblStatusTitle.Size = new System.Drawing.Size(52, 40);
			this.lblStatusTitle.TabIndex = 3;
			this.lblStatusTitle.Text = "状态";
			this.lblStatusTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// statusPanel
			// 
			this.statusPanel.Controls.Add(this.lblStatusLight);
			this.statusPanel.Controls.Add(this.lblStatusText);
			this.statusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusPanel.Location = new System.Drawing.Point(484, 4);
			this.statusPanel.Margin = new System.Windows.Forms.Padding(4);
			this.statusPanel.Name = "statusPanel";
			this.statusPanel.Size = new System.Drawing.Size(152, 32);
			this.statusPanel.TabIndex = 4;
			// 
			// lblStatusLight
			// 
			this.lblStatusLight.Location = new System.Drawing.Point(8, 9);
			this.lblStatusLight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatusLight.Name = "lblStatusLight";
			this.lblStatusLight.Size = new System.Drawing.Size(16, 16);
			this.lblStatusLight.TabIndex = 0;
			// 
			// lblStatusText
			// 
			this.lblStatusText.Location = new System.Drawing.Point(34, 4);
			this.lblStatusText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatusText.Name = "lblStatusText";
			this.lblStatusText.Size = new System.Drawing.Size(110, 26);
			this.lblStatusText.TabIndex = 1;
			this.lblStatusText.Text = "Ready";
			this.lblStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnReferenceDll
			// 
			this.btnReferenceDll.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnReferenceDll.Location = new System.Drawing.Point(1148, 4);
			this.btnReferenceDll.Margin = new System.Windows.Forms.Padding(4);
			this.btnReferenceDll.Name = "btnReferenceDll";
			this.btnReferenceDll.Size = new System.Drawing.Size(122, 32);
			this.btnReferenceDll.TabIndex = 8;
			this.btnReferenceDll.Text = "引用信息";
			// 
			// btnSave
			// 
			this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnSave.Location = new System.Drawing.Point(1278, 4);
			this.btnSave.Margin = new System.Windows.Forms.Padding(4);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(92, 32);
			this.btnSave.TabIndex = 9;
			this.btnSave.Text = "保存";
			// 
			// btnCompile
			// 
			this.btnCompile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnCompile.Location = new System.Drawing.Point(1378, 4);
			this.btnCompile.Margin = new System.Windows.Forms.Padding(4);
			this.btnCompile.Name = "btnCompile";
			this.btnCompile.Size = new System.Drawing.Size(92, 32);
			this.btnCompile.TabIndex = 10;
			this.btnCompile.Text = "编译";
			// 
			// btnRun
			// 
			this.btnRun.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnRun.Location = new System.Drawing.Point(1478, 4);
			this.btnRun.Margin = new System.Windows.Forms.Padding(4);
			this.btnRun.Name = "btnRun";
			this.btnRun.Size = new System.Drawing.Size(122, 32);
			this.btnRun.TabIndex = 11;
			this.btnRun.Text = "调试运行";
			// 
			// mainSplit
			// 
			this.mainSplit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplit.Location = new System.Drawing.Point(12, 84);
			this.mainSplit.Margin = new System.Windows.Forms.Padding(0);
			this.mainSplit.Name = "mainSplit";
			// 
			// mainSplit.Panel1
			// 
			this.mainSplit.Panel1.Controls.Add(this.leftSplit);
			// 
			// mainSplit.Panel2
			// 
			this.mainSplit.Panel2.Controls.Add(this.codePanel);
			this.mainSplit.Size = new System.Drawing.Size(1626, 794);
			this.mainSplit.SplitterDistance = 540;
			this.mainSplit.SplitterWidth = 8;
			this.mainSplit.TabIndex = 1;
			// 
			// leftSplit
			// 
			this.leftSplit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.leftSplit.Location = new System.Drawing.Point(0, 0);
			this.leftSplit.Margin = new System.Windows.Forms.Padding(0);
			this.leftSplit.Name = "leftSplit";
			this.leftSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// leftSplit.Panel1
			// 
			this.leftSplit.Panel1.Controls.Add(this.pinHostPanel);
			this.leftSplit.Panel2Collapsed = true;
			this.leftSplit.Size = new System.Drawing.Size(540, 794);
			this.leftSplit.SplitterDistance = 769;
			this.leftSplit.SplitterWidth = 1;
			this.leftSplit.TabIndex = 0;
			// 
			// pinHostPanel
			// 
			this.pinHostPanel.Controls.Add(this.pinContentPanel);
			this.pinHostPanel.Controls.Add(this.pinToolPanel);
			this.pinHostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pinHostPanel.Location = new System.Drawing.Point(0, 0);
			this.pinHostPanel.Margin = new System.Windows.Forms.Padding(0);
			this.pinHostPanel.Name = "pinHostPanel";
			this.pinHostPanel.Padding = new System.Windows.Forms.Padding(8);
			this.pinHostPanel.Size = new System.Drawing.Size(540, 794);
			this.pinHostPanel.TabIndex = 0;
			// 
			// pinContentPanel
			// 
			this.pinContentPanel.Controls.Add(this.inputPanel);
			this.pinContentPanel.Controls.Add(this.outputPanel);
			this.pinContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pinContentPanel.Location = new System.Drawing.Point(8, 54);
			this.pinContentPanel.Margin = new System.Windows.Forms.Padding(0);
			this.pinContentPanel.Name = "pinContentPanel";
			this.pinContentPanel.Size = new System.Drawing.Size(524, 732);
			this.pinContentPanel.TabIndex = 1;
			// 
			// inputPanel
			// 
			this.inputPanel.Controls.Add(this.gridInputs);
			this.inputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.inputPanel.Location = new System.Drawing.Point(0, 0);
			this.inputPanel.Margin = new System.Windows.Forms.Padding(0);
			this.inputPanel.Name = "inputPanel";
			this.inputPanel.Size = new System.Drawing.Size(524, 732);
			this.inputPanel.TabIndex = 0;
			// 
			// gridInputs
			// 
			this.gridInputs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridInputs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridInputs.Location = new System.Drawing.Point(0, 0);
			this.gridInputs.Margin = new System.Windows.Forms.Padding(0);
			this.gridInputs.Name = "gridInputs";
			this.gridInputs.RowHeadersWidth = 62;
			this.gridInputs.Size = new System.Drawing.Size(524, 732);
			this.gridInputs.TabIndex = 0;
			// 
			// outputPanel
			// 
			this.outputPanel.Controls.Add(this.gridOutputs);
			this.outputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outputPanel.Location = new System.Drawing.Point(0, 0);
			this.outputPanel.Margin = new System.Windows.Forms.Padding(0);
			this.outputPanel.Name = "outputPanel";
			this.outputPanel.Size = new System.Drawing.Size(524, 732);
			this.outputPanel.TabIndex = 1;
			this.outputPanel.Visible = false;
			// 
			// gridOutputs
			// 
			this.gridOutputs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridOutputs.Location = new System.Drawing.Point(0, 0);
			this.gridOutputs.Margin = new System.Windows.Forms.Padding(0);
			this.gridOutputs.Name = "gridOutputs";
			this.gridOutputs.RowHeadersWidth = 62;
			this.gridOutputs.Size = new System.Drawing.Size(524, 732);
			this.gridOutputs.TabIndex = 0;
			// 
			// pinToolPanel
			// 
			this.pinToolPanel.Controls.Add(this.btnShowInputs);
			this.pinToolPanel.Controls.Add(this.btnShowOutputs);
			this.pinToolPanel.Controls.Add(this.lblInputTitle);
			this.pinToolPanel.Controls.Add(this.lblOutputTitle);
			this.pinToolPanel.Controls.Add(this.btnInputAdd);
			this.pinToolPanel.Controls.Add(this.btnInputDelete);
			this.pinToolPanel.Controls.Add(this.btnOutputAdd);
			this.pinToolPanel.Controls.Add(this.btnOutputDelete);
			this.pinToolPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.pinToolPanel.Location = new System.Drawing.Point(8, 8);
			this.pinToolPanel.Margin = new System.Windows.Forms.Padding(0);
			this.pinToolPanel.Name = "pinToolPanel";
			this.pinToolPanel.Size = new System.Drawing.Size(524, 46);
			this.pinToolPanel.TabIndex = 0;
			// 
			// btnShowInputs
			// 
			this.btnShowInputs.Location = new System.Drawing.Point(0, 6);
			this.btnShowInputs.Margin = new System.Windows.Forms.Padding(0);
			this.btnShowInputs.Name = "btnShowInputs";
			this.btnShowInputs.Size = new System.Drawing.Size(88, 32);
			this.btnShowInputs.TabIndex = 0;
			this.btnShowInputs.Text = "输入";
			// 
			// btnShowOutputs
			// 
			this.btnShowOutputs.Location = new System.Drawing.Point(92, 6);
			this.btnShowOutputs.Margin = new System.Windows.Forms.Padding(0);
			this.btnShowOutputs.Name = "btnShowOutputs";
			this.btnShowOutputs.Size = new System.Drawing.Size(88, 32);
			this.btnShowOutputs.TabIndex = 1;
			this.btnShowOutputs.Text = "输出";
			// 
			// lblInputTitle
			// 
			this.lblInputTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblInputTitle.Location = new System.Drawing.Point(190, 8);
			this.lblInputTitle.Margin = new System.Windows.Forms.Padding(0);
			this.lblInputTitle.Name = "lblInputTitle";
			this.lblInputTitle.Size = new System.Drawing.Size(190, 28);
			this.lblInputTitle.TabIndex = 2;
			this.lblInputTitle.Text = "输入定义 Inputs";
			this.lblInputTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblOutputTitle
			// 
			this.lblOutputTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblOutputTitle.Location = new System.Drawing.Point(190, 8);
			this.lblOutputTitle.Margin = new System.Windows.Forms.Padding(0);
			this.lblOutputTitle.Name = "lblOutputTitle";
			this.lblOutputTitle.Size = new System.Drawing.Size(190, 28);
			this.lblOutputTitle.TabIndex = 3;
			this.lblOutputTitle.Text = "输出定义 Outputs";
			this.lblOutputTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblOutputTitle.Visible = false;
			// 
			// btnInputAdd
			// 
			this.btnInputAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnInputAdd.Location = new System.Drawing.Point(442, 6);
			this.btnInputAdd.Margin = new System.Windows.Forms.Padding(0);
			this.btnInputAdd.Name = "btnInputAdd";
			this.btnInputAdd.Size = new System.Drawing.Size(38, 32);
			this.btnInputAdd.TabIndex = 4;
			this.btnInputAdd.Text = "+";
			// 
			// btnInputDelete
			// 
			this.btnInputDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnInputDelete.Location = new System.Drawing.Point(486, 6);
			this.btnInputDelete.Margin = new System.Windows.Forms.Padding(0);
			this.btnInputDelete.Name = "btnInputDelete";
			this.btnInputDelete.Size = new System.Drawing.Size(38, 32);
			this.btnInputDelete.TabIndex = 5;
			this.btnInputDelete.Text = "-";
			// 
			// btnOutputAdd
			// 
			this.btnOutputAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOutputAdd.Location = new System.Drawing.Point(442, 6);
			this.btnOutputAdd.Margin = new System.Windows.Forms.Padding(0);
			this.btnOutputAdd.Name = "btnOutputAdd";
			this.btnOutputAdd.Size = new System.Drawing.Size(38, 32);
			this.btnOutputAdd.TabIndex = 6;
			this.btnOutputAdd.Text = "+";
			this.btnOutputAdd.Visible = false;
			// 
			// btnOutputDelete
			// 
			this.btnOutputDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOutputDelete.Location = new System.Drawing.Point(486, 6);
			this.btnOutputDelete.Margin = new System.Windows.Forms.Padding(0);
			this.btnOutputDelete.Name = "btnOutputDelete";
			this.btnOutputDelete.Size = new System.Drawing.Size(38, 32);
			this.btnOutputDelete.TabIndex = 7;
			this.btnOutputDelete.Text = "-";
			this.btnOutputDelete.Visible = false;
			// 
			// codePanel
			// 
			this.codePanel.Controls.Add(this.codeEditorHost);
			this.codePanel.Controls.Add(this.lblCodeTitle);
			this.codePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.codePanel.Location = new System.Drawing.Point(0, 0);
			this.codePanel.Margin = new System.Windows.Forms.Padding(4);
			this.codePanel.Name = "codePanel";
			this.codePanel.Padding = new System.Windows.Forms.Padding(0, 38, 0, 0);
			this.codePanel.Size = new System.Drawing.Size(1078, 794);
			this.codePanel.TabIndex = 0;
			// 
			// codeEditorHost
			// 
			this.codeEditorHost.Controls.Add(this.txtCode);
			this.codeEditorHost.Controls.Add(this.panelLineNumbers);
			this.codeEditorHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.codeEditorHost.Location = new System.Drawing.Point(0, 76);
			this.codeEditorHost.Margin = new System.Windows.Forms.Padding(4);
			this.codeEditorHost.Name = "codeEditorHost";
			this.codeEditorHost.Size = new System.Drawing.Size(1078, 718);
			this.codeEditorHost.TabIndex = 0;
			// 
			// txtCode
			// 
			this.txtCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtCode.Location = new System.Drawing.Point(54, 0);
			this.txtCode.Margin = new System.Windows.Forms.Padding(4);
			this.txtCode.Name = "txtCode";
			this.txtCode.Size = new System.Drawing.Size(1024, 718);
			this.txtCode.TabIndex = 0;
			this.txtCode.Text = "";
			// 
			// panelLineNumbers
			// 
			this.panelLineNumbers.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelLineNumbers.Location = new System.Drawing.Point(0, 0);
			this.panelLineNumbers.Margin = new System.Windows.Forms.Padding(4);
			this.panelLineNumbers.Name = "panelLineNumbers";
			this.panelLineNumbers.Size = new System.Drawing.Size(54, 718);
			this.panelLineNumbers.TabIndex = 1;
			// 
			// lblCodeTitle
			// 
			this.lblCodeTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblCodeTitle.Location = new System.Drawing.Point(0, 38);
			this.lblCodeTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblCodeTitle.Name = "lblCodeTitle";
			this.lblCodeTitle.Size = new System.Drawing.Size(1078, 38);
			this.lblCodeTitle.TabIndex = 1;
			this.lblCodeTitle.Text = "C# Script Code";
			this.lblCodeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// logPanel
			// 
			this.logPanel.Controls.Add(this.gridLogs);
			this.logPanel.Controls.Add(this.lblLogTitle);
			this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logPanel.Location = new System.Drawing.Point(12, 888);
			this.logPanel.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.logPanel.Name = "logPanel";
			this.logPanel.Padding = new System.Windows.Forms.Padding(0, 38, 0, 0);
			this.logPanel.Size = new System.Drawing.Size(1626, 180);
			this.logPanel.TabIndex = 2;
			// 
			// gridLogs
			// 
			this.gridLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridLogs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridLogs.Location = new System.Drawing.Point(0, 76);
			this.gridLogs.Margin = new System.Windows.Forms.Padding(4);
			this.gridLogs.Name = "gridLogs";
			this.gridLogs.RowHeadersWidth = 62;
			this.gridLogs.Size = new System.Drawing.Size(1626, 104);
			this.gridLogs.TabIndex = 0;
			// 
			// lblLogTitle
			// 
			this.lblLogTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblLogTitle.Location = new System.Drawing.Point(0, 38);
			this.lblLogTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLogTitle.Name = "lblLogTitle";
			this.lblLogTitle.Size = new System.Drawing.Size(1626, 38);
			this.lblLogTitle.TabIndex = 1;
			this.lblLogTitle.Text = "编译 / 运行日志";
			this.lblLogTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblScriptFile
			// 
			this.lblScriptFile.Location = new System.Drawing.Point(0, 0);
			this.lblScriptFile.Name = "lblScriptFile";
			this.lblScriptFile.Size = new System.Drawing.Size(100, 23);
			this.lblScriptFile.TabIndex = 0;
			this.lblScriptFile.Visible = false;
			// 
			// txtScriptPath
			// 
			this.txtScriptPath.Location = new System.Drawing.Point(0, 0);
			this.txtScriptPath.Name = "txtScriptPath";
			this.txtScriptPath.Size = new System.Drawing.Size(100, 28);
			this.txtScriptPath.TabIndex = 0;
			this.txtScriptPath.Visible = false;
			// 
			// btnBrowseScript
			// 
			this.btnBrowseScript.Location = new System.Drawing.Point(0, 0);
			this.btnBrowseScript.Name = "btnBrowseScript";
			this.btnBrowseScript.Size = new System.Drawing.Size(75, 23);
			this.btnBrowseScript.TabIndex = 0;
			this.btnBrowseScript.Visible = false;
			// 
			// CSharpScriptStepEditorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.rootLayout);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "CSharpScriptStepEditorControl";
			this.Size = new System.Drawing.Size(1650, 1080);
			this.rootLayout.ResumeLayout(false);
			this.topPanel.ResumeLayout(false);
			this.topLayout.ResumeLayout(false);
			this.topLayout.PerformLayout();
			this.statusPanel.ResumeLayout(false);
			this.mainSplit.Panel1.ResumeLayout(false);
			this.mainSplit.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplit)).EndInit();
			this.mainSplit.ResumeLayout(false);
			this.leftSplit.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.leftSplit)).EndInit();
			this.leftSplit.ResumeLayout(false);
			this.pinHostPanel.ResumeLayout(false);
			this.pinContentPanel.ResumeLayout(false);
			this.inputPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridInputs)).EndInit();
			this.outputPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridOutputs)).EndInit();
			this.pinToolPanel.ResumeLayout(false);
			this.codePanel.ResumeLayout(false);
			this.codeEditorHost.ResumeLayout(false);
			this.logPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridLogs)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel rootLayout;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.TableLayoutPanel topLayout;
		private System.Windows.Forms.Label lblStepName;
		private System.Windows.Forms.TextBox txtStepName;
		private System.Windows.Forms.CheckBox chkEnable;
		private System.Windows.Forms.Label lblStatusTitle;
		private System.Windows.Forms.Panel statusPanel;
		private System.Windows.Forms.Label lblStatusLight;
		private System.Windows.Forms.Label lblStatusText;
		private System.Windows.Forms.Label lblScriptFile;
		private System.Windows.Forms.TextBox txtScriptPath;
		private System.Windows.Forms.Button btnBrowseScript;
		private System.Windows.Forms.Button btnReferenceDll;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnCompile;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.SplitContainer mainSplit;
		private System.Windows.Forms.SplitContainer leftSplit;
		private System.Windows.Forms.Panel pinHostPanel;
		private System.Windows.Forms.Panel pinToolPanel;
		private System.Windows.Forms.Panel pinContentPanel;
		private System.Windows.Forms.Button btnShowInputs;
		private System.Windows.Forms.Button btnShowOutputs;
		private System.Windows.Forms.Panel inputPanel;
		private System.Windows.Forms.Label lblInputTitle;
		private System.Windows.Forms.Button btnInputAdd;
		private System.Windows.Forms.Button btnInputDelete;
		private System.Windows.Forms.DataGridView gridInputs;
		private System.Windows.Forms.Panel outputPanel;
		private System.Windows.Forms.Label lblOutputTitle;
		private System.Windows.Forms.Button btnOutputAdd;
		private System.Windows.Forms.Button btnOutputDelete;
		private System.Windows.Forms.DataGridView gridOutputs;
		private System.Windows.Forms.Panel codePanel;
		private System.Windows.Forms.Label lblCodeTitle;
		private System.Windows.Forms.Panel codeEditorHost;
		private System.Windows.Forms.Panel panelLineNumbers;
		private System.Windows.Forms.RichTextBox txtCode;
		private System.Windows.Forms.Panel logPanel;
		private System.Windows.Forms.Label lblLogTitle;
		private System.Windows.Forms.DataGridView gridLogs;
	}
}
