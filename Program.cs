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
			DiagnosticLogStore.Initialize();
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += Application_ThreadException;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			Application.ApplicationExit += Application_ApplicationExit;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			bool isEnglish = LanguagePreferenceStore.LoadIsEnglish();
			using (StartupSplashController splash = StartupSplashController.Start(isEnglish))
			{
				splash.UpdateStatus(isEnglish ? "Loading main UI controls" : "加载主界面控件", 12);
				Form1 mainForm = new Form1();
				splash.UpdateStatus(isEnglish ? "Preparing main window" : "准备显示主窗体", 92);
				mainForm.Shown += delegate
				{
					splash.CompleteAndClose();
				};
				Application.Run(mainForm);
			}
		}

		private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			HandleUnexpectedException(e == null ? null : e.Exception, "WinFormsThreadException", false);
		}

		private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			HandleUnexpectedException(e == null ? null : e.Exception, "UnobservedTaskException", false);
			if (e != null)
			{
				e.SetObserved();
			}
		}

		private static void Application_ApplicationExit(object sender, EventArgs e)
		{
			DiagnosticLogStore.Append(DiagnosticLogLevel.Info, "Application", "Application exiting.");
			StopCommunicationRuntime();
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleUnexpectedException(e == null ? null : e.ExceptionObject as Exception, "AppDomainUnhandledException", true);
			StopCommunicationRuntime();
		}

		private static void HandleUnexpectedException(Exception ex, string source, bool terminating)
		{
			try
			{
				DiagnosticLogStore.WriteCrashReport(ex, source, terminating);
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
