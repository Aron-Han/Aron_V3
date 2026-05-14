using System;
using System.Windows.Forms;

namespace Aron_V3
{
	/// <summary>
	/// 全局用户操作监听器。
	/// 用于自动注销逻辑：只有连续一段时间没有鼠标/键盘操作，才会触发自动注销。
	/// </summary>
	public class UserActivityMessageFilter : IMessageFilter
	{
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private const int WM_SYSKEYDOWN = 0x0104;
		private const int WM_SYSKEYUP = 0x0105;

		private const int WM_MOUSEMOVE = 0x0200;
		private const int WM_LBUTTONDOWN = 0x0201;
		private const int WM_LBUTTONUP = 0x0202;
		private const int WM_LBUTTONDBLCLK = 0x0203;
		private const int WM_RBUTTONDOWN = 0x0204;
		private const int WM_RBUTTONUP = 0x0205;
		private const int WM_RBUTTONDBLCLK = 0x0206;
		private const int WM_MBUTTONDOWN = 0x0207;
		private const int WM_MBUTTONUP = 0x0208;
		private const int WM_MOUSEWHEEL = 0x020A;

		public bool PreFilterMessage(ref Message m)
		{
			if (IsUserActivityMessage(m.Msg))
			{
				LoginSession.Touch();
			}

			return false;
		}

		private bool IsUserActivityMessage(int msg)
		{
			switch (msg)
			{
				case WM_KEYDOWN:
				case WM_KEYUP:
				case WM_SYSKEYDOWN:
				case WM_SYSKEYUP:
				case WM_MOUSEMOVE:
				case WM_LBUTTONDOWN:
				case WM_LBUTTONUP:
				case WM_LBUTTONDBLCLK:
				case WM_RBUTTONDOWN:
				case WM_RBUTTONUP:
				case WM_RBUTTONDBLCLK:
				case WM_MBUTTONDOWN:
				case WM_MBUTTONUP:
				case WM_MOUSEWHEEL:
					return true;

				default:
					return false;
			}
		}
	}
}
