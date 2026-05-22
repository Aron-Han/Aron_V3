using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class AlgorithmModuleControl
	{
		/// <summary>
		/// AlgorithmModuleControl layout.
		/// 控件布局已移动到 Designer 文件，后续需要调整布局时优先修改本文件。
		/// 注意：本 Designer 仍复用主类中的 CreatePanel/CreateGroupBox/CreateListBox 等工厂方法，
		/// 这样可以保持原有深色主题和样式一致。
		/// </summary>
		private void InitializeComponent()
		{
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);
			this.Dock = DockStyle.Fill;

			rootLayout = new TableLayoutPanel();
			rootLayout.Dock = DockStyle.Fill;
			rootLayout.BackColor = Color.FromArgb(2, 10, 20);
			rootLayout.Padding = new Padding(8, 10, 10, 10);
			rootLayout.Margin = new Padding(0);
			rootLayout.ColumnCount = 7;
			rootLayout.RowCount = 1;

			// 区域1：算法库。缩窄左侧区域，给 VPP 编辑区更多空间。
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
			// 间隔
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
			// 区域2、3：Job / Task。原来偏宽，这里缩窄。
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225F));
			// 间隔
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
			// 区域4：VPP 列表。缩窄。
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210F));
			// 间隔
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
			// 区域5、6
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			panelLibrary = CreatePanel();
			panelLibrary.Padding = new Padding(10, 14, 10, 10);

			btnVM = CreateLibraryButton("VM");
			btnHdev = CreateLibraryButton("Hdev");
			btnScript = CreateLibraryButton("Script");
			btnVpp = CreateLibraryButton("Vpp");

			chkEnableVpp = CreateEnableCheckBox(_moduleConfig.EnableVpp);
			chkEnableScript = CreateEnableCheckBox(_moduleConfig.EnableScript);
			chkEnableHdev = CreateEnableCheckBox(_moduleConfig.EnableHdev);
			chkEnableVM = CreateEnableCheckBox(_moduleConfig.EnableVM);

			Panel cardVpp = CreateLibraryCard(btnVpp, chkEnableVpp);
			Panel cardScript = CreateLibraryCard(btnScript, chkEnableScript);
			Panel cardHdev = CreateLibraryCard(btnHdev, chkEnableHdev);
			Panel cardVM = CreateLibraryCard(btnVM, chkEnableVM);

			// Dock=Top 显示顺序和 Add 顺序有关，这里倒序 Add。
			panelLibrary.Controls.Add(cardVM);
			panelLibrary.Controls.Add(CreateGapPanel(12));
			panelLibrary.Controls.Add(cardHdev);
			panelLibrary.Controls.Add(CreateGapPanel(12));
			panelLibrary.Controls.Add(cardScript);
			panelLibrary.Controls.Add(CreateGapPanel(12));
			panelLibrary.Controls.Add(cardVpp);

			btnVpp.Click += delegate { SelectLibrary(AlgorithmLibraryType.Vpp); };
			btnScript.Click += delegate { SelectLibrary(AlgorithmLibraryType.Script); };
			btnHdev.Click += delegate { SelectLibrary(AlgorithmLibraryType.Hdev); };
			btnVM.Click += delegate { SelectLibrary(AlgorithmLibraryType.VM); };

			chkEnableVpp.CheckedChanged += chkEnable_CheckedChanged;
			chkEnableScript.CheckedChanged += chkEnable_CheckedChanged;
			chkEnableHdev.CheckedChanged += chkEnable_CheckedChanged;
			chkEnableVM.CheckedChanged += chkEnable_CheckedChanged;

			TableLayoutPanel jobTaskLayout = new TableLayoutPanel();
			jobTaskLayout.Dock = DockStyle.Fill;
			jobTaskLayout.BackColor = Color.FromArgb(2, 10, 20);
			jobTaskLayout.ColumnCount = 1;
			jobTaskLayout.RowCount = 3;
			jobTaskLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			jobTaskLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
			jobTaskLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

			grpJobs = CreateGroupBox("所有 JobID");
			listJobs = CreateListBox();
			listJobs.DoubleClick += listJobs_DoubleClick;
			grpJobs.Controls.Add(listJobs);

			grpTasks = CreateGroupBox("所有 Task");
			listTasks = CreateListBox();
			listTasks.DoubleClick += listTasks_DoubleClick;
			grpTasks.Controls.Add(listTasks);

			jobTaskLayout.Controls.Add(grpJobs, 0, 0);
			jobTaskLayout.Controls.Add(CreatePlainGapPanel(), 0, 1);
			jobTaskLayout.Controls.Add(grpTasks, 0, 2);

			grpFiles = CreateGroupBox("所有 VPP");
			listAlgorithmFiles = CreateListBox();
			listAlgorithmFiles.DoubleClick += listAlgorithmFiles_DoubleClick;
			grpFiles.Controls.Add(listAlgorithmFiles);

			splitRight = new SplitContainer();
			splitRight.Dock = DockStyle.Fill;
			splitRight.Orientation = Orientation.Horizontal;
			splitRight.SplitterWidth = 1;
			splitRight.BackColor = Color.FromArgb(2, 10, 20);
			splitRight.Panel1.BackColor = Color.FromArgb(2, 10, 20);
			splitRight.Panel2.BackColor = Color.FromArgb(2, 10, 20);
			splitRight.IsSplitterFixed = true;
			// CogToolBlockEditV2 已经改为独立弹窗显示，主界面不再显示底部 VPP 编辑器区域。
			splitRight.Panel2Collapsed = true;

			grpPins = CreateGroupBox("输入/输出引脚");

			TableLayoutPanel pinLayout = new TableLayoutPanel();
			pinLayout.Dock = DockStyle.Fill;
			pinLayout.Margin = new Padding(0);
			pinLayout.Padding = new Padding(0);
			pinLayout.BackColor = Color.FromArgb(3, 14, 27);
			pinLayout.ColumnCount = 1;
			pinLayout.RowCount = 2;
			pinLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			pinLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

			dgvPins = CreatePinsGrid();

			panelPinButtons = new Panel();
			panelPinButtons.Dock = DockStyle.Fill;
			panelPinButtons.BackColor = Color.FromArgb(3, 14, 27);

			btnApplyInputs = CreateSmallActionButton("应用输入", 0, 6, 95);
			btnRunReplay = CreateSmallActionButton("回放运行", 105, 6, 95);
			btnSaveVpp = CreateSmallActionButton("保存 VPP", 210, 6, 95);
			btnSaveVpp.BackColor = Color.FromArgb(0, 95, 220);

			btnApplyInputs.Click += btnApplyInputs_Click;
			btnRunReplay.Click += btnRunReplay_Click;
			btnSaveVpp.Click += btnSaveVpp_Click;

			panelPinButtons.Controls.Add(btnApplyInputs);
			panelPinButtons.Controls.Add(btnRunReplay);
			panelPinButtons.Controls.Add(btnSaveVpp);

			pinLayout.Controls.Add(dgvPins, 0, 0);
			pinLayout.Controls.Add(panelPinButtons, 0, 1);
			grpPins.Controls.Add(pinLayout);
			vppPinContent = pinLayout;

			grpEditor = CreateGroupBox("VPP 编辑器");
			panelEditorHost = new Panel();
			panelEditorHost.Dock = DockStyle.Fill;
			panelEditorHost.BackColor = Color.FromArgb(1, 8, 16);
			panelEditorHost.Padding = new Padding(8);

			lblEditorInfo = new Label();
			lblEditorInfo.Dock = DockStyle.Fill;
			lblEditorInfo.TextAlign = ContentAlignment.MiddleCenter;
			lblEditorInfo.ForeColor = Color.FromArgb(140, 165, 190);
			lblEditorInfo.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
			lblEditorInfo.Text = "请选择 Job、Task 和 VPP。";

			panelEditorHost.Controls.Add(lblEditorInfo);
			grpEditor.Controls.Add(panelEditorHost);

			splitRight.Panel1.Controls.Add(grpPins);
			// 主界面不再嵌入 VPP 编辑器，保留对象但不显示。
			splitRight.Panel2.Controls.Add(grpEditor);
			splitRight.Panel2Collapsed = true;

			rootLayout.Controls.Add(panelLibrary, 0, 0);
			rootLayout.Controls.Add(CreatePlainGapPanel(), 1, 0);
			rootLayout.Controls.Add(jobTaskLayout, 2, 0);
			rootLayout.Controls.Add(CreatePlainGapPanel(), 3, 0);
			rootLayout.Controls.Add(grpFiles, 4, 0);
			rootLayout.Controls.Add(CreatePlainGapPanel(), 5, 0);
			rootLayout.Controls.Add(splitRight, 6, 0);

			this.Controls.Add(rootLayout);

			ApplyLibraryEnabledState();

			AlgorithmLibraryType? firstEnabled = GetFirstEnabledLibrary();

			if (firstEnabled.HasValue)
			{
				SelectLibrary(firstEnabled.Value);
			}
			else
			{
				ShowNoEnabledModuleMessage();
			}
		}
	}
}
