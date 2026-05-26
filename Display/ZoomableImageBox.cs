using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Aron_V3
{
	internal sealed class ZoomableImageBox : Control
	{
		private Image _image;
		private float _zoom;
		private PointF _pan;
		private bool _dragging;
		private Point _lastMouse;
		private string _displayMode;

		public ZoomableImageBox()
		{
			DoubleBuffered = true;
			BackColor = Color.Black;
			_displayMode = "Fit";
			_zoom = 1F;
			_pan = PointF.Empty;
			SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.UserPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw, true);
		}

		public Image Image
		{
			get { return _image; }
			set
			{
				if (ReferenceEquals(_image, value))
				{
					return;
				}

				Image old = _image;
				_image = value;
				ResetView();

				if (old != null)
				{
					old.Dispose();
				}
			}
		}

		public string DisplayMode
		{
			get { return _displayMode; }
			set
			{
				_displayMode = string.IsNullOrWhiteSpace(value) ? "Fit" : value;
				ResetView();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _image != null)
			{
				_image.Dispose();
				_image = null;
			}

			base.Dispose(disposing);
		}

		public void ResetView()
		{
			_zoom = 1F;
			_pan = PointF.Empty;
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.Clear(BackColor);

			if (_image == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
			{
				return;
			}

			RectangleF bounds = GetImageBounds();
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.DrawImage(_image, bounds);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (_image == null)
			{
				return;
			}

			RectangleF oldBounds = GetImageBounds();
			if (oldBounds.Width <= 0F || oldBounds.Height <= 0F)
			{
				return;
			}

			float relativeX = (e.X - oldBounds.X) / oldBounds.Width;
			float relativeY = (e.Y - oldBounds.Y) / oldBounds.Height;

			float oldZoom = _zoom;
			float factor = e.Delta > 0 ? 1.15F : 1F / 1.15F;
			_zoom = Math.Max(0.1F, Math.Min(30F, _zoom * factor));

			if (Math.Abs(_zoom - oldZoom) < 0.0001F)
			{
				return;
			}

			float scale = GetBaseScale() * _zoom;
			float width = _image.Width * scale;
			float height = _image.Height * scale;

			_pan = new PointF(
				e.X - relativeX * width - (ClientSize.Width - width) / 2F,
				e.Y - relativeY * height - (ClientSize.Height - height) / 2F);
			Invalidate();
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			Focus();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Focus();
			if (e.Button == MouseButtons.Left)
			{
				_dragging = true;
				_lastMouse = e.Location;
				Capture = true;
				Cursor = Cursors.SizeAll;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!_dragging)
			{
				return;
			}

			_pan = new PointF(
				_pan.X + e.X - _lastMouse.X,
				_pan.Y + e.Y - _lastMouse.Y);
			_lastMouse = e.Location;
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left)
			{
				_dragging = false;
				Capture = false;
				Cursor = Cursors.Default;
			}
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			if (e.Button == MouseButtons.Left)
			{
				ResetView();
			}
		}

		private RectangleF GetImageBounds()
		{
			float scale = GetBaseScale() * _zoom;
			float width = _image.Width * scale;
			float height = _image.Height * scale;
			float x = (ClientSize.Width - width) / 2F + _pan.X;
			float y = (ClientSize.Height - height) / 2F + _pan.Y;
			return new RectangleF(x, y, width, height);
		}

		private float GetBaseScale()
		{
			if (_image == null || _image.Width <= 0 || _image.Height <= 0)
			{
				return 1F;
			}

			if (string.Equals(_displayMode, "Original", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(_displayMode, "Center", StringComparison.OrdinalIgnoreCase))
			{
				return 1F;
			}

			float horizontalScale = ClientSize.Width / (float)_image.Width;
			float verticalScale = ClientSize.Height / (float)_image.Height;
			float scale = Math.Min(horizontalScale, verticalScale);
			return scale > 0F ? scale : 1F;
		}
	}
}
