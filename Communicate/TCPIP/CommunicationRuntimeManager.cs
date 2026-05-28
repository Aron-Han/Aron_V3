using System;
using System.Collections.Generic;
using System.Threading;

namespace Aron_V3
{
	/// <summary>
	/// 通讯运行管理器。
	/// 
	/// 后续扩展方式：
	/// 1. Profinet 写 ProfinetCommunicationService : ICommunicationRuntime
	/// 2. S7 写 S7CommunicationService : ICommunicationRuntime
	/// 3. 在构造函数中加入 _runtimes[CommunicationType.Profinet] = new ProfinetCommunicationService();
	/// </summary>
	public sealed class CommunicationRuntimeManager : IDisposable
	{
		private static readonly Lazy<CommunicationRuntimeManager> _instance =
			new Lazy<CommunicationRuntimeManager>(delegate { return new CommunicationRuntimeManager(); });

		private readonly Dictionary<CommunicationType, ICommunicationRuntime> _runtimes;
		private readonly Dictionary<string, ICommunicationRuntime> _instanceRuntimes;
		private readonly CommunicationHeartbeatRuntimeService _heartbeatRuntime;
		private bool _disposed;

		public static CommunicationRuntimeManager Instance
		{
			get { return _instance.Value; }
		}

		public event EventHandler<CommunicationStatusChangedEventArgs> StatusChanged;
		public event EventHandler<CommunicationDataReceivedEventArgs> DataReceived;
		public event EventHandler<Exception> ErrorOccurred;

		private CommunicationRuntimeManager()
		{
			_runtimes = new Dictionary<CommunicationType, ICommunicationRuntime>();
			_instanceRuntimes = new Dictionary<string, ICommunicationRuntime>(StringComparer.OrdinalIgnoreCase);
			_heartbeatRuntime = new CommunicationHeartbeatRuntimeService();
			RegisterRuntime(new TcpIpCommunicationService());
		}

		private void RegisterRuntime(ICommunicationRuntime runtime)
		{
			if (runtime == null)
			{
				return;
			}

			_runtimes[runtime.CommunicationType] = runtime;
			AttachRuntimeEvents(runtime);
		}

		private void AttachRuntimeEvents(ICommunicationRuntime runtime)
		{
			if (runtime == null)
			{
				return;
			}

			runtime.StatusChanged += Runtime_StatusChanged;
			runtime.DataReceived += Runtime_DataReceived;
			runtime.ErrorOccurred += Runtime_ErrorOccurred;
		}

		private void DetachRuntimeEvents(ICommunicationRuntime runtime)
		{
			if (runtime == null)
			{
				return;
			}

			runtime.StatusChanged -= Runtime_StatusChanged;
			runtime.DataReceived -= Runtime_DataReceived;
			runtime.ErrorOccurred -= Runtime_ErrorOccurred;
		}

		public ICommunicationRuntime GetRuntime(CommunicationType type)
		{
			if (type == CommunicationType.TcpIp)
			{
				ICommunicationRuntime tcpRuntime = GetPreferredTcpRuntime();
				if (tcpRuntime != null)
				{
					return tcpRuntime;
				}
			}

			ICommunicationRuntime runtime;

			if (_runtimes.TryGetValue(type, out runtime))
			{
				return runtime;
			}

			return null;
		}

		public TcpIpCommunicationService TcpIp
		{
			get { return GetRuntime(CommunicationType.TcpIp) as TcpIpCommunicationService; }
		}

		public ICommunicationRuntime GetRuntime(string instanceName)
		{
			if (string.IsNullOrWhiteSpace(instanceName))
			{
				return null;
			}

			ICommunicationRuntime runtime;
			if (_instanceRuntimes.TryGetValue(instanceName.Trim(), out runtime))
			{
				return runtime;
			}

			return null;
		}

		private ICommunicationRuntime GetPreferredTcpRuntime()
		{
			foreach (KeyValuePair<string, ICommunicationRuntime> pair in _instanceRuntimes)
			{
				if (pair.Value != null && pair.Value.IsConnected)
				{
					return pair.Value;
				}
			}

			foreach (KeyValuePair<string, ICommunicationRuntime> pair in _instanceRuntimes)
			{
				if (pair.Value != null && pair.Value.IsRunning)
				{
					return pair.Value;
				}
			}

			return null;
		}

		public void StartInstance(CommunicationInstanceConfig instance)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("CommunicationRuntimeManager");
			}

			if (instance == null || instance.CommunicationType != CommunicationType.TcpIp)
			{
				return;
			}

			ICommunicationRuntime runtime = GetOrCreateTcpInstanceRuntime(instance.InstanceName);
			CommunicationConfig config = BuildTcpRuntimeConfig(instance);
			runtime.Start(config);
		}

		public void StopInstance(string instanceName)
		{
			ICommunicationRuntime runtime = GetRuntime(instanceName);
			if (runtime != null)
			{
				runtime.Stop();
			}
		}

		public bool IsConnected(string instanceName)
		{
			ICommunicationRuntime runtime = GetRuntime(instanceName);
			return runtime != null && runtime.IsConnected;
		}

		public bool IsRunning(string instanceName)
		{
			ICommunicationRuntime runtime = GetRuntime(instanceName);
			return runtime != null && runtime.IsRunning;
		}

		public bool SendString(string instanceName, string text)
		{
			ICommunicationRuntime runtime = GetRuntime(instanceName);

			if (runtime == null)
			{
				return false;
			}

			return runtime.SendString(text);
		}

		public bool SendTcpString(string instanceName, string text)
		{
			return SendString(instanceName, text);
		}

		private ICommunicationRuntime GetOrCreateTcpInstanceRuntime(string instanceName)
		{
			string key = string.IsNullOrWhiteSpace(instanceName)
				? "TCPIP_01"
				: instanceName.Trim();

			ICommunicationRuntime runtime;
			if (_instanceRuntimes.TryGetValue(key, out runtime))
			{
				return runtime;
			}

			runtime = new TcpIpCommunicationService(key);
			_instanceRuntimes[key] = runtime;
			AttachRuntimeEvents(runtime);
			return runtime;
		}

		private static CommunicationConfig BuildTcpRuntimeConfig(CommunicationInstanceConfig instance)
		{
			CommunicationConfig config = new CommunicationConfig();
			config.SelectedType = CommunicationType.TcpIp;
			config.TcpIp = instance == null || instance.TcpIp == null
				? new TcpIpConfig()
				: instance.TcpIp;
			config.Profinet = new ProfinetConfig();
			config.S7 = new S7Config();
			config.Instances = new List<CommunicationInstanceConfig>();
			if (instance != null)
			{
				config.Instances.Add(instance);
			}

			return config;
		}

		public void StartFromSavedConfig()
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			Start(config);
		}

		public void Start(CommunicationConfig config)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("CommunicationRuntimeManager");
			}

			if (config == null)
			{
				config = new CommunicationConfig();
			}

			bool startedTcpInstances = StartConfiguredTcpInstances(config);
			ICommunicationRuntime tcp = null;
			_runtimes.TryGetValue(CommunicationType.TcpIp, out tcp);

			if (tcp != null)
			{
				if (startedTcpInstances)
				{
					tcp.Stop();
				}
				else
				{
					tcp.Start(config);
				}
			}

			_heartbeatRuntime.Start(config);

			// 后续扩展：
			// ICommunicationRuntime profinet = GetRuntime(CommunicationType.Profinet);
			// if (profinet != null) profinet.Start(config);
			//
			// ICommunicationRuntime s7 = GetRuntime(CommunicationType.S7);
			// if (s7 != null) s7.Start(config);
		}

		private bool StartConfiguredTcpInstances(CommunicationConfig config)
		{
			if (config == null || config.Instances == null || config.Instances.Count <= 0)
			{
				return false;
			}

			bool started = false;
			foreach (CommunicationInstanceConfig instance in config.Instances)
			{
				if (instance == null ||
					instance.CommunicationType != CommunicationType.TcpIp ||
					instance.TcpIp == null ||
					!instance.TcpIp.Enabled)
				{
					continue;
				}

				StartInstance(instance);
				started = true;
			}

			return started;
		}

		public void Restart()
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			Stop();
			Start(config);
		}

		public void Stop()
		{
			_heartbeatRuntime.Stop();

			foreach (KeyValuePair<CommunicationType, ICommunicationRuntime> pair in _runtimes)
			{
				if (pair.Value != null)
				{
					pair.Value.Stop();
				}
			}

			foreach (KeyValuePair<string, ICommunicationRuntime> pair in _instanceRuntimes)
			{
				if (pair.Value != null)
				{
					pair.Value.Stop();
				}
			}
		}

		public bool IsConnected(CommunicationType type)
		{
			ICommunicationRuntime runtime = GetRuntime(type);
			return runtime != null && runtime.IsConnected;
		}

		public bool SendString(CommunicationType type, string text)
		{
			ICommunicationRuntime runtime = GetRuntime(type);

			if (runtime == null)
			{
				return false;
			}

			return runtime.SendString(text);
		}

		public bool SendTcpString(string text)
		{
			return SendString(CommunicationType.TcpIp, text);
		}

		private void Runtime_StatusChanged(object sender, CommunicationStatusChangedEventArgs e)
		{
			EventHandler<CommunicationStatusChangedEventArgs> handler = StatusChanged;

			if (handler != null)
			{
				handler(sender, e);
			}
		}

		private void Runtime_DataReceived(object sender, CommunicationDataReceivedEventArgs e)
		{
			EventHandler<CommunicationDataReceivedEventArgs> handler = DataReceived;

			if (handler != null)
			{
				handler(sender, e);
			}
		}

		private void Runtime_ErrorOccurred(object sender, Exception e)
		{
			EventHandler<Exception> handler = ErrorOccurred;

			if (handler != null)
			{
				handler(sender, e);
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			_heartbeatRuntime.Stop();

			foreach (KeyValuePair<CommunicationType, ICommunicationRuntime> pair in _runtimes)
			{
				if (pair.Value != null)
				{
					DetachRuntimeEvents(pair.Value);
					pair.Value.Dispose();
				}
			}

			foreach (KeyValuePair<string, ICommunicationRuntime> pair in _instanceRuntimes)
			{
				if (pair.Value != null)
				{
					DetachRuntimeEvents(pair.Value);
					pair.Value.Dispose();
				}
			}

			_runtimes.Clear();
			_instanceRuntimes.Clear();
		}
	}

	internal sealed class CommunicationHeartbeatRuntimeService : IDisposable
	{
		private readonly object _syncRoot = new object();
		private readonly RuntimeCommunicationOutputService _outputService = new RuntimeCommunicationOutputService();
		private Timer _timer;
		private CommunicationHeartbeatConfig _heartbeat;
		private string _protocolName = string.Empty;
		private string _instanceName = string.Empty;
		private bool _disposed;
		private int _sending;

		public void Start(CommunicationConfig config)
		{
			Stop();

			if (_disposed || config == null || config.TcpIp == null || config.TcpIp.Heartbeat == null)
			{
				return;
			}

			if (!config.TcpIp.Enabled || !config.TcpIp.Heartbeat.Enabled)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(config.TcpIp.Heartbeat.OutputName))
			{
				return;
			}

			CommunicationHeartbeatConfig heartbeat = CloneHeartbeat(config.TcpIp.Heartbeat);
			int interval = Math.Max(heartbeat.IntervalMs, 50);

			lock (_syncRoot)
			{
				_protocolName = "TCP/IP";
				_instanceName = CommunicationRuntimeNaming.GetDefaultInstanceName("TCP/IP", config);
				_heartbeat = heartbeat;
				_timer = new Timer(TimerCallback, null, interval, interval);
			}
		}

		public void Stop()
		{
			Timer oldTimer;

			lock (_syncRoot)
			{
				oldTimer = _timer;
				_timer = null;
				_heartbeat = null;
				_protocolName = string.Empty;
				_instanceName = string.Empty;
			}

			if (oldTimer != null)
			{
				try
				{
					oldTimer.Dispose();
				}
				catch
				{
				}
			}
		}

		private void TimerCallback(object state)
		{
			if (Interlocked.Exchange(ref _sending, 1) == 1)
			{
				return;
			}

			try
			{
				CommunicationHeartbeatConfig heartbeat;
				string protocolName;
				string instanceName;

				lock (_syncRoot)
				{
					if (_timer == null || _heartbeat == null)
					{
						return;
					}

					heartbeat = CloneHeartbeat(_heartbeat);
					protocolName = _protocolName;
					instanceName = _instanceName;
				}

				if (!heartbeat.Enabled || string.IsNullOrWhiteSpace(heartbeat.OutputName))
				{
					return;
				}

				if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) &&
					!CommunicationRuntimeManager.Instance.IsConnected(CommunicationType.TcpIp))
				{
					return;
				}

				bool sent = _outputService.SendHeartbeatOutput(
					protocolName,
					heartbeat.OutputName,
					heartbeat.HeartbeatText);

				if (!sent)
				{
					RuntimeLogStore.Append(
						DateTime.Now,
						RuntimeLogCategory.Communication,
						"Heartbeat output failed. Communication=" +
						CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName) +
						", Output=" + heartbeat.OutputName,
						true);
				}
			}
			finally
			{
				Interlocked.Exchange(ref _sending, 0);
			}
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
}
