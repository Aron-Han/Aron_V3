using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Aron_V3
{
	public partial class TriggerManagerControl : UserControl, ILocalizable
	{

		private Panel panelJobButtons;
		private Button btnAddJob;
		private Button btnDeleteJob;

		private ComboLikePopupForm _activeComboPopup;

		private const int COL_TASK_NAME = 0;
		private const int COL_PROTOCOL = 1;
		private const int COL_TRIGGER_NAME = 2;
		private const int COL_TRIGGER_VALUE = 3;
		private const int COL_IMAGE_SOURCE = 4;
		private const int COL_POSITION_NAME = 5;
		private const int COL_POSITION_VALUE = 6;
		private const int COL_REMARK = 7;

		private bool _loading = false;
		private bool _isEnglish = false;
		private bool _refreshComboPending = false;

		public TriggerManagerControl()
		{
			InitializeComponent();


			CommunicationConfigChangedHub.ConfigChanged += CommunicationConfigChangedHub_ConfigChanged;
			this.VisibleChanged += TriggerManagerControl_VisibleChanged;

			ConfigureTriggerGrid();
			CreateJobActionButtons();

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
			CreateJobActionButtons();
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

			DataGridViewComboBoxColumn colProtocol = new DataGridViewComboBoxColumn();
			colProtocol.Name = "colProtocol";
			colProtocol.HeaderText = "通讯协议";
			colProtocol.FillWeight = 90;
			colProtocol.FlatStyle = FlatStyle.Flat;
			colProtocol.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			dgvTrigger.Columns.Add(colProtocol);

			DataGridViewComboBoxColumn colTrigger = new DataGridViewComboBoxColumn();
			colTrigger.Name = "colTriggerName";
			colTrigger.HeaderText = "触发源名称";
			colTrigger.FillWeight = 115;
			colTrigger.FlatStyle = FlatStyle.Flat;
			colTrigger.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			dgvTrigger.Columns.Add(colTrigger);

			DataGridViewTextBoxColumn colTriggerValue = new DataGridViewTextBoxColumn();
			colTriggerValue.Name = "colTriggerValue";
			colTriggerValue.HeaderText = "触发源值";
			colTriggerValue.FillWeight = 90;
			dgvTrigger.Columns.Add(colTriggerValue);

			DataGridViewTextBoxColumn colImage = new DataGridViewTextBoxColumn();
			colImage.Name = "colImageSource";
			colImage.HeaderText = "图像源";
			colImage.FillWeight = 180;
			colImage.ReadOnly = true;
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
			dgvTrigger.Columns[COL_TRIGGER_NAME].ReadOnly = true;
			dgvTrigger.Columns[COL_IMAGE_SOURCE].ReadOnly = true;
			dgvTrigger.Columns[COL_POSITION_NAME].ReadOnly = true;

			// 普通值列需要可以直接编辑。
			dgvTrigger.Columns[COL_TRIGGER_VALUE].ReadOnly = false;
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

				string imageSourceText = GetCellString(row, COL_IMAGE_SOURCE);
				int lineCount = 1;

				if (!string.IsNullOrWhiteSpace(imageSourceText) &&
					!imageSourceText.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
				{
					lineCount = SplitImageSourceSelection(imageSourceText).Count;

					if (lineCount <= 0)
					{
						lineCount = 1;
					}
				}

				// 34 是单行基础高度；每增加一个图像源，行高增加约 22 像素。
				int targetHeight = Math.Max(34, 26 + lineCount * 22);
				row.Height = targetHeight;
			}
		}



		private void RefreshComboColumnOptions()
		{
			List<string> protocols = LoadEnabledProtocolOptions();

			DataGridViewComboBoxColumn protocolCol = dgvTrigger.Columns[COL_PROTOCOL] as DataGridViewComboBoxColumn;

			if (protocolCol != null)
			{
				protocolCol.Items.Clear();

				foreach (string protocol in protocols)
				{
					protocolCol.Items.Add(protocol);
				}
			}

			// 图像源只允许从相机目录真实文件生成。
			// 不再使用旧的 HardwareConfig.xml 解析逻辑，避免 Cam1 被转换成 Cam1.Raw。
			RefreshImageSourceColumnItems();
		}

		private void FlowConfigStore_FlowConfigSaved(object sender, EventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new EventHandler(FlowConfigStore_FlowConfigSaved), sender, e);
				return;
			}
			string oldJob = GetSelectedJobNameSafe();
			LoadFlowConfigToJobList();
			SelectListItem(listJobs, oldJob);
			LoadCurrentJobTasksToGrid();
		}

		private void LoadFlowConfigToJobList()
		{
			_loading = true;

			try
			{
				string oldJob = GetSelectedJobNameSafe();

				listJobs.Items.Clear();
				dgvTrigger.Rows.Clear();

				List<string> jobs = GetAllLocalJobNames();

				foreach (string jobName in jobs)
				{
					listJobs.Items.Add(jobName);
				}

				if (!string.IsNullOrWhiteSpace(oldJob))
				{
					SelectListItem(listJobs, oldJob);
				}

				if (listJobs.SelectedIndex < 0 && listJobs.Items.Count > 0)
				{
					listJobs.SelectedIndex = 0;
				}

				LoadCurrentJobTasksToGrid();
			}
			finally
			{
				_loading = false;
				CreateJobActionButtons();
			}
		}


		private List<string> GetAllLocalJobNames()
		{
			List<string> jobs = new List<string>();

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

			if (config != null && config.Jobs != null)
			{
				foreach (JobConfig job in config.Jobs)
				{
					if (job == null || string.IsNullOrWhiteSpace(job.JobName))
					{
						continue;
					}

					AddJobNameIfNotExists(jobs, NormalizeJobName(job.JobName));
				}
			}

			string jobRoot = GetFlowJobRootFolder();

			if (Directory.Exists(jobRoot))
			{
				foreach (string dir in Directory.GetDirectories(jobRoot))
				{
					string name = Path.GetFileName(dir);

					if (string.IsNullOrWhiteSpace(name))
					{
						continue;
					}

					AddJobNameIfNotExists(jobs, NormalizeJobName(name));
				}
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

		private void LoadCurrentJobTasksToGrid()
		{
			dgvTrigger.Rows.Clear();
			RefreshComboColumnOptions();
			string jobName = GetSelectedJobNameSafe();
			if (string.IsNullOrEmpty(jobName)) return;
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = config.Jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));
			if (job == null) return;
			foreach (TaskConfig task in job.Tasks.OrderBy(t => t.RunOrder)) AddTaskRowToGrid(task);
		}

		private void AddTaskRowToGrid(TaskConfig task)
		{
			if (task == null) return;

			string protocol = task.CommunicationProtocol;
			if (string.IsNullOrEmpty(protocol)) protocol = GetDefaultEnabledProtocol();
			EnsureProtocolValueExists(protocol);

			string triggerName = task.TriggerName;
			if (string.IsNullOrEmpty(triggerName)) triggerName = GetDefaultTriggerSource(protocol);

			string triggerValue = task.TriggerValue;
			if (string.IsNullOrEmpty(triggerValue)) triggerValue = "1";

			string imageSource = task.ImageSourceKey;
			if (string.IsNullOrEmpty(imageSource)) imageSource = "Not Use";
			imageSource = NormalizeImageSourceSelection(imageSource, GetAllCameraImageSourcesFromFiles());
			if (string.IsNullOrEmpty(imageSource)) imageSource = "Not Use";

			string positionName = task.PositionName;
			if (string.IsNullOrEmpty(positionName)) positionName = task.FlagBit.ToString();
			if (string.IsNullOrEmpty(positionName) || positionName == "0") positionName = GetDefaultPositionSource(protocol);

			string positionValue = task.PositionValue;
			if (string.IsNullOrEmpty(positionValue)) positionValue = task.FlagValue;
			if (string.IsNullOrEmpty(positionValue)) positionValue = "1";

			int rowIndex = dgvTrigger.Rows.Add(
				task.TaskName,
				protocol,
				triggerName,
				triggerValue,
				imageSource,
				positionName,
				positionValue,
				task.Remark);
			ShowAllTriggerRows();
			UpdateTriggerGridRowHeights();

			UpdateTriggerSourceCellOptions(rowIndex, protocol, triggerName);
			UpdatePositionSourceCellOptions(rowIndex, protocol, positionName);
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
				string protocol = GetCellString(dgvTrigger.Rows[e.RowIndex], COL_PROTOCOL);
				UpdateTriggerSourceCellOptions(e.RowIndex, protocol, string.Empty);
				UpdatePositionSourceCellOptions(e.RowIndex, protocol, string.Empty);
			}
		}




		private bool IsComboLikeColumn(int columnIndex)
		{
			return columnIndex == COL_PROTOCOL ||
				columnIndex == COL_TRIGGER_NAME ||
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
					UpdateTriggerSourceCellOptions(rowIndex, selectedValue, "");
					UpdatePositionSourceCellOptions(rowIndex, selectedValue, "");
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

			if (e.ColumnIndex == COL_IMAGE_SOURCE)
			{
				dgvTrigger.Rows[e.RowIndex].Cells[COL_IMAGE_SOURCE].ToolTipText =
					"Double click to select one or more image sources. Multiple sources are displayed in separate lines.";
				return;
			}

			if (!dgvTrigger.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
			{
				dgvTrigger.CurrentCell = dgvTrigger.Rows[e.RowIndex].Cells[e.ColumnIndex];
				dgvTrigger.BeginEdit(true);
			}
		}

		private void dgvTrigger_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex != COL_IMAGE_SOURCE)
			{
				return;
			}

			OpenImageSourceMultiSelectDialog(e.RowIndex);
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

		private void UpdateTriggerSourceCellOptions(int rowIndex, string protocol, string currentValue)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count) return;
			DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
			cell.FlatStyle = FlatStyle.Flat;
			List<string> triggers = LoadTriggerSourceOptions(protocol);
			foreach (string trigger in triggers) cell.Items.Add(trigger);
			if (string.IsNullOrEmpty(currentValue) || !triggers.Contains(currentValue)) currentValue = triggers.Count > 0 ? triggers[0] : string.Empty;
			cell.Value = currentValue;
			dgvTrigger.Rows[rowIndex].Cells[COL_TRIGGER_NAME] = cell;
		}

		private void UpdatePositionSourceCellOptions(int rowIndex, string protocol, string currentValue)
		{
			if (rowIndex < 0 || rowIndex >= dgvTrigger.Rows.Count) return;

			DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
			cell.FlatStyle = FlatStyle.Flat;

			List<string> positions = LoadPositionSourceOptions(protocol);
			foreach (string position in positions) cell.Items.Add(position);

			if (string.IsNullOrEmpty(currentValue) || !positions.Contains(currentValue))
			{
				currentValue = positions.Count > 0 ? positions[0] : string.Empty;
			}

			cell.Value = currentValue;
			dgvTrigger.Rows[rowIndex].Cells[COL_POSITION_NAME] = cell;
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

		private string GetDefaultEnabledProtocol()
		{
			List<string> protocols = LoadEnabledProtocolOptions();
			return protocols.Count > 0 ? protocols[0] : "Not Use";
		}

		private void EnsureProtocolValueExists(string protocol)
		{
			if (string.IsNullOrEmpty(protocol)) return;
			DataGridViewComboBoxColumn col = dgvTrigger.Columns[COL_PROTOCOL] as DataGridViewComboBoxColumn;
			if (col != null && !col.Items.Contains(protocol)) col.Items.Add(protocol);
		}

		private List<string> LoadTriggerSourceOptions(string protocol)
		{
			List<string> result = new List<string>();
			if (string.IsNullOrEmpty(protocol) || protocol == "Not Use") { result.Add("Not Use"); return result; }
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			if (protocol.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				result.Add("engine0"); result.Add("engine1"); result.Add("engine2"); result.Add("engine3");
			}
			else if (protocol.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				if (config.TcpIp != null && config.TcpIp.InputVariables != null)
					foreach (CommInputVariable item in config.TcpIp.InputVariables)
						if (item.UseAsTrigger) result.Add(item.Name);
			}
			else if (protocol.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				if (config.S7 != null && config.S7.InputVariables != null)
					foreach (CommInputVariable item in config.S7.InputVariables)
						if (item.UseAsTrigger) result.Add(item.Name);
			}
			if (result.Count == 0) result.Add("Not Use");
			return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private string GetDefaultTriggerSource(string protocol)
		{
			List<string> triggers = LoadTriggerSourceOptions(protocol);
			return triggers.Count > 0 ? triggers[0] : "Not Use";
		}

		private List<string> LoadPositionSourceOptions(string protocol)
		{
			List<string> result = new List<string>();

			if (string.IsNullOrEmpty(protocol) || protocol == "Not Use")
			{
				result.Add("Not Use");
				return result;
			}

			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();

			if (protocol.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				if (config.Profinet != null && config.Profinet.InputVariables != null)
				{
					foreach (CommInputVariable item in config.Profinet.InputVariables)
					{
						if (item.UseAsPosition) result.Add(item.Name);
					}
				}
			}
			else if (protocol.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				if (config.TcpIp != null && config.TcpIp.InputVariables != null)
				{
					foreach (CommInputVariable item in config.TcpIp.InputVariables)
					{
						if (item.UseAsPosition) result.Add(item.Name);
					}
				}
			}
			else if (protocol.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				if (config.S7 != null && config.S7.InputVariables != null)
				{
					foreach (CommInputVariable item in config.S7.InputVariables)
					{
						if (item.UseAsPosition) result.Add(item.Name);
					}
				}
			}

			if (result.Count == 0) result.Add("Not Use");
			return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private string GetDefaultPositionSource(string protocol)
		{
			List<string> positions = LoadPositionSourceOptions(protocol);
			return positions.Count > 0 ? positions[0] : "Not Use";
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

			string jobRoot = GetFlowJobRootFolder();
			string jobFolder = Path.Combine(jobRoot, newJobName);

			try
			{
				Directory.CreateDirectory(jobFolder);

				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
				FlowConfigStore.GetOrCreateJob(config, newJobName);
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

				if (config.Jobs != null)
				{
					config.Jobs.RemoveAll(j => j != null && string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));
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
			string protocol = GetDefaultEnabledProtocol();
			string trigger = GetDefaultTriggerSource(protocol);
			string position = GetDefaultPositionSource(protocol);
			int rowIndex = dgvTrigger.Rows.Add("Task_New_" + (dgvTrigger.Rows.Count + 1).ToString("00"), protocol, trigger, "1", "Not Use", position, "1", string.Empty);
			UpdateTriggerSourceCellOptions(rowIndex, protocol, trigger);
			UpdatePositionSourceCellOptions(rowIndex, protocol, position);
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
			ListBox jobList = GetJobListBox();

			if (jobList == null || jobList.SelectedItem == null)
			{
				return string.Empty;
			}

			return jobList.SelectedItem.ToString();
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
			string jobFolder = Path.Combine(ProjectPathStore.ProjectRoot, "Job");

			if (!Directory.Exists(jobFolder))
			{
				Directory.CreateDirectory(jobFolder);
			}

			return jobFolder;
		}


		private List<string> GetPossibleJobLocalPaths(string jobName)
		{
			List<string> paths = new List<string>();

			if (string.IsNullOrWhiteSpace(jobName))
			{
				return paths;
			}

			string normalizedJobName = NormalizeJobName(jobName);
			string jobRoot = GetFlowJobRootFolder();
			string jobFolder = Path.Combine(jobRoot, normalizedJobName);

			if (!paths.Any(x => string.Equals(x, jobFolder, StringComparison.OrdinalIgnoreCase)))
			{
				paths.Add(jobFolder);
			}

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
			RefreshImageSourceColumnItems();
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
			string jobName = GetSelectedJobNameSafe();
			if (string.IsNullOrEmpty(jobName)) { MessageBox.Show("Please select Job first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = FlowConfigStore.GetOrCreateJob(config, jobName);
			Dictionary<string, TaskConfig> oldTaskDict = job.Tasks.ToDictionary(t => t.TaskName, t => t, StringComparer.OrdinalIgnoreCase);
			job.Tasks.Clear();
			int runOrder = 1;
			foreach (DataGridViewRow row in dgvTrigger.Rows)
			{
				if (row.IsNewRow) continue;
				string taskName = GetCellString(row, COL_TASK_NAME);
				if (string.IsNullOrEmpty(taskName)) continue;
				TaskConfig task = oldTaskDict.ContainsKey(taskName) ? oldTaskDict[taskName] : FlowConfigStore.CreateDefaultTask(jobName, taskName, runOrder);
				task.TaskName = taskName;
				task.RunOrder = runOrder;
				task.Enabled = true;
				task.CommunicationProtocol = GetCellString(row, COL_PROTOCOL);
				task.TriggerName = GetCellString(row, COL_TRIGGER_NAME);
				task.TriggerValue = GetCellString(row, COL_TRIGGER_VALUE);
				if (string.IsNullOrEmpty(task.TriggerValue)) task.TriggerValue = "1";
				string imageSource = GetCellString(row, COL_IMAGE_SOURCE);
				imageSource = NormalizeImageSourceSelection(imageSource, GetAllCameraImageSourcesFromFiles());
				if (string.IsNullOrEmpty(imageSource)) imageSource = "Not Use";
				task.ImageSourceKey = imageSource;
				task.InputAddress = imageSource;
				task.PositionName = GetCellString(row, COL_POSITION_NAME);
				task.PositionValue = GetCellString(row, COL_POSITION_VALUE);
				if (string.IsNullOrEmpty(task.PositionName)) task.PositionName = "0";
				if (string.IsNullOrEmpty(task.PositionValue)) task.PositionValue = "1";

				// 旧字段同步保留，避免旧代码读取 FlagBit / FlagValue 时失效。
				int oldFlagBit;
				if (int.TryParse(task.PositionName, out oldFlagBit)) task.FlagBit = oldFlagBit;
				else task.FlagBit = 0;
				task.FlagValue = task.PositionValue;
				task.Remark = GetCellString(row, COL_REMARK);
				if (task.Steps == null) task.Steps = new List<StepConfig>();
				if (task.StepFlow == null) task.StepFlow = new List<StepFlowItem>();
				job.Tasks.Add(task);
				Directory.CreateDirectory(FlowConfigStore.PathManager.GetTaskFolder(jobName, taskName));
				runOrder++;
			}
			FlowConfigStore.Save(config);
			MoveLegacyTaskFoldersUnderTaskFolder(jobName);
			RefreshTriggerGridAfterSave();

			MessageBox.Show("Trigger configuration saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void MoveLegacyTaskFoldersUnderTaskFolder(string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = config.Jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job == null || job.Tasks == null)
			{
				return;
			}

			string jobFolder = FlowConfigStore.PathManager.GetJobFolder(jobName);

			foreach (TaskConfig task in job.Tasks)
			{
				if (task == null || string.IsNullOrWhiteSpace(task.TaskName))
				{
					continue;
				}

				string legacyFolder = Path.Combine(jobFolder, task.TaskName);
				string newFolder = FlowConfigStore.PathManager.GetTaskFolder(jobName, task.TaskName);

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

			_refreshComboPending = true;

			if (this.Visible)
			{
				RefreshByCommunicationConfigChanged();
				RefreshImageSourceColumnItems();
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
					string triggerName = GetCellString(row, COL_TRIGGER_NAME);
					string positionName = GetCellString(row, COL_POSITION_NAME);

					EnsureProtocolValueExists(protocol);
					UpdateTriggerSourceCellOptions(i, protocol, triggerName);
					UpdatePositionSourceCellOptions(i, protocol, positionName);
				}

				dgvTrigger.Invalidate();
				_refreshComboPending = false;
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


		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;
			if (dgvTrigger.Columns.Count <= COL_REMARK) return;

			if (isEnglish)
			{
				dgvTrigger.Columns[COL_TASK_NAME].HeaderText = "Task Name";
				dgvTrigger.Columns[COL_PROTOCOL].HeaderText = "Protocol";
				dgvTrigger.Columns[COL_TRIGGER_NAME].HeaderText = "Trigger Source";
				dgvTrigger.Columns[COL_TRIGGER_VALUE].HeaderText = "Trigger Value";
				dgvTrigger.Columns[COL_IMAGE_SOURCE].HeaderText = "Image Source";
				dgvTrigger.Columns[COL_POSITION_NAME].HeaderText = "Position No.";
				dgvTrigger.Columns[COL_POSITION_VALUE].HeaderText = "Position Value";
				dgvTrigger.Columns[COL_REMARK].HeaderText = "Remark";
				btnAddTask.Text = "+ Add Task";
				btnDeleteSelected.Text = "Delete";
				btnSave.Text = "Save";
			}
			else
			{
				dgvTrigger.Columns[COL_TASK_NAME].HeaderText = "task名称";
				dgvTrigger.Columns[COL_PROTOCOL].HeaderText = "通讯协议";
				dgvTrigger.Columns[COL_TRIGGER_NAME].HeaderText = "触发源名称";
				dgvTrigger.Columns[COL_TRIGGER_VALUE].HeaderText = "触发源值";
				dgvTrigger.Columns[COL_IMAGE_SOURCE].HeaderText = "图像源";
				dgvTrigger.Columns[COL_POSITION_NAME].HeaderText = "位置号";
				dgvTrigger.Columns[COL_POSITION_VALUE].HeaderText = "位置号值";
				dgvTrigger.Columns[COL_REMARK].HeaderText = "备注";
				btnAddTask.Text = "+ 新增 task";
				btnDeleteSelected.Text = "▦ 删除选中";
				btnSave.Text = "▣ 保存";
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
