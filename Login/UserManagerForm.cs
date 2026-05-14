using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Aron_V3
{
	// 说明：
	// 这个窗体是纯代码创建控件，不需要 UserManagerForm.Designer.cs，也不需要 UserManagerForm.resx。
	// 本版修复：底部 Add/Delete/Reset/Save/Close 按钮不显示的问题。
	public class UserManagerForm : Form
	{
		private const int WM_SETREDRAW = 0x000B;

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private Panel panelMain;
		private Panel panelHeader;
		private Panel panelButtonBar;
		private DataGridView dgvUsers;
		private NumericUpDown nudAutoLogout;
		private Button btnAdd;
		private Button btnDelete;
		private Button btnResetPassword;
		private Button btnSave;
		private Button btnClose;

		public UserManagerForm()
		{
			this.Opacity = 0;
			this.SuspendLayout();

			InitializeUi();
			LoadUsers();

			this.ResumeLayout(false);
			this.PerformLayout();

			this.Shown += UserManagerForm_Shown;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
				return cp;
			}
		}

		private void UserManagerForm_Shown(object sender, EventArgs e)
		{
			this.BeginInvoke(new MethodInvoker(delegate
			{
				this.Opacity = 1;
			}));
		}

		private void InitializeUi()
		{
			this.Text = "User Management";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.ClientSize = new Size(1050, 620);
			this.MinimumSize = new Size(900, 520);
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);

			SetDoubleBuffered(this);

			panelMain = new Panel();
			panelMain.Dock = DockStyle.Fill;
			panelMain.Padding = new Padding(24, 18, 24, 16);
			panelMain.BackColor = Color.FromArgb(2, 10, 20);
			SetDoubleBuffered(panelMain);

			panelHeader = new Panel();
			panelHeader.Dock = DockStyle.Top;
			panelHeader.Height = 78;
			panelHeader.BackColor = Color.FromArgb(2, 10, 20);
			SetDoubleBuffered(panelHeader);

			Label title = new Label();
			title.Text = "User Management / Permission Setting";
			title.ForeColor = Color.White;
			title.BackColor = Color.Transparent;
			title.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold);
			title.Location = new Point(0, 2);
			title.Size = new Size(560, 34);
			SetDoubleBuffered(title);

			Label lblAutoLogout = new Label();
			lblAutoLogout.Text = "Auto Logout Minutes";
			lblAutoLogout.ForeColor = Color.White;
			lblAutoLogout.BackColor = Color.Transparent;
			lblAutoLogout.Location = new Point(0, 48);
			lblAutoLogout.Size = new Size(160, 24);
			SetDoubleBuffered(lblAutoLogout);

			nudAutoLogout = new NumericUpDown();
			nudAutoLogout.Location = new Point(170, 46);
			nudAutoLogout.Minimum = 1;
			nudAutoLogout.Maximum = 1440;
			nudAutoLogout.Value = 30;
			nudAutoLogout.BackColor = Color.FromArgb(3, 14, 27);
			nudAutoLogout.ForeColor = Color.White;
			nudAutoLogout.Size = new Size(100, 24);

			panelHeader.Controls.Add(title);
			panelHeader.Controls.Add(lblAutoLogout);
			panelHeader.Controls.Add(nudAutoLogout);

			panelButtonBar = new Panel();
			panelButtonBar.Dock = DockStyle.Bottom;
			panelButtonBar.Height = 58;
			panelButtonBar.BackColor = Color.FromArgb(2, 10, 20);
			SetDoubleBuffered(panelButtonBar);

			btnAdd = CreateButton("+ Add User", 0, 12, 115, false);
			btnDelete = CreateButton("Delete", 125, 12, 115, false);
			btnResetPassword = CreateButton("Reset Password", 250, 12, 135, false);
			btnSave = CreateButton("Save", 0, 12, 115, true);
			btnClose = CreateButton("Close", 125, 12, 115, false);

			btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;

			// 右侧按钮位置必须在 panelButtonBar 加入窗体后才能根据宽度计算。
			panelButtonBar.Resize += panelButtonBar_Resize;

			btnAdd.Click += btnAdd_Click;
			btnDelete.Click += btnDelete_Click;
			btnResetPassword.Click += btnResetPassword_Click;
			btnSave.Click += btnSave_Click;
			btnClose.Click += delegate { this.Close(); };

			panelButtonBar.Controls.Add(btnAdd);
			panelButtonBar.Controls.Add(btnDelete);
			panelButtonBar.Controls.Add(btnResetPassword);
			panelButtonBar.Controls.Add(btnSave);
			panelButtonBar.Controls.Add(btnClose);

			dgvUsers = new DataGridView();
			dgvUsers.Dock = DockStyle.Fill;
			dgvUsers.Margin = new Padding(0);
			dgvUsers.AllowUserToAddRows = false;
			dgvUsers.AllowUserToDeleteRows = false;
			dgvUsers.RowHeadersVisible = false;
			dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvUsers.MultiSelect = false;
			dgvUsers.BackgroundColor = Color.FromArgb(2, 10, 20);
			dgvUsers.GridColor = Color.FromArgb(45, 70, 95);
			dgvUsers.BorderStyle = BorderStyle.None;
			dgvUsers.EnableHeadersVisualStyles = false;
			dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
			dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			dgvUsers.ColumnHeadersHeight = 32;
			dgvUsers.RowTemplate.Height = 28;
			dgvUsers.ScrollBars = ScrollBars.Both;
			dgvUsers.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

			dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(8, 28, 48);
			dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);

			dgvUsers.DefaultCellStyle.BackColor = Color.FromArgb(2, 10, 20);
			dgvUsers.DefaultCellStyle.ForeColor = Color.White;
			dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 200);
			dgvUsers.DefaultCellStyle.SelectionForeColor = Color.White;
			dgvUsers.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

			SetDoubleBuffered(dgvUsers);

			dgvUsers.DataError -= dgvUsers_DataError;
			dgvUsers.DataError += dgvUsers_DataError;

			dgvUsers.Columns.Add(CreateTextColumn("UserName", "User Name", 110, true));
			dgvUsers.Columns.Add(CreateTextColumn("DisplayName", "Display Name", 130, false));
			dgvUsers.Columns.Add(CreateTextColumn("Role", "Role", 100, false));
			dgvUsers.Columns.Add(CreateCheckColumn("Enabled", "Enabled", 80));
			dgvUsers.Columns.Add(CreateCheckColumn("CanRun", "Run", 70));
			dgvUsers.Columns.Add(CreateCheckColumn("CanHardwareConfig", "Hardware", 90));
			dgvUsers.Columns.Add(CreateCheckColumn("CanAlgorithmConfig", "Algorithm", 90));
			dgvUsers.Columns.Add(CreateCheckColumn("CanFlowConfig", "Flow", 80));
			dgvUsers.Columns.Add(CreateCheckColumn("CanCommunicationConfig", "Comm", 80));
			dgvUsers.Columns.Add(CreateCheckColumn("CanDatabaseConfig", "Database", 90));
			dgvUsers.Columns.Add(CreateCheckColumn("CanSystemConfig", "System", 80));
			dgvUsers.Columns.Add(CreateCheckColumn("CanUserManagement", "Users", 80));

			// Dock 顺序很重要：
			// 先加入 Fill 的 dgv，再加入 Bottom 的按钮栏和 Top 的标题栏，避免按钮被 DataGridView 覆盖。
			panelMain.Controls.Add(dgvUsers);
			panelMain.Controls.Add(panelButtonBar);
			panelMain.Controls.Add(panelHeader);

			this.Controls.Add(panelMain);

			RepositionRightButtons();
		}

		private void panelButtonBar_Resize(object sender, EventArgs e)
		{
			RepositionRightButtons();
		}

		private void RepositionRightButtons()
		{
			if (panelButtonBar == null || btnSave == null || btnClose == null)
			{
				return;
			}

			int marginRight = 0;
			int gap = 12;

			btnClose.Left = panelButtonBar.ClientSize.Width - btnClose.Width - marginRight;
			btnSave.Left = btnClose.Left - btnSave.Width - gap;

			if (btnSave.Left < 400)
			{
				btnSave.Left = 400;
				btnClose.Left = btnSave.Right + gap;
			}
		}

		private DataGridViewTextBoxColumn CreateTextColumn(string name, string header, int width, bool readOnly)
		{
			DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
			col.Name = name;
			col.HeaderText = header;
			col.Width = width;
			col.ReadOnly = readOnly;
			col.SortMode = DataGridViewColumnSortMode.NotSortable;
			return col;
		}

		private DataGridViewCheckBoxColumn CreateCheckColumn(string name, string header, int width)
		{
			DataGridViewCheckBoxColumn col = new DataGridViewCheckBoxColumn();
			col.Name = name;
			col.HeaderText = header;
			col.Width = width;
			col.SortMode = DataGridViewColumnSortMode.NotSortable;
			return col;
		}

		private Button CreateButton(string text, int x, int y, int width, bool primary)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(width, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.ForeColor = Color.White;
			btn.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			btn.UseVisualStyleBackColor = false;
			SetDoubleBuffered(btn);
			return btn;
		}

		private void LoadUsers()
		{
			BeginPageUpdate();

			try
			{
				UserAccountConfig config = UserAccountStore.LoadOrCreateDefault();
				nudAutoLogout.Value = config.AutoLogoutMinutes;

				dgvUsers.Rows.Clear();

				foreach (UserAccount user in config.Users)
				{
					if (user.Permission == null)
					{
						user.Permission = UserPermission.CreateOperatorPermission();
					}

					dgvUsers.Rows.Add(
						user.UserName,
						user.DisplayName,
						user.Role,
						user.Enabled,
						user.Permission.CanRun,
						user.Permission.CanHardwareConfig,
						user.Permission.CanAlgorithmConfig,
						user.Permission.CanFlowConfig,
						user.Permission.CanCommunicationConfig,
						user.Permission.CanDatabaseConfig,
						user.Permission.CanSystemConfig,
						user.Permission.CanUserManagement);
				}

				dgvUsers.ClearSelection();
			}
			finally
			{
				EndPageUpdate();
			}
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			RegisterUserForm form = new RegisterUserForm();

			if (form.ShowDialog(this) == DialogResult.OK)
			{
				LoadUsers();
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (dgvUsers.SelectedRows.Count <= 0)
			{
				return;
			}

			string userName = Convert.ToString(dgvUsers.SelectedRows[0].Cells["UserName"].Value);

			if (string.Equals(userName, "admin", StringComparison.OrdinalIgnoreCase))
			{
				MessageBox.Show("Default admin cannot be deleted.", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (MessageBox.Show("Delete selected user?", "User Management", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			BeginPageUpdate();

			try
			{
				UserAccountConfig config = UserAccountStore.LoadOrCreateDefault();
				UserAccount user = config.Users.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

				if (user != null)
				{
					config.Users.Remove(user);
					UserAccountStore.Save(config);
				}
			}
			finally
			{
				EndPageUpdate();
			}

			LoadUsers();
		}

		private void btnResetPassword_Click(object sender, EventArgs e)
		{
			if (dgvUsers.SelectedRows.Count <= 0)
			{
				return;
			}

			string userName = Convert.ToString(dgvUsers.SelectedRows[0].Cells["UserName"].Value);
			string error;

			if (UserAccountStore.ResetPasswordByAdmin(userName, "123456", out error))
			{
				MessageBox.Show("Password has been reset to 123456.", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				MessageBox.Show(error, "User Management", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			BeginPageUpdate();

			try
			{
				if (dgvUsers.IsCurrentCellDirty)
				{
					dgvUsers.CommitEdit(DataGridViewDataErrorContexts.Commit);
				}

				dgvUsers.EndEdit();

				UserAccountConfig config = UserAccountStore.LoadOrCreateDefault();
				config.AutoLogoutMinutes = Convert.ToInt32(nudAutoLogout.Value);

				foreach (DataGridViewRow row in dgvUsers.Rows)
				{
					string userName = Convert.ToString(row.Cells["UserName"].Value);

					UserAccount user = config.Users.FirstOrDefault(u =>
						string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

					if (user == null)
					{
						continue;
					}

					user.DisplayName = Convert.ToString(row.Cells["DisplayName"].Value);
					user.Role = Convert.ToString(row.Cells["Role"].Value);
					user.Enabled = ToBool(row.Cells["Enabled"].Value);

					if (user.Permission == null)
					{
						user.Permission = new UserPermission();
					}

					user.Permission.CanRun = ToBool(row.Cells["CanRun"].Value);
					user.Permission.CanHardwareConfig = ToBool(row.Cells["CanHardwareConfig"].Value);
					user.Permission.CanAlgorithmConfig = ToBool(row.Cells["CanAlgorithmConfig"].Value);
					user.Permission.CanFlowConfig = ToBool(row.Cells["CanFlowConfig"].Value);
					user.Permission.CanCommunicationConfig = ToBool(row.Cells["CanCommunicationConfig"].Value);
					user.Permission.CanDatabaseConfig = ToBool(row.Cells["CanDatabaseConfig"].Value);
					user.Permission.CanSystemConfig = ToBool(row.Cells["CanSystemConfig"].Value);
					user.Permission.CanUserManagement = ToBool(row.Cells["CanUserManagement"].Value);
				}

				UserAccountStore.Save(config);
			}
			finally
			{
				EndPageUpdate();
			}

			MessageBox.Show("User configuration saved.", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private bool ToBool(object value)
		{
			if (value == null)
			{
				return false;
			}

			bool result;

			if (bool.TryParse(value.ToString(), out result))
			{
				return result;
			}

			return false;
		}

		private void dgvUsers_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private void BeginPageUpdate()
		{
			if (panelMain == null || dgvUsers == null)
			{
				return;
			}

			this.SuspendLayout();
			panelMain.SuspendLayout();
			dgvUsers.SuspendLayout();

			BeginUpdateControl(this);
			BeginUpdateControl(panelMain);
			BeginUpdateControl(dgvUsers);
		}

		private void EndPageUpdate()
		{
			if (panelMain == null || dgvUsers == null)
			{
				return;
			}

			EndUpdateControl(dgvUsers);
			EndUpdateControl(panelMain);
			EndUpdateControl(this);

			dgvUsers.ResumeLayout();
			panelMain.ResumeLayout();
			this.ResumeLayout();

			dgvUsers.Invalidate();
			panelMain.Invalidate();
			this.Invalidate();
		}

		private void BeginUpdateControl(Control control)
		{
			if (control == null || control.IsDisposed)
			{
				return;
			}

			if (control.IsHandleCreated)
			{
				SendMessage(control.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
			}
		}

		private void EndUpdateControl(Control control)
		{
			if (control == null || control.IsDisposed)
			{
				return;
			}

			if (control.IsHandleCreated)
			{
				SendMessage(control.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
			}
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
	}

	public class RegisterUserForm : Form
	{
		private TextBox txtUserName;
		private TextBox txtDisplayName;
		private TextBox txtPassword;
		private ComboBox cmbRole;
		private Button btnOk;
		private Button btnCancel;
		private Panel panelMain;

		public RegisterUserForm()
		{
			this.Opacity = 0;
			this.SuspendLayout();

			InitializeUi();

			this.ResumeLayout(false);
			this.PerformLayout();

			this.Shown += RegisterUserForm_Shown;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;
				return cp;
			}
		}

		private void RegisterUserForm_Shown(object sender, EventArgs e)
		{
			this.BeginInvoke(new MethodInvoker(delegate
			{
				this.Opacity = 1;
				txtUserName.Focus();
			}));
		}

		private void InitializeUi()
		{
			this.Text = "Register User";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.ClientSize = new Size(430, 320);
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);
			this.MaximizeBox = false;
			this.MinimizeBox = false;

			SetDoubleBuffered(this);

			panelMain = new Panel();
			panelMain.Dock = DockStyle.Fill;
			panelMain.BackColor = Color.FromArgb(2, 10, 20);
			SetDoubleBuffered(panelMain);
			panelMain.SuspendLayout();

			AddLabel("User Name", 45, 55);
			AddLabel("Display Name", 45, 100);
			AddLabel("Password", 45, 145);
			AddLabel("Role", 45, 190);

			txtUserName = AddTextBox(165, 52);
			txtDisplayName = AddTextBox(165, 97);
			txtPassword = AddTextBox(165, 142);
			txtPassword.PasswordChar = '*';

			cmbRole = new ComboBox();
			cmbRole.Location = new Point(165, 187);
			cmbRole.Size = new Size(210, 24);
			cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbRole.BackColor = Color.FromArgb(3, 14, 27);
			cmbRole.ForeColor = Color.White;
			cmbRole.Items.Add("Operator");
			cmbRole.Items.Add("Engineer");
			cmbRole.Items.Add("Admin");
			cmbRole.SelectedIndex = 0;

			btnOk = CreateButton("OK", 165, 245, true);
			btnCancel = CreateButton("Cancel", 285, 245, false);

			btnOk.Click += btnOk_Click;
			btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };

			panelMain.Controls.Add(cmbRole);
			panelMain.Controls.Add(btnOk);
			panelMain.Controls.Add(btnCancel);

			panelMain.ResumeLayout(false);
			this.Controls.Add(panelMain);

			this.AcceptButton = btnOk;
			this.CancelButton = btnCancel;
		}

		private void AddLabel(string text, int x, int y)
		{
			Label label = new Label();
			label.Text = text;
			label.ForeColor = Color.White;
			label.BackColor = Color.Transparent;
			label.Location = new Point(x, y);
			label.Size = new Size(110, 24);
			SetDoubleBuffered(label);
			panelMain.Controls.Add(label);
		}

		private TextBox AddTextBox(int x, int y)
		{
			TextBox txt = new TextBox();
			txt.Location = new Point(x, y);
			txt.Size = new Size(210, 24);
			txt.BackColor = Color.FromArgb(3, 14, 27);
			txt.ForeColor = Color.White;
			txt.BorderStyle = BorderStyle.FixedSingle;
			panelMain.Controls.Add(txt);
			return txt;
		}

		private Button CreateButton(string text, int x, int y, bool primary)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(90, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.ForeColor = Color.White;
			btn.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			btn.UseVisualStyleBackColor = false;
			SetDoubleBuffered(btn);
			return btn;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			UserPermission permission;

			if (cmbRole.Text == "Admin")
			{
				permission = UserPermission.CreateAdminPermission();
			}
			else if (cmbRole.Text == "Engineer")
			{
				permission = UserPermission.CreateEngineerPermission();
			}
			else
			{
				permission = UserPermission.CreateOperatorPermission();
			}

			string error;

			if (!UserAccountStore.AddUser(txtUserName.Text, txtDisplayName.Text, txtPassword.Text, cmbRole.Text, permission, out error))
			{
				MessageBox.Show(error, "Register User", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			MessageBox.Show("User registered successfully.", "Register User", MessageBoxButtons.OK, MessageBoxIcon.Information);
			this.DialogResult = DialogResult.OK;
			this.Close();
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
	}
}
