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
		private const int COL_TASK_NAME = 0;
		private const int COL_PROTOCOL = 1;
		private const int COL_TRIGGER_NAME = 2;
		private const int COL_IMAGE_SOURCE = 3;
		private const int COL_FLAG_BIT = 4;
		private const int COL_FLAG_VALUE = 5;
		private const int COL_REMARK = 6;

		private bool _loading = false;
		private bool _isEnglish = false;

		public TriggerManagerControl()
		{
			InitializeComponent();

			ConfigureTriggerGrid();

			listJobs.SelectedIndexChanged -= listJobs_SelectedIndexChanged;
			listJobs.SelectedIndexChanged += listJobs_SelectedIndexChanged;

			dgvTrigger.CurrentCellDirtyStateChanged -= dgvTrigger_CurrentCellDirtyStateChanged;
			dgvTrigger.CurrentCellDirtyStateChanged += dgvTrigger_CurrentCellDirtyStateChanged;

			dgvTrigger.CellValueChanged -= dgvTrigger_CellValueChanged;
			dgvTrigger.CellValueChanged += dgvTrigger_CellValueChanged;

			dgvTrigger.DataError -= dgvTrigger_DataError;
			dgvTrigger.DataError += dgvTrigger_DataError;

			FlowConfigStore.FlowConfigSaved -= FlowConfigStore_FlowConfigSaved;
			FlowConfigStore.FlowConfigSaved += FlowConfigStore_FlowConfigSaved;

			LoadFlowConfigToJobList();
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

			DataGridViewTextBoxColumn colTask = new DataGridViewTextBoxColumn();
			colTask.Name = "colTaskName";
			colTask.HeaderText = "task名称";
			colTask.FillWeight = 120;
			dgvTrigger.Columns.Add(colTask);

			DataGridViewComboBoxColumn colProtocol = new DataGridViewComboBoxColumn();
			colProtocol.Name = "colProtocol";
			colProtocol.HeaderText = "通讯协议";
			colProtocol.FillWeight = 100;
			colProtocol.FlatStyle = FlatStyle.Flat;
			colProtocol.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			dgvTrigger.Columns.Add(colProtocol);

			DataGridViewComboBoxColumn colTrigger = new DataGridViewComboBoxColumn();
			colTrigger.Name = "colTriggerName";
			colTrigger.HeaderText = "触发源名称";
			colTrigger.FillWeight = 120;
			colTrigger.FlatStyle = FlatStyle.Flat;
			colTrigger.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			dgvTrigger.Columns.Add(colTrigger);

			DataGridViewComboBoxColumn colImage = new DataGridViewComboBoxColumn();
			colImage.Name = "colImageSource";
			colImage.HeaderText = "图像源";
			colImage.FillWeight = 120;
			colImage.FlatStyle = FlatStyle.Flat;
			colImage.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			dgvTrigger.Columns.Add(colImage);

			DataGridViewTextBoxColumn colFlagBit = new DataGridViewTextBoxColumn();
			colFlagBit.Name = "colFlagBit";
			colFlagBit.HeaderText = "标志位";
			colFlagBit.FillWeight = 80;
			dgvTrigger.Columns.Add(colFlagBit);

			DataGridViewTextBoxColumn colFlagValue = new DataGridViewTextBoxColumn();
			colFlagValue.Name = "colFlagValue";
			colFlagValue.HeaderText = "标志值";
			colFlagValue.FillWeight = 80;
			dgvTrigger.Columns.Add(colFlagValue);

			DataGridViewTextBoxColumn colRemark = new DataGridViewTextBoxColumn();
			colRemark.Name = "colRemark";
			colRemark.HeaderText = "备注";
			colRemark.FillWeight = 140;
			dgvTrigger.Columns.Add(colRemark);

			ApplyGridStyle();
			RefreshComboColumnOptions();
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
		}

		private void RefreshComboColumnOptions()
		{
			List<string> protocols = LoadEnabledProtocolOptions();
			DataGridViewComboBoxColumn protocolCol = dgvTrigger.Columns[COL_PROTOCOL] as DataGridViewComboBoxColumn;
			if (protocolCol != null)
			{
				protocolCol.Items.Clear();
				foreach (string protocol in protocols) protocolCol.Items.Add(protocol);
			}

			List<string> imageSources = LoadImageSourceOptions();
			DataGridViewComboBoxColumn imageCol = dgvTrigger.Columns[COL_IMAGE_SOURCE] as DataGridViewComboBoxColumn;
			if (imageCol != null)
			{
				imageCol.Items.Clear();
				foreach (string imageSource in imageSources) imageCol.Items.Add(imageSource);
			}
		}

		private void FlowConfigStore_FlowConfigSaved(object sender, EventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new EventHandler(FlowConfigStore_FlowConfigSaved), sender, e);
				return;
			}
			string oldJob = GetSelectedJobName();
			LoadFlowConfigToJobList();
			SelectListItem(listJobs, oldJob);
			LoadCurrentJobTasksToGrid();
		}

		private void LoadFlowConfigToJobList()
		{
			_loading = true;
			try
			{
				listJobs.Items.Clear();
				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
				foreach (JobConfig job in config.Jobs) listJobs.Items.Add(job.JobName);
				if (listJobs.Items.Count > 0) listJobs.SelectedIndex = 0;
				LoadCurrentJobTasksToGrid();
			}
			finally { _loading = false; }
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
			string jobName = GetSelectedJobName();
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
			string imageSource = task.ImageSourceKey;
			if (string.IsNullOrEmpty(imageSource)) imageSource = "Not Use";
			EnsureImageSourceValueExists(imageSource);
			int rowIndex = dgvTrigger.Rows.Add(task.TaskName, protocol, triggerName, imageSource, task.FlagBit, task.FlagValue, task.Remark);
			UpdateTriggerSourceCellOptions(rowIndex, protocol, triggerName);
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
			}
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

		private List<string> LoadImageSourceOptions()
		{
			List<string> result = new List<string>();
			result.Add("Not Use");
			try
			{
				string hardwareConfigRoot = FlowConfigStore.PathManager.HardwareConfigRoot;
				if (Directory.Exists(hardwareConfigRoot))
				{
					string[] xmlFiles = Directory.GetFiles(hardwareConfigRoot, "*.xml", SearchOption.AllDirectories);
					foreach (string xmlFile in xmlFiles) AddImageSourcesFromHardwareXml(xmlFile, result);
				}
			}
			catch { }
			if (result.Count <= 1) { result.Add("Cam1.Raw"); result.Add("Cam2.Raw"); }
			return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private void AddImageSourcesFromHardwareXml(string xmlFile, List<string> result)
		{
			if (string.IsNullOrEmpty(xmlFile) || !File.Exists(xmlFile)) return;
			try
			{
				XmlDocument doc = new XmlDocument(); doc.Load(xmlFile);
				XmlNodeList nodes = doc.SelectNodes("//*");
				if (nodes == null) return;
				foreach (XmlNode node in nodes) AddImageSourceByNode(node, result);
			}
			catch { }
		}

		private void AddImageSourceByNode(XmlNode node, List<string> result)
		{
			if (node == null) return;
			if (node.Attributes != null)
			{
				AddImageSourceByAttribute(node, "ImageSourceKey", result);
				AddImageSourceByAttribute(node, "ImageKey", result);
				AddImageSourceByAttribute(node, "Key", result);
				AddImageSourceByAttribute(node, "Name", result);
				AddImageSourceByAttribute(node, "CameraName", result);
				AddImageSourceByAttribute(node, "CameraKey", result);
				AddImageSourceByAttribute(node, "ChannelName", result);
				AddImageSourceByAttribute(node, "DeviceName", result);
			}
			string nodeName = node.Name.ToLower();
			if (nodeName.Contains("camera") || nodeName.Contains("imagesource") || nodeName.Contains("imagekey") || nodeName.Contains("channel"))
			{
				string text = node.InnerText == null ? string.Empty : node.InnerText.Trim();
				if (!string.IsNullOrEmpty(text) && text.Length <= 64 && !text.Contains("<")) AddImageSourceValue(text, result);
			}
		}

		private void AddImageSourceByAttribute(XmlNode node, string attributeName, List<string> result)
		{
			XmlAttribute attr = node.Attributes[attributeName];
			if (attr != null) AddImageSourceValue(attr.Value, result);
		}

		private void AddImageSourceValue(string value, List<string> result)
		{
			if (string.IsNullOrWhiteSpace(value)) return;
			string source = value.Trim();
			if (!source.Contains(".") && !source.Equals("Not Use", StringComparison.OrdinalIgnoreCase) && !source.Equals("无", StringComparison.OrdinalIgnoreCase) && !source.Equals("None", StringComparison.OrdinalIgnoreCase)) source = source + ".Raw";
			if (!result.Any(x => string.Equals(x, source, StringComparison.OrdinalIgnoreCase))) result.Add(source);
		}

		private void EnsureImageSourceValueExists(string imageSource)
		{
			if (string.IsNullOrEmpty(imageSource)) imageSource = "Not Use";
			DataGridViewComboBoxColumn col = dgvTrigger.Columns[COL_IMAGE_SOURCE] as DataGridViewComboBoxColumn;
			if (col != null && !col.Items.Contains(imageSource)) col.Items.Add(imageSource);
		}

		private void btnAddTask_Click(object sender, EventArgs e)
		{
			string protocol = GetDefaultEnabledProtocol();
			string trigger = GetDefaultTriggerSource(protocol);
			int rowIndex = dgvTrigger.Rows.Add("Task_New_" + (dgvTrigger.Rows.Count + 1).ToString("00"), protocol, trigger, "Not Use", 0, "1", string.Empty);
			UpdateTriggerSourceCellOptions(rowIndex, protocol, trigger);
		}

		private void btnDeleteSelected_Click(object sender, EventArgs e)
		{
			if (dgvTrigger.SelectedRows.Count <= 0) return;
			foreach (DataGridViewRow row in dgvTrigger.SelectedRows)
				if (!row.IsNewRow) dgvTrigger.Rows.Remove(row);
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
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
				string imageSource = GetCellString(row, COL_IMAGE_SOURCE);
				if (string.IsNullOrEmpty(imageSource)) imageSource = "Not Use";
				task.ImageSourceKey = imageSource;
				task.InputAddress = imageSource;
				task.FlagBit = GetCellInt(row, COL_FLAG_BIT, 0);
				task.FlagValue = GetCellString(row, COL_FLAG_VALUE);
				task.Remark = GetCellString(row, COL_REMARK);
				if (task.Steps == null) task.Steps = new List<StepConfig>();
				if (task.StepFlow == null) task.StepFlow = new List<StepFlowItem>();
				job.Tasks.Add(task);
				runOrder++;
			}
			FlowConfigStore.Save(config);
			MessageBox.Show("Trigger configuration saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private string GetSelectedJobName()
		{
			return listJobs.SelectedItem == null ? string.Empty : listJobs.SelectedItem.ToString();
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

		private int GetCellInt(DataGridViewRow row, int columnIndex, int defaultValue)
		{
			int value;
			if (int.TryParse(GetCellString(row, columnIndex), out value)) return value;
			return defaultValue;
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
				dgvTrigger.Columns[COL_IMAGE_SOURCE].HeaderText = "Image Source";
				dgvTrigger.Columns[COL_FLAG_BIT].HeaderText = "Flag Bit";
				dgvTrigger.Columns[COL_FLAG_VALUE].HeaderText = "Flag Value";
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
				dgvTrigger.Columns[COL_IMAGE_SOURCE].HeaderText = "图像源";
				dgvTrigger.Columns[COL_FLAG_BIT].HeaderText = "标志位";
				dgvTrigger.Columns[COL_FLAG_VALUE].HeaderText = "标志值";
				dgvTrigger.Columns[COL_REMARK].HeaderText = "备注";
				btnAddTask.Text = "+ 新增 task";
				btnDeleteSelected.Text = "▦ 删除选中";
				btnSave.Text = "▣ 保存";
			}
		}
	}
}
