using System;
using System.IO;
using System.Windows.Forms;

namespace Aron_V3
{
	public static class ProjectPathStore
	{
		public static string ProjectRoot
		{
			get
			{
				string path = Path.Combine(Application.StartupPath, "Project");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string ConfigRoot
		{
			get
			{
				string path = Path.Combine(ProjectRoot, "Config");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string SystemConfigRoot
		{
			get
			{
				string path = Path.Combine(ConfigRoot, "System");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string AlgorithmConfigRoot
		{
			get
			{
				string path = Path.Combine(ConfigRoot, "Algorithm");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string CommunicationConfigRoot
		{
			get
			{
				string path = Path.Combine(ConfigRoot, "Communication");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string JobRoot
		{
			get
			{
				string path = Path.Combine(ProjectRoot, "Job");
				Directory.CreateDirectory(path);
				return path;
			}
		}

		public static string GetJobFolder(string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				jobName = "Job_001";
			}

			string path = Path.Combine(JobRoot, MakeSafeName(jobName));
			Directory.CreateDirectory(path);
			return path;
		}

		public static string GetJobCameraRoot(string jobName)
		{
			string path = Path.Combine(GetJobFolder(jobName), "Camera");
			Directory.CreateDirectory(path);
			return path;
		}

		public static string GetCameraFolder(string jobName, string cameraName)
		{
			if (string.IsNullOrWhiteSpace(cameraName))
			{
				cameraName = "Cam1";
			}

			string path = Path.Combine(GetJobCameraRoot(jobName), MakeSafeName(cameraName));
			Directory.CreateDirectory(path);
			return path;
		}

		public static string MakeSafeName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				name = "New";
			}

			foreach (char c in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(c, '_');
			}

			return name.Trim();
		}
	}
}