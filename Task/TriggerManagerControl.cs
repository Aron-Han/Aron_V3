	using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Cognex.VisionPro.ImageFile;

namespace Aron_V3
{

	public class TaskTestRequestedEventArgs : EventArgs
	{
		public string JobName { get; private set; }
		public string TaskName { get; private set; }
		public TaskRunOptions Options { get; private set; }
		public bool Handled { get; set; }
		public Exception Error { get; set; }

		public TaskTestRequestedEventArgs(string jobName, string taskName, TaskRunOptions options)
		{
			JobName = jobName;
			TaskName = taskName;
			Options = options;
			Handled = false;
			Error = null;
		}
	}

	public partial class TriggerManagerControl : UserControl, ILocalizable
	{
		private sealed class TaskGridRowTag
		{
			public string JobName;
			public string OriginalTaskName;
			public string OriginalProtocol;
			public string OriginalChannel;
		}

		private Panel panelJobButtons;
		private Button btnAddJob;
		private Button btnDeleteJob;
		private Button btnTestTask;
		private Label lblProtocolsTitle;
		private ListBox listProtocols;
		private Label lblChannelsTitle;
		private ListBox listChannels;

		private ComboLikePopupForm _activeComboPopup;

		private const int COL_TASK_NAME = 0;
		private const int COL_PROTOCOL = 1;
		private const int COL_CHANNEL = 2;
		private const int COL_TRIGGER_NAME = 3;
		private const int COL_TRIGGER_VALUE = 4;
		private const int COL_IMAGE_SOURCE = 5;
		private const int COL_POSITION_NAME = 6;
		private const int COL_POSITION_VALUE = 7;
		private const int COL_REMARK = 8;

		private bool _loading = false;
		private bool _isEnglish = false;

		// 外部可绑定真实 Task 执行入口。
		// 推荐在 Form1 或 FlowConfigForm 中赋值：
		// triggerPage.TaskTestExecutor = delegate(string job, string task, TaskRunOptions options) { ...; return true; };
		public Func<string, string, TaskRunOptions, bool> TaskTestExecutor { get; set; }

		public event EventHandler<TaskTestRequestedEventArgs> TaskTestRequested;


		public TriggerManagerControl()
		{
			InitializeComponent();


			CommunicationConfigChangedHub.ConfigChanged += CommunicationConfigChangedHub_ConfigChanged;
			this.VisibleChanged += TriggerManagerControl_VisibleChanged;

			ConfigureTaskManagementLayout();
			ConfigureTriggerGrid();
			CreateTaskTestButton();

			if (listProtocols != null)
			{
				listProtocols.SelectedIndexChanged -= listProtocols_SelectedIndexChanged;
				listProtocols.SelectedIndexChanged += listProtocols_SelectedIndexChanged;
			}

			if (listChannels != null)
			{
				listChannels.SelectedIndexChanged -= listChannels_SelectedIndexChanged;
				listChannels.SelectedIndexChanged += listChannels_SelectedIndexChanged;
			}

			listJobs.SelectedIndexChanged -= listJobs_SelectedIndexChanged;
			listJobs.SelectedIndexChanged += listJobs_SelectedIndexChanged;

			dgvTrigger.CurrentCellDirtyStateChanged -= dgvTrigger_CurrentCellDirtyStateChanged;
			dgvTrigger.CurrentCellDirtyStateChanged += dgvTrigger_CurrentCellDirtyStateChanged;

			dgvTrigger.CellValueChanged -= dgvTrigger_CellValueChanged;
			dgvTrigger.CellValueChanged += dgvTrigger_CellValueChanged;

			dgvTrigger.CellDoubleClick -= dgvTrigger_CellDoubleClick;
			dgvTrigger.CellDoubleClick += dgvTrigger_CellDoubleClick;

			dgvTrigger.CellClick -= dgvTrigger_CellClick;
			dgvTrigger.CellClick += dgvTrigger_CellClick;

			dgvTrigger.Scroll -= dgvTrigger_Scroll;
			dgvTrigger.Scroll += dgvTrigger_Scroll;

			dgvTrigger.Leave -= dgvTrigger_Leave;
			dgvTrigger.Leave += dgvTrigger_Leave;

			dgvTrigger.CellFormatting -= dgvTrigger_CellFormatting;
			dgvTrigger.CellFormatting += dgvTrigger_CellFormatting;

			dgvTrigger.CellPainting -= dgvTrigger_CellPainting;
			dgvTrigger.CellPainting += dgvTrigger_CellPainting;

			dgvTrigger.EditingControlShowing -= dgvTrigger_EditingControlShowing;
			dgvTrigger.EditingControlShowing += dgvTrigger_EditingControlShowing;

			dgvTrigger.DataError -= dgvTrigger_DataError;
			dgvTrigger.DataError += dgvTrigger_DataError;

			FlowConfigStore.FlowConfigSaved -= FlowConfigStore_FlowConfigSaved;
			FlowConfigStore.FlowConfigSaved += FlowConfigStore_FlowConfigSaved;

			LoadFlowConfigToJobList();
			CreateTaskTestButton();
		}

		private void ConfigureTaskManagementLayout()
		{
			if (panelJobs != null)
			{
				panelJobs.Visible = false;
				panelJobs.Enabled = false;
				panelJobs.Margin = new Padding(0);
			}

			if (rootLayout != null && rootLayout.ColumnStyles.Count >= 2)
			{
				rootLayout.ColumnStyles[0].SizeType = SizeType.Absolute;
				rootLayout.ColumnStyles[0].Width = 0F;
				rootLayout.ColumnStyles[1].SizeType = SizeType.Percent;
				rootLayout.ColumnStyles[1].Width = 100F;
			}

			if (panelTrigger != null)
			{
				panelTrigger.Margin = new Padding(0);
			}

			if (lblTriggerTitle != null)
			{
				lblTriggerTitle.Text = _isEnglish ? "Task Settings" : "任务设置";
			}
		}

		private void ConfigureTriggerGrid()
		{
			dgvTrigger.Columns.Clear();
			dgvTrigger.AllowUserToAddRows = false;
			dgvTrigger.AllowUserToDeleteRows = false;
			dgvTrigger.RowHeadersVisible = false;
			dgvTrigger.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvTrigger.MultiSelect = false;
			dgvTrigger.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvTrigger.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			dgvTrigger.RowTemplate.Height = 34;
			dgvTrigger.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
			dgvTrigger.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvTrigger.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvTrigger.EditMode = DataGridViewEditMode.EditProgrammatically;

			DataGridViewTextBoxColumn colTask = new DataGridViewTextBoxColumn();
			colTask.Name = "colTaskName";
			colTask.HeaderText = "task名称";
			colTask.FillWeight = 110;
			dgvTrigger.Columns.Add(colTask);

			DataGridViewTextBoxColumn colProtocol = new DataGridViewTextBoxColumn();
			colProtocol.Name = "colProtocol";
			colProtocol.HeaderText = "协议";
			colProtocol.FillWeight = 105;
			colProtocol.ReadOnly = true;
			dgvTrigger.Columns.Add(colProtocol);

			DataGridViewComboBoxColumn colChannel = new DataGridViewComboBoxColumn();
			colChannel.Name = "colChannel";
			colChannel.HeaderText = "通道";
			colChannel.FillWeight = 95;
			colChannel.FlatStyle = FlatStyle.Flat;
			colChannel.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			dgvTrigger.Columns.Add(colChannel);

			DataGridViewComboBoxColumn colTrigger = new DataGridViewComboBoxColumn();
			colTrigger.Name = "colTriggerName";
			colTrigger.HeaderText = "触发源";
			colTrigger.FillWeight = 115;
			colTrigger.FlatStyle = FlatStyle.Flat;
			colTrigger.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			dgvTrigger.Columns.Add(colTrigger);

			DataGridViewTextBoxColumn colTriggerValue = new DataGridViewTextBoxColumn();
			colTriggerValue.Name = "colTriggerValue";
			colTriggerValue.HeaderText = "触发源值";
			colTriggerValue.FillWeight = 90;
			colTriggerValue.Visible = false;
			dgvTrigger.Columns.Add(colTriggerValue);

			DataGridViewTextBoxColumn colImage = new DataGridViewTextBoxColumn();
			colImage.Name = "colImageSource";
			colImage.HeaderText = "图像源";
			colImage.FillWeight = 180;
			colImage.ReadOnly = true;
			colImage.Visible = false;
			colImage.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			colImage.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvTrigger.Columns.Add(colImage);

			DataGridViewComboBoxColumn colPositionName = new DataGridViewComboBoxColumn();
			colPositionName.Name = "colPositionName";
			colPositionName.HeaderText = "位置号";
			colPositionName.FillWeight = 90;
			colPositionName.FlatStyle = FlatStyle.Flat;
			colPositionName.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			dgvTrigger.Columns.Add(colPositionName);

			DataGridViewTextBoxColumn colPositionValue = new DataGridViewTextBoxColumn();
			colPositionValue.Name = "colPositionValue";
			colPositionValue.HeaderText = "位置号值";
			colPositionValue.FillWeight = 90;
			dgvTrigger.Columns.Add(colPositionValue);

			DataGridViewTextBoxColumn colRemark = new DataGridViewTextBoxColumn();
			colRemark.Name = "colRemark";
			colRemark.HeaderText = "备注";
			colRemark.FillWeight = 140;
			dgvTrigger.Columns.Add(colRemark);

			ApplyTriggerGridEditableColumns();

			ApplyGridStyle();
			RefreshComboColumnOptions();
		}


		private void ApplyTriggerGridEditableColumns()
		{
			if (dgvTrigger == null || dgvTrigger.Columns.Count <= 0)
			{
				return;
			}

			// 下拉列由自定义弹出菜单处理。
			dgvTrigger.Columns[COL_PROTOCOL].ReadOnly = true;
			dgvTrigger.Columns[COL_CHANNEL].ReadOnly = true;
			dgvTrigger.Columns[COL_TRIGGER_NAME].ReadOnly = true;
			dgvTrigger.Columns[COL_IMAGE_SOURCE].ReadOnly = true;
			dgvTrigger.Columns[COL_POSITION_NAME].ReadOnly = true;

			// 普通值列需要可以直接编辑。
			dgvTrigger.Columns[COL_TRIGGER_VALUE].ReadOnly = true;
			dgvTrigger.Columns[COL_POSITION_VALUE].ReadOnly = false;

			if (dgvTrigger.Columns.Count > COL_REMARK)
			{
				dgvTrigger.Columns[COL_REMARK].ReadOnly = false;
			}
		}


		private void ApplyGridStyle()
		{
			dgvTrigger.EnableHeadersVisualStyles = false;
			dgvTrigger.BackgroundColor = Color.FromArgb(2, 10, 20);
			dgvTrigger.GridColor = Color.FromArgb(45, 70, 95);
			dgvTrigger.BorderStyle = BorderStyle.None;
			dgvTrigger.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			dgvTrigger.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgvTrigger.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvTrigger.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			dgvTrigger.DefaultCellStyle.BackColor = Color.FromArgb(2, 10, 20);
			dgvTrigger.DefaultCellStyle.ForeColor = Color.White;
			dgvTrigger.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			dgvTrigger.DefaultCellStyle.SelectionForeColor = Color.White;
			dgvTrigger.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvTrigger.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			dgvTrigger.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		}

		private void BuildProtocolNavigation()
		{
			if (panelJobs == null || listProtocols != null)
			{
				return;
			}

			lblProtocolsTitle = new Label();
			lblProtocolsTitle.Name = "lblProtocolsTitle";
			lblProtocolsTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			lblProtocolsTitle.ForeColor = Color.White;
			lblProtocolsTitle.TextAlign = ContentAlignment.MiddleLeft;
			lblProtocolsTitle.Text = _isEnglish ? "All Protocol" : "所有 通讯协议";
			lblProtocolsTitle.AutoSize = false;
			lblChannelsTitle = new Label();
			lblChannelsTitle.Name = "lblChannelsTitle";
			lblChannelsTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			lblChannelsTitle.ForeColor = Color.White;
			lblChannelsTitle.TextAlign = ContentAlignment.MiddleLeft;
			lblChannelsTitle.Text = _isEnglish ? "All Channel" : "所有 通道";
			lblChannelsTitle.AutoSize = false;
			lblJobsTitle.Text = _isEnglish ? "All Program" : "所有 程序号";

			listProtocols = new ListBox();
			listProtocols.Name = "listProtocols";
			listProtocols.BackColor = Color.FromArgb(5, 14, 28);
			listProtocols.BorderStyle = BorderStyle.FixedSingle;
			listProtocols.Font = new Font("Microsoft YaHei UI", 9.5F);
			listProtocols.ForeColor = Color.FromArgb(220, 230, 240);
			listProtocols.ItemHeight = 25;

			listChannels = new ListBox();
			listChannels.Name = "listChannels";
			listChannels.BackColor = Color.FromArgb(5, 14, 28);
			listChannels.BorderStyle = BorderStyle.FixedSingle;
			listChannels.Font = new Font("Microsoft YaHei UI", 9.5F);
			listChannels.ForeColor = Color.FromArgb(220, 230, 240);
			listChannels.ItemHeight = 25;

			panelJobs.Controls.Add(lblProtocolsTitle);
			panelJobs.Controls.Add(listProtocols);
			panelJobs.Controls.Add(lblChannelsTitle);
			panelJobs.Controls.Add(listChannels);

			lblJobsTitle.Dock = DockStyle.None;
			listJobs.Dock = DockStyle.None;
			panelJobs.Resize -= TriggerNavigation_Resize;
			panelJobs.Resize += TriggerNavigation_Resize;
			LayoutTriggerNavigation();
		}

		private void TriggerNavigation_Resize(object sender, EventArgs e)
		{
			LayoutTriggerNavigation();
		}

		private void LayoutTriggerNavigation()
		{
			if (panelJobs == null || lblProtocolsTitle == null || listProtocols == null ||
				lblChannelsTitle == null || listChannels == null || lblJobsTitle == null || listJobs == null)
			{
				return;
			}

			int left = 18;
			int top = 16;
			int width = Math.Max(60, panelJobs.ClientSize.Width - 36);
			int selectorHeight = Math.Max(70, Math.Min(120, panelJobs.ClientSize.Height / 6));

			lblProtocolsTitle.SetBounds(left, top, width, 30);
			listProtocols.SetBounds(left, lblProtocolsTitle.Bottom, width, selectorHeight);

			lblChannelsTitle.SetBounds(left, listProtocols.Bottom + 12, width, 30);
			listChannels.SetBounds(left, lblChannelsTitle.Bottom, width, selectorHeight);

			lblJobsTitle.SetBounds(left, listChannels.Bottom + 16, width, 30);
			listJobs.Left = left;
			listJobs.Top = lblJobsTitle.Bottom;
			listJobs.Width = width;

			LayoutJobActionButtons();
		}

		private List<string> GetCandidateCameraRootFolders()
		{
			List<string> roots = new List<string>();
			string jobName = GetSelectedJobNameSafe();

			if (string.IsNullOrWhiteSpace(jobName))
			{
				return roots;
			}

			string jobFolder = Path.Combine(GetFlowJobRootFolder(), jobName);

			// 当前标准目录：Project\Job\Job_001\Hardware\Camera\Cam1\*.vpp
			string currentJobHardwareCameraRoot = Path.Combine(jobFolder, "Hardware", "Camera");

			if (Directory.Exists(currentJobHardwareCameraRoot) && !roots.Contains(currentJobHardwareCameraRoot))
			{
				roots.Add(currentJobHardwareCameraRoot);
			}

			// 兼容旧目录：Project\Job\Job_001\Camera\Cam1\*.vpp
			string oldJobCameraRoot = Path.Combine(jobFolder, "Camera");

			if (Directory.Exists(oldJobCameraRoot) && !roots.Contains(oldJobCameraRoot))
			{
				roots.Add(oldJobCameraRoot);
			}

			return roots;
		}



		private bool IsImageSourceConfigFile(string file)
		{
			string ext = Path.GetExtension(file);

			if (!string.Equals(ext, ".vpp", StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(ext, ".xml", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);

			if (string.IsNullOrWhiteSpace(fileNameWithoutExt))
			{
				return false;
			}

			if (fileNameWithoutExt.Equals("HardwareConfig", StringComparison.OrdinalIgnoreCase) ||
				fileNameWithoutExt.Equals("ImageSources", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			// 过滤旧默认 Camera.vpp，避免图像源列表显示自动生成的占位文件。
			if (fileNameWithoutExt.Equals("Camera", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return true;
		}


		private List<string> GetAllCameraImageSourcesFromFiles()
		{
			List<string> result = new List<string>();
			result.Add("Not Use");

			List<string> cameraRoots = GetCandidateCameraRootFolders();

			foreach (string cameraRoot in cameraRoots)
			{
				if (!Directory.Exists(cameraRoot))
				{
					continue;
				}

				string[] cameraFolders = Directory.GetDirectories(cameraRoot);

				foreach (string cameraFolder in cameraFolders)
				{
					string cameraName = Path.GetFileName(cameraFolder);

					if (string.IsNullOrWhiteSpace(cameraName))
					{
						continue;
					}

					// 当前标准目录：
					// Project\Job\当前Job\Camera\Cam1\*.VPP
					// 同时兼容 *.vpp / *.xml，大小写不敏感。
					string[] rootFiles = Directory.GetFiles(cameraFolder, "*.*", SearchOption.TopDirectoryOnly);

					foreach (string file in rootFiles)
					{
						if (!IsImageSourceConfigFile(file))
						{
							continue;
						}

						string sourceKey = cameraName + "." + Path.GetFileName(file);

						if (!result.Contains(sourceKey))
						{
							result.Add(sourceKey);
						}
					}

					// 兼容旧目录：如果旧工程文件仍在 Cam1 的子目录中，也能被扫描到。
					string[] subFiles = Directory.GetFiles(cameraFolder, "*.*", SearchOption.AllDirectories);

					foreach (string file in subFiles)
					{
						if (!IsImageSourceConfigFile(file))
						{
							continue;
						}

						string sourceKey = cameraName + "." + Path.GetFileName(file);

						if (!result.Contains(sourceKey))
						{
							result.Add(sourceKey);
						}
					}
				}
			}

			return result;
		}

		private void RefreshImageSourceColumnItems()
		{
			if (dgvTrigger == null || dgvTrigger.Columns.Count <= COL_IMAGE_SOURCE)
			{
				return;
			}

			List<string> imageSources = GetAllCameraImageSourcesFromFiles();

			foreach (DataGridViewRow row in dgvTrigger.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				string current = GetCellString(row, COL_IMAGE_SOURCE);
				string normalized = NormalizeImageSourceSelection(current, imageSources);

				if (string.IsNullOrWhiteSpace(normalized))
				{
					normalized = "Not Use";
				}

				row.Cells[COL_IMAGE_SOURCE].Value = normalized;
			}

			UpdateTriggerGridRowHeights();
		}


		private void UpdateTriggerGridRowHeights()
		{
			if (dgvTrigger == null || dgvTrigger.Rows.Count <= 0)
			{
				return;
			}

			foreach (DataGridViewRow row in dgvTrigger.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				row.Height = 34;
			}
		}



		private void RefreshComboColumnOptions()
		{
			// 协议列改为双击勾选，不再维护 DataGridViewComboBoxColumn 的 Items。
		}

		private void FlowConfigStore_FlowConfigSaved(object sender, EventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new EventHandler(FlowConfigStore_FlowConfigSaved), sender, e);
				return;
			}
			string oldProtocol = GetSelectedProtocolNameSafe();
			string oldChannel = GetSelectedChannelNameSafe();
			string oldJob = GetSelectedJobNameSafe();
			LoadFlowConfigToJobList();
			SelectListItem(listProtocols, oldProtocol);
			SelectListItem(listChannels, oldChannel);
			SelectListItem(listJobs, oldJob);
			LoadCurrentJobTasksToGrid();
		}

		private void LoadFlowConfigToJobList()
		{
			_loading = true;

			try
			{
				if (listProtocols != null)
				{
					listProtocols.Items.Clear();
				}
				if (listChannels != null)
				{
					listChannels.Items.Clear();
				}
				listJobs.Items.Clear();
				dgvTrigger.Rows.Clear();

				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

				List<string> jobs = GetAllLocalJobNames();

				foreach (string jobName in jobs)
				{
					listJobs.Items.Add(jobName);
				}

				if (listJobs.SelectedIndex < 0 && listJobs.Items.Count > 0)
				{
					listJobs.SelectedIndex = 0;
				}

				LoadAllTasksToGrid(config);
			}
			finally
			{
				_loading = false;
			}
		}


		private List<string> GetAllLocalJobNames()
		{
			List<string> jobs = new List<string>();

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

			foreach (JobConfig job in EnumerateAllJobs(config))
			{
				if (job == null || string.IsNullOrWhiteSpace(job.JobName))
				{
					continue;
				}

				AddJobNameIfNotExists(jobs, NormalizeJobName(job.JobName));
			}

			jobs.Sort(StringComparer.OrdinalIgnoreCase);
			return jobs;
		}

		private void AddJobNameIfNotExists(List<string> jobs, string jobName)
		{
			if (jobs == null || string.IsNullOrWhiteSpace(jobName))
			{
				return;
			}

			if (!jobs.Any(x => string.Equals(x, jobName, StringComparison.OrdinalIgnoreCase)))
			{
				jobs.Add(jobName);
			}
		}

		private string NormalizeJobName(string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				return string.Empty;
			}

			jobName = jobName.Trim();

			foreach (char c in Path.GetInvalidFileNameChars())
			{
				jobName = jobName.Replace(c, '_');
			}

			if (jobName.StartsWith("job_", StringComparison.OrdinalIgnoreCase))
			{
				string suffix = jobName.Substring(4);
				jobName = "Job_" + suffix;
			}

			return jobName;
		}


		private void listJobs_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading) return;
			LoadCurrentJobTasksToGrid();
		}

		private void listProtocols_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading) return;
			LoadFlowConfigToJobList();
		}

		private void listChannels_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading) return;
			LoadFlowConfigToJobList();
		}

		private void LoadCurrentJobTasksToGrid()
		{
			dgvTrigger.Rows.Clear();
			RefreshComboColumnOptions();
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			LoadAllTasksToGrid(config);
		}

		private void LoadAllTasksToGrid(ProjectFlowConfig config)
		{
			dgvTrigger.Rows.Clear();
			RefreshComboColumnOptions();

			foreach (JobConfig job in EnumerateAllJobs(config)
				.OrderBy(x => x == null ? string.Empty : x.JobName)
				.ThenBy(x => x == null ? string.Empty : x.ProtocolName)
				.ThenBy(x => x == null ? string.Empty : x.ChannelName))
			{
				if (job == null || job.Tasks == null) continue;
				foreach (TaskConfig task in job.Tasks.OrderBy(t => t.RunOrder)) AddTaskRowToGrid(job, task);
			}
		}

		private IEnumerable<JobConfig> EnumerateAllJobs(ProjectFlowConfig config)
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

				foreach (ChannelFlowConfig channel in protocol.Channels)
				{
					if (channel == null || channel.Jobs == null)
					{
						continue;
					}

					foreach (JobConfig job in channel.Jobs)
					{
						if (job != null)
						{
							yield return job;
						}
					}
				}
			}
		}

		private void AddTaskRowToGrid(JobConfig job, TaskConfig task)
		{
			if (task == null) return;

			string protocolSelection = GetTaskProtocolSelection(job, task);
			string protocol = GetPrimaryProtocol(protocolSelection);

			string channelName = GetTaskChannelName(job, task);

			string triggerName = task.TriggerName;
			TaskCommunicationTriggerBinding firstBinding = GetFirstTriggerBinding(task);
			if (firstBinding != null && !string.IsNullOrWhiteSpace(firstBinding.TriggerName))
			{
				triggerName = firstBinding.TriggerName;
			}
			if (string.IsNullOrEmpty(triggerName)) triggerName = GetDefaultTriggerSource(protocolSelection, channelName);

			string triggerValue = task.TriggerValue;
			if (firstBinding != null && !string.IsNullOrWhiteSpace(firstBinding.TriggerValue))
			{
				triggerValue = firstBinding.TriggerValue;
			}
			if (string.IsNullOrEmpty(triggerValue)) triggerValue = GetTriggerExpectedValue(protocolSelection, channelName, triggerName);
			if (string.IsNullOrEmpty(triggerValue)) triggerValue = "1";

			string positionName = task.PositionName;
			if (firstBinding != null && !string.IsNullOrWhiteSpace(firstBinding.PositionName))
			{
				positionName = firstBinding.PositionName;
			}
			if (string.IsNullOrEmpty(positionName)) positionName = task.PositionOptionName;
			if (string.IsNullOrEmpty(positionName)) positionName = task.FlagBit.ToString();
			if (string.IsNullOrEmpty(positionName) || positionName == "0") positionName = GetDefaultPositionSource(protocolSelection, channelName);

			string positionValue = task.PositionValue;
			if (firstBinding != null && !string.IsNullOrWhiteSpace(firstBinding.PositionValue))
			{
				positionValue = firstBinding.PositionValue;
			}
			if (string.IsNullOrEmpty(positionValue)) positionValue = task.FlagValue;
			if (string.IsNullOrEmpty(positionValue)) positionValue = GetPositionExpectedValue(protocolSelection, channelName, positionName);
			if (string.IsNullOrEmpty(positionValue)) positionValue = "1";

			int rowIndex = dgvTrigger.Rows.Add(
				task.TaskName,
				protocolSelection,
				channelName,
				triggerName,
				triggerValue,
				"Not Use",
				positionName,
				positionValue,
				task.Remark);
			dgvTrigger.Rows[rowIndex].Tag = new TaskGridRowTag
			{
				JobName = job == null ? GetDefaultTaskJobName() : job.JobName,
				OriginalTaskName = task.TaskName,
				OriginalProtocol = protocol,
				OriginalChannel = channelName
			};
			ShowAllTriggerRows();
			UpdateTriggerGridRowHeights();

			UpdateChannelCellOptions(rowIndex, protocolSelection, channelName);
			UpdateTriggerSourceCellOptions(rowIndex, protocolSelection, channelName, triggerName);
			UpdatePositionSourceCellOptions(rowIndex, protocolSelection, channelName, positionName);
		}

		private void dgvTrigger_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (dgvTrigger.IsCurrentCellDirty) dgvTrigger.CommitEdit(DataGridViewDataErrorContexts.Commit);
		}

		private void dgvTrigger_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (_loading || e.RowIndex < 0 || e.ColumnIndex < 0) return;
			if (e.ColumnIndex == COL_PROTOCOL)
			{
				RefreshCommunicationOptionCells(e.RowIndex, true);
			}
			else if (e.ColumnIndex == COL_CHANNEL || e.ColumnIndex == COL_TRIGGER_NAME || e.ColumnIndex == COL_POSITION_NAME)
			{
				UpdateChannelDerivedValueCells(e.RowIndex);
			}
		}




		private bool IsComboLikeColumn(int columnIndex)
		{
			return columnIndex == COL_PROTOCOL ||
				columnIndex == COL_TRIGGER_NAME ||
				columnIndex == COL_CHANNEL ||
				columnIndex == COL_POSITION_NAME;
		}

		private void dgvTrigger_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				return;
			}

			if (!IsComboLikeColumn(e.ColumnIndex))
			{
				return;
			}

			// 当前正在编辑的 ComboBox 交给系统控件绘制，避免影响下拉。
			if (dgvTrigger.CurrentCell != null &&
				dgvTrigger.CurrentCell.RowIndex == e.RowIndex &&
				dgvTrigger.CurrentCell.ColumnIndex == e.ColumnIndex &&
				dgvTrigger.IsCurrentCellInEditMode)
			{
				return;
			}

			e.Handled = true;

			e.PaintBackground(e.CellBounds, true);
			e.Paint(e.CellBounds, DataGridViewPaintParts.Border);

			string text = e.FormattedValue == null ? string.Empty : e.FormattedValue.ToString();
			Rectangle textRect = e.CellBounds;
			textRect.X += 4;
			textRect.Width -= 22;

			TextRenderer.DrawText(
				e.Graphics,
				text,
				e.CellStyle.Font,
				textRect,
				e.CellStyle.ForeColor,
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis);

			Rectangle arrowRect = new Rectangle(
				e.CellBounds.Right - 18,
				e.CellBounds.Top + (e.CellBounds.Height - 16) / 2,
				16,
				16);

			ControlPaint.DrawComboButton(e.Graphics, arrowRect, ButtonState.Normal);
		}

		private void dgvTrigger_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			// 下拉列现在使用 ContextMenuStrip 弹出选择，不进入 DataGridViewComboBoxCell 编辑模式。
			// 保留此方法是为了兼容事件绑定。
		}



		private void ShowComboLikePopup(int rowIndex, int columnIndex)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count)
			{
				return;
			}

			DataGridViewCell cell = dgvTrigger.Rows[rowIndex].Cells[columnIndex];

			if (cell == null)
			{
				return;
			}

			List<string> items = GetComboLikeItems(rowIndex, columnIndex);

			if (items.Count <= 0)
			{
				return;
			}

			CloseActiveComboPopup();

			Rectangle rect = dgvTrigger.GetCellDisplayRectangle(columnIndex, rowIndex, true);

			// 宽度严格跟随母单元格，不再按文本长度或最小宽度自动收缩。
			int popupWidth = Math.Max(1, rect.Width);
			int itemHeight = 30;
			int popupHeight = Math.Min(Math.Max(itemHeight, items.Count * itemHeight + 2), 240);

			Point screenPoint = dgvTrigger.PointToScreen(new Point(rect.Left, rect.Bottom));
			Rectangle screen = Screen.FromControl(dgvTrigger).WorkingArea;

			if (screenPoint.Y + popupHeight > screen.Bottom)
			{
				screenPoint = dgvTrigger.PointToScreen(new Point(rect.Left, rect.Top - popupHeight));
			}

			string current = cell.Value == null ? string.Empty : cell.Value.ToString();

			_activeComboPopup = new ComboLikePopupForm(items, current, popupWidth, popupHeight);
			_activeComboPopup.StartPosition = FormStartPosition.Manual;
			_activeComboPopup.Location = screenPoint;

			_activeComboPopup.ValueSelected += delegate (string selectedValue)
			{
				if (string.IsNullOrWhiteSpace(selectedValue))
				{
					return;
				}

				cell.Value = selectedValue;

				if (columnIndex == COL_PROTOCOL)
				{
					RefreshCommunicationOptionCells(rowIndex, true);
				}
				else if (columnIndex == COL_CHANNEL)
				{
					RefreshCommunicationOptionCells(rowIndex, false);
				}
				else if (columnIndex == COL_TRIGGER_NAME || columnIndex == COL_POSITION_NAME)
				{
					UpdateChannelDerivedValueCells(rowIndex);
				}

				dgvTrigger.Invalidate();
			};

			_activeComboPopup.FormClosed += delegate
			{
				_activeComboPopup = null;
			};

			Form owner = this.FindForm();

			if (owner != null)
			{
				_activeComboPopup.Show(owner);
			}
			else
			{
				_activeComboPopup.Show();
			}

			_activeComboPopup.Activate();
		}

		private void CloseActiveComboPopup()
		{
			if (_activeComboPopup == null)
			{
				return;
			}

			if (!_activeComboPopup.IsDisposed)
			{
				_activeComboPopup.Close();
			}

			_activeComboPopup = null;
		}

		private List<string> GetComboLikeItems(int rowIndex, int columnIndex)
		{
			List<string> result = new List<string>();

			if (columnIndex == COL_PROTOCOL)
			{
				return LoadCommunicationInstanceOptions();
			}

			DataGridViewCell cell = dgvTrigger.Rows[rowIndex].Cells[columnIndex];

			DataGridViewComboBoxCell comboCell = cell as DataGridViewComboBoxCell;

			if (comboCell != null)
			{
				foreach (object item in comboCell.Items)
				{
					if (item != null)
					{
						result.Add(item.ToString());
					}
				}
			}

			if (result.Count <= 0)
			{
				DataGridViewComboBoxColumn comboColumn = dgvTrigger.Columns[columnIndex] as DataGridViewComboBoxColumn;

				if (comboColumn != null)
				{
					foreach (object item in comboColumn.Items)
					{
						if (item != null)
						{
							result.Add(item.ToString());
						}
					}
				}
			}

			return result;
		}



		private void dgvTrigger_Scroll(object sender, ScrollEventArgs e)
		{
			CloseActiveComboPopup();
		}

		private void dgvTrigger_Leave(object sender, EventArgs e)
		{
			// 如果焦点切到弹窗本身，不关闭；弹窗 Deactivate 会自己处理。
			if (_activeComboPopup != null && _activeComboPopup.ContainsFocus)
			{
				return;
			}

			CloseActiveComboPopup();
		}


		private void dgvTrigger_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex != COL_IMAGE_SOURCE)
			{
				return;
			}

			if (e.Value == null)
			{
				return;
			}

			string value = e.Value.ToString();

			if (string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			if (value.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			// 只影响界面显示：
			// 实际单元格值仍然保持 Cam1.xxx;Cam2.xxx，保存 XML 不受影响。
			e.Value = value.Replace(";", Environment.NewLine);
			e.FormattingApplied = true;
		}


		private void dgvTrigger_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
			{
				CloseActiveComboPopup();
				return;
			}

			if (IsComboLikeColumn(e.ColumnIndex))
			{
				ShowComboLikePopup(e.RowIndex, e.ColumnIndex);
				return;
			}

			CloseActiveComboPopup();

			if (!dgvTrigger.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
			{
				dgvTrigger.CurrentCell = dgvTrigger.Rows[e.RowIndex].Cells[e.ColumnIndex];
				dgvTrigger.BeginEdit(true);
			}
		}

		private void dgvTrigger_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			// 协议、通道、触发源、位置号都通过单击弹出的单选菜单处理。
		}

		private void OpenImageSourceMultiSelectDialog(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count)
			{
				return;
			}

			List<string> allSources = GetAllCameraImageSourcesFromFiles();
			string currentValue = GetCellString(dgvTrigger.Rows[rowIndex], COL_IMAGE_SOURCE);
			List<string> selectedSources = SplitImageSourceSelection(currentValue);

			using (ImageSourceMultiSelectForm dialog = new ImageSourceMultiSelectForm(allSources, selectedSources))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				string value = JoinImageSourceSelection(dialog.SelectedImageSources);

				if (string.IsNullOrWhiteSpace(value))
				{
					value = "Not Use";
				}

				dgvTrigger.Rows[rowIndex].Cells[COL_IMAGE_SOURCE].Value = value;
				UpdateTriggerGridRowHeights();
			}
		}

		private List<string> SplitImageSourceSelection(string value)
		{
			List<string> result = new List<string>();

			if (string.IsNullOrWhiteSpace(value))
			{
				return result;
			}

			string[] parts = value.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string part in parts)
			{
				string item = part.Trim();

				if (string.IsNullOrWhiteSpace(item))
				{
					continue;
				}

				if (item.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!result.Any(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
				{
					result.Add(item);
				}
			}

			return result;
		}

		private string JoinImageSourceSelection(List<string> values)
		{
			if (values == null || values.Count <= 0)
			{
				return "Not Use";
			}

			List<string> validValues = new List<string>();

			foreach (string value in values)
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					continue;
				}

				if (value.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!validValues.Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase)))
				{
					validValues.Add(value);
				}
			}

			if (validValues.Count <= 0)
			{
				return "Not Use";
			}

			return string.Join(";", validValues.ToArray());
		}

		private string NormalizeImageSourceSelection(string value, List<string> allSources)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return "Not Use";
			}

			if (allSources == null || allSources.Count <= 0)
			{
				return "Not Use";
			}

			List<string> selected = SplitImageSourceSelection(value);
			List<string> valid = new List<string>();

			foreach (string item in selected)
			{
				if (allSources.Any(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
				{
					string exact = allSources.First(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase));

					if (!valid.Any(x => string.Equals(x, exact, StringComparison.OrdinalIgnoreCase)))
					{
						valid.Add(exact);
					}
				}
			}

			if (valid.Count <= 0)
			{
				return "Not Use";
			}

			return JoinImageSourceSelection(valid);
		}


		private void dgvTrigger_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private TaskCommunicationTriggerBinding GetFirstTriggerBinding(TaskConfig task)
		{
			if (task == null || task.CommunicationTriggerBindings == null)
			{
				return null;
			}

			return task.CommunicationTriggerBindings.FirstOrDefault(x =>
				x != null &&
				!string.IsNullOrWhiteSpace(x.CommunicationProtocol) &&
				!x.CommunicationProtocol.Equals("Not Use", StringComparison.OrdinalIgnoreCase));
		}

		private string GetTaskProtocolSelection(JobConfig job, TaskConfig task)
		{
			List<string> protocols = new List<string>();

			if (task != null && task.CommunicationTriggerBindings != null)
			{
				foreach (TaskCommunicationTriggerBinding binding in task.CommunicationTriggerBindings)
				{
					if (binding == null)
					{
						continue;
					}

					if (!string.IsNullOrWhiteSpace(binding.CommunicationInstanceName))
					{
						AddProtocolIfValid(protocols, binding.CommunicationInstanceName);
					}
					else
					{
						AddProtocolIfValid(protocols, binding.CommunicationProtocol);
					}
				}
			}

			if (task != null)
			{
				if (!string.IsNullOrWhiteSpace(task.CommunicationInstanceName))
				{
					AddProtocolIfValid(protocols, task.CommunicationInstanceName);
				}
				else
				{
					AddProtocolIfValid(protocols, task.CommunicationProtocol);
				}
			}

			if (job != null)
			{
				AddProtocolIfValid(protocols, job.ProtocolName);
			}

			if (protocols.Count <= 0)
			{
				AddProtocolIfValid(protocols, GetDefaultCommunicationSelection());
			}

			return JoinProtocolSelection(protocols);
		}

		private string GetTaskChannelName(JobConfig job, TaskConfig task)
		{
			TaskCommunicationTriggerBinding binding = GetFirstTriggerBinding(task);
			if (binding != null && !string.IsNullOrWhiteSpace(binding.CommunicationChannel))
			{
				return binding.CommunicationChannel;
			}

			if (task != null && !string.IsNullOrWhiteSpace(task.CommunicationChannel))
			{
				return task.CommunicationChannel;
			}

			if (job != null && !string.IsNullOrWhiteSpace(job.ChannelName))
			{
				return job.ChannelName;
			}

			return GetDefaultChannelName(GetDefaultEnabledProtocol());
		}

		private void AddProtocolIfValid(List<string> protocols, string protocol)
		{
			if (protocols == null || string.IsNullOrWhiteSpace(protocol))
			{
				return;
			}

			string selection = ResolveCommunicationSelectionName(protocol);
			if (selection.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				selection.Equals("None", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (!protocols.Any(x => string.Equals(x, selection, StringComparison.OrdinalIgnoreCase)))
			{
				protocols.Add(selection);
			}
		}

		private string ResolveCommunicationSelectionName(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			string text = value.Trim();
			CommunicationInstanceConfig instance = FindCommunicationInstance(text);
			if (instance != null)
			{
				return instance.InstanceName;
			}

			string protocol = FlowConfigStore.NormalizeProtocolName(text);
			instance = FindFirstCommunicationInstanceByProtocol(protocol);
			if (instance != null)
			{
				return instance.InstanceName;
			}

			return protocol;
		}

		private CommunicationInstanceConfig FindCommunicationInstance(string instanceName)
		{
			if (string.IsNullOrWhiteSpace(instanceName))
			{
				return null;
			}

			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			if (config == null || config.Instances == null)
			{
				return null;
			}

			return config.Instances.FirstOrDefault(x =>
				x != null &&
				!string.IsNullOrWhiteSpace(x.InstanceName) &&
				string.Equals(x.InstanceName, instanceName.Trim(), StringComparison.OrdinalIgnoreCase));
		}

		private CommunicationInstanceConfig FindFirstCommunicationInstanceByProtocol(string protocolName)
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			if (config == null || config.Instances == null)
			{
				return null;
			}

			CommunicationType type;
			if (!TryGetCommunicationType(protocolName, out type))
			{
				return null;
			}

			return config.Instances.FirstOrDefault(x => x != null && x.CommunicationType == type);
		}

		private bool TryGetCommunicationType(string protocolName, out CommunicationType type)
		{
			string normalized = FlowConfigStore.NormalizeProtocolName(protocolName);
			if (normalized.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				normalized.Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
			{
				type = CommunicationType.TcpIp;
				return true;
			}

			if (normalized.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				type = CommunicationType.Profinet;
				return true;
			}

			if (normalized.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				type = CommunicationType.S7;
				return true;
			}

			type = CommunicationType.TcpIp;
			return false;
		}

		private string GetProtocolNameForCommunicationSelection(string selection)
		{
			CommunicationInstanceConfig instance = FindCommunicationInstance(selection);
			if (instance != null)
			{
				return GetProtocolName(instance.CommunicationType);
			}

			return FlowConfigStore.NormalizeProtocolName(selection);
		}

		private string GetInstanceNameForCommunicationSelection(string selection)
		{
			CommunicationInstanceConfig instance = FindCommunicationInstance(selection);
			if (instance != null)
			{
				return instance.InstanceName;
			}

			return GetDefaultCommunicationInstanceName(selection);
		}

		private string GetProtocolName(CommunicationType type)
		{
			if (type == CommunicationType.TcpIp)
			{
				return "TCP/IP";
			}

			if (type == CommunicationType.Profinet)
			{
				return "Profinet";
			}

			return "S7";
		}

		private List<string> SplitProtocolSelection(string value)
		{
			List<string> result = new List<string>();

			if (!string.IsNullOrWhiteSpace(value))
			{
				string[] parts = value.Split(new char[] { ';', ',', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string part in parts)
				{
					AddProtocolIfValid(result, part);
					if (result.Count > 0)
					{
						break;
					}
				}
			}

			if (result.Count <= 0)
			{
				AddProtocolIfValid(result, GetDefaultCommunicationSelection());
			}

			return result;
		}

		private string JoinProtocolSelection(List<string> values)
		{
			List<string> result = new List<string>();

			if (values != null)
			{
				foreach (string value in values)
				{
					AddProtocolIfValid(result, value);
					if (result.Count > 0)
					{
						break;
					}
				}
			}

			if (result.Count <= 0)
			{
				return GetDefaultCommunicationSelection();
			}

			return result[0];
		}

		private string GetPrimaryProtocol(string protocolSelection)
		{
			List<string> protocols = SplitProtocolSelection(protocolSelection);
			return protocols.Count > 0 ? GetProtocolNameForCommunicationSelection(protocols[0]) : GetDefaultEnabledProtocol();
		}

		private string GetPrimaryInstanceName(string protocolSelection)
		{
			List<string> protocols = SplitProtocolSelection(protocolSelection);
			return protocols.Count > 0 ? GetInstanceNameForCommunicationSelection(protocols[0]) : GetDefaultCommunicationInstanceName(GetDefaultEnabledProtocol());
		}

		private void UpdateTriggerSourceCellOptions(int rowIndex, string protocol, string channelName, string currentValue)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count) return;
			DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
			cell.FlatStyle = FlatStyle.Flat;
			List<string> triggers = LoadTriggerSourceOptionsForProtocols(protocol, channelName);
			foreach (string trigger in triggers) cell.Items.Add(trigger);
			if (string.IsNullOrEmpty(currentValue) || !triggers.Contains(currentValue)) currentValue = triggers.Count > 0 ? triggers[0] : string.Empty;
			cell.Value = currentValue;
			dgvTrigger.Rows[rowIndex].Cells[COL_TRIGGER_NAME] = cell;
		}

		private void UpdateChannelCellOptions(int rowIndex, string protocol, string currentValue)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count) return;

			DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
			cell.FlatStyle = FlatStyle.Flat;

			List<string> channels = LoadChannelOptionsForProtocols(protocol);
			foreach (string channel in channels) cell.Items.Add(channel);

			if (string.IsNullOrEmpty(currentValue) || !channels.Contains(currentValue))
			{
				currentValue = channels.Count > 0 ? channels[0] : string.Empty;
			}

			cell.Value = currentValue;
			dgvTrigger.Rows[rowIndex].Cells[COL_CHANNEL] = cell;
		}

		private void UpdatePositionSourceCellOptions(int rowIndex, string protocol, string channelName, string currentValue)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count) return;

			DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
			cell.FlatStyle = FlatStyle.Flat;

			List<string> positions = LoadPositionSourceOptionsForProtocols(protocol, channelName);
			foreach (string position in positions) cell.Items.Add(position);

			if (string.IsNullOrEmpty(currentValue) || !positions.Contains(currentValue))
			{
				currentValue = positions.Count > 0 ? positions[0] : string.Empty;
			}

			cell.Value = currentValue;
			dgvTrigger.Rows[rowIndex].Cells[COL_POSITION_NAME] = cell;
			UpdateChannelDerivedValueCells(rowIndex);
		}

		private List<string> LoadEnabledProtocolOptions()
		{
			List<string> result = new List<string>();
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			if (config.TcpIp != null && config.TcpIp.Enabled) result.Add("TCP/IP");
			if (config.Profinet != null && config.Profinet.Enabled) result.Add("Profinet");
			if (config.S7 != null && config.S7.Enabled) result.Add("S7");
			if (result.Count == 0) result.Add("Not Use");
			return result;
		}

		private List<string> LoadCommunicationInstanceOptions()
		{
			List<string> result = new List<string>();
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();

			if (config != null && config.Instances != null)
			{
				foreach (CommunicationInstanceConfig instance in config.Instances)
				{
					if (instance == null || string.IsNullOrWhiteSpace(instance.InstanceName))
					{
						continue;
					}

					if (instance.CommunicationType == CommunicationType.Profinet)
					{
						continue;
					}

					if (!result.Any(x => string.Equals(x, instance.InstanceName, StringComparison.OrdinalIgnoreCase)))
					{
						result.Add(instance.InstanceName);
					}
				}
			}

			if (result.Count <= 0)
			{
				string protocol = GetDefaultEnabledProtocol();
				string instanceName = GetDefaultCommunicationInstanceName(protocol);
				result.Add(string.IsNullOrWhiteSpace(instanceName) ? protocol : instanceName);
			}

			return result;
		}

		private string GetDefaultEnabledProtocol()
		{
			List<string> protocols = LoadEnabledProtocolOptions();
			return protocols.Count > 0 ? protocols[0] : "Not Use";
		}

		private string GetDefaultCommunicationSelection()
		{
			List<string> instances = LoadCommunicationInstanceOptions();
			return instances.Count > 0 ? instances[0] : GetDefaultEnabledProtocol();
		}

		private List<string> LoadChannelOptions(string protocol)
		{
			List<string> result = new List<string>();
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			List<CommunicationChannelConfig> channels = GetChannelsByProtocol(config, protocol);

			foreach (CommunicationChannelConfig channel in channels)
			{
				if (channel == null || !channel.Enabled || string.IsNullOrWhiteSpace(channel.ChannelName))
				{
					continue;
				}

				result.Add(channel.ChannelName);
			}

			if (result.Count == 0)
			{
				result.Add("Channel01");
			}

			return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private List<string> LoadChannelOptionsForProtocols(string protocolSelection)
		{
			List<string> result = new List<string>();
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();

			foreach (string protocol in SplitProtocolSelection(protocolSelection))
			{
				foreach (CommunicationChannelConfig channelConfig in GetChannelsByCommunicationSelection(config, protocol))
				{
					if (channelConfig == null || !channelConfig.Enabled || string.IsNullOrWhiteSpace(channelConfig.ChannelName))
					{
						continue;
					}

					if (!result.Any(x => string.Equals(x, channelConfig.ChannelName, StringComparison.OrdinalIgnoreCase)))
					{
						result.Add(channelConfig.ChannelName);
					}
				}
			}

			if (result.Count <= 0)
			{
				result.Add("Channel01");
			}

			return result;
		}

		private string GetDefaultChannelName(string protocol)
		{
			List<string> channels = LoadChannelOptions(protocol);
			return channels.Count > 0 ? channels[0] : "Channel01";
		}

		private void EnsureProtocolValueExists(string protocol)
		{
			if (string.IsNullOrEmpty(protocol)) return;
			DataGridViewComboBoxColumn col = dgvTrigger.Columns[COL_PROTOCOL] as DataGridViewComboBoxColumn;
			if (col != null && !col.Items.Contains(protocol)) col.Items.Add(protocol);
		}

		private List<string> LoadTriggerSourceOptions(string protocol, string channelName)
		{
			List<string> result = new List<string>();
			if (string.IsNullOrEmpty(protocol) || protocol == "Not Use") { result.Add("Not Use"); return result; }
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			CommunicationChannelConfig channel = GetChannelByNameForSelection(config, protocol, channelName);
			if (channel != null)
			{
				string triggerGlobal = NormalizeConfiguredCommunicationValue(channel.TriggerGlobalVariableName);
				if (!string.IsNullOrWhiteSpace(triggerGlobal))
				{
					result.Add(triggerGlobal);
				}

				string legacyTrigger = NormalizeConfiguredCommunicationValue(channel.TriggerName);
				if (!string.IsNullOrWhiteSpace(legacyTrigger) &&
					!legacyTrigger.Equals("Trigger", StringComparison.OrdinalIgnoreCase) &&
					!result.Any(x => string.Equals(x, legacyTrigger, StringComparison.OrdinalIgnoreCase)))
				{
					result.Add(legacyTrigger);
				}

				string customTriggerGlobal = NormalizeConfiguredCommunicationValue(channel.CustomTriggerGlobalVariableName);
				if (!string.IsNullOrWhiteSpace(customTriggerGlobal) &&
					!result.Any(x => string.Equals(x, customTriggerGlobal, StringComparison.OrdinalIgnoreCase)))
				{
					result.Add(customTriggerGlobal);
				}
			}
			if (result.Count == 0) result.Add("Not Use");
			return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private string NormalizeConfiguredCommunicationValue(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			string text = value.Trim();
			if (text.Equals("Not Use", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("None", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("Select", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("Select...", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("选择", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("选择...", StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}

			return text;
		}

		private List<string> LoadTriggerSourceOptionsForProtocols(string protocolSelection, string channelName)
		{
			List<string> result = new List<string>();

			foreach (string protocol in SplitProtocolSelection(protocolSelection))
			{
				foreach (string trigger in LoadTriggerSourceOptions(protocol, channelName))
				{
					if (!result.Any(x => string.Equals(x, trigger, StringComparison.OrdinalIgnoreCase)))
					{
						result.Add(trigger);
					}
				}
			}

			if (result.Any(x => !x.Equals("Not Use", StringComparison.OrdinalIgnoreCase)))
			{
				result.RemoveAll(x => x.Equals("Not Use", StringComparison.OrdinalIgnoreCase));
			}

			if (result.Count <= 0)
			{
				result.Add("Not Use");
			}

			return result;
		}

		private string GetDefaultTriggerSource(string protocol, string channelName)
		{
			List<string> triggers = LoadTriggerSourceOptions(protocol, channelName);
			return triggers.Count > 0 ? triggers[0] : "Not Use";
		}

		private List<string> LoadPositionSourceOptions(string protocol, string channelName)
		{
			List<string> result = new List<string>();
			result.Add("Not Use");

			if (string.IsNullOrEmpty(protocol) || protocol == "Not Use")
			{
				return result;
			}

			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			CommunicationChannelConfig channel = GetChannelByNameForSelection(config, protocol, channelName);
			if (channel != null && !string.IsNullOrWhiteSpace(channel.PositionGlobalVariableName))
			{
				result.Add(channel.PositionGlobalVariableName);
			}
			else if (channel != null && !string.IsNullOrWhiteSpace(channel.PositionSourceName) &&
				!channel.PositionSourceName.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
			{
				result.Add(channel.PositionSourceName);
			}

			if (channel != null && channel.PositionOptions != null)
			{
				foreach (CommunicationPositionOption option in channel.PositionOptions)
				{
					if (option != null && !string.IsNullOrWhiteSpace(option.Name))
					{
						result.Add(option.Name);
					}
				}
			}

			List<CommInputVariable> inputVariables = GetInputVariablesByCommunicationSelection(config, protocol);
			if (inputVariables != null)
			{
				foreach (CommInputVariable item in inputVariables)
				{
					if (item != null && item.UseAsPosition)
					{
						result.Add(item.Name);
					}
				}
			}

			return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private List<string> LoadPositionSourceOptionsForProtocols(string protocolSelection, string channelName)
		{
			List<string> result = new List<string>();

			foreach (string protocol in SplitProtocolSelection(protocolSelection))
			{
				foreach (string position in LoadPositionSourceOptions(protocol, channelName))
				{
					if (!result.Any(x => string.Equals(x, position, StringComparison.OrdinalIgnoreCase)))
					{
						result.Add(position);
					}
				}
			}

			if (result.Count <= 0)
			{
				result.Add("Not Use");
			}

			return result;
		}

		private string GetDefaultPositionSource(string protocol, string channelName)
		{
			List<string> positions = LoadPositionSourceOptions(protocol, channelName);
			return positions.Count > 0 ? positions[0] : "Not Use";
		}

		private string GetPositionExpectedValue(string protocol, string channelName, string positionOptionName)
		{
			CommunicationChannelConfig channel = GetChannelByNameForSelection(CommunicationConfigStore.LoadOrCreateDefault(), protocol, channelName);
			CommunicationPositionOption option = channel == null || channel.PositionOptions == null
				? null
				: channel.PositionOptions.FirstOrDefault(x =>
					x != null && string.Equals(x.Name, positionOptionName, StringComparison.OrdinalIgnoreCase));

			return option == null ? string.Empty : option.ExpectedValue;
		}

		private string GetTriggerExpectedValue(string protocol, string channelName, string triggerName)
		{
			CommunicationChannelConfig channel = GetChannelByNameForSelection(CommunicationConfigStore.LoadOrCreateDefault(), protocol, channelName);
			if (channel == null)
			{
				return string.Empty;
			}

			string selectedTrigger = NormalizeConfiguredCommunicationValue(triggerName);
			if (string.IsNullOrWhiteSpace(selectedTrigger))
			{
				return string.Empty;
			}

			string customTriggerGlobal = NormalizeConfiguredCommunicationValue(channel.CustomTriggerGlobalVariableName);
			if (!string.IsNullOrWhiteSpace(customTriggerGlobal) &&
				string.Equals(selectedTrigger, customTriggerGlobal, StringComparison.OrdinalIgnoreCase))
			{
				return channel.CustomTriggerExpectedValue;
			}

			string triggerGlobal = NormalizeConfiguredCommunicationValue(channel.TriggerGlobalVariableName);
			if (!string.IsNullOrWhiteSpace(triggerGlobal) &&
				string.Equals(selectedTrigger, triggerGlobal, StringComparison.OrdinalIgnoreCase))
			{
				return channel.TriggerExpectedValue;
			}

			string legacyTrigger = NormalizeConfiguredCommunicationValue(channel.TriggerName);
			if (!string.IsNullOrWhiteSpace(legacyTrigger) &&
				!legacyTrigger.Equals("Trigger", StringComparison.OrdinalIgnoreCase) &&
				string.Equals(selectedTrigger, legacyTrigger, StringComparison.OrdinalIgnoreCase))
			{
				return channel.TriggerExpectedValue;
			}

			return string.Empty;
		}

		private void RefreshCommunicationOptionCells(int rowIndex, bool resetDerivedSelections)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count)
			{
				return;
			}

			DataGridViewRow row = dgvTrigger.Rows[rowIndex];
			string protocolSelection = GetCellString(row, COL_PROTOCOL);
			string channelName = GetCellString(row, COL_CHANNEL);

			UpdateChannelCellOptions(rowIndex, protocolSelection, channelName);
			channelName = GetCellString(row, COL_CHANNEL);

			string triggerName = resetDerivedSelections ? string.Empty : GetCellString(row, COL_TRIGGER_NAME);
			UpdateTriggerSourceCellOptions(rowIndex, protocolSelection, channelName, triggerName);

			string positionName = resetDerivedSelections ? string.Empty : GetCellString(row, COL_POSITION_NAME);
			UpdatePositionSourceCellOptions(rowIndex, protocolSelection, channelName, positionName);
			UpdateChannelDerivedValueCells(rowIndex);
		}

		private void UpdateChannelDerivedValueCells(int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count)
			{
				return;
			}

			DataGridViewRow row = dgvTrigger.Rows[rowIndex];
			string protocolSelection = GetCellString(row, COL_PROTOCOL);
			string channelName = GetCellString(row, COL_CHANNEL);
			if (string.IsNullOrWhiteSpace(channelName))
			{
				List<string> channels = LoadChannelOptionsForProtocols(protocolSelection);
				channelName = channels.Count > 0 ? channels[0] : "Channel01";
			}
			CommunicationChannelConfig channel = GetChannelByNameForSelection(CommunicationConfigStore.LoadOrCreateDefault(), protocolSelection, channelName);

			if (channel != null)
			{
				string triggerName = GetCellString(row, COL_TRIGGER_NAME);
				List<string> triggerOptions = LoadTriggerSourceOptionsForProtocols(protocolSelection, channelName);
				if (string.IsNullOrWhiteSpace(triggerName) ||
					!triggerOptions.Any(x => string.Equals(x, triggerName, StringComparison.OrdinalIgnoreCase)))
				{
					triggerName = triggerOptions.Count > 0 ? triggerOptions[0] : string.Empty;
					row.Cells[COL_TRIGGER_NAME].Value = triggerName;
				}

				row.Cells[COL_TRIGGER_VALUE].Value = GetTriggerExpectedValue(protocolSelection, channelName, triggerName);
			}

			string positionName = GetCellString(row, COL_POSITION_NAME);
			string expectedValue = GetPositionExpectedValue(protocolSelection, channelName, positionName);
			string currentPositionValue = GetCellString(row, COL_POSITION_VALUE);
			if (string.IsNullOrWhiteSpace(currentPositionValue) && !string.IsNullOrEmpty(expectedValue))
			{
				row.Cells[COL_POSITION_VALUE].Value = expectedValue;
			}
			else if (string.IsNullOrWhiteSpace(currentPositionValue) &&
				string.Equals(positionName, "Not Use", StringComparison.OrdinalIgnoreCase))
			{
				row.Cells[COL_POSITION_VALUE].Value = string.Empty;
			}
		}

		private List<CommunicationChannelConfig> GetChannelsByProtocol(CommunicationConfig config, string protocol)
		{
			if (config == null || string.IsNullOrWhiteSpace(protocol))
			{
				return new List<CommunicationChannelConfig>();
			}

			if (protocol.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				protocol.Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
			{
				return config.TcpIp == null || config.TcpIp.Channels == null
					? new List<CommunicationChannelConfig>()
					: config.TcpIp.Channels;
			}

			if (protocol.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return config.Profinet == null || config.Profinet.Channels == null
					? new List<CommunicationChannelConfig>()
					: config.Profinet.Channels;
			}

			if (protocol.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return config.S7 == null || config.S7.Channels == null
					? new List<CommunicationChannelConfig>()
					: config.S7.Channels;
			}

			return new List<CommunicationChannelConfig>();
		}

		private List<CommunicationChannelConfig> GetChannelsByCommunicationSelection(CommunicationConfig config, string selection)
		{
			if (config == null || string.IsNullOrWhiteSpace(selection))
			{
				return new List<CommunicationChannelConfig>();
			}

			CommunicationInstanceConfig instance = FindCommunicationInstance(selection);
			if (instance != null)
			{
				if (instance.CommunicationType == CommunicationType.TcpIp)
				{
					if (instance.TcpIp != null && instance.TcpIp.Channels != null)
					{
						return instance.TcpIp.Channels;
					}
				}
				else if (instance.CommunicationType == CommunicationType.Profinet)
				{
					if (instance.Profinet != null && instance.Profinet.Channels != null)
					{
						return instance.Profinet.Channels;
					}
				}
				else if (instance.CommunicationType == CommunicationType.S7)
				{
					if (instance.S7 != null && instance.S7.Channels != null)
					{
						return instance.S7.Channels;
					}
				}

				return instance.Channels == null
					? new List<CommunicationChannelConfig>()
					: instance.Channels;
			}

			return GetChannelsByProtocol(config, GetProtocolNameForCommunicationSelection(selection));
		}

		private List<CommInputVariable> GetInputVariablesByCommunicationSelection(CommunicationConfig config, string selection)
		{
			if (config == null || string.IsNullOrWhiteSpace(selection))
			{
				return new List<CommInputVariable>();
			}

			CommunicationInstanceConfig instance = FindCommunicationInstance(selection);
			if (instance != null)
			{
				if (instance.CommunicationType == CommunicationType.TcpIp)
				{
					return instance.TcpIp == null || instance.TcpIp.InputVariables == null
						? new List<CommInputVariable>()
						: instance.TcpIp.InputVariables;
				}

				if (instance.CommunicationType == CommunicationType.Profinet)
				{
					return instance.Profinet == null || instance.Profinet.InputVariables == null
						? new List<CommInputVariable>()
						: instance.Profinet.InputVariables;
				}

				if (instance.CommunicationType == CommunicationType.S7)
				{
					return instance.S7 == null || instance.S7.InputVariables == null
						? new List<CommInputVariable>()
						: instance.S7.InputVariables;
				}
			}

			string protocol = GetProtocolNameForCommunicationSelection(selection);
			if (protocol.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				return config.TcpIp == null || config.TcpIp.InputVariables == null
					? new List<CommInputVariable>()
					: config.TcpIp.InputVariables;
			}

			if (protocol.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return config.Profinet == null || config.Profinet.InputVariables == null
					? new List<CommInputVariable>()
					: config.Profinet.InputVariables;
			}

			if (protocol.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return config.S7 == null || config.S7.InputVariables == null
					? new List<CommInputVariable>()
					: config.S7.InputVariables;
			}

			return new List<CommInputVariable>();
		}

		private CommunicationChannelConfig GetChannelByName(CommunicationConfig config, string protocol, string channelName)
		{
			List<CommunicationChannelConfig> channels = GetChannelsByProtocol(config, protocol);
			if (string.IsNullOrWhiteSpace(channelName))
			{
				channelName = "Channel01";
			}

			return channels.FirstOrDefault(x =>
				x != null && string.Equals(x.ChannelName, channelName, StringComparison.OrdinalIgnoreCase));
		}

		private CommunicationChannelConfig GetChannelByNameForSelection(CommunicationConfig config, string selection, string channelName)
		{
			List<CommunicationChannelConfig> channels = GetChannelsByCommunicationSelection(config, selection);
			if (string.IsNullOrWhiteSpace(channelName))
			{
				channelName = "Channel01";
			}

			return channels.FirstOrDefault(x =>
				x != null && string.Equals(x.ChannelName, channelName, StringComparison.OrdinalIgnoreCase));
		}


		private void CreateTaskTestButton()
		{
			if (panelButtons == null || panelButtons.IsDisposed)
			{
				return;
			}

			if (btnTestTask == null || btnTestTask.IsDisposed)
			{
				btnTestTask = CreateBottomActionButton(_isEnglish ? "Task Test" : "▶ Task测试");
				btnTestTask.Name = "btnTestTask";
				btnTestTask.Click -= btnTestTask_Click;
				btnTestTask.Click += btnTestTask_Click;
			}

			TableLayoutPanel layout = panelButtons as TableLayoutPanel;

			if (layout == null)
			{
				if (btnTestTask.Parent != panelButtons)
				{
					panelButtons.Controls.Add(btnTestTask);
				}

				btnTestTask.Width = 130;
				btnTestTask.Height = 32;
				btnTestTask.Left = Math.Max(0, panelButtons.ClientSize.Width - btnTestTask.Width - 150);
				btnTestTask.Top = 10;
				btnTestTask.Anchor = AnchorStyles.Right | AnchorStyles.Top;
				btnTestTask.BringToFront();
				return;
			}

			layout.SuspendLayout();

			try
			{
				layout.Controls.Remove(btnAddTask);
				layout.Controls.Remove(btnDeleteSelected);
				layout.Controls.Remove(btnTestTask);
				layout.Controls.Remove(btnSave);

				layout.ColumnStyles.Clear();
				layout.ColumnCount = 5;
				layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
				layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
				layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
				layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
				layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));

				if (btnAddTask != null)
				{
					layout.Controls.Add(btnAddTask, 0, 0);
				}

				if (btnDeleteSelected != null)
				{
					layout.Controls.Add(btnDeleteSelected, 1, 0);
				}

				layout.Controls.Add(btnTestTask, 2, 0);

				if (btnSave != null)
				{
					layout.Controls.Add(btnSave, 4, 0);
				}
			}
			finally
			{
				layout.ResumeLayout(true);
			}
		}

		private Button CreateBottomActionButton(string text)
		{
			Button button = new Button();
			button.Text = text;
			button.Dock = DockStyle.Fill;
			button.Margin = new Padding(6, 0, 6, 0);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(8, 35, 60);
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(5, 25, 45);
			button.BackColor = Color.FromArgb(3, 14, 27);
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			button.Cursor = Cursors.Hand;
			button.UseVisualStyleBackColor = false;
			return button;
		}

		private void btnTestTask_Click(object sender, EventArgs e)
		{
			TestSelectedTask();
		}


		private void CreateJobActionButtons()
		{
			ListBox jobList = GetJobListBox();

			if (jobList == null || jobList.Parent == null)
			{
				return;
			}

			Control parent = jobList.Parent;

			if (panelJobButtons == null || panelJobButtons.IsDisposed)
			{
				panelJobButtons = new Panel();
				panelJobButtons.Name = "panelJobButtons";
				panelJobButtons.Height = 40;
				panelJobButtons.BackColor = Color.Transparent;
				panelJobButtons.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

				btnAddJob = CreateSmallSquareButton("+");
				btnAddJob.Name = "btnAddJob";
				btnAddJob.Left = 0;
				btnAddJob.Top = 5;
				btnAddJob.Click -= btnAddJob_Click;
				btnAddJob.Click += btnAddJob_Click;

				btnDeleteJob = CreateSmallSquareButton("-");
				btnDeleteJob.Name = "btnDeleteJob";
				btnDeleteJob.Left = 52;
				btnDeleteJob.Top = 5;
				btnDeleteJob.Click -= btnDeleteJob_Click;
				btnDeleteJob.Click += btnDeleteJob_Click;

				panelJobButtons.Controls.Add(btnAddJob);
				panelJobButtons.Controls.Add(btnDeleteJob);
				parent.Controls.Add(panelJobButtons);
			}
			else if (panelJobButtons.Parent != parent)
			{
				panelJobButtons.Parent.Controls.Remove(panelJobButtons);
				parent.Controls.Add(panelJobButtons);
			}

			parent.Resize -= JobListParent_Resize;
			parent.Resize += JobListParent_Resize;

			LayoutJobActionButtons();
			panelJobButtons.BringToFront();
		}


		private void JobListParent_Resize(object sender, EventArgs e)
		{
			LayoutJobActionButtons();
		}

		private void LayoutJobActionButtons()
		{
			ListBox jobList = GetJobListBox();

			if (jobList == null || jobList.Parent == null || panelJobButtons == null)
			{
				return;
			}

			Control parent = jobList.Parent;
			int margin = 8;
			int panelHeight = 40;

			// 注意：这里必须取消 Dock。否则 listJobs 会占满父容器，把底部 + / - 按钮盖住。
			jobList.Dock = DockStyle.None;
			jobList.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

			panelJobButtons.Left = jobList.Left;
			panelJobButtons.Width = Math.Max(90, jobList.Width);
			panelJobButtons.Height = panelHeight;
			panelJobButtons.Top = Math.Max(jobList.Top + 40, parent.ClientSize.Height - panelHeight - margin);

			jobList.Height = Math.Max(40, panelJobButtons.Top - jobList.Top - margin);

			panelJobButtons.BringToFront();
		}


		private Button CreateSmallSquareButton(string text)
		{
			Button button = new Button();
			button.Text = text;
			button.Size = new Size(42, 30);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			button.BackColor = Color.FromArgb(3, 14, 27);
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
			button.Cursor = Cursors.Hand;
			return button;
		}



		private void btnAddJob_Click(object sender, EventArgs e)
		{
			ListBox jobList = GetJobListBox();

			if (jobList == null)
			{
				return;
			}

			string newJobName = GetNextJobName(jobList);

			using (InputTextDialog dialog = new InputTextDialog("Add Job", "Job Name", newJobName))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				newJobName = NormalizeJobName(dialog.InputValue);
			}

			if (string.IsNullOrWhiteSpace(newJobName))
			{
				MessageBox.Show("Job name cannot be empty.", "Add Job", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (GetAllLocalJobNames().Any(x => string.Equals(x, newJobName, StringComparison.OrdinalIgnoreCase)))
			{
				MessageBox.Show("Job already exists.", "Add Job", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string protocolName = GetSelectedProtocolNameSafe();
			string channelName = GetSelectedChannelNameSafe();

			try
			{
				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
				JobConfig job = FlowConfigStore.GetOrCreateJob(config, protocolName, channelName, newJobName);
				job.ProgramNo = GetNextProgramNo(config, protocolName, channelName);
				FlowConfigStore.Save(config);

				LoadFlowConfigToJobList();
				SelectListItem(jobList, newJobName);
				LoadCurrentJobTasksToGrid();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Add Job failed: " + ex.Message, "Add Job", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}


		private string GetNextJobName(ListBox jobList)
		{
			int index = 1;
			List<string> jobs = GetAllLocalJobNames();

			while (true)
			{
				string name = "Job_" + index.ToString("000");

				if (!jobs.Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)))
				{
					return name;
				}

				index++;
			}
		}


		private void btnDeleteJob_Click(object sender, EventArgs e)
		{
			ListBox jobList = GetJobListBox();

			if (jobList == null || jobList.SelectedItem == null)
			{
				MessageBox.Show("Please select a Job first.", "Delete Job", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string jobName = NormalizeJobName(jobList.SelectedItem.ToString());

			if (string.IsNullOrWhiteSpace(jobName))
			{
				return;
			}

			DialogResult result = MessageBox.Show(
				"Delete Job [" + jobName + "] and all local files under Project\\Job?",
				"Delete Job",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);

			if (result != DialogResult.Yes)
			{
				return;
			}

			try
			{
				DeleteLocalJobFiles(jobName);

				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
				string protocolName = GetSelectedProtocolNameSafe();
				string channelName = GetSelectedChannelNameSafe();
				ChannelFlowConfig channel = FlowConfigStore.GetChannel(config, protocolName, channelName);
				if (channel != null && channel.Jobs != null)
				{
					channel.Jobs.RemoveAll(j => j != null && string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));
				}

				FlowConfigStore.Save(config);

				LoadFlowConfigToJobList();

				if (jobList.Items.Count == 0)
				{
					dgvTrigger.Rows.Clear();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Delete Job failed: " + ex.Message, "Delete Job", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}


		private void DeleteLocalJobFiles(string jobName)
		{
			List<string> paths = GetPossibleJobLocalPaths(jobName);

			foreach (string path in paths)
			{
				if (string.IsNullOrWhiteSpace(path))
				{
					continue;
				}

				if (Directory.Exists(path))
				{
					Directory.Delete(path, true);
					continue;
				}

				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
		}


		private void btnAddTask_Click(object sender, EventArgs e)
		{
			string protocolSelection = GetCurrentTriggerGridProtocolText();
			if (string.IsNullOrWhiteSpace(protocolSelection))
			{
				protocolSelection = GetDefaultCommunicationSelection();
			}

			string protocol = GetPrimaryProtocol(protocolSelection);
			string channel = GetCurrentTriggerGridChannelText();
			if (string.IsNullOrWhiteSpace(channel))
			{
				List<string> channels = LoadChannelOptionsForProtocols(protocolSelection);
				channel = channels.Count > 0 ? channels[0] : "Channel01";
			}
			string trigger = GetDefaultTriggerSource(protocolSelection, channel);
			string position = GetDefaultPositionSource(protocolSelection, channel);
			CommunicationChannelConfig channelConfig = GetChannelByNameForSelection(CommunicationConfigStore.LoadOrCreateDefault(), protocolSelection, channel);
			string triggerValue = channelConfig == null
				? "1"
				: channelConfig.TriggerExpectedValue;
			string positionValue = GetPositionExpectedValue(protocolSelection, channel, position);
			if (string.IsNullOrEmpty(positionValue)) positionValue = "1";
			int rowIndex = dgvTrigger.Rows.Add("Task_New_" + (dgvTrigger.Rows.Count + 1).ToString("00"), protocolSelection, channel, trigger, triggerValue, "Not Use", position, positionValue, string.Empty);
			UpdateChannelCellOptions(rowIndex, protocolSelection, channel);
			UpdateTriggerSourceCellOptions(rowIndex, protocolSelection, channel, trigger);
			UpdatePositionSourceCellOptions(rowIndex, protocolSelection, channel, position);
			dgvTrigger.Rows[rowIndex].Tag = new TaskGridRowTag
			{
				JobName = GetDefaultTaskJobName(),
				OriginalTaskName = string.Empty,
				OriginalProtocol = protocol,
				OriginalChannel = channel
			};
		}

		private void btnDeleteSelected_Click(object sender, EventArgs e)
		{
			if (dgvTrigger.SelectedRows.Count <= 0) return;
			foreach (DataGridViewRow row in dgvTrigger.SelectedRows)
				if (!row.IsNewRow) dgvTrigger.Rows.Remove(row);
		}

		private void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDir);

			if (!dir.Exists)
			{
				return;
			}

			if (!Directory.Exists(destDir))
			{
				Directory.CreateDirectory(destDir);
			}

			foreach (FileInfo file in dir.GetFiles())
			{
				string targetFilePath = Path.Combine(destDir, file.Name);
				file.CopyTo(targetFilePath, true);
			}

			if (!copySubDirs)
			{
				return;
			}

			foreach (DirectoryInfo subDir in dir.GetDirectories())
			{
				string newDestinationDir = Path.Combine(destDir, subDir.Name);
				DirectoryCopy(subDir.FullName, newDestinationDir, true);
			}
		}


		private ListBox GetJobListBox()
		{
			// Designer 文件里的真实控件名就是 listJobs，必须优先返回它。
			// 之前先找 lstJob / lstJobs，Job 列表为空时会返回 null，导致 + / - 按钮创建失败。
			if (listJobs != null)
			{
				return listJobs;
			}

			ListBox named = FindControlRecursive<ListBox>(this, "listJobs");

			if (named != null)
			{
				return named;
			}

			named = FindControlRecursive<ListBox>(this, "lstJob");

			if (named != null)
			{
				return named;
			}

			ListBox fallback = FindControlRecursive<ListBox>(this, "lstJobs");

			if (fallback != null)
			{
				return fallback;
			}

			return FindFirstJobLikeListBox(this);
		}


		private ListBox FindFirstJobLikeListBox(Control parent)
		{
			if (parent == null)
			{
				return null;
			}

			foreach (Control child in parent.Controls)
			{
				ListBox listBox = child as ListBox;

				if (listBox != null)
				{
					string text = string.Empty;

					foreach (object item in listBox.Items)
					{
						if (item != null)
						{
							text += item.ToString() + ";";
						}
					}

					if (text.IndexOf("Job", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return listBox;
					}
				}

				ListBox inner = FindFirstJobLikeListBox(child);

				if (inner != null)
				{
					return inner;
				}
			}

			return null;
		}


		private T FindControlRecursive<T>(Control parent, string name) where T : Control
		{
			if (parent == null)
			{
				return null;
			}

			foreach (Control child in parent.Controls)
			{
				T target = child as T;

				if (target != null && string.Equals(target.Name, name, StringComparison.OrdinalIgnoreCase))
				{
					return target;
				}

				T inner = FindControlRecursive<T>(child, name);

				if (inner != null)
				{
					return inner;
				}
			}

			return null;
		}

		private string GetSelectedJobNameSafe()
		{
			DataGridViewRow row = GetCurrentTriggerGridRow();
			TaskGridRowTag tag = row == null ? null : row.Tag as TaskGridRowTag;
			if (tag != null && !string.IsNullOrWhiteSpace(tag.JobName))
			{
				return tag.JobName;
			}

			ListBox jobList = GetJobListBox();

			if (jobList == null || jobList.SelectedItem == null)
			{
				return GetDefaultTaskJobName();
			}

			return jobList.SelectedItem.ToString();
		}

		private string GetSelectedProtocolNameSafe()
		{
			DataGridViewRow row = GetCurrentTriggerGridRow();
			if (row != null)
			{
				string protocolSelection = GetCellString(row, COL_PROTOCOL);
				if (!string.IsNullOrWhiteSpace(protocolSelection))
				{
					return GetPrimaryProtocol(protocolSelection);
				}
			}

			if (listProtocols != null && listProtocols.SelectedItem != null)
			{
				return listProtocols.SelectedItem.ToString();
			}

			return GetDefaultEnabledProtocol();
		}

		private string GetSelectedChannelNameSafe()
		{
			DataGridViewRow row = GetCurrentTriggerGridRow();
			if (row != null)
			{
				string channelName = GetCellString(row, COL_CHANNEL);
				if (!string.IsNullOrWhiteSpace(channelName))
				{
					return channelName;
				}
			}

			if (listChannels != null && listChannels.SelectedItem != null)
			{
				return listChannels.SelectedItem.ToString();
			}

			return GetDefaultChannelName(GetSelectedProtocolNameSafe());
		}

		private DataGridViewRow GetCurrentTriggerGridRow()
		{
			if (dgvTrigger == null)
			{
				return null;
			}

			if (dgvTrigger.CurrentRow != null && !dgvTrigger.CurrentRow.IsNewRow)
			{
				return dgvTrigger.CurrentRow;
			}

			if (dgvTrigger.SelectedRows.Count > 0 && !dgvTrigger.SelectedRows[0].IsNewRow)
			{
				return dgvTrigger.SelectedRows[0];
			}

			return null;
		}

		private List<JobConfig> GetJobsForSelectedProtocol(ProjectFlowConfig config, string jobName)
		{
			List<JobConfig> result = new List<JobConfig>();
			if (config == null || string.IsNullOrWhiteSpace(jobName))
			{
				return result;
			}

			string protocolName = GetSelectedProtocolNameSafe();
			string channelName = GetSelectedChannelNameSafe();
			ChannelFlowConfig channel = FlowConfigStore.GetChannel(config, protocolName, channelName);

			if (channel == null || channel.Jobs == null)
			{
				return result;
			}

			foreach (JobConfig job in channel.Jobs)
			{
				if (job != null && string.Equals(job.JobName, jobName, StringComparison.OrdinalIgnoreCase))
				{
					result.Add(job);
				}
			}

			return result;
		}

		private string GetProjectRootFolderForJobFile()
		{
			string projectRoot = ProjectPathStore.ProjectRoot;

			if (!Directory.Exists(projectRoot))
			{
				Directory.CreateDirectory(projectRoot);
			}

			return projectRoot;
		}


		private string GetFlowJobRootFolder()
		{
			return Path.Combine(ProjectPathStore.ProjectRoot, "Job");
		}


		private List<string> GetPossibleJobLocalPaths(string jobName)
		{
			List<string> paths = new List<string>();

			if (string.IsNullOrWhiteSpace(jobName))
			{
				return paths;
			}

			string normalizedJobName = NormalizeJobName(jobName);
			string protocolName = GetSelectedProtocolNameSafe();
			string channelName = GetSelectedChannelNameSafe();
			paths.Add(FlowConfigStore.PathManager.GetJobFolder(protocolName, channelName, normalizedJobName));

			return paths;
		}

		private void ShowAllTriggerRows()
		{
			if (dgvTrigger == null)
			{
				return;
			}

			foreach (DataGridViewRow row in dgvTrigger.Rows)
			{
				if (row == null || row.IsNewRow)
				{
					continue;
				}

				row.Visible = true;
			}
		}

		private void RefreshTriggerGridAfterSave()
		{
			CloseActiveComboPopup();

			if (dgvTrigger == null)
			{
				return;
			}

			dgvTrigger.EndEdit();
			ShowAllTriggerRows();
			RefreshComboColumnOptions();
			UpdateTriggerGridRowHeights();
			dgvTrigger.ClearSelection();

			if (dgvTrigger.Rows.Count > 0)
			{
				foreach (DataGridViewRow row in dgvTrigger.Rows)
				{
					if (row != null && !row.IsNewRow && row.Visible)
					{
						row.Selected = true;
						dgvTrigger.CurrentCell = row.Cells[0];
						break;
					}
				}
			}

			dgvTrigger.Invalidate();
		}


		private void btnSave_Click(object sender, EventArgs e)
		{
			CloseActiveComboPopup();
			dgvTrigger.EndEdit();
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			Dictionary<string, TaskConfig> oldTaskDict = CollectExistingTaskDictionary(config);
			ClearAllJobTasks(config);

			int runOrder = 1;
			foreach (DataGridViewRow row in dgvTrigger.Rows)
			{
				if (row.IsNewRow) continue;
				string taskName = GetCellString(row, COL_TASK_NAME);
				if (string.IsNullOrEmpty(taskName)) continue;

				TaskGridRowTag tag = row.Tag as TaskGridRowTag;
				string jobName = tag == null || string.IsNullOrWhiteSpace(tag.JobName)
					? GetDefaultTaskJobName(config)
					: NormalizeJobName(tag.JobName);
				string originalTaskName = tag == null ? string.Empty : tag.OriginalTaskName;
				if (string.IsNullOrWhiteSpace(originalTaskName))
				{
					originalTaskName = taskName;
				}

				string protocolSelection = JoinProtocolSelection(SplitProtocolSelection(GetCellString(row, COL_PROTOCOL)));
				List<string> protocols = SplitProtocolSelection(protocolSelection);
				string primaryProtocol = protocols.Count > 0 ? protocols[0] : GetDefaultEnabledProtocol();
				primaryProtocol = GetProtocolNameForCommunicationSelection(primaryProtocol);
				string primaryInstanceName = GetPrimaryInstanceName(protocolSelection);
				string rowChannel = GetCellString(row, COL_CHANNEL);
				if (string.IsNullOrWhiteSpace(rowChannel))
				{
					rowChannel = GetDefaultChannelName(primaryProtocol);
				}

				TaskConfig task = ResolveExistingTaskForRow(oldTaskDict, tag, taskName, jobName, primaryProtocol, rowChannel, runOrder);
				JobConfig rowJob = FlowConfigStore.GetOrCreateJob(config, primaryProtocol, rowChannel, jobName);
				RenameTaskFolderIfNeeded(
					tag == null ? primaryProtocol : tag.OriginalProtocol,
					tag == null ? rowChannel : tag.OriginalChannel,
					jobName,
					originalTaskName,
					taskName);
				task.TaskName = taskName;
				task.RunOrder = runOrder;
				task.Enabled = true;
				task.CommunicationProtocol = primaryProtocol;
				task.CommunicationChannel = rowChannel;
				if (string.IsNullOrEmpty(task.CommunicationChannel)) task.CommunicationChannel = "Channel01";
				task.CommunicationInstanceName = primaryInstanceName;
				task.TriggerName = GetCellString(row, COL_TRIGGER_NAME);
				task.TriggerValue = GetCellString(row, COL_TRIGGER_VALUE);
				if (string.IsNullOrEmpty(task.TriggerValue)) task.TriggerValue = "1";
				task.ImageSourceKey = "Not Use";
				task.InputAddress = string.Empty;
				task.PositionName = GetCellString(row, COL_POSITION_NAME);
				task.PositionOptionName = task.PositionName;
				task.PositionValue = GetCellString(row, COL_POSITION_VALUE);
				if (string.IsNullOrEmpty(task.PositionName)) task.PositionName = "Not Use";
				if (string.IsNullOrEmpty(task.PositionValue)) task.PositionValue = "1";

				// 旧字段同步保留，避免旧代码读取 FlagBit / FlagValue 时失效。
				int oldFlagBit;
				if (int.TryParse(task.PositionName, out oldFlagBit)) task.FlagBit = oldFlagBit;
				else task.FlagBit = 0;
				task.FlagValue = task.PositionValue;
				task.Remark = GetCellString(row, COL_REMARK);
				if (task.Steps == null) task.Steps = new List<StepConfig>();
				if (task.StepFlow == null) task.StepFlow = new List<StepFlowItem>();
				task.CommunicationTriggerBindings = BuildTriggerBindingsForRow(protocols, rowChannel, task);

				// 图像源取消选择后，历史调度行不能继续要求旧图像源。
				foreach (StepFlowItem flowItem in task.StepFlow)
				{
					if (flowItem != null)
					{
						flowItem.InputImageKey = string.Empty;
					}
				}

				rowJob.Tasks.Add(task);
				Directory.CreateDirectory(FlowConfigStore.PathManager.GetTaskFolder(primaryProtocol, rowChannel, jobName, taskName));
				row.Tag = new TaskGridRowTag
				{
					JobName = jobName,
					OriginalTaskName = taskName,
					OriginalProtocol = primaryProtocol,
					OriginalChannel = rowChannel
				};
				runOrder++;
			}
			FlowConfigStore.Save(config);
			RefreshTriggerGridAfterSave();

			MessageBox.Show("Task configuration saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private Dictionary<string, TaskConfig> CollectExistingTaskDictionary(ProjectFlowConfig config)
		{
			Dictionary<string, TaskConfig> result = new Dictionary<string, TaskConfig>(StringComparer.OrdinalIgnoreCase);

			foreach (JobConfig job in EnumerateAllJobs(config))
			{
				if (job == null || job.Tasks == null)
				{
					continue;
				}

				foreach (TaskConfig task in job.Tasks)
				{
					if (task == null || string.IsNullOrWhiteSpace(task.TaskName))
					{
						continue;
					}

					AddTaskToDictionary(result, BuildTaskKey(job.JobName, job.ProtocolName, job.ChannelName, task.TaskName), task);
					AddTaskToDictionary(result, task.TaskName, task);
				}
			}

			return result;
		}

		private void AddTaskToDictionary(Dictionary<string, TaskConfig> dict, string key, TaskConfig task)
		{
			if (dict == null || string.IsNullOrWhiteSpace(key) || task == null || dict.ContainsKey(key))
			{
				return;
			}

			dict.Add(key, task);
		}

		private void ClearAllJobTasks(ProjectFlowConfig config)
		{
			foreach (JobConfig job in EnumerateAllJobs(config))
			{
				if (job != null && job.Tasks != null)
				{
					job.Tasks.Clear();
				}
			}
		}

		private TaskConfig ResolveExistingTaskForRow(
			Dictionary<string, TaskConfig> oldTaskDict,
			TaskGridRowTag tag,
			string taskName,
			string jobName,
			string protocolName,
			string channelName,
			int runOrder)
		{
			string originalTaskName = tag == null ? string.Empty : tag.OriginalTaskName;
			string originalProtocol = tag == null ? protocolName : tag.OriginalProtocol;
			string originalChannel = tag == null ? channelName : tag.OriginalChannel;
			string key = BuildTaskKey(jobName, originalProtocol, originalChannel, originalTaskName);

			if (!string.IsNullOrWhiteSpace(key) && oldTaskDict.ContainsKey(key))
			{
				return oldTaskDict[key];
			}

			if (!string.IsNullOrWhiteSpace(originalTaskName) && oldTaskDict.ContainsKey(originalTaskName))
			{
				return oldTaskDict[originalTaskName];
			}

			if (!string.IsNullOrWhiteSpace(taskName) && oldTaskDict.ContainsKey(taskName))
			{
				return oldTaskDict[taskName];
			}

			return FlowConfigStore.CreateDefaultTask(jobName, taskName, runOrder);
		}

		private string BuildTaskKey(string jobName, string protocolName, string channelName, string taskName)
		{
			if (string.IsNullOrWhiteSpace(taskName))
			{
				return string.Empty;
			}

			return NormalizeJobName(jobName) + "|" +
				FlowConfigStore.NormalizeProtocolName(protocolName) + "|" +
				FlowConfigStore.NormalizeChannelName(channelName) + "|" +
				taskName.Trim();
		}

		private string GetDefaultTaskJobName()
		{
			return GetDefaultTaskJobName(FlowConfigStore.LoadOrCreateDefault());
		}

		private string GetDefaultTaskJobName(ProjectFlowConfig config)
		{
			JobConfig firstJob = EnumerateAllJobs(config)
				.FirstOrDefault(x => x != null && !string.IsNullOrWhiteSpace(x.JobName));

			if (firstJob != null)
			{
				return NormalizeJobName(firstJob.JobName);
			}

			return "Job_001";
		}

		private string GetDefaultCommunicationInstanceName(string protocolName)
		{
			CommunicationInstanceConfig instance = FindCommunicationInstance(protocolName);
			if (instance != null)
			{
				return instance.InstanceName;
			}

			instance = FindFirstCommunicationInstanceByProtocol(protocolName);
			if (instance != null)
			{
				return instance.InstanceName;
			}

			string normalized = FlowConfigStore.NormalizeProtocolName(protocolName);

			if (normalized.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				normalized.Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
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

		private List<TaskCommunicationTriggerBinding> BuildTriggerBindingsForRow(
			List<string> protocols,
			string channelName,
			TaskConfig task)
		{
			List<TaskCommunicationTriggerBinding> result = new List<TaskCommunicationTriggerBinding>();

			if (protocols == null || protocols.Count <= 0)
			{
				protocols = SplitProtocolSelection(GetDefaultCommunicationSelection());
			}

			foreach (string protocol in protocols)
			{
				if (string.IsNullOrWhiteSpace(protocol))
				{
					continue;
				}

				TaskCommunicationTriggerBinding binding = new TaskCommunicationTriggerBinding();
				binding.CommunicationProtocol = GetProtocolNameForCommunicationSelection(protocol);
				binding.CommunicationChannel = string.IsNullOrWhiteSpace(channelName) ? "Channel01" : channelName;
				binding.CommunicationInstanceName = GetInstanceNameForCommunicationSelection(protocol);
				binding.TriggerName = task.TriggerName;
				binding.TriggerValue = task.TriggerValue;
				binding.TriggerCompare = task.TriggerCompare;
				binding.PositionName = task.PositionName;
				binding.PositionValue = task.PositionValue;
				binding.PositionCompare = task.PositionCompare;
				result.Add(binding);
			}

			return result;
		}

		private void RenameTaskFolderIfNeeded(string jobName, string oldTaskName, string newTaskName)
		{
			RenameTaskFolderIfNeeded("TCP/IP", "Channel01", jobName, oldTaskName, newTaskName);
		}

		private void RenameTaskFolderIfNeeded(string protocolName, string channelName, string jobName, string oldTaskName, string newTaskName)
		{
			if (string.IsNullOrWhiteSpace(jobName) ||
				string.IsNullOrWhiteSpace(oldTaskName) ||
				string.IsNullOrWhiteSpace(newTaskName) ||
				string.Equals(oldTaskName, newTaskName, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			string oldFolder = FlowConfigStore.PathManager.GetTaskFolder(protocolName, channelName, jobName, oldTaskName);
			string newFolder = FlowConfigStore.PathManager.GetTaskFolder(protocolName, channelName, jobName, newTaskName);

			if (!Directory.Exists(oldFolder))
			{
				return;
			}

			if (!Directory.Exists(newFolder))
			{
				Directory.Move(oldFolder, newFolder);
				return;
			}

			MergeDirectory(oldFolder, newFolder);

			try
			{
				Directory.Delete(oldFolder, true);
			}
			catch
			{
			}
		}

		private string GetNextProgramNo(ProjectFlowConfig config, string protocolName, string channelName)
		{
			int index = 1;
			List<JobConfig> jobs = FlowConfigStore.GetJobs(config, protocolName, channelName);

			while (jobs != null && jobs.Any(j => string.Equals(j == null ? string.Empty : j.ProgramNo, index.ToString(), StringComparison.OrdinalIgnoreCase)))
			{
				index++;
			}

			return index.ToString();
		}

		private void MergeDirectory(string sourceFolder, string targetFolder)
		{
			if (string.IsNullOrWhiteSpace(sourceFolder) ||
				string.IsNullOrWhiteSpace(targetFolder) ||
				!Directory.Exists(sourceFolder))
			{
				return;
			}

			Directory.CreateDirectory(targetFolder);

			foreach (string file in Directory.GetFiles(sourceFolder))
			{
				string target = Path.Combine(targetFolder, Path.GetFileName(file));
				if (File.Exists(target))
				{
					File.Delete(target);
				}
				File.Move(file, target);
			}

			foreach (string dir in Directory.GetDirectories(sourceFolder))
			{
				string target = Path.Combine(targetFolder, Path.GetFileName(dir));
				if (Directory.Exists(target))
				{
					MergeDirectory(dir, target);
					try
					{
						Directory.Delete(dir, true);
					}
					catch
					{
					}
				}
				else
				{
					Directory.Move(dir, target);
				}
			}
		}

		private void MoveLegacyTaskFoldersUnderTaskFolder(string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			string protocolName = GetSelectedProtocolNameSafe();

			foreach (JobConfig job in GetJobsForSelectedProtocol(config, jobName))
			{
				if (job == null || job.Tasks == null)
				{
					continue;
				}

				string jobFolder = FlowConfigStore.PathManager.GetJobFolder(protocolName, job.ChannelName, jobName);
				foreach (TaskConfig task in job.Tasks)
				{
					if (task == null || string.IsNullOrWhiteSpace(task.TaskName))
					{
						continue;
					}

					string legacyFolder = Path.Combine(jobFolder, task.TaskName);
					string newFolder = FlowConfigStore.PathManager.GetTaskFolder(protocolName, job.ChannelName, jobName, task.TaskName);

					if (!Directory.Exists(legacyFolder))
					{
						continue;
					}

					if (!Directory.Exists(newFolder))
					{
						Directory.CreateDirectory(newFolder);
					}

					foreach (string file in Directory.GetFiles(legacyFolder))
					{
						string target = Path.Combine(newFolder, Path.GetFileName(file));

						if (File.Exists(target))
						{
							File.Delete(target);
						}

						File.Move(file, target);
					}

					foreach (string dir in Directory.GetDirectories(legacyFolder))
					{
						string target = Path.Combine(newFolder, Path.GetFileName(dir));

						if (Directory.Exists(target))
						{
							Directory.Delete(target, true);
						}

						Directory.Move(dir, target);
					}

					try
					{
						Directory.Delete(legacyFolder, true);
					}
					catch
					{
					}
				}
			}
		}

		private void SelectListItem(ListBox listBox, string itemText)
		{
			if (listBox == null || string.IsNullOrEmpty(itemText)) return;
			for (int i = 0; i < listBox.Items.Count; i++)
			{
				if (string.Equals(listBox.Items[i].ToString(), itemText, StringComparison.OrdinalIgnoreCase)) { listBox.SelectedIndex = i; return; }
			}
			if (listBox.Items.Count > 0 && listBox.SelectedIndex < 0) listBox.SelectedIndex = 0;
		}

		private string GetCellString(DataGridViewRow row, int columnIndex)
		{
			if (row.Cells[columnIndex].Value == null) return string.Empty;
			return row.Cells[columnIndex].Value.ToString().Trim();
		}

		private void CommunicationConfigChangedHub_ConfigChanged(object sender, EventArgs e)
		{
			if (this.IsDisposed)
			{
				return;
			}

			if (this.InvokeRequired)
			{
				try
				{
					this.BeginInvoke(new MethodInvoker(delegate
					{
						CommunicationConfigChangedHub_ConfigChanged(sender, e);
					}));
				}
				catch
				{
				}

				return;
			}

			if (this.Visible)
			{
				RefreshByCommunicationConfigChanged();
			}
		}

		private void TriggerManagerControl_VisibleChanged(object sender, EventArgs e)
		{
			CloseActiveComboPopup();
			if (this.Visible)
			{
				RefreshByCommunicationConfigChanged();
			}
		}

		public void RefreshByCommunicationConfigChanged()
		{
			if (dgvTrigger == null || dgvTrigger.IsDisposed)
			{
				return;
			}

			try
			{
				string oldProtocol = GetSelectedProtocolNameSafe();
				string oldChannel = GetSelectedChannelNameSafe();
				string oldJob = GetSelectedJobNameSafe();
				LoadFlowConfigToJobList();
				SelectListItem(listProtocols, oldProtocol);
				SelectListItem(listChannels, oldChannel);
				SelectListItem(listJobs, oldJob);
				dgvTrigger.SuspendLayout();

				RefreshComboColumnOptions();

				for (int i = 0; i < dgvTrigger.Rows.Count; i++)
				{
					DataGridViewRow row = dgvTrigger.Rows[i];

					if (row == null || row.IsNewRow)
					{
						continue;
					}

					string protocol = GetCellString(row, COL_PROTOCOL);
					string channel = GetCellString(row, COL_CHANNEL);
					string triggerName = GetCellString(row, COL_TRIGGER_NAME);
					string positionName = GetCellString(row, COL_POSITION_NAME);

					UpdateChannelCellOptions(i, protocol, channel);
					channel = GetCellString(row, COL_CHANNEL);
					UpdateTriggerSourceCellOptions(i, protocol, channel, triggerName);
					UpdatePositionSourceCellOptions(i, protocol, channel, positionName);
				}

				dgvTrigger.Invalidate();
			}
			finally
			{
				dgvTrigger.ResumeLayout(true);
			}
		}

		public List<string> DebugGetCurrentImageSourceItems()
		{
			return GetAllCameraImageSourcesFromFiles();
		}


		public void RefreshImageSourcesFromHardware()
		{
			RefreshImageSourceColumnItems();
		}


		protected override void OnHandleDestroyed(EventArgs e)
		{
			CommunicationConfigChangedHub.ConfigChanged -= CommunicationConfigChangedHub_ConfigChanged;
			this.VisibleChanged -= TriggerManagerControl_VisibleChanged;
			base.OnHandleDestroyed(e);
		}

		private void TestSelectedTask()
		{
			CloseActiveComboPopup();
			dgvTrigger.EndEdit();

			string jobName = GetSelectedJobNameSafe();
			string taskName = GetSelectedTaskNameSafe();

			if (string.IsNullOrWhiteSpace(jobName))
			{
				MessageBox.Show("Please select one Job first.", "Task Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			if (string.IsNullOrWhiteSpace(taskName))
			{
				MessageBox.Show("Please select one Task first.", "Task Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

			if (config == null || config.Jobs == null)
			{
				MessageBox.Show("Flow configuration was not found.", "Task Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string protocolName = GetPrimaryProtocol(GetCurrentTriggerGridProtocolText());
			string channelName = GetCurrentTriggerGridChannelText();
			JobConfig job = FlowConfigStore.GetJobs(config, protocolName, channelName)
				.FirstOrDefault(j => j != null && string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));
			if (job == null)
			{
				job = GetJobsForSelectedProtocol(config, jobName).FirstOrDefault();
			}

			if (job == null)
			{
				MessageBox.Show("Job not found: " + jobName, "Task Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			TaskConfig task = job.Tasks == null ? null : job.Tasks.FirstOrDefault(t =>
				t != null && string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase));

			if (task == null)
			{
				MessageBox.Show("Task not found: " + taskName, "Task Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			List<string> imageSources = CollectTaskReplayImageSources(task);

			using (TaskTestDialog dialog = new TaskTestDialog(taskName, imageSources))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				TaskRunOptions options = TaskRunOptions.Test(dialog.Options.EnableCommunicationOutput);

				foreach (TaskTestImageSource item in dialog.Options.ImageSources)
				{
					object image = LoadLocalImageForTaskTest(item.LocalImagePath);

					if (image != null)
					{
						options.OverrideImageSources[item.ImageSourceName] = image;
					}
				}

				try
				{
					bool executed = ExecuteTaskTest(jobName, taskName, options);

					if (executed)
					{
						MessageBox.Show("Task test finished.", "Task Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Task test failed:\r\n" + ex.Message, "Task Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		private string GetCurrentTriggerGridImageSourceText()
		{
			if (dgvTrigger == null)
			{
				return string.Empty;
			}

			DataGridViewRow row = null;

			if (dgvTrigger.CurrentRow != null && !dgvTrigger.CurrentRow.IsNewRow)
			{
				row = dgvTrigger.CurrentRow;
			}
			else if (dgvTrigger.SelectedRows.Count > 0 && !dgvTrigger.SelectedRows[0].IsNewRow)
			{
				row = dgvTrigger.SelectedRows[0];
			}

			if (row == null)
			{
				return string.Empty;
			}

			try
			{
				if (dgvTrigger.Columns.Count > COL_IMAGE_SOURCE)
				{
					object value = row.Cells[COL_IMAGE_SOURCE].Value;
					return value == null ? string.Empty : Convert.ToString(value);
				}
			}
			catch
			{
			}

			return string.Empty;
		}

		private string GetCurrentTriggerGridProtocolText()
		{
			DataGridViewRow row = GetCurrentTriggerGridRow();
			if (row == null)
			{
				return string.Empty;
			}

			try
			{
				object value = row.Cells[COL_PROTOCOL].Value;
				return value == null ? string.Empty : Convert.ToString(value);
			}
			catch
			{
				return string.Empty;
			}
		}

		private string GetCurrentTriggerGridChannelText()
		{
			DataGridViewRow row = GetCurrentTriggerGridRow();
			if (row != null)
			{
				try
				{
					object value = row.Cells[COL_CHANNEL].Value;
					string channel = value == null ? string.Empty : Convert.ToString(value);
					if (!string.IsNullOrWhiteSpace(channel))
					{
						return channel;
					}
				}
				catch
				{
				}
			}

			return GetSelectedChannelNameSafe();
		}



		private bool ExecuteTaskTest(string jobName, string taskName, TaskRunOptions options)
		{
			if (TaskTestExecutor != null)
			{
				return TaskTestExecutor(jobName, taskName, options);
			}

			EventHandler<TaskTestRequestedEventArgs> handler = TaskTestRequested;

			if (handler != null)
			{
				TaskTestRequestedEventArgs args = new TaskTestRequestedEventArgs(jobName, taskName, options);
				handler(this, args);

				if (args.Error != null)
				{
					throw args.Error;
				}

				if (args.Handled)
				{
					return true;
				}
			}

			MessageBox.Show(
				"Task test options are ready, but the real task execution entrance has not been bound.\r\n\r\n" +
				"Please bind TriggerManagerControl.TaskTestExecutor in Form1 or FlowConfigForm.",
				"Task Test",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);

			return false;
		}

		private List<string> CollectTaskImageSources(TaskConfig task)
		{
			List<string> result = new List<string>();

			if (task == null)
			{
				return result;
			}

			// 触发管理表格中的图像源字段。
			// 如果你的 TaskConfig 没有 ImageSourceKey，则删除这一行。
			AddImageSourceName(result, task.ImageSourceKey);

			if (task.StepFlow != null)
			{
				foreach (StepFlowItem step in task.StepFlow)
				{
					if (step == null)
					{
						continue;
					}

					// 右侧 Task 调度 Step 表格里的图像源。
					// 如果你的 StepFlowItem 没有 InputImageKey，则删除这一行。
					AddImageSourceName(result, step.InputImageKey);
				}
			}

			return result;
		}

		private List<string> CollectTaskReplayImageSources(TaskConfig task)
		{
			List<string> hardwareSources = CollectTaskHardwareImageSources(task);
			if (hardwareSources.Count > 0)
			{
				return hardwareSources;
			}

			return CollectTaskImageSources(task);
		}

		private List<string> CollectTaskHardwareImageSources(TaskConfig task)
		{
			List<string> result = new List<string>();

			if (task == null || task.StepFlow == null)
			{
				return result;
			}

			foreach (StepFlowItem flowItem in task.StepFlow
				.Where(x => x != null && x.Enabled)
				.OrderBy(x => x.RunOrder))
			{
				if (!IsHardwareFlowBlock(flowItem))
				{
					continue;
				}

				AddImageSourceName(result, GetHardwareFlowImageSourceName(flowItem));
			}

			return result;
		}

		private bool IsHardwareFlowBlock(StepFlowItem flowItem)
		{
			return flowItem != null &&
				string.Equals(flowItem.BlockType, "Hardware", StringComparison.OrdinalIgnoreCase);
		}

		private string GetHardwareFlowImageSourceName(StepFlowItem flowItem)
		{
			if (flowItem == null)
			{
				return string.Empty;
			}

			if (!string.IsNullOrWhiteSpace(flowItem.BlockName))
			{
				return flowItem.BlockName.Trim();
			}

			if (!string.IsNullOrWhiteSpace(flowItem.StepName))
			{
				return flowItem.StepName.Trim();
			}

			string path = flowItem.BlockPath;
			if (string.IsNullOrWhiteSpace(path))
			{
				path = flowItem.Remark;
			}

			return ConvertHardwarePathToImageSourceName(path);
		}

		private string ConvertHardwarePathToImageSourceName(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			string text = path.Trim();
			try
			{
				text = Path.GetFileName(text);
				string folder = Path.GetFileName(Path.GetDirectoryName(path));
				if (!string.IsNullOrWhiteSpace(folder))
				{
					text = folder + "." + text;
				}
			}
			catch
			{
			}

			return text
				.Replace(Path.DirectorySeparatorChar, '.')
				.Replace(Path.AltDirectorySeparatorChar, '.')
				.Trim('.');
		}

		private bool IsNotUseSelection(string value)
		{
			return string.IsNullOrWhiteSpace(value) ||
				string.Equals(value.Trim(), "Not Use", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(value.Trim(), "None", StringComparison.OrdinalIgnoreCase);
		}




		private string GetSelectedTaskNameSafe()
		{
			if (dgvTrigger == null || dgvTrigger.Rows.Count <= 0)
			{
				return string.Empty;
			}

			DataGridViewRow row = null;

			if (dgvTrigger.CurrentRow != null && !dgvTrigger.CurrentRow.IsNewRow)
			{
				row = dgvTrigger.CurrentRow;
			}
			else if (dgvTrigger.SelectedRows.Count > 0 && !dgvTrigger.SelectedRows[0].IsNewRow)
			{
				row = dgvTrigger.SelectedRows[0];
			}
			else
			{
				foreach (DataGridViewRow item in dgvTrigger.Rows)
				{
					if (item != null && !item.IsNewRow)
					{
						row = item;
						break;
					}
				}
			}

			if (row == null)
			{
				return string.Empty;
			}

			return GetCellString(row, COL_TASK_NAME);
		}

		private object LoadLocalImageForTaskTest(string imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath))
			{
				return null;
			}

			if (!File.Exists(imagePath))
			{
				return null;
			}

			CogImageFile imageFile = null;

			try
			{
				imageFile = new CogImageFile();
				imageFile.Open(imagePath, CogImageFileModeConstants.Read);
				return imageFile[0];
			}
			finally
			{
				if (imageFile != null)
				{
					imageFile.Close();
				}
			}
		}

		private void AddImageSourceName(List<string> result, string sourceName)
		{
			if (result == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(sourceName))
			{
				return;
			}

			string normalized = sourceName.Replace("\r\n", "\n")
										  .Replace("\r", "\n")
										  .Replace("；", ";")
										  .Replace("，", ",")
										  .Replace("、", ";");

			string[] parts = normalized.Split(
				new char[] { ';', ',', '|', '\n', '\t' },
				StringSplitOptions.RemoveEmptyEntries);

			foreach (string part in parts)
			{
				string item = part.Trim();

				if (string.IsNullOrWhiteSpace(item))
				{
					continue;
				}

				if (string.Equals(item, "Not Use", StringComparison.OrdinalIgnoreCase) ||
					string.Equals(item, "None", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!result.Exists(x => string.Equals(x, item, StringComparison.OrdinalIgnoreCase)))
				{
					result.Add(item);
				}
			}
		}




		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;
			if (dgvTrigger.Columns.Count <= COL_REMARK) return;

			if (isEnglish)
			{
				if (lblTriggerTitle != null) lblTriggerTitle.Text = "Task Settings";
				dgvTrigger.Columns[COL_TASK_NAME].HeaderText = "Task Name";
				dgvTrigger.Columns[COL_PROTOCOL].HeaderText = "Protocol";
				dgvTrigger.Columns[COL_CHANNEL].HeaderText = "Channel";
				dgvTrigger.Columns[COL_TRIGGER_NAME].HeaderText = "Trigger Source";
				dgvTrigger.Columns[COL_TRIGGER_VALUE].HeaderText = "Trigger Value";
				dgvTrigger.Columns[COL_POSITION_NAME].HeaderText = "Position No.";
				dgvTrigger.Columns[COL_POSITION_VALUE].HeaderText = "Position Value";
				dgvTrigger.Columns[COL_REMARK].HeaderText = "Remark";
				btnAddTask.Text = "+ Add Task";
				btnDeleteSelected.Text = "Delete";
				btnSave.Text = "Save";
				if (btnTestTask != null) btnTestTask.Text = "Task Test";
			}
			else
			{
				if (lblTriggerTitle != null) lblTriggerTitle.Text = "任务设置";
				dgvTrigger.Columns[COL_TASK_NAME].HeaderText = "task名称";
				dgvTrigger.Columns[COL_PROTOCOL].HeaderText = "协议";
				dgvTrigger.Columns[COL_CHANNEL].HeaderText = "通道";
				dgvTrigger.Columns[COL_TRIGGER_NAME].HeaderText = "触发源";
				dgvTrigger.Columns[COL_TRIGGER_VALUE].HeaderText = "触发源值";
				dgvTrigger.Columns[COL_POSITION_NAME].HeaderText = "位置号";
				dgvTrigger.Columns[COL_POSITION_VALUE].HeaderText = "位置号值";
				dgvTrigger.Columns[COL_REMARK].HeaderText = "备注";
				btnAddTask.Text = "+ 新增 task";
				btnDeleteSelected.Text = "▦ 删除选中";
				btnSave.Text = "▣ 保存";
				if (btnTestTask != null) btnTestTask.Text = "▶ Task测试";
			}
		}
	}



	public class InputTextDialog : Form
	{
		private readonly TextBox _textBox;
		private readonly Button _btnOk;
		private readonly Button _btnCancel;

		public string InputValue
		{
			get { return _textBox.Text; }
		}

		public InputTextDialog(string title, string labelText, string defaultValue)
		{
			this.Text = title;
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.ClientSize = new Size(520, 210);
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);
			this.KeyPreview = true;

			Label label = new Label();
			label.Text = labelText;
			label.ForeColor = Color.FromArgb(220, 235, 245);
			label.Location = new Point(42, 58);
			label.Size = new Size(135, 30);
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

			_textBox = new TextBox();
			_textBox.Text = defaultValue == null ? string.Empty : defaultValue;
			_textBox.Location = new Point(180, 58);
			_textBox.Size = new Size(300, 30);
			_textBox.BackColor = Color.FromArgb(3, 14, 27);
			_textBox.ForeColor = Color.White;
			_textBox.BorderStyle = BorderStyle.FixedSingle;
			_textBox.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular);
			_textBox.Margin = new Padding(0);

			_btnOk = CreateDialogButton("OK", 170, 135, true);
			_btnCancel = CreateDialogButton("Cancel", 310, 135, false);

			_btnOk.Click += btnOk_Click;

			_btnCancel.Click += delegate
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close();
			};

			this.Controls.Add(label);
			this.Controls.Add(_textBox);
			this.Controls.Add(_btnOk);
			this.Controls.Add(_btnCancel);

			this.AcceptButton = _btnOk;
			this.CancelButton = _btnCancel;

			this.Shown += delegate
			{
				_textBox.Focus();
				_textBox.SelectionStart = _textBox.Text.Length;
				_textBox.SelectionLength = 0;
			};
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_textBox.Text))
			{
				MessageBox.Show("Job name cannot be empty.", "Add Job", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				_textBox.Focus();
				return;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private Button CreateDialogButton(string text, int x, int y, bool primary)
		{
			Button button = new Button();
			button.Text = text;
			button.Location = new Point(x, y);
			button.Size = new Size(110, 38);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(20, 110, 235) : Color.FromArgb(8, 35, 60);
			button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(0, 75, 180) : Color.FromArgb(5, 25, 45);
			button.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
			button.Cursor = Cursors.Hand;
			button.UseVisualStyleBackColor = false;
			return button;
		}
	}



	public class ComboLikePopupForm : Form
	{
		private readonly ListBox _listBox;
		private readonly List<string> _items;

		public event Action<string> ValueSelected;

		public string SelectedValue { get; private set; }

		public ComboLikePopupForm(List<string> items, string currentValue, int width, int height)
		{
			_items = items == null ? new List<string>() : items;
			SelectedValue = string.Empty;

			this.FormBorderStyle = FormBorderStyle.None;
			this.ShowInTaskbar = false;
			this.TopMost = false;
			this.ClientSize = new Size(width, height);
			this.MinimumSize = new Size(width, height);
			this.MaximumSize = new Size(width, height);
			this.BackColor = Color.FromArgb(38, 62, 86);
			this.Padding = new Padding(1);
			this.Deactivate += ComboLikePopupForm_Deactivate;

			_listBox = new ListBox();
			_listBox.Dock = DockStyle.Fill;
			_listBox.BorderStyle = BorderStyle.None;
			_listBox.BackColor = Color.FromArgb(3, 14, 27);
			_listBox.ForeColor = Color.White;
			_listBox.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			_listBox.ItemHeight = 28;
			_listBox.IntegralHeight = false;
			_listBox.DrawMode = DrawMode.OwnerDrawFixed;

			_listBox.DrawItem += ListBox_DrawItem;
			_listBox.MouseDown += ListBox_MouseDown;
			_listBox.KeyDown += ListBox_KeyDown;

			this.Controls.Add(_listBox);

			foreach (string item in _items)
			{
				if (!string.IsNullOrWhiteSpace(item))
				{
					_listBox.Items.Add(item);
				}
			}

			if (!string.IsNullOrWhiteSpace(currentValue))
			{
				for (int i = 0; i < _listBox.Items.Count; i++)
				{
					if (string.Equals(_listBox.Items[i].ToString(), currentValue, StringComparison.OrdinalIgnoreCase))
					{
						_listBox.SelectedIndex = i;
						break;
					}
				}
			}

			if (_listBox.SelectedIndex < 0 && _listBox.Items.Count > 0)
			{
				_listBox.SelectedIndex = 0;
			}
		}

		private void ComboLikePopupForm_Deactivate(object sender, EventArgs e)
		{
			this.Close();
		}

		private void ListBox_MouseDown(object sender, MouseEventArgs e)
		{
			int index = _listBox.IndexFromPoint(e.Location);

			if (index < 0 || index >= _listBox.Items.Count)
			{
				this.Close();
				return;
			}

			_listBox.SelectedIndex = index;
			ConfirmSelection();
		}

		private void ListBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				ConfirmSelection();
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Escape)
			{
				this.Close();
				e.Handled = true;
			}
		}

		private void ConfirmSelection()
		{
			if (_listBox.SelectedItem == null)
			{
				return;
			}

			SelectedValue = _listBox.SelectedItem.ToString();

			Action<string> handler = ValueSelected;

			if (handler != null)
			{
				handler(SelectedValue);
			}

			this.Close();
		}

		private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0 || e.Index >= _listBox.Items.Count)
			{
				return;
			}

			bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
			Color backColor = selected ? Color.FromArgb(0, 120, 200) : Color.FromArgb(3, 14, 27);
			Color foreColor = Color.White;

			using (SolidBrush backBrush = new SolidBrush(backColor))
			{
				e.Graphics.FillRectangle(backBrush, e.Bounds);
			}

			string text = _listBox.Items[e.Index].ToString();

			TextRenderer.DrawText(
				e.Graphics,
				text,
				_listBox.Font,
				e.Bounds,
				foreColor,
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis);

			using (Pen borderPen = new Pen(Color.FromArgb(38, 62, 86)))
			{
				e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
			}
		}
	}


	public class ImageSourceMultiSelectForm : Form
	{
		private readonly CheckedListBox _list;
		private readonly Button _btnOk;
		private readonly Button _btnCancel;
		private readonly Button _btnClear;
		private readonly Label _lblTitle;

		public List<string> SelectedImageSources { get; private set; }

		public ImageSourceMultiSelectForm(List<string> allSources, List<string> selectedSources)
		{
			SelectedImageSources = new List<string>();

			this.Text = "Select Image Sources";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(520, 520);
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);

			_lblTitle = new Label();
			_lblTitle.Text = "Select one or more image sources";
			_lblTitle.ForeColor = Color.FromArgb(220, 235, 245);
			_lblTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
			_lblTitle.Location = new Point(22, 18);
			_lblTitle.Size = new Size(460, 28);

			_list = new CheckedListBox();
			_list.CheckOnClick = true;
			_list.BorderStyle = BorderStyle.FixedSingle;
			_list.BackColor = Color.FromArgb(3, 14, 27);
			_list.ForeColor = Color.FromArgb(220, 235, 245);
			_list.Location = new Point(22, 58);
			_list.Size = new Size(476, 360);

			_btnClear = CreateButton("Clear", 22, 450, false);
			_btnOk = CreateButton("OK", 300, 450, true);
			_btnCancel = CreateButton("Cancel", 408, 450, false);

			_btnClear.Click += delegate
			{
				for (int i = 0; i < _list.Items.Count; i++)
				{
					_list.SetItemChecked(i, false);
				}
			};

			_btnOk.Click += delegate
			{
				SelectedImageSources.Clear();

				foreach (object item in _list.CheckedItems)
				{
					if (item != null)
					{
						SelectedImageSources.Add(item.ToString());
					}
				}

				this.DialogResult = DialogResult.OK;
				this.Close();
			};

			_btnCancel.Click += delegate
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close();
			};

			this.Controls.Add(_lblTitle);
			this.Controls.Add(_list);
			this.Controls.Add(_btnClear);
			this.Controls.Add(_btnOk);
			this.Controls.Add(_btnCancel);

			LoadSources(allSources, selectedSources);
		}

		private void LoadSources(List<string> allSources, List<string> selectedSources)
		{
			_list.Items.Clear();

			if (allSources == null)
			{
				return;
			}

			foreach (string source in allSources)
			{
				if (string.IsNullOrWhiteSpace(source))
				{
					continue;
				}

				if (source.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				bool isChecked = selectedSources != null &&
					selectedSources.Any(x => string.Equals(x, source, StringComparison.OrdinalIgnoreCase));

				_list.Items.Add(source, isChecked);
			}
		}

		private Button CreateButton(string text, int x, int y, bool primary)
		{
			Button button = new Button();
			button.Text = text;
			button.Location = new Point(x, y);
			button.Size = new Size(90, 34);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			button.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			button.ForeColor = Color.White;
			return button;
		}
	}

}
