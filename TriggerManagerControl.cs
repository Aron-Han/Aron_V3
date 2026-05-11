using System;
using System.Windows.Forms;

namespace Aron_V3
{
	public partial class TriggerManagerControl : UserControl, ILocalizable
	{
		public TriggerManagerControl()
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

			if (listJobs.Items.Count > 0)
			{
				listJobs.SelectedIndex = 0;
			}
		}

		private void btnAddTask_Click(object sender, EventArgs e)
		{
			dgvTrigger.Rows.Add("Task_Main", "Trigger_0", "PLC.Input[0]", "0", "1", "");
		}

		private void btnDeleteSelected_Click(object sender, EventArgs e)
		{
			for (int i = dgvTrigger.SelectedRows.Count - 1; i >= 0; i--)
			{
				DataGridViewRow row = dgvTrigger.SelectedRows[i];
				if (!row.IsNewRow)
				{
					dgvTrigger.Rows.Remove(row);
				}
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			// TODO:
			// 保存到 Project/Config/Flow/TriggerConfig.xml
			MessageBox.Show("Trigger configuration saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public void ApplyLanguage(bool isEnglish)
		{
			if (isEnglish)
			{
				lblJobsTitle.Text = "All JobID";
				lblTriggerTitle.Text = "Trigger Source Settings";

				colTaskName.HeaderText = "Task Name";
				colTriggerName.HeaderText = "Trigger Name";
				colInputAddress.HeaderText = "Input Address";
				colFlagBit.HeaderText = "Flag Bit";
				colFlagValue.HeaderText = "Flag Value";
				colRemark.HeaderText = "Remark";

				btnAddTask.Text = "+  Add Task";
				btnDeleteSelected.Text = "▦  Delete";
				btnSave.Text = "▣  Save";
			}
			else
			{
				lblJobsTitle.Text = "所有 JobID";
				lblTriggerTitle.Text = "触发源设置";

				colTaskName.HeaderText = "task名称";
				colTriggerName.HeaderText = "触发源名称";
				colInputAddress.HeaderText = "输入地址";
				colFlagBit.HeaderText = "标志位";
				colFlagValue.HeaderText = "标志位值";
				colRemark.HeaderText = "备注";

				btnAddTask.Text = "+  新增 task";
				btnDeleteSelected.Text = "▦  删除选中";
				btnSave.Text = "▣  保存";
			}
		}
	}
}
