using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace Aron_V3
{
	public sealed class StartupSplashController : IDisposable
	{
		private readonly ManualResetEventSlim _readyEvent = new ManualResetEventSlim(false);
		private Thread _thread;
		private StartupSplashForm _form;
		private bool _isEnglish;
		private bool _disposed;

		public static StartupSplashController Current { get; private set; }

		public static StartupSplashController Start()
		{
			return Start(LanguagePreferenceStore.LoadIsEnglish());
		}

		public static StartupSplashController Start(bool isEnglish)
		{
			StartupSplashController controller = new StartupSplashController();
			controller._isEnglish = isEnglish;
			Current = controller;
			controller.StartThread();
			return controller;
		}

		public static void UpdateCurrent(string status, int progress)
		{
			StartupSplashController controller = Current;
			if (controller != null)
			{
				controller.UpdateStatus(status, progress);
			}
		}

		private void StartThread()
		{
			_thread = new Thread(RunSplashThread);
			_thread.Name = "Startup Splash";
			_thread.IsBackground = true;
			_thread.SetApartmentState(ApartmentState.STA);
			_thread.Start();
			_readyEvent.Wait(1500);
		}

		private void RunSplashThread()
		{
			try
			{
				_form = new StartupSplashForm(_isEnglish);
				_readyEvent.Set();
				Application.Run(_form);
			}
			catch
			{
				_readyEvent.Set();
			}
		}

		public void UpdateStatus(string status, int progress)
		{
			StartupSplashForm form = _form;
			if (form == null || form.IsDisposed)
			{
				return;
			}

			try
			{
				form.SetStatus(status, progress);
			}
			catch
			{
			}
		}

		public void CompleteAndClose()
		{
			StartupSplashForm form = _form;
			if (form == null || form.IsDisposed)
			{
				ClearCurrent();
				return;
			}

			try
			{
				form.CompleteAndClose();
			}
			catch
			{
				Close();
			}
		}

		public void Close()
		{
			StartupSplashForm form = _form;
			if (form == null || form.IsDisposed)
			{
				ClearCurrent();
				return;
			}

			try
			{
				if (form.InvokeRequired)
				{
					form.BeginInvoke(new MethodInvoker(delegate { form.Close(); }));
				}
				else
				{
					form.Close();
				}
			}
			catch
			{
			}

			ClearCurrent();
		}

		private void ClearCurrent()
		{
			if (object.ReferenceEquals(Current, this))
			{
				Current = null;
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			Close();
			_readyEvent.Dispose();
		}
	}

	public sealed class StartupSplashForm : Form
	{
		private readonly System.Windows.Forms.Timer _timer;
		private readonly Stopwatch _watch;
		private readonly string[] _stages;
		private readonly bool _isEnglish;
		private readonly Image _brandLogoImage;
		private string _status;
		private int _targetProgress;
		private float _displayProgress;
		private float _phase;
		private bool _closing;

		public StartupSplashForm(bool isEnglish)
		{
			_isEnglish = isEnglish;
			_stages = isEnglish
				? new[]
				{
					"UI",
					"Project",
					"Scripts",
					"Runtime",
					"Comm"
				}
				: new[]
				{
					"界面",
					"项目",
					"脚本",
					"运行时",
					"通讯"
				};
			_status = isEnglish ? "Preparing startup..." : "准备启动...";
			_targetProgress = 3;
			_displayProgress = 0;
			_watch = Stopwatch.StartNew();

			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.CenterScreen;
			ClientSize = new Size(560, 320);
			BackColor = Color.FromArgb(3, 10, 20);
			ShowInTaskbar = false;
			TopMost = true;
			Opacity = 0;
			DoubleBuffered = true;
			_brandLogoImage = AppBrandAssets.LoadImage(AppBrandAssets.BrandLogoFileName);

			SetStyle(
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.UserPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw,
				true);

			_timer = new System.Windows.Forms.Timer();
			_timer.Interval = 16;
			_timer.Tick += Timer_Tick;
			_timer.Start();
		}

		public void SetStatus(string status, int progress)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new MethodInvoker(delegate { SetStatus(status, progress); }));
				return;
			}

			if (!string.IsNullOrWhiteSpace(status))
			{
				_status = status.Trim();
			}

			if (progress < 0) progress = 0;
			if (progress > 100) progress = 100;
			_targetProgress = progress;
			Invalidate();
		}

		public void CompleteAndClose()
		{
			if (InvokeRequired)
			{
				BeginInvoke(new MethodInvoker(CompleteAndClose));
				return;
			}

			_status = _isEnglish ? "Startup completed" : "启动完成";
			_targetProgress = 100;
			_closing = true;
			Invalidate();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			_phase += 0.035f;
			if (_phase > 1f)
			{
				_phase -= 1f;
			}

			if (_displayProgress < _targetProgress)
			{
				_displayProgress += Math.Max(0.35f, (_targetProgress - _displayProgress) * 0.12f);
				if (_displayProgress > _targetProgress)
				{
					_displayProgress = _targetProgress;
				}
			}

			if (!_closing && Opacity < 0.98)
			{
				Opacity = Math.Min(0.98, Opacity + 0.08);
			}

			if (_closing)
			{
				_displayProgress = Math.Max(_displayProgress, 100);
				Opacity -= 0.08;
				if (Opacity <= 0.05)
				{
					_timer.Stop();
					Close();
					return;
				}
			}

			Invalidate();
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			if (_timer != null)
			{
				_timer.Stop();
			}

			if (_brandLogoImage != null)
			{
				_brandLogoImage.Dispose();
			}

			base.OnFormClosed(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			Rectangle bounds = ClientRectangle;
			using (LinearGradientBrush brush = new LinearGradientBrush(
				bounds,
				Color.FromArgb(4, 13, 27),
				Color.FromArgb(8, 24, 43),
				LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, bounds);
			}

			DrawOuterFrame(g, bounds);
			DrawBrand(g);
			DrawSpinner(g);
			DrawStatus(g);
			DrawProgress(g);
			DrawStages(g);
		}

		private void DrawOuterFrame(Graphics g, Rectangle bounds)
		{
			Rectangle frame = new Rectangle(0, 0, bounds.Width - 1, bounds.Height - 1);
			using (Pen border = new Pen(Color.FromArgb(70, 120, 165), 1))
			{
				g.DrawRectangle(border, frame);
			}

			int highlightWidth = 130;
			int highlightX = (int)((bounds.Width + highlightWidth) * _phase) - highlightWidth;
			using (Pen cyan = new Pen(Color.FromArgb(0, 190, 255), 2))
			{
				g.DrawLine(cyan, highlightX, 0, highlightX + highlightWidth, 0);
			}
		}

        private void DrawBrand(Graphics g)
        {
            if (_brandLogoImage != null)
            {
                InterpolationMode oldInterpolationMode = g.InterpolationMode;
                PixelOffsetMode oldPixelOffsetMode = g.PixelOffsetMode;

                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                Rectangle logoBounds = new Rectangle(44, 38, 318, 105);
                g.DrawImage(_brandLogoImage, logoBounds);

                g.InterpolationMode = oldInterpolationMode;
                g.PixelOffsetMode = oldPixelOffsetMode;
                return;
            }

            using (Font logoFont = new Font("Microsoft YaHei UI", 48f, FontStyle.Bold))
            using (Font titleFont = new Font("Microsoft YaHei UI", 17f, FontStyle.Bold))
            using (SolidBrush logoBrush = new SolidBrush(Color.FromArgb(30, 125, 255)))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(235, 242, 252)))
            {
                g.DrawString("B", logoFont, logoBrush, 42, 34);
                g.DrawString("Betterway", titleFont, titleBrush, 112, 49);
                g.DrawString("Vision-Base", titleFont, titleBrush, 112, 78);
            }
        }

		private void DrawSpinner(Graphics g)
		{
			Rectangle ring = new Rectangle(ClientSize.Width - 86, 42, 42, 42);
			using (Pen basePen = new Pen(Color.FromArgb(32, 65, 95), 5))
			using (Pen activePen = new Pen(Color.FromArgb(0, 185, 255), 5))
			{
				g.DrawEllipse(basePen, ring);
				g.DrawArc(activePen, ring, _phase * 360f, 105f);
			}
		}

        private void DrawStatus(Graphics g)
        {
            using (Font captionFont = new Font("Microsoft YaHei UI", 10.5f, FontStyle.Bold))
            using (Font statusFont = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold))
            using (Font timeFont = new Font("Consolas", 9f, FontStyle.Regular))
            using (SolidBrush captionBrush = new SolidBrush(Color.FromArgb(120, 155, 190)))
            using (SolidBrush statusBrush = new SolidBrush(Color.FromArgb(232, 243, 255)))
            using (SolidBrush timeBrush = new SolidBrush(Color.FromArgb(92, 122, 150)))
            {
                g.DrawString(
                    _isEnglish ? "Initializing runtime workspace" : "正在初始化运行工作区",
                    captionFont,
                    captionBrush,
                    44,
                    157);
                g.DrawString(_status, statusFont, statusBrush, 44, 181);
                g.DrawString(_watch.Elapsed.TotalSeconds.ToString("0.0") + " s", timeFont, timeBrush, ClientSize.Width - 86, 160);
            }
        }

		private void DrawProgress(Graphics g)
		{
			Rectangle bar = new Rectangle(44, 222, ClientSize.Width - 88, 13);
			using (SolidBrush bg = new SolidBrush(Color.FromArgb(5, 18, 34)))
			using (Pen border = new Pen(Color.FromArgb(36, 75, 110), 1))
			{
				g.FillRectangle(bg, bar);
				g.DrawRectangle(border, bar);
			}

			int fillWidth = (int)((bar.Width - 2) * (_displayProgress / 100f));
			if (fillWidth > 0)
			{
				Rectangle fill = new Rectangle(bar.X + 1, bar.Y + 1, fillWidth, bar.Height - 2);
				using (LinearGradientBrush fillBrush = new LinearGradientBrush(
					fill,
					Color.FromArgb(0, 110, 210),
					Color.FromArgb(0, 205, 255),
					LinearGradientMode.Horizontal))
				{
					g.FillRectangle(fillBrush, fill);
				}

				int shimmerX = fill.X + (int)((fill.Width + 40) * _phase) - 40;
				Rectangle shimmer = new Rectangle(shimmerX, fill.Y, 34, fill.Height);
				using (SolidBrush shimmerBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
				{
					g.FillRectangle(shimmerBrush, shimmer);
				}
			}

			using (Font percentFont = new Font("Consolas", 10f, FontStyle.Bold))
			using (SolidBrush percentBrush = new SolidBrush(Color.FromArgb(185, 218, 245)))
			{
				string percent = ((int)_displayProgress).ToString() + "%";
				SizeF size = g.MeasureString(percent, percentFont);
				g.DrawString(percent, percentFont, percentBrush, bar.Right - size.Width, bar.Y - 24);
			}
		}

		private void DrawStages(Graphics g)
		{
			int baseY = 258;
			int startX = 46;
			int gap = 96;
			int activeIndex = Math.Min(_stages.Length - 1, Math.Max(0, (int)(_displayProgress / 22f)));

			using (Font stageFont = new Font("Microsoft YaHei UI", 8.5f, FontStyle.Regular))
			{
				for (int i = 0; i < _stages.Length; i++)
				{
					bool done = i < activeIndex || _displayProgress >= 100;
					bool active = i == activeIndex && _displayProgress < 100;
					Color dotColor = done
						? Color.FromArgb(45, 210, 120)
						: active
							? Color.FromArgb(0, 185, 255)
							: Color.FromArgb(55, 78, 100);
					Color textColor = done || active
						? Color.FromArgb(205, 225, 242)
						: Color.FromArgb(93, 117, 142);

					int x = startX + i * gap;
					using (SolidBrush dotBrush = new SolidBrush(dotColor))
					using (SolidBrush textBrush = new SolidBrush(textColor))
					{
						g.FillEllipse(dotBrush, x, baseY, 8, 8);
						g.DrawString(_stages[i], stageFont, textBrush, x - 7, baseY + 16);
					}

					if (i < _stages.Length - 1)
					{
						using (Pen linePen = new Pen(done ? Color.FromArgb(30, 145, 90) : Color.FromArgb(28, 52, 76), 1))
						{
							g.DrawLine(linePen, x + 14, baseY + 4, x + gap - 12, baseY + 4);
						}
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_timer != null)
				{
					_timer.Stop();
					_timer.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
