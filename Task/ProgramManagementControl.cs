using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Aron_V3
{
	public sealed class ProgramManagementControl : UserControl, ILocalizable
	{
		private readonly Color _back = Color.FromArgb(2, 10, 20);
		private readonly Color _panel = Color.FromArgb(5, 18, 34);
		private readonly Color _border = Color.FromArgb(38, 62, 86);
		private readonly Color _accent = Color.FromArgb(0, 150, 220);
		private readonly Color _selected = Color.FromArgb(0, 95, 170);
		private readonly Color _text = Color.FromArgb(220, 235, 245);
		private readonly Color _muted = Color.FromArgb(130, 155, 175);

		private bool _isEnglish;
		private Label _titleLabel;
		private Label _hintLabel;
		private DataGridView _grid;
		private Button _btnClone;
		private Button _btnDelete;
		private Button _btnRefresh;

		public ProgramManagementControl()
		{
			Dock = DockStyle.Fill;
			BackColor = _back;
			BuildUi();
			FlowConfigStore.FlowConfigSaved -= FlowConfigStore_FlowConfigSaved;
			FlowConfigStore.FlowConfigSaved += FlowConfigStore_FlowConfigSaved;
			RefreshPrograms();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			FlowConfigStore.FlowConfigSaved -= FlowConfigStore_FlowConfigSaved;
			base.OnHandleDestroyed(e);
		}

		private void FlowConfigStore_FlowConfigSaved(object sender, EventArgs e)
		{
			if (IsDisposed)
			{
				return;
			}

			if (InvokeRequired)
			{
				try
				{
					BeginInvoke(new EventHandler(FlowConfigStore_FlowConfigSaved), sender, e);
				}
				catch
				{
				}

				return;
			}

			RefreshPrograms();
		}

		public void ApplyLanguage(bool isEnglish)
		{
			_isEnglish = isEnglish;
			if (_titleLabel != null) _titleLabel.Text = T("\u7a0b\u5e8f\u53f7\u7ba1\u7406", "Program Management");
			if (_hintLabel != null)
			{
				_hintLabel.Text = T(
					"\u65b0\u589e\u7a0b\u5e8f\u53f7\u4f1a\u514b\u9686\u6e90\u7a0b\u5e8f\u4e2d\u5df2\u542f\u7528\u201c\u5207\u6362\u7a0b\u5e8f\u201d\u7684 Task\uff0c\u5e76\u590d\u5236\u5bf9\u5e94 Step \u6587\u4ef6\u3002",
					"Adding a program clones switch-enabled tasks from the source program and copies their step files.");
			}
			if (_btnClone != null) _btnClone.Text = T("+ \u514b\u9686\u65b0\u589e", "+ Clone Add");
			if (_btnDelete != null) _btnDelete.Text = T("\u5220\u9664\u7a0b\u5e8f", "Delete Program");
			if (_btnRefresh != null) _btnRefresh.Text = T("\u5237\u65b0", "Refresh");

			ApplyGridHeaders();
		}

		private void BuildUi()
		{
			Controls.Clear();

			TableLayoutPanel root = new TableLayoutPanel();
			root.Dock = DockStyle.Fill;
			root.BackColor = _back;
			root.RowCount = 3;
			root.ColumnCount = 1;
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
			root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
			root.Padding = new Padding(16);

			Panel header = new Panel();
			header.Dock = DockStyle.Fill;
			header.BackColor = _panel;
			header.Padding = new Padding(14, 8, 14, 8);

			_titleLabel = new Label();
			_titleLabel.Dock = DockStyle.Top;
			_titleLabel.Height = 28;
			_titleLabel.TextAlign = ContentAlignment.MiddleLeft;
			_titleLabel.ForeColor = _text;
			_titleLabel.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);

			_hintLabel = new Label();
			_hintLabel.Dock = DockStyle.Fill;
			_hintLabel.TextAlign = ContentAlignment.MiddleLeft;
			_hintLabel.ForeColor = _muted;
			_hintLabel.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			_hintLabel.AutoEllipsis = true;

			header.Controls.Add(_hintLabel);
			header.Controls.Add(_titleLabel);

			_grid = new DataGridView();
			_grid.Dock = DockStyle.Fill;
			_grid.AllowUserToAddRows = false;
			_grid.AllowUserToDeleteRows = false;
			_grid.AllowUserToResizeRows = false;
			_grid.RowHeadersVisible = false;
			_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			_grid.MultiSelect = false;
			_grid.ReadOnly = true;
			_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			_grid.BackgroundColor = _back;
			_grid.BorderStyle = BorderStyle.None;
			_grid.GridColor = _border;
			_grid.EnableHeadersVisualStyles = false;
			_grid.ColumnHeadersHeight = 30;
			_grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			_grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			_grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			_grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			_grid.DefaultCellStyle.BackColor = _back;
			_grid.DefaultCellStyle.ForeColor = Color.White;
			_grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			_grid.DefaultCellStyle.SelectionForeColor = Color.White;
			_grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			_grid.RowTemplate.Height = 30;

			_grid.Columns.Add(CreateTextColumn("ProgramName", 120));
			_grid.Columns.Add(CreateTextColumn("ProgramNo", 90));
			_grid.Columns.Add(CreateTextColumn("Channel", 160));
			_grid.Columns.Add(CreateTextColumn("SwitchTasks", 90));
			_grid.Columns.Add(CreateTextColumn("TotalTasks", 90));

			Panel footer = new Panel();
			footer.Dock = DockStyle.Fill;
			footer.BackColor = _back;

			_btnClone = CreateButton("+ \u514b\u9686\u65b0\u589e", true);
			_btnDelete = CreateButton("\u5220\u9664\u7a0b\u5e8f", false);
			_btnRefresh = CreateButton("\u5237\u65b0", false);

			_btnClone.SetBounds(0, 10, 128, 34);
			_btnDelete.SetBounds(140, 10, 118, 34);
			_btnRefresh.SetBounds(270, 10, 96, 34);

			_btnClone.Click += btnClone_Click;
			_btnDelete.Click += btnDelete_Click;
			_btnRefresh.Click += delegate { RefreshPrograms(); };

			footer.Controls.Add(_btnClone);
			footer.Controls.Add(_btnDelete);
			footer.Controls.Add(_btnRefresh);

			root.Controls.Add(header, 0, 0);
			root.Controls.Add(_grid, 0, 1);
			root.Controls.Add(footer, 0, 2);
			Controls.Add(root);

			ApplyLanguage(_isEnglish);
		}

		private DataGridViewTextBoxColumn CreateTextColumn(string name, float weight)
		{
			DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
			column.Name = name;
			column.FillWeight = weight;
			column.ReadOnly = true;
			return column;
		}

		private Button CreateButton(string text, bool primary)
		{
			Button button = new Button();
			button.Text = text;
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = primary ? _accent : _border;
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(8, 35, 60);
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(5, 25, 45);
			button.BackColor = primary ? _selected : _panel;
			button.ForeColor = Color.White;
			button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			button.Cursor = Cursors.Hand;
			button.UseVisualStyleBackColor = false;
			return button;
		}

		private void ApplyGridHeaders()
		{
			if (_grid == null || _grid.Columns.Count < 5)
			{
				return;
			}

			_grid.Columns["ProgramName"].HeaderText = T("\u7a0b\u5e8f\u53f7", "Program");
			_grid.Columns["ProgramNo"].HeaderText = T("\u7a0b\u5e8f\u503c", "Program No");
			_grid.Columns["Channel"].HeaderText = T("\u901a\u9053", "Channel");
			_grid.Columns["SwitchTasks"].HeaderText = T("\u5207\u6362Task", "Switch Tasks");
			_grid.Columns["TotalTasks"].HeaderText = T("Task\u603b\u6570", "Total Tasks");
		}

		private void RefreshPrograms()
		{
			if (_grid == null)
			{
				return;
			}

			_grid.Rows.Clear();
			List<ProgramSummary> summaries = ProgramManagementService.GetProgramSummaries();
			foreach (ProgramSummary summary in summaries)
			{
				int rowIndex = _grid.Rows.Add(
					summary.ProgramName,
					summary.ProgramNosText,
					summary.ChannelsText,
					summary.SwitchTaskCount.ToString(),
					summary.TotalTaskCount.ToString());
				_grid.Rows[rowIndex].Tag = summary;
			}
		}

		private void btnClone_Click(object sender, EventArgs e)
		{
			List<ProgramSummary> summaries = ProgramManagementService.GetProgramSummaries();
			if (summaries.Count <= 0)
			{
				MessageBox.Show(
					this,
					T("\u5f53\u524d\u6ca1\u6709\u53ef\u514b\u9686\u7684\u7a0b\u5e8f\u53f7\u3002\u8bf7\u5148\u5728\u4efb\u52a1\u7ba1\u7406\u4e2d\u521b\u5efa\u521d\u59cb Task\u3002", "No source program is available. Create an initial task first."),
					T("\u7a0b\u5e8f\u53f7\u7ba1\u7406", "Program Management"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			using (ProgramCloneDialog dialog = new ProgramCloneDialog(
				summaries.Select(x => x.ProgramName).ToList(),
				ProgramManagementService.GetNextProgramName(),
				_isEnglish))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				try
				{
					ProgramCloneResult result = ProgramManagementService.CloneProgram(dialog.SourceProgramName, dialog.NewProgramName);
					RefreshPrograms();
					MessageBox.Show(
						this,
						T(
							"\u7a0b\u5e8f\u53f7\u5df2\u65b0\u589e\uff0c\u514b\u9686 Task \u6570\uff1a" + result.ClonedTaskCount.ToString(),
							"Program added. Cloned tasks: " + result.ClonedTaskCount.ToString()),
						T("\u7a0b\u5e8f\u53f7\u7ba1\u7406", "Program Management"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						this,
						T("\u65b0\u589e\u7a0b\u5e8f\u53f7\u5931\u8d25\uff1a", "Add program failed: ") + ex.Message,
						T("\u7a0b\u5e8f\u53f7\u7ba1\u7406", "Program Management"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
				}
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			ProgramSummary selected = GetSelectedSummary();
			if (selected == null)
			{
				MessageBox.Show(
					this,
					T("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u7a0b\u5e8f\u53f7\u3002", "Select a program first."),
					T("\u5220\u9664\u7a0b\u5e8f", "Delete Program"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			bool confirm = ThemedDialog.Confirm(
				this,
				T("\u5220\u9664\u7a0b\u5e8f", "Delete Program"),
				T("\u5220\u9664\u7a0b\u5e8f\u53f7 \"" + selected.ProgramName + "\" \u5417\uff1f", "Delete program \"" + selected.ProgramName + "\"?"),
				T("\u8be5\u7a0b\u5e8f\u4e0b\u7684 Task \u7248\u672c\u3001Step \u6587\u4ef6\u548c\u7a0b\u5e8f\u786c\u4ef6\u914d\u7f6e\u4f1a\u88ab\u5220\u9664\u3002", "The task variants, step files, and program hardware config will be deleted."),
				T("\u6b64\u64cd\u4f5c\u4e0d\u5f71\u54cd\u5176\u5b83\u7a0b\u5e8f\u53f7\u3002", "Other programs are not affected."),
				T("\u5220\u9664", "Delete"),
				T("\u53d6\u6d88", "Cancel"),
				ThemedDialogIconKind.Delete,
				true);

			if (!confirm)
			{
				return;
			}

			try
			{
				ProgramManagementService.DeleteProgram(selected.ProgramName);
				RefreshPrograms();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					this,
					T("\u5220\u9664\u7a0b\u5e8f\u53f7\u5931\u8d25\uff1a", "Delete program failed: ") + ex.Message,
					T("\u5220\u9664\u7a0b\u5e8f", "Delete Program"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}

		private ProgramSummary GetSelectedSummary()
		{
			if (_grid == null || _grid.SelectedRows.Count <= 0)
			{
				return null;
			}

			return _grid.SelectedRows[0].Tag as ProgramSummary;
		}

		private string T(string cn, string en)
		{
			return _isEnglish ? en : cn;
		}
	}

	internal sealed class ProgramCloneDialog : Form
	{
		private readonly bool _isEnglish;
		private readonly ComboBox _cmbSource;
		private readonly TextBox _txtNewProgram;
		private readonly Button _btnOk;
		private readonly Button _btnCancel;

		public string SourceProgramName { get; private set; }
		public string NewProgramName { get; private set; }

		public ProgramCloneDialog(List<string> sourcePrograms, string defaultProgramName, bool isEnglish)
		{
			_isEnglish = isEnglish;
			SourceProgramName = string.Empty;
			NewProgramName = string.Empty;

			AutoScaleMode = AutoScaleMode.None;
			StartPosition = FormStartPosition.CenterParent;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			ClientSize = new Size(520, 245);
			BackColor = Color.FromArgb(2, 10, 20);
			ForeColor = Color.White;
			Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			Text = T("\u514b\u9686\u65b0\u589e\u7a0b\u5e8f\u53f7", "Clone Program");

			Label title = CreateLabel(T("\u514b\u9686\u65b0\u589e\u7a0b\u5e8f\u53f7", "Clone Program"), 22, 18, 420, 30, 13F, FontStyle.Bold, Color.White);
			Label lblSource = CreateLabel(T("\u514b\u9686\u6765\u6e90", "Clone From"), 34, 70, 120, 26, 9F, FontStyle.Bold, Color.FromArgb(220, 235, 245));
			Label lblNew = CreateLabel(T("\u65b0\u7a0b\u5e8f\u53f7", "New Program"), 34, 116, 120, 26, 9F, FontStyle.Bold, Color.FromArgb(220, 235, 245));

			_cmbSource = new ComboBox();
			_cmbSource.DropDownStyle = ComboBoxStyle.DropDownList;
			_cmbSource.SetBounds(156, 70, 300, 28);
			_cmbSource.BackColor = Color.FromArgb(5, 18, 34);
			_cmbSource.ForeColor = Color.White;
			_cmbSource.FlatStyle = FlatStyle.Flat;
			if (sourcePrograms != null)
			{
				foreach (string program in sourcePrograms.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
				{
					_cmbSource.Items.Add(program);
				}
			}
			if (_cmbSource.Items.Count > 0) _cmbSource.SelectedIndex = 0;

			_txtNewProgram = new TextBox();
			_txtNewProgram.SetBounds(156, 116, 300, 28);
			_txtNewProgram.BackColor = Color.FromArgb(5, 18, 34);
			_txtNewProgram.ForeColor = Color.White;
			_txtNewProgram.BorderStyle = BorderStyle.FixedSingle;
			_txtNewProgram.Text = defaultProgramName ?? string.Empty;

			_btnOk = CreateDialogButton(T("\u786e\u5b9a", "OK"), true);
			_btnOk.SetBounds(250, 182, 96, 34);
			_btnOk.Click += btnOk_Click;

			_btnCancel = CreateDialogButton(T("\u53d6\u6d88", "Cancel"), false);
			_btnCancel.SetBounds(360, 182, 96, 34);
			_btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

			Controls.Add(title);
			Controls.Add(lblSource);
			Controls.Add(lblNew);
			Controls.Add(_cmbSource);
			Controls.Add(_txtNewProgram);
			Controls.Add(_btnOk);
			Controls.Add(_btnCancel);

			AcceptButton = _btnOk;
			CancelButton = _btnCancel;
		}

		private Label CreateLabel(string text, int x, int y, int width, int height, float size, FontStyle style, Color color)
		{
			Label label = new Label();
			label.Text = text;
			label.SetBounds(x, y, width, height);
			label.Font = new Font("Microsoft YaHei UI", size, style);
			label.ForeColor = color;
			label.BackColor = Color.Transparent;
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.AutoEllipsis = true;
			return label;
		}

		private Button CreateDialogButton(string text, bool primary)
		{
			Button button = new Button();
			button.Text = text;
			button.FlatStyle = FlatStyle.Flat;
			button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
			button.ForeColor = Color.White;
			button.BackColor = primary ? Color.FromArgb(0, 95, 170) : Color.FromArgb(2, 10, 20);
			button.FlatAppearance.BorderColor = primary ? Color.FromArgb(0, 150, 220) : Color.FromArgb(38, 62, 86);
			button.FlatAppearance.BorderSize = 1;
			return button;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			SourceProgramName = _cmbSource.SelectedItem == null ? string.Empty : _cmbSource.SelectedItem.ToString().Trim();
			NewProgramName = _txtNewProgram.Text == null ? string.Empty : _txtNewProgram.Text.Trim();

			if (string.IsNullOrWhiteSpace(SourceProgramName))
			{
				MessageBox.Show(this, T("\u8bf7\u9009\u62e9\u514b\u9686\u6765\u6e90\u3002", "Select a source program."), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(NewProgramName))
			{
				MessageBox.Show(this, T("\u8bf7\u8f93\u5165\u65b0\u7a0b\u5e8f\u53f7\u3002", "Enter the new program name."), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private string T(string cn, string en)
		{
			return _isEnglish ? en : cn;
		}
	}

	internal sealed class ProgramSummary
	{
		public string ProgramName { get; set; }
		public string ProgramNosText { get; set; }
		public string ChannelsText { get; set; }
		public int SwitchTaskCount { get; set; }
		public int TotalTaskCount { get; set; }
	}

	internal sealed class ProgramCloneResult
	{
		public int ClonedTaskCount { get; set; }
	}

	internal sealed class ProgramTaskSyncPlan
	{
		public int MissingTaskCount { get; set; }
		public List<string> TaskNames { get; private set; }
		public List<string> TargetProgramNames { get; private set; }

		public ProgramTaskSyncPlan()
		{
			TaskNames = new List<string>();
			TargetProgramNames = new List<string>();
		}
	}

	internal sealed class ProgramTaskSyncResult
	{
		public int ClonedTaskCount { get; set; }
		public List<string> TaskNames { get; private set; }
		public List<string> TargetProgramNames { get; private set; }

		public ProgramTaskSyncResult()
		{
			TaskNames = new List<string>();
			TargetProgramNames = new List<string>();
		}
	}

	internal static class ProgramManagementService
	{
		private sealed class SwitchTaskSource
		{
			public JobConfig Job;
			public TaskConfig Task;
		}

		private sealed class SwitchTaskSyncRequest
		{
			public JobConfig SourceJob;
			public TaskConfig SourceTask;
			public JobConfig TargetJob;
			public string TaskName;
		}

		public static List<ProgramSummary> GetProgramSummaries()
		{
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			CommunicationConfig communicationConfig = LoadCommunicationConfig();
			return EnumerateJobs(config)
				.Where(x => x != null &&
					!string.IsNullOrWhiteSpace(x.JobName) &&
					x.Tasks != null &&
					x.Tasks.Any(t => t != null))
				.GroupBy(x => x.JobName, StringComparer.OrdinalIgnoreCase)
				.Select(g => new ProgramSummary
				{
					ProgramName = g.Key,
					ProgramNosText = string.Join(", ", g.Select(x => string.IsNullOrWhiteSpace(x.ProgramNo) ? "1" : x.ProgramNo).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()),
					ChannelsText = string.Join(", ", g.SelectMany(x => GetCommunicationDisplayNames(x, communicationConfig)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()),
					SwitchTaskCount = g.SelectMany(x => x.Tasks ?? new List<TaskConfig>()).Count(t => t != null && t.ProgramSwitchEnabled),
					TotalTaskCount = g.SelectMany(x => x.Tasks ?? new List<TaskConfig>()).Count(t => t != null)
				})
				.OrderBy(x => x.ProgramName)
				.ToList();
		}

		private static CommunicationConfig LoadCommunicationConfig()
		{
			try
			{
				return CommunicationConfigStore.LoadOrCreateDefault();
			}
			catch
			{
				return null;
			}
		}

		private static IEnumerable<string> GetCommunicationDisplayNames(JobConfig job, CommunicationConfig communicationConfig)
		{
			List<string> result = new List<string>();
			if (job == null)
			{
				return result;
			}

			if (job.Tasks != null)
			{
				foreach (TaskConfig task in job.Tasks)
				{
					if (task == null)
					{
						continue;
					}

					AddCommunicationDisplayName(
						result,
						task.CommunicationProtocol,
						task.CommunicationInstanceName,
						task.CommunicationChannel,
						communicationConfig);

					if (task.CommunicationTriggerBindings == null)
					{
						continue;
					}

					foreach (TaskCommunicationTriggerBinding binding in task.CommunicationTriggerBindings)
					{
						if (binding == null)
						{
							continue;
						}

						AddCommunicationDisplayName(
							result,
							binding.CommunicationProtocol,
							binding.CommunicationInstanceName,
							binding.CommunicationChannel,
							communicationConfig);
					}
				}
			}

			if (result.Count <= 0)
			{
				AddCommunicationDisplayName(
					result,
					job.ProtocolName,
					string.Empty,
					job.ChannelName,
					communicationConfig);
			}

			return result;
		}

		private static void AddCommunicationDisplayName(
			List<string> result,
			string protocolName,
			string instanceName,
			string channelName,
			CommunicationConfig communicationConfig)
		{
			if (result == null)
			{
				return;
			}

			string protocol = FlowConfigStore.NormalizeProtocolName(protocolName);
			if (string.IsNullOrWhiteSpace(protocol) || protocol.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			string instance = CommunicationRuntimeNaming.NormalizeInstanceName(protocol, instanceName, communicationConfig);
			string displayName = string.IsNullOrWhiteSpace(instance) ? protocol : instance;
			string channel = FlowConfigStore.NormalizeChannelName(channelName);
			string text = displayName + "/" + channel;
			if (!result.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase)))
			{
				result.Add(text);
			}
		}

		public static string GetNextProgramName()
		{
			List<string> names = GetProgramSummaries()
				.Select(x => x.ProgramName)
				.ToList();
			int index = 1;

			while (true)
			{
				string candidate = "Job_" + index.ToString("000");
				if (!names.Any(x => string.Equals(x, candidate, StringComparison.OrdinalIgnoreCase)))
				{
					return candidate;
				}

				index++;
			}
		}

		public static ProgramCloneResult CloneProgram(string sourceProgramName, string newProgramName)
		{
			sourceProgramName = NormalizeProgramName(sourceProgramName);
			newProgramName = NormalizeProgramName(newProgramName);

			if (string.IsNullOrWhiteSpace(sourceProgramName))
			{
				throw new InvalidOperationException("Source program is empty.");
			}

			if (string.IsNullOrWhiteSpace(newProgramName))
			{
				throw new InvalidOperationException("New program is empty.");
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			if (ProgramExists(config, newProgramName))
			{
				throw new InvalidOperationException("Program already exists: " + newProgramName);
			}

			List<JobConfig> sourceJobs = EnumerateJobs(config)
				.Where(x => x != null && string.Equals(x.JobName, sourceProgramName, StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (sourceJobs.Count <= 0)
			{
				throw new InvalidOperationException("Source program was not found: " + sourceProgramName);
			}

			int clonedTaskCount = 0;
			foreach (JobConfig sourceJob in sourceJobs)
			{
				List<TaskConfig> switchTasks = sourceJob.Tasks == null
					? new List<TaskConfig>()
					: sourceJob.Tasks
						.Where(x => x != null && x.ProgramSwitchEnabled)
						.OrderBy(x => x.RunOrder)
						.ToList();

				if (switchTasks.Count <= 0)
				{
					continue;
				}

				string protocolName = FlowConfigStore.NormalizeProtocolName(sourceJob.ProtocolName);
				string channelName = FlowConfigStore.NormalizeChannelName(sourceJob.ChannelName);
				string programNo = GetNextProgramNo(config, protocolName, channelName);
				JobConfig targetJob = FlowConfigStore.GetOrCreateJob(config, protocolName, channelName, newProgramName);
				targetJob.Enabled = sourceJob.Enabled;
				targetJob.ProgramNo = programNo;

				foreach (TaskConfig sourceTask in switchTasks)
				{
					if (CloneTaskToTargetJob(sourceJob, targetJob, sourceTask) != null)
					{
						clonedTaskCount++;
					}
				}

				CopyProgramHardware(sourceProgramName, newProgramName);
			}

			if (clonedTaskCount <= 0)
			{
				throw new InvalidOperationException("The source program has no switch-enabled task.");
			}

			FlowConfigStore.Save(config);
			return new ProgramCloneResult
			{
				ClonedTaskCount = clonedTaskCount
			};
		}

		public static ProgramTaskSyncPlan GetSwitchTaskSyncPlan(IEnumerable<string> taskNames)
		{
			return GetSwitchTaskSyncPlan(taskNames, string.Empty);
		}

		public static ProgramTaskSyncPlan GetSwitchTaskSyncPlan(IEnumerable<string> taskNames, string sourceProgramName)
		{
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			List<SwitchTaskSyncRequest> requests = BuildSwitchTaskSyncRequests(config, taskNames, sourceProgramName);
			return BuildSwitchTaskSyncPlan(requests);
		}

		public static ProgramTaskSyncResult SyncSwitchTasksToExistingPrograms(IEnumerable<string> taskNames)
		{
			return SyncSwitchTasksToExistingPrograms(taskNames, string.Empty);
		}

		public static ProgramTaskSyncResult SyncSwitchTasksToExistingPrograms(IEnumerable<string> taskNames, string sourceProgramName)
		{
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			List<SwitchTaskSyncRequest> requests = BuildSwitchTaskSyncRequests(config, taskNames, sourceProgramName);
			ProgramTaskSyncResult result = new ProgramTaskSyncResult();

			foreach (SwitchTaskSyncRequest request in requests)
			{
				if (request == null ||
					request.SourceJob == null ||
					request.SourceTask == null ||
					request.TargetJob == null ||
					string.IsNullOrWhiteSpace(request.TaskName))
				{
					continue;
				}

				TaskConfig targetTask = FindTask(request.TargetJob, request.TaskName);
				if (targetTask != null && !IsTaskEmpty(targetTask))
				{
					continue;
				}

				TaskConfig clonedTask = CloneTaskToTargetJob(request.SourceJob, request.TargetJob, request.SourceTask);
				if (clonedTask == null)
				{
					continue;
				}

				result.ClonedTaskCount++;
				AddUnique(result.TaskNames, clonedTask.TaskName);
				AddUnique(result.TargetProgramNames, request.TargetJob.JobName);
			}

			if (result.ClonedTaskCount > 0)
			{
				FlowConfigStore.Save(config);
			}

			return result;
		}

		public static void DeleteProgram(string programName)
		{
			programName = NormalizeProgramName(programName);
			if (string.IsNullOrWhiteSpace(programName))
			{
				return;
			}

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			List<JobConfig> jobs = EnumerateJobs(config)
				.Where(x => x != null && string.Equals(x.JobName, programName, StringComparison.OrdinalIgnoreCase))
				.ToList();

			foreach (JobConfig job in jobs)
			{
				if (job.Tasks != null)
				{
					foreach (TaskConfig task in job.Tasks)
					{
						DeleteTaskFiles(job, task);
					}
				}
			}

			if (config.Protocols != null)
			{
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

						channel.Jobs.RemoveAll(x => x != null && string.Equals(x.JobName, programName, StringComparison.OrdinalIgnoreCase));
					}
				}
			}

			DeleteProgramHardware(programName);
			CleanupOrphanTaskFolders(config);
			FlowConfigStore.Save(config);
		}

		private static TaskConfig CloneTaskToTargetJob(JobConfig sourceJob, JobConfig targetJob, TaskConfig sourceTask)
		{
			if (sourceJob == null || targetJob == null || sourceTask == null)
			{
				return null;
			}

			if (targetJob.Tasks == null)
			{
				targetJob.Tasks = new List<TaskConfig>();
			}

			TaskConfig clonedTask = CloneByXml(sourceTask);
			clonedTask.ProgramSwitchEnabled = true;
			clonedTask.CommunicationProtocol = FlowConfigStore.NormalizeProtocolName(targetJob.ProtocolName);
			clonedTask.CommunicationChannel = FlowConfigStore.NormalizeChannelName(targetJob.ChannelName);
			RetargetClonedTaskPaths(sourceJob, targetJob, sourceTask, clonedTask);

			targetJob.Tasks.RemoveAll(x => x != null && string.Equals(x.TaskName, clonedTask.TaskName, StringComparison.OrdinalIgnoreCase));
			targetJob.Tasks.Add(clonedTask);
			CopyTaskFiles(sourceJob, targetJob, sourceTask);
			return clonedTask;
		}

		private static ProgramTaskSyncPlan BuildSwitchTaskSyncPlan(List<SwitchTaskSyncRequest> requests)
		{
			ProgramTaskSyncPlan plan = new ProgramTaskSyncPlan();
			if (requests == null)
			{
				return plan;
			}

			foreach (SwitchTaskSyncRequest request in requests)
			{
				if (request == null)
				{
					continue;
				}

				plan.MissingTaskCount++;
				AddUnique(plan.TaskNames, request.TaskName);
				if (request.TargetJob != null)
				{
					AddUnique(plan.TargetProgramNames, request.TargetJob.JobName);
				}
			}

			return plan;
		}

		private static List<SwitchTaskSyncRequest> BuildSwitchTaskSyncRequests(ProjectFlowConfig config, IEnumerable<string> taskNames, string sourceProgramName)
		{
			List<SwitchTaskSyncRequest> result = new List<SwitchTaskSyncRequest>();
			HashSet<string> taskFilter = NormalizeTaskNameSet(taskNames);
			string preferredSourceProgram = NormalizeProgramName(sourceProgramName);
			if (config == null || config.Protocols == null || taskFilter.Count <= 0)
			{
				return result;
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
					List<JobConfig> jobs = channel.Jobs
						.Where(x => x != null && !string.IsNullOrWhiteSpace(x.JobName))
						.ToList();

					foreach (JobConfig job in jobs)
					{
						job.ProtocolName = protocolName;
						job.ChannelName = channelName;
						if (job.Tasks == null)
						{
							job.Tasks = new List<TaskConfig>();
						}
					}

					List<string> switchTaskNames = jobs
						.SelectMany(x => x.Tasks)
						.Where(x => x != null &&
							x.ProgramSwitchEnabled &&
							!string.IsNullOrWhiteSpace(x.TaskName) &&
							taskFilter.Contains(x.TaskName.Trim()))
						.Select(x => x.TaskName.Trim())
						.Distinct(StringComparer.OrdinalIgnoreCase)
						.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
						.ToList();

					foreach (string taskName in switchTaskNames)
					{
						List<SwitchTaskSource> sources = jobs
							.Select(x => new SwitchTaskSource
							{
								Job = x,
								Task = FindTask(x, taskName)
							})
							.Where(x => x.Task != null && x.Task.ProgramSwitchEnabled)
							.OrderBy(x => ParseProgramNo(x.Job == null ? string.Empty : x.Job.ProgramNo))
							.ThenBy(x => x.Job == null ? string.Empty : x.Job.JobName)
							.ToList();

						if (sources.Count <= 0)
						{
							continue;
						}

						foreach (JobConfig targetJob in jobs)
						{
							TaskConfig targetTask = FindTask(targetJob, taskName);
							if (targetTask != null && !IsTaskEmpty(targetTask))
							{
								continue;
							}

							SwitchTaskSource source = string.IsNullOrWhiteSpace(preferredSourceProgram)
								? null
								: sources.FirstOrDefault(x =>
									x.Job != null &&
									string.Equals(NormalizeProgramName(x.Job.JobName), preferredSourceProgram, StringComparison.OrdinalIgnoreCase));
							if (source != null && object.ReferenceEquals(source.Job, targetJob))
							{
								source = null;
							}

							if (source == null)
							{
								source = sources.FirstOrDefault(x => !object.ReferenceEquals(x.Job, targetJob));
							}
							if (source == null)
							{
								source = sources[0];
							}

							result.Add(new SwitchTaskSyncRequest
							{
								SourceJob = source.Job,
								SourceTask = source.Task,
								TargetJob = targetJob,
								TaskName = taskName
							});
						}
					}
				}
			}

			return result;
		}

		private static HashSet<string> NormalizeTaskNameSet(IEnumerable<string> taskNames)
		{
			HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (taskNames == null)
			{
				return result;
			}

			foreach (string taskName in taskNames)
			{
				if (!string.IsNullOrWhiteSpace(taskName))
				{
					result.Add(taskName.Trim());
				}
			}

			return result;
		}

		private static TaskConfig FindTask(JobConfig job, string taskName)
		{
			if (job == null || job.Tasks == null || string.IsNullOrWhiteSpace(taskName))
			{
				return null;
			}

			string normalizedTaskName = taskName.Trim();
			return job.Tasks.FirstOrDefault(x =>
				x != null &&
				!string.IsNullOrWhiteSpace(x.TaskName) &&
				string.Equals(x.TaskName.Trim(), normalizedTaskName, StringComparison.OrdinalIgnoreCase));
		}

		private static bool IsTaskEmpty(TaskConfig task)
		{
			if (task == null)
			{
				return true;
			}

			bool hasSteps = task.Steps != null && task.Steps.Any(x => x != null);
			bool hasStepFlow = task.StepFlow != null && task.StepFlow.Any(x => x != null);
			return !hasSteps && !hasStepFlow;
		}

		private static void AddUnique(List<string> values, string value)
		{
			if (values == null || string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			if (!values.Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase)))
			{
				values.Add(value);
			}
		}

		private static int ParseProgramNo(string programNo)
		{
			int value;
			if (int.TryParse(programNo, out value))
			{
				return value;
			}

			return int.MaxValue;
		}

		private static IEnumerable<JobConfig> EnumerateJobs(ProjectFlowConfig config)
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

		private static bool ProgramExists(ProjectFlowConfig config, string programName)
		{
			return EnumerateJobs(config).Any(x => x != null && string.Equals(x.JobName, programName, StringComparison.OrdinalIgnoreCase));
		}

		private static string GetNextProgramNo(ProjectFlowConfig config, string protocolName, string channelName)
		{
			List<JobConfig> jobs = FlowConfigStore.GetJobs(config, protocolName, channelName);
			int index = 1;

			while (jobs != null && jobs.Any(j => string.Equals(j == null ? string.Empty : j.ProgramNo, index.ToString(), StringComparison.OrdinalIgnoreCase)))
			{
				index++;
			}

			return index.ToString();
		}

		private static void RetargetClonedTaskPaths(JobConfig sourceJob, JobConfig targetJob, TaskConfig sourceTask, TaskConfig clonedTask)
		{
			if (sourceJob == null || targetJob == null || sourceTask == null || clonedTask == null)
			{
				return;
			}

			string sourceFolder = FlowConfigStore.PathManager.ResolveExistingTaskFolder(sourceJob.ProtocolName, sourceJob.ChannelName, sourceJob.JobName, sourceTask.TaskName);
			string targetFolder = FlowConfigStore.PathManager.GetTaskFolder(targetJob.ProtocolName, targetJob.ChannelName, targetJob.JobName, clonedTask.TaskName);

			if (clonedTask.Steps == null)
			{
				return;
			}

			foreach (StepConfig step in clonedTask.Steps)
			{
				if (step == null)
				{
					continue;
				}

				step.StepFolder = Path.Combine("Task", clonedTask.TaskName, targetJob.JobName);
				step.SourceFilePath = RetargetPath(step.SourceFilePath, sourceFolder, targetFolder);
				step.ProjectFilePath = RetargetPath(step.ProjectFilePath, sourceFolder, targetFolder);

				if (step.VppFiles != null)
				{
					for (int i = 0; i < step.VppFiles.Count; i++)
					{
						step.VppFiles[i] = RetargetPath(step.VppFiles[i], sourceFolder, targetFolder);
					}
				}

				if (step.ScriptFiles != null)
				{
					for (int i = 0; i < step.ScriptFiles.Count; i++)
					{
						step.ScriptFiles[i] = RetargetPath(step.ScriptFiles[i], sourceFolder, targetFolder);
					}
				}
			}
		}

		private static string RetargetPath(string path, string sourceFolder, string targetFolder)
		{
			if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(sourceFolder) || string.IsNullOrWhiteSpace(targetFolder))
			{
				return path;
			}

			if (!Path.IsPathRooted(path))
			{
				return path;
			}

			try
			{
				string fullPath = Path.GetFullPath(path);
				string fullSource = Path.GetFullPath(sourceFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				if (!fullPath.StartsWith(fullSource + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
				{
					return path;
				}

				string relative = fullPath.Substring(fullSource.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				return Path.Combine(targetFolder, relative);
			}
			catch
			{
				return path;
			}
		}

		private static void CopyTaskFiles(JobConfig sourceJob, JobConfig targetJob, TaskConfig sourceTask)
		{
			if (sourceJob == null || targetJob == null || sourceTask == null || string.IsNullOrWhiteSpace(sourceTask.TaskName))
			{
				return;
			}

			string sourceFolder = FlowConfigStore.PathManager.ResolveExistingTaskFolder(sourceJob.ProtocolName, sourceJob.ChannelName, sourceJob.JobName, sourceTask.TaskName);
			string targetFolder = FlowConfigStore.PathManager.GetTaskFolder(targetJob.ProtocolName, targetJob.ChannelName, targetJob.JobName, sourceTask.TaskName);
			CopyDirectory(sourceFolder, targetFolder);
			Directory.CreateDirectory(Path.Combine(targetFolder, "VPP"));
			Directory.CreateDirectory(Path.Combine(targetFolder, "Script"));
			Directory.CreateDirectory(Path.Combine(targetFolder, "Hdev"));
		}

		public static void DeleteTaskFiles(JobConfig job, TaskConfig task)
		{
			if (job == null || task == null || string.IsNullOrWhiteSpace(job.JobName) || string.IsNullOrWhiteSpace(task.TaskName))
			{
				return;
			}

			ProjectPathManager path = FlowConfigStore.PathManager;
			List<string> deleted = new List<string>();
			foreach (string folder in path.GetTaskFolderCandidates(job.ProtocolName, job.ChannelName, job.JobName, task.TaskName))
			{
				if (string.IsNullOrWhiteSpace(folder) ||
					deleted.Any(x => string.Equals(x, folder, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}

				DeleteDirectoryIfUnder(folder, path.ProjectRoot);
				deleted.Add(folder);
			}

			string safeTaskName = path.MakeSafeName(task.TaskName);
			DeleteEmptyDirectoryIfUnder(Path.Combine(path.TaskRoot, safeTaskName), path.TaskRoot);
			DeleteEmptyDirectoryIfUnder(Path.Combine(path.ProjectRoot, safeTaskName), path.ProjectRoot);
		}

		public static void CleanupOrphanTaskFolders(ProjectFlowConfig config)
		{
			ProjectPathManager path = FlowConfigStore.PathManager;
			if (config == null || string.IsNullOrWhiteSpace(path.TaskRoot) || !Directory.Exists(path.TaskRoot))
			{
				return;
			}

			Dictionary<string, HashSet<string>> validJobsByTask = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (JobConfig job in EnumerateJobs(config))
			{
				if (job == null || string.IsNullOrWhiteSpace(job.JobName) || job.Tasks == null)
				{
					continue;
				}

				string safeJobName = path.MakeSafeName(job.JobName);
				foreach (TaskConfig task in job.Tasks)
				{
					if (task == null || string.IsNullOrWhiteSpace(task.TaskName))
					{
						continue;
					}

					string safeTaskName = path.MakeSafeName(task.TaskName);
					HashSet<string> validJobs;
					if (!validJobsByTask.TryGetValue(safeTaskName, out validJobs))
					{
						validJobs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						validJobsByTask[safeTaskName] = validJobs;
					}

					validJobs.Add(safeJobName);
				}
			}

			foreach (string taskFolder in Directory.GetDirectories(path.TaskRoot))
			{
				string taskFolderName = Path.GetFileName(taskFolder);
				HashSet<string> validJobs;
				if (string.IsNullOrWhiteSpace(taskFolderName) ||
					!validJobsByTask.TryGetValue(taskFolderName, out validJobs))
				{
					DeleteDirectoryIfUnder(taskFolder, path.TaskRoot);
					continue;
				}

				foreach (string jobFolder in Directory.GetDirectories(taskFolder))
				{
					string jobFolderName = Path.GetFileName(jobFolder);
					if (string.IsNullOrWhiteSpace(jobFolderName) || !validJobs.Contains(jobFolderName))
					{
						DeleteDirectoryIfUnder(jobFolder, path.TaskRoot);
					}
				}

				DeleteEmptyDirectoryIfUnder(taskFolder, path.TaskRoot);
			}
		}

		private static void CopyProgramHardware(string sourceProgramName, string newProgramName)
		{
			string source = Path.Combine(HardwareConfigStore.JobRootContainer, NormalizeProgramName(sourceProgramName));
			string target = Path.Combine(HardwareConfigStore.JobRootContainer, NormalizeProgramName(newProgramName));
			CopyDirectory(source, target);

			string legacySource = Path.Combine(HardwareConfigStore.LegacyJobRootContainer, NormalizeProgramName(sourceProgramName), "Hardware");
			string currentTarget = Path.Combine(target, "Hardware");
			CopyDirectory(legacySource, currentTarget);
			RewriteProgramHardwarePaths(target, sourceProgramName, newProgramName);
		}

		private static void RewriteProgramHardwarePaths(string targetProgramFolder, string sourceProgramName, string newProgramName)
		{
			if (string.IsNullOrWhiteSpace(targetProgramFolder) || !Directory.Exists(targetProgramFolder))
			{
				return;
			}

			string sourceProgram = NormalizeProgramName(sourceProgramName);
			string targetProgram = NormalizeProgramName(newProgramName);
			if (string.IsNullOrWhiteSpace(sourceProgram) || string.IsNullOrWhiteSpace(targetProgram))
			{
				return;
			}

			string currentSource = Path.GetFullPath(Path.Combine(HardwareConfigStore.JobRootContainer, sourceProgram));
			string currentTarget = Path.GetFullPath(Path.Combine(HardwareConfigStore.JobRootContainer, targetProgram));
			string legacySource = Path.GetFullPath(Path.Combine(HardwareConfigStore.LegacyJobRootContainer, sourceProgram));

			foreach (string file in Directory.GetFiles(targetProgramFolder, "*.xml", SearchOption.AllDirectories))
			{
				string text = File.ReadAllText(file);
				string updated = text
					.Replace(currentSource, currentTarget)
					.Replace(legacySource, currentTarget);

				if (!string.Equals(text, updated, StringComparison.Ordinal))
				{
					File.WriteAllText(file, updated);
				}
			}
		}

		private static void DeleteProgramHardware(string programName)
		{
			string current = Path.Combine(HardwareConfigStore.JobRootContainer, NormalizeProgramName(programName));
			DeleteDirectoryIfUnder(current, HardwareConfigStore.JobRootContainer);

			string legacy = Path.Combine(HardwareConfigStore.LegacyJobRootContainer, NormalizeProgramName(programName));
			DeleteDirectoryIfUnder(legacy, HardwareConfigStore.LegacyJobRootContainer);
		}

		private static void CopyDirectory(string sourceDir, string targetDir)
		{
			if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(targetDir) || !Directory.Exists(sourceDir))
			{
				return;
			}

			Directory.CreateDirectory(targetDir);

			foreach (string file in Directory.GetFiles(sourceDir))
			{
				File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
			}

			foreach (string dir in Directory.GetDirectories(sourceDir))
			{
				CopyDirectory(dir, Path.Combine(targetDir, Path.GetFileName(dir)));
			}
		}

		private static void DeleteDirectoryIfUnder(string folder, string root)
		{
			if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(root) || !Directory.Exists(folder))
			{
				return;
			}

			string fullFolder = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (!fullFolder.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(fullFolder, fullRoot, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			Directory.Delete(fullFolder, true);
		}

		private static void DeleteEmptyDirectoryIfUnder(string folder, string root)
		{
			if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(root) || !Directory.Exists(folder))
			{
				return;
			}

			string fullFolder = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (string.Equals(fullFolder, fullRoot, StringComparison.OrdinalIgnoreCase) ||
				!fullFolder.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (Directory.EnumerateFileSystemEntries(fullFolder).Any())
			{
				return;
			}

			Directory.Delete(fullFolder, false);
		}

		private static string NormalizeProgramName(string programName)
		{
			return HardwareConfigStore.NormalizeFileName(programName, string.Empty);
		}

		private static T CloneByXml<T>(T value) where T : class, new()
		{
			if (value == null)
			{
				return new T();
			}

			XmlSerializer serializer = new XmlSerializer(typeof(T));
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = System.Text.Encoding.UTF8;
			settings.Indent = false;

			using (StringWriter writer = new StringWriter())
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
				{
					serializer.Serialize(xmlWriter, value);
				}

				using (StringReader reader = new StringReader(writer.ToString()))
				{
					object obj = serializer.Deserialize(reader);
					return obj as T ?? new T();
				}
			}
		}
	}
}
