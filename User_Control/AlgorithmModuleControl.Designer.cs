using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class AlgorithmModuleControl
	{
		/// <summary>
		/// Designer 只保留纯控件创建和布局。
		/// 本文件的 InitializeComponent 不调用主 .cs 的任何自定义方法，也不做事件绑定/项目读取/运行时切换。
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
			this.panelLibrary = new System.Windows.Forms.Panel();
			this.cardVM = new System.Windows.Forms.Panel();
			this.cardVMLayout = new System.Windows.Forms.TableLayoutPanel();
			this.btnVM = new System.Windows.Forms.Button();
			this.lblEnableVM = new System.Windows.Forms.Label();
			this.chkEnableVM = new System.Windows.Forms.CheckBox();
			this.gapLibrary3 = new System.Windows.Forms.Panel();
			this.cardHdev = new System.Windows.Forms.Panel();
			this.cardHdevLayout = new System.Windows.Forms.TableLayoutPanel();
			this.btnHdev = new System.Windows.Forms.Button();
			this.lblEnableHdev = new System.Windows.Forms.Label();
			this.chkEnableHdev = new System.Windows.Forms.CheckBox();
			this.gapLibrary2 = new System.Windows.Forms.Panel();
			this.cardScript = new System.Windows.Forms.Panel();
			this.cardScriptLayout = new System.Windows.Forms.TableLayoutPanel();
			this.btnScript = new System.Windows.Forms.Button();
			this.lblEnableScript = new System.Windows.Forms.Label();
			this.chkEnableScript = new System.Windows.Forms.CheckBox();
			this.gapLibrary1 = new System.Windows.Forms.Panel();
			this.cardVpp = new System.Windows.Forms.Panel();
			this.cardVppLayout = new System.Windows.Forms.TableLayoutPanel();
			this.btnVpp = new System.Windows.Forms.Button();
			this.lblEnableVpp = new System.Windows.Forms.Label();
			this.chkEnableVpp = new System.Windows.Forms.CheckBox();
			this.gapRoot1 = new System.Windows.Forms.Panel();
			this.jobTaskLayout = new System.Windows.Forms.TableLayoutPanel();
			this.grpJobs = new System.Windows.Forms.GroupBox();
			this.listJobs = new System.Windows.Forms.ListBox();
			this.gapJobTask = new System.Windows.Forms.Panel();
			this.grpTasks = new System.Windows.Forms.GroupBox();
			this.listTasks = new System.Windows.Forms.ListBox();
			this.gapRoot2 = new System.Windows.Forms.Panel();
			this.grpFiles = new System.Windows.Forms.GroupBox();
			this.listAlgorithmFiles = new System.Windows.Forms.ListBox();
			this.gapRoot3 = new System.Windows.Forms.Panel();
			this.splitRight = new System.Windows.Forms.SplitContainer();
			this.grpPins = new System.Windows.Forms.GroupBox();
			this.pinLayout = new System.Windows.Forms.TableLayoutPanel();
			this.dgvPins = new System.Windows.Forms.DataGridView();
			this.colDirection = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colGlobalVariable = new System.Windows.Forms.DataGridViewButtonColumn();
			this.panelPinButtons = new System.Windows.Forms.Panel();
			this.btnApplyInputs = new System.Windows.Forms.Button();
			this.btnRunReplay = new System.Windows.Forms.Button();
			this.btnLoadEditor = new System.Windows.Forms.Button();
			this.btnSaveVpp = new System.Windows.Forms.Button();
			this.grpEditor = new System.Windows.Forms.GroupBox();
			this.panelEditorHost = new System.Windows.Forms.Panel();
			this.lblEditorInfo = new System.Windows.Forms.Label();
			this.rootLayout.SuspendLayout();
			this.panelLibrary.SuspendLayout();
			this.cardVM.SuspendLayout();
			this.cardVMLayout.SuspendLayout();
			this.cardHdev.SuspendLayout();
			this.cardHdevLayout.SuspendLayout();
			this.cardScript.SuspendLayout();
			this.cardScriptLayout.SuspendLayout();
			this.cardVpp.SuspendLayout();
			this.cardVppLayout.SuspendLayout();
			this.jobTaskLayout.SuspendLayout();
			this.grpJobs.SuspendLayout();
			this.grpTasks.SuspendLayout();
			this.grpFiles.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
			this.splitRight.Panel1.SuspendLayout();
			this.splitRight.Panel2.SuspendLayout();
			this.splitRight.SuspendLayout();
			this.grpPins.SuspendLayout();
			this.pinLayout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvPins)).BeginInit();
			this.panelPinButtons.SuspendLayout();
			this.grpEditor.SuspendLayout();
			this.panelEditorHost.SuspendLayout();
			this.SuspendLayout();
			// 
			// rootLayout
			// 
			this.rootLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.rootLayout.ColumnCount = 7;
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 225F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 210F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Controls.Add(this.panelLibrary, 0, 0);
			this.rootLayout.Controls.Add(this.gapRoot1, 1, 0);
			this.rootLayout.Controls.Add(this.jobTaskLayout, 2, 0);
			this.rootLayout.Controls.Add(this.gapRoot2, 3, 0);
			this.rootLayout.Controls.Add(this.grpFiles, 4, 0);
			this.rootLayout.Controls.Add(this.gapRoot3, 5, 0);
			this.rootLayout.Controls.Add(this.splitRight, 6, 0);
			this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rootLayout.Location = new System.Drawing.Point(0, 0);
			this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
			this.rootLayout.Name = "rootLayout";
			this.rootLayout.Padding = new System.Windows.Forms.Padding(8, 10, 10, 10);
			this.rootLayout.RowCount = 1;
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Size = new System.Drawing.Size(1903, 933);
			this.rootLayout.TabIndex = 0;
			// 
			// panelLibrary
			// 
			this.panelLibrary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.panelLibrary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelLibrary.Controls.Add(this.cardVM);
			this.panelLibrary.Controls.Add(this.gapLibrary3);
			this.panelLibrary.Controls.Add(this.cardHdev);
			this.panelLibrary.Controls.Add(this.gapLibrary2);
			this.panelLibrary.Controls.Add(this.cardScript);
			this.panelLibrary.Controls.Add(this.gapLibrary1);
			this.panelLibrary.Controls.Add(this.cardVpp);
			this.panelLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLibrary.Location = new System.Drawing.Point(8, 10);
			this.panelLibrary.Margin = new System.Windows.Forms.Padding(0);
			this.panelLibrary.Name = "panelLibrary";
			this.panelLibrary.Padding = new System.Windows.Forms.Padding(10, 14, 10, 10);
			this.panelLibrary.Size = new System.Drawing.Size(190, 913);
			this.panelLibrary.TabIndex = 0;
			// 
			// cardVM
			// 
			this.cardVM.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.cardVM.Controls.Add(this.cardVMLayout);
			this.cardVM.Dock = System.Windows.Forms.DockStyle.Top;
			this.cardVM.Location = new System.Drawing.Point(10, 200);
			this.cardVM.Margin = new System.Windows.Forms.Padding(0);
			this.cardVM.Name = "cardVM";
			this.cardVM.Size = new System.Drawing.Size(168, 50);
			this.cardVM.TabIndex = 0;
			// 
			// cardVMLayout
			// 
			this.cardVMLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.cardVMLayout.ColumnCount = 3;
			this.cardVMLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cardVMLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
			this.cardVMLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.cardVMLayout.Controls.Add(this.btnVM, 0, 0);
			this.cardVMLayout.Controls.Add(this.lblEnableVM, 1, 0);
			this.cardVMLayout.Controls.Add(this.chkEnableVM, 2, 0);
			this.cardVMLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cardVMLayout.Location = new System.Drawing.Point(0, 0);
			this.cardVMLayout.Margin = new System.Windows.Forms.Padding(0);
			this.cardVMLayout.Name = "cardVMLayout";
			this.cardVMLayout.RowCount = 1;
			this.cardVMLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cardVMLayout.Size = new System.Drawing.Size(168, 50);
			this.cardVMLayout.TabIndex = 0;
			// 
			// btnVM
			// 
			this.btnVM.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnVM.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnVM.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(65)))), ((int)(((byte)(95)))));
			this.btnVM.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnVM.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnVM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnVM.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnVM.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(220)))), ((int)(((byte)(235)))));
			this.btnVM.Location = new System.Drawing.Point(0, 0);
			this.btnVM.Margin = new System.Windows.Forms.Padding(0);
			this.btnVM.Name = "btnVM";
			this.btnVM.Size = new System.Drawing.Size(102, 50);
			this.btnVM.TabIndex = 0;
			this.btnVM.Text = "VM";
			this.btnVM.UseVisualStyleBackColor = false;
			// 
			// lblEnableVM
			// 
			this.lblEnableVM.BackColor = System.Drawing.Color.Transparent;
			this.lblEnableVM.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblEnableVM.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Bold);
			this.lblEnableVM.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(210)))), ((int)(((byte)(230)))));
			this.lblEnableVM.Location = new System.Drawing.Point(102, 0);
			this.lblEnableVM.Margin = new System.Windows.Forms.Padding(0);
			this.lblEnableVM.Name = "lblEnableVM";
			this.lblEnableVM.Size = new System.Drawing.Size(44, 50);
			this.lblEnableVM.TabIndex = 1;
			this.lblEnableVM.Text = "启用";
			this.lblEnableVM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkEnableVM
			// 
			this.chkEnableVM.BackColor = System.Drawing.Color.Transparent;
			this.chkEnableVM.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.chkEnableVM.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkEnableVM.Location = new System.Drawing.Point(146, 0);
			this.chkEnableVM.Margin = new System.Windows.Forms.Padding(0);
			this.chkEnableVM.Name = "chkEnableVM";
			this.chkEnableVM.Size = new System.Drawing.Size(22, 50);
			this.chkEnableVM.TabIndex = 2;
			this.chkEnableVM.UseVisualStyleBackColor = false;
			// 
			// gapLibrary3
			// 
			this.gapLibrary3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.gapLibrary3.Dock = System.Windows.Forms.DockStyle.Top;
			this.gapLibrary3.Location = new System.Drawing.Point(10, 188);
			this.gapLibrary3.Margin = new System.Windows.Forms.Padding(0);
			this.gapLibrary3.Name = "gapLibrary3";
			this.gapLibrary3.Size = new System.Drawing.Size(168, 12);
			this.gapLibrary3.TabIndex = 1;
			// 
			// cardHdev
			// 
			this.cardHdev.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.cardHdev.Controls.Add(this.cardHdevLayout);
			this.cardHdev.Dock = System.Windows.Forms.DockStyle.Top;
			this.cardHdev.Location = new System.Drawing.Point(10, 138);
			this.cardHdev.Margin = new System.Windows.Forms.Padding(0);
			this.cardHdev.Name = "cardHdev";
			this.cardHdev.Size = new System.Drawing.Size(168, 50);
			this.cardHdev.TabIndex = 2;
			// 
			// cardHdevLayout
			// 
			this.cardHdevLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.cardHdevLayout.ColumnCount = 3;
			this.cardHdevLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cardHdevLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
			this.cardHdevLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.cardHdevLayout.Controls.Add(this.btnHdev, 0, 0);
			this.cardHdevLayout.Controls.Add(this.lblEnableHdev, 1, 0);
			this.cardHdevLayout.Controls.Add(this.chkEnableHdev, 2, 0);
			this.cardHdevLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cardHdevLayout.Location = new System.Drawing.Point(0, 0);
			this.cardHdevLayout.Margin = new System.Windows.Forms.Padding(0);
			this.cardHdevLayout.Name = "cardHdevLayout";
			this.cardHdevLayout.RowCount = 1;
			this.cardHdevLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cardHdevLayout.Size = new System.Drawing.Size(168, 50);
			this.cardHdevLayout.TabIndex = 0;
			// 
			// btnHdev
			// 
			this.btnHdev.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnHdev.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnHdev.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(65)))), ((int)(((byte)(95)))));
			this.btnHdev.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnHdev.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnHdev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnHdev.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnHdev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(220)))), ((int)(((byte)(235)))));
			this.btnHdev.Location = new System.Drawing.Point(0, 0);
			this.btnHdev.Margin = new System.Windows.Forms.Padding(0);
			this.btnHdev.Name = "btnHdev";
			this.btnHdev.Size = new System.Drawing.Size(102, 50);
			this.btnHdev.TabIndex = 0;
			this.btnHdev.Text = "Hdev";
			this.btnHdev.UseVisualStyleBackColor = false;
			// 
			// lblEnableHdev
			// 
			this.lblEnableHdev.BackColor = System.Drawing.Color.Transparent;
			this.lblEnableHdev.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblEnableHdev.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Bold);
			this.lblEnableHdev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(210)))), ((int)(((byte)(230)))));
			this.lblEnableHdev.Location = new System.Drawing.Point(102, 0);
			this.lblEnableHdev.Margin = new System.Windows.Forms.Padding(0);
			this.lblEnableHdev.Name = "lblEnableHdev";
			this.lblEnableHdev.Size = new System.Drawing.Size(44, 50);
			this.lblEnableHdev.TabIndex = 1;
			this.lblEnableHdev.Text = "启用";
			this.lblEnableHdev.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkEnableHdev
			// 
			this.chkEnableHdev.BackColor = System.Drawing.Color.Transparent;
			this.chkEnableHdev.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.chkEnableHdev.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkEnableHdev.Location = new System.Drawing.Point(146, 0);
			this.chkEnableHdev.Margin = new System.Windows.Forms.Padding(0);
			this.chkEnableHdev.Name = "chkEnableHdev";
			this.chkEnableHdev.Size = new System.Drawing.Size(22, 50);
			this.chkEnableHdev.TabIndex = 2;
			this.chkEnableHdev.UseVisualStyleBackColor = false;
			// 
			// gapLibrary2
			// 
			this.gapLibrary2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.gapLibrary2.Dock = System.Windows.Forms.DockStyle.Top;
			this.gapLibrary2.Location = new System.Drawing.Point(10, 126);
			this.gapLibrary2.Margin = new System.Windows.Forms.Padding(0);
			this.gapLibrary2.Name = "gapLibrary2";
			this.gapLibrary2.Size = new System.Drawing.Size(168, 12);
			this.gapLibrary2.TabIndex = 3;
			// 
			// cardScript
			// 
			this.cardScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.cardScript.Controls.Add(this.cardScriptLayout);
			this.cardScript.Dock = System.Windows.Forms.DockStyle.Top;
			this.cardScript.Location = new System.Drawing.Point(10, 76);
			this.cardScript.Margin = new System.Windows.Forms.Padding(0);
			this.cardScript.Name = "cardScript";
			this.cardScript.Size = new System.Drawing.Size(168, 50);
			this.cardScript.TabIndex = 4;
			// 
			// cardScriptLayout
			// 
			this.cardScriptLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.cardScriptLayout.ColumnCount = 3;
			this.cardScriptLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cardScriptLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
			this.cardScriptLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.cardScriptLayout.Controls.Add(this.btnScript, 0, 0);
			this.cardScriptLayout.Controls.Add(this.lblEnableScript, 1, 0);
			this.cardScriptLayout.Controls.Add(this.chkEnableScript, 2, 0);
			this.cardScriptLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cardScriptLayout.Location = new System.Drawing.Point(0, 0);
			this.cardScriptLayout.Margin = new System.Windows.Forms.Padding(0);
			this.cardScriptLayout.Name = "cardScriptLayout";
			this.cardScriptLayout.RowCount = 1;
			this.cardScriptLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cardScriptLayout.Size = new System.Drawing.Size(168, 50);
			this.cardScriptLayout.TabIndex = 0;
			// 
			// btnScript
			// 
			this.btnScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnScript.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(65)))), ((int)(((byte)(95)))));
			this.btnScript.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnScript.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnScript.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnScript.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnScript.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(220)))), ((int)(((byte)(235)))));
			this.btnScript.Location = new System.Drawing.Point(0, 0);
			this.btnScript.Margin = new System.Windows.Forms.Padding(0);
			this.btnScript.Name = "btnScript";
			this.btnScript.Size = new System.Drawing.Size(102, 50);
			this.btnScript.TabIndex = 0;
			this.btnScript.Text = "Script";
			this.btnScript.UseVisualStyleBackColor = false;
			// 
			// lblEnableScript
			// 
			this.lblEnableScript.BackColor = System.Drawing.Color.Transparent;
			this.lblEnableScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblEnableScript.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Bold);
			this.lblEnableScript.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(210)))), ((int)(((byte)(230)))));
			this.lblEnableScript.Location = new System.Drawing.Point(102, 0);
			this.lblEnableScript.Margin = new System.Windows.Forms.Padding(0);
			this.lblEnableScript.Name = "lblEnableScript";
			this.lblEnableScript.Size = new System.Drawing.Size(44, 50);
			this.lblEnableScript.TabIndex = 1;
			this.lblEnableScript.Text = "启用";
			this.lblEnableScript.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkEnableScript
			// 
			this.chkEnableScript.BackColor = System.Drawing.Color.Transparent;
			this.chkEnableScript.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.chkEnableScript.Checked = true;
			this.chkEnableScript.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkEnableScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkEnableScript.Location = new System.Drawing.Point(146, 0);
			this.chkEnableScript.Margin = new System.Windows.Forms.Padding(0);
			this.chkEnableScript.Name = "chkEnableScript";
			this.chkEnableScript.Size = new System.Drawing.Size(22, 50);
			this.chkEnableScript.TabIndex = 2;
			this.chkEnableScript.UseVisualStyleBackColor = false;
			// 
			// gapLibrary1
			// 
			this.gapLibrary1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.gapLibrary1.Dock = System.Windows.Forms.DockStyle.Top;
			this.gapLibrary1.Location = new System.Drawing.Point(10, 64);
			this.gapLibrary1.Margin = new System.Windows.Forms.Padding(0);
			this.gapLibrary1.Name = "gapLibrary1";
			this.gapLibrary1.Size = new System.Drawing.Size(168, 12);
			this.gapLibrary1.TabIndex = 5;
			// 
			// cardVpp
			// 
			this.cardVpp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.cardVpp.Controls.Add(this.cardVppLayout);
			this.cardVpp.Dock = System.Windows.Forms.DockStyle.Top;
			this.cardVpp.Location = new System.Drawing.Point(10, 14);
			this.cardVpp.Margin = new System.Windows.Forms.Padding(0);
			this.cardVpp.Name = "cardVpp";
			this.cardVpp.Size = new System.Drawing.Size(168, 50);
			this.cardVpp.TabIndex = 6;
			// 
			// cardVppLayout
			// 
			this.cardVppLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.cardVppLayout.ColumnCount = 3;
			this.cardVppLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cardVppLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
			this.cardVppLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.cardVppLayout.Controls.Add(this.btnVpp, 0, 0);
			this.cardVppLayout.Controls.Add(this.lblEnableVpp, 1, 0);
			this.cardVppLayout.Controls.Add(this.chkEnableVpp, 2, 0);
			this.cardVppLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cardVppLayout.Location = new System.Drawing.Point(0, 0);
			this.cardVppLayout.Margin = new System.Windows.Forms.Padding(0);
			this.cardVppLayout.Name = "cardVppLayout";
			this.cardVppLayout.RowCount = 1;
			this.cardVppLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.cardVppLayout.Size = new System.Drawing.Size(168, 50);
			this.cardVppLayout.TabIndex = 0;
			// 
			// btnVpp
			// 
			this.btnVpp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnVpp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnVpp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(65)))), ((int)(((byte)(95)))));
			this.btnVpp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnVpp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnVpp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnVpp.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.btnVpp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(220)))), ((int)(((byte)(235)))));
			this.btnVpp.Location = new System.Drawing.Point(0, 0);
			this.btnVpp.Margin = new System.Windows.Forms.Padding(0);
			this.btnVpp.Name = "btnVpp";
			this.btnVpp.Size = new System.Drawing.Size(102, 50);
			this.btnVpp.TabIndex = 0;
			this.btnVpp.Text = "Vpp";
			this.btnVpp.UseVisualStyleBackColor = false;
			// 
			// lblEnableVpp
			// 
			this.lblEnableVpp.BackColor = System.Drawing.Color.Transparent;
			this.lblEnableVpp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblEnableVpp.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Bold);
			this.lblEnableVpp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(210)))), ((int)(((byte)(230)))));
			this.lblEnableVpp.Location = new System.Drawing.Point(102, 0);
			this.lblEnableVpp.Margin = new System.Windows.Forms.Padding(0);
			this.lblEnableVpp.Name = "lblEnableVpp";
			this.lblEnableVpp.Size = new System.Drawing.Size(44, 50);
			this.lblEnableVpp.TabIndex = 1;
			this.lblEnableVpp.Text = "启用";
			this.lblEnableVpp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// chkEnableVpp
			// 
			this.chkEnableVpp.BackColor = System.Drawing.Color.Transparent;
			this.chkEnableVpp.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.chkEnableVpp.Checked = true;
			this.chkEnableVpp.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkEnableVpp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkEnableVpp.Location = new System.Drawing.Point(146, 0);
			this.chkEnableVpp.Margin = new System.Windows.Forms.Padding(0);
			this.chkEnableVpp.Name = "chkEnableVpp";
			this.chkEnableVpp.Size = new System.Drawing.Size(22, 50);
			this.chkEnableVpp.TabIndex = 2;
			this.chkEnableVpp.UseVisualStyleBackColor = false;
			// 
			// gapRoot1
			// 
			this.gapRoot1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.gapRoot1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gapRoot1.Location = new System.Drawing.Point(198, 10);
			this.gapRoot1.Margin = new System.Windows.Forms.Padding(0);
			this.gapRoot1.Name = "gapRoot1";
			this.gapRoot1.Size = new System.Drawing.Size(10, 913);
			this.gapRoot1.TabIndex = 1;
			// 
			// jobTaskLayout
			// 
			this.jobTaskLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.jobTaskLayout.ColumnCount = 1;
			this.jobTaskLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.jobTaskLayout.Controls.Add(this.grpJobs, 0, 0);
			this.jobTaskLayout.Controls.Add(this.gapJobTask, 0, 1);
			this.jobTaskLayout.Controls.Add(this.grpTasks, 0, 2);
			this.jobTaskLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobTaskLayout.Location = new System.Drawing.Point(208, 10);
			this.jobTaskLayout.Margin = new System.Windows.Forms.Padding(0);
			this.jobTaskLayout.Name = "jobTaskLayout";
			this.jobTaskLayout.RowCount = 3;
			this.jobTaskLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.jobTaskLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.jobTaskLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.jobTaskLayout.Size = new System.Drawing.Size(225, 913);
			this.jobTaskLayout.TabIndex = 2;
			// 
			// grpJobs
			// 
			this.grpJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.grpJobs.Controls.Add(this.listJobs);
			this.grpJobs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grpJobs.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.grpJobs.ForeColor = System.Drawing.Color.White;
			this.grpJobs.Location = new System.Drawing.Point(0, 0);
			this.grpJobs.Margin = new System.Windows.Forms.Padding(0);
			this.grpJobs.Name = "grpJobs";
			this.grpJobs.Padding = new System.Windows.Forms.Padding(12, 26, 12, 12);
			this.grpJobs.Size = new System.Drawing.Size(225, 448);
			this.grpJobs.TabIndex = 0;
			this.grpJobs.TabStop = false;
			this.grpJobs.Text = "所有 JobID";
			// 
			// listJobs
			// 
			this.listJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(8)))), ((int)(((byte)(16)))));
			this.listJobs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listJobs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listJobs.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.listJobs.ForeColor = System.Drawing.Color.White;
			this.listJobs.IntegralHeight = false;
			this.listJobs.ItemHeight = 24;
			this.listJobs.Location = new System.Drawing.Point(12, 52);
			this.listJobs.Name = "listJobs";
			this.listJobs.Size = new System.Drawing.Size(201, 384);
			this.listJobs.TabIndex = 0;
			// 
			// gapJobTask
			// 
			this.gapJobTask.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.gapJobTask.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gapJobTask.Location = new System.Drawing.Point(0, 448);
			this.gapJobTask.Margin = new System.Windows.Forms.Padding(0);
			this.gapJobTask.Name = "gapJobTask";
			this.gapJobTask.Size = new System.Drawing.Size(225, 16);
			this.gapJobTask.TabIndex = 1;
			// 
			// grpTasks
			// 
			this.grpTasks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.grpTasks.Controls.Add(this.listTasks);
			this.grpTasks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grpTasks.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.grpTasks.ForeColor = System.Drawing.Color.White;
			this.grpTasks.Location = new System.Drawing.Point(0, 464);
			this.grpTasks.Margin = new System.Windows.Forms.Padding(0);
			this.grpTasks.Name = "grpTasks";
			this.grpTasks.Padding = new System.Windows.Forms.Padding(12, 26, 12, 12);
			this.grpTasks.Size = new System.Drawing.Size(225, 449);
			this.grpTasks.TabIndex = 2;
			this.grpTasks.TabStop = false;
			this.grpTasks.Text = "所有 Task";
			// 
			// listTasks
			// 
			this.listTasks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(8)))), ((int)(((byte)(16)))));
			this.listTasks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listTasks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listTasks.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.listTasks.ForeColor = System.Drawing.Color.White;
			this.listTasks.IntegralHeight = false;
			this.listTasks.ItemHeight = 24;
			this.listTasks.Location = new System.Drawing.Point(12, 52);
			this.listTasks.Name = "listTasks";
			this.listTasks.Size = new System.Drawing.Size(201, 385);
			this.listTasks.TabIndex = 0;
			// 
			// gapRoot2
			// 
			this.gapRoot2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.gapRoot2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gapRoot2.Location = new System.Drawing.Point(433, 10);
			this.gapRoot2.Margin = new System.Windows.Forms.Padding(0);
			this.gapRoot2.Name = "gapRoot2";
			this.gapRoot2.Size = new System.Drawing.Size(10, 913);
			this.gapRoot2.TabIndex = 3;
			// 
			// grpFiles
			// 
			this.grpFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.grpFiles.Controls.Add(this.listAlgorithmFiles);
			this.grpFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grpFiles.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.grpFiles.ForeColor = System.Drawing.Color.White;
			this.grpFiles.Location = new System.Drawing.Point(443, 10);
			this.grpFiles.Margin = new System.Windows.Forms.Padding(0);
			this.grpFiles.Name = "grpFiles";
			this.grpFiles.Padding = new System.Windows.Forms.Padding(12, 26, 12, 12);
			this.grpFiles.Size = new System.Drawing.Size(210, 913);
			this.grpFiles.TabIndex = 4;
			this.grpFiles.TabStop = false;
			this.grpFiles.Text = "所有 VPP";
			// 
			// listAlgorithmFiles
			// 
			this.listAlgorithmFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(8)))), ((int)(((byte)(16)))));
			this.listAlgorithmFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listAlgorithmFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listAlgorithmFiles.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.listAlgorithmFiles.ForeColor = System.Drawing.Color.White;
			this.listAlgorithmFiles.IntegralHeight = false;
			this.listAlgorithmFiles.ItemHeight = 24;
			this.listAlgorithmFiles.Location = new System.Drawing.Point(12, 52);
			this.listAlgorithmFiles.Name = "listAlgorithmFiles";
			this.listAlgorithmFiles.Size = new System.Drawing.Size(186, 849);
			this.listAlgorithmFiles.TabIndex = 0;
			// 
			// gapRoot3
			// 
			this.gapRoot3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.gapRoot3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gapRoot3.Location = new System.Drawing.Point(653, 10);
			this.gapRoot3.Margin = new System.Windows.Forms.Padding(0);
			this.gapRoot3.Name = "gapRoot3";
			this.gapRoot3.Size = new System.Drawing.Size(10, 913);
			this.gapRoot3.TabIndex = 5;
			// 
			// splitRight
			// 
			this.splitRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitRight.IsSplitterFixed = true;
			this.splitRight.Location = new System.Drawing.Point(663, 10);
			this.splitRight.Margin = new System.Windows.Forms.Padding(0);
			this.splitRight.Name = "splitRight";
			this.splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitRight.Panel1
			// 
			this.splitRight.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.splitRight.Panel1.Controls.Add(this.grpPins);
			// 
			// splitRight.Panel2
			// 
			this.splitRight.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.splitRight.Panel2.Controls.Add(this.grpEditor);
			this.splitRight.Panel2Collapsed = true;
			this.splitRight.Size = new System.Drawing.Size(1230, 913);
			this.splitRight.SplitterWidth = 1;
			this.splitRight.TabIndex = 6;
			// 
			// grpPins
			// 
			this.grpPins.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.grpPins.Controls.Add(this.pinLayout);
			this.grpPins.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grpPins.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.grpPins.ForeColor = System.Drawing.Color.White;
			this.grpPins.Location = new System.Drawing.Point(0, 0);
			this.grpPins.Margin = new System.Windows.Forms.Padding(0);
			this.grpPins.Name = "grpPins";
			this.grpPins.Padding = new System.Windows.Forms.Padding(12, 26, 12, 12);
			this.grpPins.Size = new System.Drawing.Size(1230, 913);
			this.grpPins.TabIndex = 0;
			this.grpPins.TabStop = false;
			this.grpPins.Text = "输入/输出引脚";
			// 
			// pinLayout
			// 
			this.pinLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.pinLayout.ColumnCount = 1;
			this.pinLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.pinLayout.Controls.Add(this.dgvPins, 0, 0);
			this.pinLayout.Controls.Add(this.panelPinButtons, 0, 1);
			this.pinLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pinLayout.Location = new System.Drawing.Point(12, 52);
			this.pinLayout.Margin = new System.Windows.Forms.Padding(0);
			this.pinLayout.Name = "pinLayout";
			this.pinLayout.RowCount = 2;
			this.pinLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.pinLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
			this.pinLayout.Size = new System.Drawing.Size(1206, 849);
			this.pinLayout.TabIndex = 0;
			// 
			// dgvPins
			// 
			this.dgvPins.AllowUserToAddRows = false;
			this.dgvPins.AllowUserToDeleteRows = false;
			this.dgvPins.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvPins.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(8)))), ((int)(((byte)(16)))));
			this.dgvPins.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(28)))), ((int)(((byte)(48)))));
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvPins.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dgvPins.ColumnHeadersHeight = 34;
			this.dgvPins.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDirection,
            this.colName,
            this.colDataType,
            this.colValue,
            this.colGlobalVariable});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(8)))), ((int)(((byte)(16)))));
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(200)))));
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvPins.DefaultCellStyle = dataGridViewCellStyle2;
			this.dgvPins.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvPins.EnableHeadersVisualStyles = false;
			this.dgvPins.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(70)))), ((int)(((byte)(95)))));
			this.dgvPins.Location = new System.Drawing.Point(3, 3);
			this.dgvPins.MultiSelect = false;
			this.dgvPins.Name = "dgvPins";
			this.dgvPins.RowHeadersVisible = false;
			this.dgvPins.RowHeadersWidth = 62;
			this.dgvPins.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvPins.Size = new System.Drawing.Size(1200, 801);
			this.dgvPins.TabIndex = 0;
			// 
			// colDirection
			// 
			this.colDirection.FillWeight = 60F;
			this.colDirection.HeaderText = "类型";
			this.colDirection.MinimumWidth = 8;
			this.colDirection.Name = "colDirection";
			this.colDirection.ReadOnly = true;
			// 
			// colName
			// 
			this.colName.FillWeight = 130F;
			this.colName.HeaderText = "引脚名称";
			this.colName.MinimumWidth = 8;
			this.colName.Name = "colName";
			this.colName.ReadOnly = true;
			// 
			// colDataType
			// 
			this.colDataType.FillWeight = 110F;
			this.colDataType.HeaderText = "数据类型";
			this.colDataType.MinimumWidth = 8;
			this.colDataType.Name = "colDataType";
			this.colDataType.ReadOnly = true;
			// 
			// colValue
			// 
			this.colValue.FillWeight = 180F;
			this.colValue.HeaderText = "当前值 / 自定义值";
			this.colValue.MinimumWidth = 8;
			this.colValue.Name = "colValue";
			// 
			// colGlobalVariable
			// 
			this.colGlobalVariable.FillWeight = 120F;
			this.colGlobalVariable.HeaderText = "关联全局变量";
			this.colGlobalVariable.MinimumWidth = 8;
			this.colGlobalVariable.Name = "colGlobalVariable";
			this.colGlobalVariable.Text = "选择...";
			this.colGlobalVariable.UseColumnTextForButtonValue = false;
			// 
			// panelPinButtons
			// 
			this.panelPinButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.panelPinButtons.Controls.Add(this.btnApplyInputs);
			this.panelPinButtons.Controls.Add(this.btnRunReplay);
			this.panelPinButtons.Controls.Add(this.btnLoadEditor);
			this.panelPinButtons.Controls.Add(this.btnSaveVpp);
			this.panelPinButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelPinButtons.Location = new System.Drawing.Point(0, 807);
			this.panelPinButtons.Margin = new System.Windows.Forms.Padding(0);
			this.panelPinButtons.Name = "panelPinButtons";
			this.panelPinButtons.Size = new System.Drawing.Size(1206, 42);
			this.panelPinButtons.TabIndex = 1;
			// 
			// btnApplyInputs
			// 
			this.btnApplyInputs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.btnApplyInputs.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnApplyInputs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnApplyInputs.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnApplyInputs.ForeColor = System.Drawing.Color.White;
			this.btnApplyInputs.Location = new System.Drawing.Point(0, 6);
			this.btnApplyInputs.Name = "btnApplyInputs";
			this.btnApplyInputs.Size = new System.Drawing.Size(95, 30);
			this.btnApplyInputs.TabIndex = 0;
			this.btnApplyInputs.Text = "应用输入";
			this.btnApplyInputs.UseVisualStyleBackColor = false;
			// 
			// btnRunReplay
			// 
			this.btnRunReplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.btnRunReplay.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnRunReplay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRunReplay.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnRunReplay.ForeColor = System.Drawing.Color.White;
			this.btnRunReplay.Location = new System.Drawing.Point(105, 6);
			this.btnRunReplay.Name = "btnRunReplay";
			this.btnRunReplay.Size = new System.Drawing.Size(95, 30);
			this.btnRunReplay.TabIndex = 1;
			this.btnRunReplay.Text = "回放运行";
			this.btnRunReplay.UseVisualStyleBackColor = false;
			// 
			// btnLoadEditor
			// 
			this.btnLoadEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.btnLoadEditor.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnLoadEditor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLoadEditor.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnLoadEditor.ForeColor = System.Drawing.Color.White;
			this.btnLoadEditor.Location = new System.Drawing.Point(210, 6);
			this.btnLoadEditor.Name = "btnLoadEditor";
			this.btnLoadEditor.Size = new System.Drawing.Size(95, 30);
			this.btnLoadEditor.TabIndex = 2;
			this.btnLoadEditor.Text = "修改工具";
			this.btnLoadEditor.UseVisualStyleBackColor = false;
			// 
			// btnSaveVpp
			// 
			this.btnSaveVpp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(220)))));
			this.btnSaveVpp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(150)))), ((int)(((byte)(220)))));
			this.btnSaveVpp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSaveVpp.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnSaveVpp.ForeColor = System.Drawing.Color.White;
			this.btnSaveVpp.Location = new System.Drawing.Point(315, 6);
			this.btnSaveVpp.Name = "btnSaveVpp";
			this.btnSaveVpp.Size = new System.Drawing.Size(95, 30);
			this.btnSaveVpp.TabIndex = 3;
			this.btnSaveVpp.Text = "保存 VPP";
			this.btnSaveVpp.UseVisualStyleBackColor = false;
			// 
			// grpEditor
			// 
			this.grpEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(14)))), ((int)(((byte)(27)))));
			this.grpEditor.Controls.Add(this.panelEditorHost);
			this.grpEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grpEditor.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
			this.grpEditor.ForeColor = System.Drawing.Color.White;
			this.grpEditor.Location = new System.Drawing.Point(0, 0);
			this.grpEditor.Margin = new System.Windows.Forms.Padding(0);
			this.grpEditor.Name = "grpEditor";
			this.grpEditor.Padding = new System.Windows.Forms.Padding(12, 26, 12, 12);
			this.grpEditor.Size = new System.Drawing.Size(150, 46);
			this.grpEditor.TabIndex = 0;
			this.grpEditor.TabStop = false;
			this.grpEditor.Text = "VPP 编辑器";
			// 
			// panelEditorHost
			// 
			this.panelEditorHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(8)))), ((int)(((byte)(16)))));
			this.panelEditorHost.Controls.Add(this.lblEditorInfo);
			this.panelEditorHost.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelEditorHost.Location = new System.Drawing.Point(12, 52);
			this.panelEditorHost.Name = "panelEditorHost";
			this.panelEditorHost.Padding = new System.Windows.Forms.Padding(8);
			this.panelEditorHost.Size = new System.Drawing.Size(126, 0);
			this.panelEditorHost.TabIndex = 0;
			// 
			// lblEditorInfo
			// 
			this.lblEditorInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblEditorInfo.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblEditorInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(165)))), ((int)(((byte)(190)))));
			this.lblEditorInfo.Location = new System.Drawing.Point(8, 8);
			this.lblEditorInfo.Name = "lblEditorInfo";
			this.lblEditorInfo.Size = new System.Drawing.Size(110, 0);
			this.lblEditorInfo.TabIndex = 0;
			this.lblEditorInfo.Text = "请选择 Job、Task 和 VPP。";
			this.lblEditorInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// AlgorithmModuleControl
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(10)))), ((int)(((byte)(20)))));
			this.Controls.Add(this.rootLayout);
			this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.Name = "AlgorithmModuleControl";
			this.Size = new System.Drawing.Size(1903, 933);
			this.rootLayout.ResumeLayout(false);
			this.panelLibrary.ResumeLayout(false);
			this.cardVM.ResumeLayout(false);
			this.cardVMLayout.ResumeLayout(false);
			this.cardHdev.ResumeLayout(false);
			this.cardHdevLayout.ResumeLayout(false);
			this.cardScript.ResumeLayout(false);
			this.cardScriptLayout.ResumeLayout(false);
			this.cardVpp.ResumeLayout(false);
			this.cardVppLayout.ResumeLayout(false);
			this.jobTaskLayout.ResumeLayout(false);
			this.grpJobs.ResumeLayout(false);
			this.grpTasks.ResumeLayout(false);
			this.grpFiles.ResumeLayout(false);
			this.splitRight.Panel1.ResumeLayout(false);
			this.splitRight.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
			this.splitRight.ResumeLayout(false);
			this.grpPins.ResumeLayout(false);
			this.pinLayout.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvPins)).EndInit();
			this.panelPinButtons.ResumeLayout(false);
			this.grpEditor.ResumeLayout(false);
			this.panelEditorHost.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private Panel cardVM;
		private TableLayoutPanel cardVMLayout;
		private Label lblEnableVM;
		private Panel gapLibrary3;
		private Panel cardHdev;
		private TableLayoutPanel cardHdevLayout;
		private Label lblEnableHdev;
		private Panel gapLibrary2;
		private Panel cardScript;
		private TableLayoutPanel cardScriptLayout;
		private Label lblEnableScript;
		private Panel gapLibrary1;
		private Panel cardVpp;
		private TableLayoutPanel cardVppLayout;
		private Label lblEnableVpp;
		private Panel gapRoot1;
		private TableLayoutPanel jobTaskLayout;
		private Panel gapJobTask;
		private Panel gapRoot2;
		private Panel gapRoot3;
		private TableLayoutPanel pinLayout;
		private DataGridViewTextBoxColumn colDirection;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colDataType;
		private DataGridViewTextBoxColumn colValue;
		private DataGridViewButtonColumn colGlobalVariable;
	}
}
