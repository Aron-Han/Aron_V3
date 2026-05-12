using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class TaskSchedulerControl : UserControl, ILocalizable
	{
		private bool _loading = false;

		public TaskSchedulerControl()
		{
			InitializeComponent();

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

		private void btnAddJob_Click(object sender, EventArgs e)
		{
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();

			string jobName = "Job_" + (config.Jobs.Count + 1).ToString("000");

			JobConfig job = new JobConfig();
			job.JobName = jobName;
			job.Enabled = true;
			config.Jobs.Add(job);

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

			if (!string.IsNullOrEmpty(task.ImageSourceKey) &&
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
			dgvSteps.Rows.Clear();

			if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(taskName)) return;

			TaskConfig task = GetTaskConfig(jobName, taskName);
			if (task == null) return;

			foreach (StepFlowItem item in task.StepFlow.OrderBy(x => x.RunOrder))
			{
				dgvSteps.Rows.Add(
					item.StepName,
					item.InputImageKey,
					item.RunOrder.ToString(),
					item.Remark);
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

				string stepName = GetCellString(row, 0);

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

				// 关键：只有右侧流程中真正使用到的 Step，点击保存时才复制到 Project/Steps 对应目录。
				bool fileSaved = SaveUsedStepFileToProject(jobName, taskName, usedStep);

				if (!fileSaved)
				{
					continue;
				}

				StepFlowItem item = new StepFlowItem();
				item.StepName = stepName;
				item.InputImageKey = GetCellString(row, 1);

				// 关键：执行步序允许重复，例如 1、1、2。
				// RunOrder 相同的一组 Step 后续由 TaskRunner 并行执行。
				item.RunOrder = GetCellInt(row, 2, fallbackOrder);

				item.Enabled = true;
				item.Remark = GetCellString(row, 3);

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

			// 已经保存到 Project 内部的，不重复处理。
			if (!string.IsNullOrEmpty(step.ProjectFilePath))
			{
				string existedProjectFile = GetAbsoluteProjectStepFilePath(jobName, taskName, step);

				if (File.Exists(existedProjectFile))
				{
					return true;
				}
			}

			string sourceFilePath = step.SourceFilePath;

			// 兼容旧数据：
			// 如果 SourceFilePath 为空，但 VppFiles / ScriptFiles 里已有相对路径，则尝试从 Project 目录读取。
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
			// Project/Steps/JobName/TaskName/VPP/xxx.vpp
			// Project/Steps/JobName/TaskName/Scripts/xxx.csx
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

			// StepFolder 也改成新结构
			step.StepFolder = Path.Combine("Steps", jobName, taskName);

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
				if (Directory.Exists(taskFolder)) Directory.Delete(taskFolder, true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to delete task folder.\r\n\r\n" + ex.Message, "Delete Task Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void DeleteStepFolder(string jobName, string taskName, string stepName)
		{
			try
			{
				ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
				TaskConfig task = GetTaskConfig(config, jobName, taskName);

				if (task == null)
				{
					return;
				}

				StepConfig step = task.Steps.FirstOrDefault(s =>
					string.Equals(s.StepName, stepName, StringComparison.OrdinalIgnoreCase));

				if (step == null)
				{
					return;
				}

				DeleteStepProjectFile(jobName, taskName, step);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Failed to delete step file.\r\n\r\n" + ex.Message,
					"Delete Step File",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
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
				btnMoveUp.Text = "▲  Move Up";
				btnMoveDown.Text = "▼  Move Down";
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
				btnMoveUp.Text = "▲  上移选中";
				btnMoveDown.Text = "▼  下移选中";
				btnSave.Text = "▣  保存";
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
}
