using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

		// 行号刷新优化：不要每敲一个字符就重绘，避免 RichTextBox 左侧行号闪烁/跳动。
		private Timer _lineNumberRefreshTimer;
		private int _lastKnownCodeLineCount = -1;
		private bool _forceLineNumberRefresh;
		private bool _loadingCodeText;

		// Script 输入 BindingPath 下拉来源。
		// 来源包括：任务调度中当前 Script 已选择的前序模块输出引脚、当前脚本已有绑定值；不再默认加入 Comm.* 示例项。
		private List<string> _bindingSourceOptions = new List<string>();

		// 左侧 Script Pin 区域当前显示 Inputs 还是 Outputs。
		private bool _showInputPins = true;

		public CSharpScriptStepEditorControl()
		{
			InitializeComponent();
			EnableSmoothUi();

			_config = CSharpScriptStepStore.CreateDefaultConfig();

			InitTheme();
			InitGrids();
			InitializePinToggleLayout();
			BindEvents();
			ShowPinPage(true);

			LoadConfigToUi();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;


			lblStepName.Text = isEnglish ? "Current Script" : "当前脚本";
			lblStatusTitle.Text = isEnglish ? "Status" : "状态";
			btnReferenceDll.Text = isEnglish ? "References" : "引用信息";
			btnSave.Text = isEnglish ? "Save" : "保存";
			btnCompile.Text = isEnglish ? "Compile" : "编译";
			btnRun.Text = isEnglish ? "Debug Run" : "调试运行";
			lblStatusTitle.Text = isEnglish ? "Status" : "状态";
			lblInputTitle.Text = isEnglish ? "Inputs    Edit Current/Default value for debug" : "输入定义 Inputs    调试时直接修改“当前值/默认值”列";
			lblOutputTitle.Text = isEnglish ? "Outputs" : "输出定义 Outputs";
			lblCodeTitle.Text = isEnglish ? "C# Script Code" : "C# Script Code";
			lblLogTitle.Text = isEnglish ? "Compile / Run Log" : "编译 / 运行日志";

			if (btnShowInputs != null) btnShowInputs.Text = isEnglish ? "Inputs" : "输入";
			if (btnShowOutputs != null) btnShowOutputs.Text = isEnglish ? "Outputs" : "输出";

			SetGridHeaders();
			UpdatePinToolbarText();
		}

		public void LoadScriptStep(string jobName, string taskName, string stepName)
		{
			_jobName = string.IsNullOrWhiteSpace(jobName) ? "Job_001" : jobName;
			_taskName = string.IsNullOrWhiteSpace(taskName) ? "Task_New_01" : taskName;

			string rawSelection = string.IsNullOrWhiteSpace(stepName) ? "CS_Script" : stepName.Trim();
			string safeStep = NormalizeScriptSelectionName(rawSelection);

			// 切换脚本时先清空旧日志，避免看起来还停留在上一个脚本。
			ClearLogs();

			StepConfig flowStep = FindScriptStepConfig(_jobName, _taskName, rawSelection);
			_scriptPath = ResolveScriptPath(_jobName, _taskName, rawSelection, flowStep);

			// 兼容外部传入不带扩展名的 StepName。
			if ((string.IsNullOrWhiteSpace(_scriptPath) || !File.Exists(_scriptPath)) &&
				!string.Equals(rawSelection, safeStep, StringComparison.OrdinalIgnoreCase))
			{
				flowStep = FindScriptStepConfig(_jobName, _taskName, safeStep);
				_scriptPath = ResolveScriptPath(_jobName, _taskName, safeStep, flowStep);
			}

			_configPath = ResolveScriptConfigPath(_jobName, _taskName, safeStep, _scriptPath);

			_config = CSharpScriptStepStore.Load(_configPath);
			if (_config == null)
			{
				_config = CSharpScriptStepStore.CreateDefaultConfig();
			}

			_config.StepName = GetScriptDisplayName(safeStep, _scriptPath);
			_config.Enable = true;
			_config.ScriptFilePath = _scriptPath;
			_config.ScriptFileName = Path.GetFileName(_scriptPath);

			// 不再自动创建默认 CS_Script.csx。
			// 只加载“所有 Script”列表中双击选中的脚本文件。
			LoadConfigToUi();

			if (!string.IsNullOrWhiteSpace(_scriptPath) && File.Exists(_scriptPath))
			{
				SetCodeTextSafely(File.ReadAllText(_scriptPath, System.Text.Encoding.UTF8));
				RefreshCodeLineNumbersNow();
				SetStatusReady();
				LogInfo("Script step loaded: " + Path.GetFileNameWithoutExtension(_scriptPath));
			}
			else
			{
				SetCodeTextSafely(string.Empty);
				RefreshCodeLineNumbersNow();
				LogError("Script file was not found. Selected: " + rawSelection);
				SetStatusError("Script file not found");
			}
		}

		/// <summary>
		/// 给 AlgorithmModuleControl 使用：如果左侧 Script 列表拿到的是文件名或完整路径，直接调用这个方法。
		/// </summary>
		public void LoadScriptFile(string jobName, string taskName, string scriptFilePathOrName)
		{
			LoadScriptStep(jobName, taskName, scriptFilePathOrName);
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

				string raw = string.IsNullOrWhiteSpace(stepName) ? string.Empty : stepName.Trim();
				string normalized = NormalizeScriptSelectionName(raw);

				StepConfig byName = task.Steps.FirstOrDefault(s =>
					s.StepType == StepType.Script &&
					(string.Equals(s.StepName, raw, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(s.StepName, normalized, StringComparison.OrdinalIgnoreCase)));

				if (byName != null) return byName;

				return task.Steps.FirstOrDefault(s =>
					s.StepType == StepType.Script &&
					IsScriptStepFileNameMatch(s, raw));
			}
			catch
			{
				return null;
			}
		}

		private bool IsScriptStepFileNameMatch(StepConfig step, string name)
		{
			if (step == null || string.IsNullOrWhiteSpace(name)) return false;

			string n = NormalizeScriptSelectionName(name);

			if (!string.IsNullOrWhiteSpace(step.ProjectFilePath) && IsSameScriptName(step.ProjectFilePath, n)) return true;
			if (!string.IsNullOrWhiteSpace(step.SourceFilePath) && IsSameScriptName(step.SourceFilePath, n)) return true;

			if (step.ScriptFiles != null)
			{
				foreach (string f in step.ScriptFiles)
				{
					if (IsSameScriptName(f, n)) return true;
				}
			}

			return false;
		}

		private bool IsSameScriptName(string pathOrName, string normalizedName)
		{
			if (string.IsNullOrWhiteSpace(pathOrName) || string.IsNullOrWhiteSpace(normalizedName)) return false;
			string name = NormalizeScriptSelectionName(pathOrName);
			return string.Equals(name, normalizedName, StringComparison.OrdinalIgnoreCase);
		}

		private string ResolveScriptPath(string jobName, string taskName, string stepName, StepConfig step)
		{
			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(jobName, taskName);
			string scriptsFolder = Path.Combine(taskFolder, "Scripts");

			// 1. 如果外部直接传入完整文件路径，优先使用。
			if (!string.IsNullOrWhiteSpace(stepName) && Path.IsPathRooted(stepName) && IsScriptCodeFile(stepName) && File.Exists(stepName))
			{
				return stepName;
			}

			// 2. 优先用 StepConfig 里的 ProjectFilePath / ScriptFiles / SourceFilePath。
			if (step != null)
			{
				string candidate = ResolveStepFilePath(taskFolder, step.ProjectFilePath);
				if (IsScriptCodeFile(candidate) && File.Exists(candidate)) return candidate;

				if (step.ScriptFiles != null && step.ScriptFiles.Count > 0)
				{
					foreach (string relative in step.ScriptFiles)
					{
						candidate = ResolveStepFilePath(taskFolder, relative);
						if (IsScriptCodeFile(candidate) && File.Exists(candidate)) return candidate;
					}
				}

				if (!string.IsNullOrWhiteSpace(step.SourceFilePath) && IsScriptCodeFile(step.SourceFilePath) && File.Exists(step.SourceFilePath))
				{
					return step.SourceFilePath;
				}
			}

			// 3. 再按左侧列表传进来的文件名查找。
			string file = FindScriptFileInTaskFolder(scriptsFolder, stepName);
			if (!string.IsNullOrWhiteSpace(file)) return file;

			// 4. 最后按去扩展名后的 StepName 兜底。
			string normalized = NormalizeScriptSelectionName(stepName);
			file = FindScriptFileInTaskFolder(scriptsFolder, normalized);
			if (!string.IsNullOrWhiteSpace(file)) return file;

			return Path.Combine(scriptsFolder, MakeSafeFileName(normalized) + ".csx");
		}

		private string FindScriptFileInTaskFolder(string scriptsFolder, string scriptNameOrFileName)
		{
			if (string.IsNullOrWhiteSpace(scriptsFolder) || !Directory.Exists(scriptsFolder))
			{
				return string.Empty;
			}

			if (string.IsNullOrWhiteSpace(scriptNameOrFileName))
			{
				return string.Empty;
			}

			string fileName = Path.GetFileName(scriptNameOrFileName.Trim());
			string normalized = NormalizeScriptSelectionName(fileName);

			// 先精确匹配完整文件名，例如 Step_New_02.csx。
			foreach (string file in Directory.GetFiles(scriptsFolder, "*.*", SearchOption.TopDirectoryOnly))
			{
				if (!IsScriptCodeFile(file)) continue;
				if (string.Equals(Path.GetFileName(file), fileName, StringComparison.OrdinalIgnoreCase)) return file;
			}

			// 再匹配不带扩展名，例如 Step_New_02。
			foreach (string file in Directory.GetFiles(scriptsFolder, "*.*", SearchOption.TopDirectoryOnly))
			{
				if (!IsScriptCodeFile(file)) continue;
				if (string.Equals(Path.GetFileNameWithoutExtension(file), normalized, StringComparison.OrdinalIgnoreCase)) return file;
			}

			return string.Empty;
		}

		private bool IsScriptCodeFile(string path)
		{
			if (string.IsNullOrWhiteSpace(path)) return false;
			string ext = Path.GetExtension(path);
			return ext.Equals(".csx", StringComparison.OrdinalIgnoreCase) ||
				   ext.Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
				   ext.Equals(".txt", StringComparison.OrdinalIgnoreCase);
		}

		private string NormalizeScriptSelectionName(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return "CS_Script";

			string fileName = Path.GetFileName(name.Trim());
			string ext = Path.GetExtension(fileName);

			if (ext.Equals(".csx", StringComparison.OrdinalIgnoreCase) ||
				ext.Equals(".cs", StringComparison.OrdinalIgnoreCase) ||
				ext.Equals(".txt", StringComparison.OrdinalIgnoreCase))
			{
				fileName = Path.GetFileNameWithoutExtension(fileName);
			}

			return MakeSafeFileName(fileName);
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
			StyleButton(btnShowInputs);
			StyleButton(btnShowOutputs);
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

		private void RebuildTopBarLayout()
		{
			if (topPanel == null)
			{
				return;
			}

			topPanel.SuspendLayout();

			try
			{
				// 顶部只保留一行：当前脚本 + 脚本名 + 状态 + DLL目录 + 保存 + 编译 + 调试运行。
				// 不再依赖 Designer 里的 topLayout 两行布局，避免按钮被 TableLayoutPanel 挤到不可见区域。
				if (rootLayout != null && rootLayout.RowStyles.Count > 0)
				{
					rootLayout.RowStyles[0].SizeType = SizeType.Absolute;
					rootLayout.RowStyles[0].Height = 68F;
				}

				topPanel.Margin = new Padding(0, 0, 0, 8);
				topPanel.Padding = new Padding(8, 6, 8, 6);
				topPanel.BackColor = _back;

				// 先从旧 topLayout/statusPanel 中移除，改为直接放到 topPanel 上做绝对布局。
				// WinForms 控件只能有一个 Parent，Controls.Add 会自动从旧 Parent 移出。
				topPanel.Controls.Clear();

				if (topLayout != null)
				{
					topLayout.Visible = false;
				}

				if (chkEnable != null) chkEnable.Visible = false;
				if (lblScriptFile != null) lblScriptFile.Visible = false;
				if (txtScriptPath != null) txtScriptPath.Visible = false;
				if (btnBrowseScript != null) btnBrowseScript.Visible = false;

				PrepareHeaderLabel(lblStepName, _isEnglish ? "Current Script" : "当前脚本", true);
				PrepareHeaderScriptNameBox(txtStepName);
				PrepareHeaderLabel(lblStatusTitle, _isEnglish ? "Status" : "状态", true);
				PrepareHeaderStatusPanel();
				PrepareHeaderButton(btnReferenceDll, _isEnglish ? "References" : "引用信息");
				PrepareHeaderButton(btnSave, _isEnglish ? "Save" : "保存");
				PrepareHeaderButton(btnCompile, _isEnglish ? "Compile" : "编译");
				PrepareHeaderButton(btnRun, _isEnglish ? "Debug Run" : "调试运行");

				topPanel.Controls.Add(lblStepName);
				topPanel.Controls.Add(txtStepName);
				topPanel.Controls.Add(lblStatusTitle);
				topPanel.Controls.Add(statusPanel);
				topPanel.Controls.Add(btnReferenceDll);
				topPanel.Controls.Add(btnSave);
				topPanel.Controls.Add(btnCompile);
				topPanel.Controls.Add(btnRun);

				btnReferenceDll.BringToFront();
				btnSave.BringToFront();
				btnCompile.BringToFront();
				btnRun.BringToFront();

				topPanel.Resize -= TopPanel_Resize;
				topPanel.Resize += TopPanel_Resize;

				LayoutTopBarControls();
			}
			finally
			{
				topPanel.ResumeLayout(true);
			}
		}

		private void TopPanel_Resize(object sender, EventArgs e)
		{
			LayoutTopBarControls();
		}

		private void PrepareHeaderLabel(Label label, string text, bool bold)
		{
			if (label == null)
			{
				return;
			}

			label.Visible = true;
			label.AutoSize = false;
			label.Dock = DockStyle.None;
			label.Margin = new Padding(0);
			label.Padding = new Padding(0);
			label.Text = text;
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.BackColor = _back;
			label.ForeColor = _text;
			label.Font = new Font("Microsoft YaHei UI", 9F, bold ? FontStyle.Bold : FontStyle.Regular);
		}

		private void PrepareHeaderScriptNameBox(TextBox textBox)
		{
			if (textBox == null)
			{
				return;
			}

			textBox.Visible = true;
			textBox.Dock = DockStyle.None;
			textBox.Margin = new Padding(0);
			textBox.BorderStyle = BorderStyle.None;
			textBox.ReadOnly = true;
			textBox.TabStop = false;
			textBox.BackColor = _back;
			textBox.ForeColor = Color.White;
			textBox.Font = new Font("Consolas", 10F, FontStyle.Bold);
			textBox.TextAlign = HorizontalAlignment.Left;
		}

		private void PrepareHeaderStatusPanel()
		{
			if (statusPanel == null)
			{
				return;
			}

			statusPanel.Visible = true;
			statusPanel.Dock = DockStyle.None;
			statusPanel.Margin = new Padding(0);
			statusPanel.Padding = new Padding(0);
			statusPanel.BackColor = _back;
			statusPanel.Controls.Clear();

			lblStatusText.Visible = true;
			lblStatusText.Dock = DockStyle.Fill;
			lblStatusText.Margin = new Padding(0);
			lblStatusText.Padding = new Padding(28, 0, 0, 0);
			lblStatusText.TextAlign = ContentAlignment.MiddleLeft;
			lblStatusText.BackColor = _back;
			lblStatusText.ForeColor = _muted;
			lblStatusText.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			if (string.IsNullOrWhiteSpace(lblStatusText.Text))
			{
				lblStatusText.Text = "Ready";
			}

			lblStatusLight.Visible = true;
			lblStatusLight.AutoSize = false;
			lblStatusLight.BackColor = lblStatusLight.BackColor == Color.Empty ? Color.FromArgb(120, 120, 120) : lblStatusLight.BackColor;
			lblStatusLight.Width = 14;
			lblStatusLight.Height = 14;

			statusPanel.Controls.Add(lblStatusText);
			statusPanel.Controls.Add(lblStatusLight);
			lblStatusLight.BringToFront();
		}

		private void PrepareHeaderButton(Button button, string text)
		{
			if (button == null)
			{
				return;
			}

			button.Visible = true;
			button.Enabled = true;
			button.Dock = DockStyle.None;
			button.Margin = new Padding(0);
			button.Text = text;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = _accent;
			button.FlatAppearance.BorderSize = 1;
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
			button.BackColor = button == btnRun ? Color.FromArgb(20, 125, 40) : Color.FromArgb(0, 95, 190);
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
			button.UseVisualStyleBackColor = false;
		}

		private void LayoutTopBarControls()
		{
			if (topPanel == null || topPanel.IsDisposed)
			{
				return;
			}

			int clientW = topPanel.ClientSize.Width;
			int clientH = topPanel.ClientSize.Height;

			if (clientW <= 0 || clientH <= 0)
			{
				return;
			}

			int pad = 10;
			int gap = 8;
			int h = Math.Max(28, clientH - pad * 2);
			if (h > 38) h = 38;
			int y = Math.Max(4, (clientH - h) / 2);

			int dllW = 110;
			int saveW = 90;
			int compileW = 90;
			int runW = 120;
			int statusTitleW = _isEnglish ? 58 : 44;
			int statusW = 145;
			int scriptTitleW = _isEnglish ? 108 : 76;

			// 从右往左摆按钮，保证按钮永远优先显示。
			int right = clientW - pad;

			SetControlBounds(btnRun, right - runW, y, runW, h);
			right -= runW + gap;

			SetControlBounds(btnCompile, right - compileW, y, compileW, h);
			right -= compileW + gap;

			SetControlBounds(btnSave, right - saveW, y, saveW, h);
			right -= saveW + gap;

			SetControlBounds(btnReferenceDll, right - dllW, y, dllW, h);
			right -= dllW + gap;

			SetControlBounds(statusPanel, right - statusW, y, statusW, h);
			right -= statusW + gap;

			SetControlBounds(lblStatusTitle, right - statusTitleW, y, statusTitleW, h);
			right -= statusTitleW + gap;

			int left = pad;
			SetControlBounds(lblStepName, left, y, scriptTitleW, h);
			left += scriptTitleW + gap;

			int nameW = right - left;
			if (nameW < 40)
			{
				nameW = 40;
			}

			SetControlBounds(txtStepName, left, y + 8, nameW, Math.Max(20, h - 12));

			if (lblStatusLight != null && statusPanel != null)
			{
				lblStatusLight.SetBounds(6, Math.Max(2, (statusPanel.Height - 14) / 2), 14, 14);
			}
		}

		private void SetControlBounds(Control control, int x, int y, int width, int height)
		{
			if (control == null)
			{
				return;
			}

			control.Visible = true;
			control.Dock = DockStyle.None;
			control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			control.SetBounds(x, y, Math.Max(1, width), Math.Max(1, height));
		}


		private void EnableSmoothUi()
		{
			try
			{
				SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
				UpdateStyles();
			}
			catch
			{
			}

			EnableDoubleBuffer(rootLayout);
			EnableDoubleBuffer(topPanel);
			EnableDoubleBuffer(topLayout);
			EnableDoubleBuffer(mainSplit);
			EnableDoubleBuffer(leftSplit);
			EnableDoubleBuffer(inputPanel);
			EnableDoubleBuffer(outputPanel);
			EnableDoubleBuffer(codePanel);
			EnableDoubleBuffer(codeEditorHost);
			EnableDoubleBuffer(logPanel);
			EnableDoubleBuffer(panelLineNumbers);

			EnableDataGridViewDoubleBuffer(gridInputs);
			EnableDataGridViewDoubleBuffer(gridOutputs);
			EnableDataGridViewDoubleBuffer(gridLogs);

			if (gridInputs != null) gridInputs.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			if (gridOutputs != null) gridOutputs.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			if (gridLogs != null) gridLogs.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
		}

		private void EnableDoubleBuffer(Control control)
		{
			if (control == null)
			{
				return;
			}

			try
			{
				PropertyInfo property = typeof(Control).GetProperty(
					"DoubleBuffered",
					BindingFlags.Instance | BindingFlags.NonPublic);

				if (property != null)
				{
					property.SetValue(control, true, null);
				}
			}
			catch
			{
			}
		}

		private void EnableDataGridViewDoubleBuffer(DataGridView grid)
		{
			if (grid == null)
			{
				return;
			}

			try
			{
				PropertyInfo property = typeof(DataGridView).GetProperty(
					"DoubleBuffered",
					BindingFlags.Instance | BindingFlags.NonPublic);

				if (property != null)
				{
					property.SetValue(grid, true, null);
				}
			}
			catch
			{
			}
		}

		private const int WM_SETREDRAW = 0x000B;

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private void SuspendControlRedraw(Control control)
		{
			if (control == null || !control.IsHandleCreated)
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
			if (control == null || !control.IsHandleCreated)
			{
				return;
			}

			try
			{
				SendMessage(control.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
				control.Invalidate(true);
			}
			catch
			{
			}
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
			// 顶部布局统一由 RebuildTopBarLayout + LayoutTopBarControls 控制。
			// 这里不再操作 topLayout 的行列，避免 Designer 原来的两行布局再次把按钮挤掉。
			if (chkEnable != null)
			{
				chkEnable.Checked = true;
				chkEnable.Visible = false;
			}

			if (lblScriptFile != null) lblScriptFile.Visible = false;
			if (txtScriptPath != null) txtScriptPath.Visible = false;
			if (btnBrowseScript != null) btnBrowseScript.Visible = false;
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
			if (grid == null)
			{
				return;
			}

			StyleGrid(grid);
			grid.Columns.Clear();

			// 当前值/测试值放到前面，避免值显示跑到最右侧。
			// 输出不再显示“目标去向/绑定去向”，只保留只读的当前输出值。
			grid.Columns.Add(CreateTextColumn("Name", isInput ? "输入名" : "输出名", 150));
			grid.Columns.Add(CreateComboColumn("DataType", "类型", 90));

			if (isInput)
			{
				grid.Columns.Add(CreateTextColumn("DefaultValue", "当前值/测试值", 150));
				grid.Columns.Add(CreateBindingSourceColumn("BindingPath", "绑定来源", 230));
			}
			else
			{
				grid.Columns.Add(CreateTextColumn("CurrentValue", "当前输出值", 170));
			}

			grid.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			grid.Columns["DataType"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

			if (isInput)
			{
				grid.Columns["DefaultValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				grid.Columns["DefaultValue"].ReadOnly = false;
				grid.Columns["BindingPath"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
				grid.Columns["BindingPath"].MinimumWidth = 150;
			}
			else
			{
				grid.Columns["CurrentValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
				grid.Columns["CurrentValue"].ReadOnly = true;
				grid.Columns["CurrentValue"].DefaultCellStyle.BackColor = Color.FromArgb(8, 22, 36);
				grid.Columns["CurrentValue"].DefaultCellStyle.ForeColor = Color.FromArgb(180, 220, 190);
			}

			grid.Columns["Name"].ReadOnly = false;
			grid.Columns["DataType"].ReadOnly = false;

			grid.ColumnHeadersVisible = true;
			EnsurePinGridHeaderVisible(grid);

			if (isInput)
			{
				RefreshInputBindingSourceOptions();
			}
		}
		private void EnsurePinGridHeaderVisible(DataGridView grid)
		{
			if (grid == null)
			{
				return;
			}

			grid.ColumnHeadersVisible = true;
			grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			grid.ColumnHeadersHeight = 30;

			grid.EnableHeadersVisualStyles = false;
			grid.ColumnHeadersDefaultCellStyle.BackColor = _panel2;
			grid.ColumnHeadersDefaultCellStyle.ForeColor = _text;
			grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = _panel2;
			grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = _text;
			grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

			grid.RowHeadersVisible = false;
			grid.AllowUserToAddRows = false;
			grid.AllowUserToDeleteRows = false;
			grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			grid.RowTemplate.Height = 30;

			grid.Refresh();
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
			grid.ColumnHeadersVisible = true;
			grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			grid.AllowUserToAddRows = false;
			grid.AllowUserToResizeRows = true;
			grid.AllowUserToResizeColumns = true;
			grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			grid.MultiSelect = false;
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
			grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			grid.ScrollBars = ScrollBars.Both;
			grid.DataError -= PinGrid_DataError;
			grid.DataError += PinGrid_DataError;

			grid.ColumnHeadersDefaultCellStyle.BackColor = _panel2;
			grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
			grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

			grid.DefaultCellStyle.BackColor = _back;
			grid.DefaultCellStyle.ForeColor = _text;
			grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 125, 200);
			grid.DefaultCellStyle.SelectionForeColor = Color.White;
			grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
			grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

			grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(4, 16, 30);
			grid.AlternatingRowsDefaultCellStyle.ForeColor = _text;

			grid.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			grid.ColumnHeadersHeight = 30;
			grid.RowTemplate.Height = 30;
			grid.Refresh();
		}

		private void PinGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			// BindingPath 下拉列在旧配置值不在 Items 中时会触发 DataError。
			// 这里吞掉异常，同时把旧值补进下拉项，保证兼容历史配置。
			e.ThrowException = false;
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

		private DataGridViewComboBoxColumn CreateBindingSourceColumn(string name, string header, int width)
		{
			DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
			col.Name = name;
			col.HeaderText = header;
			col.Width = width;
			col.FlatStyle = FlatStyle.Flat;
			col.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
			col.DisplayStyleForCurrentCellOnly = false;
			col.Items.Add(string.Empty);
			return col;
		}

		private void RefreshInputBindingSourceOptions()
		{
			List<string> options = BuildInputBindingSourceOptions();

			// 兼容当前配置里已有的手动绑定值，但过滤旧模板/默认通用 Comm 项。
			if (_config != null && _config.Inputs != null)
			{
				foreach (ScriptPinConfig pin in _config.Inputs)
				{
					if (pin == null || IsOldDefaultSampleInput(pin))
					{
						continue;
					}

					if (ShouldKeepBindingOptionFromExistingInput(pin.BindingPath))
					{
						AddUniqueText(options, pin.BindingPath);
					}
				}
			}

			if (gridInputs != null && gridInputs.Columns.Contains("BindingPath"))
			{
				foreach (DataGridViewRow row in gridInputs.Rows)
				{
					if (row == null || row.IsNewRow)
					{
						continue;
					}

					string binding = GetPinCellString(row, "BindingPath");
					if (ShouldKeepBindingOptionFromExistingInput(binding))
					{
						AddUniqueText(options, binding);
					}
				}
			}

			// 最终再过滤一次旧模板/默认通用项，避免 Comm.JobID 等旧值继续出现在下拉框。
			for (int i = options.Count - 1; i >= 0; i--)
			{
				if (IsCommonRuntimeBinding(options[i]))
				{
					options.RemoveAt(i);
				}
			}

			if (options.Count <= 0 || !string.IsNullOrEmpty(options[0]))
			{
				options.Insert(0, string.Empty);
			}

			_bindingSourceOptions = options;
			ApplyBindingSourceOptionsToGrid();
		}

		private List<string> BuildInputBindingSourceOptions()
		{
			List<string> result = new List<string>();

			// 只保留空项 + 当前 Script 在流程管理里绑定的前序模块输出。
			// 不再默认加入 Comm.JobID / Comm.Barcode / Comm.ProductCode 等通用项，
			// 避免用户以为这些是当前流程真实可用的前序输出。
			AddUniqueText(result, string.Empty);

			TaskConfig task = GetCurrentTaskConfig();
			if (task == null || task.Steps == null || task.StepFlow == null)
			{
				return result;
			}

			StepFlowItem currentFlow = FindCurrentScriptFlowItem(task);
			List<string> sourceStepNames = new List<string>();

			if (currentFlow != null && currentFlow.ScriptInputStepKeyList != null && currentFlow.ScriptInputStepKeyList.Count > 0)
			{
				sourceStepNames.AddRange(currentFlow.ScriptInputStepKeyList);
			}


			foreach (string sourceStepName in sourceStepNames)
			{
				StepConfig step = task.Steps.FirstOrDefault(s =>
					s != null &&
					string.Equals(s.StepName, sourceStepName, StringComparison.OrdinalIgnoreCase));

				AddStepOutputBindingOptions(result, task, sourceStepName, step);
			}

			return result;
		}

		private void AddCommonRuntimeInputOptions(List<string> result)
		{
			// 这些是通讯/任务运行时常用输入，保留给手动绑定使用。
			AddUniqueText(result, "Comm.JobID");
			AddUniqueText(result, "Comm.Barcode");
			AddUniqueText(result, "Comm.ProductCode");
			AddUniqueText(result, "Comm.TriggerName");
			AddUniqueText(result, "Comm.PositionName");
		}


		private bool IsCommonRuntimeBinding(string bindingPath)
		{
			if (string.IsNullOrWhiteSpace(bindingPath))
			{
				return false;
			}

			string text = bindingPath.Trim();
			return
				string.Equals(text, "Comm.JobID", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "Comm.Barcode", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "Comm.ProductCode", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "Comm.TriggerName", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "Comm.PositionName", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "Vpp_01.Result1", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "Halcon_01.Code", StringComparison.OrdinalIgnoreCase);
		}

		private TaskConfig GetCurrentTaskConfig()
		{
			if (string.IsNullOrWhiteSpace(_jobName) || string.IsNullOrWhiteSpace(_taskName))
			{
				return null;
			}

			try
			{
				ProjectFlowConfig flow = FlowConfigStore.LoadOrCreateDefault();
				if (flow == null || flow.Jobs == null)
				{
					return null;
				}

				JobConfig job = flow.Jobs.FirstOrDefault(j =>
					j != null && string.Equals(j.JobName, _jobName, StringComparison.OrdinalIgnoreCase));

				if (job == null || job.Tasks == null)
				{
					return null;
				}

				return job.Tasks.FirstOrDefault(t =>
					t != null && string.Equals(t.TaskName, _taskName, StringComparison.OrdinalIgnoreCase));
			}
			catch
			{
				return null;
			}
		}

		private StepFlowItem FindCurrentScriptFlowItem(TaskConfig task)
		{
			if (task == null || task.StepFlow == null)
			{
				return null;
			}

			List<string> names = new List<string>();
			AddUniqueText(names, _config == null ? string.Empty : _config.StepName);
			AddUniqueText(names, GetCurrentScriptDisplayName());
			AddUniqueText(names, _scriptPath == null ? string.Empty : Path.GetFileNameWithoutExtension(_scriptPath));

			foreach (StepFlowItem item in task.StepFlow)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.StepName))
				{
					continue;
				}

				foreach (string name in names)
				{
					if (string.Equals(item.StepName, name, StringComparison.OrdinalIgnoreCase))
					{
						return item;
					}
				}
			}

			return null;
		}

		private void AddStepOutputBindingOptions(List<string> result, TaskConfig task, string sourceStepName, StepConfig step)
		{
			if (string.IsNullOrWhiteSpace(sourceStepName))
			{
				return;
			}

			EnsureStepOutputPinsAvailable(task, step);

			if (step != null && step.OutputPins != null)
			{
				foreach (PinConfig pin in step.OutputPins)
				{
					if (pin == null)
					{
						continue;
					}

					AddStepPinOption(result, sourceStepName, pin.PinName);
					AddUniqueText(result, pin.TargetKey);
					AddUniqueText(result, pin.SourceKey);
				}
			}

			if (step != null && step.StepType == StepType.Script)
			{
				CSharpScriptStepConfig scriptConfig = TryLoadScriptConfigForStep(task, step);
				if (scriptConfig != null && scriptConfig.Outputs != null)
				{
					foreach (ScriptPinConfig output in scriptConfig.Outputs)
					{
						if (output != null)
						{
							AddStepPinOption(result, sourceStepName, output.Name);
							AddUniqueText(result, output.BindingPath);
						}
					}
				}
			}
		}

		private void AddStepPinOption(List<string> result, string stepName, string pinName)
		{
			if (string.IsNullOrWhiteSpace(stepName) || string.IsNullOrWhiteSpace(pinName))
			{
				return;
			}

			string text = pinName.Trim();
			if (text.IndexOf('.') >= 0 && text.StartsWith(stepName + ".", StringComparison.OrdinalIgnoreCase))
			{
				AddUniqueText(result, text);
			}
			else
			{
				AddUniqueText(result, stepName.Trim() + "." + text);
			}
		}

		private CSharpScriptStepConfig TryLoadScriptConfigForStep(TaskConfig task, StepConfig step)
		{
			try
			{
				string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(_jobName, _taskName);

				List<string> scriptFiles = new List<string>();
				AddUniqueText(scriptFiles, step.ProjectFilePath);
				AddUniqueText(scriptFiles, step.SourceFilePath);

				if (step.ScriptFiles != null)
				{
					foreach (string file in step.ScriptFiles)
					{
						AddUniqueText(scriptFiles, file);
					}
				}

				foreach (string file in scriptFiles)
				{
					string scriptPath = ResolveStepFilePath(taskFolder, file);
					if (string.IsNullOrWhiteSpace(scriptPath))
					{
						continue;
					}

					string configPath = Path.Combine(
						Path.GetDirectoryName(scriptPath) ?? string.Empty,
						Path.GetFileNameWithoutExtension(scriptPath) + ".script.xml");

					if (File.Exists(configPath))
					{
						return CSharpScriptStepStore.Load(configPath);
					}
				}

				string fallback = CSharpScriptStepStore.GetConfigPath(_jobName, _taskName, step.StepName);
				if (File.Exists(fallback))
				{
					return CSharpScriptStepStore.Load(fallback);
				}
			}
			catch
			{
			}

			return null;
		}

		private void RemoveStalePreviousStepInputs()
		{
			if (_config == null || _config.Inputs == null || _config.Inputs.Count <= 0)
			{
				return;
			}

			TaskConfig task = GetCurrentTaskConfig();
			if (task == null || task.Steps == null || task.StepFlow == null)
			{
				return;
			}

			StepFlowItem currentFlow = FindCurrentScriptFlowItem(task);
			List<string> selectedSourceStepNames = GetSelectedScriptInputSourceStepNames(currentFlow);
			List<string> taskStepNames = GetTaskStepNames(task);
			List<string> allowedBindings = BuildAllowedSelectedPreviousOutputBindings(task, selectedSourceStepNames);

			for (int i = _config.Inputs.Count - 1; i >= 0; i--)
			{
				ScriptPinConfig pin = _config.Inputs[i];
				if (pin == null)
				{
					_config.Inputs.RemoveAt(i);
					continue;
				}

				string bindingPath = pin.BindingPath == null ? string.Empty : pin.BindingPath.Trim();
				string sourceStepName = GetBindingSourceStepName(bindingPath, taskStepNames);

				// 只清理来自 Task Step 的自动/流程绑定；自定义变量、空绑定、后续 Global.* 变量保留。
				if (string.IsNullOrWhiteSpace(sourceStepName))
				{
					string autoSource = GetAutoInputSourceFromDescription(pin.Description);
					if (!string.IsNullOrWhiteSpace(autoSource) && !ContainsText(selectedSourceStepNames, autoSource))
					{
						_config.Inputs.RemoveAt(i);
					}

					continue;
				}

				// 绑定的来源 Step 已经不在“流程管理 -> Script 输入源”中，直接删除。
				if (!ContainsText(selectedSourceStepNames, sourceStepName))
				{
					_config.Inputs.RemoveAt(i);
					continue;
				}

				// 来源 Step 仍然选中，但该输出引脚已经不存在，也删除，避免换 VPP 后残留旧输出。
				if (!ContainsText(allowedBindings, bindingPath))
				{
					_config.Inputs.RemoveAt(i);
					continue;
				}
			}
		}

		private bool ShouldKeepBindingOptionFromExistingInput(string bindingPath)
		{
			if (string.IsNullOrWhiteSpace(bindingPath))
			{
				return false;
			}

			if (IsCommonRuntimeBinding(bindingPath))
			{
				return false;
			}

			TaskConfig task = GetCurrentTaskConfig();
			if (task == null || task.Steps == null || task.StepFlow == null)
			{
				return true;
			}

			List<string> taskStepNames = GetTaskStepNames(task);
			string sourceStepName = GetBindingSourceStepName(bindingPath, taskStepNames);

			// 非 Step 输出绑定，例如手动变量、Global.xxx，允许保留。
			if (string.IsNullOrWhiteSpace(sourceStepName))
			{
				return true;
			}

			StepFlowItem currentFlow = FindCurrentScriptFlowItem(task);
			List<string> selectedSourceStepNames = GetSelectedScriptInputSourceStepNames(currentFlow);
			if (!ContainsText(selectedSourceStepNames, sourceStepName))
			{
				return false;
			}

			List<string> allowedBindings = BuildAllowedSelectedPreviousOutputBindings(task, selectedSourceStepNames);
			return ContainsText(allowedBindings, bindingPath);
		}

		private string GetAutoInputDescriptionByBinding(string bindingPath)
		{
			TaskConfig task = GetCurrentTaskConfig();
			if (task == null || task.Steps == null)
			{
				return string.Empty;
			}

			string sourceStepName = GetBindingSourceStepName(bindingPath, GetTaskStepNames(task));
			if (string.IsNullOrWhiteSpace(sourceStepName))
			{
				return string.Empty;
			}

			return "AUTO_FROM_STEP:" + sourceStepName;
		}

		private List<string> GetSelectedScriptInputSourceStepNames(StepFlowItem currentFlow)
		{
			List<string> result = new List<string>();

			if (currentFlow == null || currentFlow.ScriptInputStepKeyList == null)
			{
				return result;
			}

			foreach (string name in currentFlow.ScriptInputStepKeyList)
			{
				AddUniqueText(result, name);
			}

			return result;
		}

		private List<string> GetTaskStepNames(TaskConfig task)
		{
			List<string> result = new List<string>();

			if (task == null || task.Steps == null)
			{
				return result;
			}

			foreach (StepConfig step in task.Steps)
			{
				if (step != null)
				{
					AddUniqueText(result, step.StepName);
				}
			}

			return result;
		}

		private List<string> BuildAllowedSelectedPreviousOutputBindings(TaskConfig task, List<string> selectedSourceStepNames)
		{
			List<string> result = new List<string>();

			if (task == null || task.Steps == null || selectedSourceStepNames == null)
			{
				return result;
			}

			foreach (string sourceStepName in selectedSourceStepNames)
			{
				if (string.IsNullOrWhiteSpace(sourceStepName))
				{
					continue;
				}

				StepConfig step = task.Steps.FirstOrDefault(s =>
					s != null && string.Equals(s.StepName, sourceStepName, StringComparison.OrdinalIgnoreCase));

				if (step == null)
				{
					continue;
				}

				EnsureStepOutputPinsAvailable(task, step);

				if (step.OutputPins != null)
				{
					foreach (PinConfig pin in step.OutputPins)
					{
						if (pin == null)
						{
							continue;
						}

						if (!string.IsNullOrWhiteSpace(pin.PinName))
						{
							AddUniqueText(result, step.StepName + "." + pin.PinName.Trim());
						}

						AddUniqueText(result, pin.TargetKey);
						AddUniqueText(result, pin.SourceKey);
					}
				}

				if (step.StepType == StepType.Script)
				{
					CSharpScriptStepConfig scriptConfig = TryLoadScriptConfigForStep(task, step);
					if (scriptConfig != null && scriptConfig.Outputs != null)
					{
						foreach (ScriptPinConfig output in scriptConfig.Outputs)
						{
							if (output == null || string.IsNullOrWhiteSpace(output.Name))
							{
								continue;
							}

							AddUniqueText(result, step.StepName + "." + output.Name.Trim());
							AddUniqueText(result, output.BindingPath);
						}
					}
				}
			}

			return result;
		}

		private string GetBindingSourceStepName(string bindingPath, List<string> taskStepNames)
		{
			if (string.IsNullOrWhiteSpace(bindingPath) || taskStepNames == null)
			{
				return string.Empty;
			}

			string binding = bindingPath.Trim();
			foreach (string stepName in taskStepNames)
			{
				if (string.IsNullOrWhiteSpace(stepName))
				{
					continue;
				}

				string prefix = stepName.Trim() + ".";
				if (binding.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				{
					return stepName.Trim();
				}
			}

			return string.Empty;
		}

		private string GetAutoInputSourceFromDescription(string description)
		{
			if (string.IsNullOrWhiteSpace(description))
			{
				return string.Empty;
			}

			string text = description.Trim();
			const string newPrefix = "AUTO_FROM_STEP:";
			if (text.StartsWith(newPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return text.Substring(newPrefix.Length).Trim();
			}

			const string oldPrefix = "From ";
			if (text.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return text.Substring(oldPrefix.Length).Trim();
			}

			return string.Empty;
		}

		private bool ContainsText(List<string> list, string value)
		{
			if (list == null || string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			foreach (string item in list)
			{
				if (string.Equals(item, value.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private void RemoveDefaultSampleInputs()
		{
			if (_config == null || _config.Inputs == null)
			{
				return;
			}

			for (int i = _config.Inputs.Count - 1; i >= 0; i--)
			{
				ScriptPinConfig pin = _config.Inputs[i];
				if (IsOldDefaultSampleInput(pin))
				{
					_config.Inputs.RemoveAt(i);
				}
			}
		}

		private bool IsOldDefaultSampleInput(ScriptPinConfig pin)
		{
			if (pin == null)
			{
				return false;
			}

			string name = pin.Name == null ? string.Empty : pin.Name.Trim();
			string binding = pin.BindingPath == null ? string.Empty : pin.BindingPath.Trim();

			// 旧模板的三个示例输入统一清理。
			// 这里按名称或典型绑定路径都清理，避免旧 .script.xml 里被用户改过默认值后残留。
			return
				string.Equals(name, "JobID", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(name, "Measure1", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(name, "Barcode", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(binding, "Comm.JobID", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(binding, "Vpp_01.Result1", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(binding, "Halcon_01.Code", StringComparison.OrdinalIgnoreCase);
		}

		private void AutoMergeOutputsFromScriptCode(string code)
		{
			if (_config == null)
			{
				return;
			}

			if (_config.Outputs == null)
			{
				_config.Outputs = new List<ScriptPinConfig>();
			}

			foreach (string outputName in ExtractSetOutputNames(code))
			{
				if (ScriptOutputNameExists(outputName))
				{
					continue;
				}

				ScriptPinConfig output = new ScriptPinConfig();
				output.Name = outputName;
				output.DataType = GuessScriptOutputDataType(outputName);
				output.BindingPath = outputName;
				output.DefaultValue = string.Empty;
				output.Description = "Auto detected from context.SetOutput";
				_config.Outputs.Add(output);
			}
		}

		private List<string> ExtractSetOutputNames(string code)
		{
			List<string> result = new List<string>();

			if (string.IsNullOrWhiteSpace(code))
			{
				return result;
			}

			try
			{
				MatchCollection matches = Regex.Matches(
					code,
					@"\.SetOutput\s*\(\s*""(?<name>[^""\\]+)""",
					RegexOptions.IgnoreCase);

				foreach (Match match in matches)
				{
					if (match == null || !match.Success)
					{
						continue;
					}

					string name = match.Groups["name"].Value;
					if (!string.IsNullOrWhiteSpace(name))
					{
						AddUniqueText(result, name.Trim());
					}
				}
			}
			catch
			{
			}

			return result;
		}

		private bool ScriptOutputNameExists(string outputName)
		{
			if (_config == null || _config.Outputs == null || string.IsNullOrWhiteSpace(outputName))
			{
				return false;
			}

			foreach (ScriptPinConfig output in _config.Outputs)
			{
				if (output != null && string.Equals(output.Name, outputName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private ScriptPinDataType GuessScriptOutputDataType(string outputName)
		{
			if (string.IsNullOrWhiteSpace(outputName))
			{
				return ScriptPinDataType.String;
			}

			string name = outputName.Trim();
			if (name.IndexOf("OK", StringComparison.OrdinalIgnoreCase) >= 0 ||
				name.IndexOf("Pass", StringComparison.OrdinalIgnoreCase) >= 0 ||
				name.IndexOf("NG", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return ScriptPinDataType.Bool;
			}

			if (name.IndexOf("X", StringComparison.OrdinalIgnoreCase) >= 0 ||
				name.IndexOf("Y", StringComparison.OrdinalIgnoreCase) >= 0 ||
				name.IndexOf("A", StringComparison.OrdinalIgnoreCase) >= 0 ||
				name.IndexOf("Sum", StringComparison.OrdinalIgnoreCase) >= 0 ||
				name.IndexOf("Score", StringComparison.OrdinalIgnoreCase) >= 0 ||
				name.IndexOf("Distance", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return ScriptPinDataType.Double;
			}

			return ScriptPinDataType.String;
		}

		private void EnsureStepOutputPinsAvailable(TaskConfig task, StepConfig sourceStep)
		{
			if (task == null || sourceStep == null)
			{
				return;
			}

			if (sourceStep.OutputPins != null && sourceStep.OutputPins.Count > 0)
			{
				return;
			}

			List<PinConfig> pins = null;

			if (sourceStep.StepType == StepType.Vpp)
			{
				pins = TryReadVppOutputPins(sourceStep);
			}
			else if (sourceStep.StepType == StepType.Script)
			{
				CSharpScriptStepConfig scriptConfig = TryLoadScriptConfigForStep(task, sourceStep);
				if (scriptConfig != null && scriptConfig.Outputs != null && scriptConfig.Outputs.Count > 0)
				{
					pins = new List<PinConfig>();
					foreach (ScriptPinConfig output in scriptConfig.Outputs)
					{
						if (output == null || string.IsNullOrWhiteSpace(output.Name))
						{
							continue;
						}

						PinConfig pin = new PinConfig();
						pin.PinName = output.Name;
						pin.SourceKey = sourceStep.StepName + "." + output.Name;
						pin.TargetKey = pin.SourceKey;
						pin.DataType = ConvertScriptPinDataTypeToPinDataType(output.DataType);
						pin.Description = "Script output";
						pins.Add(pin);
					}
				}
			}

			if (pins == null || pins.Count <= 0)
			{
				return;
			}

			sourceStep.OutputPins = pins;
			TrySaveOutputPinsToFlowConfig(sourceStep.StepName, pins);
		}

		private List<PinConfig> TryReadVppOutputPins(StepConfig step)
		{
			string vppPath = ResolveStepVppFilePath(step);
			if (string.IsNullOrWhiteSpace(vppPath) || !File.Exists(vppPath))
			{
				return new List<PinConfig>();
			}

			object toolBlock = TryLoadVisionProObject(vppPath);
			if (toolBlock == null)
			{
				return new List<PinConfig>();
			}

			return ReadToolBlockPinsByReflection(toolBlock, true, step.StepName);
		}

		private string ResolveStepVppFilePath(StepConfig step)
		{
			if (step == null)
			{
				return string.Empty;
			}

			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(_jobName, _taskName);
			List<string> candidates = new List<string>();
			AddUniqueText(candidates, step.ProjectFilePath);
			AddUniqueText(candidates, step.SourceFilePath);

			if (step.VppFiles != null)
			{
				foreach (string file in step.VppFiles)
				{
					AddUniqueText(candidates, file);
				}
			}

			// 常见情况：StepName=Inspection，而文件名为 Inspection.vpp。
			AddUniqueText(candidates, step.StepName + ".vpp");
			AddUniqueText(candidates, Path.Combine("VPP", step.StepName + ".vpp"));

			foreach (string candidate in candidates)
			{
				string path = ResolveStepFilePath(taskFolder, candidate);
				if (IsVppFile(path) && File.Exists(path))
				{
					return path;
				}
			}

			try
			{
				if (!string.IsNullOrWhiteSpace(taskFolder) && Directory.Exists(taskFolder))
				{
					string[] files = Directory.GetFiles(taskFolder, "*.vpp", SearchOption.AllDirectories);
					foreach (string file in files)
					{
						if (string.Equals(Path.GetFileNameWithoutExtension(file), step.StepName, StringComparison.OrdinalIgnoreCase))
						{
							return file;
						}
					}

					if (files.Length == 1)
					{
						return files[0];
					}
				}
			}
			catch
			{
			}

			return string.Empty;
		}

		private bool IsVppFile(string path)
		{
			return !string.IsNullOrWhiteSpace(path) &&
				Path.GetExtension(path).Equals(".vpp", StringComparison.OrdinalIgnoreCase);
		}

		private object TryLoadVisionProObject(string filePath)
		{
			try
			{
				Type serializerType = Type.GetType("Cognex.VisionPro.CogSerializer, Cognex.VisionPro");
				if (serializerType == null)
				{
					foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
					{
						try
						{
							serializerType = asm.GetType("Cognex.VisionPro.CogSerializer", false, true);
							if (serializerType != null)
							{
								break;
							}
						}
						catch
						{
						}
					}
				}

				if (serializerType == null)
				{
					return null;
				}

				MethodInfo method = serializerType.GetMethod("LoadObjectFromFile", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
				if (method == null)
				{
					return null;
				}

				return method.Invoke(null, new object[] { filePath });
			}
			catch
			{
				return null;
			}
		}

		private List<PinConfig> ReadToolBlockPinsByReflection(object toolBlock, bool outputOnly, string stepName)
		{
			List<PinConfig> result = new List<PinConfig>();
			if (toolBlock == null)
			{
				return result;
			}

			object outputs = GetObjectProperty(toolBlock, "Outputs");
			foreach (object terminal in EnumerateCollectionItems(outputs))
			{
				string name = Convert.ToString(GetObjectProperty(terminal, "Name"));
				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				object value = GetObjectProperty(terminal, "Value");
				object valueType = GetObjectProperty(terminal, "ValueType");

				PinConfig pin = new PinConfig();
				pin.PinName = name.Trim();
				pin.SourceKey = stepName + "." + pin.PinName;
				pin.TargetKey = pin.SourceKey;
				pin.DataType = GuessPinDataType(value, valueType);
				pin.Description = "VPP output";
				result.Add(pin);
			}

			return result;
		}

		private object GetObjectProperty(object obj, string propertyName)
		{
			if (obj == null || string.IsNullOrWhiteSpace(propertyName))
			{
				return null;
			}

			try
			{
				PropertyInfo prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
				if (prop != null)
				{
					return prop.GetValue(obj, null);
				}
			}
			catch
			{
			}

			return null;
		}

		private List<object> EnumerateCollectionItems(object collection)
		{
			List<object> result = new List<object>();
			if (collection == null)
			{
				return result;
			}

			System.Collections.IEnumerable enumerable = collection as System.Collections.IEnumerable;
			if (enumerable != null)
			{
				try
				{
					foreach (object item in enumerable)
					{
						if (item != null)
						{
							result.Add(item);
						}
					}
					if (result.Count > 0)
					{
						return result;
					}
				}
				catch
				{
				}
			}

			try
			{
				PropertyInfo countProp = collection.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
				int count = countProp == null ? 0 : Convert.ToInt32(countProp.GetValue(collection, null));
				PropertyInfo itemProp = collection.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
				for (int i = 0; i < count; i++)
				{
					object item = null;
					if (itemProp != null)
					{
						try { item = itemProp.GetValue(collection, new object[] { i }); } catch { }
					}
					if (item == null)
					{
						MethodInfo method = collection.GetType().GetMethod("get_Item", new Type[] { typeof(int) });
						if (method != null)
						{
							try { item = method.Invoke(collection, new object[] { i }); } catch { }
						}
					}

					if (item != null)
					{
						result.Add(item);
					}
				}
			}
			catch
			{
			}

			return result;
		}

		private PinDataType GuessPinDataType(object value, object valueType)
		{
			Type type = null;
			if (value != null)
			{
				type = value.GetType();
			}
			else if (valueType is Type)
			{
				type = (Type)valueType;
			}

			string name = type == null ? Convert.ToString(valueType) : type.FullName;
			if (string.IsNullOrWhiteSpace(name))
			{
				return PinDataType.String;
			}

			if (name.IndexOf("Boolean", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Bool", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return PinDataType.Bool;
			}
			if (name.IndexOf("Int", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Short", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Long", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return PinDataType.Int;
			}
			if (name.IndexOf("Double", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Single", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Float", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Decimal", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return PinDataType.Double;
			}

			return PinDataType.String;
		}

		private PinDataType ConvertScriptPinDataTypeToPinDataType(ScriptPinDataType dataType)
		{
			if (dataType == ScriptPinDataType.Bool) return PinDataType.Bool;
			if (dataType == ScriptPinDataType.Int) return PinDataType.Int;
			if (dataType == ScriptPinDataType.Double) return PinDataType.Double;
			if (dataType == ScriptPinDataType.Decimal) return PinDataType.Double;
			return PinDataType.String;
		}

		private void TrySaveOutputPinsToFlowConfig(string stepName, List<PinConfig> pins)
		{
			if (string.IsNullOrWhiteSpace(stepName) || pins == null || pins.Count <= 0)
			{
				return;
			}

			try
			{
				ProjectFlowConfig flow = FlowConfigStore.LoadOrCreateDefault();
				if (flow == null || flow.Jobs == null)
				{
					return;
				}

				JobConfig job = flow.Jobs.FirstOrDefault(j => j != null && string.Equals(j.JobName, _jobName, StringComparison.OrdinalIgnoreCase));
				if (job == null || job.Tasks == null)
				{
					return;
				}

				TaskConfig task = job.Tasks.FirstOrDefault(t => t != null && string.Equals(t.TaskName, _taskName, StringComparison.OrdinalIgnoreCase));
				if (task == null || task.Steps == null)
				{
					return;
				}

				StepConfig step = task.Steps.FirstOrDefault(s => s != null && string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase));
				if (step == null)
				{
					return;
				}

				step.OutputPins = pins;
				FlowConfigStore.Save(flow);
			}
			catch
			{
			}
		}

		private void AutoMergeSelectedPreviousOutputPinsToInputs()
		{
			if (_config == null)
			{
				return;
			}

			if (_config.Inputs == null)
			{
				_config.Inputs = new List<ScriptPinConfig>();
			}

			TaskConfig task = GetCurrentTaskConfig();
			if (task == null || task.Steps == null || task.StepFlow == null)
			{
				return;
			}

			StepFlowItem currentFlow = FindCurrentScriptFlowItem(task);
			if (currentFlow == null || currentFlow.ScriptInputStepKeyList == null || currentFlow.ScriptInputStepKeyList.Count <= 0)
			{
				return;
			}

			foreach (string sourceStepName in currentFlow.ScriptInputStepKeyList)
			{
				if (string.IsNullOrWhiteSpace(sourceStepName))
				{
					continue;
				}

				StepConfig sourceStep = task.Steps.FirstOrDefault(s =>
					s != null && string.Equals(s.StepName, sourceStepName, StringComparison.OrdinalIgnoreCase));

				if (sourceStep == null)
				{
					continue;
				}

				// VPP/Hdev/VM 统一走 StepConfig.OutputPins。
				// 如果 VPP 还没有在算法模块页面打开过，这里会直接从 .vpp 文件读取输出引脚。
				EnsureStepOutputPinsAvailable(task, sourceStep);

				if (sourceStep.OutputPins != null && sourceStep.OutputPins.Count > 0)
				{
					foreach (PinConfig outputPin in sourceStep.OutputPins)
					{
						if (outputPin == null || string.IsNullOrWhiteSpace(outputPin.PinName))
						{
							continue;
						}

						string bindingPath = sourceStep.StepName + "." + outputPin.PinName;

						if (ScriptInputBindingExists(bindingPath))
						{
							continue;
						}

						ScriptPinConfig input = new ScriptPinConfig();
						input.Name = MakeUniqueScriptInputName(outputPin.PinName, sourceStep.StepName);
						input.DataType = ConvertPinDataTypeToScriptPinDataType(outputPin.DataType);
						input.BindingPath = bindingPath;
						input.DefaultValue = string.Empty;
						input.Description = "AUTO_FROM_STEP:" + sourceStep.StepName;

						_config.Inputs.Add(input);
					}
					continue;
				}

				// 如果前序模块本身也是 Script，则读取它的 .script.xml 输出定义。
				if (sourceStep.StepType == StepType.Script)
				{
					CSharpScriptStepConfig scriptConfig = TryLoadScriptConfigForStep(task, sourceStep);
					if (scriptConfig == null || scriptConfig.Outputs == null)
					{
						continue;
					}

					foreach (ScriptPinConfig output in scriptConfig.Outputs)
					{
						if (output == null || string.IsNullOrWhiteSpace(output.Name))
						{
							continue;
						}

						string bindingPath = sourceStep.StepName + "." + output.Name;
						if (ScriptInputBindingExists(bindingPath))
						{
							continue;
						}

						ScriptPinConfig input = new ScriptPinConfig();
						input.Name = MakeUniqueScriptInputName(output.Name, sourceStep.StepName);
						input.DataType = output.DataType;
						input.BindingPath = bindingPath;
						input.DefaultValue = string.Empty;
						input.Description = "AUTO_FROM_STEP:" + sourceStep.StepName;

						_config.Inputs.Add(input);
					}
				}
			}
		}

		private bool ScriptInputBindingExists(string bindingPath)
		{
			if (_config == null || _config.Inputs == null || string.IsNullOrWhiteSpace(bindingPath))
			{
				return false;
			}

			foreach (ScriptPinConfig input in _config.Inputs)
			{
				if (input == null)
				{
					continue;
				}

				if (string.Equals(input.BindingPath, bindingPath, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private string MakeUniqueScriptInputName(string outputPinName, string sourceStepName)
		{
			string baseName = MakeSafeIdentifier(outputPinName);
			if (string.IsNullOrWhiteSpace(baseName))
			{
				baseName = MakeSafeIdentifier(sourceStepName + "_Output");
			}

			if (!ScriptInputNameExists(baseName))
			{
				return baseName;
			}

			string prefixName = MakeSafeIdentifier(sourceStepName + "_" + outputPinName);
			if (!ScriptInputNameExists(prefixName))
			{
				return prefixName;
			}

			int index = 2;
			while (ScriptInputNameExists(prefixName + "_" + index.ToString()))
			{
				index++;
			}

			return prefixName + "_" + index.ToString();
		}

		private bool ScriptInputNameExists(string name)
		{
			if (_config == null || _config.Inputs == null || string.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			foreach (ScriptPinConfig input in _config.Inputs)
			{
				if (input != null && string.Equals(input.Name, name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private string MakeSafeIdentifier(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder();
			foreach (char c in text.Trim())
			{
				if (char.IsLetterOrDigit(c) || c == '_')
				{
					sb.Append(c);
				}
				else
				{
					sb.Append('_');
				}
			}

			string result = sb.ToString().Trim('_');
			if (string.IsNullOrWhiteSpace(result))
			{
				return string.Empty;
			}

			if (!(char.IsLetter(result[0]) || result[0] == '_'))
			{
				result = "P_" + result;
			}

			return result;
		}

		private ScriptPinDataType ConvertPinDataTypeToScriptPinDataType(PinDataType type)
		{
			switch (type)
			{
				case PinDataType.Bool:
					return ScriptPinDataType.Bool;
				case PinDataType.Int:
					return ScriptPinDataType.Int;
				case PinDataType.Double:
				case PinDataType.Float:
					return ScriptPinDataType.Double;
				case PinDataType.Image:
				case PinDataType.ByteArray:
					return ScriptPinDataType.Object;
				case PinDataType.String:
				default:
					return ScriptPinDataType.String;
			}
		}

		private void AddUniqueText(List<string> list, string value)
		{
			if (list == null || string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			string text = value.Trim();
			if (text.Length <= 0)
			{
				return;
			}

			foreach (string item in list)
			{
				if (string.Equals(item, text, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
			}

			list.Add(text);
		}

		private void EnsureBindingPathOption(string bindingPath)
		{
			if (string.IsNullOrWhiteSpace(bindingPath) || IsCommonRuntimeBinding(bindingPath))
			{
				return;
			}

			if (_bindingSourceOptions == null)
			{
				_bindingSourceOptions = new List<string>();
			}

			AddUniqueText(_bindingSourceOptions, bindingPath);
			ApplyBindingSourceOptionsToGrid();
		}

		private void ApplyBindingSourceOptionsToGrid()
		{
			if (gridInputs == null || !gridInputs.Columns.Contains("BindingPath"))
			{
				return;
			}

			DataGridViewComboBoxColumn combo = gridInputs.Columns["BindingPath"] as DataGridViewComboBoxColumn;
			if (combo == null)
			{
				return;
			}

			combo.Items.Clear();
			combo.Items.Add(string.Empty);

			if (_bindingSourceOptions != null)
			{
				foreach (string item in _bindingSourceOptions)
				{
					if (!string.IsNullOrWhiteSpace(item) && !combo.Items.Contains(item))
					{
						combo.Items.Add(item);
					}
				}
			}
		}


		private void RebuildPinSectionLayouts()
		{
			RebuildSinglePinSection(
				inputPanel,
				lblInputTitle,
				btnInputAdd,
				btnInputDelete,
				gridInputs,
				true);

			RebuildSinglePinSection(
				outputPanel,
				lblOutputTitle,
				btnOutputAdd,
				btnOutputDelete,
				gridOutputs,
				false);
		}

		private void RebuildSinglePinSection(
			Panel panel,
			Label title,
			Button addButton,
			Button deleteButton,
			DataGridView grid,
			bool isInput)
		{
			if (panel == null || title == null || addButton == null || deleteButton == null || grid == null)
			{
				return;
			}

			panel.SuspendLayout();

			try
			{
				panel.Controls.Clear();
				panel.Padding = new Padding(8);
				panel.BackColor = _back;

				Panel header = new Panel();
				header.Name = isInput ? "inputHeaderPanel" : "outputHeaderPanel";
				header.Dock = DockStyle.Top;
				header.Height = 34;
				header.Padding = new Padding(0, 0, 0, 4);
				header.BackColor = _back;

				Panel buttonHost = new Panel();
				buttonHost.Dock = DockStyle.Right;
				buttonHost.Width = 72;
				buttonHost.Padding = new Padding(0, 2, 0, 2);
				buttonHost.Margin = new Padding(0);
				buttonHost.BackColor = _back;

				PreparePinSmallButton(deleteButton, "-", 0);
				PreparePinSmallButton(addButton, "+", 36);

				buttonHost.Controls.Add(deleteButton);
				buttonHost.Controls.Add(addButton);

				title.Dock = DockStyle.Fill;
				title.Margin = new Padding(0);
				title.Padding = new Padding(0, 0, 8, 0);
				title.TextAlign = ContentAlignment.MiddleLeft;
				title.BackColor = Color.Transparent;
				title.ForeColor = _text;

				header.Controls.Add(title);
				header.Controls.Add(buttonHost);

				grid.Dock = DockStyle.Fill;
				grid.Margin = new Padding(0);
				grid.ColumnHeadersVisible = true;

				panel.Controls.Add(grid);
				panel.Controls.Add(header);
				header.BringToFront();
				addButton.BringToFront();
				deleteButton.BringToFront();
			}
			finally
			{
				panel.ResumeLayout(true);
			}
		}

		private void PreparePinSmallButton(Button button, string text, int x)
		{
			if (button == null)
			{
				return;
			}

			button.Visible = true;
			button.Enabled = true;
			button.Text = text;
			button.Dock = DockStyle.None;
			button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			button.Location = new Point(x, 2);
			button.Size = new Size(30, 26);
			button.Margin = new Padding(0);
			button.Padding = new Padding(0);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderSize = 1;
			button.FlatAppearance.BorderColor = _accent;
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
			button.BackColor = _panel;
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold);
			button.TextAlign = ContentAlignment.MiddleCenter;
			button.UseVisualStyleBackColor = false;
		}

		private void UpdatePinToolbarText()
		{
			if (btnShowInputs != null) btnShowInputs.Text = _isEnglish ? "Inputs" : "输入";
			if (btnShowOutputs != null) btnShowOutputs.Text = _isEnglish ? "Outputs" : "输出";
			if (btnInputAdd != null) btnInputAdd.Text = "+";
			if (btnInputDelete != null) btnInputDelete.Text = "-";
			if (btnOutputAdd != null) btnOutputAdd.Text = "+";
			if (btnOutputDelete != null) btnOutputDelete.Text = "-";

			LayoutPinToolbarButtons();
			ApplyPinTabButtonStyle();
		}

		private void InitializePinToggleLayout()
		{
			_showInputPins = true;

			if (pinToolPanel != null)
			{
				pinToolPanel.Resize -= pinToolPanel_Resize;
				pinToolPanel.Resize += pinToolPanel_Resize;
			}

			ShowPinPage(true);
		}

		private void pinToolPanel_Resize(object sender, EventArgs e)
		{
			LayoutPinToolbarButtons();
		}

		private void LayoutPinToolbarButtons()
		{
			if (pinToolPanel == null)
			{
				return;
			}

			int width = pinToolPanel.ClientSize.Width;
			if (width <= 0)
			{
				return;
			}

			int top = 6;
			int buttonH = 32;
			int tabW = 86;
			int smallW = 34;
			int gap = 6;
			int rightPadding = 12;

			SetPinButtonBounds(btnShowInputs, 0, top, tabW, buttonH);
			SetPinButtonBounds(btnShowOutputs, tabW + 4, top, tabW, buttonH);

			int deleteX = width - rightPadding - smallW;
			int addX = deleteX - gap - smallW;

			if (addX < tabW * 2 + 20)
			{
				addX = tabW * 2 + 20;
				deleteX = addX + gap + smallW;
			}

			SetPinButtonBounds(btnInputAdd, addX, top, smallW, buttonH);
			SetPinButtonBounds(btnInputDelete, deleteX, top, smallW, buttonH);
			SetPinButtonBounds(btnOutputAdd, addX, top, smallW, buttonH);
			SetPinButtonBounds(btnOutputDelete, deleteX, top, smallW, buttonH);

			int titleX = tabW * 2 + 18;
			int titleW = Math.Max(40, addX - titleX - 8);

			if (lblInputTitle != null)
			{
				lblInputTitle.SetBounds(titleX, 8, titleW, 28);
				lblInputTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			}

			if (lblOutputTitle != null)
			{
				lblOutputTitle.SetBounds(titleX, 8, titleW, 28);
				lblOutputTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			}
		}

		private void SetPinButtonBounds(Button button, int x, int y, int width, int height)
		{
			if (button == null)
			{
				return;
			}

			button.Dock = DockStyle.None;
			button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			button.Margin = new Padding(0);
			button.Padding = new Padding(0);
			button.SetBounds(x, y, width, height);
		}

		private void ApplyPinTabButtonStyle()
		{
			ApplyPinTabSingleButton(btnShowInputs, _showInputPins);
			ApplyPinTabSingleButton(btnShowOutputs, !_showInputPins);
		}

		private void ApplyPinTabSingleButton(Button button, bool selected)
		{
			if (button == null)
			{
				return;
			}

			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderSize = 1;
			button.FlatAppearance.BorderColor = selected ? _accent : _border;
			button.FlatAppearance.MouseDownBackColor = selected ? Color.FromArgb(0, 85, 170) : Color.FromArgb(15, 45, 78);
			button.FlatAppearance.MouseOverBackColor = selected ? Color.FromArgb(0, 110, 210) : Color.FromArgb(15, 45, 78);
			button.BackColor = selected ? Color.FromArgb(0, 95, 190) : _panel;
			button.ForeColor = selected ? Color.White : _muted;
			button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			button.UseVisualStyleBackColor = false;
		}

		private void BindEvents()
		{
			if (btnShowInputs != null)
			{
				btnShowInputs.Click -= btnShowInputs_Click;
				btnShowInputs.Click += btnShowInputs_Click;
			}

			if (btnShowOutputs != null)
			{
				btnShowOutputs.Click -= btnShowOutputs_Click;
				btnShowOutputs.Click += btnShowOutputs_Click;
			}

			btnReferenceDll.Click -= btnReferenceDll_Click;
			btnReferenceDll.Click += btnReferenceDll_Click;

			btnSave.Click -= btnSave_Click;
			btnSave.Click += btnSave_Click;

			btnCompile.Click -= btnCompile_Click;
			btnCompile.Click += btnCompile_Click;

			btnRun.Click -= btnRun_Click;
			btnRun.Click += btnRun_Click;

			if (btnInputAdd != null)
			{
				btnInputAdd.Click -= btnPinAdd_Click;
				btnInputAdd.Click += btnPinAdd_Click;
			}

			if (btnInputDelete != null)
			{
				btnInputDelete.Click -= btnPinDelete_Click;
				btnInputDelete.Click += btnPinDelete_Click;
			}

			if (btnOutputAdd != null)
			{
				btnOutputAdd.Click -= btnPinAdd_Click;
				btnOutputAdd.Click += btnPinAdd_Click;
			}

			if (btnOutputDelete != null)
			{
				btnOutputDelete.Click -= btnPinDelete_Click;
				btnOutputDelete.Click += btnPinDelete_Click;
			}

			gridInputs.CellBeginEdit -= gridInputs_CellBeginEdit;
			gridInputs.CellBeginEdit += gridInputs_CellBeginEdit;

			txtCode.TextChanged -= txtCode_TextChanged;
			txtCode.TextChanged += txtCode_TextChanged;

			txtCode.VScroll -= txtCode_VScroll;
			txtCode.VScroll += txtCode_VScroll;

			txtCode.Resize -= txtCode_Resize;
			txtCode.Resize += txtCode_Resize;

			txtCode.FontChanged -= txtCode_FontChanged;
			txtCode.FontChanged += txtCode_FontChanged;

			txtCode.KeyDown -= txtCode_KeyDown;
			txtCode.KeyDown += txtCode_KeyDown;

			txtCode.KeyUp -= txtCode_KeyUp;
			txtCode.KeyUp += txtCode_KeyUp;

			txtCode.LostFocus -= txtCode_LostFocus;
			txtCode.LostFocus += txtCode_LostFocus;

			panelLineNumbers.Paint -= panelLineNumbers_Paint;
			panelLineNumbers.Paint += panelLineNumbers_Paint;
		}

		private void btnPinAdd_Click(object sender, EventArgs e)
		{
			DataGridView targetGrid = _showInputPins ? gridInputs : gridOutputs;
			AddPinRow(targetGrid);
			ShowPinPage(_showInputPins);
		}

		private void btnPinDelete_Click(object sender, EventArgs e)
		{
			DataGridView targetGrid = _showInputPins ? gridInputs : gridOutputs;
			DeleteSelectedPinRows(targetGrid);
			ShowPinPage(_showInputPins);
		}

		private void btnShowInputs_Click(object sender, EventArgs e)
		{
			ShowPinPage(true);
		}

		private void btnShowOutputs_Click(object sender, EventArgs e)
		{
			ShowPinPage(false);
		}

		private void ShowPinPage(bool showInputs)
		{
			_showInputPins = showInputs;

			if (inputPanel != null)
			{
				inputPanel.Visible = showInputs;
				inputPanel.Dock = DockStyle.Fill;
			}

			if (outputPanel != null)
			{
				outputPanel.Visible = !showInputs;
				outputPanel.Dock = DockStyle.Fill;
			}

			if (lblInputTitle != null)
			{
				lblInputTitle.Visible = showInputs;
			}

			if (lblOutputTitle != null)
			{
				lblOutputTitle.Visible = !showInputs;
			}

			if (btnInputAdd != null)
			{
				btnInputAdd.Visible = showInputs;
				btnInputAdd.Enabled = true;
			}

			if (btnInputDelete != null)
			{
				btnInputDelete.Visible = showInputs;
				btnInputDelete.Enabled = true;
			}

			if (btnOutputAdd != null)
			{
				btnOutputAdd.Visible = !showInputs;
				btnOutputAdd.Enabled = true;
			}

			if (btnOutputDelete != null)
			{
				btnOutputDelete.Visible = !showInputs;
				btnOutputDelete.Enabled = true;
			}

			if (showInputs)
			{
				if (inputPanel != null) inputPanel.BringToFront();
				if (gridInputs != null) gridInputs.BringToFront();
				if (btnInputAdd != null) btnInputAdd.BringToFront();
				if (btnInputDelete != null) btnInputDelete.BringToFront();
			}
			else
			{
				if (outputPanel != null) outputPanel.BringToFront();
				if (gridOutputs != null) gridOutputs.BringToFront();
				if (btnOutputAdd != null) btnOutputAdd.BringToFront();
				if (btnOutputDelete != null) btnOutputDelete.BringToFront();
			}

			LayoutPinToolbarButtons();
			ApplyPinTabButtonStyle();
		}

		private void gridInputs_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0 || gridInputs == null)
			{
				return;
			}

			if (gridInputs.Columns[e.ColumnIndex].Name == "BindingPath")
			{
				RefreshInputBindingSourceOptions();
			}
		}

		private void AddPinRow(DataGridView grid)
		{
			if (grid == null)
			{
				return;
			}

			int rowIndex = grid.Rows.Add();
			DataGridViewRow row = grid.Rows[rowIndex];

			if (grid.Columns.Contains("Name"))
			{
				row.Cells["Name"].Value = string.Empty;
			}

			if (grid.Columns.Contains("DataType"))
			{
				row.Cells["DataType"].Value = ScriptPinDataType.String;
			}

			if (grid.Columns.Contains("DefaultValue"))
			{
				row.Cells["DefaultValue"].Value = string.Empty;
			}

			if (grid.Columns.Contains("BindingPath"))
			{
				row.Cells["BindingPath"].Value = string.Empty;
			}

			if (grid.Columns.Contains("CurrentValue"))
			{
				row.Cells["CurrentValue"].Value = string.Empty;
				row.Cells["CurrentValue"].ReadOnly = true;
			}

			grid.ClearSelection();
			row.Selected = true;
			if (grid.Columns.Contains("Name"))
			{
				grid.CurrentCell = row.Cells["Name"];
				grid.BeginEdit(true);
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
			// 左侧输入/输出配置区收窄，给右侧 C# 代码编辑区更多宽度。
			if (mainSplit != null && !mainSplit.IsDisposed && mainSplit.Width > 500)
			{
				int leftWidth = (int)(mainSplit.Width * 0.38);
				int minLeft = 430;
				int maxLeft = Math.Max(minLeft, mainSplit.Width - 760);
				leftWidth = Clamp(leftWidth, minLeft, maxLeft);

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

			if (grid.Columns.Contains("Name"))
			{
				grid.Columns["Name"].HeaderText = isInput
					? (_isEnglish ? "Input" : "输入名")
					: (_isEnglish ? "Output" : "输出名");
			}

			if (grid.Columns.Contains("DataType"))
			{
				grid.Columns["DataType"].HeaderText = _isEnglish ? "Type" : "类型";
			}

			if (grid.Columns.Contains("DefaultValue"))
			{
				grid.Columns["DefaultValue"].HeaderText = _isEnglish ? "Current / Test" : "当前值/测试值";
			}

			if (grid.Columns.Contains("BindingPath"))
			{
				grid.Columns["BindingPath"].HeaderText = _isEnglish ? "Source" : "绑定来源";
			}

			if (grid.Columns.Contains("CurrentValue"))
			{
				grid.Columns["CurrentValue"].HeaderText = _isEnglish ? "Current Output" : "当前输出值";
			}
		}



		private void EnsureOutputPanelVisible()
		{
			// 当前版本左侧用“输入/输出”按钮切换，不再通过 SplitContainer 同时显示两块区域。
			// 这里仅保证两个表格对象没有被折叠或错误 Dock。
			try
			{
				if (gridInputs != null)
				{
					gridInputs.Dock = DockStyle.Fill;
				}

				if (gridOutputs != null)
				{
					gridOutputs.Dock = DockStyle.Fill;
				}

				ShowPinPage(_showInputPins);
			}
			catch
			{
			}
		}

		private void LoadConfigToUi()
		{
			_loading = true;

			try
			{
				txtStepName.Text = GetCurrentScriptDisplayName();
				chkEnable.Checked = true;
				txtScriptPath.Text = _config == null ? string.Empty : _config.ScriptFilePath;

				string scriptCode = string.Empty;
				if (_config != null && !string.IsNullOrWhiteSpace(_config.ScriptFilePath) && File.Exists(_config.ScriptFilePath))
				{
					scriptCode = File.ReadAllText(_config.ScriptFilePath, System.Text.Encoding.UTF8);
				}

				// 1. 清理旧模板自动生成的 JobID / Measure1 / Barcode 示例输入。
				// 2. 根据任务调度中 Script 绑定的前序模块，自动补齐 VPP/Hdev/Script 输出引脚。
				//    VPP 即使没有在算法模块页面手动打开，也会直接读取 .vpp 文件提取 OutputPins。
				// 3. 根据脚本代码中的 context.SetOutput("xxx", ...) 自动补齐输出表。
				RemoveDefaultSampleInputs();
				RemoveStalePreviousStepInputs();
				AutoMergeSelectedPreviousOutputPinsToInputs();
				AutoMergeOutputsFromScriptCode(scriptCode);
				RefreshInputBindingSourceOptions();

				EnsureOutputPanelVisible();
				LoadPinsToGrid(gridInputs, _config == null ? null : _config.Inputs);
				LoadPinsToGrid(gridOutputs, _config == null ? null : _config.Outputs);
				EnsureOutputPanelVisible();
				txtCode.Text = scriptCode;

				RefreshCodeLineNumbers();
				SetStatusReady();
			}
			finally
			{
				_loading = false;
			}
		}

		private string GetCurrentScriptDisplayName()
		{
			if (!string.IsNullOrWhiteSpace(_scriptPath))
			{
				return Path.GetFileNameWithoutExtension(_scriptPath);
			}

			if (_config != null && !string.IsNullOrWhiteSpace(_config.ScriptFileName))
			{
				return Path.GetFileNameWithoutExtension(_config.ScriptFileName);
			}

			if (_config != null && !string.IsNullOrWhiteSpace(_config.StepName))
			{
				return _config.StepName;
			}

			return "None";
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
			if (grid == null)
			{
				return;
			}

			bool isInput = grid == gridInputs;
			grid.Rows.Clear();
			EnsurePinGridHeaderVisible(grid);

			if (pins == null)
			{
				return;
			}

			foreach (ScriptPinConfig pin in pins)
			{
				if (pin == null)
				{
					continue;
				}

				int rowIndex = grid.Rows.Add();
				DataGridViewRow row = grid.Rows[rowIndex];

				SetPinCellValueIfExists(row, "Name", pin.Name);
				SetPinCellValueIfExists(row, "DataType", pin.DataType);

				if (isInput)
				{
					SetPinCellValueIfExists(row, "DefaultValue", pin.DefaultValue);
					EnsureBindingPathOption(pin.BindingPath);
					SetPinCellValueIfExists(row, "BindingPath", pin.BindingPath);
				}
				else
				{
					SetPinCellValueIfExists(row, "CurrentValue", pin.DefaultValue);
					if (grid.Columns.Contains("CurrentValue"))
					{
						row.Cells["CurrentValue"].ReadOnly = true;
					}
				}
			}

			EnsurePinGridHeaderVisible(grid);
		}


		private List<ScriptPinConfig> ReadPinsFromGrid(DataGridView grid)
		{
			List<ScriptPinConfig> result = new List<ScriptPinConfig>();

			if (grid == null)
			{
				return result;
			}

			bool isInput = grid == gridInputs;

			foreach (DataGridViewRow row in grid.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				string name = GetPinCellString(row, "Name");

				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				ScriptPinConfig pin = new ScriptPinConfig();
				pin.Name = name.Trim();
				pin.DataType = ParseDataType(GetPinCellString(row, "DataType"));

				if (isInput)
				{
					pin.DefaultValue = GetPinCellString(row, "DefaultValue");
					pin.BindingPath = GetPinCellString(row, "BindingPath");
				}
				else
				{
					// 输出“当前输出值”只用于调试显示，不保存为绑定去向。
					pin.DefaultValue = GetPinCellString(row, "CurrentValue");
					pin.BindingPath = string.Empty;
				}

				pin.Description = isInput ? GetAutoInputDescriptionByBinding(pin.BindingPath) : string.Empty;
				result.Add(pin);
			}

			return result;
		}


		private void SetPinCellValueIfExists(DataGridViewRow row, string columnName, object value)
		{
			if (row == null || row.DataGridView == null)
			{
				return;
			}

			if (!row.DataGridView.Columns.Contains(columnName))
			{
				return;
			}

			row.Cells[columnName].Value = value;
		}

		private string GetPinCellString(DataGridViewRow row, string columnName)
		{
			if (row == null || row.DataGridView == null)
			{
				return string.Empty;
			}

			if (!row.DataGridView.Columns.Contains(columnName))
			{
				return string.Empty;
			}

			object value = row.Cells[columnName].Value;
			return value == null ? string.Empty : Convert.ToString(value);
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
				SetCodeTextSafely(File.ReadAllText(dialog.FileName, System.Text.Encoding.UTF8));
				RefreshCodeLineNumbersNow();
				LogInfo("Script file loaded: " + dialog.FileName);
			}
		}

		private void btnReferenceDll_Click(object sender, EventArgs e)
		{
			try
			{
				CSharpScriptReferenceManager.EnsureReferenceFolder();

				List<ScriptReferenceViewItem> references = BuildCurrentReferenceViewItems();
				List<string> autoUsings = BuildCurrentAutoUsingNamespaces();

				using (ScriptReferenceInfoForm form = new ScriptReferenceInfoForm(
					references,
					autoUsings,
					CSharpScriptReferenceManager.ReferenceFolder,
					CSharpScriptReferenceManager.UsingConfigFile))
				{
					DialogResult result = form.ShowDialog(this);

					if (result == DialogResult.Retry)
					{
						ImportGlobalDlls();
					}
					else if (result == DialogResult.Yes)
					{
						OpenGlobalReferenceFolder();
					}
				}

				LogCurrentReferenceInfo();
				SetStatusReady();
			}
			catch (Exception ex)
			{
				LogError("Show reference info failed: " + ex.Message);
				SetStatusError("Reference info failed");
			}
		}

		private List<ScriptReferenceViewItem> BuildCurrentReferenceViewItems()
		{
			List<ScriptReferenceViewItem> result = new List<ScriptReferenceViewItem>();

			// 这些项目要和 CSharpScriptStepRunner.AddDefaultReferences() 保持一致。
			AddReferenceViewItem(result, "Default", "mscorlib", typeof(object).Assembly.Location);
			AddReferenceViewItem(result, "Default", "System.dll", ResolveLoadedOrFrameworkAssembly("System.dll"));
			AddReferenceViewItem(result, "Default", "System.Core.dll", ResolveLoadedOrFrameworkAssembly("System.Core.dll"));
			AddReferenceViewItem(result, "Default", "System.Data.dll", ResolveLoadedOrFrameworkAssembly("System.Data.dll"));
			AddReferenceViewItem(result, "Default", "System.Drawing.dll", ResolveLoadedOrFrameworkAssembly("System.Drawing.dll"));
			AddReferenceViewItem(result, "Default", "System.Windows.Forms.dll", ResolveLoadedOrFrameworkAssembly("System.Windows.Forms.dll"));
			AddReferenceViewItem(result, "Default", "System.Xml.dll", ResolveLoadedOrFrameworkAssembly("System.Xml.dll"));
			AddReferenceViewItem(result, "Default", "System.Xml.Linq.dll", ResolveLoadedOrFrameworkAssembly("System.Xml.Linq.dll"));
			AddReferenceViewItem(result, "Default", "Microsoft.CSharp.dll", ResolveLoadedOrFrameworkAssembly("Microsoft.CSharp.dll"));

			AddReferenceViewItem(result, "Current Program", "IScriptMain / Aron_V3", typeof(IScriptMain).Assembly.Location);
			AddReferenceViewItem(result, "Current Program", "IScriptContext / Aron_V3", typeof(IScriptContext).Assembly.Location);
			AddReferenceViewItem(result, "Current Program", "CSharpScriptStepRunner", typeof(CSharpScriptStepRunner).Assembly.Location);

			try
			{
				Assembly entry = Assembly.GetEntryAssembly();
				if (entry != null && !string.IsNullOrWhiteSpace(entry.Location))
				{
					AddReferenceViewItem(result, "Entry Assembly", entry.GetName().Name, entry.Location);
				}
			}
			catch
			{
			}

			AddLoadedAssembliesByPrefixToView(result, "Loaded Aron_V3", "Aron_V3");
			AddLoadedAssembliesByPrefixToView(result, "Loaded Cognex", "Cognex.");
			AddLoadedAssembliesByPrefixToView(result, "Loaded MVTec", "MVTec.");
			AddLoadedAssembliesByPrefixToView(result, "Loaded Halcon", "Halcon");

			List<string> globalDlls = CSharpScriptReferenceManager.GetReferenceDllPaths();
			foreach (string dll in globalDlls)
			{
				AddReferenceViewItem(result, "Global ScriptReferences", Path.GetFileNameWithoutExtension(dll), dll);
			}

			if (_config != null && _config.References != null)
			{
				foreach (ScriptReferenceConfig r in _config.References)
				{
					if (r == null)
					{
						continue;
					}

					string name = string.IsNullOrWhiteSpace(r.ReferenceName)
						? Path.GetFileNameWithoutExtension(r.DllPath)
						: r.ReferenceName;

					AddReferenceViewItem(
						result,
						r.Enable ? "Script Private" : "Script Private Disabled",
						name,
						r.DllPath);
				}
			}

			return result;
		}

		private List<string> BuildCurrentAutoUsingNamespaces()
		{
			List<string> result = new List<string>();

			AddUsingNamespace(result, "System");
			AddUsingNamespace(result, "System.IO");
			AddUsingNamespace(result, "System.Text");
			AddUsingNamespace(result, "System.Linq");
			AddUsingNamespace(result, "System.Data");
			AddUsingNamespace(result, "System.Drawing");
			AddUsingNamespace(result, "System.Collections");
			AddUsingNamespace(result, "System.Collections.Generic");
			AddUsingNamespace(result, "System.Text.RegularExpressions");
			AddUsingNamespace(result, "System.Windows.Forms");
			AddUsingNamespace(result, "Aron_V3");

			foreach (string ns in CSharpScriptReferenceManager.GetAutoUsingNamespaces())
			{
				AddUsingNamespace(result, ns);
			}

			if (IsAssemblyLoadedForReferenceView("Cognex.VisionPro"))
			{
				AddUsingNamespace(result, "Cognex.VisionPro");
				AddUsingNamespace(result, "Cognex.VisionPro.ToolBlock");
				AddUsingNamespace(result, "Cognex.VisionPro.PMAlign");
				AddUsingNamespace(result, "Cognex.VisionPro.ImageProcessing");
			}

			return result;
		}

		private void AddUsingNamespace(List<string> list, string ns)
		{
			if (list == null || string.IsNullOrWhiteSpace(ns))
			{
				return;
			}

			string text = ns.Trim();
			if (text.StartsWith("using ", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(6).Trim();
			}

			if (text.EndsWith(";"))
			{
				text = text.Substring(0, text.Length - 1).Trim();
			}

			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}

			foreach (string item in list)
			{
				if (string.Equals(item, text, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
			}

			list.Add(text);
		}

		private void AddReferenceViewItem(List<ScriptReferenceViewItem> list, string source, string name, string path)
		{
			if (list == null)
			{
				return;
			}

			string finalSource = source ?? string.Empty;
			string finalName = name ?? string.Empty;
			string finalPath = path ?? string.Empty;

			foreach (ScriptReferenceViewItem item in list)
			{
				if (string.Equals(item.Path, finalPath, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(item.Name, finalName, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
			}

			ScriptReferenceViewItem row = new ScriptReferenceViewItem();
			row.Source = finalSource;
			row.Name = finalName;
			row.Path = finalPath;
			row.Exists = !string.IsNullOrWhiteSpace(finalPath) && File.Exists(finalPath);
			list.Add(row);
		}

		private void AddLoadedAssembliesByPrefixToView(List<ScriptReferenceViewItem> list, string source, string prefix)
		{
			if (string.IsNullOrWhiteSpace(prefix))
			{
				return;
			}

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly asm in assemblies)
			{
				try
				{
					if (asm == null || asm.IsDynamic)
					{
						continue;
					}

					string asmName = asm.GetName().Name;
					if (string.IsNullOrWhiteSpace(asmName))
					{
						continue;
					}

					if (!asmName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					string location = asm.Location;
					if (string.IsNullOrWhiteSpace(location))
					{
						continue;
					}

					AddReferenceViewItem(list, source, asmName, location);
				}
				catch
				{
				}
			}
		}

		private string ResolveLoadedOrFrameworkAssembly(string dllName)
		{
			if (string.IsNullOrWhiteSpace(dllName))
			{
				return string.Empty;
			}

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly asm in assemblies)
			{
				try
				{
					if (asm == null || asm.IsDynamic)
					{
						continue;
					}

					string location = asm.Location;
					if (string.IsNullOrWhiteSpace(location))
					{
						continue;
					}

					if (string.Equals(Path.GetFileName(location), dllName, StringComparison.OrdinalIgnoreCase))
					{
						return location;
					}
				}
				catch
				{
				}
			}

			try
			{
				string mscorlibDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
				if (!string.IsNullOrWhiteSpace(mscorlibDir))
				{
					string path = Path.Combine(mscorlibDir, dllName);
					if (File.Exists(path))
					{
						return path;
					}
				}
			}
			catch
			{
			}

			return dllName;
		}

		private bool IsAssemblyLoadedForReferenceView(string assemblyName)
		{
			if (string.IsNullOrWhiteSpace(assemblyName))
			{
				return false;
			}

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly asm in assemblies)
			{
				try
				{
					if (asm != null &&
						string.Equals(asm.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
				catch
				{
				}
			}

			return false;
		}

		private void OpenGlobalReferenceFolder()
		{
			try
			{
				CSharpScriptReferenceManager.EnsureReferenceFolder();
				Process.Start("explorer.exe", CSharpScriptReferenceManager.ReferenceFolder);
				LogInfo("Global reference folder opened: " + CSharpScriptReferenceManager.ReferenceFolder);
			}
			catch (Exception ex)
			{
				LogError("Open reference folder failed: " + ex.Message);
			}
		}

		private void ImportGlobalDlls()
		{
			try
			{
				CSharpScriptReferenceManager.EnsureReferenceFolder();

				using (OpenFileDialog dialog = new OpenFileDialog())
				{
					dialog.Title = "Import global script reference DLL";
					dialog.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";
					dialog.Multiselect = true;

					if (dialog.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}

					foreach (string file in dialog.FileNames)
					{
						if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
						{
							continue;
						}

						string target = Path.Combine(
							CSharpScriptReferenceManager.ReferenceFolder,
							Path.GetFileName(file));

						File.Copy(file, target, true);
						LogInfo("Imported global DLL: " + target);
					}
				}

				CSharpScriptReferenceManager.PreloadAllReferenceDlls();
				LogCurrentReferenceInfo();
				SetStatusOK("DLL imported");
			}
			catch (Exception ex)
			{
				LogError("Import DLL failed: " + ex.Message);
				SetStatusError("Import DLL failed");
			}
		}

		private void LogCurrentReferenceInfo()
		{
			try
			{
				List<ScriptReferenceViewItem> references = BuildCurrentReferenceViewItems();
				List<string> autoUsings = BuildCurrentAutoUsingNamespaces();

				LogInfo("========== Script Reference Info ==========");
				LogInfo("Current Script: " + (_config == null ? string.Empty : _config.StepName));
				LogInfo("Reference Folder: " + CSharpScriptReferenceManager.ReferenceFolder);
				LogInfo("Using Config File: " + CSharpScriptReferenceManager.UsingConfigFile);

				foreach (ScriptReferenceViewItem item in references)
				{
					if (item == null)
					{
						continue;
					}

					LogInfo(
						"[" + item.Source + "] " +
						item.Name +
						" | Exists=" + item.Exists.ToString() +
						" | " + item.Path);
				}

				LogInfo("---------- Auto Using ----------");
				foreach (string ns in autoUsings)
				{
					LogInfo("using " + ns + ";");
				}

				LogInfo("===========================================");
			}
			catch (Exception ex)
			{
				LogError("Show reference info failed: " + ex.Message);
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
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				string name = GetPinCellString(row, "Name");
				string bindingPath = GetPinCellString(row, "BindingPath");
				string value = GetPinCellString(row, "DefaultValue");

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
				UpdateOutputCurrentValue(pair.Key, pair.Value);
			}
		}

		private void UpdateOutputCurrentValue(string outputName, object value)
		{
			if (gridOutputs == null || string.IsNullOrWhiteSpace(outputName))
			{
				return;
			}

			if (!gridOutputs.Columns.Contains("Name") || !gridOutputs.Columns.Contains("CurrentValue"))
			{
				return;
			}

			foreach (DataGridViewRow row in gridOutputs.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				string name = GetPinCellString(row, "Name");

				if (string.Equals(name, outputName, StringComparison.OrdinalIgnoreCase))
				{
					row.Cells["CurrentValue"].Value = value == null ? string.Empty : Convert.ToString(value);
					row.Cells["CurrentValue"].ReadOnly = true;
					return;
				}
			}

			int rowIndex = gridOutputs.Rows.Add();
			DataGridViewRow newRow = gridOutputs.Rows[rowIndex];

			SetPinCellValueIfExists(newRow, "Name", outputName);
			SetPinCellValueIfExists(newRow, "DataType", ScriptPinDataType.String);
			SetPinCellValueIfExists(newRow, "CurrentValue", value == null ? string.Empty : Convert.ToString(value));

			if (gridOutputs.Columns.Contains("CurrentValue"))
			{
				newRow.Cells["CurrentValue"].ReadOnly = true;
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
			// 关键优化：输入普通字符时行号数量没有变化，不需要重绘行号栏。
			// 原来的写法是每次 TextChanged 都 Invalidate，写代码时会导致左侧行号跳动和闪烁。
			if (_loadingCodeText)
			{
				return;
			}

			int lineCount = GetCodeLineCountFast();
			if (lineCount != _lastKnownCodeLineCount)
			{
				_lastKnownCodeLineCount = lineCount;
				RequestLineNumberRefresh(false);
			}
		}

		private void txtCode_VScroll(object sender, EventArgs e)
		{
			// 滚动时必须刷新，但仍通过短延时合并多次滚动消息。
			RequestLineNumberRefresh(false);
		}

		private void txtCode_Resize(object sender, EventArgs e)
		{
			RequestLineNumberRefresh(true);
		}

		private void txtCode_FontChanged(object sender, EventArgs e)
		{
			RequestLineNumberRefresh(true);
		}

		private void RefreshCodeLineNumbers()
		{
			RequestLineNumberRefresh(false);
		}

		private void RefreshCodeLineNumbersNow()
		{
			if (panelLineNumbers == null || panelLineNumbers.IsDisposed)
			{
				return;
			}

			_lastKnownCodeLineCount = GetCodeLineCountFast();

			if (_lineNumberRefreshTimer != null)
			{
				_lineNumberRefreshTimer.Stop();
			}

			panelLineNumbers.Invalidate();
		}

		private void RequestLineNumberRefresh(bool immediate)
		{
			if (panelLineNumbers == null || panelLineNumbers.IsDisposed)
			{
				return;
			}

			if (immediate || !panelLineNumbers.IsHandleCreated)
			{
				RefreshCodeLineNumbersNow();
				return;
			}

			_forceLineNumberRefresh = true;

			if (_lineNumberRefreshTimer == null)
			{
				_lineNumberRefreshTimer = new Timer();
				_lineNumberRefreshTimer.Interval = 80;
				_lineNumberRefreshTimer.Tick += delegate
				{
					_lineNumberRefreshTimer.Stop();

					if (!_forceLineNumberRefresh)
					{
						return;
					}

					_forceLineNumberRefresh = false;

					if (panelLineNumbers != null && !panelLineNumbers.IsDisposed)
					{
						panelLineNumbers.Invalidate();
					}
				};
			}

			_lineNumberRefreshTimer.Stop();
			_lineNumberRefreshTimer.Start();
		}

		private int GetCodeLineCountFast()
		{
			if (txtCode == null)
			{
				return 1;
			}

			try
			{
				int length = txtCode.TextLength;
				if (length <= 0)
				{
					return 1;
				}

				return txtCode.GetLineFromCharIndex(length - 1) + 1;
			}
			catch
			{
				return 1;
			}
		}

		private void SetCodeTextSafely(string code)
		{
			if (txtCode == null)
			{
				return;
			}

			_loadingCodeText = true;
			SuspendControlRedraw(txtCode);

			try
			{
				txtCode.Text = code ?? string.Empty;
				txtCode.SelectionStart = 0;
				_lastKnownCodeLineCount = GetCodeLineCountFast();
			}
			finally
			{
				ResumeControlRedraw(txtCode);
				_loadingCodeText = false;
			}
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

				if (txtCode.ClientSize.Height <= 0)
				{
					return;
				}

				int firstCharIndex = txtCode.GetCharIndexFromPosition(new Point(0, 0));
				int firstLine = txtCode.GetLineFromCharIndex(firstCharIndex);

				int lastCharIndex = txtCode.GetCharIndexFromPosition(new Point(0, txtCode.ClientSize.Height - 1));
				int lastLine = txtCode.GetLineFromCharIndex(lastCharIndex);

				if (lastLine < firstLine)
				{
					lastLine = firstLine;
				}

				int totalLines = GetCodeLineCountFast();
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

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (_lineNumberRefreshTimer != null)
			{
				_lineNumberRefreshTimer.Stop();
				_lineNumberRefreshTimer.Dispose();
				_lineNumberRefreshTimer = null;
			}

			base.OnHandleDestroyed(e);
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

	public class ScriptReferenceViewItem
	{
		public string Source { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public bool Exists { get; set; }

		public ScriptReferenceViewItem()
		{
			Source = string.Empty;
			Name = string.Empty;
			Path = string.Empty;
			Exists = false;
		}
	}

	public class ScriptReferenceInfoForm : Form
	{
		private DataGridView grid;
		private TextBox txtUsings;
		private TextBox txtFolder;
		private Button btnOpenFolder;
		private Button btnImportDll;
		private Button btnClose;
		private string _folder;
		private string _usingFile;

		public ScriptReferenceInfoForm(
			List<ScriptReferenceViewItem> references,
			List<string> usings,
			string folder,
			string usingFile)
		{
			_folder = folder ?? string.Empty;
			_usingFile = usingFile ?? string.Empty;

			InitializeUi();
			LoadReferences(references);
			LoadUsings(usings);
		}

		private void InitializeUi()
		{
			Text = "Script Reference Info";
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(1080, 680);
			MinimumSize = new Size(920, 560);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;

			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.ColumnCount = 1;
			root.RowCount = 5;
			root.Padding = new Padding(10);
			root.BackColor = BackColor;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));

			Label lblTitle = new Label();
			lblTitle.Dock = DockStyle.Fill;
			lblTitle.TextAlign = ContentAlignment.MiddleLeft;
			lblTitle.ForeColor = Color.White;
			lblTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			lblTitle.Text = "当前脚本编译引用 DLL / Current script compile references";

			txtFolder = new TextBox();
			txtFolder.Dock = DockStyle.Fill;
			txtFolder.ReadOnly = true;
			txtFolder.BorderStyle = BorderStyle.FixedSingle;
			txtFolder.BackColor = Color.FromArgb(1, 8, 16);
			txtFolder.ForeColor = Color.FromArgb(210, 230, 245);
			txtFolder.Font = new Font("Consolas", 9F);
			txtFolder.Text = "DLL Folder: " + _folder;

			grid = new DataGridView();
			grid.Dock = DockStyle.Fill;
			grid.AllowUserToAddRows = false;
			grid.AllowUserToDeleteRows = false;
			grid.ReadOnly = true;
			grid.RowHeadersVisible = false;
			grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			grid.MultiSelect = false;
			grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			grid.BackgroundColor = Color.FromArgb(1, 8, 16);
			grid.GridColor = Color.FromArgb(45, 70, 95);
			grid.BorderStyle = BorderStyle.FixedSingle;
			grid.EnableHeadersVisualStyles = false;

			grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);

			grid.DefaultCellStyle.BackColor = Color.FromArgb(1, 8, 16);
			grid.DefaultCellStyle.ForeColor = Color.White;
			grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			grid.DefaultCellStyle.SelectionForeColor = Color.White;
			grid.RowTemplate.Height = 24;

			grid.Columns.Add("Source", "来源");
			grid.Columns.Add("Name", "名称");
			grid.Columns.Add("Exists", "存在");
			grid.Columns.Add("Path", "路径");

			grid.Columns["Source"].FillWeight = 135;
			grid.Columns["Name"].FillWeight = 170;
			grid.Columns["Exists"].FillWeight = 55;
			grid.Columns["Path"].FillWeight = 540;

			txtUsings = new TextBox();
			txtUsings.Dock = DockStyle.Fill;
			txtUsings.Multiline = true;
			txtUsings.ReadOnly = true;
			txtUsings.ScrollBars = ScrollBars.Both;
			txtUsings.WordWrap = false;
			txtUsings.BackColor = Color.FromArgb(1, 8, 16);
			txtUsings.ForeColor = Color.FromArgb(210, 230, 245);
			txtUsings.Font = new Font("Consolas", 10F);

			Panel buttonPanel = new Panel();
			buttonPanel.Dock = DockStyle.Fill;
			buttonPanel.BackColor = BackColor;

			btnOpenFolder = CreateButton("打开DLL目录", 10, 8, 130);
			btnImportDll = CreateButton("导入DLL", 150, 8, 120);
			btnClose = CreateButton("关闭", 930, 8, 100);
			btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;

			btnOpenFolder.Click += btnOpenFolder_Click;
			btnImportDll.Click += btnImportDll_Click;
			btnClose.Click += btnClose_Click;

			buttonPanel.Controls.Add(btnOpenFolder);
			buttonPanel.Controls.Add(btnImportDll);
			buttonPanel.Controls.Add(btnClose);

			root.Controls.Add(lblTitle, 0, 0);
			root.Controls.Add(txtFolder, 0, 1);
			root.Controls.Add(grid, 0, 2);
			root.Controls.Add(txtUsings, 0, 3);
			root.Controls.Add(buttonPanel, 0, 4);

			Controls.Add(root);
		}

		private Button CreateButton(string text, int x, int y, int width)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(width, 30);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.FlatAppearance.BorderSize = 1;
			btn.BackColor = Color.FromArgb(0, 95, 190);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private void LoadReferences(List<ScriptReferenceViewItem> references)
		{
			grid.Rows.Clear();
			if (references == null)
			{
				return;
			}

			foreach (ScriptReferenceViewItem item in references)
			{
				if (item == null)
				{
					continue;
				}

				int row = grid.Rows.Add(
					item.Source,
					item.Name,
					item.Exists ? "Yes" : "No",
					item.Path);

				if (!item.Exists)
				{
					grid.Rows[row].DefaultCellStyle.ForeColor = Color.FromArgb(255, 120, 120);
				}
			}
		}

		private void LoadUsings(List<string> usings)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Auto using namespaces used during compilation:");

			if (usings != null)
			{
				foreach (string ns in usings)
				{
					if (!string.IsNullOrWhiteSpace(ns))
					{
						sb.AppendLine("using " + ns + ";");
					}
				}
			}

			sb.AppendLine();
			sb.AppendLine("Using config file:");
			sb.AppendLine(_usingFile);
			sb.AppendLine();
			sb.AppendLine("说明：如果 DLL 的真实 namespace 和文件名不同，请手动编辑 ScriptUsings.txt，每行写一个 namespace。");
			txtUsings.Text = sb.ToString();
		}

		private void btnOpenFolder_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			Close();
		}

		private void btnImportDll_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Retry;
			Close();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}

}
