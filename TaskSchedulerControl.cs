using System;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class TaskSchedulerControl : UserControl, ILocalizable
	{
		public TaskSchedulerControl()
		{
			InitializeComponent();
			LoadDemoData();
		}

		private void LoadDemoData()
		{
			listJobs.Items.Clear();
			for (int i = 1; i <= 10; i++)
			{
				listJobs.Items.Add("Job_" + i.ToString("000"));
			}

			listTasks.Items.Clear();
			listTasks.Items.AddRange(new object[]
			{
				"Task_Main",
				"Task_Inspect",
				"Task_Locate",
				"Task_Measure",
				"Task_OCR",
				"Task_Align",
				"Task_Classify"
			});

			listSteps.Items.Clear();
			for (int i = 1; i <= 6; i++)
			{
				listSteps.Items.Add("Step_" + i.ToString("000"));
			}

			if (listJobs.Items.Count > 0)
			{
				listJobs.SelectedIndex = 0;
			}

			if (listTasks.Items.Count > 0)
			{
				listTasks.SelectedIndex = 0;
			}

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = 0;
			}

			UpdateStepDetailTitle();
		}

		#region Job 双击 / 按钮事件

		private void listJobs_DoubleClick(object sender, EventArgs e)
		{
			if (listJobs.SelectedItem == null)
			{
				return;
			}

			string selectedJobName = listJobs.SelectedItem.ToString();

			// 后续这里替换为从 XML 中读取当前 Job 对应的 Task 列表
			RefreshTasksByJob(selectedJobName);

			UpdateStepDetailTitle();
		}

		private void RefreshTasksByJob(string jobName)
		{
			listTasks.Items.Clear();

			// TODO:
			// 后续从 Project/Config/Flow/TaskSchedulerConfig.xml 中读取：
			// 当前 JobName 下绑定的 Task 列表。
			//
			// 当前先用 Demo 数据模拟刷新。
			listTasks.Items.AddRange(new object[]
			{
				"Task_Main",
				"Task_Inspect",
				"Task_Locate",
				"Task_Measure",
				"Task_OCR",
				"Task_Align",
				"Task_Classify"
			});

			if (listTasks.Items.Count > 0)
			{
				listTasks.SelectedIndex = 0;
			}

			// Job 切换后，Step 目录和右侧明细也建议同步刷新
			RefreshStepsByTask(GetSelectedJobName(), GetSelectedTaskName());
			LoadStepDetailFromXml(GetSelectedJobName(), GetSelectedTaskName(), GetSelectedStepName());
		}

		private void btnAddJob_Click(object sender, EventArgs e)
		{
			string jobName = "Job_" + (listJobs.Items.Count + 1).ToString("000");
			listJobs.Items.Add(jobName);
			listJobs.SelectedIndex = listJobs.Items.Count - 1;
			UpdateStepDetailTitle();
		}

		private void btnDeleteJob_Click(object sender, EventArgs e)
		{
			if (listJobs.SelectedIndex < 0)
			{
				return;
			}

			int index = listJobs.SelectedIndex;
			listJobs.Items.RemoveAt(index);

			if (listJobs.Items.Count > 0)
			{
				listJobs.SelectedIndex = Math.Min(index, listJobs.Items.Count - 1);
			}

			UpdateStepDetailTitle();
		}

		#endregion

		#region Task 双击 / 按钮事件

		private void listTasks_DoubleClick(object sender, EventArgs e)
		{
			if (listTasks.SelectedItem == null)
			{
				return;
			}

			string selectedJobName = GetSelectedJobName();
			string selectedTaskName = GetSelectedTaskName();

			UpdateStepDetailTitle();

			// 后续这里替换为从 XML 中读取当前 Job + Task 对应的 Step 目录
			RefreshStepsByTask(selectedJobName, selectedTaskName);

			// 后续这里替换为从 XML 中读取当前 Job + Task + Step 对应的明细表格
			LoadStepDetailFromXml(selectedJobName, selectedTaskName, GetSelectedStepName());
		}

		private void RefreshStepsByTask(string jobName, string taskName)
		{
			listSteps.Items.Clear();

			// TODO:
			// 后续从 Project/Config/Flow/TaskSchedulerConfig.xml 中读取：
			// 当前 JobName + TaskName 下绑定的 Step 目录。
			//
			// 当前先用 Demo 数据模拟刷新。
			for (int i = 1; i <= 6; i++)
			{
				listSteps.Items.Add("Step_" + i.ToString("000"));
			}

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = 0;
			}
		}

		private void btnAddTask_Click(object sender, EventArgs e)
		{
			string taskName = "Task_New_" + (listTasks.Items.Count + 1).ToString("00");
			listTasks.Items.Add(taskName);
			listTasks.SelectedIndex = listTasks.Items.Count - 1;
			UpdateStepDetailTitle();
		}

		private void btnDeleteTask_Click(object sender, EventArgs e)
		{
			if (listTasks.SelectedIndex < 0)
			{
				return;
			}

			int index = listTasks.SelectedIndex;
			listTasks.Items.RemoveAt(index);

			if (listTasks.Items.Count > 0)
			{
				listTasks.SelectedIndex = Math.Min(index, listTasks.Items.Count - 1);
			}

			UpdateStepDetailTitle();
		}

		#endregion

		#region Step 目录按钮 / 双击事件

		private void listSteps_DoubleClick(object sender, EventArgs e)
		{
			LoadStepDetailFromXml(GetSelectedJobName(), GetSelectedTaskName(), GetSelectedStepName());
		}

		private void btnAddStepItem_Click(object sender, EventArgs e)
		{
			string stepName = "Step_" + (listSteps.Items.Count + 1).ToString("000");
			listSteps.Items.Add(stepName);
			listSteps.SelectedIndex = listSteps.Items.Count - 1;
		}

		private void btnBatchAddStepItem_Click(object sender, EventArgs e)
		{
			int startIndex = listSteps.Items.Count + 1;

			for (int i = 0; i < 5; i++)
			{
				listSteps.Items.Add("Step_" + (startIndex + i).ToString("000"));
			}

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = startIndex - 1;
			}
		}

		private void btnDeleteStepItem_Click(object sender, EventArgs e)
		{
			if (listSteps.SelectedIndex < 0)
			{
				return;
			}

			int index = listSteps.SelectedIndex;
			listSteps.Items.RemoveAt(index);

			if (listSteps.Items.Count > 0)
			{
				listSteps.SelectedIndex = Math.Min(index, listSteps.Items.Count - 1);
			}
		}

		private void btnRefreshStepItem_Click(object sender, EventArgs e)
		{
			RefreshStepsByTask(GetSelectedJobName(), GetSelectedTaskName());
			LoadStepDetailFromXml(GetSelectedJobName(), GetSelectedTaskName(), GetSelectedStepName());
		}

		#endregion

		#region 当前 Step 详细信息按钮事件

		private void btnAddStep_Click(object sender, EventArgs e)
		{
			string stepName = GetSelectedStepName();

			if (string.IsNullOrEmpty(stepName))
			{
				stepName = "Step_001";
			}

			dgvSteps.Rows.Add(stepName, "Cam1", "1", "");
		}

		private void btnDeleteSelected_Click(object sender, EventArgs e)
		{
			for (int i = dgvSteps.SelectedRows.Count - 1; i >= 0; i--)
			{
				DataGridViewRow row = dgvSteps.SelectedRows[i];
				if (!row.IsNewRow)
				{
					dgvSteps.Rows.Remove(row);
				}
			}
		}

		private void btnMoveUp_Click(object sender, EventArgs e)
		{
			if (dgvSteps.SelectedRows.Count <= 0)
			{
				return;
			}

			int index = dgvSteps.SelectedRows[0].Index;
			if (index <= 0)
			{
				return;
			}

			MoveRow(index, index - 1);
		}

		private void btnMoveDown_Click(object sender, EventArgs e)
		{
			if (dgvSteps.SelectedRows.Count <= 0)
			{
				return;
			}

			int index = dgvSteps.SelectedRows[0].Index;
			if (index >= dgvSteps.Rows.Count - 1)
			{
				return;
			}

			MoveRow(index, index + 1);
		}

		private void MoveRow(int oldIndex, int newIndex)
		{
			if (oldIndex < 0 || oldIndex >= dgvSteps.Rows.Count)
			{
				return;
			}

			if (newIndex < 0 || newIndex >= dgvSteps.Rows.Count)
			{
				return;
			}

			object[] values = new object[dgvSteps.Columns.Count];

			for (int i = 0; i < dgvSteps.Columns.Count; i++)
			{
				values[i] = dgvSteps.Rows[oldIndex].Cells[i].Value;
			}

			dgvSteps.Rows.RemoveAt(oldIndex);
			dgvSteps.Rows.Insert(newIndex, values);

			dgvSteps.ClearSelection();
			dgvSteps.Rows[newIndex].Selected = true;
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			// TODO:
			// 保存到 Project/Config/Flow/TaskSchedulerConfig.xml
			MessageBox.Show("Task scheduler configuration saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion

		#region XML 绑定预留方法

		private void LoadStepDetailFromXml(string jobName, string taskName, string stepName)
		{
			dgvSteps.Rows.Clear();

			// TODO:
			// 后续这里读取 Project/Config/Flow/TaskSchedulerConfig.xml
			// 根据 jobName + taskName + stepName 找到对应的记录并刷新 dgvSteps。
			//
			// 例如：
			// <Job Name="Job_001">
			//   <Task Name="Task_Main">
			//     <Step Name="Step_001" ImageSource="Cam1" RunOrder="1" Remark="" />
			//   </Task>
			// </Job>

			if (string.IsNullOrEmpty(stepName))
			{
				return;
			}

			// 当前先用一行 Demo 数据，证明双击 Task / Step 后右侧会刷新
			dgvSteps.Rows.Add(stepName, "Cam1", "1", "Loaded by selected Job/Task/Step");
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
			return listSteps.SelectedItem == null ? string.Empty : listSteps.SelectedItem.ToString();
		}

		private void UpdateStepDetailTitle()
		{
			string jobName = GetSelectedJobName();
			string taskName = GetSelectedTaskName();

			if (string.IsNullOrEmpty(jobName) && string.IsNullOrEmpty(taskName))
			{
				lblStepsTitle.Text = "当前 step 的详细信息";
				return;
			}

			if (string.IsNullOrEmpty(jobName))
			{
				jobName = "---";
			}

			if (string.IsNullOrEmpty(taskName))
			{
				taskName = "---";
			}

			lblStepsTitle.Text = "当前 Job:  " + jobName + "    Task:  " + taskName;
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

			btnAddJob.Text = "+";
			btnDeleteJob.Text = "×";
			btnAddTask.Text = "+";
			btnDeleteTask.Text = "×";
			btnAddStepItem.Text = "+";
			btnBatchAddStepItem.Text = "▦";
			btnDeleteStepItem.Text = "×";
			btnRefreshStepItem.Text = "↻";
		}
	}
}
