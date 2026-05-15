namespace Aron_V3
{
	partial class TriggerManagerControl
	{
		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.TableLayoutPanel rootLayout;
		private System.Windows.Forms.Panel panelJobs;
		private System.Windows.Forms.Panel panelTrigger;

		private System.Windows.Forms.Label lblJobsTitle;
		private System.Windows.Forms.ListBox listJobs;

		private System.Windows.Forms.Label lblTriggerTitle;
		private System.Windows.Forms.DataGridView dgvTrigger;

		private System.Windows.Forms.TableLayoutPanel panelButtons;
		private System.Windows.Forms.Button btnAddTask;
		private System.Windows.Forms.Button btnDeleteSelected;
		private System.Windows.Forms.Button btnSave;

		private System.Windows.Forms.DataGridViewTextBoxColumn colTaskName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTriggerName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colInputAddress;
		private System.Windows.Forms.DataGridViewTextBoxColumn colFlagBit;
		private System.Windows.Forms.DataGridViewTextBoxColumn colFlagValue;
		private System.Windows.Forms.DataGridViewTextBoxColumn colRemark;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();

			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
			this.panelJobs = new System.Windows.Forms.Panel();
			this.listJobs = new System.Windows.Forms.ListBox();
			this.lblJobsTitle = new System.Windows.Forms.Label();
			this.panelTrigger = new System.Windows.Forms.Panel();
			this.dgvTrigger = new System.Windows.Forms.DataGridView();
			this.colTaskName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTriggerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colInputAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colFlagBit = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colFlagValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panelButtons = new System.Windows.Forms.TableLayoutPanel();
			this.btnAddTask = new System.Windows.Forms.Button();
			this.btnDeleteSelected = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.lblTriggerTitle = new System.Windows.Forms.Label();
			this.rootLayout.SuspendLayout();
			this.panelJobs.SuspendLayout();
			this.panelTrigger.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvTrigger)).BeginInit();
			this.panelButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// rootLayout
			// 
			this.rootLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.rootLayout.ColumnCount = 2;
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
			this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Controls.Add(this.panelJobs, 0, 0);
			this.rootLayout.Controls.Add(this.panelTrigger, 1, 0);
			this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rootLayout.Location = new System.Drawing.Point(0, 0);
			this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
			this.rootLayout.Name = "rootLayout";
			this.rootLayout.RowCount = 1;
			this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.rootLayout.Size = new System.Drawing.Size(1200, 700);
			this.rootLayout.TabIndex = 0;
			// 
			// panelJobs
			// 
			this.panelJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelJobs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelJobs.Controls.Add(this.listJobs);
			this.panelJobs.Controls.Add(this.lblJobsTitle);
			this.panelJobs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelJobs.Location = new System.Drawing.Point(0, 0);
			this.panelJobs.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
			this.panelJobs.Name = "panelJobs";
			this.panelJobs.Padding = new System.Windows.Forms.Padding(18, 16, 18, 18);
			this.panelJobs.Size = new System.Drawing.Size(246, 700);
			this.panelJobs.TabIndex = 0;
			// 
			// listJobs
			// 
			this.listJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.listJobs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listJobs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listJobs.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
			this.listJobs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			this.listJobs.ItemHeight = 25;
			this.listJobs.Location = new System.Drawing.Point(18, 52);
			this.listJobs.Name = "listJobs";
			this.listJobs.Size = new System.Drawing.Size(208, 628);
			this.listJobs.TabIndex = 0;
			// 
			// lblJobsTitle
			// 
			this.lblJobsTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblJobsTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblJobsTitle.ForeColor = System.Drawing.Color.White;
			this.lblJobsTitle.Location = new System.Drawing.Point(18, 16);
			this.lblJobsTitle.Name = "lblJobsTitle";
			this.lblJobsTitle.Size = new System.Drawing.Size(208, 36);
			this.lblJobsTitle.TabIndex = 2;
			this.lblJobsTitle.Text = "所有 JobID";
			this.lblJobsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelTrigger
			// 
			this.panelTrigger.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.panelTrigger.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTrigger.Controls.Add(this.dgvTrigger);
			this.panelTrigger.Controls.Add(this.panelButtons);
			this.panelTrigger.Controls.Add(this.lblTriggerTitle);
			this.panelTrigger.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTrigger.Location = new System.Drawing.Point(260, 0);
			this.panelTrigger.Margin = new System.Windows.Forms.Padding(0);
			this.panelTrigger.Name = "panelTrigger";
			this.panelTrigger.Padding = new System.Windows.Forms.Padding(18, 16, 18, 18);
			this.panelTrigger.Size = new System.Drawing.Size(940, 700);
			this.panelTrigger.TabIndex = 1;
			// 
			// dgvTrigger
			// 
			this.dgvTrigger.AllowUserToAddRows = false;
			this.dgvTrigger.AllowUserToDeleteRows = false;
			this.dgvTrigger.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvTrigger.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(29)))), ((int)(((byte)(50)))));
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(29)))), ((int)(((byte)(50)))));
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
			this.dgvTrigger.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dgvTrigger.ColumnHeadersHeight = 42;
			this.dgvTrigger.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colTaskName,
			this.colTriggerName,
			this.colInputAddress,
			this.colFlagBit,
			this.colFlagValue,
			this.colRemark});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(125)))), ((int)(((byte)(210)))));
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvTrigger.DefaultCellStyle = dataGridViewCellStyle2;
			this.dgvTrigger.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvTrigger.EnableHeadersVisualStyles = false;
			this.dgvTrigger.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(80)))), ((int)(((byte)(105)))));
			this.dgvTrigger.Location = new System.Drawing.Point(18, 52);
			this.dgvTrigger.Name = "dgvTrigger";
			this.dgvTrigger.RowHeadersVisible = false;
			this.dgvTrigger.RowHeadersWidth = 62;
			this.dgvTrigger.RowTemplate.Height = 30;
			this.dgvTrigger.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvTrigger.Size = new System.Drawing.Size(902, 570);
			this.dgvTrigger.TabIndex = 0;
			// 
			// colTaskName
			// 
			this.colTaskName.HeaderText = "task名称";
			this.colTaskName.MinimumWidth = 8;
			this.colTaskName.Name = "colTaskName";
			// 
			// colTriggerName
			// 
			this.colTriggerName.HeaderText = "触发源名称";
			this.colTriggerName.MinimumWidth = 8;
			this.colTriggerName.Name = "colTriggerName";
			// 
			// colInputAddress
			// 
			this.colInputAddress.HeaderText = "输入地址";
			this.colInputAddress.MinimumWidth = 8;
			this.colInputAddress.Name = "colInputAddress";
			// 
			// colFlagBit
			// 
			this.colFlagBit.HeaderText = "标志位";
			this.colFlagBit.MinimumWidth = 8;
			this.colFlagBit.Name = "colFlagBit";
			// 
			// colFlagValue
			// 
			this.colFlagValue.HeaderText = "标志位值";
			this.colFlagValue.MinimumWidth = 8;
			this.colFlagValue.Name = "colFlagValue";
			// 
			// colRemark
			// 
			this.colRemark.HeaderText = "备注";
			this.colRemark.MinimumWidth = 8;
			this.colRemark.Name = "colRemark";
			// 
			// panelButtons
			// 
			this.panelButtons.ColumnCount = 4;
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.panelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
			this.panelButtons.Controls.Add(this.btnAddTask, 0, 0);
			this.panelButtons.Controls.Add(this.btnDeleteSelected, 1, 0);
			this.panelButtons.Controls.Add(this.btnSave, 3, 0);
			this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelButtons.Location = new System.Drawing.Point(18, 622);
			this.panelButtons.Margin = new System.Windows.Forms.Padding(0);
			this.panelButtons.Name = "panelButtons";
			this.panelButtons.Padding = new System.Windows.Forms.Padding(0, 10, 0, 8);
			this.panelButtons.RowCount = 1;
			this.panelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.panelButtons.Size = new System.Drawing.Size(902, 58);
			this.panelButtons.TabIndex = 1;
			// 
			// btnAddTask
			// 
			this.btnAddTask.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnAddTask.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnAddTask.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnAddTask.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnAddTask.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnAddTask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddTask.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnAddTask.ForeColor = System.Drawing.Color.White;
			this.btnAddTask.Location = new System.Drawing.Point(0, 10);
			this.btnAddTask.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this.btnAddTask.Name = "btnAddTask";
			this.btnAddTask.Size = new System.Drawing.Size(130, 40);
			this.btnAddTask.TabIndex = 0;
			this.btnAddTask.Text = "+  新增 task";
			this.btnAddTask.UseVisualStyleBackColor = false;
			this.btnAddTask.Click += new System.EventHandler(this.btnAddTask_Click);
			// 
			// btnDeleteSelected
			// 
			this.btnDeleteSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(21)))), ((int)(((byte)(39)))));
			this.btnDeleteSelected.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnDeleteSelected.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(75)))), ((int)(((byte)(100)))));
			this.btnDeleteSelected.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(70)))), ((int)(((byte)(135)))));
			this.btnDeleteSelected.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(45)))), ((int)(((byte)(78)))));
			this.btnDeleteSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDeleteSelected.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnDeleteSelected.ForeColor = System.Drawing.Color.White;
			this.btnDeleteSelected.Location = new System.Drawing.Point(140, 10);
			this.btnDeleteSelected.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this.btnDeleteSelected.Name = "btnDeleteSelected";
			this.btnDeleteSelected.Size = new System.Drawing.Size(140, 40);
			this.btnDeleteSelected.TabIndex = 1;
			this.btnDeleteSelected.Text = "▦  删除选中";
			this.btnDeleteSelected.UseVisualStyleBackColor = false;
			this.btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);
			// 
			// btnSave
			// 
			this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(90)))), ((int)(((byte)(215)))));
			this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
			this.btnSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(70)))), ((int)(((byte)(190)))));
			this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(110)))), ((int)(((byte)(245)))));
			this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.btnSave.ForeColor = System.Drawing.Color.White;
			this.btnSave.Location = new System.Drawing.Point(782, 10);
			this.btnSave.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(120, 40);
			this.btnSave.TabIndex = 2;
			this.btnSave.Text = "▣  保存";
			this.btnSave.UseVisualStyleBackColor = false;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// lblTriggerTitle
			// 
			this.lblTriggerTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTriggerTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblTriggerTitle.ForeColor = System.Drawing.Color.White;
			this.lblTriggerTitle.Location = new System.Drawing.Point(18, 16);
			this.lblTriggerTitle.Name = "lblTriggerTitle";
			this.lblTriggerTitle.Size = new System.Drawing.Size(902, 36);
			this.lblTriggerTitle.TabIndex = 2;
			this.lblTriggerTitle.Text = "触发源设置";
			this.lblTriggerTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TriggerManagerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(14)))), ((int)(((byte)(28)))));
			this.Controls.Add(this.rootLayout);
			this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
			this.Name = "TriggerManagerControl";
			this.Size = new System.Drawing.Size(1200, 700);
			this.rootLayout.ResumeLayout(false);
			this.panelJobs.ResumeLayout(false);
			this.panelTrigger.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvTrigger)).EndInit();
			this.panelButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

	}
}
