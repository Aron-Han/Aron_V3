using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Aron_V3
{
	// 说明：
	// 这个窗体是纯代码创建控件，不需要 LoginForm.Designer.cs，也不需要 LoginForm.resx。
	public class LoginForm : Form
	{
		private ComboBox cmbUser;
		private TextBox txtPassword;
		private Button btnLogin;
		private Button btnCancel;
		private Panel panelMain;
		private readonly bool _isEnglish;

		public UserAccount LoginUser { get; private set; }

		public LoginForm()
		{
			_isEnglish = LanguagePreferenceStore.LoadIsEnglish();

			// 先隐藏窗体，等控件全部创建完成后再显示，减少打开时闪烁。
			this.Opacity = 0;
			this.SuspendLayout();

			InitializeUi();
			LoadUserList();

			this.ResumeLayout(false);
			this.PerformLayout();

			this.Shown += LoginForm_Shown;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED，减少窗体打开和控件绘制闪烁
				return cp;
			}
		}

		private void LoginForm_Shown(object sender, EventArgs e)
		{
			this.BeginInvoke(new MethodInvoker(delegate
			{
				this.Opacity = 1;
				txtPassword.Focus();
			}));
		}

		private void InitializeUi()
		{
			this.Text = T("用户登录", "User Login");
			this.StartPosition = FormStartPosition.CenterParent;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(420, 280);
			this.BackColor = Color.FromArgb(2, 10, 20);
			this.Font = new Font("Microsoft YaHei UI", 9F);
			this.Activated += LoginForm_Activated;

			SetDoubleBuffered(this);

			panelMain = new Panel();
			panelMain.Dock = DockStyle.Fill;
			panelMain.BackColor = Color.FromArgb(2, 10, 20);
			panelMain.SuspendLayout();
			SetDoubleBuffered(panelMain);

			Label title = CreateLabel(T("用户登录", "User Login"), 40, 30, 240, 34, 16, true);
			Label lblUser = CreateLabel(T("用户名", "User Name"), 45, 92, 100, 24, 9, false);
			Label lblPassword = CreateLabel(T("密码", "Password"), 45, 136, 100, 24, 9, false);

			cmbUser = new ComboBox();
			cmbUser.Location = new Point(150, 90);
			cmbUser.Size = new Size(220, 24);
			cmbUser.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbUser.BackColor = Color.FromArgb(3, 14, 27);
			cmbUser.ForeColor = Color.White;
			SetDoubleBuffered(cmbUser);

			txtPassword = CreateTextBox(150, 134, true);

			btnLogin = CreateButton(T("登录", "Login"), 150, 195, true);
			btnCancel = CreateButton(T("取消", "Cancel"), 270, 195, false);

			btnLogin.Click += btnLogin_Click;
			btnCancel.Click += btnCancel_Click;

			panelMain.Controls.Add(title);
			panelMain.Controls.Add(lblUser);
			panelMain.Controls.Add(cmbUser);
			panelMain.Controls.Add(lblPassword);
			panelMain.Controls.Add(txtPassword);
			panelMain.Controls.Add(btnLogin);
			panelMain.Controls.Add(btnCancel);

			panelMain.ResumeLayout(false);
			this.Controls.Add(panelMain);

			this.AcceptButton = btnLogin;
			this.CancelButton = btnCancel;
		}

		private void LoginForm_Activated(object sender, EventArgs e)
		{
			LoadUserList();
		}

		private void LoadUserList()
		{
			if (cmbUser == null)
			{
				return;
			}

			string oldSelected = cmbUser.SelectedItem == null ? string.Empty : cmbUser.SelectedItem.ToString();

			UserAccountConfig config = UserAccountStore.LoadOrCreateDefault();

			List<string> userNames = config.Users
				.Where(u => u.Enabled)
				.Select(u => u.UserName)
				.OrderBy(u => u)
				.ToList();

			cmbUser.BeginUpdate();

			try
			{
				cmbUser.Items.Clear();

				foreach (string userName in userNames)
				{
					cmbUser.Items.Add(userName);
				}

				if (!string.IsNullOrEmpty(oldSelected) && cmbUser.Items.Contains(oldSelected))
				{
					cmbUser.SelectedItem = oldSelected;
				}
				else if (cmbUser.Items.Contains("admin"))
				{
					cmbUser.SelectedItem = "admin";
				}
				else if (cmbUser.Items.Count > 0)
				{
					cmbUser.SelectedIndex = 0;
				}
			}
			finally
			{
				cmbUser.EndUpdate();
			}
		}

		private Label CreateLabel(string text, int x, int y, int w, int h, int fontSize, bool bold)
		{
			Label label = new Label();
			label.Text = text;
			label.ForeColor = Color.White;
			label.BackColor = Color.Transparent;
			label.Location = new Point(x, y);
			label.Size = new Size(w, h);
			label.Font = new Font("Microsoft YaHei UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular);
			SetDoubleBuffered(label);
			return label;
		}

		private TextBox CreateTextBox(int x, int y, bool password)
		{
			TextBox txt = new TextBox();
			txt.Location = new Point(x, y);
			txt.Size = new Size(220, 24);
			txt.BackColor = Color.FromArgb(3, 14, 27);
			txt.ForeColor = Color.White;
			txt.BorderStyle = BorderStyle.FixedSingle;

			if (password)
			{
				txt.PasswordChar = '*';
			}

			return txt;
		}

		private Button CreateButton(string text, int x, int y, bool primary)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.Location = new Point(x, y);
			btn.Size = new Size(100, 34);
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 220);
			btn.ForeColor = Color.White;
			btn.BackColor = primary ? Color.FromArgb(0, 95, 220) : Color.FromArgb(3, 14, 27);
			btn.UseVisualStyleBackColor = false;
			SetDoubleBuffered(btn);
			return btn;
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

		private void btnLogin_Click(object sender, EventArgs e)
		{
			if (cmbUser.SelectedItem == null)
			{
				MessageBox.Show(
					T("请选择用户。", "Please select user."),
					T("登录", "Login"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			string userName = cmbUser.SelectedItem.ToString();

			UserAccount user = UserAccountStore.Authenticate(userName, txtPassword.Text);

			if (user == null)
			{
				MessageBox.Show(
					T("用户名或密码错误。", "Invalid user name or password."),
					T("登录", "Login"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			LoginUser = user;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private string T(string chinese, string english)
		{
			return _isEnglish ? english : chinese;
		}
	}
}
