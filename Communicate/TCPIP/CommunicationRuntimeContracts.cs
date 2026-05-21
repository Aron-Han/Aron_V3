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
		public CommunicationConnectionState State { get; private set; }
		public string Message { get; private set; }

		public CommunicationStatusChangedEventArgs(
			CommunicationType communicationType,
			CommunicationConnectionState state,
			string message)
		{
			CommunicationType = communicationType;
			State = state;
			Message = message ?? string.Empty;
		}
	}

	public class CommunicationDataReceivedEventArgs : EventArgs
	{
		public CommunicationType CommunicationType { get; private set; }
		public string RawText { get; private set; }
		public byte[] RawBytes { get; private set; }
		public Dictionary<string, string> Values { get; private set; }
		public DateTime ReceiveTime { get; private set; }

		public CommunicationDataReceivedEventArgs(
			CommunicationType communicationType,
			string rawText,
			byte[] rawBytes,
			Dictionary<string, string> values)
		{
			CommunicationType = communicationType;
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
}
