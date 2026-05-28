using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aron_V3
{
	/// <summary>
	/// TCP/IP runtime for Betterway Vision-Base.
	/// 
	/// 设计目标：
	/// 1. 同时支持 Server / Client。
	/// 2. 不依赖 UI 控件，方便后续在主程序、流程管理、测试窗口中复用。
	/// 3. TCP/IP 可按 String 或 Byte 模式处理，Byte 模式支持大小端转换。
	/// 4. 接收到原始数据后，会按 CommunicationConfig 中的 InputVariables 解析出变量字典。
	/// 5. 后续 Profinet / S7 也可以实现 ICommunicationRuntime，统一由 CommunicationRuntimeManager 管理。
	/// </summary>
	public class TcpIpCommunicationService : ICommunicationRuntime
	{
		private readonly object _syncRoot = new object();
		private readonly object _sendLock = new object();
		private readonly string _instanceName;

		private TcpIpConfig _tcpConfig;
		private CancellationTokenSource _cts;

		private TcpListener _listener;
		private TcpClient _clientForClientMode;

		private readonly List<TcpClient> _clients = new List<TcpClient>();
		private readonly List<Task> _workers = new List<Task>();

		private CommunicationConnectionState _state = CommunicationConnectionState.Stopped;
		private bool _disposed;

		public CommunicationType CommunicationType
		{
			get { return CommunicationType.TcpIp; }
		}

		public string InstanceName
		{
			get { return _instanceName; }
		}

		public CommunicationConnectionState State
		{
			get { return _state; }
		}

		public bool IsRunning
		{
			get
			{
				lock (_syncRoot)
				{
					return _cts != null && !_cts.IsCancellationRequested;
				}
			}
		}

		public bool IsConnected
		{
			get
			{
				lock (_syncRoot)
				{
					if (_tcpConfig != null && _tcpConfig.IsServer)
					{
						return _clients.Count > 0;
					}

					return _clientForClientMode != null && IsTcpClientConnected(_clientForClientMode);
				}
			}
		}

		public event EventHandler<CommunicationStatusChangedEventArgs> StatusChanged;
		public event EventHandler<CommunicationDataReceivedEventArgs> DataReceived;
		public event EventHandler<Exception> ErrorOccurred;

		public TcpIpCommunicationService()
			: this("TCPIP_01")
		{
		}

		public TcpIpCommunicationService(string instanceName)
		{
			_instanceName = string.IsNullOrWhiteSpace(instanceName)
				? "TCPIP_01"
				: instanceName.Trim();
		}

		public void Start(CommunicationConfig config)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("TcpIpCommunicationService");
			}

			Stop();

			if (config == null || config.TcpIp == null || !config.TcpIp.Enabled)
			{
				_tcpConfig = null;
				SetState(CommunicationConnectionState.Disabled, "TCP/IP disabled.");
				return;
			}

			_tcpConfig = CloneTcpConfig(config.TcpIp);
			_cts = new CancellationTokenSource();

			if (_tcpConfig.IsServer)
			{
				Task task = Task.Factory.StartNew(
					delegate { ServerLoop(_cts.Token); },
					_cts.Token,
					TaskCreationOptions.LongRunning,
					TaskScheduler.Default);

				AddWorker(task);
			}
			else
			{
				Task task = Task.Factory.StartNew(
					delegate { ClientLoop(_cts.Token); },
					_cts.Token,
					TaskCreationOptions.LongRunning,
					TaskScheduler.Default);

				AddWorker(task);
			}
		}

		public void Stop()
		{
			CancellationTokenSource oldCts = null;

			lock (_syncRoot)
			{
				oldCts = _cts;
				_cts = null;
			}

			if (oldCts != null)
			{
				try
				{
					oldCts.Cancel();
				}
				catch
				{
				}
			}

			CloseListener();
			CloseAllClients();

			if (oldCts != null)
			{
				try
				{
					oldCts.Dispose();
				}
				catch
				{
				}
			}

			SetState(CommunicationConnectionState.Stopped, "TCP/IP stopped.");
		}

		public bool SendString(string text)
		{
			if (text == null)
			{
				text = string.Empty;
			}

			return SendBytes(Encoding.ASCII.GetBytes(text));
		}

		public bool SendLine(string text)
		{
			if (text == null)
			{
				text = string.Empty;
			}

			return SendString(text + "\r\n");
		}

		public bool SendBytes(byte[] data)
		{
			if (data == null)
			{
				data = new byte[0];
			}

			if (_tcpConfig == null || !_tcpConfig.Enabled)
			{
				return false;
			}

			lock (_sendLock)
			{
				if (_tcpConfig.IsServer)
				{
					return SendBytesToServerClients(data);
				}

				return SendBytesToClientModeServer(data);
			}
		}

		private bool SendBytesToClientModeServer(byte[] data)
		{
			TcpClient client;

			lock (_syncRoot)
			{
				client = _clientForClientMode;
			}

			if (client == null || !IsTcpClientConnected(client))
			{
				return false;
			}

			try
			{
				NetworkStream stream = client.GetStream();

				if (stream == null || !stream.CanWrite)
				{
					return false;
				}

				stream.Write(data, 0, data.Length);
				stream.Flush();
				return true;
			}
			catch (Exception ex)
			{
				OnError(ex);
				return false;
			}
		}

		private bool SendBytesToServerClients(byte[] data)
		{
			List<TcpClient> snapshot;

			lock (_syncRoot)
			{
				snapshot = new List<TcpClient>(_clients);
			}

			bool anySuccess = false;

			foreach (TcpClient client in snapshot)
			{
				if (client == null || !IsTcpClientConnected(client))
				{
					RemoveClient(client);
					continue;
				}

				try
				{
					NetworkStream stream = client.GetStream();

					if (stream == null || !stream.CanWrite)
					{
						RemoveClient(client);
						continue;
					}

					stream.Write(data, 0, data.Length);
					stream.Flush();
					anySuccess = true;
				}
				catch (Exception ex)
				{
					RemoveClient(client);
					OnError(ex);
				}
			}

			return anySuccess;
		}

		private void ServerLoop(CancellationToken token)
		{
			try
			{
				IPAddress localAddress = IPAddress.Any;
				int port = NormalizePort(_tcpConfig.LocalPort, 5000);

				_listener = new TcpListener(localAddress, port);
				_listener.Start();

				SetState(
					CommunicationConnectionState.Listening,
					"TCP/IP server listening on " + localAddress + ":" + port);

				while (!token.IsCancellationRequested)
				{
					TcpClient client = null;

					try
					{
						client = _listener.AcceptTcpClient();
					}
					catch (SocketException ex)
					{
						if (!token.IsCancellationRequested)
						{
							OnError(ex);
							SetState(CommunicationConnectionState.Error, ex.Message);
						}

						break;
					}
					catch (ObjectDisposedException)
					{
						break;
					}

					if (client == null)
					{
						continue;
					}

					client.NoDelay = true;
					AddClient(client);

					IPEndPoint remote = client.Client.RemoteEndPoint as IPEndPoint;

					SetState(
						CommunicationConnectionState.Connected,
						"TCP/IP client connected: " + (remote == null ? string.Empty : remote.ToString()));

					TcpClient capturedClient = client;

					Task receiveTask = Task.Factory.StartNew(
						delegate { ReceiveLoop(capturedClient, token); },
						token,
						TaskCreationOptions.LongRunning,
						TaskScheduler.Default);

					AddWorker(receiveTask);
				}
			}
			catch (Exception ex)
			{
				if (!token.IsCancellationRequested)
				{
					OnError(ex);
					SetState(CommunicationConnectionState.Error, ex.Message);
				}
			}
		}

		private void ClientLoop(CancellationToken token)
		{
			int reconnectDelayMs = 1000;

			while (!token.IsCancellationRequested)
			{
				TcpClient client = null;

				try
				{
					string remoteIp = string.IsNullOrWhiteSpace(_tcpConfig.RemoteIP)
						? "127.0.0.1"
						: _tcpConfig.RemoteIP.Trim();

					int port = NormalizePort(_tcpConfig.RemotePort, 5000);

					SetState(
						CommunicationConnectionState.Connecting,
						"TCP/IP client connecting to " + remoteIp + ":" + port);

					client = new TcpClient();
					client.NoDelay = true;
					client.Connect(remoteIp, port);

					lock (_syncRoot)
					{
						_clientForClientMode = client;
					}

					SetState(
						CommunicationConnectionState.Connected,
						"TCP/IP client connected to " + remoteIp + ":" + port);

					ReceiveLoop(client, token);
				}
				catch (SocketException ex)
				{
					if (!token.IsCancellationRequested)
					{
						OnError(ex);
						SetState(CommunicationConnectionState.Disconnected, ex.Message);
					}
				}
				catch (Exception ex)
				{
					if (!token.IsCancellationRequested)
					{
						OnError(ex);
						SetState(CommunicationConnectionState.Disconnected, ex.Message);
					}
				}
				finally
				{
					RemoveClient(client);

					lock (_syncRoot)
					{
						if (ReferenceEquals(_clientForClientMode, client))
						{
							_clientForClientMode = null;
						}
					}
				}

				if (!token.IsCancellationRequested)
				{
					SleepWithCancel(token, reconnectDelayMs);
				}
			}
		}

		private void ReceiveLoop(TcpClient client, CancellationToken token)
		{
			NetworkStream stream = null;

			try
			{
				stream = client.GetStream();
				byte[] buffer = new byte[4096];

				while (!token.IsCancellationRequested)
				{
					int count = stream.Read(buffer, 0, buffer.Length);

					if (count <= 0)
					{
						break;
					}

					byte[] actual = new byte[count];
					Buffer.BlockCopy(buffer, 0, actual, 0, count);

					string text = TcpIpPayloadCodec.BuildRawDisplayText(actual, _tcpConfig);
					Dictionary<string, string> values = TcpIpPayloadCodec.ParseInputVariables(actual, text, _tcpConfig);

					OnDataReceived(new CommunicationDataReceivedEventArgs(
						CommunicationType.TcpIp,
						text,
						actual,
						values,
						InstanceName));
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				if (!token.IsCancellationRequested)
				{
					OnError(ex);
				}
			}
			finally
			{
				RemoveClient(client);

				if (_tcpConfig != null && _tcpConfig.IsServer)
				{
					if (IsRunning)
					{
						if (IsConnected)
						{
							SetState(CommunicationConnectionState.Connected, "TCP/IP client disconnected, other clients still connected.");
						}
						else
						{
							SetState(CommunicationConnectionState.Listening, "TCP/IP server listening.");
						}
					}
				}
				else
				{
					if (IsRunning)
					{
						SetState(CommunicationConnectionState.Disconnected, "TCP/IP client disconnected.");
					}
				}
			}
		}

		public string BuildOutputMessage(Dictionary<string, object> outputValues)
		{
			return BuildOutputMessage(outputValues, _tcpConfig);
		}

		public string BuildOutputMessage(Dictionary<string, object> outputValues, TcpIpConfig tcpConfig)
		{
			if (tcpConfig == null || tcpConfig.OutputVariables == null)
			{
				return string.Empty;
			}

			if (outputValues == null)
			{
				outputValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			}

			int length = 0;

			foreach (CommOutputVariable item in tcpConfig.OutputVariables)
			{
				if (item == null)
				{
					continue;
				}

				int end = item.ByteOffset + Math.Max(0, item.Length);
				if (end > length)
				{
					length = end;
				}
			}

			if (length <= 0)
			{
				return string.Empty;
			}

			char[] chars = new char[length];

			for (int i = 0; i < chars.Length; i++)
			{
				chars[i] = ' ';
			}

			foreach (CommOutputVariable item in tcpConfig.OutputVariables)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}

				object rawValue;
				if (!outputValues.TryGetValue(item.Name, out rawValue))
				{
					continue;
				}

				string value = Convert.ToString(rawValue);

				if (value == null)
				{
					value = string.Empty;
				}

				int start = item.ByteOffset;
				int maxLength = item.Length;

				if (start < 0 || start >= chars.Length)
				{
					continue;
				}

				if (maxLength <= 0)
				{
					maxLength = chars.Length - start;
				}

				if (value.Length > maxLength)
				{
					value = value.Substring(0, maxLength);
				}

				for (int i = 0; i < value.Length && start + i < chars.Length; i++)
				{
					chars[start + i] = value[i];
				}
			}

			return new string(chars);
		}

		public bool SendOutputValues(Dictionary<string, object> outputValues)
		{
			return SendOutputValues(outputValues, _tcpConfig);
		}

		public bool SendOutputValues(Dictionary<string, object> outputValues, TcpIpConfig tcpConfig)
		{
			if (TcpIpPayloadCodec.IsByteMode(tcpConfig))
			{
				byte[] data = TcpIpPayloadCodec.BuildOutputBytes(outputValues, tcpConfig);
				return SendBytes(data);
			}

			string message = BuildOutputMessage(outputValues, tcpConfig);
			return SendString(message);
		}

		private void AddClient(TcpClient client)
		{
			if (client == null)
			{
				return;
			}

			lock (_syncRoot)
			{
				if (!_clients.Contains(client))
				{
					_clients.Add(client);
				}
			}
		}

		private void RemoveClient(TcpClient client)
		{
			if (client == null)
			{
				return;
			}

			lock (_syncRoot)
			{
				_clients.Remove(client);

				if (ReferenceEquals(_clientForClientMode, client))
				{
					_clientForClientMode = null;
				}
			}

			CloseTcpClient(client);
		}

		private void CloseAllClients()
		{
			List<TcpClient> snapshot;

			lock (_syncRoot)
			{
				snapshot = new List<TcpClient>(_clients);
				_clients.Clear();

				if (_clientForClientMode != null)
				{
					snapshot.Add(_clientForClientMode);
					_clientForClientMode = null;
				}
			}

			foreach (TcpClient client in snapshot)
			{
				CloseTcpClient(client);
			}
		}

		private static void CloseTcpClient(TcpClient client)
		{
			if (client == null)
			{
				return;
			}

			try
			{
				Socket socket = client.Client;

				if (socket != null && socket.Connected)
				{
					socket.Shutdown(SocketShutdown.Both);
				}
			}
			catch
			{
			}

			try
			{
				client.Close();
			}
			catch
			{
			}
		}

		private void CloseListener()
		{
			try
			{
				if (_listener != null)
				{
					_listener.Stop();
				}
			}
			catch
			{
			}
			finally
			{
				_listener = null;
			}
		}

		private void AddWorker(Task task)
		{
			if (task == null)
			{
				return;
			}

			lock (_syncRoot)
			{
				_workers.Add(task);
				_workers.RemoveAll(t => t == null || t.IsCompleted || t.IsCanceled || t.IsFaulted);
			}
		}

		private void SetState(CommunicationConnectionState state, string message)
		{
			_state = state;

			EventHandler<CommunicationStatusChangedEventArgs> handler = StatusChanged;

			if (handler != null)
			{
				handler(this, new CommunicationStatusChangedEventArgs(
					CommunicationType.TcpIp,
					state,
					message,
					InstanceName));
			}
		}

		private void OnDataReceived(CommunicationDataReceivedEventArgs e)
		{
			EventHandler<CommunicationDataReceivedEventArgs> handler = DataReceived;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void OnError(Exception ex)
		{
			EventHandler<Exception> handler = ErrorOccurred;

			if (handler != null)
			{
				handler(this, ex);
			}
		}

		private static IPAddress ParseLocalIPAddress(string ip)
		{
			if (string.IsNullOrWhiteSpace(ip) ||
				ip.Trim() == "0.0.0.0" ||
				ip.Trim().Equals("Any", StringComparison.OrdinalIgnoreCase))
			{
				return IPAddress.Any;
			}

			IPAddress address;
			if (IPAddress.TryParse(ip.Trim(), out address))
			{
				return address;
			}

			return IPAddress.Any;
		}

		private static int NormalizePort(int port, int defaultPort)
		{
			if (port <= 0 || port > 65535)
			{
				return defaultPort;
			}

			return port;
		}

		private static bool IsTcpClientConnected(TcpClient client)
		{
			if (client == null || client.Client == null)
			{
				return false;
			}

			try
			{
				if (!client.Connected)
				{
					return false;
				}

				bool disconnected = client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0;
				return !disconnected;
			}
			catch
			{
				return false;
			}
		}

		private static void SleepWithCancel(CancellationToken token, int milliseconds)
		{
			int elapsed = 0;
			int step = 50;

			while (!token.IsCancellationRequested && elapsed < milliseconds)
			{
				Thread.Sleep(step);
				elapsed += step;
			}
		}

		private static TcpIpConfig CloneTcpConfig(TcpIpConfig source)
		{
			TcpIpConfig target = new TcpIpConfig();

			if (source == null)
			{
				return target;
			}

			target.Enabled = source.Enabled;
			target.IsServer = source.IsServer;
			target.LocalIP = source.LocalIP;
			target.LocalPort = source.LocalPort;
			target.RemoteIP = source.RemoteIP;
			target.RemotePort = source.RemotePort;
			target.PayloadMode = source.PayloadMode;
			target.ByteOrder = source.ByteOrder;
			target.Heartbeat = CloneHeartbeat(source.Heartbeat);

			target.InputVariables = new List<CommInputVariable>();
			target.OutputVariables = new List<CommOutputVariable>();

			if (source.InputVariables != null)
			{
				foreach (CommInputVariable item in source.InputVariables)
				{
					if (item == null)
					{
						continue;
					}

					target.InputVariables.Add(new CommInputVariable
					{
						Name = item.Name,
						UseAsTrigger = item.UseAsTrigger,
						UseAsPosition = item.UseAsPosition,
						EngineName = item.EngineName,
						DataType = item.DataType,
						ByteOffset = item.ByteOffset,
						BitOffset = item.BitOffset,
						Length = item.Length,
						Remark = item.Remark,
						GlobalVariableName = item.GlobalVariableName
					});
				}
			}

			if (source.OutputVariables != null)
			{
				foreach (CommOutputVariable item in source.OutputVariables)
				{
					if (item == null)
					{
						continue;
					}

					target.OutputVariables.Add(new CommOutputVariable
					{
						Name = item.Name,
						DataType = item.DataType,
						ByteOffset = item.ByteOffset,
						BitOffset = item.BitOffset,
						Length = item.Length,
						Remark = item.Remark,
						GlobalVariableName = item.GlobalVariableName
					});
				}
			}

			return target;
		}

		private static CommunicationHeartbeatConfig CloneHeartbeat(CommunicationHeartbeatConfig source)
		{
			CommunicationHeartbeatConfig target = new CommunicationHeartbeatConfig();
			if (source == null)
			{
				return target;
			}

			target.Enabled = source.Enabled;
			target.OutputName = source.OutputName;
			target.HeartbeatText = source.HeartbeatText;
			target.IntervalMs = source.IntervalMs;
			return target;
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			Stop();
		}
	}

	internal static class TcpIpPayloadCodec
	{
		public static bool IsByteMode(TcpIpConfig config)
		{
			return config != null && config.PayloadMode == TcpIpPayloadMode.Byte;
		}

		public static string BuildRawDisplayText(byte[] data, TcpIpConfig config)
		{
			if (data == null)
			{
				data = new byte[0];
			}

			if (IsByteMode(config))
			{
				return ToHexString(data);
			}

			return Encoding.ASCII.GetString(data, 0, data.Length);
		}

		public static Dictionary<string, string> ParseInputVariables(byte[] data, string rawText, TcpIpConfig config)
		{
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (config == null || config.InputVariables == null)
			{
				return result;
			}

			foreach (CommInputVariable item in config.InputVariables)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}

				result[item.Name] = IsByteMode(config)
					? ParseInputVariableFromBytes(data, item, config.ByteOrder)
					: ParseInputVariableFromString(rawText, item);
			}

			return result;
		}

		public static string ParseInputVariableFromString(string rawText, CommInputVariable item)
		{
			if (rawText == null)
			{
				rawText = string.Empty;
			}

			if (item == null)
			{
				return string.Empty;
			}

			int offset = item.ByteOffset < 0 ? 0 : item.ByteOffset;
			int length = item.Length <= 0 ? rawText.Length - offset : item.Length;

			if (offset >= rawText.Length || length <= 0)
			{
				return string.Empty;
			}

			int realLength = Math.Min(length, rawText.Length - offset);
			string value = rawText.Substring(offset, realLength);

			return value.Trim();
		}

		public static string ParseInputVariableFromBytes(byte[] data, CommInputVariable item, CommByteOrder byteOrder)
		{
			if (data == null)
			{
				data = new byte[0];
			}

			if (item == null)
			{
				return string.Empty;
			}

			int offset = item.ByteOffset < 0 ? 0 : item.ByteOffset;

			if (offset >= data.Length)
			{
				return string.Empty;
			}

			if (item.DataType == CommVariableDataType.Bool)
			{
				int bitOffset = item.BitOffset;
				if (bitOffset >= 0 && bitOffset <= 7)
				{
					return (data[offset] & (1 << bitOffset)) != 0 ? "1" : "0";
				}

				int boolLength = item.Length <= 0 ? 1 : item.Length;
				int realBoolLength = Math.Min(boolLength, data.Length - offset);
				for (int i = 0; i < realBoolLength; i++)
				{
					if (data[offset + i] != 0)
					{
						return "1";
					}
				}

				return "0";
			}

			if (item.DataType == CommVariableDataType.String)
			{
				int stringLength = item.Length <= 0 ? data.Length - offset : item.Length;
				if (stringLength <= 0)
				{
					return string.Empty;
				}

				int realStringLength = Math.Min(stringLength, data.Length - offset);
				return Encoding.ASCII.GetString(data, offset, realStringLength).TrimEnd('\0').Trim();
			}

			if (item.DataType == CommVariableDataType.Bytes)
			{
				int bytesLength = item.Length <= 0 ? data.Length - offset : item.Length;
				if (bytesLength <= 0)
				{
					return string.Empty;
				}

				int realBytesLength = Math.Min(bytesLength, data.Length - offset);
				byte[] slice = new byte[realBytesLength];
				Buffer.BlockCopy(data, offset, slice, 0, realBytesLength);
				return ToHexString(slice);
			}

			int length = GetFixedByteLength(item.DataType);
			byte[] bytes = ReadFixedBytes(data, offset, length, byteOrder);

			if (bytes == null)
			{
				return string.Empty;
			}

			try
			{
				switch (item.DataType)
				{
					case CommVariableDataType.Float:
						return BitConverter.ToSingle(bytes, 0).ToString("G", CultureInfo.InvariantCulture);
					case CommVariableDataType.Double:
						return BitConverter.ToDouble(bytes, 0).ToString("G", CultureInfo.InvariantCulture);
					case CommVariableDataType.ShortInt:
						return BitConverter.ToInt16(bytes, 0).ToString(CultureInfo.InvariantCulture);
					case CommVariableDataType.LongInt:
						return BitConverter.ToInt32(bytes, 0).ToString(CultureInfo.InvariantCulture);
					default:
						return string.Empty;
				}
			}
			catch
			{
				return string.Empty;
			}
		}

		public static byte[] BuildOutputBytes(Dictionary<string, object> outputValues, TcpIpConfig config)
		{
			if (config == null || config.OutputVariables == null)
			{
				return new byte[0];
			}

			if (outputValues == null)
			{
				outputValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			}

			int totalLength = 0;

			foreach (CommOutputVariable item in config.OutputVariables)
			{
				if (item == null)
				{
					continue;
				}

				object value;
				TryGetOutputValue(outputValues, item.Name, out value);

				int offset = item.ByteOffset < 0 ? 0 : item.ByteOffset;
				int end = offset + GetOutputByteLength(item, value);
				if (end > totalLength)
				{
					totalLength = end;
				}
			}

			if (totalLength <= 0)
			{
				return new byte[0];
			}

			byte[] buffer = new byte[totalLength];

			foreach (CommOutputVariable item in config.OutputVariables)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}

				object rawValue;
				if (!TryGetOutputValue(outputValues, item.Name, out rawValue))
				{
					continue;
				}

				WriteOutputVariable(buffer, item, rawValue, config.ByteOrder);
			}

			return buffer;
		}

		public static bool TryParseHexText(string text, out byte[] data, out string error)
		{
			data = new byte[0];
			error = string.Empty;

			if (string.IsNullOrWhiteSpace(text))
			{
				error = "Hex text is empty.";
				return false;
			}

			char[] separators = new char[] { ' ', ',', ';', '-', '\r', '\n', '\t' };
			string[] tokens = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			List<byte> bytes = new List<byte>();

			if (tokens.Length == 1)
			{
				string compact = NormalizeHexToken(tokens[0]);
				if (compact.Length > 2 && compact.Length % 2 == 0)
				{
					for (int i = 0; i < compact.Length; i += 2)
					{
						byte value;
						if (!TryParseHexByte(compact.Substring(i, 2), out value))
						{
							error = "Invalid hex byte: " + compact.Substring(i, 2);
							return false;
						}

						bytes.Add(value);
					}
				}
				else
				{
					byte value;
					if (!TryParseHexByte(compact, out value))
					{
						error = "Invalid hex byte: " + compact;
						return false;
					}

					bytes.Add(value);
				}
			}
			else
			{
				foreach (string token in tokens)
				{
					string normalized = NormalizeHexToken(token);

					if (normalized.Length > 2 && normalized.Length % 2 == 0)
					{
						for (int i = 0; i < normalized.Length; i += 2)
						{
							byte value;
							if (!TryParseHexByte(normalized.Substring(i, 2), out value))
							{
								error = "Invalid hex byte: " + normalized.Substring(i, 2);
								return false;
							}

							bytes.Add(value);
						}
					}
					else
					{
						byte value;
						if (!TryParseHexByte(normalized, out value))
						{
							error = "Invalid hex byte: " + normalized;
							return false;
						}

						bytes.Add(value);
					}
				}
			}

			data = bytes.ToArray();
			return true;
		}

		public static string ToHexString(byte[] data)
		{
			if (data == null || data.Length <= 0)
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder(data.Length * 3);

			for (int i = 0; i < data.Length; i++)
			{
				if (i > 0)
				{
					sb.Append(' ');
				}

				sb.Append(data[i].ToString("X2", CultureInfo.InvariantCulture));
			}

			return sb.ToString();
		}

		private static void WriteOutputVariable(byte[] buffer, CommOutputVariable item, object rawValue, CommByteOrder byteOrder)
		{
			int offset = item.ByteOffset < 0 ? 0 : item.ByteOffset;

			if (offset >= buffer.Length)
			{
				return;
			}

			if (item.DataType == CommVariableDataType.Bool)
			{
				bool boolValue = ToBool(rawValue);
				int bitOffset = item.BitOffset;

				if (bitOffset >= 0 && bitOffset <= 7)
				{
					byte mask = (byte)(1 << bitOffset);
					if (boolValue)
					{
						buffer[offset] = (byte)(buffer[offset] | mask);
					}
					else
					{
						buffer[offset] = (byte)(buffer[offset] & ~mask);
					}
				}
				else
				{
					buffer[offset] = boolValue ? (byte)1 : (byte)0;
				}

				return;
			}

			if (item.DataType == CommVariableDataType.String)
			{
				string text = Convert.ToString(rawValue, CultureInfo.InvariantCulture);
				if (text == null)
				{
					text = string.Empty;
				}

				int length = item.Length <= 0 ? Encoding.ASCII.GetByteCount(text) : item.Length;
				byte[] textBytes = Encoding.ASCII.GetBytes(text);
				WriteBytes(buffer, offset, textBytes, length);
				return;
			}

			if (item.DataType == CommVariableDataType.Bytes)
			{
				byte[] rawBytes = ConvertToRawBytes(rawValue);
				int length = item.Length <= 0 ? rawBytes.Length : item.Length;
				WriteBytes(buffer, offset, rawBytes, length);
				return;
			}

			byte[] data = BuildNumericBytes(item.DataType, rawValue);
			if (data == null || data.Length <= 0)
			{
				return;
			}

			ApplyTargetByteOrder(data, byteOrder);
			WriteBytes(buffer, offset, data, data.Length);
		}

		private static byte[] BuildNumericBytes(CommVariableDataType dataType, object rawValue)
		{
			switch (dataType)
			{
				case CommVariableDataType.Float:
					float floatValue;
					if (!TryToSingle(rawValue, out floatValue)) return null;
					return BitConverter.GetBytes(floatValue);
				case CommVariableDataType.Double:
					double doubleValue;
					if (!TryToDouble(rawValue, out doubleValue)) return null;
					return BitConverter.GetBytes(doubleValue);
				case CommVariableDataType.ShortInt:
					short shortValue;
					if (!TryToInt16(rawValue, out shortValue)) return null;
					return BitConverter.GetBytes(shortValue);
				case CommVariableDataType.LongInt:
					int intValue;
					if (!TryToInt32(rawValue, out intValue)) return null;
					return BitConverter.GetBytes(intValue);
				default:
					return null;
			}
		}

		private static int GetOutputByteLength(CommOutputVariable item, object value)
		{
			if (item == null)
			{
				return 0;
			}

			if (item.DataType == CommVariableDataType.String)
			{
				if (item.Length > 0)
				{
					return item.Length;
				}

				string text = value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
				return Math.Max(1, Encoding.ASCII.GetByteCount(text));
			}

			if (item.DataType == CommVariableDataType.Bytes)
			{
				if (item.Length > 0)
				{
					return item.Length;
				}

				byte[] data = ConvertToRawBytes(value);
				return Math.Max(1, data.Length);
			}

			if (item.DataType == CommVariableDataType.Bool)
			{
				return 1;
			}

			return GetFixedByteLength(item.DataType);
		}

		private static int GetFixedByteLength(CommVariableDataType dataType)
		{
			switch (dataType)
			{
				case CommVariableDataType.Float:
					return 4;
				case CommVariableDataType.Double:
					return 8;
				case CommVariableDataType.ShortInt:
					return 2;
				case CommVariableDataType.LongInt:
					return 4;
				case CommVariableDataType.Bool:
					return 1;
				case CommVariableDataType.Bytes:
					return 1;
				default:
					return 1;
			}
		}

		private static byte[] ConvertToRawBytes(object value)
		{
			if (value == null)
			{
				return new byte[0];
			}

			byte[] bytes = value as byte[];
			if (bytes != null)
			{
				byte[] copy = new byte[bytes.Length];
				Buffer.BlockCopy(bytes, 0, copy, 0, bytes.Length);
				return copy;
			}

			string text = Convert.ToString(value, CultureInfo.InvariantCulture);
			byte[] data;
			string error;
			return TryParseHexText(text, out data, out error) ? data : new byte[0];
		}

		private static byte[] ReadFixedBytes(byte[] data, int offset, int length, CommByteOrder byteOrder)
		{
			if (data == null || length <= 0 || offset < 0 || offset + length > data.Length)
			{
				return null;
			}

			byte[] result = new byte[length];
			Buffer.BlockCopy(data, offset, result, 0, length);
			ApplySourceByteOrder(result, byteOrder);
			return result;
		}

		private static void ApplySourceByteOrder(byte[] data, CommByteOrder byteOrder)
		{
			ApplyEndianForBitConverter(data, byteOrder);
		}

		private static void ApplyTargetByteOrder(byte[] data, CommByteOrder byteOrder)
		{
			ApplyEndianForBitConverter(data, byteOrder);
		}

		private static void ApplyEndianForBitConverter(byte[] data, CommByteOrder byteOrder)
		{
			if (data == null || data.Length <= 1)
			{
				return;
			}

			bool targetLittleEndian = byteOrder == CommByteOrder.LittleEndian;
			if (BitConverter.IsLittleEndian != targetLittleEndian)
			{
				Array.Reverse(data);
			}
		}

		private static void WriteBytes(byte[] buffer, int offset, byte[] data, int length)
		{
			if (buffer == null || data == null || offset < 0 || offset >= buffer.Length || length <= 0)
			{
				return;
			}

			int realLength = Math.Min(length, buffer.Length - offset);
			int copyLength = Math.Min(realLength, data.Length);

			if (copyLength > 0)
			{
				Buffer.BlockCopy(data, 0, buffer, offset, copyLength);
			}
		}

		private static bool TryGetOutputValue(Dictionary<string, object> values, string name, out object value)
		{
			value = null;

			if (values == null || string.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			if (values.TryGetValue(name, out value))
			{
				return true;
			}

			foreach (KeyValuePair<string, object> pair in values)
			{
				if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
				{
					value = pair.Value;
					return true;
				}
			}

			return false;
		}

		private static bool TryToSingle(object value, out float result)
		{
			result = 0;
			double doubleValue;
			if (!TryToDouble(value, out doubleValue))
			{
				return false;
			}

			result = (float)doubleValue;
			return true;
		}

		private static bool TryToDouble(object value, out double result)
		{
			result = 0;

			if (value == null)
			{
				return false;
			}

			try
			{
				if (value is string)
				{
					string text = ((string)value).Trim();
					return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ||
						   double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
				}

				result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static bool TryToInt16(object value, out short result)
		{
			result = 0;
			int intValue;
			if (!TryToInt32(value, out intValue))
			{
				return false;
			}

			if (intValue < short.MinValue || intValue > short.MaxValue)
			{
				return false;
			}

			result = (short)intValue;
			return true;
		}

		private static bool TryToInt32(object value, out int result)
		{
			result = 0;

			if (value == null)
			{
				return false;
			}

			if (value is bool)
			{
				result = (bool)value ? 1 : 0;
				return true;
			}

			try
			{
				if (value is string)
				{
					string text = ((string)value).Trim();
					if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ||
						int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
					{
						return true;
					}

					double doubleValue;
					if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleValue) ||
						double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out doubleValue))
					{
						result = Convert.ToInt32(doubleValue);
						return true;
					}

					return false;
				}

				result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static bool ToBool(object value)
		{
			if (value == null)
			{
				return false;
			}

			if (value is bool)
			{
				return (bool)value;
			}

			string text = Convert.ToString(value, CultureInfo.InvariantCulture);

			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			text = text.Trim();

			return text.Equals("1", StringComparison.OrdinalIgnoreCase) ||
				   text.Equals("true", StringComparison.OrdinalIgnoreCase) ||
				   text.Equals("ok", StringComparison.OrdinalIgnoreCase) ||
				   text.Equals("yes", StringComparison.OrdinalIgnoreCase);
		}

		private static string NormalizeHexToken(string token)
		{
			if (token == null)
			{
				return string.Empty;
			}

			string text = token.Trim();
			if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(2);
			}

			return text;
		}

		private static bool TryParseHexByte(string token, out byte value)
		{
			value = 0;
			if (string.IsNullOrWhiteSpace(token) || token.Length > 2)
			{
				return false;
			}

			return byte.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
		}
	}
}
