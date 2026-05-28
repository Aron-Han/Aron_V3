using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aron_V3;

namespace WindowsFormsApp1
{
	internal static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += Application_ThreadException;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			Application.ApplicationExit += Application_ApplicationExit;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			HandleUnexpectedException(e == null ? null : e.Exception);
		}

		private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			HandleUnexpectedException(e == null ? null : e.Exception);
			if (e != null)
			{
				e.SetObserved();
			}
		}

		private static void Application_ApplicationExit(object sender, EventArgs e)
		{
			StopCommunicationRuntime();
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleUnexpectedException(e == null ? null : e.ExceptionObject as Exception);
			StopCommunicationRuntime();
		}

		private static void HandleUnexpectedException(Exception ex)
		{
			try
			{
				RuntimeLogStore.Append(
					DateTime.Now,
					RuntimeLogCategory.Task,
					"Unexpected application exception: " + (ex == null ? "unknown" : ex.Message),
					true);
			}
			catch
			{
			}
		}

		private static void StopCommunicationRuntime()
		{
			try
			{
				CommunicationRuntimeManager.Instance.Stop();
			}
			catch
			{
			}
		}
	}
}
