using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Aron_V3
{
	public enum CameraAcquisitionMode
	{
		VPro,
		SDK
	}

	public enum CameraSdkBrand
	{
		None,
		LMI,
		Keyence,
		Hikvision,
		Dahua
	}

	[Serializable]
	public class HardwareProjectConfig
	{
		public List<CameraDeviceConfig> Cameras { get; set; }

		public HardwareProjectConfig()
		{
			Cameras = new List<CameraDeviceConfig>();
		}
	}

	[Serializable]
	public class CameraDeviceConfig
	{
		public string CameraName { get; set; }
		public bool Enable { get; set; }
		public CameraAcquisitionMode AcquisitionMode { get; set; }
		public CameraSdkBrand SdkBrand { get; set; }
		public string Status { get; set; }

		// 后续如果不同程序号 / Task 需要不同取像参数，可以用 ProfileName 区分。
		// 例如：Default / Job001_TaskMain / Pos1 / OfflineReplay 等。
		public string AcqProfileName { get; set; }

		public VisionProAcqConfig VisionPro { get; set; }
		public SdkCameraConfig Sdk { get; set; }

		public CameraDeviceConfig()
		{
			CameraName = "Cam1";
			Enable = true;
			AcquisitionMode = CameraAcquisitionMode.VPro;
			SdkBrand = CameraSdkBrand.None;
			Status = "Disconnected";
			AcqProfileName = "Default";
			VisionPro = new VisionProAcqConfig();
			Sdk = new SdkCameraConfig();
		}
	}

	[Serializable]
	public class VisionProAcqConfig
	{
		public string ToolName { get; set; }
		public string AcqVppPath { get; set; }
		public string DeviceName { get; set; }
		public string VideoFormat { get; set; }
		public string PixelFormat { get; set; }
		public double ExposureUs { get; set; }
		public int TimeoutMs { get; set; }
		public string SerialNumber { get; set; }

		public VisionProAcqConfig()
		{
			ToolName = string.Empty;
			AcqVppPath = string.Empty;
			DeviceName = "default";
			VideoFormat = "Mono8";
			PixelFormat = "Mono8";
			ExposureUs = 5000;
			TimeoutMs = 5000;
			SerialNumber = string.Empty;
		}
	}

	[Serializable]
	public class SdkCameraConfig
	{
		public CameraSdkBrand Brand { get; set; }
		public string ToolName { get; set; }
		public string ConfigPath { get; set; }
		public string IpAddress { get; set; }
		public int Port { get; set; }
		public string SerialNumber { get; set; }
		public string TriggerMode { get; set; }
		public double ExposureUs { get; set; }
		public double GainDb { get; set; }
		public string PixelFormat { get; set; }

		// 品牌 SDK 特有参数保存为 Key/Value，方便后续 LMI / Keyence / 海康 / 大华各自扩展。
		public List<SdkParameterItem> ExtraParameters { get; set; }

		public SdkCameraConfig()
		{
			Brand = CameraSdkBrand.Hikvision;
			ToolName = "Cam_Sdk";
			ConfigPath = string.Empty;
			IpAddress = "192.168.1.100";
			Port = 3956;
			SerialNumber = string.Empty;
			TriggerMode = "Off";
			ExposureUs = 5000;
			GainDb = 0;
			PixelFormat = "Mono8";
			ExtraParameters = new List<SdkParameterItem>();
		}
	}

	[Serializable]
	public class SdkParameterItem
	{
		public string Name { get; set; }
		public string Value { get; set; }

		public SdkParameterItem()
		{
			Name = string.Empty;
			Value = string.Empty;
		}

		public SdkParameterItem(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}

	[Serializable]
	public class ImageSourceConfig
	{
		public string SourceName { get; set; }
		public string CameraName { get; set; }
		public string AcqMode { get; set; }
		public string SdkBrand { get; set; }
		public string ProfileName { get; set; }
		public string ToolName { get; set; }
		public string ConfigPath { get; set; }
		public string OutputImageKey { get; set; }
		public bool Enable { get; set; }

		public ImageSourceConfig()
		{
			SourceName = string.Empty;
			CameraName = string.Empty;
			AcqMode = string.Empty;
			SdkBrand = string.Empty;
			ProfileName = "Default";
			ToolName = string.Empty;
			ConfigPath = string.Empty;
			OutputImageKey = string.Empty;
			Enable = true;
		}
	}

	[Serializable]
	public class ImageSourceConfigList
	{
		public List<ImageSourceConfig> Sources { get; set; }

		public ImageSourceConfigList()
		{
			Sources = new List<ImageSourceConfig>();
		}
	}


	public interface ICameraSdkAdapter
	{
		CameraSdkBrand Brand { get; }
		bool IsConnected { get; }
		void LoadConfig(SdkCameraConfig config);
		SdkCameraConfig ExportConfig();
		void Connect();
		void Disconnect();
		object Grab();
		void StartLive();
		void StopLive();
	}

	public static class CameraSdkAdapterFactory
	{
		private static readonly Dictionary<CameraSdkBrand, Type> _registeredAdapters = new Dictionary<CameraSdkBrand, Type>();

		public static void Register<TAdapter>(CameraSdkBrand brand) where TAdapter : ICameraSdkAdapter, new()
		{
			if (_registeredAdapters.ContainsKey(brand))
			{
				_registeredAdapters[brand] = typeof(TAdapter);
			}
			else
			{
				_registeredAdapters.Add(brand, typeof(TAdapter));
			}
		}

		public static ICameraSdkAdapter Create(CameraSdkBrand brand)
		{
			if (_registeredAdapters.ContainsKey(brand))
			{
				return Activator.CreateInstance(_registeredAdapters[brand]) as ICameraSdkAdapter;
			}

			return null;
		}
	}

	public static class HardwareConfigStore
	{
		private static string _currentJobName = string.Empty;

		public static string CurrentJobName
		{
			get { return _currentJobName; }
		}

		public static bool HasCurrentJob
		{
			get { return !string.IsNullOrWhiteSpace(_currentJobName); }
		}

		public static void SetCurrentJobName(string jobName)
		{
			_currentJobName = NormalizeFileName(jobName, string.Empty);
		}

		public static void ClearCurrentJobName()
		{
			_currentJobName = string.Empty;
		}

		public static string ProjectRoot
		{
			get
			{
				string folder = Path.Combine(Application.StartupPath, "Project");
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
				return folder;
			}
		}

		public static string JobRootContainer
		{
			get
			{
				string folder = Path.Combine(ProjectRoot, "Config", "Program");
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
				return folder;
			}
		}

		public static string LegacyJobRootContainer
		{
			get { return Path.Combine(ProjectRoot, "Job"); }
		}

		public static string JobRootFolder
		{
			get { return GetJobRootFolder(_currentJobName, false); }
		}

		public static string ConfigFolder
		{
			get
			{
				string jobFolder = JobRootFolder;
				if (string.IsNullOrWhiteSpace(jobFolder))
				{
					return string.Empty;
				}

				string folder = Path.Combine(jobFolder, "Hardware");
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}

				return folder;
			}
		}

		public static string ConfigFilePath
		{
			get
			{
				string folder = ConfigFolder;
				if (string.IsNullOrWhiteSpace(folder))
				{
					return string.Empty;
				}

				return Path.Combine(folder, "HardwareConfig.xml");
			}
		}

		public static string ImageSourceConfigPath
		{
			get
			{
				string folder = ConfigFolder;
				if (string.IsNullOrWhiteSpace(folder))
				{
					return string.Empty;
				}

				return Path.Combine(folder, "ImageSources.xml");
			}
		}

		public static void AutoSelectFirstJobIfNeeded()
		{
			if (HasCurrentJob)
			{
				return;
			}

			string jobRoot = JobRootContainer;
			string[] dirs = Directory.Exists(jobRoot)
				? Directory.GetDirectories(jobRoot)
				: new string[0];

			if ((dirs == null || dirs.Length == 0) && Directory.Exists(LegacyJobRootContainer))
			{
				dirs = Directory.GetDirectories(LegacyJobRootContainer);
			}

			if (dirs == null || dirs.Length == 0)
			{
				try
				{
					ProjectFlowConfig flowConfig = FlowConfigStore.LoadOrCreateDefault();
					JobConfig firstJob = flowConfig == null || flowConfig.Jobs == null
						? null
						: flowConfig.Jobs.FirstOrDefault(x => x != null && !string.IsNullOrWhiteSpace(x.JobName));
					if (firstJob != null)
					{
						_currentJobName = NormalizeFileName(firstJob.JobName, string.Empty);
					}
				}
				catch
				{
				}
				return;
			}

			Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
			_currentJobName = Path.GetFileName(dirs[0]);
		}

		public static string GetJobRootFolder()
		{
			return GetJobRootFolder(_currentJobName, false);
		}

		public static string GetJobRootFolder(string jobName)
		{
			return GetJobRootFolder(jobName, true);
		}

		private static string GetJobRootFolder(string jobName, bool createIfMissing)
		{
			string safeJob = NormalizeFileName(jobName, string.Empty);

			if (string.IsNullOrWhiteSpace(safeJob))
			{
				return string.Empty;
			}

			string folder = Path.Combine(JobRootContainer, safeJob);
			string legacyFolder = Path.Combine(LegacyJobRootContainer, safeJob);

			if (!Directory.Exists(folder) && Directory.Exists(legacyFolder))
			{
				MigrateLegacyHardwareFolder(legacyFolder, folder);
			}

			if (createIfMissing && !Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			return folder;
		}

		private static void MigrateLegacyHardwareFolder(string legacyJobFolder, string currentJobFolder)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(legacyJobFolder) ||
					string.IsNullOrWhiteSpace(currentJobFolder) ||
					!Directory.Exists(legacyJobFolder))
				{
					return;
				}

				string legacyHardware = Path.Combine(legacyJobFolder, "Hardware");
				string currentHardware = Path.Combine(currentJobFolder, "Hardware");
				if (Directory.Exists(legacyHardware))
				{
					MoveDirectoryContent(legacyHardware, currentHardware);
				}

				string legacyCamera = Path.Combine(legacyJobFolder, "Camera");
				if (Directory.Exists(legacyCamera))
				{
					MoveDirectoryContent(legacyCamera, Path.Combine(currentHardware, "Camera"));
				}
			}
			catch
			{
			}
		}

		private static void MoveDirectoryContent(string sourceDir, string targetDir)
		{
			if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(targetDir) || !Directory.Exists(sourceDir))
			{
				return;
			}

			if (!Directory.Exists(targetDir))
			{
				Directory.CreateDirectory(targetDir);
			}

			foreach (string file in Directory.GetFiles(sourceDir))
			{
				string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
				if (File.Exists(targetFile))
				{
					File.Delete(targetFile);
				}
				File.Move(file, targetFile);
			}

			foreach (string dir in Directory.GetDirectories(sourceDir))
			{
				string targetSubDir = Path.Combine(targetDir, Path.GetFileName(dir));
				MoveDirectoryContent(dir, targetSubDir);
				try
				{
					if (Directory.Exists(dir) && Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
					{
						Directory.Delete(dir, false);
					}
				}
				catch
				{
				}
			}
		}

		public static string GetCameraRootFolder()
		{
			return GetCameraRootFolder(_currentJobName, true);
		}

		public static string GetCameraRootFolder(string jobName)
		{
			return GetCameraRootFolder(jobName, true);
		}

		private static string GetCameraRootFolder(string jobName, bool createIfMissing)
		{
			string jobFolder = GetJobRootFolder(jobName, createIfMissing);

			if (string.IsNullOrWhiteSpace(jobFolder))
			{
				return string.Empty;
			}

			string folder = Path.Combine(jobFolder, "Hardware", "Camera");

			if (createIfMissing && !Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			return folder;
		}

		public static string GetCameraFolder(string cameraName)
		{
			return GetCameraFolder(cameraName, _currentJobName);
		}

		public static string GetCameraFolder(string cameraName, string jobName)
		{
			string root = GetCameraRootFolder(jobName, true);

			if (string.IsNullOrWhiteSpace(root))
			{
				return string.Empty;
			}

			string safeCamera = NormalizeFileName(cameraName, "Cam1");
			string folder = Path.Combine(root, safeCamera);

			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			return folder;
		}

		public static string GetCameraProfileFolder(string cameraName, string profileName)
		{
			return GetCameraFolder(cameraName);
		}

		public static string GetVisionProFolder(string cameraName, string profileName)
		{
			return GetCameraFolder(cameraName);
		}

		public static string GetSdkFolder(string cameraName, string profileName)
		{
			return GetCameraFolder(cameraName);
		}

		public static string GetDefaultVisionProAcqPath(string cameraName)
		{
			return string.Empty;
		}

		public static string GetDefaultVisionProAcqPath(string cameraName, string profileName)
		{
			return string.Empty;
		}


		public static string GetVisionProAcqPath(string cameraName, string profileName, string toolName)
		{
			string folder = GetVisionProFolder(cameraName, profileName);

			if (string.IsNullOrWhiteSpace(folder))
			{
				return string.Empty;
			}

			toolName = NormalizeFileName(toolName, cameraName + "_Acq");
			return Path.Combine(folder, toolName + ".vpp");
		}

		public static string NormalizeFileName(string fileName, string defaultName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = defaultName;
			}

			if (fileName == null)
			{
				return string.Empty;
			}

			foreach (char c in Path.GetInvalidFileNameChars())
			{
				fileName = fileName.Replace(c, '_');
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = defaultName;
			}

			if (fileName == null)
			{
				return string.Empty;
			}

			return fileName.Trim();
		}

		public static string GetSdkConfigPath(string cameraName, string profileName, CameraSdkBrand brand)
		{
			return GetSdkConfigPath(cameraName, profileName, brand, cameraName + "_" + brand + "_Sdk");
		}

		public static string GetSdkConfigPath(string cameraName, string profileName, CameraSdkBrand brand, string toolName)
		{
			string folder = GetSdkFolder(cameraName, profileName);

			if (string.IsNullOrWhiteSpace(folder))
			{
				return string.Empty;
			}

			toolName = NormalizeFileName(toolName, cameraName + "_" + brand + "_Sdk");
			return Path.Combine(folder, toolName + ".xml");
		}

		public static string CopyImportedFileToProject(string sourceFile, string targetFolder, string targetFileNameWithoutExtension, string extensionWithDot, bool overwrite)
		{
			if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
			{
				throw new FileNotFoundException("Source file does not exist.", sourceFile);
			}

			if (string.IsNullOrWhiteSpace(targetFolder))
			{
				throw new ArgumentException("Target folder is empty.");
			}

			if (!Directory.Exists(targetFolder))
			{
				Directory.CreateDirectory(targetFolder);
			}

			string safeName = NormalizeFileName(targetFileNameWithoutExtension, Path.GetFileNameWithoutExtension(sourceFile));

			if (string.IsNullOrWhiteSpace(extensionWithDot))
			{
				extensionWithDot = Path.GetExtension(sourceFile);
			}

			if (!extensionWithDot.StartsWith("."))
			{
				extensionWithDot = "." + extensionWithDot;
			}

			string targetPath = Path.Combine(targetFolder, safeName + extensionWithDot);

			if (File.Exists(targetPath) && !overwrite)
			{
				throw new IOException("Target file already exists: " + targetPath);
			}

			if (string.Equals(Path.GetFullPath(sourceFile), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
			{
				return targetPath;
			}

			File.Copy(sourceFile, targetPath, true);
			return targetPath;
		}

		public static HardwareProjectConfig LoadOrCreateDefault()
		{
			try
			{
				AutoSelectFirstJobIfNeeded();

				if (!HasCurrentJob)
				{
					return CreateDefault();
				}

				if (!Directory.Exists(ConfigFolder))
				{
					Directory.CreateDirectory(ConfigFolder);
				}

				if (string.IsNullOrWhiteSpace(ConfigFilePath) || !File.Exists(ConfigFilePath))
				{
					HardwareProjectConfig emptyConfig = CreateDefault();
					Save(emptyConfig);
					return emptyConfig;
				}

				XmlSerializer serializer = new XmlSerializer(typeof(HardwareProjectConfig));

				HardwareProjectConfig config;
				using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read))
				{
					config = serializer.Deserialize(fs) as HardwareProjectConfig;
				}

				if (config == null)
				{
					config = CreateDefault();
				}

				EnsureCameraFolders(config);
				Save(config);
				return config;
			}
			catch
			{
				return CreateDefault();
			}
		}

		public static void Save(HardwareProjectConfig config)
		{
			if (config == null)
			{
				config = CreateDefault();
			}

			if (!HasCurrentJob)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(ConfigFolder) || string.IsNullOrWhiteSpace(ConfigFilePath))
			{
				return;
			}

			if (!Directory.Exists(ConfigFolder))
			{
				Directory.CreateDirectory(ConfigFolder);
			}

			EnsureCameraFolders(config);

			XmlSerializer serializer = new XmlSerializer(typeof(HardwareProjectConfig));

			using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Create, FileAccess.Write))
			{
				serializer.Serialize(fs, config);
			}

			SaveImageSourceList(config);
		}

		public static void SaveSdkConfig(CameraDeviceConfig camera)
		{
			if (!HasCurrentJob)
			{
				return;
			}

			if (camera == null || camera.Sdk == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(camera.Sdk.ToolName))
			{
				camera.Sdk.ToolName = camera.CameraName + "_" + camera.SdkBrand + "_Sdk";
			}

			string path = GetSdkConfigPath(camera.CameraName, camera.AcqProfileName, camera.SdkBrand, camera.Sdk.ToolName);

			if (string.IsNullOrWhiteSpace(path))
			{
				return;
			}

			camera.Sdk.ConfigPath = path;

			string folder = Path.GetDirectoryName(path);

			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(SdkCameraConfig));

			using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				serializer.Serialize(fs, camera.Sdk);
			}
		}

		public static SdkCameraConfig LoadSdkConfig(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
			{
				return null;
			}

			XmlSerializer serializer = new XmlSerializer(typeof(SdkCameraConfig));

			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				return serializer.Deserialize(fs) as SdkCameraConfig;
			}
		}

		public static void DeleteCameraFolder(string cameraName)
		{
			if (string.IsNullOrWhiteSpace(cameraName) || !HasCurrentJob)
			{
				return;
			}

			string folder = GetCameraFolder(cameraName);

			if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
			{
				return;
			}

			Directory.Delete(folder, true);
		}

		public static void SaveImageSourceList(HardwareProjectConfig config)
		{
			if (!HasCurrentJob || string.IsNullOrWhiteSpace(ImageSourceConfigPath))
			{
				return;
			}

			ImageSourceConfigList list = new ImageSourceConfigList();

			if (config != null && config.Cameras != null)
			{
				foreach (CameraDeviceConfig cam in config.Cameras)
				{
					if (cam == null || string.IsNullOrWhiteSpace(cam.CameraName))
					{
						continue;
					}

					string cameraFolder = GetCameraFolder(cam.CameraName);

					if (string.IsNullOrWhiteSpace(cameraFolder) || !Directory.Exists(cameraFolder))
					{
						continue;
					}

					string[] files = Directory.GetFiles(cameraFolder, "*.*", SearchOption.TopDirectoryOnly);

					foreach (string file in files)
					{
						if (!IsCameraConfigFileForImageSource(file, cam))
						{
							continue;
						}

						ImageSourceConfig source = new ImageSourceConfig();
						source.SourceName = cam.CameraName + "." + Path.GetFileName(file);
						source.CameraName = cam.CameraName;
						source.AcqMode = string.Equals(Path.GetExtension(file), ".vpp", StringComparison.OrdinalIgnoreCase) ? CameraAcquisitionMode.VPro.ToString() : CameraAcquisitionMode.SDK.ToString();
						source.SdkBrand = cam.SdkBrand.ToString();
						source.ProfileName = string.IsNullOrWhiteSpace(cam.AcqProfileName) ? "Default" : cam.AcqProfileName;
						source.ToolName = Path.GetFileNameWithoutExtension(file);
						source.ConfigPath = file;
						source.OutputImageKey = source.SourceName;
						source.Enable = cam.Enable;

						list.Sources.Add(source);
					}
				}
			}

			if (!Directory.Exists(ConfigFolder))
			{
				Directory.CreateDirectory(ConfigFolder);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(ImageSourceConfigList));

			using (FileStream fs = new FileStream(ImageSourceConfigPath, FileMode.Create, FileAccess.Write))
			{
				serializer.Serialize(fs, list);
			}
		}

		private static bool IsCameraConfigFileForImageSource(string file, CameraDeviceConfig camera)
		{
			if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
			{
				return false;
			}

			string ext = Path.GetExtension(file);

			if (!string.Equals(ext, ".vpp", StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(ext, ".xml", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			string name = Path.GetFileNameWithoutExtension(file);

			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			if (name.Equals("HardwareConfig", StringComparison.OrdinalIgnoreCase) ||
				name.Equals("ImageSources", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (name.Equals("Camera", StringComparison.OrdinalIgnoreCase))
			{
				string currentPath = camera == null || camera.VisionPro == null ? string.Empty : camera.VisionPro.AcqVppPath;

				if (string.IsNullOrWhiteSpace(currentPath) ||
					!string.Equals(Path.GetFullPath(file), Path.GetFullPath(currentPath), StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			return true;
		}


		private static HardwareProjectConfig CreateDefault()
		{
			return new HardwareProjectConfig();
		}

		private static void EnsureCameraFolders(HardwareProjectConfig config)
		{
			if (!HasCurrentJob)
			{
				return;
			}

			if (config == null || config.Cameras == null)
			{
				return;
			}

			foreach (CameraDeviceConfig camera in config.Cameras)
			{
				if (camera == null || string.IsNullOrWhiteSpace(camera.CameraName))
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(camera.AcqProfileName))
				{
					camera.AcqProfileName = "Default";
				}

				string cameraFolder = GetCameraFolder(camera.CameraName);

				if (!string.IsNullOrWhiteSpace(cameraFolder) && !Directory.Exists(cameraFolder))
				{
					Directory.CreateDirectory(cameraFolder);
				}

				if (camera.VisionPro == null)
				{
					camera.VisionPro = new VisionProAcqConfig();
				}

				if (camera.Sdk == null)
				{
					camera.Sdk = new SdkCameraConfig();
				}

				if (string.IsNullOrWhiteSpace(camera.VisionPro.ToolName))
				{
					camera.VisionPro.ToolName = string.Empty;
				}

				if (string.IsNullOrWhiteSpace(camera.VisionPro.AcqVppPath))
				{
					camera.VisionPro.AcqVppPath = string.Empty;
				}

				if (!string.IsNullOrWhiteSpace(camera.VisionPro.AcqVppPath))
				{
					camera.VisionPro.AcqVppPath = NormalizeStoredPathForCurrentJob(camera.VisionPro.AcqVppPath);
					string fileName = Path.GetFileNameWithoutExtension(camera.VisionPro.AcqVppPath);
					camera.VisionPro.ToolName = NormalizeFileName(fileName, camera.VisionPro.ToolName);

					string fullPath = Path.GetFullPath(camera.VisionPro.AcqVppPath);
					string cameraRoot = Path.GetFullPath(cameraFolder);

					if (fullPath.StartsWith(cameraRoot, StringComparison.OrdinalIgnoreCase))
					{
						camera.VisionPro.AcqVppPath = fullPath;
					}
				}

				if (camera.SdkBrand != CameraSdkBrand.None)
				{
					camera.Sdk.Brand = camera.SdkBrand;
				}

				if (string.IsNullOrWhiteSpace(camera.Sdk.ToolName))
				{
					camera.Sdk.ToolName = camera.CameraName + "_" + camera.SdkBrand + "_Sdk";
				}

				if (string.IsNullOrWhiteSpace(camera.Sdk.ConfigPath))
				{
					camera.Sdk.ConfigPath = string.Empty;
				}

				if (!string.IsNullOrWhiteSpace(camera.Sdk.ConfigPath))
				{
					camera.Sdk.ConfigPath = NormalizeStoredPathForCurrentJob(camera.Sdk.ConfigPath);
					string fileName = Path.GetFileNameWithoutExtension(camera.Sdk.ConfigPath);
					camera.Sdk.ToolName = NormalizeFileName(fileName, camera.Sdk.ToolName);
				}

				DeleteLegacyCameraVppIfNotUsed(camera);
			}
		}

		private static string NormalizeStoredPathForCurrentJob(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || !HasCurrentJob)
			{
				return path;
			}

			try
			{
				string safeJob = NormalizeFileName(_currentJobName, string.Empty);
				if (string.IsNullOrWhiteSpace(safeJob))
				{
					return path;
				}

				string fullPath = Path.GetFullPath(path);
				string currentJobRoot = Path.GetFullPath(Path.Combine(JobRootContainer, safeJob));
				string legacyJobRoot = Path.GetFullPath(Path.Combine(LegacyJobRootContainer, safeJob));

				if (fullPath.StartsWith(currentJobRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
				{
					return fullPath;
				}

				if (!fullPath.StartsWith(legacyJobRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
				{
					return path;
				}

				string relative = fullPath.Substring(legacyJobRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string mapped = Path.GetFullPath(Path.Combine(currentJobRoot, relative));
				if (File.Exists(mapped))
				{
					return mapped;
				}

				if (relative.StartsWith("Camera" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
					relative.StartsWith("Camera" + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
				{
					string hardwareMapped = Path.GetFullPath(Path.Combine(currentJobRoot, "Hardware", relative));
					if (File.Exists(hardwareMapped))
					{
						return hardwareMapped;
					}
				}
			}
			catch
			{
			}

			return path;
		}

		private static void DeleteLegacyCameraVppIfNotUsed(CameraDeviceConfig camera)
		{
			try
			{
				if (camera == null || string.IsNullOrWhiteSpace(camera.CameraName))
				{
					return;
				}

				string folder = GetCameraFolder(camera.CameraName);
				string legacyFile = Path.Combine(folder, "Camera.vpp");

				if (!File.Exists(legacyFile))
				{
					return;
				}

				string selected = camera.VisionPro == null ? string.Empty : camera.VisionPro.AcqVppPath;

				if (!string.IsNullOrWhiteSpace(selected) &&
					string.Equals(Path.GetFullPath(selected), Path.GetFullPath(legacyFile), StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				File.Delete(legacyFile);
			}
			catch
			{
			}
		}

	}
}
