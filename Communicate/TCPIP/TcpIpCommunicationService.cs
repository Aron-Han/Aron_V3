using System;
using System.Collections.Generic;
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
	/// 3. TCP/IP 当前按 String 处理，变量的 ByteOffset 在界面上等价为 Char Offset。
	/// 4. 接收到原始字符串后，会按 CommunicationConfig 中的 InputVariables 解析出变量字典。
	/// 5. 后续 Profinet / S7 也可以实现 ICommunicationRuntime，统一由 CommunicationRuntimeManager 管理。
	/// </summary>
	public class TcpIpCommunicationService : ICommunicationRuntime
	{
		private readonly object _syncRoot = new object();
		private readonly object _sendLock = new object();

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
					if (client != null)
					{
						try
						{
							client.Close();
						}
						catch
						{
						}
					}

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

					string text = Encoding.ASCII.GetString(actual, 0, actual.Length);
					Dictionary<string, string> values = ParseInputVariables(text);

					OnDataReceived(new CommunicationDataReceivedEventArgs(
						CommunicationType.TcpIp,
						text,
						actual,
						values));
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

				try
				{
					if (stream != null)
					{
						stream.Close();
					}
				}
				catch
				{
				}

				try
				{
					if (client != null)
					{
						client.Close();
					}
				}
				catch
				{
				}

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

		private Dictionary<string, string> ParseInputVariables(string text)
		{
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (_tcpConfig == null || _tcpConfig.InputVariables == null)
			{
				return result;
			}

			if (text == null)
			{
				text = string.Empty;
			}

			foreach (CommInputVariable item in _tcpConfig.InputVariables)
			{
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}

				int start = item.ByteOffset;
				int length = item.Length;

				if (start < 0)
				{
					start = 0;
				}

				if (length <= 0)
				{
					length = text.Length - start;
				}

				string value = string.Empty;

				if (start < text.Length)
				{
					int safeLength = Math.Min(length, text.Length - start);
					value = text.Substring(start, safeLength);
				}

				result[item.Name] = value.Trim();
			}

			return result;
		}

		public string BuildOutputMessage(Dictionary<string, object> outputValues)
		{
			if (_tcpConfig == null || _tcpConfig.OutputVariables == null)
			{
				return string.Empty;
			}

			if (outputValues == null)
			{
				outputValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			}

			int length = 0;

			foreach (CommOutputVariable item in _tcpConfig.OutputVariables)
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

			foreach (CommOutputVariable item in _tcpConfig.OutputVariables)
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
			string message = BuildOutputMessage(outputValues);
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
				try
				{
					if (client != null)
					{
						client.Close();
					}
				}
				catch
				{
				}
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
					message));
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
						Remark = item.Remark
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
						Remark = item.Remark
					});
				}
			}

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
}
