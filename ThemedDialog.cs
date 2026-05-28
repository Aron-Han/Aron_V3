using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Aron_V3
{
	internal enum ThemedDialogIconKind
	{
		Info,
		Warning,
		Delete,
		Success
	}

	internal sealed class ThemedDialog : Form
	{
		private readonly string _title;
		private readonly string _mainText;
		private readonly string _detailText;
		private readonly string _footerText;
		private readonly string _primaryText;
		private readonly string _cancelText;
		private readonly ThemedDialogIconKind _iconKind;
		private readonly bool _primaryDanger;

		private Label _lblTitle;
		private Label _lblMain;
		private Label _lblDetail;
		private Label _lblFooter;
		private Button _btnClose;
		private Button _btnPrimary;
		private Button _btnCancel;
		private DialogIconView _iconView;

		public ThemedDialog(
			string title,
			string mainText,
			string detailText,
			string footerText,
			string primaryText,
			string cancelText,
			ThemedDialogIconKind iconKind,
			bool primaryDanger)
		{
			_title = title ?? string.Empty;
			_mainText = mainText ?? string.Empty;
			_detailText = detailText ?? string.Empty;
			_footerText = footerText ?? string.Empty;
			_primaryText = primaryText ?? string.Empty;
			_cancelText = cancelText ?? string.Empty;
			_iconKind = iconKind;
			_primaryDanger = primaryDanger;

			InitializeDialog();
		}

		public static bool ConfirmDeleteCommunication(IWin32Window owner, string instanceName, bool isEnglish)
		{
			string safeName = string.IsNullOrWhiteSpace(instanceName) ? string.Empty : instanceName.Trim();
			return Confirm(
				owner,
				isEnglish ? "Delete Communication" : "\u5220\u9664\u901a\u8baf",
				isEnglish
					? "Delete communication \"" + safeName + "\"?"
					: "\u5220\u9664\u901a\u8baf \"" + safeName + "\" \u5417\uff1f",
				isEnglish
					? "After deletion, its parameters, input/output variables, channels and heartbeat settings will be removed from the configuration."
					: "\u5220\u9664\u540e\uff0c\u8be5\u901a\u8baf\u7684\u53c2\u6570\u3001\u8f93\u5165\u8f93\u51fa\u53d8\u91cf\u3001\u901a\u9053\u548c\u5fc3\u8df3\u8bbe\u7f6e\u5c06\u4ece\u914d\u7f6e\u4e2d\u79fb\u9664\u3002",
				isEnglish
					? "This only affects the selected communication instance."
					: "\u6b64\u64cd\u4f5c\u4ec5\u5f71\u54cd\u5f53\u524d\u901a\u8baf\u5b9e\u4f8b\u914d\u7f6e\u3002",
				isEnglish ? "Delete" : "\u5220\u9664",
				isEnglish ? "Cancel" : "\u53d6\u6d88",
				ThemedDialogIconKind.Delete,
				true);
		}

		public static bool Confirm(
			IWin32Window owner,
			string title,
			string mainText,
			string detailText,
			string footerText,
			string primaryText,
			string cancelText,
			ThemedDialogIconKind iconKind,
			bool primaryDanger)
		{
			using (ThemedDialog dialog = new ThemedDialog(
				title,
				mainText,
				detailText,
				footerText,
				primaryText,
				cancelText,
				iconKind,
				primaryDanger))
			{
				return dialog.ShowDialog(owner) == DialogResult.OK;
			}
		}

		public static void ShowInformation(IWin32Window owner, string title, string message, bool isEnglish)
		{
			ShowMessage(owner, title, message, ThemedDialogIconKind.Info, isEnglish);
		}

		public static void ShowWarning(IWin32Window owner, string title, string message, bool isEnglish)
		{
			ShowMessage(owner, title, message, ThemedDialogIconKind.Warning, isEnglish);
		}

		public static void ShowError(IWin32Window owner, string title, string message, bool isEnglish)
		{
			ShowMessage(owner, title, message, ThemedDialogIconKind.Warning, isEnglish);
		}

		private static void ShowMessage(
			IWin32Window owner,
			string title,
			string message,
			ThemedDialogIconKind iconKind,
			bool isEnglish)
		{
			using (ThemedDialog dialog = new ThemedDialog(
				title,
				message,
				string.Empty,
				string.Empty,
				isEnglish ? "OK" : "\u786e\u5b9a",
				string.Empty,
				iconKind,
				false))
			{
				dialog.ShowDialog(owner);
			}
		}

		private void InitializeDialog()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			DoubleBuffered = true;
			AutoScaleMode = AutoScaleMode.None;
			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			MaximizeBox = false;
			Size = new Size(560, 300);
			BackColor = Color.FromArgb(6, 20, 34);
			ForeColor = Color.White;
			Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
			Padding = new Padding(1);

			bool hasDetail = !string.IsNullOrWhiteSpace(_detailText);
			_lblTitle = CreateLabel(_title, 24, 14, 360, 32, 14F, FontStyle.Bold, Color.White);
			_lblMain = CreateLabel(_mainText, 130, hasDetail ? 92 : 104, 380, hasDetail ? 34 : 76, 15F, FontStyle.Bold, Color.White);
			_lblDetail = CreateLabel(_detailText, 130, 132, 380, 58, 9.5F, FontStyle.Regular, Color.FromArgb(175, 196, 213));
			_lblDetail.Visible = hasDetail;
			_lblFooter = CreateLabel(_footerText, 40, 220, 280, 30, 9F, FontStyle.Regular, Color.FromArgb(111, 140, 163));

			_btnClose = new Button();
			_btnClose.Text = string.Empty;
			_btnClose.SetBounds(510, 12, 32, 32);
			_btnClose.FlatStyle = FlatStyle.Flat;
			_btnClose.FlatAppearance.BorderSize = 0;
			_btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, 50, 72);
			_btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(28, 65, 92);
			_btnClose.BackColor = Color.FromArgb(11, 36, 54);
			_btnClose.TabStop = false;
			_btnClose.Paint += btnClose_Paint;
			_btnClose.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

			_iconView = new DialogIconView(_iconKind);
			_iconView.SetBounds(42, 98, 62, 62);

			if (!string.IsNullOrWhiteSpace(_cancelText))
			{
				_btnCancel = CreateDialogButton(_cancelText, false);
				_btnCancel.SetBounds(330, 232, 100, 40);
				_btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
			}

			_btnPrimary = CreateDialogButton(_primaryText, _primaryDanger);
			_btnPrimary.SetBounds(446, 232, 100, 40);
			_btnPrimary.Click += delegate { DialogResult = DialogResult.OK; Close(); };

			Controls.Add(_lblTitle);
			Controls.Add(_btnClose);
			Controls.Add(_iconView);
			Controls.Add(_lblMain);
			Controls.Add(_lblDetail);
			Controls.Add(_lblFooter);
			if (_btnCancel != null)
			{
				Controls.Add(_btnCancel);
			}
			Controls.Add(_btnPrimary);

			AcceptButton = _btnPrimary;
			if (_btnCancel != null)
			{
				CancelButton = _btnCancel;
			}
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

		private Button CreateDialogButton(string text, bool danger)
		{
			Button button = new Button();
			button.Text = text;
			button.FlatStyle = FlatStyle.Flat;
			button.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
			button.ForeColor = Color.White;
			button.BackColor = danger ? Color.FromArgb(216, 74, 87) : Color.FromArgb(6, 20, 34);
			button.FlatAppearance.BorderColor = danger ? Color.FromArgb(255, 120, 132) : Color.FromArgb(45, 127, 168);
			button.FlatAppearance.BorderSize = 1;
			button.FlatAppearance.MouseOverBackColor = danger ? Color.FromArgb(232, 88, 100) : Color.FromArgb(10, 34, 52);
			button.FlatAppearance.MouseDownBackColor = danger ? Color.FromArgb(188, 54, 66) : Color.FromArgb(12, 44, 68);
			return button;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (e == null)
			{
				return;
			}

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(11, 36, 54)))
			{
				e.Graphics.FillRectangle(titleBrush, 1, 1, ClientSize.Width - 2, 58);
			}

			using (Pen borderPen = new Pen(Color.FromArgb(22, 126, 183), 1.4F))
			{
				e.Graphics.DrawRectangle(borderPen, 0.7F, 0.7F, ClientSize.Width - 1.4F, ClientSize.Height - 1.4F);
			}

			using (Pen dividerPen = new Pen(Color.FromArgb(22, 51, 74), 1F))
			{
				e.Graphics.DrawLine(dividerPen, 32, 210, ClientSize.Width - 32, 210);
			}
		}

		private void btnClose_Paint(object sender, PaintEventArgs e)
		{
			if (e == null)
			{
				return;
			}

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using (Pen pen = new Pen(Color.FromArgb(156, 182, 202), 2F))
			{
				pen.StartCap = LineCap.Round;
				pen.EndCap = LineCap.Round;
				e.Graphics.DrawLine(pen, 10, 10, 22, 22);
				e.Graphics.DrawLine(pen, 22, 10, 10, 22);
			}
		}

		private sealed class DialogIconView : Control
		{
			private readonly ThemedDialogIconKind _kind;

			public DialogIconView(ThemedDialogIconKind kind)
			{
				_kind = kind;
				SetStyle(
					ControlStyles.AllPaintingInWmPaint |
					ControlStyles.OptimizedDoubleBuffer |
					ControlStyles.UserPaint |
					ControlStyles.SupportsTransparentBackColor,
					true);
				BackColor = Color.Transparent;
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				base.OnPaint(e);

				if (e == null)
				{
					return;
				}

				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				Color accent = GetAccentColor();
				Color fill = Color.FromArgb(38, accent.R, accent.G, accent.B);
				RectangleF circle = new RectangleF(4, 4, Width - 8, Height - 8);

				using (SolidBrush fillBrush = new SolidBrush(fill))
				using (Pen pen = new Pen(accent, 2.5F))
				{
					e.Graphics.FillEllipse(fillBrush, circle);
					e.Graphics.DrawEllipse(pen, circle);
				}

				DrawGlyph(e.Graphics, accent);
			}

			private Color GetAccentColor()
			{
				if (_kind == ThemedDialogIconKind.Delete)
				{
					return Color.FromArgb(255, 95, 105);
				}

				if (_kind == ThemedDialogIconKind.Warning)
				{
					return Color.FromArgb(255, 190, 64);
				}

				if (_kind == ThemedDialogIconKind.Success)
				{
					return Color.FromArgb(70, 210, 120);
				}

				return Color.FromArgb(70, 170, 230);
			}

			private void DrawGlyph(Graphics graphics, Color accent)
			{
				using (Pen pen = new Pen(accent, 2.6F))
				{
					pen.StartCap = LineCap.Round;
					pen.EndCap = LineCap.Round;

					if (_kind == ThemedDialogIconKind.Delete)
					{
						graphics.DrawLine(pen, 23, 27, 39, 27);
						graphics.DrawLine(pen, 26, 22, 36, 22);
						graphics.DrawRectangle(pen, 25, 31, 12, 14);
						graphics.DrawLine(pen, 29, 34, 29, 42);
						graphics.DrawLine(pen, 33, 34, 33, 42);
					}
					else if (_kind == ThemedDialogIconKind.Warning)
					{
						graphics.DrawLine(pen, 31, 18, 31, 36);
						graphics.DrawLine(pen, 31, 45, 31, 45);
					}
					else if (_kind == ThemedDialogIconKind.Success)
					{
						graphics.DrawLine(pen, 20, 32, 28, 40);
						graphics.DrawLine(pen, 28, 40, 43, 23);
					}
					else
					{
						graphics.DrawLine(pen, 31, 28, 31, 44);
						graphics.DrawLine(pen, 31, 19, 31, 19);
					}
				}
			}
		}
	}

	internal static class MessageBox
	{
		public static DialogResult Show(string text)
		{
			return Show(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		public static DialogResult Show(string text, string caption)
		{
			return Show(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
		{
			return Show(null, text, caption, buttons, MessageBoxIcon.None);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return Show(null, text, caption, buttons, icon);
		}

		public static DialogResult Show(IWin32Window owner, string text)
		{
			return Show(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption)
		{
			return Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
		{
			return Show(owner, text, caption, buttons, MessageBoxIcon.None);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			bool isEnglish = IsLikelyEnglish(text, caption);
			ThemedDialogIconKind iconKind = ToThemedIcon(icon, text, caption);
			bool danger = IsDangerPrompt(text, caption);

			if (buttons == MessageBoxButtons.YesNo)
			{
				bool accepted = ThemedDialog.Confirm(
					owner,
					caption,
					text,
					string.Empty,
					string.Empty,
					isEnglish ? "Yes" : "\u662f",
					isEnglish ? "No" : "\u5426",
					iconKind,
					danger);
				return accepted ? DialogResult.Yes : DialogResult.No;
			}

			if (buttons == MessageBoxButtons.OKCancel)
			{
				bool accepted = ThemedDialog.Confirm(
					owner,
					caption,
					text,
					string.Empty,
					string.Empty,
					isEnglish ? "OK" : "\u786e\u5b9a",
					isEnglish ? "Cancel" : "\u53d6\u6d88",
					iconKind,
					danger);
				return accepted ? DialogResult.OK : DialogResult.Cancel;
			}

			using (ThemedDialog dialog = new ThemedDialog(
				caption,
				text,
				string.Empty,
				string.Empty,
				isEnglish ? "OK" : "\u786e\u5b9a",
				string.Empty,
				iconKind,
				false))
			{
				return dialog.ShowDialog(owner) == DialogResult.OK ? DialogResult.OK : DialogResult.Cancel;
			}
		}

		private static ThemedDialogIconKind ToThemedIcon(MessageBoxIcon icon, string text, string caption)
		{
			if (IsDangerPrompt(text, caption))
			{
				return ThemedDialogIconKind.Delete;
			}

			if (icon == MessageBoxIcon.Warning ||
				icon == MessageBoxIcon.Exclamation ||
				icon == MessageBoxIcon.Error ||
				icon == MessageBoxIcon.Hand ||
				icon == MessageBoxIcon.Stop)
			{
				return ThemedDialogIconKind.Warning;
			}

			return ThemedDialogIconKind.Info;
		}

		private static bool IsDangerPrompt(string text, string caption)
		{
			string combined = ((text ?? string.Empty) + " " + (caption ?? string.Empty)).ToLowerInvariant();
			return combined.IndexOf("delete", StringComparison.OrdinalIgnoreCase) >= 0 ||
				   combined.IndexOf("remove", StringComparison.OrdinalIgnoreCase) >= 0 ||
				   combined.IndexOf("overwrite", StringComparison.OrdinalIgnoreCase) >= 0 ||
				   combined.IndexOf("\u5220\u9664", StringComparison.OrdinalIgnoreCase) >= 0 ||
				   combined.IndexOf("\u79fb\u9664", StringComparison.OrdinalIgnoreCase) >= 0 ||
				   combined.IndexOf("\u8986\u76d6", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static bool IsLikelyEnglish(string text, string caption)
		{
			string combined = (text ?? string.Empty) + (caption ?? string.Empty);
			foreach (char ch in combined)
			{
				if (ch > 127)
				{
					return false;
				}
			}

			return true;
		}
	}
}
