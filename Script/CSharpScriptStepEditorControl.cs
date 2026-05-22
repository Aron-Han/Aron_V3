using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Aron_V3
{
	/// <summary>
	/// C# Script Step 编辑控件。
	/// UI 已经拆到 CSharpScriptStepEditorControl.Designer.cs。
	/// 本文件只保留业务逻辑、事件绑定、配置读写、编译运行。
	/// V3:
	/// 1. 代码编辑器支持左侧行号。
	/// 2. 删除“运行输出”区域。
	/// 3. 编译/运行日志独占底部区域。
	/// 4. 引用 DLL 跟随当前 Script 配置保存。
	/// </summary>
	public partial class CSharpScriptStepEditorControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(3, 14, 27);
		private readonly Color _panel2 = Color.FromArgb(5, 18, 34);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private readonly Color _green = Color.FromArgb(70, 210, 90);
		private readonly Color _red = Color.FromArgb(235, 54, 65);
		private readonly Color _text = Color.FromArgb(220, 235, 245);
		private readonly Color _muted = Color.FromArgb(130, 155, 175);

		private string _jobName;
		private string _taskName;
		private string _configPath;
		private string _scriptPath;
		private CSharpScriptStepConfig _config;
		private bool _loading;
		private bool _isEnglish;

		// RichTextBox 轻量级代码提示，不依赖第三方控件。
		private ListBox _completionList;
		private bool _completionUpdating;
		private int _completionStartIndex;
		private List<CompletionItem> _completionItems;

		public CSharpScriptStepEditorControl()
		{
			InitializeComponent();

			CSharpScriptReferenceManager.EnsureReferenceFolder();
			CSharpScriptReferenceManager.PreloadAllReferenceDlls();

			_config = CSharpScriptStepStore.CreateDefaultConfig();

			InitTheme();
			InitGrids();
			BindEvents();
			InitCodeCompletion();

			LoadConfigToUi();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;

			btnReferenceDll.Text = isEnglish ? "Import DLL" : "导入DLL";
			btnSave.Text = isEnglish ? "Save" : "保存";
			btnCompile.Text = isEnglish ? "Compile" : "编译";
			btnRun.Text = isEnglish ? "Debug Run" : "调试运行";
			lblStepName.Text = isEnglish ? "Current Script" : "当前脚本";
			lblScriptFile.Text = string.Empty;
			lblStatusTitle.Text = isEnglish ? "Status" : "状态";
			lblInputTitle.Text = isEnglish ? "Inputs    Edit Current/Default value for debug" : "输入定义 Inputs    调试时直接修改“当前值/默认值”列";
			lblOutputTitle.Text = isEnglish ? "Outputs" : "输出定义 Outputs";
			lblCodeTitle.Text = isEnglish ? "C# Script Code" : "C# Script Code";
			lblLogTitle.Text = isEnglish ? "Compile / Run Log" : "编译 / 运行日志";

			SetGridHeaders();
		}

		public void LoadScriptStep(string jobName, string taskName, string stepName)
		{
			_jobName = string.IsNullOrWhiteSpace(jobName) ? "Job_001" : jobName;
			_taskName = string.IsNullOrWhiteSpace(taskName) ? "Task_New_01" : taskName;

			string safeStep = string.IsNullOrWhiteSpace(stepName) ? "CS_Script" : stepName.Trim();

			StepConfig flowStep = FindScriptStepConfig(_jobName, _taskName, safeStep);
			_scriptPath = ResolveScriptPath(_jobName, _taskName, safeStep, flowStep);
			_configPath = ResolveScriptConfigPath(_jobName, _taskName, safeStep, _scriptPath);

			_config = CSharpScriptStepStore.Load(_configPath);
			_config.StepName = GetScriptDisplayName(safeStep, _scriptPath);
			_config.Enable = true;
			_config.ScriptFilePath = _scriptPath;
			_config.ScriptFileName = Path.GetFileName(_scriptPath);

			// 不再自动创建默认 CS_Script.csx。
			// 只加载“所有 Script”列表中双击选中的脚本文件。
			LoadConfigToUi();

			if (!string.IsNullOrWhiteSpace(_scriptPath) && File.Exists(_scriptPath))
			{
				txtCode.Text = File.ReadAllText(_scriptPath, System.Text.Encoding.UTF8);
				RefreshCodeLineNumbers();
				LogInfo("Script step loaded: " + _config.StepName);
			}
			else
			{
				txtCode.Text = string.Empty;
				RefreshCodeLineNumbers();
				LogError("Script file was not found. Please check Step config. Step: " + safeStep);
				SetStatusError("Script file not found");
			}
		}

		private StepConfig FindScriptStepConfig(string jobName, string taskName, string stepName)
		{
			try
			{
				ProjectFlowConfig flow = FlowConfigStore.LoadOrCreateDefault();
				JobConfig job = flow.Jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));
				if (job == null) return null;

				TaskConfig task = job.Tasks.FirstOrDefault(t => string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase));
				if (task == null || task.Steps == null) return null;

				StepConfig byName = task.Steps.FirstOrDefault(s =>
					s.StepType == StepType.Script &&
					string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase));

				if (byName != null) return byName;

				return task.Steps.FirstOrDefault(s =>
					s.StepType == StepType.Script &&
					IsScriptStepFileNameMatch(s, stepName));
			}
			catch
			{
				return null;
			}
		}

		private bool IsScriptStepFileNameMatch(StepConfig step, string name)
		{
			if (step == null || string.IsNullOrWhiteSpace(name)) return false;
			string n = Path.GetFileNameWithoutExtension(name);

			if (!string.IsNullOrWhiteSpace(step.ProjectFilePath) &&
				string.Equals(Path.GetFileNameWithoutExtension(step.ProjectFilePath), n, StringComparison.OrdinalIgnoreCase)) return true;

			if (!string.IsNullOrWhiteSpace(step.SourceFilePath) &&
				string.Equals(Path.GetFileNameWithoutExtension(step.SourceFilePath), n, StringComparison.OrdinalIgnoreCase)) return true;

			if (step.ScriptFiles != null)
			{
				foreach (string f in step.ScriptFiles)
				{
					if (string.Equals(Path.GetFileNameWithoutExtension(f), n, StringComparison.OrdinalIgnoreCase)) return true;
				}
			}

			return false;
		}

		private string ResolveScriptPath(string jobName, string taskName, string stepName, StepConfig step)
		{
			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(jobName, taskName);

			if (step != null)
			{
				string candidate = ResolveStepFilePath(taskFolder, step.ProjectFilePath);
				if (File.Exists(candidate)) return candidate;

				if (step.ScriptFiles != null && step.ScriptFiles.Count > 0)
				{
					foreach (string relative in step.ScriptFiles)
					{
						candidate = ResolveStepFilePath(taskFolder, relative);
						if (File.Exists(candidate)) return candidate;
					}
				}

				if (!string.IsNullOrWhiteSpace(step.SourceFilePath) && File.Exists(step.SourceFilePath))
				{
					return step.SourceFilePath;
				}
			}

			string byName = Path.Combine(taskFolder, "Scripts", MakeSafeFileName(stepName) + ".csx");
			if (File.Exists(byName)) return byName;

			string[] files = Directory.Exists(Path.Combine(taskFolder, "Scripts"))
				? Directory.GetFiles(Path.Combine(taskFolder, "Scripts"), "*.cs*", SearchOption.TopDirectoryOnly)
				: new string[0];

			foreach (string file in files)
			{
				if (string.Equals(Path.GetFileNameWithoutExtension(file), stepName, StringComparison.OrdinalIgnoreCase))
				{
					return file;
				}
			}

			return byName;
		}

		private string ResolveStepFilePath(string taskFolder, string relativeOrAbsolute)
		{
			if (string.IsNullOrWhiteSpace(relativeOrAbsolute)) return string.Empty;
			if (Path.IsPathRooted(relativeOrAbsolute)) return relativeOrAbsolute;
			return Path.Combine(taskFolder, relativeOrAbsolute);
		}

		private string ResolveScriptConfigPath(string jobName, string taskName, string stepName, string scriptPath)
		{
			if (!string.IsNullOrWhiteSpace(scriptPath))
			{
				string dir = Path.GetDirectoryName(scriptPath);
				string name = Path.GetFileNameWithoutExtension(scriptPath);
				if (!string.IsNullOrWhiteSpace(dir) && !string.IsNullOrWhiteSpace(name))
				{
					return Path.Combine(dir, name + ".script.xml");
				}
			}

			return CSharpScriptStepStore.GetConfigPath(jobName, taskName, stepName);
		}

		private string GetScriptDisplayName(string fallbackStepName, string scriptPath)
		{
			if (!string.IsNullOrWhiteSpace(scriptPath))
			{
				string name = Path.GetFileNameWithoutExtension(scriptPath);
				if (!string.IsNullOrWhiteSpace(name)) return name;
			}

			return string.IsNullOrWhiteSpace(fallbackStepName) ? "---" : fallbackStepName;
		}

		private string MakeSafeFileName(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return "CS_Script";
			foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
			return name.Trim();
		}

		public CSharpScriptStepConfig GetCurrentConfig()
		{
			SaveUiToConfig();
			return _config;
		}

		private void InitTheme()
		{
			BackColor = _back;
			DoubleBuffered = true;

			ApplyPanelTheme(this);

			StyleScriptNameLabel();
			StyleTextBox(txtScriptPath);
			StyleCodeBox(txtCode);

			StyleButton(btnReferenceDll);
			StyleButton(btnSave);
			StyleButton(btnCompile);
			StyleButton(btnRun);
			StyleButton(btnInputAdd);
			StyleButton(btnInputDelete);
			StyleButton(btnOutputAdd);
			StyleButton(btnOutputDelete);

			btnSave.BackColor = Color.FromArgb(0, 95, 190);
			btnCompile.BackColor = Color.FromArgb(0, 95, 190);
			btnRun.BackColor = Color.FromArgb(20, 125, 40);


			lblStatusLight.BackColor = Color.FromArgb(120, 120, 120);
			lblStatusText.ForeColor = _muted;

			StyleLabel(lblStepName);
			StyleLabel(lblScriptFile);
			StyleLabel(lblStatusTitle);
			StyleLabel(lblInputTitle);
			StyleLabel(lblOutputTitle);
			StyleLabel(lblCodeTitle);
			StyleLabel(lblLogTitle);

			panelLineNumbers.BackColor = _panel2;

			NormalizeHeaderUi();
		}

		private void ApplyPanelTheme(Control parent)
		{
			if (parent == null)
			{
				return;
			}

			foreach (Control c in parent.Controls)
			{
				Panel panel = c as Panel;
				if (panel != null)
				{
					panel.BackColor = _back;
				}

				TableLayoutPanel table = c as TableLayoutPanel;
				if (table != null)
				{
					table.BackColor = _back;
				}

				SplitContainer split = c as SplitContainer;
				if (split != null)
				{
					split.BackColor = _border;
					split.Panel1.BackColor = _back;
					split.Panel2.BackColor = _back;
				}

				ApplyPanelTheme(c);
			}
		}

		private void StyleLabel(Label lbl)
		{
			if (lbl == null)
			{
				return;
			}

			lbl.ForeColor = _text;
			lbl.BackColor = Color.Transparent;
			lbl.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			lbl.TextAlign = ContentAlignment.MiddleLeft;
		}

		private void StyleTextBox(TextBox txt)
		{
			if (txt == null)
			{
				return;
			}

			txt.BorderStyle = BorderStyle.FixedSingle;
			txt.BackColor = Color.FromArgb(1, 8, 16);
			txt.ForeColor = _text;
			txt.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
		}

		private void StyleScriptNameLabel()
		{
			if (txtStepName == null)
			{
				return;
			}

			txtStepName.BorderStyle = BorderStyle.None;
			txtStepName.ReadOnly = true;
			txtStepName.BackColor = _back;
			txtStepName.ForeColor = Color.White;
			txtStepName.Font = new Font("Consolas", 10F, FontStyle.Bold);
			txtStepName.TabStop = false;
		}

		private void NormalizeHeaderUi()
		{
			try
			{
				if (chkEnable != null)
				{
					chkEnable.Checked = true;
					chkEnable.Visible = false;
				}

				if (lblScriptFile != null) lblScriptFile.Visible = false;
				if (txtScriptPath != null) txtScriptPath.Visible = false;
				if (btnBrowseScript != null) btnBrowseScript.Visible = false;

				if (topLayout != null)
				{
					if (topLayout.RowStyles.Count > 1)
					{
						topLayout.RowStyles[0].SizeType = SizeType.Percent;
						topLayout.RowStyles[0].Height = 100F;
						topLayout.RowStyles[1].SizeType = SizeType.Absolute;
						topLayout.RowStyles[1].Height = 0F;
					}

					if (rootLayout != null && rootLayout.RowStyles.Count > 0)
					{
						rootLayout.RowStyles[0].SizeType = SizeType.Absolute;
						rootLayout.RowStyles[0].Height = 72F;
					}

					topLayout.SetColumnSpan(txtStepName, 4);
					topLayout.SetColumn(btnReferenceDll, 6);
					topLayout.SetRow(btnReferenceDll, 0);
					topLayout.SetColumn(btnSave, 7);
					topLayout.SetRow(btnSave, 0);
					topLayout.SetColumn(btnCompile, 8);
					topLayout.SetRow(btnCompile, 0);
					topLayout.SetColumn(btnRun, 9);
					topLayout.SetRow(btnRun, 0);
				}
			}
			catch
			{
			}
		}

		private void StyleCodeBox(RichTextBox txt)
		{
			if (txt == null)
			{
				return;
			}

			txt.Multiline = true;
			txt.AcceptsTab = true;
			txt.WordWrap = false;
			txt.ScrollBars = RichTextBoxScrollBars.Both;
			txt.BorderStyle = BorderStyle.FixedSingle;
			txt.BackColor = Color.FromArgb(1, 8, 16);
			txt.ForeColor = Color.FromArgb(210, 230, 245);
			txt.Font = new Font("Consolas", 10F, FontStyle.Regular);
			txt.HideSelection = false;
		}

		private void StyleButton(Button btn)
		{
			if (btn == null)
			{
				return;
			}

			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = _accent;
			btn.FlatAppearance.BorderSize = 1;
			btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
			btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
			btn.BackColor = _panel;
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
		}

		private void InitGrids()
		{
			InitPinGrid(gridInputs, true);
			InitPinGrid(gridOutputs, false);
			InitLogGrid(gridLogs);
		}

		private void InitPinGrid(DataGridView grid, bool isInput)
		{
			StyleGrid(grid);
			grid.Columns.Clear();

			grid.Columns.Add(CreateTextColumn("Name", isInput ? "输入名" : "输出名", 130));
			grid.Columns.Add(CreateComboColumn("DataType", "类型", 90));
			grid.Columns.Add(CreateTextColumn("BindingPath", isInput ? "绑定来源" : "目标去向", 220));
			grid.Columns.Add(CreateTextColumn("DefaultValue", isInput ? "当前值/默认值" : "默认值", 130));
			grid.Columns.Add(CreateTextColumn("Description", "说明", 240));

			grid.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			grid.Columns["DataType"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			grid.Columns["BindingPath"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			grid.Columns["DefaultValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			grid.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			grid.Columns["BindingPath"].FillWeight = 55;
			grid.Columns["Description"].FillWeight = 45;
		}

		private void InitLogGrid(DataGridView grid)
		{
			StyleGrid(grid);
			grid.Columns.Clear();

			grid.Columns.Add(CreateTextColumn("Time", "时间", 125));
			grid.Columns.Add(CreateTextColumn("Level", "级别", 80));
			grid.Columns.Add(CreateTextColumn("Message", "消息", 900));

			grid.Columns["Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			grid.Columns["Level"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			grid.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		}

		private void StyleGrid(DataGridView grid)
		{
			if (grid == null)
			{
				return;
			}

			grid.BackgroundColor = _back;
			grid.BorderStyle = BorderStyle.FixedSingle;
			grid.GridColor = _border;
			grid.EnableHeadersVisualStyles = false;
			grid.RowHeadersVisible = false;
			grid.AllowUserToAddRows = false;
			grid.AllowUserToResizeRows = true;
			grid.AllowUserToResizeColumns = true;
			grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			grid.MultiSelect = false;
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
			grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			grid.ScrollBars = ScrollBars.Both;

			grid.ColumnHeadersDefaultCellStyle.BackColor = _panel2;
			grid.ColumnHeadersDefaultCellStyle.ForeColor = _text;
			grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

			grid.DefaultCellStyle.BackColor = _back;
			grid.DefaultCellStyle.ForeColor = _text;
			grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 125, 200);
			grid.DefaultCellStyle.SelectionForeColor = Color.White;
			grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

			grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(4, 16, 30);
			grid.AlternatingRowsDefaultCellStyle.ForeColor = _text;

			grid.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			grid.ColumnHeadersHeight = 30;
			grid.RowTemplate.Height = 30;
		}

		private DataGridViewTextBoxColumn CreateTextColumn(string name, string header, int width)
		{
			DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
			col.Name = name;
			col.HeaderText = header;
			col.Width = width;
			return col;
		}

		private DataGridViewComboBoxColumn CreateComboColumn(string name, string header, int width)
		{
			DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
			col.Name = name;
			col.HeaderText = header;
			col.Width = width;
			col.FlatStyle = FlatStyle.Flat;
			col.Items.Add(ScriptPinDataType.String);
			col.Items.Add(ScriptPinDataType.Bool);
			col.Items.Add(ScriptPinDataType.Int);
			col.Items.Add(ScriptPinDataType.Double);
			col.Items.Add(ScriptPinDataType.Decimal);
			col.Items.Add(ScriptPinDataType.Object);
			return col;
		}

		private void BindEvents()
		{
			btnReferenceDll.Click += btnReferenceDll_Click;
			btnSave.Click += btnSave_Click;
			btnCompile.Click += btnCompile_Click;
			btnRun.Click += btnRun_Click;

			btnInputAdd.Click += delegate { AddPinRow(gridInputs); };
			btnInputDelete.Click += delegate { DeleteSelectedPinRows(gridInputs); };
			btnOutputAdd.Click += delegate { AddPinRow(gridOutputs); };
			btnOutputDelete.Click += delegate { DeleteSelectedPinRows(gridOutputs); };

			this.Resize += delegate { UpdateScriptEditorSplitter(); };
			this.HandleCreated += delegate { UpdateScriptEditorSplitter(); };

			txtCode.TextChanged += txtCode_TextChanged;
			txtCode.VScroll += txtCode_VScroll;
			txtCode.Resize += txtCode_Resize;
			txtCode.FontChanged += txtCode_FontChanged;
			txtCode.KeyDown += txtCode_KeyDown;
			txtCode.KeyUp += txtCode_KeyUp;
			txtCode.LostFocus += txtCode_LostFocus;
			panelLineNumbers.Paint += panelLineNumbers_Paint;
		}

		private void AddPinRow(DataGridView grid)
		{
			if (grid == null)
			{
				return;
			}

			int rowIndex = grid.Rows.Add();
			if (grid.Columns.Contains("DataType"))
			{
				grid.Rows[rowIndex].Cells["DataType"].Value = ScriptPinDataType.String;
			}
		}

		private void DeleteSelectedPinRows(DataGridView grid)
		{
			if (grid == null || grid.SelectedRows.Count <= 0)
			{
				return;
			}

			foreach (DataGridViewRow row in grid.SelectedRows)
			{
				if (!row.IsNewRow)
				{
					grid.Rows.Remove(row);
				}
			}
		}

		private void UpdateScriptEditorSplitter()
		{
			if (mainSplit != null && !mainSplit.IsDisposed && mainSplit.Width > 500)
			{
				int leftWidth = (int)(mainSplit.Width * 0.46);
				leftWidth = Clamp(leftWidth, 520, mainSplit.Width - 620);

				try
				{
					if (leftWidth > 0 && leftWidth < mainSplit.Width)
					{
						mainSplit.SplitterDistance = leftWidth;
					}
				}
				catch
				{
				}
			}

			if (leftSplit != null && !leftSplit.IsDisposed && leftSplit.Height > 220)
			{
				try
				{
					int h = (int)(leftSplit.Height * 0.52);
					if (h > 0 && h < leftSplit.Height)
					{
						leftSplit.SplitterDistance = h;
					}
				}
				catch
				{
				}
			}
		}

		private int Clamp(int value, int min, int max)
		{
			if (max < min)
			{
				return min;
			}

			if (value < min)
			{
				return min;
			}

			if (value > max)
			{
				return max;
			}

			return value;
		}

		private void SetGridHeaders()
		{
			SetPinGridHeaders(gridInputs, true);
			SetPinGridHeaders(gridOutputs, false);

			if (gridLogs.Columns.Contains("Time")) gridLogs.Columns["Time"].HeaderText = _isEnglish ? "Time" : "时间";
			if (gridLogs.Columns.Contains("Level")) gridLogs.Columns["Level"].HeaderText = _isEnglish ? "Level" : "级别";
			if (gridLogs.Columns.Contains("Message")) gridLogs.Columns["Message"].HeaderText = _isEnglish ? "Message" : "消息";
		}

		private void SetPinGridHeaders(DataGridView grid, bool isInput)
		{
			if (grid == null)
			{
				return;
			}

			if (grid.Columns.Contains("Name")) grid.Columns["Name"].HeaderText = isInput ? (_isEnglish ? "Input" : "输入名") : (_isEnglish ? "Output" : "输出名");
			if (grid.Columns.Contains("DataType")) grid.Columns["DataType"].HeaderText = _isEnglish ? "Type" : "类型";
			if (grid.Columns.Contains("BindingPath")) grid.Columns["BindingPath"].HeaderText = isInput ? (_isEnglish ? "Source" : "绑定来源") : (_isEnglish ? "Target" : "目标去向");
			if (grid.Columns.Contains("DefaultValue")) grid.Columns["DefaultValue"].HeaderText = isInput ? (_isEnglish ? "Current / Default" : "当前值/默认值") : (_isEnglish ? "Default" : "默认值");
			if (grid.Columns.Contains("Description")) grid.Columns["Description"].HeaderText = _isEnglish ? "Description" : "说明";
		}

		private void LoadConfigToUi()
		{
			_loading = true;

			try
			{
				txtStepName.Text = _config == null ? "---" : _config.StepName;
				chkEnable.Checked = true;
				txtScriptPath.Text = _config == null ? string.Empty : _config.ScriptFilePath;

				LoadPinsToGrid(gridInputs, _config == null ? null : _config.Inputs);
				LoadPinsToGrid(gridOutputs, _config == null ? null : _config.Outputs);

				if (_config != null && !string.IsNullOrWhiteSpace(_config.ScriptFilePath) && File.Exists(_config.ScriptFilePath))
				{
					txtCode.Text = File.ReadAllText(_config.ScriptFilePath, System.Text.Encoding.UTF8);
				}
				else
				{
					txtCode.Text = string.Empty;
				}

				RefreshCodeLineNumbers();
				SetStatusReady();
			}
			finally
			{
				_loading = false;
			}
		}

		private void SaveUiToConfig()
		{
			if (_config == null)
			{
				_config = new CSharpScriptStepConfig();
			}

			_config.StepName = string.IsNullOrWhiteSpace(txtStepName.Text) ? "CS_Script" : txtStepName.Text.Trim();
			_config.Enable = true;
			_config.ScriptFilePath = _scriptPath;
			_config.ScriptFileName = Path.GetFileName(_config.ScriptFilePath);

			_config.Inputs = ReadPinsFromGrid(gridInputs);
			_config.Outputs = ReadPinsFromGrid(gridOutputs);
		}

		private void LoadPinsToGrid(DataGridView grid, List<ScriptPinConfig> pins)
		{
			grid.Rows.Clear();

			if (pins == null)
			{
				return;
			}

			foreach (ScriptPinConfig pin in pins)
			{
				int rowIndex = grid.Rows.Add();
				DataGridViewRow row = grid.Rows[rowIndex];

				row.Cells["Name"].Value = pin.Name;
				row.Cells["DataType"].Value = pin.DataType;
				row.Cells["BindingPath"].Value = pin.BindingPath;
				row.Cells["DefaultValue"].Value = pin.DefaultValue;
				row.Cells["Description"].Value = pin.Description;
			}
		}

		private List<ScriptPinConfig> ReadPinsFromGrid(DataGridView grid)
		{
			List<ScriptPinConfig> result = new List<ScriptPinConfig>();

			if (grid == null)
			{
				return result;
			}

			foreach (DataGridViewRow row in grid.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string name = Convert.ToString(row.Cells["Name"].Value);

				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				ScriptPinConfig pin = new ScriptPinConfig();
				pin.Name = name.Trim();
				pin.DataType = ParseDataType(Convert.ToString(row.Cells["DataType"].Value));
				pin.BindingPath = Convert.ToString(row.Cells["BindingPath"].Value);
				pin.DefaultValue = Convert.ToString(row.Cells["DefaultValue"].Value);
				pin.Description = Convert.ToString(row.Cells["Description"].Value);

				result.Add(pin);
			}

			return result;
		}

		private ScriptPinDataType ParseDataType(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return ScriptPinDataType.String;
			}

			ScriptPinDataType value;
			if (Enum.TryParse(text, true, out value))
			{
				return value;
			}

			return ScriptPinDataType.String;
		}

		private void btnBrowseScript_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select C# script file";
				dialog.Filter = "C# Script (*.csx;*.cs)|*.csx;*.cs|All files (*.*)|*.*";

				if (!string.IsNullOrWhiteSpace(_scriptPath))
				{
					string dir = Path.GetDirectoryName(_scriptPath);
					if (Directory.Exists(dir))
					{
						dialog.InitialDirectory = dir;
					}
				}

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				txtScriptPath.Text = dialog.FileName;
				_scriptPath = dialog.FileName;
				txtCode.Text = File.ReadAllText(dialog.FileName, System.Text.Encoding.UTF8);
				RefreshCodeLineNumbers();
				LogInfo("Script file loaded: " + dialog.FileName);
			}
		}

		private void btnReferenceDll_Click(object sender, EventArgs e)
		{
			try
			{
				CSharpScriptReferenceManager.EnsureReferenceFolder();

				using (OpenFileDialog dialog = new OpenFileDialog())
				{
					dialog.Title = "Import global script reference DLL";
					dialog.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";
					dialog.Multiselect = true;

					if (dialog.ShowDialog(this) == DialogResult.OK)
					{
						foreach (string file in dialog.FileNames)
						{
							string target = Path.Combine(CSharpScriptReferenceManager.ReferenceFolder, Path.GetFileName(file));
							File.Copy(file, target, true);
							LogInfo("Global reference DLL imported: " + target);
						}
						CSharpScriptReferenceManager.PreloadAllReferenceDlls();
						LogInfo("Global reference folder: " + CSharpScriptReferenceManager.ReferenceFolder);
					}
				}

				try
				{
					Process.Start("explorer.exe", CSharpScriptReferenceManager.ReferenceFolder);
				}
				catch
				{
				}
			}
			catch (Exception ex)
			{
				LogError("Import DLL failed: " + ex.Message);
				SetStatusError("Import DLL failed");
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			try
			{
				SaveCurrentScriptAndConfigOnly();
				LogInfo("Script saved.");
				SetStatusOK("Saved");
			}
			catch (Exception ex)
			{
				LogError("Save failed: " + ex.Message);
				SetStatusError("Save failed");
			}
		}

		private void SaveCurrentScriptAndConfigOnly()
		{
			SaveUiToConfig();

			if (string.IsNullOrWhiteSpace(_config.ScriptFilePath))
			{
				throw new InvalidOperationException("Current script path is empty. Please select a script from the Script list first.");
			}

			string dir = Path.GetDirectoryName(_config.ScriptFilePath);
			if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			if (string.IsNullOrWhiteSpace(_configPath))
			{
				_configPath = ResolveScriptConfigPath(_jobName, _taskName, _config.StepName, _config.ScriptFilePath);
			}

			File.WriteAllText(_config.ScriptFilePath, txtCode.Text, System.Text.Encoding.UTF8);
			CSharpScriptStepStore.Save(_configPath, _config);
		}

		private void btnCompile_Click(object sender, EventArgs e)
		{
			try
			{
				ClearLogs();
				SetStatusRunning("Compiling...");
				SaveCurrentScriptAndConfigOnly();

				CSharpScriptStepRunner runner = new CSharpScriptStepRunner();
				CompilerResultProxy compile = CompileOnly(runner);

				if (compile.HasError)
				{
					LogError(compile.Message);
					SetStatusError("Compile Error");
				}
				else
				{
					LogInfo("Compile OK. Cost=" + compile.Cost.TotalMilliseconds.ToString("0.0") + " ms");
					SetStatusOK("Compile OK");
				}
			}
			catch (Exception ex)
			{
				LogError("Compile failed: " + ex.Message);
				SetStatusError("Compile Error");
			}
		}

		private void btnRun_Click(object sender, EventArgs e)
		{
			try
			{
				ClearLogs();
				SetStatusRunning("Running...");
				SaveCurrentScriptAndConfigOnly();

				btnRun.Enabled = false;
				btnCompile.Enabled = false;

				Dictionary<string, object> runtimeInputs = BuildRuntimeInputFromGrid();

				CSharpScriptStepRunner runner = new CSharpScriptStepRunner();
				CSharpScriptRunResult result = runner.CompileAndRun(_config, txtCode.Text, runtimeInputs);

				if (!result.IsCompileOK)
				{
					LogError(result.ErrorDetail);
					SetStatusError("Compile Error");
					return;
				}

				if (!result.IsRunOK)
				{
					LogError(result.ErrorDetail);
					SetStatusError("Run Error");
					return;
				}

				LogOutputsToGrid(result.Outputs);

				LogInfo(
					"Run OK. Compile=" +
					result.CompileCost.TotalMilliseconds.ToString("0.0") +
					" ms, Run=" +
					result.RunCost.TotalMilliseconds.ToString("0.0") +
					" ms");

				SetStatusOK("Run OK");
			}
			catch (Exception ex)
			{
				LogError("Run failed: " + ex.Message);
				SetStatusError("Run Error");
			}
			finally
			{
				btnRun.Enabled = true;
				btnCompile.Enabled = true;
			}
		}

		private Dictionary<string, object> BuildRuntimeInputFromGrid()
		{
			Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			if (gridInputs == null)
			{
				return dict;
			}

			foreach (DataGridViewRow row in gridInputs.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string name = Convert.ToString(row.Cells["Name"].Value);
				string bindingPath = Convert.ToString(row.Cells["BindingPath"].Value);
				string value = Convert.ToString(row.Cells["DefaultValue"].Value);

				if (!string.IsNullOrWhiteSpace(name))
				{
					dict[name.Trim()] = value;
				}

				if (!string.IsNullOrWhiteSpace(bindingPath))
				{
					dict[bindingPath.Trim()] = value;
				}
			}

			return dict;
		}

		private CompilerResultProxy CompileOnly(CSharpScriptStepRunner runner)
		{
			CompilerResultProxy proxy = new CompilerResultProxy();

			System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
			System.CodeDom.Compiler.CompilerResults cr = runner.Compile(_config, txtCode.Text);
			sw.Stop();

			proxy.Cost = sw.Elapsed;

			if (cr.Errors.HasErrors)
			{
				proxy.HasError = true;

				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				foreach (System.CodeDom.Compiler.CompilerError error in cr.Errors)
				{
					sb.AppendLine("Line " + error.Line + ", Column " + error.Column + ", " + error.ErrorNumber + ": " + error.ErrorText);
				}

				proxy.Message = sb.ToString();
			}
			else
			{
				proxy.HasError = false;
				proxy.Message = "Compile OK";
			}

			return proxy;
		}

		private void LogOutputsToGrid(Dictionary<string, object> outputs)
		{
			if (outputs == null || outputs.Count <= 0)
			{
				LogInfo("No script output.");
				return;
			}

			foreach (KeyValuePair<string, object> pair in outputs)
			{
				LogInfo("Output: " + pair.Key + " = " + Convert.ToString(pair.Value));
			}
		}

		private void ClearLogs()
		{
			if (gridLogs != null)
			{
				gridLogs.Rows.Clear();
			}
		}

		private void LogInfo(string message)
		{
			AddLog("INFO", message, _green);
		}

		private void LogError(string message)
		{
			AddLog("ERROR", message, _red);
		}

		private void AddLog(string level, string message, Color color)
		{
			if (gridLogs == null)
			{
				return;
			}

			int rowIndex = gridLogs.Rows.Add();
			DataGridViewRow row = gridLogs.Rows[rowIndex];

			row.Cells["Time"].Value = DateTime.Now.ToString("HH:mm:ss.fff");
			row.Cells["Level"].Value = level;
			row.Cells["Message"].Value = message;

			row.Cells["Level"].Style.ForeColor = color;
			row.Cells["Message"].Style.ForeColor = color;

			if (gridLogs.Rows.Count > 0)
			{
				try
				{
					gridLogs.FirstDisplayedScrollingRowIndex = gridLogs.Rows.Count - 1;
				}
				catch
				{
				}
			}
		}

		private void SetStatusReady()
		{
			lblStatusLight.BackColor = Color.FromArgb(120, 120, 120);
			lblStatusText.Text = "Ready";
			lblStatusText.ForeColor = _muted;
		}

		private void SetStatusRunning(string text)
		{
			lblStatusLight.BackColor = Color.FromArgb(0, 150, 220);
			lblStatusText.Text = text;
			lblStatusText.ForeColor = Color.FromArgb(0, 180, 255);
		}

		private void SetStatusOK(string text)
		{
			lblStatusLight.BackColor = _green;
			lblStatusText.Text = text;
			lblStatusText.ForeColor = _green;
		}

		private void SetStatusError(string text)
		{
			lblStatusLight.BackColor = _red;
			lblStatusText.Text = text;
			lblStatusText.ForeColor = _red;
		}

		#region Code Completion

		private void InitCodeCompletion()
		{
			_completionItems = BuildCompletionItems();

			_completionList = new ListBox();
			_completionList.Visible = false;
			_completionList.IntegralHeight = false;
			_completionList.BorderStyle = BorderStyle.FixedSingle;
			_completionList.BackColor = Color.FromArgb(6, 22, 40);
			_completionList.ForeColor = _text;
			_completionList.Font = new Font("Consolas", 9F, FontStyle.Regular);
			_completionList.ItemHeight = 18;
			_completionList.Width = 360;
			_completionList.Height = 180;
			_completionList.DoubleClick += delegate { InsertSelectedCompletion(); };
			_completionList.MouseDown += delegate { txtCode.Focus(); };

			if (codeEditorHost != null)
			{
				codeEditorHost.Controls.Add(_completionList);
				_completionList.BringToFront();
			}
		}

		private List<CompletionItem> BuildCompletionItems()
		{
			List<CompletionItem> items = new List<CompletionItem>();

			AddCompletion(items, "public", "public");
			AddCompletion(items, "private", "private");
			AddCompletion(items, "protected", "protected");
			AddCompletion(items, "class", "class");
			AddCompletion(items, "void", "void");
			AddCompletion(items, "return", "return");
			AddCompletion(items, "if", "if");
			AddCompletion(items, "else", "else");
			AddCompletion(items, "for", "for");
			AddCompletion(items, "foreach", "foreach");
			AddCompletion(items, "while", "while");
			AddCompletion(items, "try", "try");
			AddCompletion(items, "catch", "catch");
			AddCompletion(items, "finally", "finally");
			AddCompletion(items, "new", "new");
			AddCompletion(items, "null", "null");
			AddCompletion(items, "true", "true");
			AddCompletion(items, "false", "false");
			AddCompletion(items, "string", "string");
			AddCompletion(items, "int", "int");
			AddCompletion(items, "double", "double");
			AddCompletion(items, "decimal", "decimal");
			AddCompletion(items, "bool", "bool");
			AddCompletion(items, "object", "object");
			AddCompletion(items, "var", "var");

			AddCompletion(items, "MessageBox.Show", "MessageBox.Show(\"message\");");
			AddCompletion(items, "Math.Abs", "Math.Abs(value)");
			AddCompletion(items, "Math.Round", "Math.Round(value, 3)");
			AddCompletion(items, "Convert.ToString", "Convert.ToString(value)");
			AddCompletion(items, "Convert.ToInt32", "Convert.ToInt32(value)");
			AddCompletion(items, "Convert.ToDouble", "Convert.ToDouble(value)");
			AddCompletion(items, "DateTime.Now", "DateTime.Now");
			AddCompletion(items, "string.Format", "string.Format(\"{0}\", value)");
			AddCompletion(items, "List<string>", "List<string> list = new List<string>();");
			AddCompletion(items, "List<double>", "List<double> list = new List<double>();");
			AddCompletion(items, "Dictionary<string, object>", "Dictionary<string, object> dict = new Dictionary<string, object>();");

			AddCompletion(items, "IScriptMain", "IScriptMain");
			AddCompletion(items, "IScriptContext", "IScriptContext");
			AddCompletion(items, "ScriptMain", "ScriptMain");
			AddCompletion(items, "context.GetInput", "context.GetInput(\"Name\")");
			AddCompletion(items, "context.GetInputString", "context.GetInputString(\"Name\")");
			AddCompletion(items, "context.GetInputInt", "context.GetInputInt(\"Name\")");
			AddCompletion(items, "context.GetInputDouble", "context.GetInputDouble(\"Name\")");
			AddCompletion(items, "context.GetInputBool", "context.GetInputBool(\"Name\")");
			AddCompletion(items, "context.SetOutput", "context.SetOutput(\"Name\", value);");
			AddCompletion(items, "context.GetOutput", "context.GetOutput(\"Name\")");

			AddCompletion(items, "snippet: ScriptMain", "public class ScriptMain : IScriptMain\r\n{\r\n    public void Execute(IScriptContext context)\r\n    {\r\n        // TODO\r\n    }\r\n}\r\n");
			AddCompletion(items, "snippet: if block", "if (condition)\r\n{\r\n    \r\n}");
			AddCompletion(items, "snippet: for loop", "for (int i = 0; i < count; i++)\r\n{\r\n    \r\n}");
			AddCompletion(items, "snippet: foreach loop", "foreach (var item in items)\r\n{\r\n    \r\n}");
			AddCompletion(items, "snippet: try catch", "try\r\n{\r\n    \r\n}\r\ncatch (Exception ex)\r\n{\r\n    MessageBox.Show(ex.Message);\r\n}");

			return items;
		}

		private void AddCompletion(List<CompletionItem> items, string displayText, string insertText)
		{
			items.Add(new CompletionItem(displayText, insertText));
		}

		private void RefreshDynamicCompletionItems()
		{
			_completionItems = BuildCompletionItems();

			AddPinCompletionItems(gridInputs, true);
			AddPinCompletionItems(gridOutputs, false);
		}

		private void AddPinCompletionItems(DataGridView grid, bool isInput)
		{
			if (grid == null || _completionItems == null)
			{
				return;
			}

			foreach (DataGridViewRow row in grid.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string name = Convert.ToString(row.Cells["Name"].Value);
				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				string cleanName = name.Trim();
				ScriptPinDataType dataType = ParseDataType(Convert.ToString(row.Cells["DataType"].Value));

				if (isInput)
				{
					string methodName = "GetInputString";
					if (dataType == ScriptPinDataType.Int) methodName = "GetInputInt";
					else if (dataType == ScriptPinDataType.Double || dataType == ScriptPinDataType.Decimal) methodName = "GetInputDouble";
					else if (dataType == ScriptPinDataType.Bool) methodName = "GetInputBool";
					else if (dataType == ScriptPinDataType.Object) methodName = "GetInput";

					AddCompletion(_completionItems, "input: " + cleanName, "context." + methodName + "(\"" + cleanName + "\")");
				}
				else
				{
					AddCompletion(_completionItems, "output: " + cleanName, "context.SetOutput(\"" + cleanName + "\", value);");
				}
			}
		}

		private void txtCode_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.Space)
			{
				ShowCompletion(true);
				e.SuppressKeyPress = true;
				return;
			}

			if (_completionList != null && _completionList.Visible)
			{
				if (e.KeyCode == Keys.Down)
				{
					MoveCompletionSelection(1);
					e.SuppressKeyPress = true;
					return;
				}

				if (e.KeyCode == Keys.Up)
				{
					MoveCompletionSelection(-1);
					e.SuppressKeyPress = true;
					return;
				}

				if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
				{
					InsertSelectedCompletion();
					e.SuppressKeyPress = true;
					return;
				}

				if (e.KeyCode == Keys.Escape)
				{
					HideCompletion();
					e.SuppressKeyPress = true;
					return;
				}
			}
		}

		private void txtCode_KeyUp(object sender, KeyEventArgs e)
		{
			if (_completionUpdating)
			{
				return;
			}

			if (e.Control || e.Alt)
			{
				return;
			}

			if (IsTextKey(e.KeyCode) || e.KeyCode == Keys.OemPeriod || e.KeyCode == Keys.Decimal)
			{
				ShowCompletion(false);
			}
			else if (e.KeyCode == Keys.Back)
			{
				ShowCompletion(false);
			}
			else if (e.KeyCode != Keys.Down && e.KeyCode != Keys.Up)
			{
				HideCompletion();
			}
		}

		private bool IsTextKey(Keys keyCode)
		{
			return (keyCode >= Keys.A && keyCode <= Keys.Z) ||
				   (keyCode >= Keys.D0 && keyCode <= Keys.D9) ||
				   (keyCode >= Keys.NumPad0 && keyCode <= Keys.NumPad9) ||
				   keyCode == Keys.OemMinus ||
				   keyCode == Keys.Subtract;
		}

		private void txtCode_LostFocus(object sender, EventArgs e)
		{
			if (_completionList != null && !_completionList.Focused)
			{
				HideCompletion();
			}
		}

		private void ShowCompletion(bool forceShow)
		{
			if (txtCode == null || _completionList == null || txtCode.SelectionStart < 0)
			{
				return;
			}

			RefreshDynamicCompletionItems();

			string token = GetCurrentCompletionToken();
			if (!forceShow && token.Length < 1)
			{
				HideCompletion();
				return;
			}

			List<CompletionItem> filtered = FilterCompletionItems(token, forceShow);
			if (filtered.Count <= 0)
			{
				HideCompletion();
				return;
			}

			_completionUpdating = true;
			_completionList.BeginUpdate();
			_completionList.Items.Clear();

			foreach (CompletionItem item in filtered)
			{
				_completionList.Items.Add(item);
			}

			_completionList.EndUpdate();
			_completionList.SelectedIndex = 0;
			_completionUpdating = false;

			Point charPoint = txtCode.GetPositionFromCharIndex(txtCode.SelectionStart);
			int x = txtCode.Left + charPoint.X;
			int y = txtCode.Top + charPoint.Y + txtCode.Font.Height + 4;

			if (x + _completionList.Width > codeEditorHost.ClientSize.Width)
			{
				x = Math.Max(0, codeEditorHost.ClientSize.Width - _completionList.Width - 2);
			}

			if (y + _completionList.Height > codeEditorHost.ClientSize.Height)
			{
				y = Math.Max(0, txtCode.Top + charPoint.Y - _completionList.Height - 4);
			}

			_completionList.Location = new Point(x, y);
			_completionList.Visible = true;
			_completionList.BringToFront();
			txtCode.Focus();
		}

		private List<CompletionItem> FilterCompletionItems(string token, bool forceShow)
		{
			List<CompletionItem> result = new List<CompletionItem>();

			if (_completionItems == null)
			{
				return result;
			}

			string filter = token == null ? string.Empty : token.Trim();
			string filterLower = filter.ToLowerInvariant();

			foreach (CompletionItem item in _completionItems)
			{
				if (item == null)
				{
					continue;
				}

				if (forceShow || string.IsNullOrEmpty(filterLower))
				{
					result.Add(item);
					continue;
				}

				string display = item.DisplayText == null ? string.Empty : item.DisplayText.ToLowerInvariant();
				string insert = item.InsertText == null ? string.Empty : item.InsertText.ToLowerInvariant();

				if (display.StartsWith(filterLower) || insert.StartsWith(filterLower) || display.IndexOf(filterLower) >= 0)
				{
					result.Add(item);
				}
			}

			return result;
		}

		private string GetCurrentCompletionToken()
		{
			int start = GetCurrentCompletionTokenStart();
			_completionStartIndex = start;

			int length = txtCode.SelectionStart - start;
			if (length <= 0)
			{
				return string.Empty;
			}

			return txtCode.Text.Substring(start, length);
		}

		private int GetCurrentCompletionTokenStart()
		{
			int pos = txtCode.SelectionStart;
			string text = txtCode.Text;
			int start = pos;

			while (start > 0)
			{
				char c = text[start - 1];
				if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
				{
					start--;
					continue;
				}

				break;
			}

			return start;
		}

		private void MoveCompletionSelection(int offset)
		{
			if (_completionList == null || _completionList.Items.Count <= 0)
			{
				return;
			}

			int index = _completionList.SelectedIndex;
			if (index < 0)
			{
				index = 0;
			}

			index += offset;
			if (index < 0) index = _completionList.Items.Count - 1;
			if (index >= _completionList.Items.Count) index = 0;

			_completionList.SelectedIndex = index;
		}

		private void InsertSelectedCompletion()
		{
			if (_completionList == null || !_completionList.Visible || _completionList.SelectedItem == null)
			{
				return;
			}

			CompletionItem item = _completionList.SelectedItem as CompletionItem;
			if (item == null)
			{
				return;
			}

			int current = txtCode.SelectionStart;
			int start = _completionStartIndex;
			if (start < 0 || start > current)
			{
				start = GetCurrentCompletionTokenStart();
			}

			_completionUpdating = true;
			txtCode.Select(start, current - start);
			txtCode.SelectedText = item.InsertText;
			txtCode.SelectionStart = start + item.InsertText.Length;
			_completionUpdating = false;

			HideCompletion();
			RefreshCodeLineNumbers();
		}

		private void HideCompletion()
		{
			if (_completionList != null)
			{
				_completionList.Visible = false;
			}
		}

		private class CompletionItem
		{
			public string DisplayText { get; private set; }
			public string InsertText { get; private set; }

			public CompletionItem(string displayText, string insertText)
			{
				DisplayText = displayText ?? string.Empty;
				InsertText = insertText ?? string.Empty;
			}

			public override string ToString()
			{
				return DisplayText;
			}
		}

		#endregion

		private void txtCode_TextChanged(object sender, EventArgs e)
		{
			RefreshCodeLineNumbers();
		}

		private void txtCode_VScroll(object sender, EventArgs e)
		{
			RefreshCodeLineNumbers();
		}

		private void txtCode_Resize(object sender, EventArgs e)
		{
			RefreshCodeLineNumbers();
		}

		private void txtCode_FontChanged(object sender, EventArgs e)
		{
			RefreshCodeLineNumbers();
		}

		private void RefreshCodeLineNumbers()
		{
			if (panelLineNumbers == null || panelLineNumbers.IsDisposed)
			{
				return;
			}

			panelLineNumbers.Invalidate();
		}

		private void panelLineNumbers_Paint(object sender, PaintEventArgs e)
		{
			if (txtCode == null || panelLineNumbers == null)
			{
				return;
			}

			e.Graphics.Clear(_panel2);

			using (SolidBrush textBrush = new SolidBrush(_muted))
			using (Pen borderPen = new Pen(_border))
			{
				e.Graphics.DrawLine(
					borderPen,
					panelLineNumbers.Width - 1,
					0,
					panelLineNumbers.Width - 1,
					panelLineNumbers.Height);

				int firstCharIndex = txtCode.GetCharIndexFromPosition(new Point(0, 0));
				int firstLine = txtCode.GetLineFromCharIndex(firstCharIndex);

				int lastCharIndex = txtCode.GetCharIndexFromPosition(new Point(0, txtCode.ClientSize.Height - 1));
				int lastLine = txtCode.GetLineFromCharIndex(lastCharIndex);

				if (lastLine < firstLine)
				{
					lastLine = firstLine;
				}

				int totalLines = txtCode.Lines == null ? 1 : Math.Max(1, txtCode.Lines.Length);
				lastLine = Math.Min(lastLine + 1, totalLines - 1);

				for (int line = firstLine; line <= lastLine; line++)
				{
					int firstCharOfLine = txtCode.GetFirstCharIndexFromLine(line);

					if (firstCharOfLine < 0)
					{
						continue;
					}

					Point pos = txtCode.GetPositionFromCharIndex(firstCharOfLine);
					string lineNo = (line + 1).ToString();

					SizeF size = e.Graphics.MeasureString(lineNo, txtCode.Font);
					float x = panelLineNumbers.Width - size.Width - 6;
					float y = pos.Y + 1;

					e.Graphics.DrawString(lineNo, txtCode.Font, textBrush, x, y);
				}
			}
		}

		private class CompilerResultProxy
		{
			public bool HasError { get; set; }
			public string Message { get; set; }
			public TimeSpan Cost { get; set; }

			public CompilerResultProxy()
			{
				Message = string.Empty;
			}
		}
	}
}
