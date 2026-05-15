using System;
using System.Collections.Generic;
using System.IO;
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
			ToolName = "Cam_Acq";
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
		private static string _currentJobName = "Job_001";

		public static string CurrentJobName
		{
			get { return _currentJobName; }
		}

		public static void SetCurrentJobName(string jobName)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				_currentJobName = "Job_001";
				return;
			}

			_currentJobName = jobName.Trim();
		}

		public static string ProjectRoot
		{
			get { return Path.Combine(Application.StartupPath, "Project"); }
		}

		public static string JobRootFolder
		{
			get { return GetJobRootFolder(_currentJobName); }
		}

		public static string ConfigFolder
		{
			get { return JobRootFolder; }
		}

		public static string ConfigFilePath
		{
			get { return Path.Combine(ConfigFolder, "HardwareConfig.xml"); }
		}

		public static string ImageSourceConfigPath
		{
			get { return Path.Combine(ConfigFolder, "ImageSources.xml"); }
		}

		public static string GetJobRootFolder()
		{
			return GetJobRootFolder(_currentJobName);
		}

		public static string GetJobRootFolder(string jobName)
		{
			string safeJob = NormalizeFileName(jobName, "Job_001");
			string folder = Path.Combine(ProjectRoot, "Job", safeJob);

			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			return folder;
		}

		public static string GetCameraRootFolder()
		{
			return GetCameraRootFolder(_currentJobName);
		}

		public static string GetCameraRootFolder(string jobName)
		{
			string folder = Path.Combine(GetJobRootFolder(jobName), "Camera");

			if (!Directory.Exists(folder))
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
			string safeCamera = NormalizeFileName(cameraName, "Cam1");
			string folder = Path.Combine(GetCameraRootFolder(jobName), safeCamera);

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
			return GetVisionProAcqPath(cameraName, "Default", cameraName + "_Acq");
		}

		public static string GetDefaultVisionProAcqPath(string cameraName, string profileName)
		{
			return GetVisionProAcqPath(cameraName, profileName, cameraName + "_Acq");
		}

		public static string GetVisionProAcqPath(string cameraName, string profileName, string toolName)
		{
			toolName = NormalizeFileName(toolName, cameraName + "_Acq");
			return Path.Combine(GetVisionProFolder(cameraName, profileName), toolName + ".vpp");
		}

		public static string NormalizeFileName(string fileName, string defaultName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = defaultName;
			}

			foreach (char c in Path.GetInvalidFileNameChars())
			{
				fileName = fileName.Replace(c, '_');
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = defaultName;
			}

			return fileName.Trim();
		}

		public static string GetSdkConfigPath(string cameraName, string profileName, CameraSdkBrand brand)
		{
			return GetSdkConfigPath(cameraName, profileName, brand, cameraName + "_" + brand + "_Sdk");
		}

		public static string GetSdkConfigPath(string cameraName, string profileName, CameraSdkBrand brand, string toolName)
		{
			toolName = NormalizeFileName(toolName, cameraName + "_" + brand + "_Sdk");
			return Path.Combine(GetSdkFolder(cameraName, profileName), toolName + ".xml");
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
				if (!Directory.Exists(ConfigFolder))
				{
					Directory.CreateDirectory(ConfigFolder);
				}

				if (!File.Exists(ConfigFilePath))
				{
					HardwareProjectConfig defaultConfig = CreateDefault();
					Save(defaultConfig);
					return defaultConfig;
				}

				XmlSerializer serializer = new XmlSerializer(typeof(HardwareProjectConfig));

				using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read))
				{
					HardwareProjectConfig config = serializer.Deserialize(fs) as HardwareProjectConfig;

					if (config == null)
					{
						config = CreateDefault();
					}

					EnsureCameraFolders(config);
					SaveImageSourceList(config);
					return config;
				}
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
			if (camera == null || camera.Sdk == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(camera.Sdk.ToolName))
			{
				camera.Sdk.ToolName = camera.CameraName + "_" + camera.SdkBrand + "_Sdk";
			}

			string path = GetSdkConfigPath(camera.CameraName, camera.AcqProfileName, camera.SdkBrand, camera.Sdk.ToolName);
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
			if (string.IsNullOrWhiteSpace(cameraName))
			{
				return;
			}

			string folder = GetCameraFolder(cameraName);

			if (!Directory.Exists(folder))
			{
				return;
			}

			Directory.Delete(folder, true);
		}

		public static void SaveImageSourceList(HardwareProjectConfig config)
		{
			ImageSourceConfigList list = new ImageSourceConfigList();

			if (config != null && config.Cameras != null)
			{
				foreach (CameraDeviceConfig cam in config.Cameras)
				{
					if (cam == null || string.IsNullOrWhiteSpace(cam.CameraName))
					{
						continue;
					}

					ImageSourceConfig source = new ImageSourceConfig();
					source.SourceName = cam.CameraName + ".Raw";
					source.CameraName = cam.CameraName;
					source.AcqMode = cam.AcquisitionMode.ToString();
					source.SdkBrand = cam.SdkBrand.ToString();
					source.ProfileName = string.IsNullOrWhiteSpace(cam.AcqProfileName) ? "Default" : cam.AcqProfileName;
					source.OutputImageKey = cam.CameraName + ".Raw";
					source.Enable = cam.Enable;

					if (cam.AcquisitionMode == CameraAcquisitionMode.VPro)
					{
						source.ToolName = cam.VisionPro == null ? string.Empty : cam.VisionPro.ToolName;
						source.ConfigPath = cam.VisionPro == null ? string.Empty : cam.VisionPro.AcqVppPath;
					}
					else
					{
						source.ToolName = cam.Sdk == null ? string.Empty : cam.Sdk.ToolName;
						source.ConfigPath = cam.Sdk == null || string.IsNullOrWhiteSpace(cam.Sdk.ConfigPath)
							? GetSdkConfigPath(cam.CameraName, cam.AcqProfileName, cam.SdkBrand)
							: cam.Sdk.ConfigPath;
					}

					list.Sources.Add(source);
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

		private static HardwareProjectConfig CreateDefault()
		{
			HardwareProjectConfig config = new HardwareProjectConfig();

			CameraDeviceConfig cam1 = new CameraDeviceConfig();
			cam1.CameraName = "Cam1";
			cam1.Enable = true;
			cam1.AcquisitionMode = CameraAcquisitionMode.VPro;
			cam1.SdkBrand = CameraSdkBrand.None;
			cam1.Status = "Disconnected";
			cam1.AcqProfileName = "Default";
			cam1.VisionPro.ToolName = "Camera";
			cam1.VisionPro.AcqVppPath = GetVisionProAcqPath(cam1.CameraName, cam1.AcqProfileName, cam1.VisionPro.ToolName);

			config.Cameras.Add(cam1);

			EnsureCameraFolders(config);
			SaveImageSourceList(config);
			return config;
		}

		private static void EnsureCameraFolders(HardwareProjectConfig config)
		{
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

				if (!Directory.Exists(cameraFolder))
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
					camera.VisionPro.ToolName = "Camera";
				}

				if (string.IsNullOrWhiteSpace(camera.VisionPro.AcqVppPath))
				{
					camera.VisionPro.AcqVppPath = GetVisionProAcqPath(camera.CameraName, camera.AcqProfileName, camera.VisionPro.ToolName);
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
					camera.Sdk.ConfigPath = GetSdkConfigPath(camera.CameraName, camera.AcqProfileName, camera.SdkBrand, camera.Sdk.ToolName);
				}

				if (!string.IsNullOrWhiteSpace(camera.VisionPro.AcqVppPath))
				{
					string fileName = Path.GetFileNameWithoutExtension(camera.VisionPro.AcqVppPath);
					camera.VisionPro.ToolName = NormalizeFileName(fileName, camera.VisionPro.ToolName);
					camera.VisionPro.AcqVppPath = GetVisionProAcqPath(camera.CameraName, camera.AcqProfileName, camera.VisionPro.ToolName);
				}

				if (!string.IsNullOrWhiteSpace(camera.Sdk.ConfigPath))
				{
					string fileName = Path.GetFileNameWithoutExtension(camera.Sdk.ConfigPath);
					camera.Sdk.ToolName = NormalizeFileName(fileName, camera.Sdk.ToolName);
					camera.Sdk.ConfigPath = GetSdkConfigPath(camera.CameraName, camera.AcqProfileName, camera.SdkBrand, camera.Sdk.ToolName);
				}
			}
		}
	}
}
