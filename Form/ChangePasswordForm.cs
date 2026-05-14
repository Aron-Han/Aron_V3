using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aron_V3
{
	// 说明：
	// 这个窗体是纯代码创建控件，不需要 ChangePasswordForm.Designer.cs，也不需要 ChangePasswordForm.resx。
	// 你当前报错的原因就是同一个窗体同时存在“手写 InitializeUi”和“Designer 自动生成 InitializeComponent”。
	public class ChangePasswordForm : Form
	{
		private TextBox txtOldPassword;
		private TextBox txtNewPassword;
		private TextBox txtConfirmPassword;
		private Button btnOk;
		private Button btnCancel;

		public ChangePasswordForm()
		{
			InitializeUi();
		}

		private void InitializeUi()
		{
			this.Text = "Change Password";
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(430, 300);
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);

			Label title = CreateLabel("Change Password", 40, 28, 240, 28, 15, true);
			Label lblOld = CreateLabel("Old Password", 45, 90, 120, 24, 9, false);
			Label lblNew = CreateLabel("New Password", 45, 132, 120, 24, 9, false);
			Label lblConfirm = CreateLabel("Confirm", 45, 174, 120, 24, 9, false);

			txtOldPassword = CreateTextBox(170, 88);
			txtNewPassword = CreateTextBox(170, 130);
			txtConfirmPassword = CreateTextBox(170, 172);

			btnOk = CreateButton("OK", 170, 230, true);
			btnCancel = CreateButton("Cancel", 290, 230, false);

			btnOk.Click += btnOk_Click;
			btnCancel.Click += btnCancel_Click;

			this.Controls.Add(title);
			this.Controls.Add(lblOld);
			this.Controls.Add(lblNew);
			this.Controls.Add(lblConfirm);
			this.Controls.Add(txtOldPassword);
			this.Controls.Add(txtNewPassword);
			this.Controls.Add(txtConfirmPassword);
			this.Controls.Add(btnOk);
			this.Controls.Add(btnCancel);

			this.AcceptButton = btnOk;
			this.CancelButton = btnCancel;
		}

		private Label CreateLabel(string text, int x, int y, int w, int h, int fontSize, bool bold)
		{
			Label label = new Label();
			label.Text = text;
			label.ForeColor = Color.White;
			label.Location = new Point(x, y);
			label.Size = new Size(w, h);
			label.Font = new Font("Microsoft YaHei UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular);
			return label;
		}

		private TextBox CreateTextBox(int x, int y)
		{
			TextBox txt = new TextBox();
			txt.Location = new Point(x, y);
			txt.Size = new Size(210, 24);
			txt.BackColor = Color.FromArgb(3, 14, 27);
			txt.ForeColor = Color.White;
			txt.BorderStyle = BorderStyle.FixedSingle;
			txt.PasswordChar = '*';
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
			return btn;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (!LoginSession.IsLoggedIn)
			{
				MessageBox.Show("Please login first.", "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
			{
				MessageBox.Show("New password cannot be empty.", "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (txtNewPassword.Text != txtConfirmPassword.Text)
			{
				MessageBox.Show("The two new passwords are different.", "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			string error;

			if (!UserAccountStore.ChangePassword(LoginSession.CurrentUser.UserName, txtOldPassword.Text, txtNewPassword.Text, out error))
			{
				MessageBox.Show(error, "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			MessageBox.Show("Password changed successfully.", "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
