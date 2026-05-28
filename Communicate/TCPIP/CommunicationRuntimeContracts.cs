using System;
using System.Collections.Generic;

namespace Aron_V3
{
	public enum CommunicationConnectionState
	{
		Disabled = 0,
		Stopped = 1,
		Listening = 2,
		Connecting = 3,
		Connected = 4,
		Disconnected = 5,
		Error = 6
	}

	public class CommunicationStatusChangedEventArgs : EventArgs
	{
		public CommunicationType CommunicationType { get; private set; }
		public string InstanceName { get; private set; }
		public CommunicationConnectionState State { get; private set; }
		public string Message { get; private set; }

		public CommunicationStatusChangedEventArgs(
			CommunicationType communicationType,
			CommunicationConnectionState state,
			string message,
			string instanceName = "")
		{
			CommunicationType = communicationType;
			InstanceName = instanceName ?? string.Empty;
			State = state;
			Message = message ?? string.Empty;
		}
	}

	public class CommunicationDataReceivedEventArgs : EventArgs
	{
		public CommunicationType CommunicationType { get; private set; }
		public string InstanceName { get; private set; }
		public string RawText { get; private set; }
		public byte[] RawBytes { get; private set; }
		public Dictionary<string, string> Values { get; private set; }
		public DateTime ReceiveTime { get; private set; }

		public CommunicationDataReceivedEventArgs(
			CommunicationType communicationType,
			string rawText,
			byte[] rawBytes,
			Dictionary<string, string> values,
			string instanceName = "")
		{
			CommunicationType = communicationType;
			InstanceName = instanceName ?? string.Empty;
			RawText = rawText ?? string.Empty;
			RawBytes = rawBytes ?? new byte[0];
			Values = values ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			ReceiveTime = DateTime.Now;
		}

		public string GetValue(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return string.Empty;
			}

			string value;
			if (Values.TryGetValue(name, out value))
			{
				return value;
			}

			return string.Empty;
		}
	}

	public interface ICommunicationRuntime : IDisposable
	{
		CommunicationType CommunicationType { get; }
		string InstanceName { get; }
		CommunicationConnectionState State { get; }
		bool IsRunning { get; }
		bool IsConnected { get; }

		event EventHandler<CommunicationStatusChangedEventArgs> StatusChanged;
		event EventHandler<CommunicationDataReceivedEventArgs> DataReceived;
		event EventHandler<Exception> ErrorOccurred;

		void Start(CommunicationConfig config);
		void Stop();

		bool SendString(string text);
		bool SendBytes(byte[] data);
	}

	internal static class CommunicationRuntimeNaming
	{
		public static string GetProtocolName(CommunicationType type)
		{
			if (type == CommunicationType.TcpIp)
			{
				return "TCP/IP";
			}

			if (type == CommunicationType.Profinet)
			{
				return "Profinet";
			}

			if (type == CommunicationType.S7)
			{
				return "S7";
			}

			return type.ToString();
		}

		public static string NormalizeProtocolName(string protocolName)
		{
			if (string.IsNullOrWhiteSpace(protocolName))
			{
				return string.Empty;
			}

			string text = protocolName.Trim();
			if (text.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("TcpIp", StringComparison.OrdinalIgnoreCase) ||
				text.Replace("/", string.Empty).Equals("TcpIp", StringComparison.OrdinalIgnoreCase))
			{
				return "TCP/IP";
			}

			if (text.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return "Profinet";
			}

			if (text.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return "S7";
			}

			return text;
		}

		public static string FormatCommunicationName(CommunicationType type, string instanceName)
		{
			return FormatCommunicationName(GetProtocolName(type), instanceName);
		}

		public static string FormatCommunicationName(string protocolName, string instanceName)
		{
			string protocol = NormalizeProtocolName(protocolName);
			if (string.IsNullOrWhiteSpace(protocol))
			{
				protocol = "Communication";
			}

			if (string.IsNullOrWhiteSpace(instanceName))
			{
				return protocol;
			}

			return protocol + "/" + instanceName.Trim();
		}

		public static string GetDefaultInstanceName(string protocolName, CommunicationConfig config)
		{
			CommunicationInstanceConfig instance = FindFirstInstance(config, protocolName);
			if (instance != null && !string.IsNullOrWhiteSpace(instance.InstanceName))
			{
				return instance.InstanceName.Trim();
			}

			string protocol = NormalizeProtocolName(protocolName);
			if (protocol.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				return "TCPIP_01";
			}

			if (protocol.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return "S7_01";
			}

			return string.Empty;
		}

		public static string NormalizeInstanceName(
			string protocolName,
			string instanceName,
			CommunicationConfig config)
		{
			if (!string.IsNullOrWhiteSpace(instanceName))
			{
				return instanceName.Trim();
			}

			return GetDefaultInstanceName(protocolName, config);
		}

		public static CommunicationInstanceConfig FindInstance(
			CommunicationConfig config,
			string protocolName,
			string instanceName)
		{
			if (config == null || config.Instances == null || string.IsNullOrWhiteSpace(instanceName))
			{
				return null;
			}

			string normalizedProtocol = NormalizeProtocolName(protocolName);
			string normalizedInstance = instanceName.Trim();

			foreach (CommunicationInstanceConfig instance in config.Instances)
			{
				if (instance == null ||
					!IsSameProtocol(instance, normalizedProtocol) ||
					string.IsNullOrWhiteSpace(instance.InstanceName))
				{
					continue;
				}

				if (string.Equals(instance.InstanceName.Trim(), normalizedInstance, StringComparison.OrdinalIgnoreCase))
				{
					return instance;
				}
			}

			return null;
		}

		public static CommunicationInstanceConfig FindFirstInstance(
			CommunicationConfig config,
			string protocolName)
		{
			if (config == null || config.Instances == null)
			{
				return null;
			}

			string normalizedProtocol = NormalizeProtocolName(protocolName);
			foreach (CommunicationInstanceConfig instance in config.Instances)
			{
				if (instance != null && IsSameProtocol(instance, normalizedProtocol))
				{
					return instance;
				}
			}

			return null;
		}

		private static bool IsSameProtocol(CommunicationInstanceConfig instance, string normalizedProtocol)
		{
			if (instance == null)
			{
				return false;
			}

			if (normalizedProtocol.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase))
			{
				return instance.CommunicationType == CommunicationType.TcpIp;
			}

			if (normalizedProtocol.Equals("Profinet", StringComparison.OrdinalIgnoreCase))
			{
				return instance.CommunicationType == CommunicationType.Profinet;
			}

			if (normalizedProtocol.Equals("S7", StringComparison.OrdinalIgnoreCase))
			{
				return instance.CommunicationType == CommunicationType.S7;
			}

			return false;
		}
	}
}
