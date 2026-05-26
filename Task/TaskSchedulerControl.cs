using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace Aron_V3
{
	public partial class TaskSchedulerControl : UserControl, ILocalizable
	{
		private bool _loading = false;
		private const string COL_DISPLAY_OUTPUT = "DisplayOutputKey";
		private const string COL_DISPLAY_SLOT = "DisplaySlotName";
		private const string COL_DISPLAY_MODE = "DisplayMode";

		public TaskSchedulerControl()
		{
			InitializeComponent();
			InitDisplayBindingColumns();
			MakeStepNameColumnReadOnly();
			BindStepGridReadOnlyEvents();
			ApplyFlowUiPolicy();
			BindStepFlowGridEvents();


			listJobs.SelectedIndexChanged -= listJobs_SelectedIndexChanged;
			listJobs.SelectedIndexChanged += listJobs_SelectedIndexChanged;

			listTasks.SelectedIndexChanged -= listTasks_SelectedIndexChanged;
			listTasks.SelectedIndexChanged += listTasks_SelectedIndexChanged;

			listSteps.SelectedIndexChanged -= listSteps_SelectedIndexChanged;
			listSteps.SelectedIndexChanged += listSteps_SelectedIndexChanged;

			FlowConfigStore.FlowConfigSaved -= FlowConfigStore_FlowConfigSaved;
			FlowConfigStore.FlowConfigSaved += FlowConfigStore_FlowConfigSaved;



			LoadFlowConfigToUI();
		}

		private void FlowConfigStore_FlowConfigSaved(object sender, EventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new EventHandler(FlowConfigStore_FlowConfigSaved), sender, e);
				return;
			}

			string oldJob = GetSelectedJobName();
			string oldTask = GetSelectedTaskName();
			string oldStep = GetSelectedStepName();

			LoadFlowConfigToUI();

			SelectListItem(listJobs, oldJob);
			RefreshTasksByJob(GetSelectedJobName());
			SelectListItem(listTasks, oldTask);
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			SelectListItem(listSteps, oldStep);
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
		}

		private void LoadFlowConfigToUI()
		{
			_loading = true;

			try
			{
				listJobs.Items.Clear();

				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

				foreach (JobConfig job in config.Jobs)
				{
					listJobs.Items.Add(job.JobName);
				}

				if (listJobs.Items.Count > 0)
				{
					listJobs.SelectedIndex = 0;
				}

				RefreshTasksByJob(GetSelectedJobName());
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

			RefreshTasksByJob(GetSelectedJobName());
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
			UpdateStepDetailTitle();
		}

		private void listTasks_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_loading) return;

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
			RefreshTasksByJob(GetSelectedJobName());
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
			UpdateStepDetailTitle();
		}

		private void RefreshTasksByJob(string jobName)
		{
			listTasks.Items.Clear();

			if (string.IsNullOrEmpty(jobName)) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = config.Jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job == null) return;

			foreach (TaskConfig task in job.Tasks.OrderBy(t => t.RunOrder))
			{
				listTasks.Items.Add(task.TaskName);
			}

			if (listTasks.Items.Count > 0)
			{
				listTasks.SelectedIndex = 0;
			}
		}

		private string GetNextJobName(ProjectFlowConfig config)
		{
			int index = 1;

			while (true)
			{
				string name = "Job_" + index.ToString("000");

				if (config == null || config.Jobs == null ||
					!config.Jobs.Any(j => string.Equals(j.JobName, name, StringComparison.OrdinalIgnoreCase)))
				{
					return name;
				}

				index++;
			}
		}

		private void btnAddJob_Click(object sender, EventArgs e)
		{
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

			string jobName = GetNextJobName(config);

			JobConfig job = new JobConfig();
			job.JobName = jobName;
			job.Enabled = true;
			config.Jobs.Add(job);
			Directory.CreateDirectory(FlowConfigStore.PathManager.GetJobFolder(jobName));

			FlowConfigStore.Save(config);

			LoadFlowConfigToUI();
			SelectListItem(listJobs, jobName);
		}

		private void btnDeleteJob_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			if (string.IsNullOrEmpty(jobName)) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = config.Jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

			if (job != null)
			{
				config.Jobs.Remove(job);
				DeleteJobFolder(jobName);
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
			JobConfig job = FlowConfigStore.GetOrCreateJob(config, jobName);

			string taskName = "Task_New_" + (job.Tasks.Count + 1).ToString("00");
			TaskConfig task = FlowConfigStore.CreateDefaultTask(jobName, taskName, job.Tasks.Count + 1);
			job.Tasks.Add(task);

			FlowConfigStore.Save(config);

			RefreshTasksByJob(jobName);
			SelectListItem(listTasks, taskName);
			RefreshStepLibraryByTask(jobName, taskName);
			RefreshStepFlowGrid(jobName, taskName);
		}

		private void btnDeleteTask_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName)) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			JobConfig job = FlowConfigStore.GetOrCreateJob(config, jobName);
			TaskConfig task = job.Tasks.FirstOrDefault(t => string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase));

			if (task != null)
			{
				job.Tasks.Remove(task);
				ReorderTasks(job);
				DeleteTaskFolder(jobName, taskName);
				FlowConfigStore.Save(config);

				RefreshTasksByJob(jobName);
				RefreshStepLibraryByTask(jobName, GetSelectedTaskName());
				RefreshStepFlowGrid(jobName, GetSelectedTaskName());
			}
		}

		#endregion

		#region Step Library：中间“所有 step”

		private void RefreshStepLibraryByTask(string jobName, string taskName)
		{
			listSteps.Items.Clear();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName)) return;

			TaskConfig task = GetTaskConfig(jobName, taskName);
			if (task == null) return;

			foreach (StepConfig step in task.Steps.OrderBy(s => s.RunOrder))
			{
				listSteps.Items.Add(new StepListItem(step.StepName, GetStepDisplayText(step)));
			}

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = 0;
			}
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

		// 中间 Step 库上方 “批量”：从本地选择多个 VPP 或 Script，只添加到 Step 库，不加入右侧执行流程，不立即复制到 Project。
		private void btnBatchAddStepItem_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName))
			{
				MessageBox.Show("Please select Job and Task first.", "Batch Add Step", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			using (OpenFileDialog dialog = CreateStepFileDialog(true))
			{
				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				foreach (string filePath in dialog.FileNames)
				{
					AddStepToLibraryByLocalFile(jobName, taskName, filePath);
				}
			}
		}

		private OpenFileDialog CreateStepFileDialog(bool multiSelect)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = multiSelect ? "Select VPP or Script Files" : "Select VPP or Script File";
			dialog.Filter = "Vision Step Files (*.vpp;*.cs;*.csx;*.txt)|*.vpp;*.cs;*.csx;*.txt|VPP Files (*.vpp)|*.vpp|Script Files (*.cs;*.csx;*.txt)|*.cs;*.csx;*.txt|All Files (*.*)|*.*";
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
					"Unsupported file type.\r\n\r\nOnly .vpp, .cs, .csx, .txt are supported now.\r\n\r\nFile: " + sourceFilePath,
					"Add Step Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
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
			// 只有 Step 加入右侧执行流程并点击保存时，才复制到 Project/Steps/Job/Task/VPP 或 Scripts。
			step.SourceFilePath = sourceFilePath;
			step.ProjectFilePath = string.Empty;
			step.Remark = "Source: " + sourceFilePath;

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
				task.StepFlow.RemoveAll(x => string.Equals(x.StepName, stepName, StringComparison.OrdinalIgnoreCase));

				ReorderStepLibrary(task);

				// 注意：StepFlow 的 RunOrder 允许重复，所以不要重排 StepFlow。
				FlowConfigStore.Save(config);

				RefreshStepLibraryByTask(jobName, taskName);
				RefreshStepFlowGrid(jobName, taskName);
			}
		}


		private void btnRefreshStepItem_Click(object sender, EventArgs e)
		{
			RefreshStepLibraryByTask(GetSelectedJobName(), GetSelectedTaskName());
			RefreshStepFlowGrid(GetSelectedJobName(), GetSelectedTaskName());
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

			// 新增时默认放到最后一组后面。
			// 你可以在右侧表格里手动改成相同 RunOrder，例如 1、1、2，实现同组异步并行。
			item.RunOrder = GetNextDefaultRunOrder(task);
			item.Enabled = true;
			item.Remark = BuildStepFlowRemark(step);
			if (step.StepType == StepType.Script)
			{
				item.DisplayOutputKey = "Not Use";
				item.DisplaySlotName = "Not Show";
				item.DisplayMode = "Fit";
				item.ScriptInputStepKeys = string.Empty;
			}
			else
			{
				item.DisplayOutputKey = step.StepType == StepType.Vpp
					? "LastRun.CogIPOneImageTool1.OutputImage"
					: "Not Use";
				item.DisplaySlotName = "Not Show";
				item.DisplayMode = "Fit";
			}

			task.StepFlow.Add(item);

			// 不调用 ReorderStepFlow，因为 RunOrder 允许重复。
			FlowConfigStore.Save(config);

			RefreshStepFlowGrid(jobName, taskName);
			SelectFlowGridRowByStepName(stepName);
		}

		private int GetNextDefaultRunOrder(TaskConfig task)
		{
			if (task == null || task.StepFlow == null || task.StepFlow.Count <= 0)
			{
				return 1;
			}

			return task.StepFlow.Max(x => x.RunOrder) + 1;
		}

		private void RefreshStepFlowGrid(string jobName, string taskName)
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
				int rowIndex = dgvSteps.Rows.Add();
				DataGridViewRow row = dgvSteps.Rows[rowIndex];

				row.Tag = item;
				row.Cells["colStep"].Value = item.StepName;
				row.Cells["colImageSource"].Value = item.InputImageKey;
				row.Cells["colRunOrder"].Value = item.RunOrder.ToString();
				row.Cells["colRemark"].Value = item.Remark;

				if (dgvSteps.Columns.Contains(COL_DISPLAY_OUTPUT))
				{
					row.Cells[COL_DISPLAY_OUTPUT].Value =
						string.IsNullOrWhiteSpace(item.DisplayOutputKey) ? "Not Use" : item.DisplayOutputKey;
				}

				if (dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
				{
					row.Cells[COL_DISPLAY_SLOT].Value =
						string.IsNullOrWhiteSpace(item.DisplaySlotName) ? "Not Show" : item.DisplaySlotName;
				}

				ApplyStepFlowRowVisual(row, task);
			}
		}


		private void btnDeleteSelected_Click(object sender, EventArgs e)
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName)) return;
			if (dgvSteps.SelectedRows.Count <= 0) return;

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);

			if (task == null) return;

			for (int i = dgvSteps.SelectedRows.Count - 1; i >= 0; i--)
			{
				int rowIndex = dgvSteps.SelectedRows[i].Index;

				if (rowIndex >= 0 && rowIndex < task.StepFlow.Count)
				{
					task.StepFlow.RemoveAt(rowIndex);
				}
			}

			// 关键：删除右侧流程行时，不要重新编号，因为 RunOrder 允许重复。
			FlowConfigStore.Save(config);

			RefreshStepFlowGrid(jobName, taskName);
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

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);

			if (task == null) return;
			if (oldIndex < 0 || newIndex < 0 || newIndex >= task.StepFlow.Count) return;

			StepFlowItem item = task.StepFlow[oldIndex];
			task.StepFlow.RemoveAt(oldIndex);
			task.StepFlow.Insert(newIndex, item);

			// 上移下移只改变显示顺序，不强制修改 RunOrder。
			// 如果你希望上移下移也交换 RunOrder，可以后续再加。
			FlowConfigStore.Save(config);

			RefreshStepFlowGrid(jobName, taskName);

			if (newIndex >= 0 && newIndex < dgvSteps.Rows.Count)
			{
				dgvSteps.ClearSelection();
				dgvSteps.Rows[newIndex].Selected = true;
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

			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			TaskConfig task = GetTaskConfig(config, jobName, taskName);

			if (task == null)
			{
				MessageBox.Show("Task config was not found.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			task.StepFlow.Clear();

			int fallbackOrder = 1;

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

				StepFlowItem item = new StepFlowItem();
				item.StepName = stepName;
				item.InputImageKey = GetCellString(row, "colImageSource");
				item.RunOrder = GetCellInt(row, "colRunOrder", fallbackOrder);
				item.Enabled = true;
				item.Remark = GetCellString(row, "colRemark");

				if (usedStep.StepType == StepType.Script)
				{
					// Script 只处理数据，不绑定图像源/输出图像/显示框。
					item.InputImageKey = string.Empty;
					item.DisplayOutputKey = "Not Use";
					item.DisplaySlotName = "Not Show";
					item.DisplayMode = "Fit";
					item.ScriptInputStepKeys = GetRowScriptInputStepKeys(row);
				}
				else
				{
					item.DisplayOutputKey = GetCellString(row, COL_DISPLAY_OUTPUT);
					item.DisplaySlotName = GetCellString(row, COL_DISPLAY_SLOT);
					item.DisplayMode = "Fit";

					if (string.IsNullOrWhiteSpace(item.DisplayOutputKey))
					{
						item.DisplayOutputKey = "Not Use";
					}

					if (string.IsNullOrWhiteSpace(item.DisplaySlotName))
					{
						item.DisplaySlotName = "Not Show";
					}
				}

				task.StepFlow.Add(item);
				fallbackOrder++;
			}

			FlowConfigStore.Save(config);

			RefreshStepLibraryByTask(jobName, taskName);
			RefreshStepFlowGrid(jobName, taskName);

			MessageBox.Show(
				"Task flow configuration saved.\r\nUsed step files have been copied to the project folder.",
				"Save",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}


		#endregion

		#region Save Used Step Files

		private bool SaveUsedStepFileToProject(string jobName, string taskName, StepConfig step)
		{
			if (step == null)
			{
				return false;
			}

			if (!string.IsNullOrEmpty(step.ProjectFilePath))
			{
				string existedProjectFile = GetAbsoluteProjectStepFilePath(jobName, taskName, step);

				if (File.Exists(existedProjectFile))
				{
					return true;
				}
			}

			string sourceFilePath = step.SourceFilePath;

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

			FlowConfigStore.PathManager.EnsureStepFolder(jobName, taskName, step.StepName);

			// 新目录结构：
			// Project\Job\<JobName>\Task\<TaskName>\VPP\xxx.vpp
			// Project\Job\<JobName>\Task\<TaskName>\Scripts\xxx.csx
			string taskFolder = FlowConfigStore.PathManager.GetStepFolder(jobName, taskName, step.StepName);
			string subFolderName = step.StepType == StepType.Vpp ? "VPP" : "Scripts";
			string targetFolder = Path.Combine(taskFolder, subFolderName);

			Directory.CreateDirectory(targetFolder);

			string sourceFileName = Path.GetFileName(sourceFilePath);
			string targetFileName = MakeUniqueProjectStepFileName(taskFolder, subFolderName, sourceFileName, step.StepName);
			string targetFilePath = Path.Combine(targetFolder, targetFileName);

			try
			{
				File.Copy(sourceFilePath, targetFilePath, true);
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

			step.ProjectFilePath = relativeFilePath;

			if (step.StepType == StepType.Vpp)
			{
				step.VppFiles.Clear();
				step.VppFiles.Add(relativeFilePath);
			}
			else if (step.StepType == StepType.Script)
			{
				step.ScriptFiles.Clear();
				step.ScriptFiles.Add(relativeFilePath);
			}

			// 新结构记录：
			// Job\<JobName>\Task\<TaskName>
			step.StepFolder = Path.Combine("Job", jobName, "Task", taskName);

			return true;
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

			string stepFolder = FlowConfigStore.PathManager.GetStepFolder(jobName, taskName, step.StepName);
			return Path.Combine(stepFolder, relativeFilePath);
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


		#endregion

		#region Delete Local Folders

		private void DeleteJobFolder(string jobName)
		{
			try
			{
				string jobFolder = FlowConfigStore.PathManager.GetJobFolder(jobName);
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
				string taskFolder = FlowConfigStore.PathManager.GetTaskFolder(jobName, taskName);

				if (Directory.Exists(taskFolder))
				{
					Directory.Delete(taskFolder, true);
				}

				// 兼容旧目录：Project\Job\<JobName>\<TaskName>
				string legacyTaskFolder = Path.Combine(FlowConfigStore.PathManager.GetJobFolder(jobName), taskName);

				if (Directory.Exists(legacyTaskFolder) &&
					!string.Equals(legacyTaskFolder, taskFolder, StringComparison.OrdinalIgnoreCase))
				{
					Directory.Delete(legacyTaskFolder, true);
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
			"Not Use",
			"InputImage",
			"OutputImage",
			"ResultImage",
			"LastRun.OutputImage",
			"LastRun.CogIPOneImageTool1.OutputImage",
			"DisplayImage",
			"DebugImage"
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

			// 显示方式固定为 Fit，不再在流程表中显示该列。
			if (dgvSteps.Columns.Contains(COL_DISPLAY_MODE))
			{
				dgvSteps.Columns.Remove(COL_DISPLAY_MODE);
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_OUTPUT))
			{
				dgvSteps.Columns[COL_DISPLAY_OUTPUT].FillWeight = 90;
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
			{
				dgvSteps.Columns[COL_DISPLAY_SLOT].FillWeight = 90;
			}

		}

		private void RefreshDisplaySlotComboColumn()
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

			foreach (string slotName in DisplayLayoutStore.GetDisplaySlotNames())
			{
				col.Items.Add(slotName);
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
				ShowScriptInputStepSelectorForRow(row);
			}
		}

		private StepConfig GetStepConfigForRow(DataGridViewRow row, TaskConfig task)
		{
			if (row == null || task == null || task.Steps == null)
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

		private void ApplyStepFlowRowVisual(DataGridViewRow row, TaskConfig task)
		{
			if (row == null || task == null)
			{
				return;
			}

			bool isScript = IsScriptRow(row, task);
			Color disabledBack = Color.FromArgb(18, 28, 40);
			Color disabledFore = Color.FromArgb(120, 140, 155);
			Color normalBack = Color.FromArgb(1, 8, 16);
			Color normalFore = Color.White;

			SetOptionalCellState(row, "colImageSource", !isScript, isScript ? string.Empty : null, disabledBack, disabledFore, normalBack, normalFore);
			SetOptionalCellState(row, COL_DISPLAY_OUTPUT, !isScript, isScript ? "Not Use" : null, disabledBack, disabledFore, normalBack, normalFore);
			SetOptionalCellState(row, COL_DISPLAY_SLOT, !isScript, isScript ? "Not Show" : null, disabledBack, disabledFore, normalBack, normalFore);
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

			if (IsScriptRow(row, task))
			{
				MessageBox.Show("Script step does not use image source.", "Image Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			List<string> available = GetAvailableImageSources(task);
			if (available.Count <= 0)
			{
				MessageBox.Show("No image source is available for current Task. Please configure image source in Trigger Manager first.", "Image Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			List<string> selected = ParseSeparatedKeys(GetCellString(row, "colImageSource"));

			using (MultiCheckSelectForm form = new MultiCheckSelectForm("Select Image Sources", "Select one or more image sources", available, selected, null))
			{
				if (form.ShowDialog(this) == DialogResult.OK)
				{
					row.Cells["colImageSource"].Value = JoinKeys(form.SelectedItems);
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

			foreach (string key in task.ImageSourceKeyList)
			{
				AddUniqueKey(result, key);
			}

			// 兜底：如果 Task 没有配置图像源，也允许从当前 Task 的 VPP Step 中选择。

			if (result.Count <= 0 && task.Steps != null)
			{
				foreach (StepConfig step in task.Steps)
				{
					if (step == null || step.StepType != StepType.Vpp)
					{
						continue;
					}

					if (step.VppFiles != null && step.VppFiles.Count > 0)
					{
						foreach (string file in step.VppFiles)
						{
							AddUniqueKey(result, Path.GetFileName(file));
						}
					}
					else if (!string.IsNullOrWhiteSpace(step.StepName))
					{
						AddUniqueKey(result, step.StepName);
					}
				}
			}

			return result;
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

			if (dgvSteps.Columns[e.ColumnIndex].Name == "colStep")
			{
				e.Cancel = true;
			}
		}

		#endregion



		#region Helper

		private string GetSelectedJobName()
		{
			return listJobs.SelectedItem == null ? string.Empty : listJobs.SelectedItem.ToString();
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

			if (item != null)
			{
				return item.StepName;
			}

			return listSteps.SelectedItem.ToString();
		}


		private TaskConfig GetTaskConfig(string jobName, string taskName)
		{
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			return GetTaskConfig(config, jobName, taskName);
		}

		private TaskConfig GetTaskConfig(ProjectFlowConfig config, string jobName, string taskName)
		{
			JobConfig job = config.Jobs.FirstOrDefault(j => string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));
			if (job == null) return null;

			return job.Tasks.FirstOrDefault(t => string.Equals(t.TaskName, taskName, StringComparison.OrdinalIgnoreCase));
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

				if (stepItem != null)
				{
					if (string.Equals(stepItem.StepName, itemText, StringComparison.OrdinalIgnoreCase) ||
						string.Equals(stepItem.DisplayText, itemText, StringComparison.OrdinalIgnoreCase))
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

			lblStepsTitle.Text = "当前 Job:  " + jobName + "    Task:  " + taskName;
		}

		private class StepListItem
		{
			public string StepName { get; private set; }
			public string DisplayText { get; private set; }

			public StepListItem(string stepName, string displayText)
			{
				StepName = stepName;
				DisplayText = displayText;
			}

			public override string ToString()
			{
				return DisplayText;
			}
		}


		#endregion

		public void ApplyLanguage(bool isEnglish)
		{
			if (isEnglish)
			{
				lblJobsTitle.Text = "All JobID";
				lblTasksTitle.Text = "All Task";
				lblStepListTitle.Text = "All Step";
				UpdateStepDetailTitle();

				colStep.HeaderText = "Step";
				colImageSource.HeaderText = "Image Source";
				colRunOrder.HeaderText = "Run Order";
				colRemark.HeaderText = "Remark";

				btnAddStep.Text = "+  Add Operator";
				btnDeleteSelected.Text = "▦  Delete";
				HideMoveButtons();
				btnSave.Text = "▣  Save";
			}
			else
			{
				lblJobsTitle.Text = "所有 JobID";
				lblTasksTitle.Text = "所有 task";
				lblStepListTitle.Text = "所有 step";
				UpdateStepDetailTitle();

				colStep.HeaderText = "step";
				colImageSource.HeaderText = "图像源";
				colRunOrder.HeaderText = "执行步序";
				colRemark.HeaderText = "备注";

				btnAddStep.Text = "+  新增算子";
				btnDeleteSelected.Text = "▦  删除选中";
				HideMoveButtons();
				btnSave.Text = "▣  保存";
			}
			if (dgvSteps.Columns.Contains(COL_DISPLAY_OUTPUT))
			{
				dgvSteps.Columns[COL_DISPLAY_OUTPUT].HeaderText = isEnglish ? "Output Image" : "输出图像";
			}

			if (dgvSteps.Columns.Contains(COL_DISPLAY_SLOT))
			{
				dgvSteps.Columns[COL_DISPLAY_SLOT].HeaderText = isEnglish ? "Display Slot" : "绑定显示框";
			}

			btnAddStepItem.Text = "+";
			btnBatchAddStepItem.Text = "▦";
			btnDeleteStepItem.Text = "-";
			btnRefreshStepItem.Text = "↻";
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

	public class MultiCheckSelectForm : Form
	{
		protected CheckedListBox list;
		private Button btnClear;
		private Button btnOK;
		private Button btnCancel;

		public List<string> SelectedItems { get; private set; }

		public MultiCheckSelectForm(string title, string prompt, List<string> items, List<string> selected, List<string> disabledItems)
		{
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

			btnClear = CreateButton("Clear", 32, 18, 130);
			btnOK = CreateButton("OK", 420, 18, 130);
			btnCancel = CreateButton("Cancel", 575, 18, 130);

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
	}

	public class ScriptInputSourceSelectForm : Form
	{
		private CheckedListBox list;
		private Button btnOK;
		private Button btnCancel;
		private Button btnClear;
		private List<SelectableStepSourceItem> _items;

		public List<string> SelectedItems { get; private set; }

		public ScriptInputSourceSelectForm(string title, string prompt, List<SelectableStepSourceItem> items, List<string> selected)
		{
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

			btnClear = CreateButton("Clear", 32, 18, 130);
			btnOK = CreateButton("OK", 420, 18, 130);
			btnCancel = CreateButton("Cancel", 575, 18, 130);

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
				text += "    (not previous)";
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
	}
}
