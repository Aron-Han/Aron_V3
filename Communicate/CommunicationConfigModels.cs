using System;
using System.Collections.Generic;
using System.IO;
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
		String = 5
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

		[XmlArray("InputVariables")]
		[XmlArrayItem("Input")]
		public List<CommInputVariable> InputVariables { get; set; }

		[XmlArray("OutputVariables")]
		[XmlArrayItem("Output")]
		public List<CommOutputVariable> OutputVariables { get; set; }

		public TcpIpConfig()
		{
			Enabled = false;
			IsServer = true;
			LocalIP = "0.0.0.0";
			LocalPort = 5000;
			RemoteIP = "192.168.1.10";
			RemotePort = 5000;
			InputVariables = new List<CommInputVariable>();
			OutputVariables = new List<CommOutputVariable>();
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

		public ProfinetConfig()
		{
			Enabled = false;
			DeviceName = "CC24";
			StationName = string.Empty;
			ConnectionStatus = "Disconnected";
			UseGsdFixedMapping = true;
			InputVariables = new List<CommInputVariable>();
			OutputVariables = new List<CommOutputVariable>();
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

		public CommunicationConfig()
		{
			SelectedType = CommunicationType.TcpIp;
			TcpIp = new TcpIpConfig();
			Profinet = new ProfinetConfig();
			S7 = new S7Config();
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
				   config.S7.OutputVariables.Count == 0;
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

			if (config.TcpIp.InputVariables == null) config.TcpIp.InputVariables = new List<CommInputVariable>();
			if (config.TcpIp.OutputVariables == null) config.TcpIp.OutputVariables = new List<CommOutputVariable>();

			if (config.Profinet.InputVariables == null) config.Profinet.InputVariables = new List<CommInputVariable>();
			if (config.Profinet.OutputVariables == null) config.Profinet.OutputVariables = new List<CommOutputVariable>();

			if (config.S7.InputVariables == null) config.S7.InputVariables = new List<CommInputVariable>();
			if (config.S7.OutputVariables == null) config.S7.OutputVariables = new List<CommOutputVariable>();

			if (config.Profinet.InputVariables.Count == 0 && config.Profinet.OutputVariables.Count == 0)
			{
				CreateDefaultProfinetMapping(config.Profinet);
			}

			EnsureEngineName(config.Profinet.InputVariables);
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
