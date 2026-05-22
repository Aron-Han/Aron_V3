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
			this.lblScriptFile = new System.Windows.Forms.Label();
			this.txtScriptPath = new System.Windows.Forms.TextBox();
			this.btnBrowseScript = new System.Windows.Forms.Button();
			this.btnReferenceDll = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnCompile = new System.Windows.Forms.Button();
			this.btnRun = new System.Windows.Forms.Button();
			this.mainSplit = new System.Windows.Forms.SplitContainer();
			this.leftSplit = new System.Windows.Forms.SplitContainer();
			this.inputPanel = new System.Windows.Forms.Panel();
			this.gridInputs = new System.Windows.Forms.DataGridView();
			this.lblInputTitle = new System.Windows.Forms.Label();
			this.btnInputAdd = new System.Windows.Forms.Button();
			this.btnInputDelete = new System.Windows.Forms.Button();
			this.outputPanel = new System.Windows.Forms.Panel();
			this.gridOutputs = new System.Windows.Forms.DataGridView();
			this.lblOutputTitle = new System.Windows.Forms.Label();
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
			this.leftSplit.Panel2.SuspendLayout();
			this.leftSplit.SuspendLayout();
			this.inputPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridInputs)).BeginInit();
			this.outputPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridOutputs)).BeginInit();
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
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 218F));
			this.rootLayout.Size = new System.Drawing.Size(1650, 1080);
			this.rootLayout.TabIndex = 0;
			// 
			// topPanel
			// 
			this.topPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.topPanel.Controls.Add(this.topLayout);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.Location = new System.Drawing.Point(12, 12);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(12);
			this.topPanel.Size = new System.Drawing.Size(1626, 60);
			this.topPanel.TabIndex = 0;
			// 
			// topLayout
			// 
			this.topLayout.ColumnCount = 10;
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 123F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 345F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 102F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 144F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
			this.topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
			this.topLayout.Controls.Add(this.lblStepName, 0, 0);
			this.topLayout.Controls.Add(this.txtStepName, 1, 0);
			this.topLayout.Controls.Add(this.chkEnable, 2, 0);
			this.topLayout.Controls.Add(this.lblStatusTitle, 3, 0);
			this.topLayout.Controls.Add(this.statusPanel, 4, 0);
			this.topLayout.Controls.Add(this.lblScriptFile, 0, 1);
			this.topLayout.Controls.Add(this.txtScriptPath, 1, 1);
			this.topLayout.Controls.Add(this.btnBrowseScript, 5, 1);
			this.topLayout.Controls.Add(this.btnReferenceDll, 6, 0);
			this.topLayout.Controls.Add(this.btnSave, 7, 0);
			this.topLayout.Controls.Add(this.btnCompile, 8, 0);
			this.topLayout.Controls.Add(this.btnRun, 9, 0);
			this.topLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topLayout.Location = new System.Drawing.Point(12, 12);
			this.topLayout.Margin = new System.Windows.Forms.Padding(4);
			this.topLayout.Name = "topLayout";
			this.topLayout.RowCount = 2;
			this.topLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.topLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
			this.topLayout.Size = new System.Drawing.Size(1600, 34);
			this.topLayout.TabIndex = 0;
			// 
			// lblStepName
			// 
			this.lblStepName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStepName.Location = new System.Drawing.Point(4, 0);
			this.lblStepName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStepName.Name = "lblStepName";
			this.lblStepName.Size = new System.Drawing.Size(115, 34);
			this.lblStepName.TabIndex = 0;
			this.lblStepName.Text = "当前脚本";
			this.lblStepName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtStepName
			// 
			this.txtStepName.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtStepName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtStepName.Location = new System.Drawing.Point(129, 4);
			this.txtStepName.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
			this.txtStepName.Name = "txtStepName";
			this.txtStepName.ReadOnly = true;
			this.txtStepName.Size = new System.Drawing.Size(333, 21);
			this.txtStepName.TabIndex = 1;
			// 
			// chkEnable
			// 
			this.chkEnable.Checked = true;
			this.chkEnable.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkEnable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkEnable.Location = new System.Drawing.Point(472, 4);
			this.chkEnable.Margin = new System.Windows.Forms.Padding(4);
			this.chkEnable.Name = "chkEnable";
			this.chkEnable.Size = new System.Drawing.Size(112, 26);
			this.chkEnable.TabIndex = 2;
			this.chkEnable.Text = "启用";
			this.chkEnable.Visible = false;
			// 
			// lblStatusTitle
			// 
			this.lblStatusTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStatusTitle.Location = new System.Drawing.Point(592, 0);
			this.lblStatusTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatusTitle.Name = "lblStatusTitle";
			this.lblStatusTitle.Size = new System.Drawing.Size(94, 34);
			this.lblStatusTitle.TabIndex = 3;
			this.lblStatusTitle.Text = "状态";
			this.lblStatusTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// statusPanel
			// 
			this.topLayout.SetColumnSpan(this.statusPanel, 6);
			this.statusPanel.Controls.Add(this.lblStatusLight);
			this.statusPanel.Controls.Add(this.lblStatusText);
			this.statusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusPanel.Location = new System.Drawing.Point(694, 4);
			this.statusPanel.Margin = new System.Windows.Forms.Padding(4);
			this.statusPanel.Name = "statusPanel";
			this.statusPanel.Size = new System.Drawing.Size(902, 26);
			this.statusPanel.TabIndex = 4;
			// 
			// lblStatusLight
			// 
			this.lblStatusLight.Location = new System.Drawing.Point(4, 9);
			this.lblStatusLight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatusLight.Name = "lblStatusLight";
			this.lblStatusLight.Size = new System.Drawing.Size(18, 18);
			this.lblStatusLight.TabIndex = 0;
			// 
			// lblStatusText
			// 
			this.lblStatusText.Location = new System.Drawing.Point(33, 3);
			this.lblStatusText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatusText.Name = "lblStatusText";
			this.lblStatusText.Size = new System.Drawing.Size(840, 30);
			this.lblStatusText.TabIndex = 1;
			this.lblStatusText.Text = "Ready";
			// 
			// lblScriptFile
			// 
			this.lblScriptFile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblScriptFile.Location = new System.Drawing.Point(694, 34);
			this.lblScriptFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblScriptFile.Name = "lblScriptFile";
			this.lblScriptFile.Size = new System.Drawing.Size(251, 1);
			this.lblScriptFile.TabIndex = 5;
			this.lblScriptFile.Text = "脚本文件";
			this.lblScriptFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblScriptFile.Visible = false;
			// 
			// txtScriptPath
			// 
			this.topLayout.SetColumnSpan(this.txtScriptPath, 4);
			this.txtScriptPath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtScriptPath.Location = new System.Drawing.Point(955, 38);
			this.txtScriptPath.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
			this.txtScriptPath.Name = "txtScriptPath";
			this.txtScriptPath.Size = new System.Drawing.Size(459, 28);
			this.txtScriptPath.TabIndex = 6;
			this.txtScriptPath.Visible = false;
			// 
			// btnBrowseScript
			// 
			this.btnBrowseScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnBrowseScript.Location = new System.Drawing.Point(1424, 38);
			this.btnBrowseScript.Margin = new System.Windows.Forms.Padding(4);
			this.btnBrowseScript.Name = "btnBrowseScript";
			this.btnBrowseScript.Size = new System.Drawing.Size(172, 1);
			this.btnBrowseScript.TabIndex = 7;
			this.btnBrowseScript.Text = "...";
			this.btnBrowseScript.Visible = false;
			// 
			// btnReferenceDll
			// 
			this.btnReferenceDll.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnReferenceDll.Location = new System.Drawing.Point(4, 38);
			this.btnReferenceDll.Margin = new System.Windows.Forms.Padding(4);
			this.btnReferenceDll.Name = "btnReferenceDll";
			this.btnReferenceDll.Size = new System.Drawing.Size(115, 1);
			this.btnReferenceDll.TabIndex = 8;
			this.btnReferenceDll.Text = "导入DLL";
			// 
			// btnSave
			// 
			this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnSave.Location = new System.Drawing.Point(127, 38);
			this.btnSave.Margin = new System.Windows.Forms.Padding(4);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(337, 1);
			this.btnSave.TabIndex = 9;
			this.btnSave.Text = "保存";
			// 
			// btnCompile
			// 
			this.btnCompile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnCompile.Location = new System.Drawing.Point(472, 38);
			this.btnCompile.Margin = new System.Windows.Forms.Padding(4);
			this.btnCompile.Name = "btnCompile";
			this.btnCompile.Size = new System.Drawing.Size(112, 1);
			this.btnCompile.TabIndex = 10;
			this.btnCompile.Text = "编译";
			// 
			// btnRun
			// 
			this.btnRun.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnRun.Location = new System.Drawing.Point(592, 38);
			this.btnRun.Margin = new System.Windows.Forms.Padding(4);
			this.btnRun.Name = "btnRun";
			this.btnRun.Size = new System.Drawing.Size(94, 1);
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
			this.mainSplit.Size = new System.Drawing.Size(1626, 766);
			this.mainSplit.SplitterDistance = 747;
			this.mainSplit.SplitterWidth = 8;
			this.mainSplit.TabIndex = 1;
			// 
			// leftSplit
			// 
			this.leftSplit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.leftSplit.Location = new System.Drawing.Point(0, 0);
			this.leftSplit.Margin = new System.Windows.Forms.Padding(4);
			this.leftSplit.Name = "leftSplit";
			this.leftSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// leftSplit.Panel1
			// 
			this.leftSplit.Panel1.Controls.Add(this.inputPanel);
			// 
			// leftSplit.Panel2
			// 
			this.leftSplit.Panel2.Controls.Add(this.outputPanel);
			this.leftSplit.Size = new System.Drawing.Size(747, 766);
			this.leftSplit.SplitterDistance = 397;
			this.leftSplit.SplitterWidth = 8;
			this.leftSplit.TabIndex = 0;
			// 
			// inputPanel
			// 
			this.inputPanel.Controls.Add(this.gridInputs);
			this.inputPanel.Controls.Add(this.lblInputTitle);
			this.inputPanel.Controls.Add(this.btnInputAdd);
			this.inputPanel.Controls.Add(this.btnInputDelete);
			this.inputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.inputPanel.Location = new System.Drawing.Point(0, 0);
			this.inputPanel.Margin = new System.Windows.Forms.Padding(4);
			this.inputPanel.Name = "inputPanel";
			this.inputPanel.Padding = new System.Windows.Forms.Padding(9, 51, 9, 9);
			this.inputPanel.Size = new System.Drawing.Size(747, 397);
			this.inputPanel.TabIndex = 0;
			// 
			// gridInputs
			// 
			this.gridInputs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridInputs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridInputs.Location = new System.Drawing.Point(9, 93);
			this.gridInputs.Margin = new System.Windows.Forms.Padding(4);
			this.gridInputs.Name = "gridInputs";
			this.gridInputs.RowHeadersWidth = 62;
			this.gridInputs.Size = new System.Drawing.Size(729, 295);
			this.gridInputs.TabIndex = 0;
			// 
			// lblInputTitle
			// 
			this.lblInputTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblInputTitle.Location = new System.Drawing.Point(9, 51);
			this.lblInputTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblInputTitle.Name = "lblInputTitle";
			this.lblInputTitle.Size = new System.Drawing.Size(729, 42);
			this.lblInputTitle.TabIndex = 1;
			this.lblInputTitle.Text = "输入定义 Inputs";
			// 
			// btnInputAdd
			// 
			this.btnInputAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnInputAdd.Location = new System.Drawing.Point(615, 6);
			this.btnInputAdd.Margin = new System.Windows.Forms.Padding(4);
			this.btnInputAdd.Name = "btnInputAdd";
			this.btnInputAdd.Size = new System.Drawing.Size(48, 36);
			this.btnInputAdd.TabIndex = 2;
			this.btnInputAdd.Text = "+";
			// 
			// btnInputDelete
			// 
			this.btnInputDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnInputDelete.Location = new System.Drawing.Point(675, 6);
			this.btnInputDelete.Margin = new System.Windows.Forms.Padding(4);
			this.btnInputDelete.Name = "btnInputDelete";
			this.btnInputDelete.Size = new System.Drawing.Size(48, 36);
			this.btnInputDelete.TabIndex = 3;
			this.btnInputDelete.Text = "-";
			// 
			// outputPanel
			// 
			this.outputPanel.Controls.Add(this.gridOutputs);
			this.outputPanel.Controls.Add(this.lblOutputTitle);
			this.outputPanel.Controls.Add(this.btnOutputAdd);
			this.outputPanel.Controls.Add(this.btnOutputDelete);
			this.outputPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outputPanel.Location = new System.Drawing.Point(0, 0);
			this.outputPanel.Margin = new System.Windows.Forms.Padding(4);
			this.outputPanel.Name = "outputPanel";
			this.outputPanel.Padding = new System.Windows.Forms.Padding(9, 51, 9, 9);
			this.outputPanel.Size = new System.Drawing.Size(747, 361);
			this.outputPanel.TabIndex = 0;
			// 
			// gridOutputs
			// 
			this.gridOutputs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridOutputs.Location = new System.Drawing.Point(9, 93);
			this.gridOutputs.Margin = new System.Windows.Forms.Padding(4);
			this.gridOutputs.Name = "gridOutputs";
			this.gridOutputs.RowHeadersWidth = 62;
			this.gridOutputs.Size = new System.Drawing.Size(729, 259);
			this.gridOutputs.TabIndex = 0;
			// 
			// lblOutputTitle
			// 
			this.lblOutputTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblOutputTitle.Location = new System.Drawing.Point(9, 51);
			this.lblOutputTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblOutputTitle.Name = "lblOutputTitle";
			this.lblOutputTitle.Size = new System.Drawing.Size(729, 42);
			this.lblOutputTitle.TabIndex = 1;
			this.lblOutputTitle.Text = "输出定义 Outputs";
			// 
			// btnOutputAdd
			// 
			this.btnOutputAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOutputAdd.Location = new System.Drawing.Point(615, 6);
			this.btnOutputAdd.Margin = new System.Windows.Forms.Padding(4);
			this.btnOutputAdd.Name = "btnOutputAdd";
			this.btnOutputAdd.Size = new System.Drawing.Size(48, 36);
			this.btnOutputAdd.TabIndex = 2;
			this.btnOutputAdd.Text = "+";
			// 
			// btnOutputDelete
			// 
			this.btnOutputDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOutputDelete.Location = new System.Drawing.Point(675, 6);
			this.btnOutputDelete.Margin = new System.Windows.Forms.Padding(4);
			this.btnOutputDelete.Name = "btnOutputDelete";
			this.btnOutputDelete.Size = new System.Drawing.Size(48, 36);
			this.btnOutputDelete.TabIndex = 3;
			this.btnOutputDelete.Text = "-";
			// 
			// codePanel
			// 
			this.codePanel.Controls.Add(this.codeEditorHost);
			this.codePanel.Controls.Add(this.lblCodeTitle);
			this.codePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.codePanel.Location = new System.Drawing.Point(0, 0);
			this.codePanel.Margin = new System.Windows.Forms.Padding(4);
			this.codePanel.Name = "codePanel";
			this.codePanel.Padding = new System.Windows.Forms.Padding(9);
			this.codePanel.Size = new System.Drawing.Size(871, 766);
			this.codePanel.TabIndex = 0;
			// 
			// codeEditorHost
			// 
			this.codeEditorHost.Controls.Add(this.txtCode);
			this.codeEditorHost.Controls.Add(this.panelLineNumbers);
			this.codeEditorHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.codeEditorHost.Location = new System.Drawing.Point(9, 51);
			this.codeEditorHost.Margin = new System.Windows.Forms.Padding(4);
			this.codeEditorHost.Name = "codeEditorHost";
			this.codeEditorHost.Size = new System.Drawing.Size(853, 706);
			this.codeEditorHost.TabIndex = 1;
			// 
			// txtCode
			// 
			this.txtCode.AcceptsTab = true;
			this.txtCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtCode.HideSelection = false;
			this.txtCode.Location = new System.Drawing.Point(63, 0);
			this.txtCode.Margin = new System.Windows.Forms.Padding(4);
			this.txtCode.Name = "txtCode";
			this.txtCode.Size = new System.Drawing.Size(790, 706);
			this.txtCode.TabIndex = 1;
			this.txtCode.Text = "";
			this.txtCode.WordWrap = false;
			// 
			// panelLineNumbers
			// 
			this.panelLineNumbers.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelLineNumbers.Location = new System.Drawing.Point(0, 0);
			this.panelLineNumbers.Margin = new System.Windows.Forms.Padding(4);
			this.panelLineNumbers.Name = "panelLineNumbers";
			this.panelLineNumbers.Size = new System.Drawing.Size(63, 706);
			this.panelLineNumbers.TabIndex = 0;
			// 
			// lblCodeTitle
			// 
			this.lblCodeTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblCodeTitle.Location = new System.Drawing.Point(9, 9);
			this.lblCodeTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblCodeTitle.Name = "lblCodeTitle";
			this.lblCodeTitle.Size = new System.Drawing.Size(853, 42);
			this.lblCodeTitle.TabIndex = 0;
			this.lblCodeTitle.Text = "C# Script Code";
			this.lblCodeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// logPanel
			// 
			this.logPanel.Controls.Add(this.gridLogs);
			this.logPanel.Controls.Add(this.lblLogTitle);
			this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logPanel.Location = new System.Drawing.Point(12, 862);
			this.logPanel.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
			this.logPanel.Name = "logPanel";
			this.logPanel.Padding = new System.Windows.Forms.Padding(9, 51, 9, 9);
			this.logPanel.Size = new System.Drawing.Size(1626, 206);
			this.logPanel.TabIndex = 2;
			// 
			// gridLogs
			// 
			this.gridLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridLogs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridLogs.Location = new System.Drawing.Point(9, 93);
			this.gridLogs.Margin = new System.Windows.Forms.Padding(4);
			this.gridLogs.Name = "gridLogs";
			this.gridLogs.RowHeadersWidth = 62;
			this.gridLogs.Size = new System.Drawing.Size(1608, 104);
			this.gridLogs.TabIndex = 0;
			// 
			// lblLogTitle
			// 
			this.lblLogTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblLogTitle.Location = new System.Drawing.Point(9, 51);
			this.lblLogTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLogTitle.Name = "lblLogTitle";
			this.lblLogTitle.Size = new System.Drawing.Size(1608, 42);
			this.lblLogTitle.TabIndex = 1;
			this.lblLogTitle.Text = "编译 / 运行日志";
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
			this.leftSplit.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.leftSplit)).EndInit();
			this.leftSplit.ResumeLayout(false);
			this.inputPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridInputs)).EndInit();
			this.outputPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridOutputs)).EndInit();
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
