using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;

namespace Aron_V3
{
	internal enum FunctionBlockLibraryMode
	{
		Step = 0,
		Hardware = 1,
		Signal = 2,
		Database = 3
	}

	public partial class TaskSchedulerControl : UserControl, ILocalizable
	{
		private bool _loading = false;
		private bool _applyingFlowVisual = false;
		private bool _isEnglish = false;
		private TableLayoutPanel panelStepListBottom;
		private FlowLayoutPanel panelFunctionBlockTabs;
		private ComboBox cmbFunctionBlockMode;
		private bool _updatingFunctionBlockModeCombo = false;
		private Button btnOpenStepFolder;
		private ToolTip stepActionToolTip;
		private FunctionBlockLibraryMode _functionBlockLibraryMode = FunctionBlockLibraryMode.Step;
		private const string COL_DISPLAY_OUTPUT = "DisplayOutputKey";
		private const string COL_DISPLAY_SLOT = "DisplaySlotName";
		private const string COL_DISPLAY_RESULT = "DisplayResultKey";
		private const string COL_DISPLAY_MODE = "DisplayMode";
		private const string FLOW_BLOCK_STEP = "Step";
		private const string FLOW_BLOCK_HARDWARE = "Hardware";
		private const string FLOW_BLOCK_SIGNAL = "Signal";
		private const string FLOW_BLOCK_DATABASE = "Database";
		private const string DISPLAY_OUTPUT_NOT_USE = "Not Use";
		private const string VPP_DISPLAY_OUTPUT_IMAGE = "CogIPoneImage";
		private const string HDEV_DISPLAY_OUTPUT_IMAGE = "ResultImage";
		private const string DEFAULT_DISPLAY_RESULT_OUTPUT = "ImageResult";

		public TaskSchedulerControl()
		{
			InitializeComponent();
			ConfigureTaskProgramNavigation();
			ConfigureStepListActions();
			BindStepListDrawEvents();
			InitDisplayBindingColumns();
			MakeStepNameColumnReadOnly();
			BindStepGridReadOnlyEvents();
			ApplyFlowUiPolicy();
			BindStepFlowGridEvents();
			EnableDoubleBufferForPage();


			listJobs.SelectedIndexChanged -= listJobs_SelectedIndexChanged;
			listJobs.SelectedIndexChanged += listJobs_SelectedIndexChanged;

			listTasks.SelectedIndexChanged -= listTasks_SelectedIndexChanged;
			listTasks.SelectedIndexChanged += listTasks_SelectedIndexChanged;

			listSteps.SelectedIndexChanged -= listSteps_SelectedIndexChanged;
			listSteps.SelectedIndexChanged += listSteps_SelectedIndexChanged;

			FlowConfigStore.FlowConfigSaved -= FlowConfigStore_FlowConfigSaved;
			FlowConfigStore.FlowConfigSaved += FlowConfigStore_FlowConfigSaved;
			CommunicationConfigChangedHub.ConfigChanged -= CommunicationConfigChangedHub_ConfigChanged;
			CommunicationConfigChangedHub.ConfigChanged += CommunicationConfigChangedHub_ConfigChanged;
			DisplayLayoutStore.DisplayLayoutSaved -= DisplayLayoutStore_DisplayLayoutSaved;
			DisplayLayoutStore.DisplayLayoutSaved += DisplayLayoutStore_DisplayLayoutSaved;



			LoadFlowConfigToUI();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			FlowConfigStore.FlowConfigSaved -= FlowConfigStore_FlowConfigSaved;
			CommunicationConfigChangedHub.ConfigChanged -= CommunicationConfigChangedHub_ConfigChanged;
			DisplayLayoutStore.DisplayLayoutSaved -= DisplayLayoutStore_DisplayLayoutSaved;
			base.OnHandleDestroyed(e);
		}

		private void ConfigureTaskProgramNavigation()
		{
			if (leftLayout == null || panelTasks == null || panelJobs == null)
			{
				return;
			}

			leftLayout.SuspendLayout();
			leftLayout.Controls.Clear();
			leftLayout.RowStyles.Clear();
			leftLayout.RowCount = 2;
			leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

			leftLayout.Controls.Add(panelTasks, 0, 0);
			leftLayout.Controls.Add(panelJobs, 0, 1);

			panelTasks.Margin = new Padding(0, 0, 0, 14);
			panelJobs.Margin = new Padding(0);
			lblTasksTitle.Text = "所有 task";
			lblJobsTitle.Text = "所有程序号";
			leftLayout.ResumeLayout(true);
		}

		private void ConfigureStepListActions()
		{
			if (panelStepList == null)
			{
				return;
			}

			if (stepActionToolTip == null)
			{
				stepActionToolTip = new ToolTip();
			}

			ConfigureFunctionBlockModeTabs();

			if (panelStepListBottom == null)
			{
				panelStepListBottom = new TableLayoutPanel();
				panelStepListBottom.Name = "panelStepListBottom";
				panelStepListBottom.ColumnCount = 1;
				panelStepListBottom.RowCount = 1;
				panelStepListBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
				panelStepListBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
				panelStepListBottom.Dock = DockStyle.Bottom;
				panelStepListBottom.Height = 58;
				panelStepListBottom.Padding = new Padding(6, 9, 6, 0);
				panelStepListBottom.Margin = new Padding(0);
				panelStepListBottom.BackColor = panelStepList.BackColor;
				panelStepList.Controls.Add(panelStepListBottom);
				panelStepListBottom.BringToFront();
			}

			MoveAddSelectedButtonToStepListBottom();
			ConfigureFlowBottomButtons();
			EnsureOpenStepFolderButton();
			ApplyFunctionBlockLibraryModeVisual();

			if (btnBatchAddStepItem != null)
			{
				stepActionToolTip.SetToolTip(btnBatchAddStepItem, "新建 VPP / Script / Hdev");
			}
		}

		private void ConfigureFunctionBlockModeTabs()
		{
			if (panelStepListHeader == null)
			{
				return;
			}

			if (panelFunctionBlockTabs == null)
			{
				panelFunctionBlockTabs = new FlowLayoutPanel();
				panelFunctionBlockTabs.Name = "panelFunctionBlockTabs";
				panelFunctionBlockTabs.Dock = DockStyle.Fill;
				panelFunctionBlockTabs.Margin = new Padding(0);
				panelFunctionBlockTabs.Padding = new Padding(0, 4, 0, 4);
				panelFunctionBlockTabs.WrapContents = false;
				panelFunctionBlockTabs.BackColor = panelStepList.BackColor;

				cmbFunctionBlockMode = new ComboBox();
				cmbFunctionBlockMode.Name = "cmbFunctionBlockMode";
				cmbFunctionBlockMode.DropDownStyle = ComboBoxStyle.DropDownList;
				cmbFunctionBlockMode.FlatStyle = FlatStyle.Flat;
				cmbFunctionBlockMode.Width = GetFunctionBlockModeComboWidth();
				cmbFunctionBlockMode.Height = 34;
				cmbFunctionBlockMode.Margin = new Padding(0);
				cmbFunctionBlockMode.BackColor = Color.FromArgb(8, 21, 39);
				cmbFunctionBlockMode.ForeColor = Color.White;
				cmbFunctionBlockMode.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
				cmbFunctionBlockMode.DrawMode = DrawMode.OwnerDrawFixed;
				cmbFunctionBlockMode.ItemHeight = 28;
				cmbFunctionBlockMode.DropDownWidth = GetFunctionBlockModeComboWidth();
				cmbFunctionBlockMode.DropDownHeight = 140;
				cmbFunctionBlockMode.IntegralHeight = false;
				cmbFunctionBlockMode.DrawItem += FunctionBlockModeCombo_DrawItem;
				cmbFunctionBlockMode.SelectedIndexChanged += FunctionBlockModeCombo_SelectedIndexChanged;

				panelFunctionBlockTabs.Controls.Add(cmbFunctionBlockMode);
				panelStepListHeader.Resize += panelStepListHeader_Resize;
				AdjustFunctionBlockModeComboWidth();
				PopulateFunctionBlockModeCombo();
			}

			if (panelFunctionBlockTabs.Parent != panelStepListHeader)
			{
				panelStepListHeader.Controls.Clear();
				panelStepListHeader.Controls.Add(panelFunctionBlockTabs);
			}

			if (lblStepListTitle != null)
			{
				lblStepListTitle.Visible = false;
			}
		}

		private void panelStepListHeader_Resize(object sender, EventArgs e)
		{
			AdjustFunctionBlockModeComboWidth();
		}

		private int GetFunctionBlockModeComboWidth()
		{
			int width = panelStepListHeader == null ? 252 : panelStepListHeader.ClientSize.Width;
			return Math.Max(120, width);
		}

		private void AdjustFunctionBlockModeComboWidth()
		{
			if (cmbFunctionBlockMode == null)
			{
				return;
			}

			int width = GetFunctionBlockModeComboWidth();
			cmbFunctionBlockMode.Width = width;
			cmbFunctionBlockMode.DropDownWidth = width;
		}

		private void PopulateFunctionBlockModeCombo()
		{
			if (cmbFunctionBlockMode == null)
			{
				return;
			}

			_updatingFunctionBlockModeCombo = true;
			try
			{
				FunctionBlockLibraryMode selectedMode = _functionBlockLibraryMode;
				cmbFunctionBlockMode.Items.Clear();
				cmbFunctionBlockMode.Items.Add(new FunctionBlockModeOption(FunctionBlockLibraryMode.Step, GetFunctionBlockModeText(FunctionBlockLibraryMode.Step)));
				cmbFunctionBlockMode.Items.Add(new FunctionBlockModeOption(FunctionBlockLibraryMode.Hardware, GetFunctionBlockModeText(FunctionBlockLibraryMode.Hardware)));
				cmbFunctionBlockMode.Items.Add(new FunctionBlockModeOption(FunctionBlockLibraryMode.Signal, GetFunctionBlockModeText(FunctionBlockLibraryMode.Signal)));
				cmbFunctionBlockMode.Items.Add(new FunctionBlockModeOption(FunctionBlockLibraryMode.Database, GetFunctionBlockModeText(FunctionBlockLibraryMode.Database)));
				SelectFunctionBlockModeCombo(selectedMode);
			}
			finally
			{
				_updatingFunctionBlockModeCombo = false;
			}
		}

		private string GetFunctionBlockModeText(FunctionBlockLibraryMode mode)
		{
			if (mode == FunctionBlockLibraryMode.Hardware)
			{
				return "Hardware";
			}

			if (mode == FunctionBlockLibraryMode.Signal)
			{
				return "Signal";
			}

			if (mode == FunctionBlockLibraryMode.Database)
			{
				return "Database";
			}

			return "Step";
		}

		private void SelectFunctionBlockModeCombo(FunctionBlockLibraryMode mode)
		{
			if (cmbFunctionBlockMode == null)
			{
				return;
			}

			for (int i = 0; i < cmbFunctionBlockMode.Items.Count; i++)
			{
				FunctionBlockModeOption option = cmbFunctionBlockMode.Items[i] as FunctionBlockModeOption;
				if (option != null && option.Mode == mode)
				{
					cmbFunctionBlockMode.SelectedIndex = i;
					return;
				}
			}
		}

		private void FunctionBlockModeCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_updatingFunctionBlockModeCombo)
			{
				return;
			}

			FunctionBlockModeOption option = cmbFunctionBlockMode == null
				? null
				: cmbFunctionBlockMode.SelectedItem as FunctionBlockModeOption;
			if (option == null)
			{
				return;
			}

			SetFunctionBlockLibraryMode(option.Mode, true);
		}

		private void FunctionBlockModeCombo_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0 || cmbFunctionBlockMode == null)
			{
				return;
			}

			bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
			Color backColor = selected ? Color.FromArgb(0, 120, 200) : Color.FromArgb(8, 21, 39);
			Color foreColor = selected ? Color.White : Color.FromArgb(220, 235, 245);

			using (SolidBrush brush = new SolidBrush(backColor))
			{
				e.Graphics.FillRectangle(brush, e.Bounds);
			}

			string text = Convert.ToString(cmbFunctionBlockMode.Items[e.Index]);
			Rectangle textBounds = new Rectangle(e.Bounds.Left + 10, e.Bounds.Top, e.Bounds.Width - 20, e.Bounds.Height);
			TextRenderer.DrawText(
				e.Graphics,
				text,
				cmbFunctionBlockMode.Font,
				textBounds,
				foreColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

			using (Pen pen = new Pen(Color.FromArgb(0, 145, 205)))
			{
				Rectangle border = e.Bounds;
				border.Width -= 1;
				border.Height -= 1;
				e.Graphics.DrawRectangle(pen, border);
			}
		}

		private void SetFunctionBlockLibraryMode(FunctionBlockLibraryMode mode, bool refresh)
		{
			_functionBlockLibraryMode = mode;
			ApplyFunctionBlockLibraryModeVisual();

			if (refresh)
			{
				RefreshFunctionBlockLibrary(GetSelectedJobName(), GetSelectedTaskName());
				UpdateStepDetailTitle();
			}
		}

		private void ApplyFunctionBlockLibraryModeVisual()
		{
			_updatingFunctionBlockModeCombo = true;
			try
			{
				SelectFunctionBlockModeCombo(_functionBlockLibraryMode);
			}
			finally
			{
				_updatingFunctionBlockModeCombo = false;
			}
			ApplyFunctionBlockToolbarState();
		}

		private void ApplyFunctionBlockToolbarState()
		{
			bool isStep = _functionBlockLibraryMode == FunctionBlockLibraryMode.Step;
			bool isHardware = _functionBlockLibraryMode == FunctionBlockLibraryMode.Hardware;
			bool isSignal = _functionBlockLibraryMode == FunctionBlockLibraryMode.Signal;
			bool isDatabase = _functionBlockLibraryMode == FunctionBlockLibraryMode.Database;

			if (btnAddStepItem != null)
			{
				btnAddStepItem.Visible = isStep;
				btnAddStepItem.Enabled = isStep;
				stepActionToolTip.SetToolTip(btnAddStepItem, "添加本地 Step 文件");
			}

			if (btnBatchAddStepItem != null)
			{
				btnBatchAddStepItem.Visible = isStep;
				btnBatchAddStepItem.Enabled = isStep;
				stepActionToolTip.SetToolTip(btnBatchAddStepItem, "新建 VPP / Script / Hdev");
			}

			if (btnDeleteStepItem != null)
			{
				btnDeleteStepItem.Visible = isStep;
				btnDeleteStepItem.Enabled = isStep;
				stepActionToolTip.SetToolTip(btnDeleteStepItem, "删除 Step");
			}

			if (btnRefreshStepItem != null)
			{
				btnRefreshStepItem.Visible = true;
				btnRefreshStepItem.Enabled = true;
				stepActionToolTip.SetToolTip(btnRefreshStepItem, isSignal ? "刷新通讯模块" : (isDatabase ? "刷新 Database Step" : "刷新列表"));
			}

			if (btnOpenStepFolder != null)
			{
				btnOpenStepFolder.Visible = !isSignal;
				btnOpenStepFolder.Enabled = !isSignal;
				stepActionToolTip.SetToolTip(btnOpenStepFolder, isDatabase ? "打开 Database 文件夹" : (isHardware ? "打开 Hardware 文件夹" : "打开当前 Task 文件夹"));
			}

			if (btnAddStep != null)
			{
				btnAddStep.Text = "添加选中";
				string tip = isHardware
					? "添加选中的取像文件到右侧流程"
					: (isSignal ? "添加选中的通讯模块到右侧流程" : (isDatabase ? "添加 Database Step 到右侧流程" : "添加选中的 Step 到右侧流程"));
				stepActionToolTip.SetToolTip(btnAddStep, tip);
			}
		}

		private void MoveAddSelectedButtonToStepListBottom()
		{
			if (btnAddStep == null || panelStepListBottom == null)
			{
				return;
			}

			if (btnAddStep.Parent != null)
			{
				btnAddStep.Parent.Controls.Remove(btnAddStep);
			}

			panelStepListBottom.Controls.Clear();
			panelStepListBottom.Controls.Add(btnAddStep, 0, 0);

			btnAddStep.Dock = DockStyle.Fill;
			btnAddStep.Margin = new Padding(0);
			btnAddStep.Text = "添加选中";
			btnAddStep.FlatAppearance.BorderColor = Color.FromArgb(0, 145, 205);
			stepActionToolTip.SetToolTip(btnAddStep, "添加选中的 Step 到右侧流程");
		}

		private void ConfigureFlowBottomButtons()
		{
			if (panelButtons == null)
			{
				return;
			}

			panelButtons.SuspendLayout();

			try
			{
				panelButtons.Controls.Remove(btnAddStep);
				panelButtons.Controls.Remove(btnDeleteSelected);
				panelButtons.Controls.Remove(btnMoveUp);
				panelButtons.Controls.Remove(btnMoveDown);
				panelButtons.Controls.Remove(btnSave);

				panelButtons.ColumnStyles.Clear();
				panelButtons.ColumnCount = 3;
				panelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
				panelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
				panelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));

				if (btnDeleteSelected != null)
				{
					btnDeleteSelected.Dock = DockStyle.Fill;
					btnDeleteSelected.Margin = new Padding(0, 0, 10, 0);
					panelButtons.Controls.Add(btnDeleteSelected, 0, 0);
				}

				if (btnSave != null)
				{
					btnSave.Dock = DockStyle.Fill;
					btnSave.Margin = new Padding(0);
					panelButtons.Controls.Add(btnSave, 2, 0);
				}
			}
			finally
			{
				panelButtons.ResumeLayout(true);
			}
		}

		private void EnsureOpenStepFolderButton()
		{
			if (panelStepIconBar == null)
			{
				return;
			}

			if (btnOpenStepFolder == null)
			{
				btnOpenStepFolder = new Button();
				btnOpenStepFolder.Name = "btnOpenStepFolder";
				btnOpenStepFolder.Size = new Size(42, 42);
				btnOpenStepFolder.Margin = new Padding(0, 0, 8, 0);
				btnOpenStepFolder.BackColor = Color.FromArgb(8, 21, 39);
				btnOpenStepFolder.FlatStyle = FlatStyle.Flat;
				btnOpenStepFolder.FlatAppearance.BorderColor = Color.FromArgb(0, 145, 205);
				btnOpenStepFolder.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 135);
				btnOpenStepFolder.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 45, 78);
				btnOpenStepFolder.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
				btnOpenStepFolder.ForeColor = Color.White;
				btnOpenStepFolder.Text = "📁";
				btnOpenStepFolder.UseVisualStyleBackColor = false;
				btnOpenStepFolder.Click += btnOpenStepFolder_Click;
			}

			if (btnOpenStepFolder.Parent != panelStepIconBar)
			{
				panelStepIconBar.Controls.Add(btnOpenStepFolder);
			}

			stepActionToolTip.SetToolTip(btnOpenStepFolder, "打开当前 Task 文件夹");
		}

		private void FlowConfigStore_FlowConfigSaved(object sender, EventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new EventHandler(FlowConfigStore_FlowConfigSaved), sender, e);
				return;
			}

			string oldTask = GetSelectedTaskName();
			string oldJob = GetSelectedJobName();
			string oldStep = GetSelectedStepName();

			LoadFlowConfigToUI();

			SelectListItem(listTasks, oldTask);
			RefreshProgramsByTask(GetSelectedTaskName());
			SelectListItem(listJobs, oldJob);
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			SelectListItem(listSteps, oldStep);
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
		}

		private void CommunicationConfigChangedHub_ConfigChanged(object sender, EventArgs e)
		{
			if (IsDisposed)
			{
				return;
			}

			if (InvokeRequired)
			{
				try
				{
					BeginInvoke(new EventHandler(CommunicationConfigChangedHub_ConfigChanged), sender, e);
				}
				catch
				{
				}

				return;
			}

			FlowConfigStore_FlowConfigSaved(sender, e);
		}

		private void DisplayLayoutStore_DisplayLayoutSaved(object sender, EventArgs e)
		{
			if (IsDisposed)
			{
				return;
			}

			if (InvokeRequired)
			{
				try
				{
					BeginInvoke(new EventHandler(DisplayLayoutStore_DisplayLayoutSaved), sender, e);
				}
				catch
				{
				}

				return;
			}

			RefreshDisplaySlotBindingsFromLayout();
		}

		private void BindStepListDrawEvents()
		{
			if (listSteps == null)
			{
				return;
			}

			listSteps.DrawMode = DrawMode.OwnerDrawFixed;
			listSteps.DrawItem -= listSteps_DrawItem;
			listSteps.DrawItem += listSteps_DrawItem;
		}

		private void listSteps_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0 || e.Index >= listSteps.Items.Count)
			{
				return;
			}

			StepListItem item = listSteps.Items[e.Index] as StepListItem;
			FunctionBlockListItem blockItem = listSteps.Items[e.Index] as FunctionBlockListItem;
			string text = blockItem != null ? blockItem.DisplayText : (item == null ? listSteps.Items[e.Index].ToString() : item.DisplayText);
			bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
			bool isMissing = (item != null && item.IsMissing) || (blockItem != null && blockItem.IsMissing);

			Color backColor = isSelected ? Color.FromArgb(0, 120, 200) : listSteps.BackColor;
			Color foreColor = isMissing
				? Color.FromArgb(120, 132, 145)
				: (isSelected ? Color.White : listSteps.ForeColor);

			using (SolidBrush brush = new SolidBrush(backColor))
			{
				e.Graphics.FillRectangle(brush, e.Bounds);
			}

			Rectangle textBounds = new Rectangle(e.Bounds.Left + 4, e.Bounds.Top, e.Bounds.Width - 8, e.Bounds.Height);
			TextRenderer.DrawText(
				e.Graphics,
				text,
				e.Font,
				textBounds,
				foreColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

			e.DrawFocusRectangle();
		}

		private void LoadFlowConfigToUI()
		{
			_loading = true;

			try
			{
				listTasks.Items.Clear();
				listJobs.Items.Clear();
				listSteps.Items.Clear();
				dgvSteps.Rows.Clear();

				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

				RefreshTaskList(config);
				RefreshProgramsByTask(GetSelectedTaskName());
				RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
				RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
				UpdateStepDetailTitle();
			}
			finally
			{
				_loading = false;
			}
		}

		private void listJobs_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading) return;

			SyncSelectedJobAsActiveProgram();
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
			UpdateStepDetailTitle();
		}

		private void listTasks_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading) return;

			string oldJob = GetSelectedJobName();
			_loading = true;
			try
			{
				RefreshProgramsByTask(GetSelectedTaskName());
				SelectListItem(listJobs, oldJob);
			}
			finally
			{
				_loading = false;
			}

			SyncSelectedJobAsActiveProgram();
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
			UpdateStepDetailTitle();
		}

		private void listSteps_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading) return;

			UpdateStepDetailTitle();
		}

		#region Job

		private void listJobs_DoubleClick(object sender, EventArgs e)
		{
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
			UpdateStepDetailTitle();
		}

		private void SyncSelectedJobAsActiveProgram()
		{
			string protocolName = GetSelectedProtocolName();
			string channelName = GetSelectedChannelName();
			string jobName = GetSelectedJobName();

			if (string.IsNullOrWhiteSpace(protocolName) ||
				string.IsNullOrWhiteSpace(channelName) ||
				string.IsNullOrWhiteSpace(jobName))
			{
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			ChannelFlowConfig channel = FlowConfigStore.GetOrCreateChannel(config, protocolName, channelName);
			JobConfig job = channel.Jobs.FirstOrDefault(x => x != null && string.Equals(x.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job == null)
			{
				return;
			}

			string programNo = string.IsNullOrWhiteSpace(job.ProgramNo) ? "1" : job.ProgramNo;
			if (!string.Equals(channel.ActiveProgramNo, programNo, StringComparison.OrdinalIgnoreCase))
			{
				channel.ActiveProgramNo = programNo;
				FlowConfigStore.Save(config);
			}

			UpdateDisplayInfoForSelectedJob(programNo);
		}

		private void UpdateDisplayInfoForSelectedJob(string programNo)
		{
			TaskConfig task = GetTaskConfig(GetSelectedJobName(), GetSelectedTaskName());
			if (task == null || task.StepFlow == null)
			{
				return;
			}

			foreach (string displaySlot in task.StepFlow
				.Where(x => x != null && !string.IsNullOrWhiteSpace(x.DisplaySlotName) && !x.DisplaySlotName.Equals("Not Show", StringComparison.OrdinalIgnoreCase))
				.Select(x => x.DisplaySlotName)
				.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				DisplayRuntimeManager.UpdateInfo(
					displaySlot,
					GetSelectedJobName(),
					programNo,
					string.Empty,
					GetSelectedChannelName());
			}
		}

		private void RefreshTaskList(ProjectFlowConfig config)
		{
			listTasks.Items.Clear();

			foreach (string taskName in EnumerateJobContexts(config)
				.SelectMany(x => x.Job.Tasks == null ? new List<TaskConfig>() : x.Job.Tasks)
				.Where(x => x != null && !string.IsNullOrWhiteSpace(x.TaskName))
				.Select(x => x.TaskName)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
			{
				listTasks.Items.Add(taskName);
			}

			if (listTasks.Items.Count > 0)
			{
				listTasks.SelectedIndex = 0;
			}
		}

		private void RefreshProgramsByTask(string taskName)
		{
			listJobs.Items.Clear();

			if (string.IsNullOrWhiteSpace(taskName))
			{
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			foreach (JobContext context in EnumerateJobContexts(config)
				.Where(x => x.Job.Tasks != null &&
					x.Job.Tasks.Any(t => t != null && string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase)))
				.OrderBy(x => ParseProgramNo(x.Job.ProgramNo))
				.ThenBy(x => x.Job.JobName)
				.ThenBy(x => x.ProtocolName)
				.ThenBy(x => x.ChannelName))
			{
				listJobs.Items.Add(new ProgramListItem(context));
			}

			if (listJobs.Items.Count > 0)
			{
				listJobs.SelectedIndex = 0;
			}
		}

		private string GetNextJobName(ProjectFlowConfig config)
		{
			return GetNextJobName(config == null ? null : config.Jobs);
		}

		private string GetNextJobName(List<JobConfig> jobs)
		{
			int index = 1;

			while (true)
			{
				string name = "Job_" + index.ToString("000");

				if (jobs == null ||
					!jobs.Any(j => string.Equals(j.JobName, name, StringComparison.OrdinalIgnoreCase)))
				{
					return name;
				}

				index++;
			}
		}

		private void btnAddJob_Click(object sender, EventArgs e)
		{
			string protocolName = GetSelectedProtocolName();
			string channelName = GetSelectedChannelName();
			if (string.IsNullOrWhiteSpace(protocolName) || string.IsNullOrWhiteSpace(channelName)) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			ChannelFlowConfig channel = FlowConfigStore.GetOrCreateChannel(config, protocolName, channelName);

			string jobName = GetNextJobName(channel.Jobs);

			JobConfig job = new JobConfig();
			job.JobName = jobName;
			job.ProtocolName = protocolName;
			job.ChannelName = channelName;
			job.ProgramNo = GetNextProgramNo(channel.Jobs);
			job.Enabled = true;
			channel.Jobs.Add(job);

			FlowConfigStore.Save(config);

			LoadFlowConfigToUI();
			SelectListItem(listJobs, jobName);
		}

		private void btnDeleteJob_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			if (string.IsNullOrEmpty(jobName)) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			List<JobConfig> jobs = FlowConfigStore.GetJobs(config, GetSelectedProtocolName(), GetSelectedChannelName());
			JobConfig job = jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job != null)
			{
				jobs.Remove(job);
				DeleteJobFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName);
				FlowConfigStore.Save(config);
				LoadFlowConfigToUI();
			}
		}

		#endregion

		#region Task

		private void listTasks_DoubleClick(object sender, EventArgs e)
		{
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
			UpdateStepDetailTitle();
		}

		private void btnAddTask_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			if (string.IsNullOrEmpty(jobName)) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = FlowConfigStore.GetOrCreateJob(config, GetSelectedProtocolName(), GetSelectedChannelName(), jobName);

			string taskName = "Task_New_" + (job.Tasks.Count + 1).ToString("00");
			TaskConfig task = FlowConfigStore.CreateDefaultTask(jobName, taskName, job.Tasks.Count + 1);
			task.CommunicationProtocol = GetSelectedProtocolName();
			task.CommunicationChannel = GetSelectedChannelName();
			job.Tasks.Add(task);

			FlowConfigStore.Save(config);

			LoadFlowConfigToUI();
			SelectListItem(listTasks, taskName);
			RefreshProgramsByTask(taskName);
			SelectListItem(listJobs, jobName);
			RefreshStepLibraryByTask(jobName, taskName);
			RefreshStepFlowGrid(jobName, taskName);
		}

		private void btnDeleteTask_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName)) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = FlowConfigStore.GetOrCreateJob(config, GetSelectedProtocolName(), GetSelectedChannelName(), jobName);
			TaskConfig task = job.Tasks.FirstOrDefault(t => string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase));

			if (task != null)
			{
				job.Tasks.Remove(task);
				ReorderTasks(job);
				DeleteTaskFolder(jobName, taskName);
				FlowConfigStore.Save(config);

				LoadFlowConfigToUI();
				RefreshStepLibraryByTask(jobName, GetSelectedTaskName());
				RefreshStepFlowGrid(jobName, GetSelectedTaskName());
			}
		}

		#endregion

		#region Function Block Library：中间功能块

		private void RefreshStepLibraryByTask(string jobName, string taskName)
		{
			RefreshFunctionBlockLibrary(jobName, taskName);
		}

		private void RefreshFunctionBlockLibrary(string jobName, string taskName)
		{
			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Hardware)
			{
				RefreshHardwareLibraryByProgram(jobName);
				return;
			}

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Signal)
			{
				RefreshSignalLibrary();
				return;
			}

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Database)
			{
				RefreshDatabaseLibrary();
				return;
			}

			RefreshStepLibraryItemsByTask(jobName, taskName);
		}

		private void RefreshStepLibraryItemsByTask(string jobName, string taskName)
		{
			listSteps.Items.Clear();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName)) return;

			TaskConfig task = GetTaskConfig(jobName, taskName);
			if (task == null) return;

			foreach (StepConfig step in task.Steps.OrderBy(s => s.RunOrder))
			{
				listSteps.Items.Add(new StepListItem(
					step.StepName,
					GetStepDisplayText(step),
					IsStepProjectFileMissing(jobName, taskName, step)));
			}

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = 0;
			}
		}

		private void RefreshDatabaseLibrary()
		{
			listSteps.Items.Clear();

			try
			{
				DatabaseConfig config = DatabaseConfigStore.LoadOrCreateDefault();
				string tableName = config == null ? string.Empty : config.TableName;
				FunctionBlockListItem item = new FunctionBlockListItem(
					FLOW_BLOCK_DATABASE,
					FLOW_BLOCK_DATABASE,
					string.IsNullOrWhiteSpace(tableName) ? FLOW_BLOCK_DATABASE : FLOW_BLOCK_DATABASE + " - " + tableName,
					false);
				listSteps.Items.Add(item);
			}
			catch
			{
				listSteps.Items.Add(new FunctionBlockListItem(
					FLOW_BLOCK_DATABASE,
					FLOW_BLOCK_DATABASE,
					FLOW_BLOCK_DATABASE,
					false));
			}

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = 0;
			}
		}

		private void RefreshHardwareLibraryByProgram(string jobName)
		{
			listSteps.Items.Clear();

			if (string.IsNullOrWhiteSpace(jobName))
			{
				return;
			}

			foreach (FunctionBlockListItem item in EnumerateHardwareBlockItems(jobName)
				.OrderBy(x => x.DisplayText, StringComparer.OrdinalIgnoreCase))
			{
				listSteps.Items.Add(item);
			}

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = 0;
			}
		}

		private IEnumerable<FunctionBlockListItem> EnumerateHardwareBlockItems(string jobName)
		{
			HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (string root in GetCandidateHardwareCameraRootFolders(jobName, false))
			{
				if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
				{
					continue;
				}

				foreach (string file in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
				{
					if (!IsHardwareAcquisitionFile(file))
					{
						continue;
					}

					string fullPath = NormalizeFullPath(file);
					if (!seen.Add(fullPath))
					{
						continue;
					}

					string relativePath = MakeRelativePreviewPath(root, file);
					string displayText = relativePath.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');
					if (string.IsNullOrWhiteSpace(displayText))
					{
						displayText = Path.GetFileName(file);
					}

					yield return new FunctionBlockListItem(
						FLOW_BLOCK_HARDWARE,
						displayText,
						displayText,
						false)
					{
						FilePath = file,
						RelativePath = relativePath
					};
				}
			}
		}

		private void RefreshSignalLibrary()
		{
			listSteps.Items.Clear();

			try
			{
				CommunicationConfig communication = CommunicationConfigStore.LoadOrCreateDefault();
				HashSet<string> added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

				if (communication != null && communication.Instances != null)
				{
					foreach (CommunicationInstanceConfig instance in communication.Instances
						.Where(x => x != null && !string.IsNullOrWhiteSpace(x.InstanceName))
						.OrderBy(x => GetCommunicationTypeSortOrder(x.CommunicationType))
						.ThenBy(x => x.InstanceName, StringComparer.OrdinalIgnoreCase))
					{
						AddSignalInstanceItem(instance, added);
					}
				}

			}
			catch
			{
			}

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = 0;
			}
		}

		private void AddSignalInstanceItem(CommunicationInstanceConfig instance, HashSet<string> added)
		{
			if (instance == null)
			{
				return;
			}

			string protocol = CommunicationRuntimeNaming.GetProtocolName(instance.CommunicationType);
			string instanceName = string.IsNullOrWhiteSpace(instance.InstanceName)
				? GetFallbackCommunicationInstanceName(protocol)
				: instance.InstanceName.Trim();

			if (string.IsNullOrWhiteSpace(instanceName))
			{
				return;
			}

			string key = protocol + "|" + instanceName;
			if (added != null && !added.Add(key))
			{
				return;
			}

			FunctionBlockListItem item = new FunctionBlockListItem(
				FLOW_BLOCK_SIGNAL,
				instanceName,
				instanceName,
				false);
			item.Protocol = protocol;
			item.InstanceName = instanceName;
			listSteps.Items.Add(item);
		}

		private void AddLegacySignalInstanceIfNeeded(
			CommunicationConfig communication,
			string protocolName,
			CommunicationType type,
			HashSet<string> added)
		{
			if (communication == null || communication.Instances == null)
			{
				return;
			}

			if (communication.Instances.Any(x => x != null && x.CommunicationType == type))
			{
				return;
			}

			if (!IsProtocolEnabled(communication, protocolName))
			{
				return;
			}

			string instanceName = GetFallbackCommunicationInstanceName(protocolName);
			if (string.IsNullOrWhiteSpace(instanceName))
			{
				return;
			}

			string protocol = CommunicationRuntimeNaming.GetProtocolName(type);
			string key = protocol + "|" + instanceName;
			if (added != null && !added.Add(key))
			{
				return;
			}

			FunctionBlockListItem item = new FunctionBlockListItem(
				FLOW_BLOCK_SIGNAL,
				instanceName,
				instanceName,
				false);
			item.Protocol = protocol;
			item.InstanceName = instanceName;
			listSteps.Items.Add(item);
		}

		private int GetCommunicationTypeSortOrder(CommunicationType type)
		{
			if (type == CommunicationType.TcpIp)
			{
				return 0;
			}

			if (type == CommunicationType.Profinet)
			{
				return 1;
			}

			if (type == CommunicationType.S7)
			{
				return 2;
			}

			return 99;
		}

		private bool IsCommunicationInstanceEnabled(CommunicationInstanceConfig instance)
		{
			if (instance == null || !instance.Enabled)
			{
				return false;
			}

			if (instance.CommunicationType == CommunicationType.TcpIp)
			{
				return instance.TcpIp == null || instance.TcpIp.Enabled;
			}

			if (instance.CommunicationType == CommunicationType.Profinet)
			{
				return instance.Profinet == null || instance.Profinet.Enabled;
			}

			if (instance.CommunicationType == CommunicationType.S7)
			{
				return instance.S7 == null || instance.S7.Enabled;
			}

			return instance.Enabled;
		}

		private void AddSignalModuleItem(CommunicationConfig communication, string protocolName, string displayText)
		{
			if (!IsProtocolEnabled(communication, protocolName))
			{
				return;
			}

			string instanceName = CommunicationRuntimeNaming.GetDefaultInstanceName(protocolName, communication);
			if (string.IsNullOrWhiteSpace(instanceName))
			{
				instanceName = GetFallbackCommunicationInstanceName(protocolName);
			}

			FunctionBlockListItem item = new FunctionBlockListItem(
				FLOW_BLOCK_SIGNAL,
				displayText,
				displayText,
				false);
			item.Protocol = FlowConfigStore.NormalizeProtocolName(protocolName);
			item.InstanceName = instanceName;
			listSteps.Items.Add(item);
		}

		private bool IsProtocolEnabled(CommunicationConfig communication, string protocolName)
		{
			if (communication == null)
			{
				return false;
			}

			string normalized = FlowConfigStore.NormalizeProtocolName(protocolName);
			if (normalized.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				return (communication.TcpIp != null && communication.TcpIp.Enabled) ||
					HasEnabledCommunicationInstance(communication, CommunicationType.TcpIp);
			}

			if (normalized.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return (communication.Profinet != null && communication.Profinet.Enabled) ||
					HasEnabledCommunicationInstance(communication, CommunicationType.Profinet);
			}

			if (normalized.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return (communication.S7 != null && communication.S7.Enabled) ||
					HasEnabledCommunicationInstance(communication, CommunicationType.S7);
			}

			return false;
		}

		private string GetFallbackCommunicationInstanceName(string protocolName)
		{
			string normalized = FlowConfigStore.NormalizeProtocolName(protocolName);
			if (normalized.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				return "TCPIP_01";
			}

			if (normalized.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return "Profinet_01";
			}

			if (normalized.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return "S7_01";
			}

			return string.Empty;
		}

		private bool HasEnabledCommunicationInstance(CommunicationConfig communication, CommunicationType type)
		{
			if (communication == null || communication.Instances == null)
			{
				return false;
			}

			foreach (CommunicationInstanceConfig instance in communication.Instances)
			{
				if (instance == null || instance.CommunicationType != type || !instance.Enabled)
				{
					continue;
				}

				if (type == CommunicationType.TcpIp)
				{
					return instance.TcpIp != null && instance.TcpIp.Enabled;
				}

				if (type == CommunicationType.Profinet)
				{
					return instance.Profinet != null && instance.Profinet.Enabled;
				}

				if (type == CommunicationType.S7)
				{
					return instance.S7 != null && instance.S7.Enabled;
				}
			}

			return false;
		}

		private void AddHardwareFileToLibrary(string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				MessageBox.Show("Please select Program first.", "Add Hardware", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select Acquisition File";
				dialog.Filter = "Acquisition Files (*.vpp;*.xml)|*.vpp;*.xml|VPP Files (*.vpp)|*.vpp|XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
				dialog.Multiselect = false;
				dialog.CheckFileExists = true;
				dialog.CheckPathExists = true;

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				HardwareConfigStore.SetCurrentJobName(jobName);
				HardwareProjectConfig hardwareConfig = HardwareConfigStore.LoadOrCreateDefault();
				CameraDeviceConfig camera = GetOrCreateHardwareCamera(hardwareConfig, dialog.FileName);

				string cameraFolder = HardwareConfigStore.GetCameraFolder(camera.CameraName, jobName);
				if (string.IsNullOrWhiteSpace(cameraFolder))
				{
					MessageBox.Show("Hardware camera folder was not found.", "Add Hardware", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				Directory.CreateDirectory(cameraFolder);
				string targetPath = MakeUniqueHardwareFilePath(cameraFolder, Path.GetFileName(dialog.FileName));

				try
				{
					File.Copy(dialog.FileName, targetPath, false);
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						"Failed to copy acquisition file.\r\n\r\n" + ex.Message,
						"Add Hardware",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
					return;
				}

				if (string.Equals(Path.GetExtension(targetPath), ".vpp", StringComparison.OrdinalIgnoreCase))
				{
					camera.AcquisitionMode = CameraAcquisitionMode.VPro;
					if (camera.VisionPro == null)
					{
						camera.VisionPro = new VisionProAcqConfig();
					}
					camera.VisionPro.AcqVppPath = targetPath;
					camera.VisionPro.ToolName = Path.GetFileNameWithoutExtension(targetPath);
				}
				else
				{
					camera.AcquisitionMode = CameraAcquisitionMode.SDK;
					if (camera.Sdk == null)
					{
						camera.Sdk = new SdkCameraConfig();
					}
					camera.Sdk.ConfigPath = targetPath;
					camera.Sdk.ToolName = Path.GetFileNameWithoutExtension(targetPath);
				}

				HardwareConfigStore.Save(hardwareConfig);
				HardwareConfigStore.SaveImageSourceList(hardwareConfig);
			}

			RefreshHardwareLibraryByProgram(jobName);
		}

		private CameraDeviceConfig GetOrCreateHardwareCamera(HardwareProjectConfig hardwareConfig, string sourceFilePath)
		{
			if (hardwareConfig.Cameras == null)
			{
				hardwareConfig.Cameras = new List<CameraDeviceConfig>();
			}

			CameraDeviceConfig camera = hardwareConfig.Cameras.FirstOrDefault(x => x != null && x.Enable);
			if (camera != null)
			{
				return camera;
			}

			camera = hardwareConfig.Cameras.FirstOrDefault(x => x != null);
			if (camera != null)
			{
				return camera;
			}

			camera = new CameraDeviceConfig();
			camera.CameraName = "Cam1";
			camera.Enable = true;
			camera.AcquisitionMode = string.Equals(Path.GetExtension(sourceFilePath), ".vpp", StringComparison.OrdinalIgnoreCase)
				? CameraAcquisitionMode.VPro
				: CameraAcquisitionMode.SDK;
			hardwareConfig.Cameras.Add(camera);
			return camera;
		}

		private string MakeUniqueHardwareFilePath(string folder, string fileName)
		{
			string safeFileName = string.IsNullOrWhiteSpace(fileName) ? "Acquisition.vpp" : MakeSafeName(fileName);
			string path = Path.Combine(folder, safeFileName);
			if (!File.Exists(path))
			{
				return path;
			}

			string name = Path.GetFileNameWithoutExtension(safeFileName);
			string extension = Path.GetExtension(safeFileName);
			int index = 1;
			while (true)
			{
				string candidate = Path.Combine(folder, name + "_" + index.ToString("00") + extension);
				if (!File.Exists(candidate))
				{
					return candidate;
				}

				index++;
			}
		}

		private void DeleteSelectedHardwareFile(string jobName)
		{
			FunctionBlockListItem item = GetSelectedFunctionBlockItem();
			if (item == null || string.IsNullOrWhiteSpace(item.FilePath) || !File.Exists(item.FilePath))
			{
				return;
			}

			if (!IsPathUnderAnyRoot(item.FilePath, GetCandidateHardwareCameraRootFolders(jobName, false)))
			{
				MessageBox.Show("Selected file is outside current Hardware folder.", "Delete Hardware", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			bool confirm = ThemedDialog.Confirm(
				this,
				"删除取像文件",
				"删除选中的取像文件？",
				item.DisplayText,
				"删除后，该取像文件会从当前程序的 Hardware 文件夹中移除。",
				"删除",
				"取消",
				ThemedDialogIconKind.Delete,
				true);

			if (!confirm)
			{
				return;
			}

			try
			{
				File.Delete(item.FilePath);
				HardwareConfigStore.SetCurrentJobName(jobName);
				HardwareProjectConfig hardwareConfig = HardwareConfigStore.LoadOrCreateDefault();
				ClearDeletedHardwarePath(hardwareConfig, item.FilePath);
				HardwareConfigStore.Save(hardwareConfig);
				HardwareConfigStore.SaveImageSourceList(hardwareConfig);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Failed to delete acquisition file.\r\n\r\n" + ex.Message,
					"Delete Hardware",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}

			RefreshHardwareLibraryByProgram(jobName);
		}

		private void ClearDeletedHardwarePath(HardwareProjectConfig hardwareConfig, string deletedFilePath)
		{
			if (hardwareConfig == null || hardwareConfig.Cameras == null || string.IsNullOrWhiteSpace(deletedFilePath))
			{
				return;
			}

			string deleted = NormalizeFullPath(deletedFilePath);
			foreach (CameraDeviceConfig camera in hardwareConfig.Cameras)
			{
				if (camera == null)
				{
					continue;
				}

				if (camera.VisionPro != null && string.Equals(NormalizeFullPath(camera.VisionPro.AcqVppPath), deleted, StringComparison.OrdinalIgnoreCase))
				{
					camera.VisionPro.AcqVppPath = string.Empty;
				}

				if (camera.Sdk != null && string.Equals(NormalizeFullPath(camera.Sdk.ConfigPath), deleted, StringComparison.OrdinalIgnoreCase))
				{
					camera.Sdk.ConfigPath = string.Empty;
				}
			}
		}

		private List<string> GetCandidateHardwareCameraRootFolders(string jobName, bool createCurrent)
		{
			List<string> roots = new List<string>();
			if (string.IsNullOrWhiteSpace(jobName))
			{
				return roots;
			}

			string safeJob = HardwareConfigStore.NormalizeFileName(jobName, string.Empty);
			if (createCurrent)
			{
				string current = HardwareConfigStore.GetCameraRootFolder(jobName);
				if (!string.IsNullOrWhiteSpace(current))
				{
					roots.Add(current);
				}
			}
			else
			{
				string current = Path.Combine(HardwareConfigStore.JobRootContainer, safeJob, "Hardware", "Camera");
				if (Directory.Exists(current))
				{
					roots.Add(current);
				}
			}

			string oldRoot = Path.Combine(HardwareConfigStore.JobRootContainer, safeJob, "Camera");
			if (Directory.Exists(oldRoot) && !roots.Any(x => string.Equals(NormalizeFullPath(x), NormalizeFullPath(oldRoot), StringComparison.OrdinalIgnoreCase)))
			{
				roots.Add(oldRoot);
			}

			return roots;
		}

		private bool IsHardwareAcquisitionFile(string file)
		{
			if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
			{
				return false;
			}

			string extension = Path.GetExtension(file);
			if (!string.Equals(extension, ".vpp", StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			string name = Path.GetFileNameWithoutExtension(file);
			return !string.Equals(name, "HardwareConfig", StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(name, "ImageSources", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsPathUnderAnyRoot(string filePath, List<string> roots)
		{
			if (string.IsNullOrWhiteSpace(filePath) || roots == null)
			{
				return false;
			}

			string fullPath = NormalizeFullPath(filePath);
			foreach (string root in roots)
			{
				if (string.IsNullOrWhiteSpace(root))
				{
					continue;
				}

				string fullRoot = NormalizeFullPath(root);
				if (fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(fullPath, fullRoot, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}


		private void listSteps_DoubleClick(object sender, EventArgs e)
		{
			// 双击 Step 只用于选中，不自动加入右侧流程。
			UpdateStepDetailTitle();
		}

		// 中间 Step 库上方 “+”：从本地选择 VPP 或 Script，只添加到 Step 库，不加入右侧执行流程，不立即复制到 Project。
		private void btnAddStepItem_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Hardware)
			{
				AddHardwareFileToLibrary(jobName);
				return;
			}

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Signal)
			{
				RefreshSignalLibrary();
				return;
			}

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName))
			{
				MessageBox.Show("Please select Job and Task first.", "Add Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			using (OpenFileDialog dialog = CreateStepFileDialog(false))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				AddStepToLibraryByLocalFile(jobName, taskName, dialog.FileName);
			}
		}

		// 中间 Step 库上方第二个按钮：在当前 Task 下新建 VPP / Script / Hdev Step。
		private void btnBatchAddStepItem_Click(object sender, EventArgs e)
		{
			if (_functionBlockLibraryMode != FunctionBlockLibraryMode.Step)
			{
				return;
			}

			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName))
			{
				MessageBox.Show("Please select Program and Task first.", "Add Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			TaskConfig currentTask = GetTaskConfig(jobName, taskName);
			if (currentTask == null)
			{
				MessageBox.Show("Task config was not found.", "Add Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			using (NewStepAssetDialog dialog = new NewStepAssetDialog(
				"新增 Step",
				delegate (StepType stepType)
				{
					return GetNextStepName(currentTask, stepType);
				},
				delegate (StepType stepType, string inputName)
				{
					return ValidateNewStepAssetName(currentTask, jobName, taskName, stepType, inputName);
				}))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				AddNewStepAsset(jobName, taskName, dialog.SelectedStepType, dialog.StepName);
			}
		}

		private string GetNextProgramNo(List<JobConfig> jobs)
		{
			int index = 1;
			while (jobs != null && jobs.Any(j => string.Equals(j.ProgramNo, index.ToString(), StringComparison.OrdinalIgnoreCase)))
			{
				index++;
			}

			return index.ToString();
		}

		private OpenFileDialog CreateStepFileDialog(bool multiSelect)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			bool hdevEnabled = IsHdevModuleEnabled();
			dialog.Title = multiSelect ? "Select Vision Step Files" : "Select Vision Step File";
			dialog.Filter = hdevEnabled
				? "Vision Step Files (*.vpp;*.hdev;*.cs;*.csx;*.txt)|*.vpp;*.hdev;*.cs;*.csx;*.txt|VPP Files (*.vpp)|*.vpp|Hdev Files (*.hdev)|*.hdev|Script Files (*.cs;*.csx;*.txt)|*.cs;*.csx;*.txt|All Files (*.*)|*.*"
				: "Vision Step Files (*.vpp;*.cs;*.csx;*.txt)|*.vpp;*.cs;*.csx;*.txt|VPP Files (*.vpp)|*.vpp|Script Files (*.cs;*.csx;*.txt)|*.cs;*.csx;*.txt|All Files (*.*)|*.*";
			dialog.Multiselect = multiSelect;
			dialog.CheckFileExists = true;
			dialog.CheckPathExists = true;
			return dialog;
		}

		private void AddStepToLibraryByLocalFile(string jobName, string taskName, string sourceFilePath)
		{
			if (!File.Exists(sourceFilePath))
			{
				MessageBox.Show(
					"Selected file does not exist.\r\n\r\nFile: " + sourceFilePath,
					"Add Step Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			StepType stepType = FlowConfigStore.GetStepTypeByFilePath(sourceFilePath);

			if (stepType == StepType.Unknown)
			{
				MessageBox.Show(
					"Unsupported file type.\r\n\r\nOnly .vpp, .hdev, .cs, .csx, .txt are supported now.\r\n\r\nFile: " + sourceFilePath,
					"Add Step Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			if (stepType == StepType.Halcon && !IsHdevModuleEnabled())
			{
				MessageBox.Show(
					"Hdev module is not enabled.\r\n\r\nPlease enable Hdev in Algorithm Module first.",
					"Add Step Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);

			if (task == null)
			{
				MessageBox.Show(
					"Task config was not found.\r\n\r\nJob: " + jobName + "\r\nTask: " + taskName,
					"Add Step Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			// StepName 直接使用文件名，不自动追加 _01、_02
			string baseStepName = Path.GetFileNameWithoutExtension(sourceFilePath);
			string stepName = MakeSafeName(baseStepName);

			if (string.IsNullOrWhiteSpace(stepName))
			{
				MessageBox.Show(
					"Step name is empty.\r\n\r\nFile: " + sourceFilePath,
					"Add Step Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			// 关键：当前 Task 下 StepName 重名，直接报错，不自动改名
			bool isStepNameExists = task.Steps.Any(s =>
				string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase));

			if (isStepNameExists)
			{
				MessageBox.Show(
					"Add step failed.\r\n\r\nA step with the same name already exists in the current task.\r\n\r\nStep: " + stepName,
					"Add Step Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			StepConfig step = FlowConfigStore.CreateDefaultStep(
				jobName,
				taskName,
				stepName,
				task.Steps.Count + 1,
				stepType);

			// 添加 Step 时只记录原始路径，不立即复制到 Project。
			// 只有 Step 加入右侧执行流程并点击保存时，才复制到 Project/Task/<Task>/<Program>/VPP 或 Script。
			step.SourceFilePath = sourceFilePath;
			step.ProjectFilePath = string.Empty;
			step.Remark = string.Empty;

			if (stepType == StepType.Vpp)
			{
				step.VppFiles.Clear();
				step.InputImageKey = "Cam1.Raw";
				step.OutputImageKey = step.StepName + ".OutputImage";
			}
			else if (stepType == StepType.Script)
			{
				step.ScriptFiles.Clear();
				step.InputImageKey = string.Empty;
				step.OutputImageKey = string.Empty;
			}
			else if (stepType == StepType.Halcon)
			{
				step.InputImageKey = string.Empty;
				step.OutputImageKey = string.Empty;
			}

			task.Steps.Add(step);
			ReorderStepLibrary(task);

			FlowConfigStore.Save(config);

			RefreshStepLibraryByTask(jobName, taskName);
			SelectListItem(listSteps, stepName);

			UpdateStepDetailTitle();
		}

		private string GetNextScriptStepName(string jobName, string taskName)
		{
			TaskConfig task = GetTaskConfig(jobName, taskName);
			return GetNextStepName(task, StepType.Script);
		}

		private string GetNextStepName(TaskConfig task, StepType stepType)
		{
			string prefix = GetNewStepNamePrefix(stepType);
			int index = 1;

			while (true)
			{
				string name = prefix + index.ToString("00");

				bool exists = task != null && task.Steps.Any(s =>
					string.Equals(s.StepName, name, StringComparison.OrdinalIgnoreCase));

				if (!exists)
				{
					return name;
				}

				index++;
			}
		}

		private string GetNewStepNamePrefix(StepType stepType)
		{
			if (stepType == StepType.Vpp)
			{
				return "VPP_New_";
			}

			if (stepType == StepType.Halcon)
			{
				return "Hdev_New_";
			}

			return "Script_New_";
		}

		private string NormalizeNewStepInputName(string inputName)
		{
			if (string.IsNullOrWhiteSpace(inputName))
			{
				return string.Empty;
			}

			return MakeSafeName(Path.GetFileNameWithoutExtension(inputName.Trim()));
		}

		private string ValidateNewStepAssetName(TaskConfig task, string jobName, string taskName, StepType stepType, string inputName)
		{
			string stepName = NormalizeNewStepInputName(inputName);

			if (string.IsNullOrWhiteSpace(stepName))
			{
				return "请输入名称。";
			}

			if (task != null && task.Steps != null &&
				task.Steps.Any(s => string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase)))
			{
				return "名称重名，请换一个名称。";
			}

			string filePath = GetNewStepAssetFilePath(jobName, taskName, stepType, stepName);
			if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
			{
				return "名称重名，目标文件已存在。";
			}

			if (stepType == StepType.Script)
			{
				string configPath = CSharpScriptStepStore.GetConfigPath(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName, stepName);
				if (File.Exists(configPath))
				{
					return "名称重名，脚本配置已存在。";
				}
			}

			return string.Empty;
		}

		private string GetNewStepAssetFilePath(string jobName, string taskName, StepType stepType, string stepName)
		{
			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName);
			return Path.Combine(taskFolder, GetNewStepAssetRelativePath(stepType, stepName));
		}

		private string GetNewStepAssetRelativePath(StepType stepType, string stepName)
		{
			return Path.Combine(GetStepProjectSubFolderName(stepType), stepName + GetNewStepFileExtension(stepType));
		}

		private string GetNewStepFileExtension(StepType stepType)
		{
			if (stepType == StepType.Vpp)
			{
				return ".vpp";
			}

			if (stepType == StepType.Halcon)
			{
				return ".hdev";
			}

			return ".csx";
		}

		private void AddNewStepAsset(string jobName, string taskName, StepType stepType, string inputName)
		{
			string stepName = NormalizeNewStepInputName(inputName);
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);
			string validationError = ValidateNewStepAssetName(task, jobName, taskName, stepType, stepName);

			if (!string.IsNullOrWhiteSpace(validationError))
			{
				MessageBox.Show(validationError, "Add Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (stepType == StepType.Script)
			{
				AddNewScriptStep(jobName, taskName, stepName);
				return;
			}

			if (task == null)
			{
				MessageBox.Show("Task config was not found.", "Add Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			FlowConfigStore.PathManager.EnsureStepFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName, stepName);

			string relativeFilePath = GetNewStepAssetRelativePath(stepType, stepName);
			string filePath = GetNewStepAssetFilePath(jobName, taskName, stepType, stepName);
			string folder = Path.GetDirectoryName(filePath);

			if (!string.IsNullOrWhiteSpace(folder))
			{
				Directory.CreateDirectory(folder);
			}

			string createError;
			if (stepType == StepType.Vpp)
			{
				if (!TryCreateBlankVppFile(filePath, out createError))
				{
					MessageBox.Show(createError, "Add VPP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}
			else if (stepType == StepType.Halcon)
			{
				File.WriteAllText(filePath, GetDefaultHdevTemplate(stepName), new UTF8Encoding(false));
			}
			else
			{
				MessageBox.Show("Unsupported step type.", "Add Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			StepConfig step = FlowConfigStore.CreateDefaultStep(
				jobName,
				taskName,
				stepName,
				task.Steps.Count + 1,
				stepType);

			step.SourceFilePath = filePath;
			step.Remark = string.Empty;
			ApplyStepProjectFilePath(jobName, taskName, step, relativeFilePath);

			if (stepType == StepType.Halcon)
			{
				step.InputImageKey = string.Empty;
				step.OutputImageKey = string.Empty;
			}

			task.Steps.Add(step);
			ReorderStepLibrary(task);

			FlowConfigStore.Save(config);

			RefreshStepLibraryByTask(jobName, taskName);
			SelectListItem(listSteps, stepName);
			UpdateStepDetailTitle();
		}

		private bool TryCreateBlankVppFile(string filePath, out string error)
		{
			error = string.Empty;

			try
			{
				CogToolBlock toolBlock = new CogToolBlock();
				CogSerializer.SaveObjectToFile(toolBlock, filePath);
				return true;
			}
			catch (Exception ex)
			{
				error = "Create VPP failed.\r\n\r\n" + ex.Message;
				return false;
			}
		}

		private string GetDefaultHdevTemplate(string stepName)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("* " + stepName);
			sb.AppendLine("* Auto-created by Aron_V3.");
			sb.AppendLine();
			return sb.ToString();
		}

		private void AddNewScriptStep(string jobName, string taskName, string inputName)
		{
			if (string.IsNullOrWhiteSpace(inputName))
			{
				MessageBox.Show("Script name cannot be empty.", "Add Script Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string stepName = MakeSafeName(Path.GetFileNameWithoutExtension(inputName.Trim()));

			if (string.IsNullOrWhiteSpace(stepName))
			{
				MessageBox.Show("Script name cannot be empty.", "Add Script Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);

			if (task == null)
			{
				MessageBox.Show("Task config was not found.", "Add Script Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (task.Steps.Any(s => string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase)))
			{
				MessageBox.Show(
					"Add script failed.\r\n\r\nA step with the same name already exists in the current task.\r\n\r\nStep: " + stepName,
					"Add Script Step",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			FlowConfigStore.PathManager.EnsureStepFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName, stepName);

			string scriptPath = CSharpScriptStepStore.GetScriptPath(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName, stepName);
			string configPath = CSharpScriptStepStore.GetConfigPath(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName, stepName);

			if (File.Exists(scriptPath) || File.Exists(configPath))
			{
				MessageBox.Show(
					"Add script failed.\r\n\r\nA script file with the same name already exists in the current task folder.\r\n\r\nScript: " + scriptPath,
					"Add Script Step",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			string scriptFolder = Path.GetDirectoryName(scriptPath);
			if (!string.IsNullOrWhiteSpace(scriptFolder))
			{
				Directory.CreateDirectory(scriptFolder);
			}

			File.WriteAllText(scriptPath, CSharpScriptStepStore.GetDefaultScriptTemplate(), Encoding.UTF8);

			CSharpScriptStepConfig scriptConfig = CSharpScriptStepStore.CreateDefaultConfig();
			scriptConfig.StepName = stepName;
			scriptConfig.ScriptFileName = Path.GetFileName(scriptPath);
			scriptConfig.ScriptFilePath = scriptPath;
			scriptConfig.Enable = true;
			CSharpScriptStepStore.Save(configPath, scriptConfig);

			StepConfig step = FlowConfigStore.CreateDefaultStep(
				jobName,
				taskName,
				stepName,
				task.Steps.Count + 1,
				StepType.Script);

			step.SourceFilePath = scriptPath;
			step.Remark = string.Empty;
			step.InputImageKey = string.Empty;
			step.OutputImageKey = string.Empty;
			ApplyStepProjectFilePath(jobName, taskName, step, Path.Combine("Script", Path.GetFileName(scriptPath)));

			task.Steps.Add(step);
			ReorderStepLibrary(task);

			FlowConfigStore.Save(config);

			RefreshStepLibraryByTask(jobName, taskName);
			SelectListItem(listSteps, stepName);
			UpdateStepDetailTitle();
		}


		private void btnDeleteStepItem_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();
			string stepName = GetSelectedStepName();

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Hardware)
			{
				DeleteSelectedHardwareFile(jobName);
				return;
			}

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Signal)
			{
				return;
			}

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName) || string.IsNullOrEmpty(stepName)) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);

			if (task == null) return;

			StepConfig step = task.Steps.FirstOrDefault(s => string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase));

			if (step != null)
			{
				// 1. 先删除 Project 中该 Step 对应的文件
				DeleteStepProjectFile(jobName, taskName, step);

				// 2. 再删除 XML 配置
				task.Steps.Remove(step);

				// 3. 删除右侧执行流程中引用该 Step 的项
				task.StepFlow.RemoveAll(x =>
					x != null &&
					x.IsStepBlock &&
					string.Equals(x.StepName, stepName, StringComparison.OrdinalIgnoreCase));

				ReorderStepLibrary(task);

				// 注意：StepFlow 的 RunOrder 允许重复，所以不要重排 StepFlow。
				FlowConfigStore.Save(config);

				RefreshStepLibraryByTask(jobName, taskName);
				RefreshStepFlowGrid(jobName, taskName);
			}
		}


		private void btnRefreshStepItem_Click(object sender, EventArgs e)
		{
			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Step)
			{
				ValidateCurrentTaskStepFiles();
			}

			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
		}

		private void btnOpenStepFolder_Click(object sender, EventArgs e)
		{
			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Hardware)
			{
				OpenCurrentHardwareFolder();
				return;
			}

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Database)
			{
				OpenDatabaseFolder();
				return;
			}

			OpenCurrentTaskFolder();
		}

		private void OpenCurrentTaskFolder()
		{
			string protocolName = GetSelectedProtocolName();
			string channelName = GetSelectedChannelName();
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrWhiteSpace(protocolName) ||
				string.IsNullOrWhiteSpace(channelName) ||
				string.IsNullOrWhiteSpace(jobName) ||
				string.IsNullOrWhiteSpace(taskName))
			{
				MessageBox.Show("Please select Task and Program first.", "Open Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(protocolName, channelName, jobName, taskName);

			try
			{
				Directory.CreateDirectory(taskFolder);

				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.FileName = "explorer.exe";
				startInfo.Arguments = "\"" + taskFolder + "\"";
				startInfo.UseShellExecute = true;
				Process.Start(startInfo);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to open task folder.\r\n\r\n" + ex.Message, "Open Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void OpenCurrentHardwareFolder()
		{
			string jobName = GetSelectedJobName();
			if (string.IsNullOrWhiteSpace(jobName))
			{
				MessageBox.Show("Please select Program first.", "Open Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			string folder = HardwareConfigStore.GetCameraRootFolder(jobName);

			try
			{
				Directory.CreateDirectory(folder);

				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.FileName = "explorer.exe";
				startInfo.Arguments = "\"" + folder + "\"";
				startInfo.UseShellExecute = true;
				Process.Start(startInfo);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to open hardware folder.\r\n\r\n" + ex.Message, "Open Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void OpenDatabaseFolder()
		{
			try
			{
				DatabaseConfig config = DatabaseConfigStore.LoadOrCreateDefault();
				string folder = DatabaseLocalRecordStore.GetStorageFolder(config);
				Directory.CreateDirectory(folder);

				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.FileName = "explorer.exe";
				startInfo.Arguments = "\"" + folder + "\"";
				startInfo.UseShellExecute = true;
				Process.Start(startInfo);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to open database folder.\r\n\r\n" + ex.Message, "Open Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void ValidateCurrentTaskStepFiles()
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName))
			{
				MessageBox.Show("Please select Job and Task first.", "Validate Step Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			TaskConfig task = GetTaskConfig(jobName, taskName);
			if (task == null)
			{
				return;
			}

			FlowConfigStore.PathManager.EnsureStepFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName, string.Empty);

			HashSet<string> expectedFiles = BuildExpectedStepFiles(jobName, taskName, task);
			List<string> actualFiles = GetTaskStepFiles(jobName, taskName);
			List<string> extraFiles = actualFiles
				.Where(file => !expectedFiles.Contains(NormalizeFullPath(file)))
				.OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
				.ToList();

			int missingCount = task.Steps.Count(step => IsStepProjectFileMissing(jobName, taskName, step));
			int deletedCount = 0;

			if (extraFiles.Count > 0)
			{
				string message = "发现当前 Task 目录下有 " + extraFiles.Count + " 个未在 step 表内引用的文件，是否删除？";
				message += "\r\n\r\n" + BuildFilePreview(jobName, taskName, extraFiles);

				DialogResult result = MessageBox.Show(
					message,
					"Validate Step Files",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					foreach (string file in extraFiles)
					{
						try
						{
							if (File.Exists(file))
							{
								File.Delete(file);
								deletedCount++;
							}
						}
						catch (Exception ex)
						{
							MessageBox.Show(
								"Failed to delete file.\r\n\r\nFile: " + file + "\r\n\r\n" + ex.Message,
								"Validate Step Files",
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning);
						}
					}
				}
			}

			MessageBox.Show(
				"Step 文件校验完成。\r\n\r\n缺失 Step: " + missingCount +
				"\r\n多余文件: " + extraFiles.Count +
				"\r\n已删除文件: " + deletedCount,
				"Validate Step Files",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		private HashSet<string> BuildExpectedStepFiles(string jobName, string taskName, TaskConfig task)
		{
			HashSet<string> expectedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if (task == null || task.Steps == null)
			{
				return expectedFiles;
			}

			foreach (StepConfig step in task.Steps)
			{
				string projectFile = GetAbsoluteProjectStepFilePath(jobName, taskName, step);

				if (!string.IsNullOrWhiteSpace(projectFile))
				{
					expectedFiles.Add(NormalizeFullPath(projectFile));

					if (step.StepType == StepType.Script)
					{
						string configFile = Path.Combine(
							Path.GetDirectoryName(projectFile) ?? string.Empty,
							Path.GetFileNameWithoutExtension(projectFile) + ".script.xml");

						expectedFiles.Add(NormalizeFullPath(configFile));
					}
				}
				else if (!string.IsNullOrWhiteSpace(step.SourceFilePath))
				{
					expectedFiles.Add(NormalizeFullPath(step.SourceFilePath));
				}
			}

			return expectedFiles;
		}

		private List<string> GetTaskStepFiles(string jobName, string taskName)
		{
			List<string> files = new List<string>();
			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName);

			if (!Directory.Exists(taskFolder))
			{
				return files;
			}

			string[] subFolders = new[] { "VPP", "Script", "Scripts", "Hdev" };
			HashSet<string> extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				".vpp",
				".hdev",
				".cs",
				".csx",
				".txt",
				".script.xml"
			};

			foreach (string subFolder in subFolders)
			{
				string folder = Path.Combine(taskFolder, subFolder);

				if (!Directory.Exists(folder))
				{
					continue;
				}

				foreach (string file in Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly))
				{
					string fileName = Path.GetFileName(file);
					string extension = fileName != null && fileName.EndsWith(".script.xml", StringComparison.OrdinalIgnoreCase)
						? ".script.xml"
						: Path.GetExtension(file);

					if (extensions.Contains(extension))
					{
						files.Add(file);
					}
				}
			}

			return files;
		}

		private bool IsStepProjectFileMissing(string jobName, string taskName, StepConfig step)
		{
			if (step == null)
			{
				return false;
			}

			string projectFile = GetAbsoluteProjectStepFilePath(jobName, taskName, step);

			if (!string.IsNullOrWhiteSpace(projectFile))
			{
				return !File.Exists(projectFile);
			}

			if (!string.IsNullOrWhiteSpace(step.SourceFilePath))
			{
				return !File.Exists(step.SourceFilePath);
			}

			return true;
		}

		private string BuildFilePreview(string jobName, string taskName, List<string> files)
		{
			if (files == null || files.Count == 0)
			{
				return string.Empty;
			}

			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName);
			List<string> preview = files
				.Take(12)
				.Select(file => MakeRelativePreviewPath(taskFolder, file))
				.ToList();

			if (files.Count > preview.Count)
			{
				preview.Add("...");
			}

			return string.Join("\r\n", preview);
		}

		private string MakeRelativePreviewPath(string rootFolder, string file)
		{
			try
			{
				string root = Path.GetFullPath(rootFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string full = Path.GetFullPath(file);

				if (full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
				{
					return full.Substring(root.Length + 1);
				}
			}
			catch
			{
			}

			return file;
		}

		private string NormalizeFullPath(string file)
		{
			if (string.IsNullOrWhiteSpace(file))
			{
				return string.Empty;
			}

			try
			{
				return Path.GetFullPath(file).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			}
			catch
			{
				return file;
			}
		}

		#endregion

		#region Step Flow：右侧当前 task 执行流程

		// 下方 “新增算子”：把中间选中的 Step 添加到右侧执行流程。
		private void btnAddStep_Click(object sender, EventArgs e)
		{
			AddSelectedStepToFlow();
		}

		private void AddSelectedStepToFlow()
		{
			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Hardware)
			{
				AddSelectedHardwareBlockToFlow();
				return;
			}

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Signal)
			{
				AddSelectedSignalBlockToFlow();
				return;
			}

			if (_functionBlockLibraryMode == FunctionBlockLibraryMode.Database)
			{
				AddSelectedDatabaseBlockToFlow();
				return;
			}

			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();
			string stepName = GetSelectedStepName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName) || string.IsNullOrEmpty(stepName))
			{
				MessageBox.Show("Please select Job, Task and Step first.", "Add Operator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);

			if (task == null) return;

			StepConfig step = task.Steps.FirstOrDefault(s => string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase));
			if (step == null) return;

			StepFlowItem item = new StepFlowItem();
			item.BlockType = FLOW_BLOCK_STEP;
			item.BlockName = step.StepName;
			item.StepName = step.StepName;

			// Script 不需要图像源；VPP/Hdev/VM 等视觉检测算子才使用图像源。
			if (step.StepType == StepType.Script)
			{
				item.InputImageKey = string.Empty;
			}
			else if (!string.IsNullOrEmpty(task.ImageSourceKey) &&
				!task.ImageSourceKey.Equals("Not Use", StringComparison.OrdinalIgnoreCase) &&
				!task.ImageSourceKey.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				item.InputImageKey = task.ImageSourceKey;
			}
			else
			{
				item.InputImageKey = string.Empty;
			}

			// 新增时默认放到当前表格最后一组后面。
			// 你可以在右侧表格里手动改成相同 RunOrder，例如 1、1、2，实现同组异步并行。
			item.RunOrder = GetNextDefaultRunOrderFromGrid(task);
			item.Enabled = true;
			item.EnableCommunicationOutput = false;
			item.Remark = string.Empty;
			if (step.StepType == StepType.Script)
			{
				item.DisplayOutputKey = DISPLAY_OUTPUT_NOT_USE;
				item.DisplaySlotName = "Not Show";
				item.DisplayResultKey = DISPLAY_OUTPUT_NOT_USE;
				item.DisplayMode = "Fit";
				item.ScriptInputStepKeys = string.Empty;
			}
			else
			{
				item.DisplayOutputKey = GetFixedDisplayOutputKey(step.StepType);
				item.DisplaySlotName = "Not Show";
				item.DisplayResultKey = GetDefaultDisplayResultKey(step);
				item.DisplayMode = "Fit";
			}

			AppendFlowItemToGrid(task, item);
			SelectFlowGridRowByFlowItemId(item.FlowItemId);
		}

		private void AddSelectedDatabaseBlockToFlow()
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName))
			{
				MessageBox.Show("Please select Program and Task first.", "Add Database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);
			if (task == null)
			{
				return;
			}

			StepFlowItem item = CreateNonStepFlowItem(FLOW_BLOCK_DATABASE, FLOW_BLOCK_DATABASE);
			item.RunOrder = GetNextDefaultRunOrderFromGrid(task);
			item.Remark = "Database";
			EnsureDatabaseInputBindings(item);

			AppendFlowItemToGrid(task, item);
			SelectFlowGridRowByFlowItemId(item.FlowItemId);
		}

		private void AddSelectedHardwareBlockToFlow()
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();
			FunctionBlockListItem block = GetSelectedFunctionBlockItem();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName) || block == null)
			{
				MessageBox.Show("Please select Program, Task and Hardware first.", "Add Hardware", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);
			if (task == null)
			{
				return;
			}

			StepFlowItem item = CreateNonStepFlowItem(FLOW_BLOCK_HARDWARE, block.DisplayText);
			item.BlockPath = string.IsNullOrWhiteSpace(block.RelativePath) ? block.FilePath : block.RelativePath;
			item.RunOrder = GetNextDefaultRunOrderFromGrid(task);
			item.Remark = block.RelativePath;

			AppendFlowItemToGrid(task, item);
			SelectFlowGridRowByFlowItemId(item.FlowItemId);
		}

		private void AddSelectedSignalBlockToFlow()
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();
			FunctionBlockListItem block = GetSelectedFunctionBlockItem();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName) || block == null)
			{
				MessageBox.Show("Please select Program, Task and Signal first.", "Add Signal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);
			if (task == null)
			{
				return;
			}

			StepFlowItem item = CreateNonStepFlowItem(FLOW_BLOCK_SIGNAL, block.DisplayText);
			item.SignalProtocol = block.Protocol;
			item.SignalInstanceName = block.InstanceName;
			item.CommunicationOutputProtocol = block.Protocol;
			item.CommunicationOutputInstanceName = block.InstanceName;
			item.RunOrder = GetNextDefaultRunOrderFromGrid(task);
			item.Remark = string.IsNullOrWhiteSpace(block.InstanceName) ? block.Protocol : block.Protocol + "/" + block.InstanceName;

			AppendFlowItemToGrid(task, item);
			SelectFlowGridRowByFlowItemId(item.FlowItemId);
		}

		private StepFlowItem CreateNonStepFlowItem(string blockType, string blockName)
		{
			StepFlowItem item = new StepFlowItem();
			item.BlockType = blockType;
			item.BlockName = blockName;
			item.StepName = blockName;
			item.InputImageKey = string.Empty;
			item.Enabled = true;
			item.EnableCommunicationOutput = false;
			item.DisplayOutputKey = DISPLAY_OUTPUT_NOT_USE;
			item.DisplaySlotName = "Not Show";
			item.DisplayResultKey = DISPLAY_OUTPUT_NOT_USE;
			item.DisplayMode = "Fit";
			item.ScriptInputStepKeys = string.Empty;
			return item;
		}

		private StepFlowItem CloneNonStepFlowItem(StepFlowItem source)
		{
			StepFlowItem item = new StepFlowItem();
			if (source == null)
			{
				item.BlockType = FLOW_BLOCK_HARDWARE;
				return item;
			}

			item.FlowItemId = string.IsNullOrWhiteSpace(source.FlowItemId) ? Guid.NewGuid().ToString("N") : source.FlowItemId;
			item.BlockType = string.IsNullOrWhiteSpace(source.BlockType) ? FLOW_BLOCK_HARDWARE : source.BlockType;
			item.BlockName = source.BlockName ?? string.Empty;
			item.BlockPath = source.BlockPath ?? string.Empty;
			item.SignalProtocol = source.SignalProtocol ?? string.Empty;
			item.SignalInstanceName = source.SignalInstanceName ?? string.Empty;
			item.CommunicationOutputProtocol = source.CommunicationOutputProtocol ?? string.Empty;
			item.CommunicationOutputInstanceName = source.CommunicationOutputInstanceName ?? string.Empty;
			item.SignalOutputs = CloneSignalOutputBindings(source.SignalOutputs);
			item.DatabaseInputs = CloneDatabaseInputBindings(source.DatabaseInputs);
			return item;
		}

		private List<SignalOutputBinding> CloneSignalOutputBindings(List<SignalOutputBinding> source)
		{
			List<SignalOutputBinding> result = new List<SignalOutputBinding>();
			if (source == null)
			{
				return result;
			}

			foreach (SignalOutputBinding binding in source)
			{
				if (binding == null)
				{
					continue;
				}

				result.Add(new SignalOutputBinding
				{
					OutputName = binding.OutputName ?? string.Empty,
					AssignedValue = binding.AssignedValue ?? string.Empty,
					ForceValue = binding.ForceValue,
					Enabled = binding.Enabled
				});
			}

			return result;
		}

		private List<DatabaseInputBinding> CloneDatabaseInputBindings(List<DatabaseInputBinding> source)
		{
			List<DatabaseInputBinding> result = new List<DatabaseInputBinding>();
			if (source == null)
			{
				return result;
			}

			foreach (DatabaseInputBinding binding in source)
			{
				if (binding == null)
				{
					continue;
				}

				result.Add(new DatabaseInputBinding
				{
					InputName = binding.InputName ?? string.Empty,
					GlobalVariableName = binding.GlobalVariableName ?? string.Empty,
					AssignedValue = binding.AssignedValue ?? string.Empty,
					ForceValue = binding.ForceValue,
					Enabled = binding.Enabled
				});
			}

			return result;
		}

		private void EnsureDatabaseInputBindings(StepFlowItem item)
		{
			if (item == null)
			{
				return;
			}

			List<DatabaseInputBinding> existing = item.DatabaseInputs ?? new List<DatabaseInputBinding>();
			Dictionary<string, DatabaseInputBinding> existingMap =
				new Dictionary<string, DatabaseInputBinding>(StringComparer.OrdinalIgnoreCase);
			foreach (DatabaseInputBinding binding in existing)
			{
				if (binding == null || string.IsNullOrWhiteSpace(binding.InputName))
				{
					continue;
				}

				existingMap[binding.InputName.Trim()] = binding;
			}

			HashSet<string> globalNames = new HashSet<string>(
				GlobalVariableStore.GetVariableNames(),
				StringComparer.OrdinalIgnoreCase);

			List<DatabaseInputBinding> normalized = new List<DatabaseInputBinding>();
			try
			{
				DatabaseConfig databaseConfig = DatabaseConfigStore.LoadOrCreateDefault();
				foreach (DatabaseFieldConfig field in databaseConfig.Fields.Where(x => x != null && x.Enabled))
				{
					DatabaseInputBinding binding;
					if (existingMap.TryGetValue(field.InputName, out binding) && binding != null)
					{
						normalized.Add(new DatabaseInputBinding
						{
							InputName = field.InputName,
							GlobalVariableName = binding.GlobalVariableName ?? string.Empty,
							AssignedValue = binding.AssignedValue ?? string.Empty,
							ForceValue = binding.ForceValue,
							Enabled = binding.Enabled
						});
						continue;
					}

					normalized.Add(new DatabaseInputBinding
					{
						InputName = field.InputName,
						GlobalVariableName = globalNames.Contains(field.InputName) ? field.InputName : string.Empty,
						AssignedValue = field.DefaultValue ?? string.Empty,
						ForceValue = false,
						Enabled = true
					});
				}
			}
			catch
			{
			}

			item.DatabaseInputs = normalized;
		}

		private int GetNextDefaultRunOrder(TaskConfig task)
		{
			if (task == null || task.StepFlow == null || task.StepFlow.Count <= 0)
			{
				return 1;
			}

			return task.StepFlow.Max(x => x.RunOrder) + 1;
		}

		private int GetNextDefaultRunOrderFromGrid(TaskConfig task)
		{
			int maxOrder = 0;

			if (dgvSteps != null)
			{
				foreach (DataGridViewRow row in dgvSteps.Rows)
				{
					if (row == null || row.IsNewRow)
					{
						continue;
					}

					maxOrder = Math.Max(maxOrder, GetCellInt(row, "colRunOrder", 0));
				}
			}

			if (maxOrder > 0)
			{
				return maxOrder + 1;
			}

			return GetNextDefaultRunOrder(task);
		}

		private void RefreshStepFlowGrid(string jobName, string taskName)
		{
			BeginUpdateControl(dgvSteps);
			dgvSteps.SuspendLayout();
			try
			{
			InitDisplayBindingColumns();
			MakeStepNameColumnReadOnly();
			RefreshDisplaySlotComboColumn();

			dgvSteps.Rows.Clear();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName))
			{
				return;
			}

			TaskConfig task = GetTaskConfig(jobName, taskName);

			if (task == null)
			{
				return;
			}

			foreach (StepFlowItem item in task.StepFlow.OrderBy(x => x.RunOrder))
			{
				AppendFlowItemToGrid(task, item);
			}
			}
			finally
			{
				dgvSteps.ResumeLayout();
				EndUpdateControl(dgvSteps);
			}
		}

		private void AppendFlowItemToGrid(TaskConfig task, StepFlowItem item)
		{
			if (dgvSteps == null || task == null || item == null)
			{
				return;
			}

			InitDisplayBindingColumns();
			MakeStepNameColumnReadOnly();
			RefreshDisplaySlotComboColumn();

			int rowIndex = dgvSteps.Rows.Add();
			DataGridViewRow row = dgvSteps.Rows[rowIndex];
			ApplyFlowItemToGridRow(row, task, item);
		}

		private void ApplyFlowItemToGridRow(DataGridViewRow row, TaskConfig task, StepFlowItem item)
		{
			if (row == null || task == null || item == null)
			{
				return;
			}

			row.Tag = item;
			row.Cells["colStep"].Value = GetFlowItemDisplayName(item);
			row.Cells["colImageSource"].Value = IsImageSourceSelectableRow(row, task)
				? item.InputImageKey
				: string.Empty;
			row.Cells["colRunOrder"].Value = item.RunOrder.ToString();
			row.Cells["colRemark"].Value = item.Remark;

			if (dgvSteps.Columns.Contains(COL_DISPLAY_OUTPUT))
			{
				ApplyDisplayOutputOptionsToRow(row, task);
				row.Cells[COL_DISPLAY_OUTPUT].Value = NormalizeDisplayOutputKey(
					GetStepConfigForRow(row, task),
					item.DisplayOutputKey);
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
			{
				row.Cells[COL_DISPLAY_SLOT].Value =
					string.IsNullOrWhiteSpace(item.DisplaySlotName) ? "Not Show" : item.DisplaySlotName;
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_RESULT))
			{
				ApplyDisplayResultOptionsToRow(row, task);
				row.Cells[COL_DISPLAY_RESULT].Value = NormalizeDisplayResultKey(
					GetStepConfigForRow(row, task),
					item.DisplayResultKey);
			}

			ApplyStepFlowRowVisual(row, task);
		}

		private string GetFlowItemDisplayName(StepFlowItem item)
		{
			if (item == null)
			{
				return string.Empty;
			}

			if (!item.IsStepBlock && !string.IsNullOrWhiteSpace(item.BlockName))
			{
				return item.BlockName;
			}

			return string.IsNullOrWhiteSpace(item.StepName) ? item.BlockName : item.StepName;
		}


		private void btnDeleteSelected_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName)) return;
			if (dgvSteps != null)
			{
				dgvSteps.EndEdit();
			}

			List<DataGridViewRow> rowsToDelete = GetFlowRowsForDelete();
			if (rowsToDelete.Count <= 0) return;

			int selectIndex = rowsToDelete.Min(x => x.Index);
			foreach (DataGridViewRow row in rowsToDelete)
			{
				if (row.DataGridView == dgvSteps)
				{
					dgvSteps.Rows.Remove(row);
				}
			}

			SelectFlowGridRowByIndex(Math.Min(selectIndex, dgvSteps.Rows.Count - 1));
		}

		private List<DataGridViewRow> GetFlowRowsForDelete()
		{
			if (dgvSteps == null)
			{
				return new List<DataGridViewRow>();
			}

			if (IsValidFlowGridRow(dgvSteps.CurrentRow))
			{
				return new List<DataGridViewRow> { dgvSteps.CurrentRow };
			}

			Dictionary<int, DataGridViewRow> rows = new Dictionary<int, DataGridViewRow>();
			foreach (DataGridViewRow row in dgvSteps.SelectedRows)
			{
				AddFlowRowForDelete(rows, row);
			}

			foreach (DataGridViewCell cell in dgvSteps.SelectedCells)
			{
				if (cell == null || cell.RowIndex < 0 || cell.RowIndex >= dgvSteps.Rows.Count)
				{
					continue;
				}

				AddFlowRowForDelete(rows, dgvSteps.Rows[cell.RowIndex]);
			}

			return rows.Values.OrderByDescending(x => x.Index).ToList();
		}

		private void AddFlowRowForDelete(Dictionary<int, DataGridViewRow> rows, DataGridViewRow row)
		{
			if (rows == null || !IsValidFlowGridRow(row))
			{
				return;
			}

			if (!rows.ContainsKey(row.Index))
			{
				rows.Add(row.Index, row);
			}
		}

		private bool IsValidFlowGridRow(DataGridViewRow row)
		{
			return row != null && !row.IsNewRow && row.Index >= 0;
		}

		private void btnMoveUp_Click(object sender, EventArgs e)
		{
			MoveSelectedFlowItem(-1);
		}

		private void btnMoveDown_Click(object sender, EventArgs e)
		{
			MoveSelectedFlowItem(1);
		}

		private void MoveSelectedFlowItem(int direction)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName)) return;
			if (dgvSteps.SelectedRows.Count <= 0) return;

			int oldIndex = dgvSteps.SelectedRows[0].Index;
			int newIndex = oldIndex + direction;

			if (oldIndex < 0 || newIndex < 0 || newIndex >= dgvSteps.Rows.Count) return;

			SwapFlowGridRows(oldIndex, newIndex);
			SelectFlowGridRowByIndex(newIndex);
		}

		private void SwapFlowGridRows(int firstIndex, int secondIndex)
		{
			if (dgvSteps == null ||
				firstIndex < 0 ||
				secondIndex < 0 ||
				firstIndex >= dgvSteps.Rows.Count ||
				secondIndex >= dgvSteps.Rows.Count ||
				firstIndex == secondIndex)
			{
				return;
			}

			DataGridViewRow firstRow = dgvSteps.Rows[firstIndex];
			DataGridViewRow secondRow = dgvSteps.Rows[secondIndex];
			Dictionary<string, object> firstValues = CaptureFlowGridRowValues(firstRow);
			Dictionary<string, object> secondValues = CaptureFlowGridRowValues(secondRow);
			object firstTag = firstRow.Tag;
			object secondTag = secondRow.Tag;

			firstRow.Tag = secondTag;
			secondRow.Tag = firstTag;
			RestoreFlowGridRowValues(firstRow, secondValues);
			RestoreFlowGridRowValues(secondRow, firstValues);

			TaskConfig task = GetTaskConfig(GetSelectedJobName(), GetSelectedTaskName());
			if (task != null)
			{
				ApplyStepFlowRowVisual(firstRow, task);
				ApplyStepFlowRowVisual(secondRow, task);
			}
		}

		private Dictionary<string, object> CaptureFlowGridRowValues(DataGridViewRow row)
		{
			Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			if (row == null || dgvSteps == null)
			{
				return values;
			}

			foreach (DataGridViewColumn column in dgvSteps.Columns)
			{
				values[column.Name] = row.Cells[column.Index].Value;
			}

			return values;
		}

		private void RestoreFlowGridRowValues(DataGridViewRow row, Dictionary<string, object> values)
		{
			if (row == null || values == null || dgvSteps == null)
			{
				return;
			}

			foreach (DataGridViewColumn column in dgvSteps.Columns)
			{
				object value;
				if (values.TryGetValue(column.Name, out value))
				{
					row.Cells[column.Index].Value = value;
				}
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName))
			{
				MessageBox.Show("Please select Job and Task first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (dgvSteps != null)
			{
				dgvSteps.EndEdit();
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);

			if (task == null)
			{
				MessageBox.Show("Task config was not found.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			task.CommunicationProtocol = GetSelectedProtocolName();
			task.CommunicationChannel = GetSelectedChannelName();
			task.StepFlow.Clear();

			int fallbackOrder = 1;
			List<string> scriptCompileWarnings = new List<string>();

			foreach (DataGridViewRow row in dgvSteps.Rows)
			{
				if (row.IsNewRow)
				{
					continue;
				}

				string stepName = GetCellString(row, "colStep");

				if (string.IsNullOrEmpty(stepName))
				{
					continue;
				}

				StepFlowItem sourceFlowItem = row.Tag as StepFlowItem;
				if (sourceFlowItem != null && !sourceFlowItem.IsStepBlock)
				{
					StepFlowItem blockItem = CloneNonStepFlowItem(sourceFlowItem);
					blockItem.StepName = stepName;
					blockItem.BlockName = string.IsNullOrWhiteSpace(sourceFlowItem.BlockName) ? stepName : sourceFlowItem.BlockName;
					blockItem.InputImageKey = string.Empty;
					blockItem.RunOrder = GetCellInt(row, "colRunOrder", fallbackOrder);
					blockItem.Enabled = true;
					blockItem.EnableCommunicationOutput = false;
					blockItem.Remark = GetCellString(row, "colRemark");
					blockItem.DisplayOutputKey = DISPLAY_OUTPUT_NOT_USE;
					blockItem.DisplaySlotName = "Not Show";
					blockItem.DisplayResultKey = DISPLAY_OUTPUT_NOT_USE;
					blockItem.DisplayMode = "Fit";
					blockItem.ScriptInputStepKeys = string.Empty;
					task.StepFlow.Add(blockItem);
					fallbackOrder++;
					continue;
				}

				StepConfig usedStep = task.Steps.FirstOrDefault(s =>
					string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase));

				if (usedStep == null)
				{
					MessageBox.Show(
						"Step library item was not found.\r\n\r\nStep: " + stepName,
						"Save",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
					continue;
				}

				bool fileSaved = SaveUsedStepFileToProject(jobName, taskName, usedStep);

				if (!fileSaved)
				{
					continue;
				}

				if (usedStep.StepType == StepType.Script)
				{
					string compileWarning;
					if (!TryPrecompileScriptStep(jobName, taskName, usedStep, out compileWarning))
					{
						scriptCompileWarnings.Add(stepName + ": " + compileWarning);
					}
				}

				StepFlowItem item = new StepFlowItem();
				if (sourceFlowItem != null && !string.IsNullOrWhiteSpace(sourceFlowItem.FlowItemId))
				{
					item.FlowItemId = sourceFlowItem.FlowItemId;
				}
				item.BlockType = FLOW_BLOCK_STEP;
				item.BlockName = stepName;
				item.StepName = stepName;
				item.InputImageKey = IsVisualImageSourceStep(usedStep)
					? GetCellString(row, "colImageSource")
					: string.Empty;
				item.RunOrder = GetCellInt(row, "colRunOrder", fallbackOrder);
				item.Enabled = true;
				item.EnableCommunicationOutput = false;
				item.Remark = GetCellString(row, "colRemark");

				if (usedStep.StepType == StepType.Script)
				{
					// Script 只处理数据，不绑定图像源/输出图像/显示框。
					item.InputImageKey = string.Empty;
					item.DisplayOutputKey = DISPLAY_OUTPUT_NOT_USE;
					item.DisplaySlotName = "Not Show";
					item.DisplayResultKey = DISPLAY_OUTPUT_NOT_USE;
					item.DisplayMode = "Fit";
					item.ScriptInputStepKeys = string.Empty;
					item.Remark = MergeScriptInputRemark(item.Remark, string.Empty);
				}
				else
				{
					item.DisplayOutputKey = NormalizeDisplayOutputKey(usedStep, GetCellString(row, COL_DISPLAY_OUTPUT));
					item.DisplaySlotName = GetCellString(row, COL_DISPLAY_SLOT);
					item.DisplayResultKey = NormalizeDisplayResultKey(usedStep, GetCellString(row, COL_DISPLAY_RESULT));
					item.DisplayMode = "Fit";

					if (string.IsNullOrWhiteSpace(item.DisplayOutputKey))
					{
						item.DisplayOutputKey = GetFixedDisplayOutputKey(usedStep.StepType);
					}

					if (string.IsNullOrWhiteSpace(item.DisplaySlotName))
					{
						item.DisplaySlotName = "Not Show";
					}

					if (string.IsNullOrWhiteSpace(item.DisplayResultKey))
					{
						item.DisplayResultKey = GetDefaultDisplayResultKey(usedStep);
					}
				}

				task.StepFlow.Add(item);
				fallbackOrder++;
			}

			FlowConfigStore.Save(config);

			RefreshStepLibraryByTask(jobName, taskName);
			RefreshStepFlowGrid(jobName, taskName);

			string saveMessage = "Task flow configuration saved.\r\nUsed step files have been copied to the project folder.";
			if (scriptCompileWarnings.Count > 0)
			{
				saveMessage += "\r\n\r\nScript compile cache warning:";
				foreach (string warning in scriptCompileWarnings.Take(5))
				{
					saveMessage += "\r\n" + warning;
				}
				if (scriptCompileWarnings.Count > 5)
				{
					saveMessage += "\r\n...";
				}
			}

			MessageBox.Show(
				saveMessage,
				"Save",
				MessageBoxButtons.OK,
				scriptCompileWarnings.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
		}


		#endregion

		#region Save Used Step Files

		private bool SaveUsedStepFileToProject(string jobName, string taskName, StepConfig step)
		{
			if (step == null)
			{
				return false;
			}

			string sourceFilePath = step.SourceFilePath;
			string projectRelativeFilePath;
			if (!string.IsNullOrEmpty(sourceFilePath) &&
				File.Exists(sourceFilePath) &&
				TryGetProjectRelativeStepFilePath(jobName, taskName, step, sourceFilePath, out projectRelativeFilePath))
			{
				ApplyStepProjectFilePath(jobName, taskName, step, projectRelativeFilePath);
				return true;
			}

			if (!string.IsNullOrEmpty(step.ProjectFilePath))
			{
				string existedProjectFile = GetAbsoluteProjectStepFilePath(jobName, taskName, step);

				if (File.Exists(existedProjectFile))
				{
					return true;
				}
			}

			if (string.IsNullOrEmpty(sourceFilePath))
			{
				string projectFile = GetAbsoluteProjectStepFilePath(jobName, taskName, step);

				if (File.Exists(projectFile))
				{
					step.ProjectFilePath = GetRelativeStepFilePath(step);
					return true;
				}

				MessageBox.Show(
					"Source file path is empty and project file does not exist.\r\n\r\nStep: " + step.StepName,
					"Save Step File",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return false;
			}

			if (!File.Exists(sourceFilePath))
			{
				MessageBox.Show(
					"Source file does not exist.\r\n\r\nStep: " + step.StepName + "\r\nFile: " + sourceFilePath,
					"Save Step File",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return false;
			}

			FlowConfigStore.PathManager.EnsureStepFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName, step.StepName);

			// 新目录结构：
			// Project\Task\<TaskName>\<ProgramNo>\VPP\xxx.vpp
			// Project\Task\<TaskName>\<ProgramNo>\Hdev\xxx.hdev
			// Project\Task\<TaskName>\<ProgramNo>\Script\xxx.csx
			string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName);
			string subFolderName = GetStepProjectSubFolderName(step.StepType);
			string targetFolder = Path.Combine(taskFolder, subFolderName);

			Directory.CreateDirectory(targetFolder);

			string sourceFileName = Path.GetFileName(sourceFilePath);
			string targetFileName = sourceFileName;
			string targetFilePath = Path.Combine(targetFolder, targetFileName);
			bool sourceIsTargetFile = IsSameFullPath(sourceFilePath, targetFilePath);

			if (!sourceIsTargetFile)
			{
				targetFileName = MakeUniqueProjectStepFileName(taskFolder, subFolderName, sourceFileName, step.StepName);
				targetFilePath = Path.Combine(targetFolder, targetFileName);
			}

			try
			{
				if (!sourceIsTargetFile)
				{
					File.Copy(sourceFilePath, targetFilePath, true);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Failed to copy step file to project folder.\r\n\r\nStep: " + step.StepName +
					"\r\nSource: " + sourceFilePath +
					"\r\nTarget: " + targetFilePath +
					"\r\n\r\n" + ex.Message,
					"Save Step File",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return false;
			}

			string relativeFilePath = Path.Combine(subFolderName, targetFileName);

			ApplyStepProjectFilePath(jobName, taskName, step, relativeFilePath);
			step.SourceFilePath = targetFilePath;

			return true;
		}

		private bool TryPrecompileScriptStep(string jobName, string taskName, StepConfig step, out string warning)
		{
			warning = string.Empty;

			if (step == null || step.StepType != StepType.Script)
			{
				return true;
			}

			try
			{
				string scriptPath = GetAbsoluteProjectStepFilePath(jobName, taskName, step);
				if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
				{
					warning = "script file not found.";
					return false;
				}

				string configPath = Path.ChangeExtension(scriptPath, ".script.xml");
				if (string.IsNullOrWhiteSpace(configPath) || !File.Exists(configPath))
				{
					configPath = CSharpScriptStepStore.GetConfigPath(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName, step.StepName);
				}

				CSharpScriptStepConfig scriptConfig = CSharpScriptStepStore.Load(configPath);
				if (scriptConfig == null)
				{
					scriptConfig = CSharpScriptStepStore.CreateDefaultConfig();
				}

				scriptConfig.StepName = step.StepName;
				scriptConfig.ScriptFileName = Path.GetFileName(scriptPath);
				scriptConfig.ScriptFilePath = scriptPath;
				CSharpScriptStepStore.EnsureRequiredInputs(scriptConfig);

				string code = File.ReadAllText(scriptPath, Encoding.UTF8);
				CSharpScriptRunResult result = new CSharpScriptStepRunner().CompileAndCache(scriptConfig, code);

				if (!result.IsCompileOK || !result.IsRunOK)
				{
					warning = string.IsNullOrWhiteSpace(result.ErrorDetail) ? result.Message : result.ErrorDetail;
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				warning = ex.Message;
				return false;
			}
		}

		private bool TryGetProjectRelativeStepFilePath(string jobName, string taskName, StepConfig step, string filePath, out string relativeFilePath)
		{
			relativeFilePath = string.Empty;

			if (step == null || string.IsNullOrWhiteSpace(filePath))
			{
				return false;
			}

			try
			{
				string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName);
				string subFolderName = GetStepProjectSubFolderName(step.StepType);
				string targetFolder = Path.GetFullPath(Path.Combine(taskFolder, subFolderName))
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string sourceFullPath = Path.GetFullPath(filePath);
				string sourceFolder = Path.GetDirectoryName(sourceFullPath);

				if (string.IsNullOrWhiteSpace(sourceFolder))
				{
					return false;
				}

				sourceFolder = Path.GetFullPath(sourceFolder)
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

				if (!string.Equals(sourceFolder, targetFolder, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}

				relativeFilePath = Path.Combine(subFolderName, Path.GetFileName(sourceFullPath));
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void ApplyStepProjectFilePath(string jobName, string taskName, StepConfig step, string relativeFilePath)
		{
			if (step == null)
			{
				return;
			}

			step.ProjectFilePath = relativeFilePath ?? string.Empty;

			if (step.StepType == StepType.Vpp)
			{
				if (step.VppFiles == null)
				{
					step.VppFiles = new List<string>();
				}

				step.VppFiles.Clear();
				step.VppFiles.Add(step.ProjectFilePath);
			}
			else if (step.StepType == StepType.Script)
			{
				if (step.ScriptFiles == null)
				{
					step.ScriptFiles = new List<string>();
				}

				step.ScriptFiles.Clear();
				step.ScriptFiles.Add(step.ProjectFilePath);
			}

			step.StepFolder = Path.Combine(
				"Task",
				FlowConfigStore.PathManager.MakeSafeName(taskName),
				FlowConfigStore.PathManager.MakeSafeName(jobName));
		}

		private string GetStepProjectSubFolderName(StepType stepType)
		{
			if (stepType == StepType.Vpp)
			{
				return "VPP";
			}

			if (stepType == StepType.Halcon)
			{
				return "Hdev";
			}

			return "Script";
		}



		private string GetRelativeStepFilePath(StepConfig step)
		{
			if (step == null)
			{
				return string.Empty;
			}

			if (step.StepType == StepType.Vpp && step.VppFiles != null && step.VppFiles.Count > 0)
			{
				return step.VppFiles[0];
			}

			if (step.StepType == StepType.Script && step.ScriptFiles != null && step.ScriptFiles.Count > 0)
			{
				return step.ScriptFiles[0];
			}

			return step.ProjectFilePath;
		}

		private string GetAbsoluteProjectStepFilePath(string jobName, string taskName, StepConfig step)
		{
			string relativeFilePath = GetRelativeStepFilePath(step);

			if (string.IsNullOrEmpty(relativeFilePath))
			{
				return string.Empty;
			}

			foreach (string stepFolder in FlowConfigStore.PathManager.GetTaskFolderCandidates(
				GetSelectedProtocolName(),
				GetSelectedChannelName(),
				jobName,
				taskName))
			{
				string candidate = Path.Combine(stepFolder, relativeFilePath);
				if (File.Exists(candidate))
				{
					return candidate;
				}

				string alternate = GetAlternateScriptRelativeFilePath(relativeFilePath);
				if (!string.Equals(alternate, relativeFilePath, StringComparison.OrdinalIgnoreCase))
				{
					string alternateCandidate = Path.Combine(stepFolder, alternate);
					if (File.Exists(alternateCandidate))
					{
						return alternateCandidate;
					}
				}
			}

			return Path.Combine(
				FlowConfigStore.PathManager.GetTaskFolder(GetSelectedProtocolName(), GetSelectedChannelName(), jobName, taskName),
				relativeFilePath);
		}

		private string GetAlternateScriptRelativeFilePath(string relativeFilePath)
		{
			if (string.IsNullOrWhiteSpace(relativeFilePath))
			{
				return relativeFilePath;
			}

			string normalized = relativeFilePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string scriptPrefix = "Script" + Path.DirectorySeparatorChar;
			string scriptsPrefix = "Scripts" + Path.DirectorySeparatorChar;

			if (normalized.StartsWith(scriptPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return "Scripts" + Path.DirectorySeparatorChar + normalized.Substring(scriptPrefix.Length);
			}

			if (normalized.StartsWith(scriptsPrefix, StringComparison.OrdinalIgnoreCase))
			{
				return "Script" + Path.DirectorySeparatorChar + normalized.Substring(scriptsPrefix.Length);
			}

			return relativeFilePath;
		}


		#endregion

		#region File / Name Helper

		private string MakeSafeName(string name)
		{
			foreach (char c in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(c, '_');
			}

			name = name.Replace(" ", "_");

			return name;
		}

		private string BuildStepFlowRemark(StepConfig step)
		{
			string remark = step.StepType.ToString();

			if (step.StepType == StepType.Vpp)
			{
				if (step.VppFiles.Count > 0)
				{
					remark += " | VPP: " + step.VppFiles[0];
				}
				else if (!string.IsNullOrEmpty(step.SourceFilePath))
				{
					remark += " | Source: " + step.SourceFilePath;
				}
			}
			else if (step.StepType == StepType.Script)
			{
				if (step.ScriptFiles.Count > 0)
				{
					remark += " | Script: " + step.ScriptFiles[0];
				}
				else if (!string.IsNullOrEmpty(step.SourceFilePath))
				{
					remark += " | Source: " + step.SourceFilePath;
				}
			}
			else if (step.StepType == StepType.Halcon)
			{
				if (!string.IsNullOrEmpty(step.ProjectFilePath))
				{
					remark += " | Hdev: " + step.ProjectFilePath;
				}
				else if (!string.IsNullOrEmpty(step.SourceFilePath))
				{
					remark += " | Source: " + step.SourceFilePath;
				}
			}

			return remark;
		}

		private void SelectFlowGridRowByStepName(string stepName)
		{
			for (int i = 0; i < dgvSteps.Rows.Count; i++)
			{
				if (dgvSteps.Rows[i].Cells[0].Value != null &&
					string.Equals(dgvSteps.Rows[i].Cells[0].Value.ToString(), stepName, StringComparison.OrdinalIgnoreCase))
				{
					dgvSteps.ClearSelection();
					dgvSteps.Rows[i].Selected = true;
					return;
				}
			}
		}

		private string MakeUniqueProjectStepFileName(string taskFolder, string subFolderName, string sourceFileName, string stepName)
		{
			string targetFolder = Path.Combine(taskFolder, subFolderName);

			string ext = Path.GetExtension(sourceFileName);
			string nameWithoutExt = Path.GetFileNameWithoutExtension(sourceFileName);

			if (string.IsNullOrEmpty(nameWithoutExt))
			{
				nameWithoutExt = stepName;
			}

			string targetFileName = sourceFileName;
			string targetFilePath = Path.Combine(targetFolder, targetFileName);

			if (!File.Exists(targetFilePath))
			{
				return targetFileName;
			}

			// 如果文件名已经存在，为了避免不同 Step 使用同名文件互相覆盖，
			// 自动改成：StepName_原文件名
			targetFileName = MakeSafeName(stepName) + "_" + nameWithoutExt + ext;
			targetFilePath = Path.Combine(targetFolder, targetFileName);

			if (!File.Exists(targetFilePath))
			{
				return targetFileName;
			}

			int index = 1;

			while (true)
			{
				targetFileName = MakeSafeName(stepName) + "_" + nameWithoutExt + "_" + index.ToString("00") + ext;
				targetFilePath = Path.Combine(targetFolder, targetFileName);

				if (!File.Exists(targetFilePath))
				{
					return targetFileName;
				}

				index++;
			}
		}

		private bool IsSameFullPath(string left, string right)
		{
			if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
			{
				return false;
			}

			try
			{
				string leftFullPath = Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string rightFullPath = Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

				return string.Equals(leftFullPath, rightFullPath, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}


		#endregion

		#region Delete Local Folders

		private void DeleteJobFolder(string jobName)
		{
			DeleteJobFolder("TCP/IP", "Channel01", jobName);
		}

		private void DeleteJobFolder(string protocolName, string channelName, string jobName)
		{
			try
			{
				string jobFolder = FlowConfigStore.PathManager.GetJobFolder(protocolName, channelName, jobName);
				if (Directory.Exists(jobFolder)) Directory.Delete(jobFolder, true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to delete job folder.\r\n\r\n" + ex.Message, "Delete Job Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void DeleteTaskFolder(string jobName, string taskName)
		{
			try
			{
				List<string> deleted = new List<string>();
				foreach (string taskFolder in FlowConfigStore.PathManager.GetTaskFolderCandidates(
					GetSelectedProtocolName(),
					GetSelectedChannelName(),
					jobName,
					taskName))
				{
					if (!Directory.Exists(taskFolder) ||
						deleted.Any(x => string.Equals(x, taskFolder, StringComparison.OrdinalIgnoreCase)))
					{
						continue;
					}

					Directory.Delete(taskFolder, true);
					deleted.Add(taskFolder);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to delete task folder.\r\n\r\n" + ex.Message, "Delete Task Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void DeleteStepProjectFile(string jobName, string taskName, StepConfig step)
		{
			string filePath = GetAbsoluteProjectStepFilePath(jobName, taskName, step);

			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}

		#endregion

		private void InitDisplayBindingColumns()
		{
			if (dgvSteps == null)
			{
				return;
			}

			dgvSteps.AutoGenerateColumns = false;

			if (!dgvSteps.Columns.Contains(COL_DISPLAY_OUTPUT))
			{
				DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
				col.Name = COL_DISPLAY_OUTPUT;
				col.HeaderText = "输出图像";
				col.FlatStyle = FlatStyle.Flat;
				col.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
				col.Items.AddRange(new object[]
				{
					DISPLAY_OUTPUT_NOT_USE,
					VPP_DISPLAY_OUTPUT_IMAGE,
					HDEV_DISPLAY_OUTPUT_IMAGE
				});

				dgvSteps.Columns.Insert(Math.Min(3, dgvSteps.Columns.Count), col);
			}

			if (!dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
			{
				DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
				col.Name = COL_DISPLAY_SLOT;
				col.HeaderText = "绑定显示框";
				col.FlatStyle = FlatStyle.Flat;
				col.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

				foreach (string slotName in DisplayLayoutStore.GetDisplaySlotNames())
				{
					col.Items.Add(slotName);
				}

				dgvSteps.Columns.Insert(Math.Min(4, dgvSteps.Columns.Count), col);
			}

			if (!dgvSteps.Columns.Contains(COL_DISPLAY_RESULT))
			{
				DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
				col.Name = COL_DISPLAY_RESULT;
				col.HeaderText = "图像输出绑定";
				col.FlatStyle = FlatStyle.Flat;
				col.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
				col.Items.Add(DISPLAY_OUTPUT_NOT_USE);
				col.Items.Add(DEFAULT_DISPLAY_RESULT_OUTPUT);

				int insertIndex = dgvSteps.Columns.Contains(COL_DISPLAY_SLOT)
					? dgvSteps.Columns[COL_DISPLAY_SLOT].Index + 1
					: Math.Min(5, dgvSteps.Columns.Count);
				dgvSteps.Columns.Insert(Math.Min(insertIndex, dgvSteps.Columns.Count), col);
			}

			// 显示方式固定为 Fit，不再在流程表中显示该列。
			if (dgvSteps.Columns.Contains(COL_DISPLAY_MODE))
			{
				dgvSteps.Columns.Remove(COL_DISPLAY_MODE);
			}

			if (dgvSteps.Columns.Contains("EnableCommunicationOutput"))
			{
				dgvSteps.Columns.Remove("EnableCommunicationOutput");
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_OUTPUT))
			{
				dgvSteps.Columns[COL_DISPLAY_OUTPUT].FillWeight = 90;
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
			{
				dgvSteps.Columns[COL_DISPLAY_SLOT].FillWeight = 90;
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_RESULT))
			{
				dgvSteps.Columns[COL_DISPLAY_RESULT].FillWeight = 90;
			}

		}
		private void RefreshDisplaySlotComboColumn()
		{
			RefreshDisplaySlotComboColumn(null);
		}

		private void RefreshDisplaySlotComboColumn(List<string> slotNames)
		{
			if (dgvSteps == null || !dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
			{
				return;
			}

			DataGridViewComboBoxColumn col = dgvSteps.Columns[COL_DISPLAY_SLOT] as DataGridViewComboBoxColumn;

			if (col == null)
			{
				return;
			}

			col.Items.Clear();

			if (slotNames == null)
			{
				slotNames = DisplayLayoutStore.GetDisplaySlotNames();
			}

			foreach (string slotName in slotNames)
			{
				col.Items.Add(slotName);
			}
		}

		private void RefreshDisplaySlotBindingsFromLayout()
		{
			if (dgvSteps == null || dgvSteps.IsDisposed || !dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
			{
				return;
			}

			bool oldLoading = _loading;
			List<string> slotNames = DisplayLayoutStore.GetDisplaySlotNames();
			HashSet<string> validSlotNames = new HashSet<string>(slotNames, StringComparer.OrdinalIgnoreCase);
			if (!validSlotNames.Contains("Not Show"))
			{
				validSlotNames.Add("Not Show");
			}

			_loading = true;
			BeginUpdateControl(dgvSteps);
			dgvSteps.SuspendLayout();

			try
			{
				RefreshDisplaySlotComboColumn(slotNames);

				foreach (DataGridViewRow row in dgvSteps.Rows)
				{
					if (row == null || row.IsNewRow)
					{
						continue;
					}

					DataGridViewCell cell = row.Cells[COL_DISPLAY_SLOT];
					string currentSlot = cell == null || cell.Value == null ? string.Empty : cell.Value.ToString();

					if (string.IsNullOrWhiteSpace(currentSlot) || !validSlotNames.Contains(currentSlot))
					{
						if (cell != null)
						{
							cell.Value = "Not Show";
						}

						StepFlowItem item = row.Tag as StepFlowItem;
						if (item != null)
						{
							item.DisplaySlotName = "Not Show";
						}
					}
				}

				TaskConfig task = GetTaskConfig(GetSelectedJobName(), GetSelectedTaskName());
				if (task != null)
				{
					foreach (DataGridViewRow row in dgvSteps.Rows)
					{
						if (row != null && !row.IsNewRow)
						{
							ApplyStepFlowRowVisual(row, task);
						}
					}
				}
			}
			finally
			{
				dgvSteps.ResumeLayout();
				EndUpdateControl(dgvSteps);
				_loading = oldLoading;
			}
		}

		private string GetCellString(DataGridViewRow row, string columnName)
		{
			if (row == null || string.IsNullOrWhiteSpace(columnName))
			{
				return string.Empty;
			}

			if (!dgvSteps.Columns.Contains(columnName))
			{
				return string.Empty;
			}

			object value = row.Cells[columnName].Value;

			if (value == null)
			{
				return string.Empty;
			}

			return value.ToString().Trim();
		}

		private int GetCellInt(DataGridViewRow row, string columnName, int defaultValue)
		{
			int value;

			if (int.TryParse(GetCellString(row, columnName), out value))
			{
				return value;
			}

			return defaultValue;
		}

		private bool GetCellBool(DataGridViewRow row, string columnName)
		{
			if (row == null ||
				string.IsNullOrWhiteSpace(columnName) ||
				!dgvSteps.Columns.Contains(columnName))
			{
				return false;
			}

			object value = row.Cells[columnName].Value;
			if (value == null)
			{
				return false;
			}

			bool boolValue;
			if (bool.TryParse(value.ToString(), out boolValue))
			{
				return boolValue;
			}

			return false;
		}



		#region Step Flow Row Policy / Selection Dialogs

		private void ApplyFlowUiPolicy()
		{
			HideMoveButtons();

			if (dgvSteps != null)
			{
				dgvSteps.EditMode = DataGridViewEditMode.EditOnEnter;
				dgvSteps.MultiSelect = false;
				dgvSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				dgvSteps.DataError -= dgvSteps_DataError;
				dgvSteps.DataError += dgvSteps_DataError;
			}
		}

		private void EnableDoubleBufferForPage()
		{
			SetDoubleBuffered(this);
			SetDoubleBuffered(rootLayout);
			SetDoubleBuffered(leftLayout);
			SetDoubleBuffered(panelJobs);
			SetDoubleBuffered(panelTasks);
			SetDoubleBuffered(panelStepList);
			SetDoubleBuffered(panelStepListHeader);
			SetDoubleBuffered(panelStepIconBar);
			SetDoubleBuffered(panelSteps);
			SetDoubleBuffered(panelButtons);
			SetDoubleBuffered(listJobs);
			SetDoubleBuffered(listTasks);
			SetDoubleBuffered(listSteps);
			SetDoubleBuffered(dgvSteps);
		}

		private void SetDoubleBuffered(Control control)
		{
			if (control == null)
			{
				return;
			}

			try
			{
				System.Reflection.PropertyInfo property = typeof(Control).GetProperty(
					"DoubleBuffered",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic);

				if (property != null)
				{
					property.SetValue(control, true, null);
				}
			}
			catch
			{
			}
		}

		private void BeginUpdateControl(Control control)
		{
			if (control == null || control.IsDisposed)
			{
				return;
			}

			try
			{
				NativeMethods.SendMessage(control.Handle, NativeMethods.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
			}
			catch
			{
			}
		}

		private void EndUpdateControl(Control control)
		{
			if (control == null || control.IsDisposed)
			{
				return;
			}

			try
			{
				NativeMethods.SendMessage(control.Handle, NativeMethods.WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
				control.Invalidate(true);
				control.Update();
			}
			catch
			{
			}
		}

		private static class NativeMethods
		{
			public const int WM_SETREDRAW = 0x000B;

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
		}

		private void HideMoveButtons()
		{
			if (btnMoveUp != null)
			{
				btnMoveUp.Visible = false;
				btnMoveUp.Enabled = false;
			}

			if (btnMoveDown != null)
			{
				btnMoveDown.Visible = false;
				btnMoveDown.Enabled = false;
			}
		}

		private void BindStepFlowGridEvents()
		{
			if (dgvSteps == null)
			{
				return;
			}

			dgvSteps.CellDoubleClick -= dgvSteps_CellDoubleClick;
			dgvSteps.CellDoubleClick += dgvSteps_CellDoubleClick;
			dgvSteps.CurrentCellDirtyStateChanged -= dgvSteps_CurrentCellDirtyStateChanged;
			dgvSteps.CurrentCellDirtyStateChanged += dgvSteps_CurrentCellDirtyStateChanged;
			dgvSteps.CellValueChanged -= dgvSteps_CellValueChanged;
			dgvSteps.CellValueChanged += dgvSteps_CellValueChanged;
		}

		private void dgvSteps_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (_loading || _applyingFlowVisual || e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}

			string columnName = dgvSteps.Columns[e.ColumnIndex].Name;
			if (columnName != COL_DISPLAY_SLOT &&
				columnName != COL_DISPLAY_RESULT &&
				columnName != COL_DISPLAY_OUTPUT)
			{
				return;
			}

			TaskConfig task = GetTaskConfig(GetSelectedJobName(), GetSelectedTaskName());
			if (task != null)
			{
				ApplyStepFlowRowVisual(dgvSteps.Rows[e.RowIndex], task);
			}
		}

		private void dgvSteps_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (dgvSteps == null || !dgvSteps.IsCurrentCellDirty)
			{
				return;
			}

			if (dgvSteps.CurrentCell != null &&
				(dgvSteps.Columns[dgvSteps.CurrentCell.ColumnIndex].Name == COL_DISPLAY_SLOT ||
				 dgvSteps.Columns[dgvSteps.CurrentCell.ColumnIndex].Name == COL_DISPLAY_RESULT ||
				 dgvSteps.Columns[dgvSteps.CurrentCell.ColumnIndex].Name == COL_DISPLAY_OUTPUT))
			{
				dgvSteps.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private void dgvSteps_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private void dgvSteps_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}

			DataGridViewRow row = dgvSteps.Rows[e.RowIndex];
			string columnName = dgvSteps.Columns[e.ColumnIndex].Name;

			if (string.Equals(columnName, "colImageSource", StringComparison.OrdinalIgnoreCase))
			{
				ShowImageSourceSelectorForRow(row);
				return;
			}

			if (string.Equals(columnName, "colStep", StringComparison.OrdinalIgnoreCase))
			{
				StepFlowItem flowItem = row.Tag as StepFlowItem;
				if (flowItem != null &&
					string.Equals(flowItem.BlockType, FLOW_BLOCK_SIGNAL, StringComparison.OrdinalIgnoreCase))
				{
					ShowSignalOutputSettingsForRow(row, flowItem);
					return;
				}

				if (flowItem != null &&
					string.Equals(flowItem.BlockType, FLOW_BLOCK_DATABASE, StringComparison.OrdinalIgnoreCase))
				{
					ShowDatabaseInputSettingsForRow(row, flowItem);
					return;
				}

				TaskConfig task = GetTaskConfig(GetSelectedJobName(), GetSelectedTaskName());
				if (IsImageSourceSelectableRow(row, task))
				{
					ShowImageSourceSelectorForRow(row);
				}

				return;
			}
		}

		private void ShowSignalOutputSettingsForRow(DataGridViewRow row, StepFlowItem flowItem)
		{
			if (row == null || flowItem == null)
			{
				return;
			}

			string protocolName = string.IsNullOrWhiteSpace(flowItem.SignalProtocol)
				? flowItem.CommunicationOutputProtocol
				: flowItem.SignalProtocol;
			string instanceName = string.IsNullOrWhiteSpace(flowItem.SignalInstanceName)
				? flowItem.CommunicationOutputInstanceName
				: flowItem.SignalInstanceName;

			CommunicationConfig communicationConfig = CommunicationConfigStore.LoadOrCreateDefault();
			instanceName = CommunicationRuntimeNaming.NormalizeInstanceName(protocolName, instanceName, communicationConfig);
			List<CommOutputVariable> outputVariables = GetCommunicationOutputVariables(protocolName, instanceName, communicationConfig);

			using (SignalOutputSettingsDialog dialog = new SignalOutputSettingsDialog(
				CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName),
				outputVariables,
				flowItem.SignalOutputs,
				_isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				flowItem.SignalProtocol = protocolName ?? string.Empty;
				flowItem.SignalInstanceName = instanceName ?? string.Empty;
				flowItem.CommunicationOutputProtocol = flowItem.SignalProtocol;
				flowItem.CommunicationOutputInstanceName = flowItem.SignalInstanceName;
				flowItem.SignalOutputs = dialog.GetBindings();
				row.Tag = flowItem;
			}
		}

		private void ShowDatabaseInputSettingsForRow(DataGridViewRow row, StepFlowItem flowItem)
		{
			if (row == null || flowItem == null)
			{
				return;
			}

			DatabaseConfig databaseConfig = DatabaseConfigStore.LoadOrCreateDefault();
			DatabaseConfigStore.Normalize(databaseConfig);

			using (DatabaseInputSettingsDialog dialog = new DatabaseInputSettingsDialog(
				databaseConfig,
				flowItem.DatabaseInputs,
				_isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				flowItem.DatabaseInputs = dialog.GetBindings();
				row.Tag = flowItem;
			}
		}

		private List<CommOutputVariable> GetCommunicationOutputVariables(
			string protocolName,
			string instanceName,
			CommunicationConfig config)
		{
			List<CommOutputVariable> result = new List<CommOutputVariable>();
			if (config == null)
			{
				return result;
			}

			string normalizedProtocol = CommunicationRuntimeNaming.NormalizeProtocolName(protocolName);
			CommunicationInstanceConfig instance =
				CommunicationRuntimeNaming.FindInstance(config, normalizedProtocol, instanceName);

			IEnumerable<CommOutputVariable> outputs = null;
			if (normalizedProtocol.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				outputs = instance != null && instance.TcpIp != null
					? instance.TcpIp.OutputVariables
					: (config.TcpIp == null ? null : config.TcpIp.OutputVariables);
			}
			else if (normalizedProtocol.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				outputs = instance != null && instance.Profinet != null
					? instance.Profinet.OutputVariables
					: (config.Profinet == null ? null : config.Profinet.OutputVariables);
			}
			else if (normalizedProtocol.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				outputs = instance != null && instance.S7 != null
					? instance.S7.OutputVariables
					: (config.S7 == null ? null : config.S7.OutputVariables);
			}

			if (outputs == null)
			{
				return result;
			}

			foreach (CommOutputVariable output in outputs)
			{
				if (output == null || string.IsNullOrWhiteSpace(output.Name))
				{
					continue;
				}

				result.Add(output);
			}

			return result;
		}

		private StepConfig GetStepConfigForRow(DataGridViewRow row, TaskConfig task)
		{
			if (row == null || task == null || task.Steps == null)
			{
				return null;
			}

			StepFlowItem flowItem = row.Tag as StepFlowItem;
			if (flowItem != null && !flowItem.IsStepBlock)
			{
				return null;
			}

			string stepName = GetCellString(row, "colStep");
			if (string.IsNullOrWhiteSpace(stepName))
			{
				return null;
			}

			return task.Steps.FirstOrDefault(s => string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase));
		}

		private bool IsScriptRow(DataGridViewRow row, TaskConfig task)
		{
			StepConfig step = GetStepConfigForRow(row, task);
			return step != null && step.StepType == StepType.Script;
		}

		private string GetFixedDisplayOutputKey(StepType stepType)
		{
			if (stepType == StepType.Vpp)
			{
				return VPP_DISPLAY_OUTPUT_IMAGE;
			}

			if (stepType == StepType.Halcon)
			{
				return HDEV_DISPLAY_OUTPUT_IMAGE;
			}

			return DISPLAY_OUTPUT_NOT_USE;
		}

		private string GetDefaultDisplayResultKey(StepConfig step)
		{
			if (step == null ||
				(step.StepType != StepType.Vpp && step.StepType != StepType.Halcon))
			{
				return DISPLAY_OUTPUT_NOT_USE;
			}

			if (step.OutputPins != null)
			{
				PinConfig exact = step.OutputPins.FirstOrDefault(x =>
					x != null &&
					x.DataType == PinDataType.Bool &&
					string.Equals(x.PinName, DEFAULT_DISPLAY_RESULT_OUTPUT, StringComparison.OrdinalIgnoreCase));
				if (exact != null)
				{
					return exact.PinName;
				}

				PinConfig resultPin = step.OutputPins.FirstOrDefault(x =>
					x != null &&
					x.DataType == PinDataType.Bool &&
					!string.IsNullOrWhiteSpace(x.PinName) &&
					x.PinName.IndexOf("ImageResult", StringComparison.OrdinalIgnoreCase) >= 0);
				if (resultPin != null)
				{
					return resultPin.PinName;
				}
			}

			return DISPLAY_OUTPUT_NOT_USE;
		}

		private string NormalizeDisplayOutputKey(StepConfig step, string value)
		{
			if (step == null)
			{
				return DISPLAY_OUTPUT_NOT_USE;
			}

			string fixedValue = GetFixedDisplayOutputKey(step.StepType);
			if (step.StepType == StepType.Vpp || step.StepType == StepType.Halcon)
			{
				return fixedValue;
			}

			return string.IsNullOrWhiteSpace(value) ? DISPLAY_OUTPUT_NOT_USE : value;
		}

		private string NormalizeDisplayResultKey(StepConfig step, string value)
		{
			if (step == null ||
				(step.StepType != StepType.Vpp && step.StepType != StepType.Halcon))
			{
				return DISPLAY_OUTPUT_NOT_USE;
			}

			return string.IsNullOrWhiteSpace(value) ? GetDefaultDisplayResultKey(step) : value;
		}

		private void ApplyDisplayOutputOptionsToRow(DataGridViewRow row, TaskConfig task)
		{
			if (row == null || !dgvSteps.Columns.Contains(COL_DISPLAY_OUTPUT))
			{
				return;
			}

			DataGridViewComboBoxCell cell = row.Cells[COL_DISPLAY_OUTPUT] as DataGridViewComboBoxCell;
			if (cell == null)
			{
				return;
			}

			StepConfig step = GetStepConfigForRow(row, task);
			string fixedValue = NormalizeDisplayOutputKey(step, GetCellString(row, COL_DISPLAY_OUTPUT));
			cell.Items.Clear();
			cell.Items.Add(DISPLAY_OUTPUT_NOT_USE);

			if (step != null && step.StepType == StepType.Vpp)
			{
				cell.Items.Add(VPP_DISPLAY_OUTPUT_IMAGE);
			}
			else if (step != null && step.StepType == StepType.Halcon)
			{
				cell.Items.Add(HDEV_DISPLAY_OUTPUT_IMAGE);
			}

			cell.Value = fixedValue;
		}

		private void ApplyDisplayResultOptionsToRow(DataGridViewRow row, TaskConfig task)
		{
			if (row == null || !dgvSteps.Columns.Contains(COL_DISPLAY_RESULT))
			{
				return;
			}

			DataGridViewComboBoxCell cell = row.Cells[COL_DISPLAY_RESULT] as DataGridViewComboBoxCell;
			if (cell == null)
			{
				return;
			}

			StepConfig step = GetStepConfigForRow(row, task);
			string value = NormalizeDisplayResultKey(step, GetCellString(row, COL_DISPLAY_RESULT));
			cell.Items.Clear();
			cell.Items.Add(DISPLAY_OUTPUT_NOT_USE);

			if (step != null && (step.StepType == StepType.Vpp || step.StepType == StepType.Halcon))
			{
				if (step.OutputPins != null)
				{
					foreach (PinConfig pin in step.OutputPins)
					{
						if (pin != null && pin.DataType == PinDataType.Bool && !string.IsNullOrWhiteSpace(pin.PinName))
						{
							AddDisplayResultOption(cell, pin.PinName);
						}
					}
				}
			}

			if (!cell.Items.Contains(value))
			{
				cell.Items.Add(value);
			}

			cell.Value = value;
		}

		private void AddDisplayResultOption(DataGridViewComboBoxCell cell, string value)
		{
			if (cell != null && !string.IsNullOrWhiteSpace(value) && !cell.Items.Contains(value))
			{
				cell.Items.Add(value);
			}
		}

		private void ApplyStepFlowRowVisual(DataGridViewRow row, TaskConfig task)
		{
			if (row == null || task == null)
			{
				return;
			}

			if (_applyingFlowVisual)
			{
				return;
			}

			_applyingFlowVisual = true;
			try
			{

			StepFlowItem flowItem = row.Tag as StepFlowItem;
			bool isStepBlock = flowItem == null || flowItem.IsStepBlock;
			bool isScript = IsScriptRow(row, task);
			Color disabledBack = Color.FromArgb(18, 28, 40);
			Color disabledFore = Color.FromArgb(120, 140, 155);
			Color normalBack = Color.FromArgb(1, 8, 16);
			Color normalFore = Color.White;

			if (!isStepBlock)
			{
				SetOptionalCellState(row, "colImageSource", false, string.Empty, disabledBack, disabledFore, normalBack, normalFore);
				ApplyDisplayOutputOptionsToRow(row, task);
				SetOptionalCellState(row, COL_DISPLAY_OUTPUT, false, DISPLAY_OUTPUT_NOT_USE, disabledBack, disabledFore, normalBack, normalFore);
				SetOptionalCellState(row, COL_DISPLAY_SLOT, false, "Not Show", disabledBack, disabledFore, normalBack, normalFore);
				ApplyDisplayResultOptionsToRow(row, task);
				SetOptionalCellState(row, COL_DISPLAY_RESULT, false, DISPLAY_OUTPUT_NOT_USE, disabledBack, disabledFore, normalBack, normalFore);
				return;
			}

			bool imageSourceEnabled = IsImageSourceSelectableRow(row, task) && GetAvailableImageSources(task).Count > 0;
			string displaySlot = GetCellString(row, COL_DISPLAY_SLOT);
			bool displayBound = !isScript &&
				!string.IsNullOrWhiteSpace(displaySlot) &&
				!displaySlot.Equals("Not Show", StringComparison.OrdinalIgnoreCase);

			SetOptionalCellState(row, "colImageSource", imageSourceEnabled, imageSourceEnabled ? null : string.Empty, disabledBack, disabledFore, normalBack, normalFore);
			ApplyDisplayOutputOptionsToRow(row, task);
			SetOptionalCellState(row, COL_DISPLAY_OUTPUT, !isScript, isScript ? DISPLAY_OUTPUT_NOT_USE : null, disabledBack, disabledFore, normalBack, normalFore);
			SetOptionalCellState(row, COL_DISPLAY_SLOT, !isScript, isScript ? "Not Show" : null, disabledBack, disabledFore, normalBack, normalFore);
			ApplyDisplayResultOptionsToRow(row, task);
			SetOptionalCellState(row, COL_DISPLAY_RESULT, displayBound, isScript ? DISPLAY_OUTPUT_NOT_USE : null, disabledBack, disabledFore, normalBack, normalFore);

			if (dgvSteps.Columns.Contains("colImageSource"))
			{
				row.Cells["colImageSource"].ReadOnly = true;
			}
			}
			finally
			{
				_applyingFlowVisual = false;
			}
		}

		private void SetOptionalCellState(DataGridViewRow row, string columnName, bool enabled, string forcedValue,
			Color disabledBack, Color disabledFore, Color normalBack, Color normalFore)
		{
			if (row == null || string.IsNullOrWhiteSpace(columnName) || !dgvSteps.Columns.Contains(columnName))
			{
				return;
			}

			DataGridViewCell cell = row.Cells[columnName];

			if (forcedValue != null)
			{
				cell.Value = forcedValue;
			}

			cell.ReadOnly = !enabled;
			cell.Style.BackColor = enabled ? normalBack : disabledBack;
			cell.Style.ForeColor = enabled ? normalFore : disabledFore;
			cell.Style.SelectionForeColor = Color.White;
			cell.Style.SelectionBackColor = enabled ? Color.FromArgb(0, 120, 200) : Color.FromArgb(45, 60, 75);
		}

		private void ShowImageSourceSelectorForRow(DataGridViewRow row)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();
			TaskConfig task = GetTaskConfig(jobName, taskName);

			if (row == null || task == null)
			{
				return;
			}

			StepFlowItem flowItem = row.Tag as StepFlowItem;
			if (flowItem != null && !flowItem.IsStepBlock)
			{
				return;
			}

			if (IsScriptRow(row, task))
			{
				MessageBox.Show("Script step does not use image source.", "Image Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			StepConfig step = GetStepConfigForRow(row, task);
			if (!IsVisualImageSourceStep(step))
			{
				return;
			}

			List<string> available = GetAvailableImageSources(task);
			if (available.Count <= 0)
			{
				MessageBox.Show("No Hardware image source is configured for current Task.", "Image Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			List<string> selected = ParseSeparatedKeys(GetCellString(row, "colImageSource"));

			using (MultiCheckSelectForm form = new MultiCheckSelectForm("Select Image Sources", "Select one or more image sources", available, selected, null))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					row.Cells["colImageSource"].Value = JoinKeys(form.SelectedItems);
					ApplyStepFlowRowVisual(row, task);
				}
			}
		}

		private void ShowScriptInputStepSelectorForRow(DataGridViewRow row)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();
			TaskConfig task = GetTaskConfig(jobName, taskName);

			if (row == null || task == null)
			{
				return;
			}

			if (!IsScriptRow(row, task))
			{
				return;
			}

			int currentOrder = GetCellInt(row, "colRunOrder", 1);
			string currentStepName = GetCellString(row, "colStep");

			List<SelectableStepSourceItem> items = new List<SelectableStepSourceItem>();

			foreach (DataGridViewRow r in dgvSteps.Rows)
			{
				if (r == null || r.IsNewRow || r == row)
				{
					continue;
				}

				StepFlowItem flowItem = r.Tag as StepFlowItem;
				if (flowItem != null && !flowItem.IsStepBlock)
				{
					continue;
				}

				string stepName = GetCellString(r, "colStep");
				if (string.IsNullOrWhiteSpace(stepName))
				{
					continue;
				}

				int order = GetCellInt(r, "colRunOrder", 1);

				SelectableStepSourceItem item = new SelectableStepSourceItem();
				item.Name = stepName;
				item.DisplayText = stepName + "    RunOrder=" + order.ToString();
				item.Enabled = order < currentOrder;
				item.ToolTip = item.Enabled ? "" : "Only steps with a smaller RunOrder can be used as Script input source.";
				items.Add(item);
			}

			if (items.Count <= 0)
			{
				MessageBox.Show("No previous step can be selected. Script can only receive data from modules with smaller RunOrder.", "Script Input Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			List<string> selected = ParseSeparatedKeys(GetRowScriptInputStepKeys(row));

			using (ScriptInputSourceSelectForm form = new ScriptInputSourceSelectForm("Select Script Input Sources", "Select previous modules used as input objects", items, selected))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					string selectedKeys = JoinKeys(form.SelectedItems);
					SetRowScriptInputStepKeys(row, selectedKeys);
					string remark = GetCellString(row, "colRemark");
					row.Cells["colRemark"].Value = MergeScriptInputRemark(remark, selectedKeys);
				}
			}
		}

		private string MergeScriptInputRemark(string remark, string selectedKeys)
		{
			string prefix = "Script Inputs:";
			string clean = remark ?? string.Empty;
			int index = clean.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
			if (index >= 0)
			{
				clean = clean.Substring(0, index).Trim();
			}

			if (string.IsNullOrWhiteSpace(selectedKeys))
			{
				return clean;
			}

			if (!string.IsNullOrWhiteSpace(clean))
			{
				clean += " | ";
			}

			return clean + prefix + " " + selectedKeys;
		}

		private List<string> GetAvailableImageSources(TaskConfig task)
		{
			List<string> result = new List<string>();

			if (task == null)
			{
				return result;
			}

			AddHardwareImageSourcesFromGrid(result);
			AddHardwareImageSourcesFromTask(result, task);

			foreach (string key in task.ImageSourceKeyList)
			{
				AddImageSourceKeys(result, key);
			}

			return result;
		}

		private void AddHardwareImageSourcesFromGrid(List<string> result)
		{
			if (result == null || dgvSteps == null)
			{
				return;
			}

			foreach (DataGridViewRow row in dgvSteps.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				AddHardwareImageSource(result, row.Tag as StepFlowItem);
			}
		}

		private void AddHardwareImageSourcesFromTask(List<string> result, TaskConfig task)
		{
			if (result == null || task == null || task.StepFlow == null)
			{
				return;
			}

			foreach (StepFlowItem item in task.StepFlow.OrderBy(x => x == null ? int.MaxValue : x.RunOrder))
			{
				AddHardwareImageSource(result, item);
			}
		}

		private void AddHardwareImageSource(List<string> result, StepFlowItem item)
		{
			if (!IsHardwareFlowBlock(item))
			{
				return;
			}

			AddImageSourceKeys(result, GetHardwareFlowImageSourceName(item));
		}

		private bool IsHardwareFlowBlock(StepFlowItem item)
		{
			return item != null &&
				string.Equals(item.BlockType, FLOW_BLOCK_HARDWARE, StringComparison.OrdinalIgnoreCase);
		}

		private string GetHardwareFlowImageSourceName(StepFlowItem item)
		{
			if (item == null)
			{
				return string.Empty;
			}

			if (!string.IsNullOrWhiteSpace(item.BlockName))
			{
				return item.BlockName.Trim();
			}

			if (!string.IsNullOrWhiteSpace(item.StepName))
			{
				return item.StepName.Trim();
			}

			return ConvertHardwarePathToImageSourceName(item.BlockPath);
		}

		private string ConvertHardwarePathToImageSourceName(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			return path.Trim()
				.Replace(Path.DirectorySeparatorChar, '.')
				.Replace(Path.AltDirectorySeparatorChar, '.');
		}

		private bool IsImageSourceSelectableRow(DataGridViewRow row, TaskConfig task)
		{
			if (row == null || task == null)
			{
				return false;
			}

			StepFlowItem flowItem = row.Tag as StepFlowItem;
			if (flowItem != null && !flowItem.IsStepBlock)
			{
				return false;
			}

			return IsVisualImageSourceStep(GetStepConfigForRow(row, task));
		}

		private bool IsVisualImageSourceStep(StepConfig step)
		{
			return step != null &&
				(step.StepType == StepType.Vpp || step.StepType == StepType.Halcon);
		}

		private bool IsImageSourceNotUse(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return true;
			}

			string clean = value.Trim();
			return clean.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				clean.Equals("None", StringComparison.OrdinalIgnoreCase);
		}

		private void AddImageSourceKeys(List<string> list, string text)
		{
			if (list == null || string.IsNullOrWhiteSpace(text))
			{
				return;
			}

			string[] parts = text.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string part in parts)
			{
				AddUniqueKey(list, part);
			}
		}

		private void AddUniqueKey(List<string> list, string key)
		{
			if (list == null || string.IsNullOrWhiteSpace(key))
			{
				return;
			}

			key = key.Trim();

			if (key.Equals("Not Use", StringComparison.OrdinalIgnoreCase) || key.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (!list.Any(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase)))
			{
				list.Add(key);
			}
		}

		private List<string> ParseSeparatedKeys(string text)
		{
			List<string> result = new List<string>();

			if (string.IsNullOrWhiteSpace(text))
			{
				return result;
			}

			string[] parts = text.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string part in parts)
			{
				AddUniqueKey(result, part);
			}

			return result;
		}

		private string JoinKeys(List<string> keys)
		{
			if (keys == null || keys.Count <= 0)
			{
				return string.Empty;
			}

			return string.Join(";", keys.ToArray());
		}

		private bool IsHdevModuleEnabled()
		{
			try
			{
				AlgorithmModuleConfig config = AlgorithmModuleConfigStore.LoadOrCreateDefault();
				return config != null && config.EnableHdev;
			}
			catch
			{
				return false;
			}
		}

		private string GetRowScriptInputStepKeys(DataGridViewRow row)
		{
			if (row == null)
			{
				return string.Empty;
			}

			StepFlowItem item = row.Tag as StepFlowItem;
			if (item != null)
			{
				return item.ScriptInputStepKeys ?? string.Empty;
			}

			return string.Empty;
		}

		private void SetRowScriptInputStepKeys(DataGridViewRow row, string keys)
		{
			if (row == null)
			{
				return;
			}

			StepFlowItem item = row.Tag as StepFlowItem;
			if (item == null)
			{
				item = new StepFlowItem();
				row.Tag = item;
			}

			item.ScriptInputStepKeys = keys ?? string.Empty;
		}

		private void MakeStepNameColumnReadOnly()
		{
			if (dgvSteps == null)
			{
				return;
			}

			if (!dgvSteps.Columns.Contains("colStep"))
			{
				return;
			}

			DataGridViewColumn col = dgvSteps.Columns["colStep"];

			col.ReadOnly = true;
			col.SortMode = DataGridViewColumnSortMode.NotSortable;

			col.DefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			col.DefaultCellStyle.ForeColor = Color.FromArgb(210, 230, 245);
			col.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			col.DefaultCellStyle.SelectionForeColor = Color.White;
		}

		private void BindStepGridReadOnlyEvents()
		{
			if (dgvSteps == null)
			{
				return;
			}

			dgvSteps.CellBeginEdit -= dgvSteps_CellBeginEdit;
			dgvSteps.CellBeginEdit += dgvSteps_CellBeginEdit;
		}

		private void dgvSteps_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}

			string columnName = dgvSteps.Columns[e.ColumnIndex].Name;

			if (columnName == "colStep" || columnName == "colImageSource")
			{
				e.Cancel = true;
			}
		}

		#endregion



		#region Helper

		private string GetSelectedJobName()
		{
			ProgramListItem item = GetSelectedProgramItem();
			return item == null ? string.Empty : item.JobName;
		}

		private string GetSelectedProtocolName()
		{
			ProgramListItem item = GetSelectedProgramItem();
			return item == null ? string.Empty : item.ProtocolName;
		}

		private string GetSelectedChannelName()
		{
			ProgramListItem item = GetSelectedProgramItem();
			return item == null ? string.Empty : item.ChannelName;
		}

		private string GetSelectedProgramNo()
		{
			ProgramListItem item = GetSelectedProgramItem();
			return item == null ? string.Empty : item.ProgramNo;
		}

		private ProgramListItem GetSelectedProgramItem()
		{
			return listJobs == null ? null : listJobs.SelectedItem as ProgramListItem;
		}

		private string GetSelectedTaskName()
		{
			return listTasks.SelectedItem == null ? string.Empty : listTasks.SelectedItem.ToString();
		}

		private string GetSelectedStepName()
		{
			if (listSteps.SelectedItem == null)
			{
				return string.Empty;
			}

			StepListItem item = listSteps.SelectedItem as StepListItem;
			FunctionBlockListItem blockItem = listSteps.SelectedItem as FunctionBlockListItem;

			if (item != null)
			{
				return item.StepName;
			}

			if (blockItem != null)
			{
				return blockItem.Name;
			}

			return listSteps.SelectedItem.ToString();
		}

		private FunctionBlockListItem GetSelectedFunctionBlockItem()
		{
			if (listSteps == null || listSteps.SelectedItem == null)
			{
				return null;
			}

			FunctionBlockListItem blockItem = listSteps.SelectedItem as FunctionBlockListItem;
			if (blockItem != null)
			{
				return blockItem;
			}

			StepListItem stepItem = listSteps.SelectedItem as StepListItem;
			if (stepItem != null)
			{
				return new FunctionBlockListItem(FLOW_BLOCK_STEP, stepItem.StepName, stepItem.DisplayText, stepItem.IsMissing);
			}

			string text = listSteps.SelectedItem.ToString();
			return new FunctionBlockListItem(FLOW_BLOCK_STEP, text, text, false);
		}


		private TaskConfig GetTaskConfig(string jobName, string taskName)
		{
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			return GetTaskConfig(config, jobName, taskName);
		}

		private TaskConfig GetTaskConfig(ProjectFlowConfig config, string jobName, string taskName)
		{
			JobConfig job = null;
			ProgramListItem selectedProgram = GetSelectedProgramItem();
			if (selectedProgram != null &&
				string.Equals(selectedProgram.JobName, jobName, StringComparison.OrdinalIgnoreCase))
			{
				job = FlowConfigStore.GetJobs(config, selectedProgram.ProtocolName, selectedProgram.ChannelName)
					.FirstOrDefault(j => j != null && string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));
			}

			if (job == null)
			{
				job = EnumerateJobContexts(config)
					.Select(x => x.Job)
					.FirstOrDefault(j =>
						j != null &&
						string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase) &&
						j.Tasks != null &&
						j.Tasks.Any(t => t != null && string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase)));
			}

			if (job == null || job.Tasks == null) return null;

			return job.Tasks.FirstOrDefault(t => string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase));
		}

		private IEnumerable<JobContext> EnumerateJobContexts(ProjectFlowConfig config)
		{
			if (config == null || config.Protocols == null)
			{
				yield break;
			}

			foreach (ProtocolFlowConfig protocol in config.Protocols)
			{
				if (protocol == null || protocol.Channels == null)
				{
					continue;
				}

				string protocolName = FlowConfigStore.NormalizeProtocolName(protocol.ProtocolName);
				foreach (ChannelFlowConfig channel in protocol.Channels)
				{
					if (channel == null || channel.Jobs == null)
					{
						continue;
					}

					string channelName = FlowConfigStore.NormalizeChannelName(channel.ChannelName);
					foreach (JobConfig job in channel.Jobs)
					{
						if (job == null)
						{
							continue;
						}

						job.ProtocolName = protocolName;
						job.ChannelName = channelName;
						yield return new JobContext(protocolName, channelName, job);
					}
				}
			}
		}

		private void SelectFlowGridRowByFlowItemId(string flowItemId)
		{
			if (dgvSteps == null || string.IsNullOrWhiteSpace(flowItemId))
			{
				return;
			}

			for (int i = 0; i < dgvSteps.Rows.Count; i++)
			{
				StepFlowItem item = dgvSteps.Rows[i].Tag as StepFlowItem;
				if (item != null &&
					string.Equals(item.FlowItemId, flowItemId, StringComparison.OrdinalIgnoreCase))
				{
					SelectFlowGridRowByIndex(i);
					return;
				}
			}
		}

		private void SelectFlowGridRowByIndex(int rowIndex)
		{
			if (dgvSteps == null || rowIndex < 0 || rowIndex >= dgvSteps.Rows.Count)
			{
				return;
			}

			dgvSteps.ClearSelection();
			dgvSteps.Rows[rowIndex].Selected = true;
			if (dgvSteps.Rows[rowIndex].Cells.Count > 0)
			{
				dgvSteps.CurrentCell = dgvSteps.Rows[rowIndex].Cells[0];
			}
		}

		private void ReorderTasks(JobConfig job)
		{
			for (int i = 0; i < job.Tasks.Count; i++)
			{
				job.Tasks[i].RunOrder = i + 1;
			}
		}

		private void ReorderStepLibrary(TaskConfig task)
		{
			for (int i = 0; i < task.Steps.Count; i++)
			{
				task.Steps[i].RunOrder = i + 1;
			}
		}

		private void SelectListItem(ListBox listBox, string itemText)
		{
			if (listBox == null || string.IsNullOrEmpty(itemText)) return;

			for (int i = 0; i < listBox.Items.Count; i++)
			{
				StepListItem stepItem = listBox.Items[i] as StepListItem;
				ProgramListItem programItem = listBox.Items[i] as ProgramListItem;
				FunctionBlockListItem blockItem = listBox.Items[i] as FunctionBlockListItem;

				if (stepItem != null)
				{
					if (string.Equals(stepItem.StepName, itemText, StringComparison.OrdinalIgnoreCase) ||
						string.Equals(stepItem.DisplayText, itemText, StringComparison.OrdinalIgnoreCase))
					{
						listBox.SelectedIndex = i;
						return;
					}
				}
				else if (blockItem != null)
				{
					if (string.Equals(blockItem.Name, itemText, StringComparison.OrdinalIgnoreCase) ||
						string.Equals(blockItem.DisplayText, itemText, StringComparison.OrdinalIgnoreCase))
					{
						listBox.SelectedIndex = i;
						return;
					}
				}
				else if (programItem != null)
				{
					if (string.Equals(programItem.JobName, itemText, StringComparison.OrdinalIgnoreCase) ||
						string.Equals(programItem.ProgramNo, itemText, StringComparison.OrdinalIgnoreCase) ||
						string.Equals(programItem.DisplayText, itemText, StringComparison.OrdinalIgnoreCase))
					{
						listBox.SelectedIndex = i;
						return;
					}
				}
				else
				{
					if (string.Equals(listBox.Items[i].ToString(), itemText, StringComparison.OrdinalIgnoreCase))
					{
						listBox.SelectedIndex = i;
						return;
					}
				}
			}

			if (listBox.Items.Count > 0 && listBox.SelectedIndex < 0)
			{
				listBox.SelectedIndex = 0;
			}
		}


		private string GetCellString(DataGridViewRow row, int columnIndex)
		{
			if (row.Cells[columnIndex].Value == null) return string.Empty;
			return row.Cells[columnIndex].Value.ToString().Trim();
		}

		private int GetCellInt(DataGridViewRow row, int columnIndex, int defaultValue)
		{
			int value;
			if (int.TryParse(GetCellString(row, columnIndex), out value)) return value;
			return defaultValue;
		}

		private void UpdateStepDetailTitle()
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName)) jobName = "---";
			if (string.IsNullOrEmpty(taskName)) taskName = "---";

			lblStepsTitle.Text = "当前 Task:  " + taskName + "    程序号:  " + jobName;
		}

		private int ParseProgramNo(string programNo)
		{
			int value;
			return int.TryParse(programNo, out value) ? value : int.MaxValue;
		}

		private class StepListItem
		{
			public string StepName { get; private set; }
			public string DisplayText { get; private set; }
			public bool IsMissing { get; private set; }

			public StepListItem(string stepName, string displayText, bool isMissing)
			{
				StepName = stepName;
				DisplayText = displayText;
				IsMissing = isMissing;
			}

			public override string ToString()
			{
				return DisplayText;
			}
		}

		private class FunctionBlockListItem
		{
			public string BlockType { get; private set; }
			public string Name { get; private set; }
			public string DisplayText { get; private set; }
			public bool IsMissing { get; private set; }
			public string FilePath { get; set; }
			public string RelativePath { get; set; }
			public string Protocol { get; set; }
			public string InstanceName { get; set; }

			public FunctionBlockListItem(string blockType, string name, string displayText, bool isMissing)
			{
				BlockType = string.IsNullOrWhiteSpace(blockType) ? FLOW_BLOCK_STEP : blockType;
				Name = name ?? string.Empty;
				DisplayText = string.IsNullOrWhiteSpace(displayText) ? Name : displayText;
				IsMissing = isMissing;
				FilePath = string.Empty;
				RelativePath = string.Empty;
				Protocol = string.Empty;
				InstanceName = string.Empty;
			}

			public override string ToString()
			{
				return DisplayText;
			}
		}

		private class FunctionBlockModeOption
		{
			public FunctionBlockLibraryMode Mode { get; private set; }
			public string Text { get; private set; }

			public FunctionBlockModeOption(FunctionBlockLibraryMode mode, string text)
			{
				Mode = mode;
				Text = text ?? string.Empty;
			}

			public override string ToString()
			{
				return Text;
			}
		}

		private class JobContext
		{
			public string ProtocolName { get; private set; }
			public string ChannelName { get; private set; }
			public JobConfig Job { get; private set; }

			public JobContext(string protocolName, string channelName, JobConfig job)
			{
				ProtocolName = protocolName;
				ChannelName = channelName;
				Job = job;
			}
		}

		private class ProgramListItem
		{
			public string ProtocolName { get; private set; }
			public string ChannelName { get; private set; }
			public string JobName { get; private set; }
			public string ProgramNo { get; private set; }
			public string DisplayText { get; private set; }

			public ProgramListItem(JobContext context)
			{
				ProtocolName = context.ProtocolName;
				ChannelName = context.ChannelName;
				JobName = context.Job == null ? string.Empty : context.Job.JobName;
				ProgramNo = context.Job == null ? string.Empty : context.Job.ProgramNo;
				DisplayText = string.IsNullOrWhiteSpace(JobName) ? ProgramNo : JobName;
			}

			public override string ToString()
			{
				return DisplayText;
			}
		}


		#endregion

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;

			if (isEnglish)
			{
				lblJobsTitle.Text = "All Program";
				lblTasksTitle.Text = "All Task";
				lblStepListTitle.Text = "All Step";
				UpdateStepDetailTitle();

				colStep.HeaderText = "Step";
				colImageSource.HeaderText = "Image Source";
				colRunOrder.HeaderText = "Run Order";
				colRemark.HeaderText = "Remark";

				btnAddStep.Text = "Add Selected";
				btnDeleteSelected.Text = "▦  Delete";
				HideMoveButtons();
				btnSave.Text = "▣  Save";
				PopulateFunctionBlockModeCombo();
			}
			else
			{
				lblJobsTitle.Text = "所有 程序号";
				lblTasksTitle.Text = "所有 task";
				lblStepListTitle.Text = "所有 step";
				UpdateStepDetailTitle();

				colStep.HeaderText = "step";
				colImageSource.HeaderText = "图像源";
				colRunOrder.HeaderText = "执行步序";
				colRemark.HeaderText = "备注";

				btnAddStep.Text = "添加选中";
				btnDeleteSelected.Text = "▦  删除选中";
				HideMoveButtons();
				btnSave.Text = "▣  保存";
				PopulateFunctionBlockModeCombo();
			}
			ApplyFunctionBlockLibraryModeVisual();
			if (dgvSteps.Columns.Contains(COL_DISPLAY_OUTPUT))
			{
				dgvSteps.Columns[COL_DISPLAY_OUTPUT].HeaderText = isEnglish ? "Output Image" : "输出图像";
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
			{
				dgvSteps.Columns[COL_DISPLAY_SLOT].HeaderText = isEnglish ? "Display Slot" : "绑定显示框";
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_RESULT))
			{
				dgvSteps.Columns[COL_DISPLAY_RESULT].HeaderText = isEnglish ? "Image Result" : "图像输出绑定";
			}

			btnAddStepItem.Text = "+";
			btnBatchAddStepItem.Text = "▦";
			btnDeleteStepItem.Text = "-";
			btnRefreshStepItem.Text = "↻";
			if (btnOpenStepFolder != null) btnOpenStepFolder.Text = "📁";
		}

		// 新增 GetStepDisplayText 方法。
		// 作用：根据 StepConfig 生成列表显示文本。
		private string GetStepDisplayText(StepConfig step)
		{
			if (step == null)
			{
				return string.Empty;
			}

			string fileName = string.Empty;

			// 1. 优先显示原始导入文件名
			if (!string.IsNullOrEmpty(step.SourceFilePath))
			{
				fileName = Path.GetFileName(step.SourceFilePath);
			}

			// 2. 如果原始路径没有，就显示 Project 内部 VPP 文件名
			if (string.IsNullOrEmpty(fileName) &&
				step.StepType == StepType.Vpp &&
				step.VppFiles != null &&
				step.VppFiles.Count > 0)
			{
				fileName = Path.GetFileName(step.VppFiles[0]);
			}

			// 3. 如果是 Script，就显示 Project 内部 Script 文件名
			if (string.IsNullOrEmpty(fileName) &&
				step.StepType == StepType.Script &&
				step.ScriptFiles != null &&
				step.ScriptFiles.Count > 0)
			{
				fileName = Path.GetFileName(step.ScriptFiles[0]);
			}

			if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(step.ProjectFilePath))
			{
				fileName = Path.GetFileName(step.ProjectFilePath);
			}

			// 4. 兜底：如果没有文件路径，则根据 StepType 添加后缀提示
			if (string.IsNullOrEmpty(fileName))
			{
				if (step.StepType == StepType.Vpp)
				{
					return step.StepName + ".vpp";
				}

				if (step.StepType == StepType.Script)
				{
					return step.StepName + ".csx";
				}

				if (step.StepType == StepType.Halcon)
				{
					return step.StepName + ".hdev";
				}

				return step.StepName;
			}

			return fileName;
		}


	}


	public class SelectableStepSourceItem
	{
		public string Name { get; set; }
		public string DisplayText { get; set; }
		public bool Enabled { get; set; }
		public string ToolTip { get; set; }

		public SelectableStepSourceItem()
		{
			Name = string.Empty;
			DisplayText = string.Empty;
			Enabled = true;
			ToolTip = string.Empty;
		}

	}

	public class SignalOutputSettingsDialog : Form
	{
		private readonly List<CommOutputVariable> _outputVariables;
		private readonly List<SignalOutputBinding> _existingBindings;
		private readonly bool _isEnglish;
		private DataGridView dgvOutputs;
		private Button btnOK;
		private Button btnCancel;

		public SignalOutputSettingsDialog(
			string communicationName,
			List<CommOutputVariable> outputVariables,
			List<SignalOutputBinding> existingBindings,
			bool isEnglish)
		{
			_outputVariables = outputVariables ?? new List<CommOutputVariable>();
			_existingBindings = existingBindings ?? new List<SignalOutputBinding>();
			_isEnglish = isEnglish;
			InitializeUi(communicationName);
			LoadRows();
		}

		public List<SignalOutputBinding> GetBindings()
		{
			List<SignalOutputBinding> result = new List<SignalOutputBinding>();
			if (dgvOutputs == null)
			{
				return result;
			}

			dgvOutputs.EndEdit();

			foreach (DataGridViewRow row in dgvOutputs.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				string outputName = GetCellString(row, "colOutputName");
				if (string.IsNullOrWhiteSpace(outputName))
				{
					continue;
				}

				result.Add(new SignalOutputBinding
				{
					OutputName = outputName,
					AssignedValue = GetCellString(row, "colAssignedValue"),
					ForceValue = GetCellBool(row, "colForce"),
					Enabled = GetCellBool(row, "colEnabled")
				});
			}

			return result;
		}

		private void InitializeUi(string communicationName)
		{
			Text = _isEnglish ? "Signal Output" : "Signal 输出设置";
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(720, 520);
			MinimumSize = new Size(720, 520);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			Font = new Font("Microsoft YaHei UI", 9F);
			DoubleBuffered = true;

			Label lblTitle = new Label();
			lblTitle.Text = (_isEnglish ? "Communication: " : "通讯实例: ") + (communicationName ?? string.Empty);
			lblTitle.Location = new Point(24, 18);
			lblTitle.Size = new Size(650, 28);
			lblTitle.ForeColor = Color.White;
			lblTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);

			dgvOutputs = new BufferedDataGridView();
			dgvOutputs.Location = new Point(24, 58);
			dgvOutputs.Size = new Size(660, 370);
			dgvOutputs.AllowUserToAddRows = false;
			dgvOutputs.AllowUserToDeleteRows = false;
			dgvOutputs.RowHeadersVisible = false;
			dgvOutputs.MultiSelect = false;
			dgvOutputs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvOutputs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvOutputs.BackgroundColor = Color.FromArgb(2, 10, 20);
			dgvOutputs.GridColor = Color.FromArgb(45, 70, 95);
			dgvOutputs.BorderStyle = BorderStyle.FixedSingle;
			dgvOutputs.EnableHeadersVisualStyles = false;
			dgvOutputs.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			dgvOutputs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgvOutputs.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvOutputs.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			dgvOutputs.DefaultCellStyle.BackColor = Color.FromArgb(2, 10, 20);
			dgvOutputs.DefaultCellStyle.ForeColor = Color.White;
			dgvOutputs.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			dgvOutputs.DefaultCellStyle.SelectionForeColor = Color.White;
			dgvOutputs.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvOutputs.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };
			dgvOutputs.CurrentCellDirtyStateChanged += dgvOutputs_CurrentCellDirtyStateChanged;
			dgvOutputs.CellValueChanged += dgvOutputs_CellValueChanged;

			DataGridViewTextBoxColumn nameColumn = CreateTextColumn("colOutputName", _isEnglish ? "Output" : "输出项", true, 170);
			DataGridViewTextBoxColumn typeColumn = CreateTextColumn("colDataType", _isEnglish ? "Type" : "类型", true, 100);
			DataGridViewTextBoxColumn valueColumn = CreateTextColumn("colAssignedValue", _isEnglish ? "Assigned Value" : "赋值", false, 230);
			DataGridViewCheckBoxColumn forceColumn = new DataGridViewCheckBoxColumn();
			forceColumn.Name = "colForce";
			forceColumn.HeaderText = _isEnglish ? "Force" : "强制";
			forceColumn.FillWeight = 75;
			DataGridViewCheckBoxColumn enabledColumn = new DataGridViewCheckBoxColumn();
			enabledColumn.Name = "colEnabled";
			enabledColumn.HeaderText = _isEnglish ? "Output" : "是否输出";
			enabledColumn.FillWeight = 90;

			dgvOutputs.Columns.Add(nameColumn);
			dgvOutputs.Columns.Add(typeColumn);
			dgvOutputs.Columns.Add(valueColumn);
			dgvOutputs.Columns.Add(forceColumn);
			dgvOutputs.Columns.Add(enabledColumn);
			SetDoubleBuffered(dgvOutputs);

			btnOK = CreateButton(_isEnglish ? "OK" : "确定", 438, 448, true);
			btnCancel = CreateButton(_isEnglish ? "Cancel" : "取消", 574, 448, false);
			btnOK.DialogResult = DialogResult.OK;
			btnCancel.DialogResult = DialogResult.Cancel;
			AcceptButton = btnOK;
			CancelButton = btnCancel;

			Controls.Add(lblTitle);
			Controls.Add(dgvOutputs);
			Controls.Add(btnOK);
			Controls.Add(btnCancel);
		}

		private void dgvOutputs_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (dgvOutputs != null && dgvOutputs.IsCurrentCellDirty)
			{
				dgvOutputs.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private void dgvOutputs_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvOutputs == null)
			{
				return;
			}

			string columnName = dgvOutputs.Columns[e.ColumnIndex].Name;
			if (string.Equals(columnName, "colForce", StringComparison.OrdinalIgnoreCase))
			{
				ApplyAssignedValueCellState(dgvOutputs.Rows[e.RowIndex]);
			}
		}

		private DataGridViewTextBoxColumn CreateTextColumn(string name, string header, bool readOnly, int fillWeight)
		{
			DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
			column.Name = name;
			column.HeaderText = header;
			column.ReadOnly = readOnly;
			column.FillWeight = fillWeight;
			return column;
		}

		private Button CreateButton(string text, int x, int y, bool primary)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(110, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private void LoadRows()
		{
			BeginUpdateControl(dgvOutputs);
			dgvOutputs.SuspendLayout();
			try
			{
			Dictionary<string, SignalOutputBinding> bindingMap =
				new Dictionary<string, SignalOutputBinding>(StringComparer.OrdinalIgnoreCase);
			foreach (SignalOutputBinding binding in _existingBindings)
			{
				if (binding == null || string.IsNullOrWhiteSpace(binding.OutputName))
				{
					continue;
				}

				bindingMap[binding.OutputName.Trim()] = binding;
			}

			foreach (CommOutputVariable output in _outputVariables)
			{
				if (output == null || string.IsNullOrWhiteSpace(output.Name))
				{
					continue;
				}

				SignalOutputBinding binding;
				bindingMap.TryGetValue(output.Name.Trim(), out binding);

				int rowIndex = dgvOutputs.Rows.Add();
				DataGridViewRow row = dgvOutputs.Rows[rowIndex];
				row.Cells["colOutputName"].Value = output.Name;
				row.Cells["colDataType"].Value = output.DataType.ToString();
				row.Cells["colAssignedValue"].Value = binding == null ? string.Empty : binding.AssignedValue;
				row.Cells["colForce"].Value = binding != null && binding.ForceValue;
				row.Cells["colEnabled"].Value = binding != null && binding.Enabled;
				row.Tag = output;
				ApplyAssignedValueCellState(row);
			}
			}
			finally
			{
				dgvOutputs.ResumeLayout();
				EndUpdateControl(dgvOutputs);
			}
		}

		private void ApplyAssignedValueCellState(DataGridViewRow row)
		{
			if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains("colAssignedValue"))
			{
				return;
			}

			bool forceValue = GetCellBool(row, "colForce");
			DataGridViewCell cell = row.Cells["colAssignedValue"];
			cell.ReadOnly = !forceValue;
			cell.Style.BackColor = forceValue ? Color.FromArgb(2, 10, 20) : Color.FromArgb(18, 28, 40);
			cell.Style.ForeColor = forceValue ? Color.White : Color.FromArgb(120, 140, 155);
			cell.Style.SelectionBackColor = forceValue ? Color.FromArgb(0, 120, 200) : Color.FromArgb(45, 60, 75);
			cell.Style.SelectionForeColor = Color.White;
		}

		private void SetDoubleBuffered(Control control)
		{
			if (control == null)
			{
				return;
			}

			try
			{
				System.Reflection.PropertyInfo property = typeof(Control).GetProperty(
					"DoubleBuffered",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic);

				if (property != null)
				{
					property.SetValue(control, true, null);
				}
			}
			catch
			{
			}
		}

		private void BeginUpdateControl(Control control)
		{
			if (control == null || control.IsDisposed)
			{
				return;
			}

			try
			{
				NativeMethods.SendMessage(control.Handle, NativeMethods.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
			}
			catch
			{
			}
		}

		private void EndUpdateControl(Control control)
		{
			if (control == null || control.IsDisposed)
			{
				return;
			}

			try
			{
				NativeMethods.SendMessage(control.Handle, NativeMethods.WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
				control.Invalidate(true);
				control.Update();
			}
			catch
			{
			}
		}

		private static class NativeMethods
		{
			public const int WM_SETREDRAW = 0x000B;

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
		}

		private class BufferedDataGridView : DataGridView
		{
			public BufferedDataGridView()
			{
				DoubleBuffered = true;
			}
		}

		private string GetCellString(DataGridViewRow row, string columnName)
		{
			if (row == null ||
				row.DataGridView == null ||
				string.IsNullOrWhiteSpace(columnName) ||
				!row.DataGridView.Columns.Contains(columnName))
			{
				return string.Empty;
			}

			object value = row.Cells[columnName].Value;
			return value == null ? string.Empty : value.ToString().Trim();
		}

		private bool GetCellBool(DataGridViewRow row, string columnName)
		{
			if (row == null ||
				row.DataGridView == null ||
				string.IsNullOrWhiteSpace(columnName) ||
				!row.DataGridView.Columns.Contains(columnName))
			{
				return false;
			}

			object value = row.Cells[columnName].Value;
			if (value == null)
			{
				return false;
			}

			bool parsed;
			return bool.TryParse(value.ToString(), out parsed) && parsed;
		}
	}

	public class DatabaseInputSettingsDialog : Form
	{
		private readonly DatabaseConfig _databaseConfig;
		private readonly List<DatabaseInputBinding> _existingBindings;
		private readonly bool _isEnglish;
		private DataGridView dgvInputs;
		private Button btnOK;
		private Button btnCancel;

		public DatabaseInputSettingsDialog(
			DatabaseConfig databaseConfig,
			List<DatabaseInputBinding> existingBindings,
			bool isEnglish)
		{
			_databaseConfig = databaseConfig ?? DatabaseConfigStore.LoadOrCreateDefault();
			DatabaseConfigStore.Normalize(_databaseConfig);
			_existingBindings = existingBindings ?? new List<DatabaseInputBinding>();
			_isEnglish = isEnglish;
			InitializeUi();
			LoadRows();
		}

		public List<DatabaseInputBinding> GetBindings()
		{
			List<DatabaseInputBinding> result = new List<DatabaseInputBinding>();
			if (dgvInputs == null)
			{
				return result;
			}

			CommitInputGridEdits();
			foreach (DataGridViewRow row in dgvInputs.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				string inputName = GetCellString(row, "colInputName");
				if (string.IsNullOrWhiteSpace(inputName))
				{
					continue;
				}

				result.Add(new DatabaseInputBinding
				{
					InputName = inputName,
					GlobalVariableName = GlobalVariableBindingUi.GetCellValue(row, "colGlobalVariable"),
					AssignedValue = GetCellString(row, "colAssignedValue"),
					ForceValue = GetCellBool(row, "colForce"),
					Enabled = GetCellBool(row, "colEnabled")
				});
			}

			return result;
		}

		private void InitializeUi()
		{
			Text = T("Database 输入设置", "Database Input Settings");
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(860, 540);
			MinimumSize = new Size(780, 500);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			Font = new Font("Microsoft YaHei UI", 9F);
			DoubleBuffered = true;

			Label lblTitle = new Label();
			lblTitle.Text = T("数据库表: ", "Table: ") + (_databaseConfig.TableName ?? string.Empty);
			lblTitle.Location = new Point(24, 18);
			lblTitle.Size = new Size(780, 28);
			lblTitle.ForeColor = Color.White;
			lblTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);

			dgvInputs = new BufferedDataGridView();
			dgvInputs.Location = new Point(24, 58);
			dgvInputs.Size = new Size(810, 370);
			dgvInputs.AllowUserToAddRows = false;
			dgvInputs.AllowUserToDeleteRows = false;
			dgvInputs.RowHeadersVisible = false;
			dgvInputs.MultiSelect = false;
			dgvInputs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvInputs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvInputs.BackgroundColor = Color.FromArgb(2, 10, 20);
			dgvInputs.GridColor = Color.FromArgb(45, 70, 95);
			dgvInputs.BorderStyle = BorderStyle.FixedSingle;
			dgvInputs.EnableHeadersVisualStyles = false;
			dgvInputs.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			dgvInputs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgvInputs.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvInputs.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			dgvInputs.DefaultCellStyle.BackColor = Color.FromArgb(2, 10, 20);
			dgvInputs.DefaultCellStyle.ForeColor = Color.White;
			dgvInputs.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			dgvInputs.DefaultCellStyle.SelectionForeColor = Color.White;
			dgvInputs.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvInputs.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };
			dgvInputs.CurrentCellDirtyStateChanged += dgvInputs_CurrentCellDirtyStateChanged;
			dgvInputs.CellValueChanged += dgvInputs_CellValueChanged;
			dgvInputs.CellContentClick += dgvInputs_CellContentClick;

			dgvInputs.Columns.Add(CreateCheckColumn("colEnabled", T("写入", "Write"), 65));
			dgvInputs.Columns.Add(CreateTextColumn("colInputName", T("输入名称", "Input"), true, 140));
			dgvInputs.Columns.Add(CreateTextColumn("colDataFormat", T("数据格式", "Format"), true, 90));
			dgvInputs.Columns.Add(GlobalVariableBindingUi.CreateButtonColumn("colGlobalVariable", T("全局变量", "Global Variable"), 170));
			dgvInputs.Columns.Add(CreateCheckColumn("colForce", T("强制", "Force"), 65));
			dgvInputs.Columns.Add(CreateTextColumn("colAssignedValue", T("手动赋值", "Manual Value"), false, 150));
			dgvInputs.Columns.Add(CreateTextColumn("colRemark", T("备注", "Remark"), true, 160));
			SetDoubleBuffered(dgvInputs);

			Label lblHint = new Label();
			lblHint.Text = T("未勾选强制时，运行时读取绑定全局变量；未绑定时尝试读取同名运行数据。", "Without Force, runtime reads the bound global variable; if empty it tries runtime data with the same name.");
			lblHint.Location = new Point(24, 438);
			lblHint.Size = new Size(620, 24);
			lblHint.ForeColor = Color.FromArgb(170, 205, 225);

			btnOK = CreateButton(T("确定", "OK"), 592, 462, true);
			btnCancel = CreateButton(T("取消", "Cancel"), 724, 462, false);
			btnOK.DialogResult = DialogResult.OK;
			btnCancel.DialogResult = DialogResult.Cancel;
			btnOK.Click += btnOK_Click;
			AcceptButton = btnOK;
			CancelButton = btnCancel;

			Controls.Add(lblTitle);
			Controls.Add(dgvInputs);
			Controls.Add(lblHint);
			Controls.Add(btnOK);
			Controls.Add(btnCancel);
		}

		private DataGridViewTextBoxColumn CreateTextColumn(string name, string header, bool readOnly, int fillWeight)
		{
			DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
			column.Name = name;
			column.HeaderText = header;
			column.ReadOnly = readOnly;
			column.FillWeight = fillWeight;
			return column;
		}

		private DataGridViewCheckBoxColumn CreateCheckColumn(string name, string header, int fillWeight)
		{
			DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();
			column.Name = name;
			column.HeaderText = header;
			column.FillWeight = fillWeight;
			return column;
		}

		private Button CreateButton(string text, int x, int y, bool primary)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(110, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private void LoadRows()
		{
			Dictionary<string, DatabaseInputBinding> bindingMap =
				new Dictionary<string, DatabaseInputBinding>(StringComparer.OrdinalIgnoreCase);
			foreach (DatabaseInputBinding binding in _existingBindings)
			{
				if (binding == null || string.IsNullOrWhiteSpace(binding.InputName))
				{
					continue;
				}

				bindingMap[binding.InputName.Trim()] = binding;
			}

			HashSet<string> globalNames = new HashSet<string>(
				GlobalVariableStore.GetVariableNames(),
				StringComparer.OrdinalIgnoreCase);

			foreach (DatabaseFieldConfig field in _databaseConfig.Fields.Where(x => x != null && x.Enabled))
			{
				DatabaseInputBinding binding;
				bindingMap.TryGetValue(field.InputName, out binding);

				int rowIndex = dgvInputs.Rows.Add();
				DataGridViewRow row = dgvInputs.Rows[rowIndex];
				row.Cells["colEnabled"].Value = binding == null ? true : binding.Enabled;
				row.Cells["colInputName"].Value = field.InputName;
				row.Cells["colDataFormat"].Value = GetDatabaseFormatName(field.DataFormat);
				GlobalVariableBindingUi.SetCellValue(
					row,
					"colGlobalVariable",
					binding == null
						? (globalNames.Contains(field.InputName) ? field.InputName : string.Empty)
						: binding.GlobalVariableName);
				row.Cells["colForce"].Value = binding != null && binding.ForceValue;
				row.Cells["colAssignedValue"].Value = binding == null ? (field.DefaultValue ?? string.Empty) : binding.AssignedValue;
				row.Cells["colRemark"].Value = field.Remark ?? string.Empty;
				row.Tag = field;
				ApplyAssignedValueCellState(row);
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			CommitInputGridEdits();
		}

		private void CommitInputGridEdits()
		{
			if (dgvInputs == null)
			{
				return;
			}

			try
			{
				if (dgvInputs.IsCurrentCellDirty)
				{
					dgvInputs.CommitEdit(DataGridViewDataErrorContexts.Commit);
				}

				dgvInputs.EndEdit();
			}
			catch
			{
			}
		}

		private string GetDatabaseFormatName(DatabaseFieldDataFormat format)
		{
			if (format == DatabaseFieldDataFormat.String || format == DatabaseFieldDataFormat.Text)
			{
				return "String";
			}

			return format.ToString();
		}

		private void dgvInputs_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (dgvInputs != null && dgvInputs.IsCurrentCellDirty)
			{
				dgvInputs.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private void dgvInputs_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvInputs == null)
			{
				return;
			}

			string columnName = dgvInputs.Columns[e.ColumnIndex].Name;
			if (string.Equals(columnName, "colForce", StringComparison.OrdinalIgnoreCase))
			{
				ApplyAssignedValueCellState(dgvInputs.Rows[e.RowIndex]);
			}
		}

		private void dgvInputs_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvInputs == null)
			{
				return;
			}

			if (string.Equals(dgvInputs.Columns[e.ColumnIndex].Name, "colGlobalVariable", StringComparison.OrdinalIgnoreCase))
			{
				GlobalVariableBindingUi.SelectForCell(this, dgvInputs.Rows[e.RowIndex], "colGlobalVariable");
			}
		}

		private void ApplyAssignedValueCellState(DataGridViewRow row)
		{
			if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains("colAssignedValue"))
			{
				return;
			}

			bool forceValue = GetCellBool(row, "colForce");
			DataGridViewCell cell = row.Cells["colAssignedValue"];
			cell.ReadOnly = !forceValue;
			cell.Style.BackColor = forceValue ? Color.FromArgb(2, 10, 20) : Color.FromArgb(18, 28, 40);
			cell.Style.ForeColor = forceValue ? Color.White : Color.FromArgb(120, 140, 155);
			cell.Style.SelectionBackColor = forceValue ? Color.FromArgb(0, 120, 200) : Color.FromArgb(45, 60, 75);
			cell.Style.SelectionForeColor = Color.White;
		}

		private void SetDoubleBuffered(Control control)
		{
			if (control == null)
			{
				return;
			}

			try
			{
				System.Reflection.PropertyInfo property = typeof(Control).GetProperty(
					"DoubleBuffered",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic);

				if (property != null)
				{
					property.SetValue(control, true, null);
				}
			}
			catch
			{
			}
		}

		private string GetCellString(DataGridViewRow row, string columnName)
		{
			if (row == null ||
				row.DataGridView == null ||
				string.IsNullOrWhiteSpace(columnName) ||
				!row.DataGridView.Columns.Contains(columnName))
			{
				return string.Empty;
			}

			object value = row.Cells[columnName].Value;
			return value == null ? string.Empty : value.ToString().Trim();
		}

		private bool GetCellBool(DataGridViewRow row, string columnName)
		{
			if (row == null ||
				row.DataGridView == null ||
				string.IsNullOrWhiteSpace(columnName) ||
				!row.DataGridView.Columns.Contains(columnName))
			{
				return false;
			}

			object value = row.Cells[columnName].Value;
			if (value == null)
			{
				return false;
			}

			bool parsed;
			return bool.TryParse(value.ToString(), out parsed) && parsed;
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}

		private class BufferedDataGridView : DataGridView
		{
			public BufferedDataGridView()
			{
				DoubleBuffered = true;
			}
		}
	}

	public class NewStepAssetDialog : Form
	{
		private readonly Func<StepType, string> _defaultNameProvider;
		private readonly Func<StepType, string, string> _validator;
		private readonly bool _isEnglish;
		private ComboBox cmbType;
		private TextBox txtName;
		private Label lblHint;
		private Button btnOK;
		private Button btnCancel;

		public StepType SelectedStepType
		{
			get
			{
				StepTypeOption item = cmbType == null ? null : cmbType.SelectedItem as StepTypeOption;
				return item == null ? StepType.Vpp : item.StepType;
			}
		}

		public string StepName
		{
			get { return txtName == null ? string.Empty : txtName.Text.Trim(); }
		}

		public NewStepAssetDialog(
			string title,
			Func<StepType, string> defaultNameProvider,
			Func<StepType, string, string> validator)
		{
			_isEnglish = LanguagePreferenceStore.LoadIsEnglish();
			_defaultNameProvider = defaultNameProvider;
			_validator = validator;
			InitializeUi(title);
		}

		private void InitializeUi(string title)
		{
			Text = _isEnglish ? "New Step" : title;
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(460, 260);
			MinimumSize = new Size(460, 260);
			MaximumSize = new Size(460, 260);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			Font = new Font("Microsoft YaHei UI", 9F);

			Label lblTitle = new Label();
			lblTitle.Text = T("新建 Step", "New Step");
			lblTitle.Location = new Point(24, 18);
			lblTitle.Size = new Size(390, 26);
			lblTitle.ForeColor = Color.White;
			lblTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);

			Label lblType = CreateLabel(T("类型", "Type"), 24, 58);
			cmbType = new ComboBox();
			cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbType.Location = new Point(112, 54);
			cmbType.Size = new Size(300, 28);
			cmbType.BackColor = Color.FromArgb(5, 14, 28);
			cmbType.ForeColor = Color.White;
			cmbType.FlatStyle = FlatStyle.Flat;
			cmbType.Items.Add(new StepTypeOption("VPP", StepType.Vpp));
			cmbType.Items.Add(new StepTypeOption("Script", StepType.Script));
			cmbType.Items.Add(new StepTypeOption("Hdev", StepType.Halcon));
			cmbType.SelectedIndex = 0;
			cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;

			Label lblName = CreateLabel(T("名称", "Name"), 24, 100);
			txtName = new TextBox();
			txtName.Location = new Point(112, 96);
			txtName.Size = new Size(300, 26);
			txtName.BackColor = Color.FromArgb(5, 14, 28);
			txtName.ForeColor = Color.White;
			txtName.BorderStyle = BorderStyle.FixedSingle;
			txtName.Font = new Font("Microsoft YaHei UI", 10F);
			txtName.TextChanged += txtName_TextChanged;

			lblHint = new Label();
			lblHint.Location = new Point(112, 128);
			lblHint.Size = new Size(300, 42);
			lblHint.ForeColor = Color.FromArgb(140, 175, 205);
			lblHint.Font = new Font("Microsoft YaHei UI", 8.5F);
			lblHint.TextAlign = ContentAlignment.TopLeft;

			btnOK = CreateButton(T("确认", "OK"), 174, 182, true);
			btnCancel = CreateButton(T("取消", "Cancel"), 302, 182, false);
			btnOK.DialogResult = DialogResult.OK;
			btnCancel.DialogResult = DialogResult.Cancel;
			btnOK.EnabledChanged += btnOK_EnabledChanged;

			AcceptButton = btnOK;
			CancelButton = btnCancel;

			Controls.Add(lblTitle);
			Controls.Add(lblType);
			Controls.Add(cmbType);
			Controls.Add(lblName);
			Controls.Add(txtName);
			Controls.Add(lblHint);
			Controls.Add(btnOK);
			Controls.Add(btnCancel);

			SetDefaultNameForSelectedType();
			UpdateValidationState();
			Shown += NewStepAssetDialog_Shown;
		}

		private Label CreateLabel(string text, int x, int y)
		{
			Label lbl = new Label();
			lbl.Text = text;
			lbl.Location = new Point(x, y);
			lbl.Size = new Size(80, 26);
			lbl.ForeColor = Color.FromArgb(210, 230, 245);
			lbl.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			lbl.TextAlign = ContentAlignment.MiddleLeft;
			return lbl;
		}

		private Button CreateButton(string text, int x, int y, bool primary)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(110, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private void btnOK_EnabledChanged(object sender, EventArgs e)
		{
			if (btnOK == null)
			{
				return;
			}

			btnOK.BackColor = btnOK.Enabled
				? Color.FromArgb(0, 95, 220)
				: Color.FromArgb(80, 86, 96);
			btnOK.FlatAppearance.BorderColor = btnOK.Enabled
				? Color.FromArgb(0, 150, 220)
				: Color.FromArgb(95, 100, 110);
		}

		private void NewStepAssetDialog_Shown(object sender, EventArgs e)
		{
			if (txtName == null)
			{
				return;
			}

			txtName.Focus();
			txtName.SelectAll();
		}

		private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetDefaultNameForSelectedType();
			UpdateValidationState();
		}

		private void txtName_TextChanged(object sender, EventArgs e)
		{
			UpdateValidationState();
		}

		private void SetDefaultNameForSelectedType()
		{
			if (txtName == null || _defaultNameProvider == null)
			{
				return;
			}

			txtName.Text = _defaultNameProvider(SelectedStepType) ?? string.Empty;
			txtName.SelectAll();
		}

		private void UpdateValidationState()
		{
			if (txtName == null || btnOK == null || lblHint == null)
			{
				return;
			}

			string error = _validator == null
				? string.Empty
				: _validator(SelectedStepType, txtName.Text);

			bool hasError = !string.IsNullOrWhiteSpace(error);
			bool duplicate = hasError && (error.IndexOf("重名", StringComparison.OrdinalIgnoreCase) >= 0 ||
				error.IndexOf("exists", StringComparison.OrdinalIgnoreCase) >= 0);

			btnOK.Enabled = !hasError;
			lblHint.Text = hasError ? error : T("名称可用。", "Name is available.");
			lblHint.ForeColor = duplicate
				? Color.FromArgb(255, 145, 185)
				: (hasError ? Color.FromArgb(255, 205, 115) : Color.FromArgb(120, 210, 170));

			txtName.BackColor = duplicate
				? Color.FromArgb(255, 220, 235)
				: Color.FromArgb(5, 14, 28);
			txtName.ForeColor = duplicate
				? Color.FromArgb(55, 10, 28)
				: Color.White;
		}

		private class StepTypeOption
		{
			public string Text { get; private set; }
			public StepType StepType { get; private set; }

			public StepTypeOption(string text, StepType stepType)
			{
				Text = text;
				StepType = stepType;
			}

			public override string ToString()
			{
				return Text;
			}
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}

	public class TextInputDialog : Form
	{
		private TextBox txtInput;
		private Button btnOK;
		private Button btnCancel;
		private readonly bool _isEnglish;

		public string InputText
		{
			get { return txtInput == null ? string.Empty : txtInput.Text; }
		}

		public TextInputDialog(string title, string prompt, string defaultText)
		{
			_isEnglish = LanguagePreferenceStore.LoadIsEnglish();
			InitializeUi(title, prompt, defaultText);
		}

		private void InitializeUi(string title, string prompt, string defaultText)
		{
			Text = title;
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(460, 190);
			MinimumSize = new Size(420, 170);
			MaximumSize = new Size(620, 220);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;

			Label lblPrompt = new Label();
			lblPrompt.Text = prompt ?? string.Empty;
			lblPrompt.Location = new Point(22, 18);
			lblPrompt.Size = new Size(395, 26);
			lblPrompt.ForeColor = Color.White;
			lblPrompt.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

			txtInput = new TextBox();
			txtInput.Location = new Point(24, 54);
			txtInput.Size = new Size(390, 26);
			txtInput.Text = defaultText ?? string.Empty;
			txtInput.BackColor = Color.FromArgb(5, 14, 28);
			txtInput.ForeColor = Color.White;
			txtInput.BorderStyle = BorderStyle.FixedSingle;
			txtInput.Font = new Font("Microsoft YaHei UI", 10F);

			btnOK = CreateButton(T("确定", "OK"), 154, 104);
			btnCancel = CreateButton(T("取消", "Cancel"), 284, 104);
			btnOK.DialogResult = DialogResult.OK;
			btnCancel.DialogResult = DialogResult.Cancel;

			AcceptButton = btnOK;
			CancelButton = btnCancel;

			Controls.Add(lblPrompt);
			Controls.Add(txtInput);
			Controls.Add(btnOK);
			Controls.Add(btnCancel);

			Shown += TextInputDialog_Shown;
		}

		private Button CreateButton(string text, int x, int y)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(110, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = Color.FromArgb(0, 95, 190);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private void TextInputDialog_Shown(object sender, EventArgs e)
		{
			if (txtInput == null)
			{
				return;
			}

			txtInput.Focus();
			txtInput.SelectAll();
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}

	public class MultiCheckSelectForm : Form
	{
		protected CheckedListBox list;
		private Button btnClear;
		private Button btnOK;
		private Button btnCancel;
		private readonly bool _isEnglish;

		public List<string> SelectedItems { get; private set; }

		public MultiCheckSelectForm(string title, string prompt, List<string> items, List<string> selected, List<string> disabledItems)
		{
			_isEnglish = LanguagePreferenceStore.LoadIsEnglish();
			SelectedItems = new List<string>();
			InitializeUi(title, prompt);
			LoadItems(items, selected, disabledItems);
		}

		protected virtual void InitializeUi(string title, string prompt)
		{
			Text = title;
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(760, 560);
			MinimumSize = new Size(620, 420);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;

			Label lbl = new Label();
			lbl.Text = prompt;
			lbl.Dock = DockStyle.Top;
			lbl.Height = 54;
			lbl.TextAlign = ContentAlignment.MiddleLeft;
			lbl.Padding = new Padding(28, 0, 0, 0);
			lbl.ForeColor = Color.White;
			lbl.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);

			list = new CheckedListBox();
			list.Dock = DockStyle.Fill;
			list.CheckOnClick = true;
			list.BorderStyle = BorderStyle.FixedSingle;
			list.BackColor = Color.FromArgb(1, 8, 16);
			list.ForeColor = Color.White;
			list.Font = new Font("Microsoft YaHei UI", 10F);
			list.IntegralHeight = false;

			Panel panel = new Panel();
			panel.Dock = DockStyle.Bottom;
			panel.Height = 70;
			panel.BackColor = BackColor;

			btnClear = CreateButton(T("清空", "Clear"), 32, 18, 130);
			btnOK = CreateButton(T("确定", "OK"), 420, 18, 130);
			btnCancel = CreateButton(T("取消", "Cancel"), 575, 18, 130);

			btnClear.Click += btnClear_Click;
			btnOK.Click += btnOK_Click;
			btnCancel.Click += btnCancel_Click;

			panel.Controls.Add(btnClear);
			panel.Controls.Add(btnOK);
			panel.Controls.Add(btnCancel);

			Controls.Add(list);
			Controls.Add(panel);
			Controls.Add(lbl);
		}

		private Button CreateButton(string text, int x, int y, int width)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(width, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = Color.FromArgb(0, 95, 190);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		protected virtual void LoadItems(List<string> items, List<string> selected, List<string> disabledItems)
		{
			list.Items.Clear();
			if (items == null)
			{
				return;
			}

			foreach (string item in items)
			{
				if (string.IsNullOrWhiteSpace(item))
				{
					continue;
				}

				int index = list.Items.Add(item);
				if (selected != null && selected.Any(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
				{
					list.SetItemChecked(index, true);
				}
			}
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < list.Items.Count; i++)
			{
				list.SetItemChecked(i, false);
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			SelectedItems.Clear();
			foreach (object item in list.CheckedItems)
			{
				if (item != null)
				{
					SelectedItems.Add(item.ToString());
				}
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		protected string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}

	public class ScriptInputSourceSelectForm : Form
	{
		private CheckedListBox list;
		private Button btnOK;
		private Button btnCancel;
		private Button btnClear;
		private List<SelectableStepSourceItem> _items;
		private readonly bool _isEnglish;

		public List<string> SelectedItems { get; private set; }

		public ScriptInputSourceSelectForm(string title, string prompt, List<SelectableStepSourceItem> items, List<string> selected)
		{
			_isEnglish = LanguagePreferenceStore.LoadIsEnglish();
			SelectedItems = new List<string>();
			_items = items ?? new List<SelectableStepSourceItem>();
			InitializeUi(title, prompt);
			LoadItems(selected);
		}

		private void InitializeUi(string title, string prompt)
		{
			Text = title;
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(760, 560);
			MinimumSize = new Size(620, 420);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;

			Label lbl = new Label();
			lbl.Text = prompt;
			lbl.Dock = DockStyle.Top;
			lbl.Height = 54;
			lbl.TextAlign = ContentAlignment.MiddleLeft;
			lbl.Padding = new Padding(28, 0, 0, 0);
			lbl.ForeColor = Color.White;
			lbl.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);

			list = new CheckedListBox();
			list.Dock = DockStyle.Fill;
			list.CheckOnClick = true;
			list.BorderStyle = BorderStyle.FixedSingle;
			list.BackColor = Color.FromArgb(1, 8, 16);
			list.ForeColor = Color.White;
			list.Font = new Font("Microsoft YaHei UI", 10F);
			list.IntegralHeight = false;
			list.ItemCheck += list_ItemCheck;
			list.DrawMode = DrawMode.OwnerDrawFixed;
			list.DrawItem += list_DrawItem;

			Panel panel = new Panel();
			panel.Dock = DockStyle.Bottom;
			panel.Height = 70;
			panel.BackColor = BackColor;

			btnClear = CreateButton(T("清空", "Clear"), 32, 18, 130);
			btnOK = CreateButton(T("确定", "OK"), 420, 18, 130);
			btnCancel = CreateButton(T("取消", "Cancel"), 575, 18, 130);

			btnClear.Click += btnClear_Click;
			btnOK.Click += btnOK_Click;
			btnCancel.Click += btnCancel_Click;

			panel.Controls.Add(btnClear);
			panel.Controls.Add(btnOK);
			panel.Controls.Add(btnCancel);

			Controls.Add(list);
			Controls.Add(panel);
			Controls.Add(lbl);
		}

		private Button CreateButton(string text, int x, int y, int width)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(width, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.BackColor = Color.FromArgb(0, 95, 190);
			btn.ForeColor = Color.White;
			btn.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			btn.UseVisualStyleBackColor = false;
			return btn;
		}

		private void LoadItems(List<string> selected)
		{
			list.Items.Clear();
			foreach (SelectableStepSourceItem item in _items)
			{
				int index = list.Items.Add(item.DisplayText);
				if (item.Enabled && selected != null && selected.Any(x => string.Equals(x, item.Name, StringComparison.OrdinalIgnoreCase)))
				{
					list.SetItemChecked(index, true);
				}
			}
		}

		private void list_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (e.Index < 0 || e.Index >= _items.Count)
			{
				return;
			}

			if (!_items[e.Index].Enabled)
			{
				e.NewValue = CheckState.Unchecked;
			}
		}

		private void list_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0 || e.Index >= list.Items.Count)
			{
				return;
			}

			bool enabled = e.Index < _items.Count && _items[e.Index].Enabled;
			Color back = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? Color.FromArgb(0, 120, 200) : Color.FromArgb(1, 8, 16);
			Color fore = enabled ? Color.White : Color.FromArgb(120, 140, 155);

			using (SolidBrush b = new SolidBrush(back))
			{
				e.Graphics.FillRectangle(b, e.Bounds);
			}

			string text = Convert.ToString(list.Items[e.Index]);
			if (!enabled)
			{
				text += T("    (不是前序)", "    (not previous)");
			}

			TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, fore, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
			e.DrawFocusRectangle();
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < list.Items.Count; i++)
			{
				list.SetItemChecked(i, false);
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			SelectedItems.Clear();

			for (int i = 0; i < list.Items.Count && i < _items.Count; i++)
			{
				if (!_items[i].Enabled)
				{
					continue;
				}

				if (list.GetItemChecked(i))
				{
					SelectedItems.Add(_items[i].Name);
				}
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}
}
