using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ImageFile;

namespace Aron_V3
{
	/// <summary>
	/// 算法模块配置页第一版。
	/// 
	/// 区域说明：
	/// 1. 算法库选择：Vpp / Script / Hdev / VM
	/// 2. 当前工程 JobID 列表
	/// 3. 当前 Job 下 Task 列表
	/// 4. 当前 Task 下对应算法文件列表
	/// 5. 当前 VPP 输入/输出引脚
	/// 6. VPP 编辑区域：优先嵌入 CogToolBlockEditV2，没有 VisionPro 环境时显示占位信息
	/// 
	/// 注意：
	/// 该控件不直接引用 Cognex VisionPro DLL，避免没有添加引用时编译失败。
	/// 它通过反射尝试加载 Cognex 控件和 VPP 文件。
	/// </summary>
	public partial class AlgorithmModuleControl : UserControl, ILocalizable
	{
		private enum AlgorithmLibraryType
		{
			Vpp,
			Script,
			Hdev,
			VM
		}


		private AlgorithmLibraryType _currentLibrary = AlgorithmLibraryType.Vpp;
		private bool _isEnglish = false;

		private TableLayoutPanel rootLayout;

		private Panel panelLibrary;
		private Button btnVpp;
		private Button btnScript;
		private Button btnHdev;
		private Button btnVM;

		private CheckBox chkEnableVpp;
		private CheckBox chkEnableScript;
		private CheckBox chkEnableHdev;
		private CheckBox chkEnableVM;

		private AlgorithmModuleConfig _moduleConfig;
		private bool _preloadStarted = false;
		private SynchronizationContext _uiContext;
		private readonly Dictionary<string, bool> _openedDetachedEditors = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, object> _preloadedToolBlocks = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, Control> _preloadedEditors = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase);

		private GroupBox grpJobs;
		private ListBox listJobs;

		private GroupBox grpTasks;
		private ListBox listTasks;

		private GroupBox grpFiles;
		private ListBox listAlgorithmFiles;

		private SplitContainer splitRight;
		private GroupBox grpPins;
		private DataGridView dgvPins;
		private Panel panelPinButtons;
		private Button btnApplyInputs;
		private Button btnRunReplay;
		private Button btnSaveVpp;

		private GroupBox grpEditor;
		private Panel panelEditorHost;
		private Label lblEditorInfo;
		private Button btnLoadEditor;

		private object _currentToolBlock;
		private Control _currentVisionProEditor;
		private bool _loadingPins = false;
		private bool _loadingVpp = false;

		private string _currentJobName = string.Empty;
		private string _currentTaskName = string.Empty;
		private string _currentAlgorithmName = string.Empty;
		private string _currentAlgorithmPath = string.Empty;
		private string _currentProjectSavePath = string.Empty;
		private AlgorithmFileItem _currentAlgorithmItem;

		private CSharpScriptStepEditorControl _scriptEditor;
		private Control vppPinContent;
		private bool _showingScriptEditor = false;

		public AlgorithmModuleControl()
		{
			_uiContext = SynchronizationContext.Current;
			_moduleConfig = AlgorithmModuleConfigStore.LoadOrCreateDefault();

			InitializeComponent();

			// 设计器打开控件时不要执行加载 Job / 选择库 / 文件扫描等运行时逻辑，
			// 否则 WinForms Designer 会把这些语句误认为 InitializeComponent 的设计器代码。
			if (IsInDesignMode())
			{
				return;
			}

			EnableDoubleBuffer(this);
			BindRuntimeEvents();
			ApplyLibraryEnabledState();
			LoadJobs();

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

		private bool IsInDesignMode()
		{
			try
			{
				if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
				{
					return true;
				}

				Control c = this;
				while (c != null)
				{
					if (c.Site != null && c.Site.DesignMode)
					{
						return true;
					}

					c = c.Parent;
				}
			}
			catch
			{
			}

			return false;
		}

		private void BindRuntimeEvents()
		{
			if (btnVpp != null)
			{
				btnVpp.Click -= btnVpp_Click;
				btnVpp.Click += btnVpp_Click;
			}

			if (btnScript != null)
			{
				btnScript.Click -= btnScript_Click;
				btnScript.Click += btnScript_Click;
			}

			if (btnHdev != null)
			{
				btnHdev.Click -= btnHdev_Click;
				btnHdev.Click += btnHdev_Click;
			}

			if (btnVM != null)
			{
				btnVM.Click -= btnVM_Click;
				btnVM.Click += btnVM_Click;
			}

			if (chkEnableVpp != null)
			{
				chkEnableVpp.CheckedChanged -= chkEnable_CheckedChanged;
				chkEnableVpp.CheckedChanged += chkEnable_CheckedChanged;
			}

			if (chkEnableScript != null)
			{
				chkEnableScript.CheckedChanged -= chkEnable_CheckedChanged;
				chkEnableScript.CheckedChanged += chkEnable_CheckedChanged;
			}

			if (chkEnableHdev != null)
			{
				chkEnableHdev.CheckedChanged -= chkEnable_CheckedChanged;
				chkEnableHdev.CheckedChanged += chkEnable_CheckedChanged;
			}

			if (chkEnableVM != null)
			{
				chkEnableVM.CheckedChanged -= chkEnable_CheckedChanged;
				chkEnableVM.CheckedChanged += chkEnable_CheckedChanged;
			}

			if (listJobs != null)
			{
				listJobs.DoubleClick -= listJobs_DoubleClick;
				listJobs.DoubleClick += listJobs_DoubleClick;
			}

			if (listTasks != null)
			{
				listTasks.DoubleClick -= listTasks_DoubleClick;
				listTasks.DoubleClick += listTasks_DoubleClick;
			}

			if (listAlgorithmFiles != null)
			{
				listAlgorithmFiles.DoubleClick -= listAlgorithmFiles_DoubleClick;
				listAlgorithmFiles.DoubleClick += listAlgorithmFiles_DoubleClick;
			}

			if (btnApplyInputs != null)
			{
				btnApplyInputs.Click -= btnApplyInputs_Click;
				btnApplyInputs.Click += btnApplyInputs_Click;
			}

			if (btnRunReplay != null)
			{
				btnRunReplay.Click -= btnRunReplay_Click;
				btnRunReplay.Click += btnRunReplay_Click;
			}

			if (btnSaveVpp != null)
			{
				btnSaveVpp.Click -= btnSaveVpp_Click;
				btnSaveVpp.Click += btnSaveVpp_Click;
			}

			if (btnLoadEditor != null)
			{
				btnLoadEditor.Click -= btnLoadEditor_Click;
				btnLoadEditor.Click += btnLoadEditor_Click;
			}
		}

		private void btnVpp_Click(object sender, EventArgs e)
		{
			SelectLibrary(AlgorithmLibraryType.Vpp);
		}

		private void btnScript_Click(object sender, EventArgs e)
		{
			SelectLibrary(AlgorithmLibraryType.Script);
		}

		private void btnHdev_Click(object sender, EventArgs e)
		{
			SelectLibrary(AlgorithmLibraryType.Hdev);
		}

		private void btnVM_Click(object sender, EventArgs e)
		{
			SelectLibrary(AlgorithmLibraryType.VM);
		}



		// 不再在控件创建/页面切换时自动预加载，避免点击“算法模块”时卡住主界面。
		// 真正的软件启动预加载，请在 Form1 启动完成后延迟调用 StartPreloadIfNeeded()。
		private void AlgorithmModuleControl_HandleCreated(object sender, EventArgs e)
		{
		}

		public void StartPreloadIfNeeded()
		{
			if (_preloadStarted)
			{
				return;
			}

			_preloadStarted = true;

			if (_moduleConfig != null && _moduleConfig.EnableVpp)
			{
				PreloadVppEditorsAsync();
			}
		}


		// UI layout moved to AlgorithmModuleControl.Designer.cs for easier visual/layout editing.


		private Panel CreatePanel()
		{
			Panel panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.Margin = new Padding(0);
			panel.BackColor = Color.FromArgb(3, 14, 27);
			panel.BorderStyle = BorderStyle.FixedSingle;
			return panel;
		}

		private Panel CreatePlainGapPanel()
		{
			Panel panel = new Panel();
			panel.Dock = DockStyle.Fill;
			panel.Margin = new Padding(0);
			panel.BackColor = Color.FromArgb(2, 10, 20);
			return panel;
		}

		private Panel CreateGapPanel(int height)
		{
			Panel panel = new Panel();
			panel.Dock = DockStyle.Top;
			panel.Height = height;
			panel.Margin = new Padding(0);
			panel.BackColor = Color.FromArgb(3, 14, 27);
			return panel;
		}

		private Button CreateLibraryButton(string text)
		{
			Button btn = new Button();
			btn.Dock = DockStyle.Top;
			btn.Height = 50;
			btn.Margin = new Padding(0);
			btn.Text = text;
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 1;
			btn.FlatAppearance.BorderColor = Color.FromArgb(35, 65, 95);
			btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
			btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
			btn.BackColor = Color.FromArgb(8, 21, 39);
			btn.ForeColor = Color.FromArgb(210, 220, 235);
			btn.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private Button CreateSmallActionButton(string text, int x, int y, int width)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(width, 30);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 1;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
			btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
			btn.BackColor = Color.FromArgb(3, 14, 27);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.TextAlign = ContentAlignment.MiddleCenter;
			btn.UseVisualStyleBackColor = false;
			return btn;
		}


		private CheckBox CreateEnableCheckBox(bool isChecked)
		{
			CheckBox chk = new CheckBox();
			chk.Dock = DockStyle.Fill;
			chk.Margin = new Padding(0);
			chk.Padding = new Padding(0);
			chk.Text = string.Empty;
			chk.Checked = isChecked;
			chk.ForeColor = Color.FromArgb(210, 230, 245);
			chk.BackColor = Color.Transparent;
			chk.TextAlign = ContentAlignment.MiddleCenter;
			chk.CheckAlign = ContentAlignment.MiddleCenter;
			chk.Cursor = Cursors.Hand;
			chk.UseVisualStyleBackColor = false;
			return chk;
		}

		private Panel CreateLibraryCard(Button btn, CheckBox chk)
		{
			Panel panel = new Panel();
			panel.Dock = DockStyle.Top;
			panel.Height = 50;
			panel.Margin = new Padding(0);
			panel.Padding = new Padding(0);
			panel.BackColor = Color.FromArgb(8, 21, 39);
			panel.BorderStyle = BorderStyle.None;

			TableLayoutPanel layout = new TableLayoutPanel();
			layout.Dock = DockStyle.Fill;
			layout.Margin = new Padding(0);
			layout.Padding = new Padding(0);
			layout.BackColor = Color.FromArgb(8, 21, 39);
			layout.ColumnCount = 3;
			layout.RowCount = 1;
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 22F));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

			if (btn != null)
			{
				btn.Dock = DockStyle.Fill;
				btn.Margin = new Padding(0);
				layout.Controls.Add(btn, 0, 0);
			}

			Label lblEnable = new Label();
			lblEnable.Dock = DockStyle.Fill;
			lblEnable.Margin = new Padding(0);
			lblEnable.Text = "启用";
			lblEnable.TextAlign = ContentAlignment.MiddleRight;
			lblEnable.ForeColor = Color.FromArgb(190, 210, 230);
			lblEnable.BackColor = Color.Transparent;
			lblEnable.Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold);
			layout.Controls.Add(lblEnable, 1, 0);

			if (chk != null)
			{
				chk.Dock = DockStyle.Fill;
				chk.Margin = new Padding(0);
				layout.Controls.Add(chk, 2, 0);
			}

			panel.Controls.Add(layout);
			return panel;
		}

		private GroupBox CreateGroupBox(string text)
		{
			GroupBox grp = new GroupBox();
			grp.Dock = DockStyle.Fill;
			grp.Margin = new Padding(0);
			grp.Padding = new Padding(12, 26, 12, 12);
			grp.Text = text;
			grp.ForeColor = Color.White;
			grp.BackColor = Color.FromArgb(3, 14, 27);
			grp.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			return grp;
		}

		private ListBox CreateListBox()
		{
			ListBox list = new ListBox();
			list.Dock = DockStyle.Fill;
			list.BorderStyle = BorderStyle.FixedSingle;
			list.BackColor = Color.FromArgb(1, 8, 16);
			list.ForeColor = Color.White;
			list.Font = new Font("Microsoft YaHei UI", 9F);
			list.ItemHeight = 22;
			list.IntegralHeight = false;
			return list;
		}

		private DataGridView CreatePinsGrid()
		{
			DataGridView dgv = new DataGridView();
			dgv.Dock = DockStyle.Fill;
			dgv.AllowUserToAddRows = false;
			dgv.AllowUserToDeleteRows = false;
			dgv.RowHeadersVisible = false;
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgv.MultiSelect = false;
			dgv.BackgroundColor = Color.FromArgb(1, 8, 16);
			dgv.GridColor = Color.FromArgb(45, 70, 95);
			dgv.BorderStyle = BorderStyle.None;
			dgv.EnableHeadersVisualStyles = false;
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

			dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);

			dgv.DefaultCellStyle.BackColor = Color.FromArgb(1, 8, 16);
			dgv.DefaultCellStyle.ForeColor = Color.White;
			dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			dgv.DefaultCellStyle.SelectionForeColor = Color.White;
			dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

			DataGridViewTextBoxColumn colDirection = new DataGridViewTextBoxColumn();
			colDirection.Name = "colDirection";
			colDirection.HeaderText = "类型";
			colDirection.FillWeight = 60;
			colDirection.ReadOnly = true;

			DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
			colName.Name = "colName";
			colName.HeaderText = "引脚名称";
			colName.FillWeight = 130;
			colName.ReadOnly = true;

			DataGridViewTextBoxColumn colDataType = new DataGridViewTextBoxColumn();
			colDataType.Name = "colDataType";
			colDataType.HeaderText = "数据类型";
			colDataType.FillWeight = 110;
			colDataType.ReadOnly = true;

			DataGridViewTextBoxColumn colValue = new DataGridViewTextBoxColumn();
			colValue.Name = "colValue";
			colValue.HeaderText = "当前值 / 自定义值";
			colValue.FillWeight = 180;
			colValue.ReadOnly = false;

			DataGridViewButtonColumn colBrowse = new DataGridViewButtonColumn();
			colBrowse.Name = "colBrowse";
			colBrowse.HeaderText = "文件";
			colBrowse.Text = "...";
			colBrowse.UseColumnTextForButtonValue = true;
			colBrowse.FillWeight = 50;

			DataGridViewTextBoxColumn colRemark = new DataGridViewTextBoxColumn();
			colRemark.Name = "colRemark";
			colRemark.HeaderText = "说明";
			colRemark.FillWeight = 140;
			colRemark.ReadOnly = true;

			dgv.Columns.Add(colDirection);
			dgv.Columns.Add(colName);
			dgv.Columns.Add(colDataType);
			dgv.Columns.Add(colValue);
			dgv.Columns.Add(colBrowse);
			dgv.Columns.Add(colRemark);

			dgv.CellContentClick += dgvPins_CellContentClick;
			dgv.CellEndEdit += dgvPins_CellEndEdit;
			dgv.CurrentCellDirtyStateChanged += dgvPins_CurrentCellDirtyStateChanged;
			dgv.DataError += delegate (object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };

			return dgv;
		}

		private bool IsLibraryEnabled(AlgorithmLibraryType library)
		{
			if (library == AlgorithmLibraryType.Vpp)
			{
				return chkEnableVpp != null && chkEnableVpp.Checked;
			}

			if (library == AlgorithmLibraryType.Script)
			{
				return chkEnableScript != null && chkEnableScript.Checked;
			}

			if (library == AlgorithmLibraryType.Hdev)
			{
				return chkEnableHdev != null && chkEnableHdev.Checked;
			}

			if (library == AlgorithmLibraryType.VM)
			{
				return chkEnableVM != null && chkEnableVM.Checked;
			}

			return false;
		}

		private void ApplyLibraryEnabledState()
		{
			ApplySingleLibraryEnabledState(btnVpp, chkEnableVpp != null && chkEnableVpp.Checked, _currentLibrary == AlgorithmLibraryType.Vpp);
			ApplySingleLibraryEnabledState(btnScript, chkEnableScript != null && chkEnableScript.Checked, _currentLibrary == AlgorithmLibraryType.Script);
			ApplySingleLibraryEnabledState(btnHdev, chkEnableHdev != null && chkEnableHdev.Checked, _currentLibrary == AlgorithmLibraryType.Hdev);
			ApplySingleLibraryEnabledState(btnVM, chkEnableVM != null && chkEnableVM.Checked, _currentLibrary == AlgorithmLibraryType.VM);
		}

		private void ApplySingleLibraryEnabledState(Button btn, bool enabled, bool selected)
		{
			if (btn == null)
			{
				return;
			}

			// 注意：
			// 不直接 btn.Enabled = false。
			// WinForms Button 禁用后会使用系统禁用绘制，深色主题下容易变成黑框、文字不可见。
			// 这里保持 Enabled=true，只通过 SelectLibrary 的逻辑阻止点击。
			btn.Enabled = true;
			btn.Tag = enabled ? "Enabled" : "Disabled";

			if (!enabled)
			{
				btn.BackColor = Color.FromArgb(28, 34, 42);
				btn.ForeColor = Color.FromArgb(145, 155, 165);
				btn.FlatAppearance.BorderColor = Color.FromArgb(45, 55, 65);
				btn.Cursor = Cursors.No;
				return;
			}

			btn.Cursor = Cursors.Hand;

			if (selected)
			{
				btn.BackColor = Color.FromArgb(20, 70, 135);
				btn.ForeColor = Color.White;
				btn.FlatAppearance.BorderColor = Color.FromArgb(0, 185, 255);
			}
			else
			{
				btn.BackColor = Color.FromArgb(8, 21, 39);
				btn.ForeColor = Color.FromArgb(210, 220, 235);
				btn.FlatAppearance.BorderColor = Color.FromArgb(35, 65, 95);
			}
		}

		private AlgorithmLibraryType? GetFirstEnabledLibrary()
		{
			if (chkEnableVpp != null && chkEnableVpp.Checked) return AlgorithmLibraryType.Vpp;
			if (chkEnableScript != null && chkEnableScript.Checked) return AlgorithmLibraryType.Script;
			if (chkEnableHdev != null && chkEnableHdev.Checked) return AlgorithmLibraryType.Hdev;
			if (chkEnableVM != null && chkEnableVM.Checked) return AlgorithmLibraryType.VM;
			return null;
		}

		private void ShowNoEnabledModuleMessage()
		{
			listJobs.Items.Clear();
			listTasks.Items.Clear();
			listAlgorithmFiles.Items.Clear();

			if (_currentLibrary != AlgorithmLibraryType.Script)
			{
				dgvPins.Rows.Clear();
				ClearVppEditor();
			}
			else
			{
				ShowScriptEditorForCurrentSelection(null, false);
			}

			if (grpFiles != null) grpFiles.Text = "未启用模块";
			if (grpPins != null) grpPins.Text = "参数";
			if (grpEditor != null) grpEditor.Text = "编辑器";

			if (lblEditorInfo != null)
			{
				lblEditorInfo.Text =
					"当前没有启用任何算法模块。" + Environment.NewLine +
					"请先勾选左侧 Vpp / Script / Hdev / VM 的启用项。";
			}
		}

		private void chkEnable_CheckedChanged(object sender, EventArgs e)
		{
			if (_moduleConfig == null)
			{
				_uiContext = SynchronizationContext.Current;
				_moduleConfig = AlgorithmModuleConfigStore.LoadOrCreateDefault();
			}

			if (chkEnableVpp != null) _moduleConfig.EnableVpp = chkEnableVpp.Checked;
			if (chkEnableScript != null) _moduleConfig.EnableScript = chkEnableScript.Checked;
			if (chkEnableHdev != null) _moduleConfig.EnableHdev = chkEnableHdev.Checked;
			if (chkEnableVM != null) _moduleConfig.EnableVM = chkEnableVM.Checked;

			AlgorithmModuleConfigStore.Save(_moduleConfig);

			ApplyLibraryEnabledState();

			// 如果当前选择的库被禁用，自动切换到第一个启用的库。
			if (!IsLibraryEnabled(_currentLibrary))
			{
				AlgorithmLibraryType? first = GetFirstEnabledLibrary();

				if (first.HasValue)
				{
					SelectLibrary(first.Value);
				}
				else
				{
					ShowNoEnabledModuleMessage();
				}
			}

			// 勾选 VPP 启用后，立即启动一次预加载。
			if (sender == chkEnableVpp && chkEnableVpp != null && chkEnableVpp.Checked)
			{
				_preloadStarted = false;
				StartPreloadIfNeeded();
			}
		}

		private void SelectLibrary(AlgorithmLibraryType library)
		{
			if (!IsLibraryEnabled(library))
			{
				ApplyLibraryEnabledState();

				if (_currentLibrary == library)
				{
					AlgorithmLibraryType? first = GetFirstEnabledLibrary();

					if (first.HasValue)
					{
						SelectLibrary(first.Value);
					}
					else
					{
						ShowNoEnabledModuleMessage();
					}
				}

				return;
			}

			_currentLibrary = library;
			ApplyLibraryEnabledState();

			if (library == AlgorithmLibraryType.Vpp)
			{
				grpFiles.Text = "所有 VPP";
				grpPins.Text = "VPP 输入/输出引脚";
				grpEditor.Text = "VPP 编辑器";

				if (lblEditorInfo != null)
				{
					lblEditorInfo.Text = "请选择 Job、Task 和 VPP。";
				}

				// 关键修复：从 Script 页面切回 VPP 时，必须强制把 Script 编辑器从 splitRight.Panel1 移除，
				// 并把 VPP 的 grpPins / dgvPins 放回右侧主区域。
				ShowVppPinPanel(true);
				ClearVppEditor();
				LoadAlgorithmFilesForCurrentTask();
				return;
			}

			if (library == AlgorithmLibraryType.Script)
			{
				grpFiles.Text = "所有 Script";
				grpPins.Text = "C# Script 编辑器";
				grpEditor.Text = "Script 编辑器";

				if (lblEditorInfo != null)
				{
					lblEditorInfo.Text = "请选择 Job、Task 和 Script。";
				}

				ShowScriptEditorForCurrentSelection(null, false);
				LoadAlgorithmFilesForCurrentTask();
				return;
			}

			if (library == AlgorithmLibraryType.Hdev)
			{
				grpFiles.Text = "所有 Hdev";
				grpPins.Text = "Hdev 参数";
				grpEditor.Text = "HDevelop 编辑器";

				if (lblEditorInfo != null)
				{
					lblEditorInfo.Text = "Hdev 模式后续扩展。";
				}

				ShowVppPinPanel(true);
				ClearVppEditor();
				LoadAlgorithmFilesForCurrentTask();
				return;
			}

			grpFiles.Text = "所有 VM";
			grpPins.Text = "VM 参数";
			grpEditor.Text = "VisionMaster 编辑器";

			if (lblEditorInfo != null)
			{
				lblEditorInfo.Text = "VM 模式后续扩展。";
			}

			ShowVppPinPanel(true);
			ClearVppEditor();
			LoadAlgorithmFilesForCurrentTask();
		}

		private void ApplyLibraryButtonStyle(Button btn, bool selected)
		{
			if (selected)
			{
				btn.BackColor = Color.FromArgb(20, 70, 135);
				btn.ForeColor = Color.White;
				btn.FlatAppearance.BorderColor = Color.FromArgb(0, 185, 255);
			}
			else
			{
				btn.BackColor = Color.FromArgb(8, 21, 39);
				btn.ForeColor = Color.FromArgb(210, 220, 235);
				btn.FlatAppearance.BorderColor = Color.FromArgb(35, 65, 95);
			}
		}

		public void RefreshProjectFlow()
		{
			string selectedJob = _currentJobName;
			string selectedTask = _currentTaskName;

			LoadJobs();

			if (!string.IsNullOrEmpty(selectedJob) && listJobs.Items.Contains(selectedJob))
			{
				listJobs.SelectedItem = selectedJob;
				_currentJobName = selectedJob;
				LoadTasks(selectedJob);
			}

			if (!string.IsNullOrEmpty(selectedTask) && listTasks.Items.Contains(selectedTask))
			{
				listTasks.SelectedItem = selectedTask;
				_currentTaskName = selectedTask;
				LoadAlgorithmFilesForCurrentTask();
			}
		}

		private void LoadJobs()
		{
			listJobs.Items.Clear();
			listTasks.Items.Clear();
			listAlgorithmFiles.Items.Clear();
			dgvPins.Rows.Clear();
			ClearVppEditor();

			try
			{
				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

				if (config == null || config.Jobs == null)
				{
					return;
				}

				foreach (JobConfig job in config.Jobs)
				{
					if (job == null || string.IsNullOrEmpty(job.JobName))
					{
						continue;
					}

					listJobs.Items.Add(job.JobName);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Load Job failed: " + ex.Message, "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void listJobs_DoubleClick(object sender, EventArgs e)
		{
			if (listJobs.SelectedItem == null)
			{
				return;
			}

			_currentJobName = listJobs.SelectedItem.ToString();
			LoadTasks(_currentJobName);
		}

		private void LoadTasks(string jobName)
		{
			listTasks.Items.Clear();
			listAlgorithmFiles.Items.Clear();
			dgvPins.Rows.Clear();
			ClearVppEditor();

			if (string.IsNullOrEmpty(jobName))
			{
				return;
			}

			try
			{
				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
				JobConfig job = config.Jobs.FirstOrDefault(j =>
					string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

				if (job == null || job.Tasks == null)
				{
					return;
				}

				foreach (TaskConfig task in job.Tasks.OrderBy(t => t.RunOrder))
				{
					if (task == null || string.IsNullOrEmpty(task.TaskName))
					{
						continue;
					}

					listTasks.Items.Add(task.TaskName);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Load Task failed: " + ex.Message, "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void listTasks_DoubleClick(object sender, EventArgs e)
		{
			if (listTasks.SelectedItem == null)
			{
				return;
			}

			_currentTaskName = listTasks.SelectedItem.ToString();
			LoadAlgorithmFilesForCurrentTask();
		}

		private void LoadAlgorithmFilesForCurrentTask()
		{
			if (listAlgorithmFiles == null)
			{
				return;
			}

			listAlgorithmFiles.BeginUpdate();

			try
			{
				listAlgorithmFiles.Items.Clear();

				if (_currentLibrary == AlgorithmLibraryType.Script)
				{
					// Script 模式下只显示编辑器壳，不自动加载第一个脚本。
					ShowScriptEditorForCurrentSelection(null, false);
				}
				else
				{
					// VPP/Hdev/VM 模式必须强制恢复参数区域，避免右侧残留 Script 编辑器。
					ShowVppPinPanel(false);

					if (dgvPins != null)
					{
						dgvPins.Rows.Clear();
					}

					ClearVppEditor();
				}

				if (string.IsNullOrEmpty(_currentJobName) || string.IsNullOrEmpty(_currentTaskName))
				{
					return;
				}

				List<AlgorithmFileItem> items = new List<AlgorithmFileItem>();

				if (_currentLibrary == AlgorithmLibraryType.Vpp)
				{
					items = LoadVppFilesFromFlowConfig(_currentJobName, _currentTaskName);
				}
				else if (_currentLibrary == AlgorithmLibraryType.Script)
				{
					items = LoadScriptFilesFromFlowConfig(_currentJobName, _currentTaskName);
				}
				else
				{
					if (lblEditorInfo != null)
					{
						lblEditorInfo.Text = "当前库模式后续扩展。";
					}

					return;
				}

				foreach (AlgorithmFileItem item in items)
				{
					listAlgorithmFiles.Items.Add(item);
				}

				if (items.Count == 0)
				{
					if (_currentLibrary == AlgorithmLibraryType.Vpp)
					{
						if (lblEditorInfo != null)
						{
							lblEditorInfo.Text = "当前 Task 下没有 VPP。";
						}
					}
					else if (_currentLibrary == AlgorithmLibraryType.Script)
					{
						if (_scriptEditor != null && !_scriptEditor.IsDisposed)
						{
							_scriptEditor.LoadScriptStep(_currentJobName, _currentTaskName, string.Empty);
						}

						if (lblEditorInfo != null)
						{
							lblEditorInfo.Text = "当前 Task 下没有 Script。";
						}
					}
				}
				else if (_currentLibrary == AlgorithmLibraryType.Script)
				{
					// 只选中，不自动加载。用户双击哪个脚本，就加载哪个脚本。
					listAlgorithmFiles.SelectedIndex = -1;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Load algorithm files failed: " + ex.Message, "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			finally
			{
				listAlgorithmFiles.EndUpdate();
			}
		}


		private List<AlgorithmFileItem> LoadVppFilesFromFlowConfig(string jobName, string taskName)
		{
			List<AlgorithmFileItem> result = new List<AlgorithmFileItem>();

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = config.Jobs.FirstOrDefault(j =>
				string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job == null || job.Tasks == null)
			{
				return result;
			}

			TaskConfig task = job.Tasks.FirstOrDefault(t =>
				string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase));

			if (task == null || task.Steps == null)
			{
				return result;
			}

			foreach (StepConfig step in task.Steps.OrderBy(s => s.RunOrder))
			{
				if (!IsVppStep(step))
				{
					continue;
				}

				string name = GetStepDisplayName(step);
				string path = GetStepVppPath(step, jobName, taskName);

				result.Add(new AlgorithmFileItem
				{
					Name = name,
					FilePath = path,
					Step = step,
					JobName = jobName,
					TaskName = taskName
				});
			}

			return result;
		}

		private List<AlgorithmFileItem> LoadScriptFilesFromFlowConfig(string jobName, string taskName)
		{
			List<AlgorithmFileItem> result = new List<AlgorithmFileItem>();

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = config.Jobs.FirstOrDefault(j =>
				string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job == null || job.Tasks == null)
			{
				return result;
			}

			TaskConfig task = job.Tasks.FirstOrDefault(t =>
				string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase));

			if (task == null || task.Steps == null)
			{
				return result;
			}

			foreach (StepConfig step in task.Steps.OrderBy(s => s.RunOrder))
			{
				if (!IsScriptStep(step))
				{
					continue;
				}

				string name = GetStepScriptName(step);
				string path = GetStepScriptPath(step, jobName, taskName);

				result.Add(new AlgorithmFileItem
				{
					Name = name,
					FilePath = path,
					Step = step,
					JobName = jobName,
					TaskName = taskName
				});
			}

			return result;
		}

		private bool IsScriptStep(StepConfig step)
		{
			if (step == null)
			{
				return false;
			}

			if (step.StepType == StepType.Script)
			{
				return true;
			}

			if (step.ScriptFiles != null && step.ScriptFiles.Count > 0)
			{
				return true;
			}

			string stepName = GetPropertyString(step, "StepName");
			string sourceFile = GetPropertyString(step, "SourceFilePath");
			string projectFile = GetPropertyString(step, "ProjectFilePath");

			if (EndsWithScript(stepName) || EndsWithScript(sourceFile) || EndsWithScript(projectFile))
			{
				return true;
			}

			return false;
		}

		private bool EndsWithScript(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}

			return text.EndsWith(".csx", StringComparison.OrdinalIgnoreCase) ||
				   text.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
				   text.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
				   text.EndsWith(".script.xml", StringComparison.OrdinalIgnoreCase);
		}

		private string GetStepScriptName(StepConfig step)
		{
			if (step == null)
			{
				return "CS_Script.csx";
			}

			if (step.ScriptFiles != null)
			{
				foreach (string file in step.ScriptFiles)
				{
					if (EndsWithScript(file) && !file.EndsWith(".script.xml", StringComparison.OrdinalIgnoreCase))
					{
						return Path.GetFileName(file);
					}
				}
			}

			string sourceFile = GetPropertyString(step, "SourceFilePath");

			if (EndsWithScript(sourceFile) && !sourceFile.EndsWith(".script.xml", StringComparison.OrdinalIgnoreCase))
			{
				return Path.GetFileName(sourceFile);
			}

			string projectFile = GetPropertyString(step, "ProjectFilePath");

			if (EndsWithScript(projectFile) && !projectFile.EndsWith(".script.xml", StringComparison.OrdinalIgnoreCase))
			{
				return Path.GetFileName(projectFile);
			}

			if (!string.IsNullOrWhiteSpace(step.StepName))
			{
				if (EndsWithScript(step.StepName))
				{
					return Path.GetFileName(step.StepName);
				}

				return step.StepName + ".csx";
			}

			return "CS_Script.csx";
		}

		private string GetStepScriptPath(StepConfig step, string jobName, string taskName)
		{
			string fileName = GetStepScriptName(step);

			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = "CS_Script.csx";
			}

			string taskFolder = Path.Combine(GetRuntimeProjectRoot(), "Job", jobName, "Task", taskName);
			string scriptsFolder = Path.Combine(taskFolder, "Scripts");

			string runtimePath = Path.Combine(scriptsFolder, Path.GetFileName(fileName));

			if (File.Exists(runtimePath))
			{
				return runtimePath;
			}

			// 兼容旧的 Script 单数目录。
			string oldScriptFolder = Path.Combine(taskFolder, "Script");
			string oldPath = Path.Combine(oldScriptFolder, Path.GetFileName(fileName));

			if (File.Exists(oldPath))
			{
				return oldPath;
			}

			if (step != null && step.ScriptFiles != null)
			{
				foreach (string file in step.ScriptFiles)
				{
					if (string.IsNullOrWhiteSpace(file))
					{
						continue;
					}

					if (Path.IsPathRooted(file) && File.Exists(file))
					{
						return file;
					}

					string p = Path.Combine(scriptsFolder, file);

					if (File.Exists(p))
					{
						return p;
					}
				}
			}

			string sourceFile = GetPropertyString(step, "SourceFilePath");

			if (File.Exists(sourceFile))
			{
				return sourceFile;
			}

			return runtimePath;
		}


		private bool IsVppStep(StepConfig step)
		{
			if (step == null)
			{
				return false;
			}

			string stepType = GetPropertyString(step, "StepType");
			string stepName = GetPropertyString(step, "StepName");
			string localFile = GetPropertyString(step, "LocalFilePath");
			string sourceFile = GetPropertyString(step, "SourceFilePath");
			string vppFile = GetPropertyString(step, "VppFilePath");

			if (stepType.IndexOf("Vpp", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}

			if (EndsWithVpp(stepName) || EndsWithVpp(localFile) || EndsWithVpp(sourceFile) || EndsWithVpp(vppFile))
			{
				return true;
			}

			return false;
		}

		private bool EndsWithVpp(string text)
		{
			return !string.IsNullOrEmpty(text) &&
				   text.EndsWith(".vpp", StringComparison.OrdinalIgnoreCase);
		}

		private string GetStepDisplayName(StepConfig step)
		{
			string stepName = GetPropertyString(step, "StepName");
			string localFile = GetPropertyString(step, "LocalFilePath");
			string sourceFile = GetPropertyString(step, "SourceFilePath");
			string vppFile = GetPropertyString(step, "VppFilePath");

			if (EndsWithVpp(localFile))
			{
				return Path.GetFileName(localFile);
			}

			if (EndsWithVpp(sourceFile))
			{
				return Path.GetFileName(sourceFile);
			}

			if (EndsWithVpp(vppFile))
			{
				return Path.GetFileName(vppFile);
			}

			if (!string.IsNullOrEmpty(stepName))
			{
				if (EndsWithVpp(stepName))
				{
					return stepName;
				}

				return stepName + ".vpp";
			}

			return "Unknown.vpp";
		}

		private string GetStepVppPath(StepConfig step, string jobName, string taskName)
		{
			string displayName = GetStepDisplayName(step);
			string fileName = Path.GetFileName(displayName);

			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = "Unknown.vpp";
			}

			if (!fileName.EndsWith(".vpp", StringComparison.OrdinalIgnoreCase))
			{
				fileName += ".vpp";
			}

			// 1. 优先使用当前运行目录 Project 下的工程内 VPP。
			// 这样保存后再次进入算法模块，会加载 Debug Project 内的文件。
			string runtimeProjectPath = Path.Combine(GetRuntimeProjectRoot(), "Steps", jobName, taskName, "VPP", fileName);

			if (File.Exists(runtimeProjectPath))
			{
				return runtimeProjectPath;
			}

			// 2. 再使用 XML 中记录的 LocalFilePath。
			string localFile = GetPropertyString(step, "LocalFilePath");

			if (File.Exists(localFile))
			{
				return localFile;
			}

			// 3. 再兼容旧版本字段。
			string vppFile = GetPropertyString(step, "VppFilePath");

			if (File.Exists(vppFile))
			{
				return vppFile;
			}

			string stepFolder = GetPropertyString(step, "StepFolder");

			if (!string.IsNullOrEmpty(stepFolder))
			{
				string path1 = Path.Combine(stepFolder, "VPP", fileName);

				if (File.Exists(path1))
				{
					return path1;
				}

				string path2 = Path.Combine(stepFolder, fileName);

				if (File.Exists(path2))
				{
					return path2;
				}
			}

			// 4. 最后才使用最初导入来源 SourceFilePath，比如 D:\Work\...
			// SourceFilePath 只作为导入源，不作为保存目标。
			string sourceFile = GetPropertyString(step, "SourceFilePath");

			if (File.Exists(sourceFile))
			{
				return sourceFile;
			}

			if (!string.IsNullOrEmpty(localFile))
			{
				return localFile;
			}

			if (!string.IsNullOrEmpty(vppFile))
			{
				return vppFile;
			}

			if (!string.IsNullOrEmpty(sourceFile))
			{
				return sourceFile;
			}

			return runtimeProjectPath;
		}

		private string GetPropertyString(object obj, string propertyName)
		{
			if (obj == null || string.IsNullOrEmpty(propertyName))
			{
				return string.Empty;
			}

			try
			{
				PropertyInfo p = obj.GetType().GetProperty(propertyName);

				if (p == null)
				{
					return string.Empty;
				}

				object value = p.GetValue(obj, null);

				if (value == null)
				{
					return string.Empty;
				}

				return value.ToString();
			}
			catch
			{
				return string.Empty;
			}
		}

		private AlgorithmFileItem RefreshAlgorithmFileItemFromConfig(AlgorithmFileItem oldItem)
		{
			if (oldItem == null)
			{
				return null;
			}

			List<AlgorithmFileItem> latestItems;

			if (_currentLibrary == AlgorithmLibraryType.Script)
			{
				latestItems = LoadScriptFilesFromFlowConfig(_currentJobName, _currentTaskName);
			}
			else
			{
				latestItems = LoadVppFilesFromFlowConfig(_currentJobName, _currentTaskName);
			}

			foreach (AlgorithmFileItem item in latestItems)
			{
				if (item == null)
				{
					continue;
				}

				if (string.Equals(item.Name, oldItem.Name, StringComparison.OrdinalIgnoreCase))
				{
					return item;
				}
			}

			return oldItem;
		}


		private async void listAlgorithmFiles_DoubleClick(object sender, EventArgs e)
		{
			if (_loadingVpp)
			{
				return;
			}

			if (listAlgorithmFiles.SelectedItem == null)
			{
				return;
			}

			AlgorithmFileItem item = listAlgorithmFiles.SelectedItem as AlgorithmFileItem;

			if (item == null)
			{
				return;
			}

			item = RefreshAlgorithmFileItemFromConfig(item);

			if (item == null)
			{
				return;
			}

			_currentAlgorithmName = item.Name;
			_currentAlgorithmPath = item.FilePath;
			_currentAlgorithmItem = item;

			if (_currentLibrary == AlgorithmLibraryType.Script)
			{
				ShowScriptEditorForCurrentSelection(item, true);
				return;
			}

			_currentProjectSavePath = GetRuntimeProjectVppPath(_currentJobName, _currentTaskName, item.Name);

			if (_currentLibrary == AlgorithmLibraryType.Vpp)
			{
				RestoreVppPinPanel();
			}

			if (_currentLibrary == AlgorithmLibraryType.Vpp)
			{
				if (TryShowPreloadedVpp(item))
				{
					return;
				}

				await LoadVppToEditorAsync(item);
			}
		}

		private void RestoreVppPinPanel()
		{
			ShowVppPinPanel(false);
		}

		/// <summary>
		/// 强制显示 VPP/Hdev/VM 参数区域。
		/// 这个方法专门解决：从 Script 编辑器切回 VPP 时，右侧仍停留在 Script 编辑器的问题。
		/// </summary>
		private void ShowVppPinPanel(bool clearPinRows)
		{
			if (splitRight == null || grpPins == null)
			{
				return;
			}

			SuspendControlRedraw(splitRight);
			SuspendControlRedraw(grpPins);

			try
			{
				splitRight.Panel2Collapsed = true;

				// 先把 Script 编辑器从右侧区域移除，否则它会继续占据 Panel1。
				if (_scriptEditor != null && !_scriptEditor.IsDisposed && _scriptEditor.Parent != null)
				{
					_scriptEditor.Parent.Controls.Remove(_scriptEditor);
				}

				// 强制把 VPP 参数 GroupBox 放回 splitRight.Panel1。
				if (grpPins.Parent != splitRight.Panel1)
				{
					if (grpPins.Parent != null)
					{
						grpPins.Parent.Controls.Remove(grpPins);
					}

					splitRight.Panel1.Controls.Clear();
					splitRight.Panel1.Controls.Add(grpPins);
				}
				else if (!splitRight.Panel1.Controls.Contains(grpPins))
				{
					splitRight.Panel1.Controls.Clear();
					splitRight.Panel1.Controls.Add(grpPins);
				}

				grpPins.Dock = DockStyle.Fill;
				grpPins.Visible = true;

				// 确保原来的 VPP pinLayout / dgvPins 没有被 Script 编辑器替换掉。
				if (vppPinContent != null)
				{
					if (vppPinContent.Parent != grpPins)
					{
						if (vppPinContent.Parent != null)
						{
							vppPinContent.Parent.Controls.Remove(vppPinContent);
						}

						grpPins.Controls.Clear();
						grpPins.Controls.Add(vppPinContent);
					}

					vppPinContent.Dock = DockStyle.Fill;
					vppPinContent.Visible = true;
				}

				if (dgvPins != null)
				{
					dgvPins.Visible = true;
					if (clearPinRows)
					{
						dgvPins.Rows.Clear();
					}
				}

				grpPins.BringToFront();
				_showingScriptEditor = false;
			}
			finally
			{
				ResumeControlRedraw(grpPins);
				ResumeControlRedraw(splitRight);
			}
		}


		private void ShowScriptEditorForCurrentSelection()
		{
			ShowScriptEditorForCurrentSelection(null, true);
		}

		private void ShowScriptEditorForCurrentSelection(AlgorithmFileItem selectedItem, bool loadScript)
		{
			string jobName = _currentJobName;
			string taskName = _currentTaskName;

			if (string.IsNullOrWhiteSpace(jobName) && listJobs != null && listJobs.SelectedItem != null)
			{
				jobName = listJobs.SelectedItem.ToString();
			}

			if (string.IsNullOrWhiteSpace(taskName) && listTasks != null && listTasks.SelectedItem != null)
			{
				taskName = listTasks.SelectedItem.ToString();
			}

			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			if (string.IsNullOrWhiteSpace(taskName))
			{
				taskName = "Task_New_01";
			}

			if (selectedItem == null)
			{
				selectedItem = _currentAlgorithmItem;

				if ((selectedItem == null || selectedItem.Step == null) &&
					listAlgorithmFiles != null &&
					listAlgorithmFiles.SelectedItem is AlgorithmFileItem)
				{
					selectedItem = (AlgorithmFileItem)listAlgorithmFiles.SelectedItem;
				}
			}

			if (_scriptEditor == null || _scriptEditor.IsDisposed)
			{
				_scriptEditor = new CSharpScriptStepEditorControl();
				_scriptEditor.Dock = DockStyle.Fill;
				_scriptEditor.ApplyLanguage(_isEnglish);
			}

			SuspendControlRedraw(splitRight);

			try
			{
				splitRight.Panel2Collapsed = true;

				// 如果当前已经显示的是同一个 ScriptEditor，不要反复 Clear/Add。
				// 这样能明显减少进入界面、切换脚本、写代码时的闪烁。
				if (_scriptEditor.Parent != splitRight.Panel1)
				{
					if (_scriptEditor.Parent != null)
					{
						_scriptEditor.Parent.Controls.Remove(_scriptEditor);
					}

					splitRight.Panel1.Controls.Clear();
					splitRight.Panel1.Controls.Add(_scriptEditor);
				}

				_scriptEditor.Dock = DockStyle.Fill;
				_scriptEditor.BringToFront();
				_showingScriptEditor = true;
			}
			finally
			{
				ResumeControlRedraw(splitRight);
			}

			if (!loadScript)
			{
				return;
			}

			string loadKey = GetScriptLoadKey(selectedItem);

			if (string.IsNullOrWhiteSpace(loadKey))
			{
				loadKey = selectedItem != null && !string.IsNullOrWhiteSpace(selectedItem.Name)
					? selectedItem.Name
					: string.Empty;
			}

			// 关键修复：
			// 这里必须传当前双击的脚本路径/名称，不能再固定传 CS_Script。
			_scriptEditor.LoadScriptStep(jobName, taskName, loadKey);
		}

		private string GetScriptLoadKey(AlgorithmFileItem item)
		{
			if (item == null)
			{
				return string.Empty;
			}

			if (!string.IsNullOrWhiteSpace(item.FilePath) && File.Exists(item.FilePath))
			{
				return item.FilePath;
			}

			if (item.Step != null)
			{
				if (item.Step.ScriptFiles != null && item.Step.ScriptFiles.Count > 0)
				{
					foreach (string file in item.Step.ScriptFiles)
					{
						if (string.IsNullOrWhiteSpace(file))
						{
							continue;
						}

						if (Path.IsPathRooted(file) && File.Exists(file))
						{
							return file;
						}
					}
				}

				if (!string.IsNullOrWhiteSpace(item.Step.ProjectFilePath))
				{
					return item.Step.ProjectFilePath;
				}

				if (!string.IsNullOrWhiteSpace(item.Step.SourceFilePath))
				{
					return item.Step.SourceFilePath;
				}

				if (!string.IsNullOrWhiteSpace(item.Step.StepName))
				{
					return item.Step.StepName;
				}
			}

			return item.Name;
		}



		private bool TryShowPreloadedVpp(AlgorithmFileItem item)
		{
			if (item == null || string.IsNullOrEmpty(item.FilePath))
			{
				return false;
			}

			object toolBlock;

			if (!_preloadedToolBlocks.TryGetValue(item.FilePath, out toolBlock) || toolBlock == null)
			{
				return false;
			}

			_currentAlgorithmName = item.Name;
			_currentAlgorithmPath = item.FilePath;
			_currentAlgorithmItem = item;
			_currentProjectSavePath = GetRuntimeProjectVppPath(_currentJobName, _currentTaskName, item.Name);
			_currentToolBlock = toolBlock;

			LoadPinsFromToolBlock(_currentToolBlock);
			ShowVppEditorPlaceholder(item.FilePath);
			OpenVppEditorDetached(_currentToolBlock, item.FilePath, GetRuntimeProjectVppPath(_currentJobName, _currentTaskName, item.Name));

			return true;
		}

		private async void PreloadVppEditorsAsync()
		{
			List<AlgorithmFileItem> allVppFiles = GetAllVppFilesFromProject();

			if (allVppFiles.Count <= 0)
			{
				return;
			}

			foreach (AlgorithmFileItem item in allVppFiles)
			{
				try
				{
					if (item == null || string.IsNullOrEmpty(item.FilePath))
					{
						continue;
					}

					if (_preloadedToolBlocks.ContainsKey(item.FilePath))
					{
						continue;
					}

					object loadedToolBlock = await Task.Run<object>(delegate
					{
						// 优先从运行流程里克隆，确保不直接影响主流程对象。
						object runtimeToolBlock = null;

						if (AlgorithmRuntimeBridge.Provider != null)
						{
							runtimeToolBlock = AlgorithmRuntimeBridge.Provider.TryGetRunningToolBlock(
								item.JobName,
								item.TaskName,
								item.Name);
						}

						if (runtimeToolBlock != null)
						{
							object cloned = TryCloneVisionObject(runtimeToolBlock);

							if (cloned != null)
							{
								return cloned;
							}
						}

						return TryLoadVppFileForBackground(item.FilePath);
					});

					if (loadedToolBlock == null)
					{
						continue;
					}

					_preloadedToolBlocks[item.FilePath] = loadedToolBlock;

					// 注意：
					// 这里不再提前创建 CogToolBlockEditV2。
					// CogToolBlockEditV2 必须在 UI 线程创建，提前创建会导致启动或点击算法模块时卡住。
					// 现在只预加载/克隆 ToolBlock，真正显示编辑器时仍由“加载 VPP 编辑器”按钮触发。
				}
				catch
				{
					// 单个 VPP 预加载失败，不影响其它 VPP 和主程序启动。
				}
			}
		}

		private List<AlgorithmFileItem> GetAllVppFilesFromProject()
		{
			List<AlgorithmFileItem> result = new List<AlgorithmFileItem>();

			try
			{
				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

				if (config == null || config.Jobs == null)
				{
					return result;
				}

				foreach (JobConfig job in config.Jobs)
				{
					if (job == null || job.Tasks == null)
					{
						continue;
					}

					foreach (TaskConfig task in job.Tasks)
					{
						if (task == null || task.Steps == null)
						{
							continue;
						}

						foreach (StepConfig step in task.Steps)
						{
							if (!IsVppStep(step))
							{
								continue;
							}

							string name = GetStepDisplayName(step);
							string path = GetStepVppPath(step, job.JobName, task.TaskName);

							if (string.IsNullOrEmpty(path))
							{
								continue;
							}

							result.Add(new AlgorithmFileItem
							{
								Name = name,
								FilePath = path,
								Step = step,
								JobName = job.JobName,
								TaskName = task.TaskName
							});
						}
					}
				}
			}
			catch
			{
			}

			return result;
		}

		private async Task LoadVppToEditorAsync(AlgorithmFileItem item)
		{
			if (item == null)
			{
				return;
			}

			_loadingVpp = true;
			SetAlgorithmUiBusy(true);

			ClearVppEditor();
			dgvPins.Rows.Clear();
			SetEditorMessage("正在后台加载 VPP，请稍候..." + Environment.NewLine + item.FilePath);

			try
			{
				object loadedToolBlock = await Task.Run<object>(delegate
				{
					object runtimeToolBlock = null;

					if (AlgorithmRuntimeBridge.Provider != null)
					{
						runtimeToolBlock = AlgorithmRuntimeBridge.Provider.TryGetRunningToolBlock(
							_currentJobName,
							_currentTaskName,
							item.Name);
					}

					if (runtimeToolBlock != null)
					{
						object cloned = TryCloneVisionObject(runtimeToolBlock);

						if (cloned != null)
						{
							return cloned;
						}
					}

					return TryLoadVppFileForBackground(item.FilePath);
				});

				_currentToolBlock = loadedToolBlock;

				if (_currentToolBlock == null)
				{
					// TryLoadVppFileForBackground 已经记录错误信息，下面统一显示。
					if (string.IsNullOrEmpty(_lastLoadError))
					{
						_lastLoadError = "VPP 加载失败，但没有返回具体错误。";
					}

					SetEditorMessage(_lastLoadError);
					return;
				}

				LoadPinsFromToolBlock(_currentToolBlock);
				ShowVppEditorPlaceholder(item.FilePath);
				OpenVppEditorDetached(_currentToolBlock, item.FilePath, GetRuntimeProjectVppPath(_currentJobName, _currentTaskName, item.Name));
			}
			catch (Exception ex)
			{
				SetEditorMessage("VPP 加载失败：" + Environment.NewLine + GetRealExceptionMessage(ex));
			}
			finally
			{
				SetAlgorithmUiBusy(false);
				_loadingVpp = false;
			}
		}

		private string _lastLoadError = string.Empty;

		private object TryLoadVppFileForBackground(string filePath)
		{
			_lastLoadError = string.Empty;

			if (string.IsNullOrWhiteSpace(filePath))
			{
				_lastLoadError = "VPP 路径为空。";
				return null;
			}

			if (!File.Exists(filePath))
			{
				_lastLoadError =
					"VPP 文件不存在。" + Environment.NewLine +
					"路径：" + filePath;
				return null;
			}

			try
			{
				return CogSerializer.LoadObjectFromFile(filePath);
			}
			catch (Exception ex)
			{
				_lastLoadError =
					"VPP 文件存在，但 VisionPro 加载失败。" + Environment.NewLine +
					"原因：" + GetRealExceptionMessage(ex) + Environment.NewLine +
					"路径：" + filePath;
				return null;
			}
		}

		private void SetAlgorithmUiBusy(bool busy)
		{
			listJobs.Enabled = !busy;
			listTasks.Enabled = !busy;
			listAlgorithmFiles.Enabled = !busy;
			btnVpp.Enabled = !busy;
			btnScript.Enabled = !busy;
			btnHdev.Enabled = !busy;
			btnVM.Enabled = !busy;

			if (btnApplyInputs != null) btnApplyInputs.Enabled = !busy;
			if (btnRunReplay != null) btnRunReplay.Enabled = !busy;
			if (btnSaveVpp != null) btnSaveVpp.Enabled = !busy;

			this.Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
		}

		private object TryLoadVppFile(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				SetEditorMessage("VPP 路径为空。");
				return null;
			}

			if (!File.Exists(filePath))
			{
				SetEditorMessage(
					"VPP 文件不存在。" + Environment.NewLine +
					"路径：" + filePath);
				return null;
			}

			try
			{
				Type serializerType = FindTypeFromLoadedOrLoadAssembly(
					"Cognex.VisionPro.CogSerializer",
					new string[]
					{
						"Cognex.VisionPro",
						"Cognex.VisionPro.Core"
					});

				if (serializerType == null)
				{
					SetEditorMessage(
						"VPP 文件存在，但未找到 Cognex.VisionPro.CogSerializer。" + Environment.NewLine +
						"请确认项目已经引用 Cognex.VisionPro.dll，并且复制到输出目录。" + Environment.NewLine +
						"路径：" + filePath);
					return null;
				}

				MethodInfo method = serializerType.GetMethod(
					"LoadObjectFromFile",
					BindingFlags.Public | BindingFlags.Static,
					null,
					new Type[] { typeof(string) },
					null);

				if (method == null)
				{
					SetEditorMessage(
						"已找到 CogSerializer，但未找到 LoadObjectFromFile(string) 方法。" + Environment.NewLine +
						"当前 VisionPro 版本方法签名可能不同。" + Environment.NewLine +
						"路径：" + filePath);
					return null;
				}

				object obj = method.Invoke(null, new object[] { filePath });

				if (obj == null)
				{
					SetEditorMessage(
						"CogSerializer.LoadObjectFromFile 返回 null。" + Environment.NewLine +
						"路径：" + filePath);
					return null;
				}

				return obj;
			}
			catch (TargetInvocationException ex)
			{
				string msg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;

				SetEditorMessage(
					"VPP 文件存在，但 VisionPro 加载失败。" + Environment.NewLine +
					"原因：" + msg + Environment.NewLine +
					"路径：" + filePath);

				return null;
			}
			catch (Exception ex)
			{
				SetEditorMessage(
					"VPP 文件存在，但加载时发生异常。" + Environment.NewLine +
					"原因：" + ex.Message + Environment.NewLine +
					"路径：" + filePath);

				return null;
			}
		}

		private Type FindTypeFromLoadedOrLoadAssembly(string fullTypeName, string[] assemblyNames)
		{
			if (string.IsNullOrWhiteSpace(fullTypeName))
			{
				return null;
			}

			// 1. 先从当前已经加载的程序集里找。
			Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly asm in loadedAssemblies)
			{
				try
				{
					Type t = asm.GetType(fullTypeName, false, true);

					if (t != null)
					{
						return t;
					}
				}
				catch
				{
				}
			}

			// 2. 再主动加载常见 VisionPro 程序集。
			if (assemblyNames != null)
			{
				foreach (string asmName in assemblyNames)
				{
					try
					{
						Assembly asm = Assembly.Load(asmName);
						Type t = asm.GetType(fullTypeName, false, true);

						if (t != null)
						{
							return t;
						}
					}
					catch
					{
					}
				}
			}

			// 3. 最后尝试 Type.GetType。
			foreach (string asmName in assemblyNames)
			{
				try
				{
					Type t = Type.GetType(fullTypeName + ", " + asmName, false, true);

					if (t != null)
					{
						return t;
					}
				}
				catch
				{
				}
			}

			return null;
		}

		private void SetEditorMessage(string message)
		{
			if (panelEditorHost == null)
			{
				return;
			}

			panelEditorHost.Controls.Clear();

			lblEditorInfo = new Label();
			lblEditorInfo.Dock = DockStyle.Fill;
			lblEditorInfo.TextAlign = ContentAlignment.MiddleCenter;
			lblEditorInfo.ForeColor = Color.FromArgb(140, 165, 190);
			lblEditorInfo.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			lblEditorInfo.Text = message;

			panelEditorHost.Controls.Add(lblEditorInfo);
		}

		private object TryCloneVisionObject(object source)
		{
			if (source == null)
			{
				return null;
			}

			try
			{
				Type serializerType =
					Type.GetType("Cognex.VisionPro.CogSerializer, Cognex.VisionPro") ??
					Type.GetType("Cognex.VisionPro.CogSerializer, Cognex.VisionPro.Core");

				if (serializerType == null)
				{
					return source;
				}

				MethodInfo saveToString = serializerType.GetMethod("SaveObjectToString", BindingFlags.Public | BindingFlags.Static);
				MethodInfo loadFromString = serializerType.GetMethod("LoadObjectFromString", BindingFlags.Public | BindingFlags.Static);

				if (saveToString != null && loadFromString != null)
				{
					string data = saveToString.Invoke(null, new object[] { source }) as string;

					if (!string.IsNullOrEmpty(data))
					{
						return loadFromString.Invoke(null, new object[] { data });
					}
				}
			}
			catch
			{
			}

			return source;
		}

		private void OpenVppEditorDetached(object toolBlock, string filePath, string savePath)
		{
			if (toolBlock == null || string.IsNullOrWhiteSpace(filePath))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(savePath))
			{
				savePath = GetRuntimeProjectVppPath(_currentJobName, _currentTaskName, Path.GetFileName(filePath));
			}

			string editorKey = savePath;

			if (_openedDetachedEditors.ContainsKey(editorKey) && _openedDetachedEditors[editorKey])
			{
				return;
			}

			_openedDetachedEditors[editorKey] = true;

			// 关键点：
			// CogToolBlockEditV2 的创建和显示全部放到独立 STA 线程。
			// 主 UI 线程不再创建 CogToolBlockEditV2，因此不会因为 VisionPro 控件初始化卡住主界面。
			Thread editorThread = new Thread(delegate ()
			{
				try
				{
					Application.EnableVisualStyles();

					IndependentVppEditorForm form = new IndependentVppEditorForm(
						toolBlock,
						filePath,
						savePath,
						delegate (string savedPath)
						{
							NotifyDetachedEditorSaved(savedPath);
						},
						delegate (string closedPath)
						{
							NotifyDetachedEditorClosed(closedPath);
						});

					Application.Run(form);
				}
				catch
				{
					NotifyDetachedEditorClosed(savePath);
				}
			});

			editorThread.IsBackground = true;
			editorThread.SetApartmentState(ApartmentState.STA);
			editorThread.Start();
		}

		private void NotifyDetachedEditorSaved(string savedPath)
		{
			if (_uiContext != null)
			{
				_uiContext.Post(delegate
				{
					try
					{
						_currentProjectSavePath = savedPath;
						_currentAlgorithmPath = savedPath;

						if (_currentAlgorithmItem != null)
						{
							_currentAlgorithmItem.FilePath = savedPath;
						}

						TryUpdateStepLocalPathAfterSave(savedPath);

						if (_currentToolBlock != null)
						{
							LoadPinsFromToolBlock(_currentToolBlock);
						}
					}
					catch
					{
					}
				}, null);
			}
		}

		private void NotifyDetachedEditorClosed(string closedPath)
		{
			if (_uiContext != null)
			{
				_uiContext.Post(delegate
				{
					if (!string.IsNullOrEmpty(closedPath) && _openedDetachedEditors.ContainsKey(closedPath))
					{
						_openedDetachedEditors[closedPath] = false;
					}
				}, null);
			}
		}

		private void ShowVppEditorPlaceholder(string filePath)
		{
			// 主界面底部 VPP 编辑器区域已取消显示。
			// VPP 编辑器会在独立 STA 线程窗口中自动打开。
			// 这里不再向 panelEditorHost 添加任何控件，避免占用主界面空间。
		}

		private void btnLoadEditor_Click(object sender, EventArgs e)
		{
			// 兼容旧事件。当前版本已取消主界面“加载 VPP 编辑器”按钮。
			// VPP 编辑器会在独立 STA 线程窗口中自动打开。
		}

		private void SetEditorLoadingMessage(string message)
		{
			if (panelEditorHost == null)
			{
				return;
			}

			panelEditorHost.Controls.Clear();

			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.TextAlign = ContentAlignment.MiddleCenter;
			label.ForeColor = Color.FromArgb(140, 165, 190);
			label.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			label.Text = message;

			panelEditorHost.Controls.Add(label);
			panelEditorHost.Refresh();
		}

		private void RefreshLoadedEditorSubject()
		{
			if (_currentVisionProEditor == null || _currentVisionProEditor.IsDisposed)
			{
				return;
			}

			try
			{
				PropertyInfo subjectProp = _currentVisionProEditor.GetType().GetProperty("Subject");

				if (subjectProp != null && subjectProp.CanWrite)
				{
					subjectProp.SetValue(_currentVisionProEditor, _currentToolBlock, null);
				}
			}
			catch
			{
				// 编辑器刷新失败不影响回放运行。
			}
		}

		private void ShowToolBlockEditor(object toolBlock, string filePath)
		{
			Cursor oldCursor = this.Cursor;
			this.Cursor = Cursors.WaitCursor;
			panelEditorHost.SuspendLayout();

			try
			{
				panelEditorHost.Controls.Clear();

				Control editor = TryCreateCogToolBlockEditor(toolBlock);

				if (editor != null)
				{
					if (editor.Parent != null)
					{
						editor.Parent.Controls.Remove(editor);
					}

					_currentVisionProEditor = editor;
					editor.Visible = true;
					editor.Dock = DockStyle.Fill;
					panelEditorHost.Controls.Add(editor);
					editor.BringToFront();
					return;
				}

				lblEditorInfo = new Label();
				lblEditorInfo.Dock = DockStyle.Fill;
				lblEditorInfo.TextAlign = ContentAlignment.MiddleCenter;
				lblEditorInfo.ForeColor = Color.FromArgb(140, 165, 190);
				lblEditorInfo.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
				lblEditorInfo.Text =
					"已加载 VPP，并且引脚已经读取成功。" + Environment.NewLine +
					"但未能创建 CogToolBlockEditV2 控件。" + Environment.NewLine +
					"当前对象类型：" + (toolBlock == null ? "null" : toolBlock.GetType().FullName) + Environment.NewLine +
					"请确认项目已引用 Cognex.VisionPro.ToolBlock.dll，并且运行目录能找到该 DLL。" + Environment.NewLine +
					"VPP：" + filePath;

				panelEditorHost.Controls.Add(lblEditorInfo);
			}
			finally
			{
				panelEditorHost.ResumeLayout(true);
				this.Cursor = oldCursor;
			}
		}

		private Control TryCreateCogToolBlockEditor(object toolBlock)
		{
			if (toolBlock == null)
			{
				SetEditorMessage("VPP 已加载，但 ToolBlock 对象为空。");
				return null;
			}

			// 你之前项目里使用的是 Cognex.VisionPro.ToolBlock.CogToolBlockEditV2。
			// 所以这里优先按照 CogToolBlockEditV2 直接创建，不再优先走 ToolGroup。
			CogToolBlock block = toolBlock as CogToolBlock;

			if (block != null)
			{
				try
				{
					CogToolBlockEditV2 edit = new CogToolBlockEditV2();
					edit.Dock = DockStyle.Fill;
					edit.Subject = block;
					return edit;
				}
				catch (Exception ex)
				{
					SetEditorMessage(
						"VPP 已加载，并且对象是 CogToolBlock。" + Environment.NewLine +
						"但创建 CogToolBlockEditV2 失败。" + Environment.NewLine +
						"原因：" + ex.Message + Environment.NewLine +
						"请确认项目引用了 Cognex.VisionPro.ToolBlock.dll，并且运行目录能找到该 DLL。");
					return null;
				}
			}

			// 如果不是 CogToolBlock，显示真实对象类型，方便判断这个 VPP 到底是什么对象。
			SetEditorMessage(
				"VPP 已加载，但它不是 CogToolBlock，不能使用 CogToolBlockEditV2 直接编辑。" + Environment.NewLine +
				"当前对象类型：" + toolBlock.GetType().FullName + Environment.NewLine +
				"如果它是 CogToolGroup，需要使用 CogToolGroupEditV2。");

			return null;
		}

		private Control TryCreateVisionProEditorByType(object subject, string[] editorTypeNames, string[] assemblyNames)
		{
			if (subject == null || editorTypeNames == null)
			{
				return null;
			}

			foreach (string editorTypeName in editorTypeNames)
			{
				Type editorType = FindTypeFromLoadedOrLoadAssembly(editorTypeName, assemblyNames);

				if (editorType == null)
				{
					continue;
				}

				try
				{
					object editorObj = Activator.CreateInstance(editorType);
					Control editor = editorObj as Control;

					if (editor == null)
					{
						continue;
					}

					PropertyInfo subjectProp = editorType.GetProperty("Subject");

					if (subjectProp == null)
					{
						continue;
					}

					// Subject 类型必须和当前 VPP 对象类型兼容。
					if (!subjectProp.PropertyType.IsAssignableFrom(subject.GetType()))
					{
						continue;
					}

					subjectProp.SetValue(editorObj, subject, null);
					return editor;
				}
				catch
				{
				}
			}

			return null;
		}

		private void LoadPinsFromToolBlock(object toolBlock)
		{
			_loadingPins = true;

			try
			{
				dgvPins.Rows.Clear();

				if (toolBlock == null)
				{
					return;
				}

				object inputs = GetPropertyObject(toolBlock, "Inputs");
				object outputs = GetPropertyObject(toolBlock, "Outputs");

				LoadTerminals(inputs, "Input");
				LoadTerminals(outputs, "Output");
			}
			finally
			{
				_loadingPins = false;
			}
		}

		private object GetPropertyObject(object obj, string propertyName)
		{
			if (obj == null)
			{
				return null;
			}

			try
			{
				PropertyInfo p = obj.GetType().GetProperty(propertyName);

				if (p == null)
				{
					return null;
				}

				return p.GetValue(obj, null);
			}
			catch
			{
				return null;
			}
		}

		private void LoadTerminals(object terminalCollection, string direction)
		{
			if (terminalCollection == null)
			{
				return;
			}

			try
			{
				IEnumerable enumerable = terminalCollection as IEnumerable;

				if (enumerable != null)
				{
					foreach (object terminal in enumerable)
					{
						AddTerminalRow(terminal, direction);
					}

					return;
				}

				int count = GetPropertyInt(terminalCollection, "Count");

				for (int i = 0; i < count; i++)
				{
					object terminal = GetIndexedItem(terminalCollection, i);

					if (terminal != null)
					{
						AddTerminalRow(terminal, direction);
					}
				}
			}
			catch
			{
			}
		}

		private int GetPropertyInt(object obj, string propertyName)
		{
			try
			{
				PropertyInfo p = obj.GetType().GetProperty(propertyName);

				if (p == null)
				{
					return 0;
				}

				object value = p.GetValue(obj, null);

				if (value == null)
				{
					return 0;
				}

				return Convert.ToInt32(value);
			}
			catch
			{
				return 0;
			}
		}

		private object GetIndexedItem(object collection, int index)
		{
			try
			{
				PropertyInfo indexer = collection.GetType()
					.GetProperties()
					.FirstOrDefault(p => p.GetIndexParameters().Length > 0);

				if (indexer == null)
				{
					return null;
				}

				return indexer.GetValue(collection, new object[] { index });
			}
			catch
			{
				return null;
			}
		}

		private void AddTerminalRow(object terminal, string direction)
		{
			if (terminal == null)
			{
				return;
			}

			string name = GetPropertyString(terminal, "Name");
			object value = GetPropertyObject(terminal, "Value");
			string dataType = value == null ? GetPropertyString(terminal, "ValueType") : value.GetType().Name;
			string valueText = ValueToDisplayText(value);

			string remark = direction == "Input" ? "可编辑输入值" : "输出值只读";

			int rowIndex = dgvPins.Rows.Add(direction, name, dataType, valueText, "...", remark);
			DataGridViewRow row = dgvPins.Rows[rowIndex];
			row.Tag = terminal;

			if (direction == "Output")
			{
				row.Cells["colValue"].ReadOnly = true;
				row.Cells["colBrowse"].ReadOnly = true;
			}

			if (!IsImageTerminal(name, dataType))
			{
				row.Cells["colBrowse"].ReadOnly = true;
				row.Cells["colBrowse"].Value = string.Empty;
			}
		}

		private string ValueToDisplayText(object value)
		{
			if (value == null)
			{
				return string.Empty;
			}

			if (value is string)
			{
				return value.ToString();
			}

			Type t = value.GetType();

			if (t.IsPrimitive || value is decimal)
			{
				return Convert.ToString(value);
			}

			return "[" + t.Name + "]";
		}

		private bool IsImageTerminal(string name, string dataType)
		{
			string s = ((name ?? string.Empty) + " " + (dataType ?? string.Empty)).ToLower();

			return s.Contains("image") ||
				   s.Contains("cogimage") ||
				   s.Contains("bitmap") ||
				   s.Contains("hobject") ||
				   s.Contains("mat");
		}

		private void dgvPins_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}

			if (dgvPins.Columns[e.ColumnIndex].Name != "colBrowse")
			{
				return;
			}

			DataGridViewRow row = dgvPins.Rows[e.RowIndex];

			if (row.Cells["colBrowse"].ReadOnly)
			{
				return;
			}

			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select Image File";
				dialog.Filter = "Image Files|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|All Files|*.*";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					row.Cells["colValue"].Value = dialog.FileName;
					row.Cells["colRemark"].Value = "本地图片文件，已应用到输入引脚";

					ApplyGridRowValueToInput(row, true);
				}
			}
		}

		private void dgvPins_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (dgvPins.IsCurrentCellDirty)
			{
				dgvPins.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private void dgvPins_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			if (_loadingPins || e.RowIndex < 0)
			{
				return;
			}

			DataGridViewRow row = dgvPins.Rows[e.RowIndex];
			ApplyGridRowValueToInput(row, false);
		}

		private void btnApplyInputs_Click(object sender, EventArgs e)
		{
			ApplyAllInputRows(false);
		}

		private void btnRunReplay_Click(object sender, EventArgs e)
		{
			ApplyAllInputRows(false);
			RunCurrentVppAndRefresh();
		}

		private void btnSaveVpp_Click(object sender, EventArgs e)
		{
			ApplyAllInputRows(false);
			SaveCurrentVpp();
		}

		private void ApplyAllInputRows(bool silent)
		{
			if (_currentToolBlock == null)
			{
				if (!silent)
				{
					MessageBox.Show("Please load VPP first.", "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				return;
			}

			foreach (DataGridViewRow row in dgvPins.Rows)
			{
				ApplyGridRowValueToInput(row, true);
			}

			if (!silent)
			{
				MessageBox.Show("Input values have been applied to VPP.", "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void ApplyGridRowValueToInput(DataGridViewRow row, bool silent)
		{
			if (row == null || row.IsNewRow)
			{
				return;
			}

			string direction = Convert.ToString(row.Cells["colDirection"].Value);

			if (!string.Equals(direction, "Input", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			object terminal = row.Tag;

			if (terminal == null)
			{
				return;
			}

			string valueText = Convert.ToString(row.Cells["colValue"].Value);
			string dataType = Convert.ToString(row.Cells["colDataType"].Value);
			string pinName = Convert.ToString(row.Cells["colName"].Value);

			try
			{
				object oldValue = GetPropertyObject(terminal, "Value");
				object newValue = ConvertTextToTerminalValue(valueText, dataType, oldValue, pinName);

				SetPropertyObject(terminal, "Value", newValue);

				row.Cells["colRemark"].Value = "已应用到输入引脚";
			}
			catch (Exception ex)
			{
				row.Cells["colRemark"].Value = "应用失败：" + ex.Message;

				if (!silent)
				{
					MessageBox.Show("Apply input failed: " + ex.Message, "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		private object ConvertTextToTerminalValue(string valueText, string dataType, object oldValue, string pinName)
		{
			if (IsImageTerminal(pinName, dataType))
			{
				if (!string.IsNullOrWhiteSpace(valueText) && File.Exists(valueText))
				{
					object image = TryLoadCogImage(valueText);

					if (image != null)
					{
						return image;
					}

					throw new Exception("Image file could not be loaded as CogImage. Please check Cognex.VisionPro.ImageFile.dll and image format.");
				}

				return oldValue;
			}

			Type targetType = oldValue == null ? typeof(string) : oldValue.GetType();

			if (targetType == typeof(string))
			{
				return valueText ?? string.Empty;
			}

			if (targetType == typeof(int))
			{
				int v;
				int.TryParse(valueText, out v);
				return v;
			}

			if (targetType == typeof(short))
			{
				short v;
				short.TryParse(valueText, out v);
				return v;
			}

			if (targetType == typeof(long))
			{
				long v;
				long.TryParse(valueText, out v);
				return v;
			}

			if (targetType == typeof(float))
			{
				float v;
				float.TryParse(valueText, out v);
				return v;
			}

			if (targetType == typeof(double))
			{
				double v;
				double.TryParse(valueText, out v);
				return v;
			}

			if (targetType == typeof(bool))
			{
				bool v;
				bool.TryParse(valueText, out v);
				return v;
			}

			try
			{
				return Convert.ChangeType(valueText, targetType);
			}
			catch
			{
				return valueText;
			}
		}

		private object TryLoadCogImage(string imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
			{
				return null;
			}

			CogImageFile imageFile = null;

			try
			{
				imageFile = new CogImageFile();
				imageFile.Open(imagePath, CogImageFileModeConstants.Read);

				// 注意：
				// CogImageFile 没有 Image 属性。
				// VisionPro 读取图像要通过索引器读取第一张图：
				// imageFile[0]
				object image = imageFile[0];

				if (image == null)
				{
					return null;
				}

				return image;
			}
			catch
			{
				return null;
			}
			finally
			{
				if (imageFile != null)
				{
					try
					{
						imageFile.Close();
					}
					catch
					{
					}
				}
			}
		}

		private void RunCurrentVppAndRefresh()
		{
			if (_currentToolBlock == null)
			{
				MessageBox.Show("Please load VPP first.", "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				MethodInfo runMethod = _currentToolBlock.GetType().GetMethod("Run", Type.EmptyTypes);

				if (runMethod == null)
				{
					MessageBox.Show("Current VPP object does not have Run() method.", "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				runMethod.Invoke(_currentToolBlock, null);

				LoadPinsFromToolBlock(_currentToolBlock);
				RefreshLoadedEditorSubject();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Run VPP failed: " + GetRealExceptionMessage(ex), "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void SaveCurrentVpp()
		{
			if (_currentToolBlock == null)
			{
				MessageBox.Show("Please load VPP first.", "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(_currentJobName) ||
				string.IsNullOrWhiteSpace(_currentTaskName) ||
				string.IsNullOrWhiteSpace(_currentAlgorithmName))
			{
				MessageBox.Show("Current Job / Task / VPP name is empty.", "Algorithm Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string savePath = GetRuntimeProjectVppPath(_currentJobName, _currentTaskName, _currentAlgorithmName);

			if (MessageBox.Show(
				"Save and overwrite current task VPP?" + Environment.NewLine + savePath,
				"Save VPP",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			try
			{
				string folder = Path.GetDirectoryName(savePath);

				if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}

				CogSerializer.SaveObjectToFile(_currentToolBlock, savePath);

				_currentProjectSavePath = savePath;
				_currentAlgorithmPath = savePath;

				if (_currentAlgorithmItem != null)
				{
					_currentAlgorithmItem.FilePath = savePath;
				}

				TryUpdateStepLocalPathAfterSave(savePath);

				MessageBox.Show("VPP saved successfully." + Environment.NewLine + savePath, "Save VPP", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Save VPP failed: " + GetRealExceptionMessage(ex), "Save VPP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private string GetRuntimeProjectVppPath(string jobName, string taskName, string vppName)
		{
			string fileName = Path.GetFileName(vppName);

			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = "Unknown.vpp";
			}

			if (!fileName.EndsWith(".vpp", StringComparison.OrdinalIgnoreCase))
			{
				fileName += ".vpp";
			}

			// 固定保存到当前软件运行目录下的 Project 工程内部路径：
			// bin\x64\Debug\Project\DemoProject\Steps\<JobID>\<TaskName>\VPP\<VppName>.vpp
			string projectRoot = GetRuntimeProjectRoot();

			return Path.Combine(projectRoot, "Steps", jobName, taskName, "VPP", fileName);
		}

		private string GetRuntimeProjectRoot()
		{
			// 用户要求保存到 Debug 下的 Project 文件夹。
			// 这里不要使用外部导入源路径，也不要使用 D:\Work... 这类 source path。
			return Path.Combine(Application.StartupPath, "Project", "DemoProject");
		}

		private void TryUpdateStepLocalPathAfterSave(string savePath)
		{
			try
			{
				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

				if (config == null || config.Jobs == null)
				{
					return;
				}

				foreach (JobConfig job in config.Jobs)
				{
					if (job == null || !string.Equals(job.JobName, _currentJobName, StringComparison.OrdinalIgnoreCase) || job.Tasks == null)
					{
						continue;
					}

					foreach (TaskConfig task in job.Tasks)
					{
						if (task == null || !string.Equals(task.TaskName, _currentTaskName, StringComparison.OrdinalIgnoreCase) || task.Steps == null)
						{
							continue;
						}

						foreach (StepConfig step in task.Steps)
						{
							if (step == null)
							{
								continue;
							}

							string displayName = GetStepDisplayName(step);

							if (!string.Equals(Path.GetFileName(displayName), Path.GetFileName(savePath), StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}

							SetPropertyIfExists(step, "LocalFilePath", savePath);
							SetPropertyIfExists(step, "SavedFilePath", savePath);
							SetPropertyIfExists(step, "StepFolder", Path.GetDirectoryName(Path.GetDirectoryName(savePath)));

							FlowConfigStore.Save(config);
							return;
						}
					}
				}
			}
			catch
			{
				// 不让 XML 更新失败影响 VPP 保存
			}
		}

		private void SetPropertyIfExists(object obj, string propertyName, object value)
		{
			if (obj == null)
			{
				return;
			}

			PropertyInfo p = obj.GetType().GetProperty(propertyName);

			if (p != null && p.CanWrite)
			{
				p.SetValue(obj, value, null);
			}
		}

		private string GetRealExceptionMessage(Exception ex)
		{
			TargetInvocationException tie = ex as TargetInvocationException;

			if (tie != null && tie.InnerException != null)
			{
				return tie.InnerException.Message;
			}

			return ex.Message;
		}

		private void SetPropertyObject(object obj, string propertyName, object value)
		{
			if (obj == null)
			{
				return;
			}

			PropertyInfo p = obj.GetType().GetProperty(propertyName);

			if (p == null || !p.CanWrite)
			{
				throw new Exception("Property is not writable: " + propertyName);
			}

			p.SetValue(obj, value, null);
		}

		private void ClearVppEditor()
		{
			_currentToolBlock = null;
			_currentVisionProEditor = null;

			if (panelEditorHost == null)
			{
				return;
			}

			panelEditorHost.Controls.Clear();

			lblEditorInfo = new Label();
			lblEditorInfo.Dock = DockStyle.Fill;
			lblEditorInfo.TextAlign = ContentAlignment.MiddleCenter;
			lblEditorInfo.ForeColor = Color.FromArgb(140, 165, 190);
			lblEditorInfo.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
			lblEditorInfo.Text = "请选择 Job、Task 和 VPP。";

			panelEditorHost.Controls.Add(lblEditorInfo);
		}

		private const int WM_SETREDRAW = 0x000B;

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private void EnableAlgorithmModuleSmoothUi()
		{
			try
			{
				this.SetStyle(ControlStyles.AllPaintingInWmPaint |
							  ControlStyles.OptimizedDoubleBuffer |
							  ControlStyles.ResizeRedraw, true);
				this.UpdateStyles();
			}
			catch
			{
			}

			EnableDoubleBuffer(this);

			EnableListBoxDoubleBuffer(listJobs);
			EnableListBoxDoubleBuffer(listTasks);
			EnableListBoxDoubleBuffer(listAlgorithmFiles);

			EnableDataGridViewSmooth(dgvPins);
		}

		private void EnableDoubleBuffer(Control control)
		{
			if (control == null)
			{
				return;
			}

			try
			{
				PropertyInfo p = typeof(Control).GetProperty(
					"DoubleBuffered",
					BindingFlags.Instance | BindingFlags.NonPublic);

				if (p != null)
				{
					p.SetValue(control, true, null);
				}
			}
			catch
			{
			}

			foreach (Control child in control.Controls)
			{
				EnableDoubleBuffer(child);
			}
		}

		private void EnableListBoxDoubleBuffer(ListBox list)
		{
			if (list == null)
			{
				return;
			}

			try
			{
				PropertyInfo p = typeof(Control).GetProperty(
					"DoubleBuffered",
					BindingFlags.Instance | BindingFlags.NonPublic);

				if (p != null)
				{
					p.SetValue(list, true, null);
				}
			}
			catch
			{
			}
		}

		private void EnableDataGridViewSmooth(DataGridView grid)
		{
			if (grid == null)
			{
				return;
			}

			try
			{
				PropertyInfo p = typeof(DataGridView).GetProperty(
					"DoubleBuffered",
					BindingFlags.Instance | BindingFlags.NonPublic);

				if (p != null)
				{
					p.SetValue(grid, true, null);
				}
			}
			catch
			{
			}

			try
			{
				grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
				grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			}
			catch
			{
			}
		}

		private void SuspendControlRedraw(Control control)
		{
			if (control == null || control.IsDisposed || !control.IsHandleCreated)
			{
				return;
			}

			try
			{
				SendMessage(control.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
			}
			catch
			{
			}
		}

		private void ResumeControlRedraw(Control control)
		{
			if (control == null || control.IsDisposed || !control.IsHandleCreated)
			{
				return;
			}

			try
			{
				SendMessage(control.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
				control.Invalidate(true);
				control.Update();
			}
			catch
			{
			}
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;

			if (isEnglish)
			{
				btnVpp.Text = "Vpp";
				btnScript.Text = "Script";
				btnHdev.Text = "Hdev";
				btnVM.Text = "VM";

				grpJobs.Text = "All JobID";
				grpTasks.Text = "All Task";

				dgvPins.Columns["colDirection"].HeaderText = "Type";
				dgvPins.Columns["colName"].HeaderText = "Pin Name";
				dgvPins.Columns["colDataType"].HeaderText = "Data Type";
				dgvPins.Columns["colValue"].HeaderText = "Current / Custom Value";
				dgvPins.Columns["colBrowse"].HeaderText = "File";
				dgvPins.Columns["colRemark"].HeaderText = "Remark";
				if (btnApplyInputs != null) btnApplyInputs.Text = "Apply";
				if (btnRunReplay != null) btnRunReplay.Text = "Run";
				if (btnSaveVpp != null) btnSaveVpp.Text = "Save VPP";
			}
			else
			{
				btnVpp.Text = "Vpp";
				btnScript.Text = "Script";
				btnHdev.Text = "Hdev";
				btnVM.Text = "VM";

				grpJobs.Text = "所有 JobID";
				grpTasks.Text = "所有 Task";

				dgvPins.Columns["colDirection"].HeaderText = "类型";
				dgvPins.Columns["colName"].HeaderText = "引脚名称";
				dgvPins.Columns["colDataType"].HeaderText = "数据类型";
				dgvPins.Columns["colValue"].HeaderText = "当前值 / 自定义值";
				dgvPins.Columns["colBrowse"].HeaderText = "文件";
				dgvPins.Columns["colRemark"].HeaderText = "说明";
				if (btnApplyInputs != null) btnApplyInputs.Text = "应用输入";
				if (btnRunReplay != null) btnRunReplay.Text = "回放运行";
				if (btnSaveVpp != null) btnSaveVpp.Text = "保存 VPP";
			}
			if (_scriptEditor != null && !_scriptEditor.IsDisposed)
			{
				_scriptEditor.ApplyLanguage(isEnglish);
			}

			SelectLibrary(_currentLibrary);
		}

		private class AlgorithmFileItem
		{
			public string Name { get; set; }
			public string FilePath { get; set; }
			public StepConfig Step { get; set; }
			public string JobName { get; set; }
			public string TaskName { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}
	}

	/// <summary>
	/// 当前运行程序 ToolBlock 快照提供者。
	/// 后续你可以在主程序启动时赋值：
	/// AlgorithmRuntimeBridge.Provider = new YourRuntimeProvider();
	/// 
	/// 算法配置页双击 VPP 时，会优先通过这里克隆当前运行程序中的 VPP 状态。
	/// 如果 Provider 为空，则从本地 VPP 文件加载。
	/// </summary>
	public interface IAlgorithmRuntimeSnapshotProvider
	{
		object TryGetRunningToolBlock(string jobName, string taskName, string vppName);
	}

	public static class AlgorithmRuntimeBridge
	{
		public static IAlgorithmRuntimeSnapshotProvider Provider { get; set; }
	}

	public class IndependentVppEditorForm : Form
	{
		private readonly object _toolBlock;
		private readonly string _vppPath;
		private readonly string _savePath;
		private readonly Action<string> _savedCallback;
		private readonly Action<string> _closedCallback;
		private Panel _hostPanel;
		private Button _btnSave;
		private Button _btnClose;
		private Label _lblInfo;
		private bool _dragging;
		private Point _dragStartPoint;
		private Point _formStartPoint;

		public IndependentVppEditorForm(object toolBlock, string sourcePath, string savePath, Action<string> savedCallback, Action<string> closedCallback)
		{
			_toolBlock = toolBlock;
			_vppPath = sourcePath;
			_savePath = savePath;
			_savedCallback = savedCallback;
			_closedCallback = closedCallback;

			InitializeEditorFormUi();
		}

		private void InitializeEditorFormUi()
		{
			this.Text = "VPP Editor - " + Path.GetFileName(_savePath);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(1200, 800);
			this.MinimumSize = new Size(900, 600);
			this.FormBorderStyle = FormBorderStyle.None;
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);

			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.ColumnCount = 1;
			root.RowCount = 2;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.BackColor = Color.FromArgb(2, 10, 20);
			root.Margin = new Padding(0);
			root.Padding = new Padding(8);

			Panel top = new Panel();
			top.Dock = DockStyle.Fill;
			top.BackColor = Color.FromArgb(3, 14, 27);
			top.MouseDown += DragArea_MouseDown;
			top.MouseMove += DragArea_MouseMove;
			top.MouseUp += DragArea_MouseUp;

			_lblInfo = new Label();
			_lblInfo.Dock = DockStyle.Fill;
			_lblInfo.TextAlign = ContentAlignment.MiddleLeft;
			_lblInfo.ForeColor = Color.FromArgb(180, 210, 235);
			_lblInfo.Text = "  VPP Editor  |  Save To: " + _savePath;
			_lblInfo.MouseDown += DragArea_MouseDown;
			_lblInfo.MouseMove += DragArea_MouseMove;
			_lblInfo.MouseUp += DragArea_MouseUp;

			_btnSave = CreateTopButton("保存 VPP", 100);
			_btnSave.Dock = DockStyle.Right;
			_btnSave.BackColor = Color.FromArgb(0, 95, 220);
			_btnSave.Click += btnSave_Click;

			_btnClose = CreateTopButton("关闭", 80);
			_btnClose.Dock = DockStyle.Right;
			_btnClose.Click += delegate { this.Close(); };

			top.Controls.Add(_lblInfo);
			top.Controls.Add(_btnClose);
			top.Controls.Add(_btnSave);

			_hostPanel = new Panel();
			_hostPanel.Dock = DockStyle.Fill;
			_hostPanel.BackColor = Color.FromArgb(1, 8, 16);

			root.Controls.Add(top, 0, 0);
			root.Controls.Add(_hostPanel, 0, 1);

			this.Controls.Add(root);

			this.Shown += IndependentVppEditorForm_Shown;
			this.FormClosed += IndependentVppEditorForm_FormClosed;
		}

		private void DragArea_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			_dragging = true;
			_dragStartPoint = Control.MousePosition;
			_formStartPoint = this.Location;
		}

		private void DragArea_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_dragging)
			{
				return;
			}

			Point current = Control.MousePosition;
			int offsetX = current.X - _dragStartPoint.X;
			int offsetY = current.Y - _dragStartPoint.Y;
			this.Location = new Point(_formStartPoint.X + offsetX, _formStartPoint.Y + offsetY);
		}

		private void DragArea_MouseUp(object sender, MouseEventArgs e)
		{
			_dragging = false;
		}

		private Button CreateTopButton(string text, int width)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Width = width;
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = Color.FromArgb(3, 14, 27);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private void IndependentVppEditorForm_Shown(object sender, EventArgs e)
		{
			ShowEditor();
		}

		private void IndependentVppEditorForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_closedCallback != null)
			{
				_closedCallback(_savePath);
			}
		}

		private void ShowEditor()
		{
			_hostPanel.Controls.Clear();

			try
			{
				CogToolBlock block = _toolBlock as CogToolBlock;

				if (block == null)
				{
					ShowMessage("当前 VPP 对象不是 CogToolBlock，无法使用 CogToolBlockEditV2 编辑。" + Environment.NewLine +
						"对象类型：" + (_toolBlock == null ? "null" : _toolBlock.GetType().FullName));
					return;
				}

				CogToolBlockEditV2 edit = new CogToolBlockEditV2();
				edit.Dock = DockStyle.Fill;
				edit.Subject = block;

				_hostPanel.Controls.Add(edit);
			}
			catch (Exception ex)
			{
				ShowMessage("创建 CogToolBlockEditV2 失败：" + Environment.NewLine + ex.Message);
			}
		}

		private void ShowMessage(string message)
		{
			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.TextAlign = ContentAlignment.MiddleCenter;
			label.ForeColor = Color.FromArgb(180, 210, 235);
			label.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			label.Text = message;
			_hostPanel.Controls.Add(label);
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			try
			{
				string folder = Path.GetDirectoryName(_savePath);

				if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}

				CogSerializer.SaveObjectToFile(_toolBlock, _savePath);

				if (_savedCallback != null)
				{
					_savedCallback(_savePath);
				}

				MessageBox.Show(this, "VPP saved successfully." + Environment.NewLine + _savePath, "Save VPP", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Save VPP failed: " + ex.Message, "Save VPP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
	}


	public class AlgorithmModuleConfig
	{
		public bool EnableVpp { get; set; }
		public bool EnableScript { get; set; }
		public bool EnableHdev { get; set; }
		public bool EnableVM { get; set; }

		public AlgorithmModuleConfig()
		{
			EnableVpp = false;
			EnableScript = false;
			EnableHdev = false;
			EnableVM = false;
		}
	}

	public static class AlgorithmModuleConfigStore
	{
		public static string ConfigFolder
		{
			get
			{
				return ProjectPathStore.AlgorithmConfigRoot;
			}
		}

		public static string ConfigFilePath
		{
			get
			{
				return Path.Combine(ConfigFolder, "AlgorithmModuleConfig.xml");
			}
		}

		public static AlgorithmModuleConfig LoadOrCreateDefault()
		{
			try
			{
				if (!Directory.Exists(ConfigFolder))
				{
					Directory.CreateDirectory(ConfigFolder);
				}

				if (!File.Exists(ConfigFilePath))
				{
					AlgorithmModuleConfig defaultConfig = new AlgorithmModuleConfig();
					Save(defaultConfig);
					return defaultConfig;
				}

				XmlSerializer serializer = new XmlSerializer(typeof(AlgorithmModuleConfig));

				using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read))
				{
					object obj = serializer.Deserialize(fs);
					AlgorithmModuleConfig config = obj as AlgorithmModuleConfig;

					if (config == null)
					{
						config = new AlgorithmModuleConfig();
					}

					return config;
				}
			}
			catch
			{
				return new AlgorithmModuleConfig();
			}
		}

		public static void Save(AlgorithmModuleConfig config)
		{
			if (config == null)
			{
				config = new AlgorithmModuleConfig();
			}

			if (!Directory.Exists(ConfigFolder))
			{
				Directory.CreateDirectory(ConfigFolder);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(AlgorithmModuleConfig));

			using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Create, FileAccess.Write))
			{
				serializer.Serialize(fs, config);
			}
		}
	}

}
