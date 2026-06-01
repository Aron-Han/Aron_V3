using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Aron_V3
{
	public enum CommunicationType
	{
		TcpIp = 0,
		Profinet = 1,
		S7 = 2
	}

	public enum CommVariableDataType
	{
		Float = 0,
		Double = 1,
		ShortInt = 2,
		LongInt = 3,
		Bool = 4,
		String = 5,
		Bytes = 6
	}

	public enum TcpIpPayloadMode
	{
		String = 0,
		Byte = 1
	}

	public enum CommByteOrder
	{
		BigEndian = 0,
		LittleEndian = 1
	}

	public enum CommunicationInstanceKind
	{
		TcpIpServer = 0,
		TcpIpClient = 1,
		Profinet = 2,
		S7 = 3
	}

	public class CommInputVariable
	{
		[XmlAttribute]
		public string Name { get; set; }

		// TCP/IP 和 S7 使用：
		// 是否作为触发源。
		// Profinet 不使用这个字段，Profinet 由 EngineName 决定。
		[XmlAttribute]
		public bool UseAsTrigger { get; set; }

		// 是否作为位置号来源。
		// TCP/IP 和 S7 可通过勾选输入变量作为位置号。
		// Profinet 也可勾选 PositionCode 等输入变量作为位置号来源。
		[XmlAttribute]
		public bool UseAsPosition { get; set; }

		// Profinet 使用：
		// 固定选择 engine0 ~ engine3。
		[XmlAttribute]
		public string EngineName { get; set; }

		[XmlAttribute]
		public CommVariableDataType DataType { get; set; }

		[XmlAttribute]
		public int ByteOffset { get; set; }

		[XmlAttribute]
		public int BitOffset { get; set; }

		[XmlAttribute]
		public int Length { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		[XmlAttribute]
		public string GlobalVariableName { get; set; }

		public CommInputVariable()
		{
			Name = string.Empty;
			UseAsTrigger = false;
			UseAsPosition = false;
			EngineName = "engine0";
			DataType = CommVariableDataType.Bool;
			ByteOffset = 0;
			BitOffset = 0;
			Length = 1;
			Remark = string.Empty;
			GlobalVariableName = string.Empty;
		}
	}

	public class CommOutputVariable
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public CommVariableDataType DataType { get; set; }

		[XmlAttribute]
		public int ByteOffset { get; set; }

		[XmlAttribute]
		public int BitOffset { get; set; }

		[XmlAttribute]
		public int Length { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		[XmlAttribute]
		public string GlobalVariableName { get; set; }

		public CommOutputVariable()
		{
			Name = string.Empty;
			DataType = CommVariableDataType.Bool;
			ByteOffset = 0;
			BitOffset = 0;
			Length = 1;
			Remark = string.Empty;
			GlobalVariableName = string.Empty;
		}
	}

	public class CommunicationHeartbeatConfig
	{
		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public string OutputName { get; set; }

		[XmlAttribute]
		public string HeartbeatText { get; set; }

		[XmlAttribute]
		public int IntervalMs { get; set; }

		public CommunicationHeartbeatConfig()
		{
			Enabled = false;
			OutputName = string.Empty;
			HeartbeatText = "1";
			IntervalMs = 1000;
		}
	}

	public class CommunicationPositionOption
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string ExpectedValue { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		public CommunicationPositionOption()
		{
			Name = string.Empty;
			ExpectedValue = "1";
			Remark = string.Empty;
		}
	}

	public class CommunicationCustomTriggerOption
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string ExpectedValue { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		public CommunicationCustomTriggerOption()
		{
			Name = string.Empty;
			ExpectedValue = "1";
			Remark = string.Empty;
		}
	}

	public class ProgramJobMapItem
	{
		[XmlAttribute]
		public string ProgramNo { get; set; }

		[XmlAttribute]
		public string JobName { get; set; }

		public ProgramJobMapItem()
		{
			ProgramNo = "1";
			JobName = string.Empty;
		}
	}

	public class CommunicationChannelConfig
	{
		[XmlAttribute]
		public string ChannelName { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public string TriggerName { get; set; }

		[XmlAttribute]
		public string TriggerExpectedValue { get; set; }

		[XmlAttribute]
		public string TriggerGlobalVariableName { get; set; }

		[XmlAttribute]
		public string CustomTriggerGlobalVariableName { get; set; }

		[XmlAttribute]
		public string CustomTriggerExpectedValue { get; set; }

		[XmlArray("CustomTriggers")]
		[XmlArrayItem("Trigger")]
		public List<CommunicationCustomTriggerOption> CustomTriggers { get; set; }

		[XmlAttribute]
		public string PositionSourceName { get; set; }

		[XmlAttribute]
		public string PositionGlobalVariableName { get; set; }

		[XmlAttribute]
		public string ProgramNoAddressName { get; set; }

		[XmlAttribute]
		public string ProgramSwitchEnableName { get; set; }

		[XmlAttribute]
		public string ProgramSwitchDoneName { get; set; }

		[XmlAttribute]
		public string ProgramSwitchFailName { get; set; }

		[XmlAttribute]
		public string ChannelReadyOutputName { get; set; }

		[XmlAttribute]
		public string ChannelReadyBusyValue { get; set; }

		[XmlAttribute]
		public string ChannelReadyDoneValue { get; set; }

		[XmlAttribute]
		public string ProgramNoOutputName { get; set; }

		[XmlArray("PositionOptions")]
		[XmlArrayItem("Position")]
		public List<CommunicationPositionOption> PositionOptions { get; set; }

		[XmlArray("ProgramJobMap")]
		[XmlArrayItem("Map")]
		public List<ProgramJobMapItem> ProgramJobMap { get; set; }

		public CommunicationChannelConfig()
		{
			ChannelName = "Channel01";
			Enabled = true;
			TriggerName = "Trigger";
			TriggerExpectedValue = "1";
			TriggerGlobalVariableName = string.Empty;
			CustomTriggerGlobalVariableName = string.Empty;
			CustomTriggerExpectedValue = "1";
			CustomTriggers = new List<CommunicationCustomTriggerOption>();
			PositionSourceName = "Not Use";
			PositionGlobalVariableName = string.Empty;
			ProgramNoAddressName = "JobID";
			ProgramSwitchEnableName = string.Empty;
			ProgramSwitchDoneName = string.Empty;
			ProgramSwitchFailName = string.Empty;
			ChannelReadyOutputName = string.Empty;
			ChannelReadyBusyValue = "0";
			ChannelReadyDoneValue = "1";
			ProgramNoOutputName = string.Empty;
			PositionOptions = new List<CommunicationPositionOption>();
			ProgramJobMap = new List<ProgramJobMapItem>();
		}
	}

	public class TcpIpConfig
	{
		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public bool IsServer { get; set; }

		[XmlAttribute]
		public string LocalIP { get; set; }

		[XmlAttribute]
		public int LocalPort { get; set; }

		[XmlAttribute]
		public string RemoteIP { get; set; }

		[XmlAttribute]
		public int RemotePort { get; set; }

		[XmlAttribute]
		public TcpIpPayloadMode PayloadMode { get; set; }

		[XmlAttribute]
		public CommByteOrder ByteOrder { get; set; }

		[XmlArray("InputVariables")]
		[XmlArrayItem("Input")]
		public List<CommInputVariable> InputVariables { get; set; }

		[XmlArray("OutputVariables")]
		[XmlArrayItem("Output")]
		public List<CommOutputVariable> OutputVariables { get; set; }

		[XmlArray("Channels")]
		[XmlArrayItem("Channel")]
		public List<CommunicationChannelConfig> Channels { get; set; }

		public CommunicationHeartbeatConfig Heartbeat { get; set; }

		public TcpIpConfig()
		{
			Enabled = false;
			IsServer = true;
			LocalIP = "0.0.0.0";
			LocalPort = 5000;
			RemoteIP = "192.168.1.10";
			RemotePort = 5000;
			PayloadMode = TcpIpPayloadMode.String;
			ByteOrder = CommByteOrder.BigEndian;
			InputVariables = new List<CommInputVariable>();
			OutputVariables = new List<CommOutputVariable>();
			Channels = new List<CommunicationChannelConfig>();
			Heartbeat = new CommunicationHeartbeatConfig();
		}
	}

	public class ProfinetConfig
	{
		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public string DeviceName { get; set; }

		[XmlAttribute]
		public string StationName { get; set; }

		[XmlAttribute]
		public string ConnectionStatus { get; set; }

		[XmlAttribute]
		public bool UseGsdFixedMapping { get; set; }

		[XmlArray("InputVariables")]
		[XmlArrayItem("Input")]
		public List<CommInputVariable> InputVariables { get; set; }

		[XmlArray("OutputVariables")]
		[XmlArrayItem("Output")]
		public List<CommOutputVariable> OutputVariables { get; set; }

		[XmlArray("Channels")]
		[XmlArrayItem("Channel")]
		public List<CommunicationChannelConfig> Channels { get; set; }

		public CommunicationHeartbeatConfig Heartbeat { get; set; }

		public ProfinetConfig()
		{
			Enabled = false;
			DeviceName = "CC24";
			StationName = string.Empty;
			ConnectionStatus = "Disconnected";
			UseGsdFixedMapping = true;
			InputVariables = new List<CommInputVariable>();
			OutputVariables = new List<CommOutputVariable>();
			Channels = new List<CommunicationChannelConfig>();
			Heartbeat = new CommunicationHeartbeatConfig();
		}
	}

	public class S7Config
	{
		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public string PlcIP { get; set; }

		[XmlAttribute]
		public int Rack { get; set; }

		[XmlAttribute]
		public int Slot { get; set; }

		[XmlAttribute]
		public int InputDB { get; set; }

		[XmlAttribute]
		public int OutputDB { get; set; }

		[XmlAttribute]
		public int InputStartByte { get; set; }

		[XmlAttribute]
		public int OutputStartByte { get; set; }

		[XmlArray("InputVariables")]
		[XmlArrayItem("Input")]
		public List<CommInputVariable> InputVariables { get; set; }

		[XmlArray("OutputVariables")]
		[XmlArrayItem("Output")]
		public List<CommOutputVariable> OutputVariables { get; set; }

		[XmlArray("Channels")]
		[XmlArrayItem("Channel")]
		public List<CommunicationChannelConfig> Channels { get; set; }

		public CommunicationHeartbeatConfig Heartbeat { get; set; }

		public S7Config()
		{
			Enabled = false;
			PlcIP = "192.168.1.100";
			Rack = 0;
			Slot = 1;
			InputDB = 1;
			OutputDB = 1;
			InputStartByte = 0;
			OutputStartByte = 0;
			InputVariables = new List<CommInputVariable>();
			OutputVariables = new List<CommOutputVariable>();
			Channels = new List<CommunicationChannelConfig>();
			Heartbeat = new CommunicationHeartbeatConfig();
		}
	}

	public class CommunicationInstanceConfig
	{
		[XmlAttribute]
		public string InstanceName { get; set; }

		[XmlAttribute]
		public CommunicationType CommunicationType { get; set; }

		[XmlAttribute]
		public CommunicationInstanceKind InstanceKind { get; set; }

		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		public TcpIpConfig TcpIp { get; set; }
		public ProfinetConfig Profinet { get; set; }
		public S7Config S7 { get; set; }

		[XmlArray("Channels")]
		[XmlArrayItem("Channel")]
		public List<CommunicationChannelConfig> Channels { get; set; }

		public CommunicationHeartbeatConfig Heartbeat { get; set; }

		public CommunicationInstanceConfig()
		{
			InstanceName = string.Empty;
			CommunicationType = CommunicationType.TcpIp;
			InstanceKind = CommunicationInstanceKind.TcpIpServer;
			Enabled = false;
			Remark = string.Empty;
			TcpIp = new TcpIpConfig();
			Profinet = new ProfinetConfig();
			S7 = new S7Config();
			Channels = new List<CommunicationChannelConfig>();
			Heartbeat = new CommunicationHeartbeatConfig();
		}
	}

	[XmlRoot("CommunicationConfig")]
	public class CommunicationConfig
	{
		[XmlAttribute]
		public CommunicationType SelectedType { get; set; }

		public TcpIpConfig TcpIp { get; set; }
		public ProfinetConfig Profinet { get; set; }
		public S7Config S7 { get; set; }

		[XmlArray("Instances")]
		[XmlArrayItem("Instance")]
		public List<CommunicationInstanceConfig> Instances { get; set; }

		public CommunicationConfig()
		{
			SelectedType = CommunicationType.TcpIp;
			TcpIp = new TcpIpConfig();
			Profinet = new ProfinetConfig();
			S7 = new S7Config();
			Instances = new List<CommunicationInstanceConfig>();
		}
	}


	public static class CommunicationConfigChangedHub
	{
		public static event EventHandler ConfigChanged;

		public static void RaiseConfigChanged()
		{
			EventHandler handler = ConfigChanged;

			if (handler != null)
			{
				handler(null, EventArgs.Empty);
			}
		}
	}

	public static class CommunicationConfigStore
	{
		public static string ConfigFile
		{
			get
			{
				return Path.Combine(FlowConfigStore.PathManager.CommunicationConfigRoot, "CommunicationConfig.xml");
			}
		}

		public static CommunicationConfig LoadOrCreateDefault()
		{
			FlowConfigStore.PathManager.EnsureProjectFolders();

			CommunicationConfig config = XmlConfigHelper.Load<CommunicationConfig>(ConfigFile);
			Normalize(config);

			if (IsEmpty(config))
			{
				config = CreateDefault();
				Save(config);
			}

			return config;
		}

		public static void Save(CommunicationConfig config)
		{
			if (config == null)
			{
				config = new CommunicationConfig();
			}

			Normalize(config);
			FlowConfigStore.PathManager.EnsureProjectFolders();
			XmlConfigHelper.Save(ConfigFile, config);

			DiagnosticLogStore.Append(
				DiagnosticLogLevel.Info,
				"Config",
				"Communication config saved.",
				new Dictionary<string, string> { { "path", ConfigFile } });

			CommunicationConfigChangedHub.RaiseConfigChanged();
		}

		private static bool IsEmpty(CommunicationConfig config)
		{
			if (config == null)
			{
				return true;
			}

			return config.TcpIp.InputVariables.Count == 0 &&
				   config.TcpIp.OutputVariables.Count == 0 &&
				   config.Profinet.InputVariables.Count == 0 &&
				   config.Profinet.OutputVariables.Count == 0 &&
				   config.S7.InputVariables.Count == 0 &&
				   config.S7.OutputVariables.Count == 0 &&
				   (config.Instances == null || config.Instances.Count == 0);
		}

		private static void Normalize(CommunicationConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (config.TcpIp == null) config.TcpIp = new TcpIpConfig();
			if (config.Profinet == null) config.Profinet = new ProfinetConfig();
			if (config.S7 == null) config.S7 = new S7Config();
			if (config.Instances == null) config.Instances = new List<CommunicationInstanceConfig>();

			if (config.TcpIp.InputVariables == null) config.TcpIp.InputVariables = new List<CommInputVariable>();
			if (config.TcpIp.OutputVariables == null) config.TcpIp.OutputVariables = new List<CommOutputVariable>();
			if (config.TcpIp.Channels == null) config.TcpIp.Channels = new List<CommunicationChannelConfig>();
			if (config.TcpIp.Heartbeat == null) config.TcpIp.Heartbeat = new CommunicationHeartbeatConfig();

			if (config.Profinet.InputVariables == null) config.Profinet.InputVariables = new List<CommInputVariable>();
			if (config.Profinet.OutputVariables == null) config.Profinet.OutputVariables = new List<CommOutputVariable>();
			if (config.Profinet.Channels == null) config.Profinet.Channels = new List<CommunicationChannelConfig>();
			if (config.Profinet.Heartbeat == null) config.Profinet.Heartbeat = new CommunicationHeartbeatConfig();

			if (config.S7.InputVariables == null) config.S7.InputVariables = new List<CommInputVariable>();
			if (config.S7.OutputVariables == null) config.S7.OutputVariables = new List<CommOutputVariable>();
			if (config.S7.Channels == null) config.S7.Channels = new List<CommunicationChannelConfig>();
			if (config.S7.Heartbeat == null) config.S7.Heartbeat = new CommunicationHeartbeatConfig();

			if (config.Profinet.InputVariables.Count == 0 && config.Profinet.OutputVariables.Count == 0)
			{
				CreateDefaultProfinetMapping(config.Profinet);
			}

			EnsureEngineName(config.Profinet.InputVariables);
			EnsureDefaultChannels(config);
			EnsureDefaultInstances(config);
			NormalizeInstances(config);
			NormalizeHeartbeat(config.TcpIp.Heartbeat);
			NormalizeHeartbeat(config.Profinet.Heartbeat);
			NormalizeHeartbeat(config.S7.Heartbeat);
		}

		private static void EnsureDefaultInstances(CommunicationConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (config.Instances == null)
			{
				config.Instances = new List<CommunicationInstanceConfig>();
			}

			bool instanceListWasEmpty = config.Instances.Count == 0;

			if (instanceListWasEmpty)
			{
				EnsureProtocolInstance(
					config.Instances,
					"TCPIP_01",
					CommunicationType.TcpIp,
					config.TcpIp == null || config.TcpIp.IsServer ? CommunicationInstanceKind.TcpIpServer : CommunicationInstanceKind.TcpIpClient,
					config.TcpIp,
					null,
					null);

				EnsureProtocolInstance(
					config.Instances,
					"S7_01",
					CommunicationType.S7,
					CommunicationInstanceKind.S7,
					null,
					null,
					config.S7);
			}

		}

		private static void EnsureProtocolInstance(
			List<CommunicationInstanceConfig> instances,
			string instanceName,
			CommunicationType type,
			CommunicationInstanceKind kind,
			TcpIpConfig tcpIp,
			ProfinetConfig profinet,
			S7Config s7)
		{
			if (instances == null)
			{
				return;
			}

			CommunicationInstanceConfig instance = instances.FirstOrDefault(x =>
				x != null && string.Equals(x.InstanceName, instanceName, StringComparison.OrdinalIgnoreCase));

			if (instance == null)
			{
				instance = new CommunicationInstanceConfig();
				instance.InstanceName = instanceName;
				instances.Add(instance);
			}

			instance.CommunicationType = type;
			instance.InstanceKind = kind;

			if (type == CommunicationType.TcpIp && tcpIp != null)
			{
				instance.Enabled = tcpIp.Enabled;
				instance.TcpIp = tcpIp;
				instance.Channels = tcpIp.Channels;
				instance.Heartbeat = tcpIp.Heartbeat;
			}
			else if (type == CommunicationType.Profinet && profinet != null)
			{
				instance.Enabled = profinet.Enabled;
				instance.Profinet = profinet;
				instance.Channels = profinet.Channels;
				instance.Heartbeat = profinet.Heartbeat;
			}
			else if (type == CommunicationType.S7 && s7 != null)
			{
				instance.Enabled = s7.Enabled;
				instance.S7 = s7;
				instance.Channels = s7.Channels;
				instance.Heartbeat = s7.Heartbeat;
			}
		}

		private static void NormalizeInstances(CommunicationConfig config)
		{
			if (config == null || config.Instances == null)
			{
				return;
			}

			HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			int index = 1;

			foreach (CommunicationInstanceConfig instance in config.Instances)
			{
				if (instance == null)
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(instance.InstanceName))
				{
					instance.InstanceName = GetDefaultInstanceName(instance.CommunicationType, index);
				}

				string baseName = instance.InstanceName.Trim();
				string uniqueName = baseName;
				int suffix = 2;

				while (usedNames.Contains(uniqueName))
				{
					uniqueName = baseName + "_" + suffix.ToString("00");
					suffix++;
				}

				instance.InstanceName = uniqueName;
				usedNames.Add(uniqueName);
				index++;

				if (instance.Remark == null) instance.Remark = string.Empty;
				if (instance.TcpIp == null) instance.TcpIp = new TcpIpConfig();
				if (instance.Profinet == null) instance.Profinet = new ProfinetConfig();
				if (instance.S7 == null) instance.S7 = new S7Config();
				if (instance.Channels == null) instance.Channels = new List<CommunicationChannelConfig>();
				if (instance.Heartbeat == null) instance.Heartbeat = new CommunicationHeartbeatConfig();

				NormalizeInstanceKind(instance);
				NormalizeInstanceChildren(instance);
			}
		}

		private static string GetDefaultInstanceName(CommunicationType type, int index)
		{
			string prefix = type == CommunicationType.TcpIp
				? "TCPIP"
				: type == CommunicationType.Profinet ? "Profinet" : "S7";

			return prefix + "_" + Math.Max(1, index).ToString("00");
		}

		private static void NormalizeInstanceKind(CommunicationInstanceConfig instance)
		{
			if (instance == null)
			{
				return;
			}

			if (instance.CommunicationType == CommunicationType.TcpIp)
			{
				instance.InstanceKind = instance.TcpIp != null && !instance.TcpIp.IsServer
					? CommunicationInstanceKind.TcpIpClient
					: CommunicationInstanceKind.TcpIpServer;
			}
			else if (instance.CommunicationType == CommunicationType.Profinet)
			{
				instance.InstanceKind = CommunicationInstanceKind.Profinet;
			}
			else
			{
				instance.InstanceKind = CommunicationInstanceKind.S7;
			}
		}

		private static void NormalizeInstanceChildren(CommunicationInstanceConfig instance)
		{
			if (instance == null)
			{
				return;
			}

			if (instance.CommunicationType == CommunicationType.TcpIp)
			{
				if (instance.TcpIp.InputVariables == null) instance.TcpIp.InputVariables = new List<CommInputVariable>();
				if (instance.TcpIp.OutputVariables == null) instance.TcpIp.OutputVariables = new List<CommOutputVariable>();
				if (instance.TcpIp.Channels == null) instance.TcpIp.Channels = new List<CommunicationChannelConfig>();
				if (instance.TcpIp.Heartbeat == null) instance.TcpIp.Heartbeat = new CommunicationHeartbeatConfig();
				instance.TcpIp.Enabled = instance.Enabled;
				instance.TcpIp.IsServer = instance.InstanceKind != CommunicationInstanceKind.TcpIpClient;
				instance.Channels = instance.TcpIp.Channels;
				instance.Heartbeat = instance.TcpIp.Heartbeat;
				NormalizeChannels(instance.TcpIp.Channels, instance.TcpIp.InputVariables);
				NormalizeHeartbeat(instance.TcpIp.Heartbeat);
			}
			else if (instance.CommunicationType == CommunicationType.Profinet)
			{
				if (instance.Profinet.InputVariables == null) instance.Profinet.InputVariables = new List<CommInputVariable>();
				if (instance.Profinet.OutputVariables == null) instance.Profinet.OutputVariables = new List<CommOutputVariable>();
				if (instance.Profinet.Channels == null) instance.Profinet.Channels = new List<CommunicationChannelConfig>();
				if (instance.Profinet.Heartbeat == null) instance.Profinet.Heartbeat = new CommunicationHeartbeatConfig();
				instance.Profinet.Enabled = instance.Enabled;
				instance.Channels = instance.Profinet.Channels;
				instance.Heartbeat = instance.Profinet.Heartbeat;
				EnsureEngineName(instance.Profinet.InputVariables);
				NormalizeChannels(instance.Profinet.Channels, instance.Profinet.InputVariables);
				NormalizeHeartbeat(instance.Profinet.Heartbeat);
			}
			else if (instance.CommunicationType == CommunicationType.S7)
			{
				if (instance.S7.InputVariables == null) instance.S7.InputVariables = new List<CommInputVariable>();
				if (instance.S7.OutputVariables == null) instance.S7.OutputVariables = new List<CommOutputVariable>();
				if (instance.S7.Channels == null) instance.S7.Channels = new List<CommunicationChannelConfig>();
				if (instance.S7.Heartbeat == null) instance.S7.Heartbeat = new CommunicationHeartbeatConfig();
				instance.S7.Enabled = instance.Enabled;
				instance.Channels = instance.S7.Channels;
				instance.Heartbeat = instance.S7.Heartbeat;
				NormalizeChannels(instance.S7.Channels, instance.S7.InputVariables);
				NormalizeHeartbeat(instance.S7.Heartbeat);
			}
		}

		private static void NormalizeHeartbeat(CommunicationHeartbeatConfig heartbeat)
		{
			if (heartbeat == null)
			{
				return;
			}

			if (heartbeat.OutputName == null)
			{
				heartbeat.OutputName = string.Empty;
			}

			if (heartbeat.HeartbeatText == null)
			{
				heartbeat.HeartbeatText = string.Empty;
			}

			if (heartbeat.IntervalMs <= 0)
			{
				heartbeat.IntervalMs = 1000;
			}
		}

		private static void EnsureDefaultChannels(CommunicationConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (config.TcpIp != null && config.TcpIp.Channels.Count == 0)
			{
				config.TcpIp.Channels.Add(CreateDefaultChannel("Channel01", config.TcpIp.InputVariables));
			}

			if (config.S7 != null && config.S7.Channels.Count == 0)
			{
				config.S7.Channels.Add(CreateDefaultChannel("Channel01", config.S7.InputVariables));
			}

			if (config.Profinet != null)
			{
				for (int i = 1; i <= 4; i++)
				{
					string channelName = "Channel" + i.ToString();
					if (!config.Profinet.Channels.Any(x => x != null && string.Equals(x.ChannelName, channelName, StringComparison.OrdinalIgnoreCase)))
					{
						config.Profinet.Channels.Add(CreateDefaultChannel(channelName, config.Profinet.InputVariables));
					}
				}
			}

			NormalizeChannels(config.TcpIp == null ? null : config.TcpIp.Channels, config.TcpIp == null ? null : config.TcpIp.InputVariables);
			NormalizeChannels(config.Profinet == null ? null : config.Profinet.Channels, config.Profinet == null ? null : config.Profinet.InputVariables);
			NormalizeChannels(config.S7 == null ? null : config.S7.Channels, config.S7 == null ? null : config.S7.InputVariables);
		}

		private static CommunicationChannelConfig CreateDefaultChannel(string channelName, List<CommInputVariable> inputs)
		{
			CommunicationChannelConfig channel = new CommunicationChannelConfig();
			channel.ChannelName = channelName;

			CommInputVariable trigger = inputs == null ? null : inputs.FirstOrDefault(x => x != null && x.UseAsTrigger);
			CommInputVariable position = inputs == null ? null : inputs.FirstOrDefault(x => x != null && x.UseAsPosition);
			CommInputVariable program = inputs == null ? null : inputs.FirstOrDefault(x =>
				x != null &&
				(x.Name.IndexOf("job", StringComparison.OrdinalIgnoreCase) >= 0 ||
				 x.Name.IndexOf("program", StringComparison.OrdinalIgnoreCase) >= 0));

			channel.TriggerName = trigger == null ? "Trigger" : trigger.Name;
			channel.TriggerExpectedValue = "1";
			channel.PositionSourceName = position == null ? "Not Use" : position.Name;
			channel.ProgramNoAddressName = program == null ? "JobID" : program.Name;
			channel.PositionOptions.Add(new CommunicationPositionOption
			{
				Name = "Not Use",
				ExpectedValue = string.Empty
			});
			if (position != null)
			{
				channel.PositionOptions.Add(new CommunicationPositionOption
				{
					Name = position.Name,
					ExpectedValue = "1"
				});
			}

			return channel;
		}

		private static void NormalizeChannels(List<CommunicationChannelConfig> channels, List<CommInputVariable> inputs)
		{
			if (channels == null)
			{
				return;
			}

			foreach (CommunicationChannelConfig channel in channels)
			{
				if (channel == null)
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(channel.ChannelName)) channel.ChannelName = "Channel01";
				if (string.IsNullOrWhiteSpace(channel.TriggerName)) channel.TriggerName = "Trigger";
				if (string.IsNullOrWhiteSpace(channel.TriggerExpectedValue)) channel.TriggerExpectedValue = "1";
				if (channel.TriggerGlobalVariableName == null) channel.TriggerGlobalVariableName = string.Empty;
				if (channel.CustomTriggerGlobalVariableName == null) channel.CustomTriggerGlobalVariableName = string.Empty;
				if (string.IsNullOrWhiteSpace(channel.CustomTriggerExpectedValue)) channel.CustomTriggerExpectedValue = "1";
				if (channel.CustomTriggers == null) channel.CustomTriggers = new List<CommunicationCustomTriggerOption>();
				MigrateLegacyCustomTrigger(channel);
				if (string.IsNullOrWhiteSpace(channel.PositionSourceName)) channel.PositionSourceName = "Not Use";
				if (channel.PositionGlobalVariableName == null) channel.PositionGlobalVariableName = string.Empty;
				if (string.IsNullOrWhiteSpace(channel.ProgramNoAddressName)) channel.ProgramNoAddressName = "JobID";
				if (channel.ProgramSwitchEnableName == null) channel.ProgramSwitchEnableName = string.Empty;
				if (channel.ProgramSwitchDoneName == null) channel.ProgramSwitchDoneName = string.Empty;
				if (channel.ProgramSwitchFailName == null) channel.ProgramSwitchFailName = string.Empty;
				if (channel.ChannelReadyOutputName == null) channel.ChannelReadyOutputName = string.Empty;
				if (string.IsNullOrWhiteSpace(channel.ChannelReadyBusyValue)) channel.ChannelReadyBusyValue = "0";
				if (string.IsNullOrWhiteSpace(channel.ChannelReadyDoneValue)) channel.ChannelReadyDoneValue = "1";
				if (channel.ProgramNoOutputName == null) channel.ProgramNoOutputName = string.Empty;
				if (channel.PositionOptions == null) channel.PositionOptions = new List<CommunicationPositionOption>();
				if (channel.ProgramJobMap == null) channel.ProgramJobMap = new List<ProgramJobMapItem>();
				if (!channel.PositionOptions.Any(x => x != null && string.Equals(x.Name, "Not Use", StringComparison.OrdinalIgnoreCase)))
				{
					channel.PositionOptions.Insert(0, new CommunicationPositionOption { Name = "Not Use", ExpectedValue = string.Empty });
				}

				foreach (CommunicationPositionOption option in channel.PositionOptions)
				{
					if (option == null) continue;
					if (option.Name == null) option.Name = string.Empty;
					if (option.ExpectedValue == null) option.ExpectedValue = string.Empty;
					if (option.Remark == null) option.Remark = string.Empty;
				}

				for (int i = channel.CustomTriggers.Count - 1; i >= 0; i--)
				{
					CommunicationCustomTriggerOption option = channel.CustomTriggers[i];
					if (option == null || string.IsNullOrWhiteSpace(option.Name))
					{
						channel.CustomTriggers.RemoveAt(i);
						continue;
					}

					option.Name = option.Name.Trim();
					if (option.ExpectedValue == null) option.ExpectedValue = string.Empty;
					if (option.Remark == null) option.Remark = string.Empty;
				}

				CommunicationCustomTriggerOption firstCustomTrigger = channel.CustomTriggers.FirstOrDefault();
				if (firstCustomTrigger != null)
				{
					channel.CustomTriggerGlobalVariableName = firstCustomTrigger.Name;
					channel.CustomTriggerExpectedValue = string.IsNullOrWhiteSpace(firstCustomTrigger.ExpectedValue)
						? "1"
						: firstCustomTrigger.ExpectedValue;
				}
			}
		}

		private static void MigrateLegacyCustomTrigger(CommunicationChannelConfig channel)
		{
			if (channel == null || channel.CustomTriggers == null)
			{
				return;
			}

			if (channel.CustomTriggers.Count > 0 ||
				string.IsNullOrWhiteSpace(channel.CustomTriggerGlobalVariableName))
			{
				return;
			}

			channel.CustomTriggers.Add(new CommunicationCustomTriggerOption
			{
				Name = channel.CustomTriggerGlobalVariableName.Trim(),
				ExpectedValue = string.IsNullOrWhiteSpace(channel.CustomTriggerExpectedValue)
					? "1"
					: channel.CustomTriggerExpectedValue.Trim()
			});
		}

		private static void EnsureEngineName(List<CommInputVariable> list)
		{
			if (list == null)
			{
				return;
			}

			foreach (CommInputVariable item in list)
			{
				if (string.IsNullOrEmpty(item.EngineName))
				{
					item.EngineName = "engine0";
				}
			}
		}

		private static CommunicationConfig CreateDefault()
		{
			CommunicationConfig config = new CommunicationConfig();
			config.SelectedType = CommunicationType.TcpIp;

			config.TcpIp.InputVariables.Add(new CommInputVariable
			{
				Name = "Trigger",
				UseAsTrigger = true,
				UseAsPosition = false,
				EngineName = string.Empty,
				DataType = CommVariableDataType.Bool,
				ByteOffset = 0,
				BitOffset = 0,
				Length = 1,
				Remark = "TCP input trigger"
			});

			config.TcpIp.InputVariables.Add(new CommInputVariable
			{
				Name = "JobID",
				UseAsTrigger = false,
				UseAsPosition = true,
				EngineName = string.Empty,
				DataType = CommVariableDataType.ShortInt,
				ByteOffset = 2,
				BitOffset = 0,
				Length = 2,
				Remark = "TCP program number"
			});

			config.TcpIp.OutputVariables.Add(new CommOutputVariable
			{
				Name = "ResultOK",
				DataType = CommVariableDataType.Bool,
				ByteOffset = 0,
				BitOffset = 0,
				Length = 1,
				Remark = "TCP output result"
			});

			config.TcpIp.OutputVariables.Add(new CommOutputVariable
			{
				Name = "ErrorCode",
				DataType = CommVariableDataType.ShortInt,
				ByteOffset = 2,
				BitOffset = 0,
				Length = 2,
				Remark = "TCP error code"
			});

			CreateDefaultProfinetMapping(config.Profinet);

			config.S7.InputVariables.Add(new CommInputVariable
			{
				Name = "Trigger",
				UseAsTrigger = true,
				UseAsPosition = false,
				EngineName = string.Empty,
				DataType = CommVariableDataType.Bool,
				ByteOffset = 0,
				BitOffset = 0,
				Length = 1,
				Remark = "S7 DB input trigger"
			});

			config.S7.InputVariables.Add(new CommInputVariable
			{
				Name = "JobID",
				UseAsTrigger = false,
				UseAsPosition = true,
				EngineName = string.Empty,
				DataType = CommVariableDataType.ShortInt,
				ByteOffset = 2,
				BitOffset = 0,
				Length = 2,
				Remark = "S7 DB program number"
			});

			config.S7.OutputVariables.Add(new CommOutputVariable
			{
				Name = "ResultOK",
				DataType = CommVariableDataType.Bool,
				ByteOffset = 0,
				BitOffset = 0,
				Length = 1,
				Remark = "S7 DB output result"
			});

			config.S7.OutputVariables.Add(new CommOutputVariable
			{
				Name = "ErrorCode",
				DataType = CommVariableDataType.ShortInt,
				ByteOffset = 2,
				BitOffset = 0,
				Length = 2,
				Remark = "S7 DB error code"
			});

			return config;
		}

		private static void CreateDefaultProfinetMapping(ProfinetConfig profinet)
		{
			if (profinet == null)
			{
				return;
			}

			profinet.UseGsdFixedMapping = true;

			profinet.InputVariables.Clear();
			profinet.OutputVariables.Clear();

			profinet.InputVariables.Add(new CommInputVariable
			{
				Name = "EngineSelect",
				UseAsTrigger = false,
				EngineName = "engine0",
				DataType = CommVariableDataType.ShortInt,
				ByteOffset = 0,
				BitOffset = 0,
				Length = 2,
				Remark = "Select engine0~engine3"
			});

			profinet.InputVariables.Add(new CommInputVariable
			{
				Name = "Clear",
				UseAsTrigger = false,
				EngineName = "engine0",
				DataType = CommVariableDataType.Bool,
				ByteOffset = 2,
				BitOffset = 0,
				Length = 1,
				Remark = "GSD fixed input"
			});

			profinet.InputVariables.Add(new CommInputVariable
			{
				Name = "Trigger",
				UseAsTrigger = false,
				EngineName = "engine0",
				DataType = CommVariableDataType.Bool,
				ByteOffset = 2,
				BitOffset = 1,
				Length = 1,
				Remark = "GSD fixed trigger"
			});

			profinet.InputVariables.Add(new CommInputVariable
			{
				Name = "JobID",
				UseAsTrigger = false,
				EngineName = "engine0",
				DataType = CommVariableDataType.ShortInt,
				ByteOffset = 4,
				BitOffset = 0,
				Length = 2,
				Remark = "GSD fixed program number"
			});

			profinet.InputVariables.Add(new CommInputVariable
			{
				Name = "PositionCode",
				UseAsTrigger = false,
				UseAsPosition = true,
				EngineName = "engine0",
				DataType = CommVariableDataType.ShortInt,
				ByteOffset = 6,
				BitOffset = 0,
				Length = 2,
				Remark = "GSD fixed position code"
			});

			profinet.OutputVariables.Add(new CommOutputVariable
			{
				Name = "Busy",
				DataType = CommVariableDataType.Bool,
				ByteOffset = 0,
				BitOffset = 0,
				Length = 1,
				Remark = "GSD fixed output"
			});

			profinet.OutputVariables.Add(new CommOutputVariable
			{
				Name = "Done",
				DataType = CommVariableDataType.Bool,
				ByteOffset = 0,
				BitOffset = 1,
				Length = 1,
				Remark = "GSD fixed output"
			});

			profinet.OutputVariables.Add(new CommOutputVariable
			{
				Name = "ResultOK",
				DataType = CommVariableDataType.Bool,
				ByteOffset = 0,
				BitOffset = 2,
				Length = 1,
				Remark = "GSD fixed result"
			});

			profinet.OutputVariables.Add(new CommOutputVariable
			{
				Name = "ErrorCode",
				DataType = CommVariableDataType.ShortInt,
				ByteOffset = 2,
				BitOffset = 0,
				Length = 2,
				Remark = "GSD fixed NG code"
			});
		}
	}
}
