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

			if (!instance.Enabled || instance.TcpIp == null || !instance.TcpIp.Enabled)
			{
				StopInstance(instance.InstanceName);
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

			bool hasConfiguredTcpInstances = HasConfiguredTcpInstances(config);
			bool startedTcpInstances = StartConfiguredTcpInstances(config);
			ICommunicationRuntime tcp = null;
			_runtimes.TryGetValue(CommunicationType.TcpIp, out tcp);

			if (tcp != null)
			{
				if (hasConfiguredTcpInstances)
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

		public void ReloadHeartbeatConfig(CommunicationConfig config)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("CommunicationRuntimeManager");
			}

			if (config == null)
			{
				config = CommunicationConfigStore.LoadOrCreateDefault();
			}

			_heartbeatRuntime.Start(config);
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
					!instance.Enabled ||
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

		private static bool HasConfiguredTcpInstances(CommunicationConfig config)
		{
			if (config == null || config.Instances == null)
			{
				return false;
			}

			foreach (CommunicationInstanceConfig instance in config.Instances)
			{
				if (instance != null && instance.CommunicationType == CommunicationType.TcpIp)
				{
					return true;
				}
			}

			return false;
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
		private readonly Dictionary<string, HeartbeatRuntimeEntry> _entries =
			new Dictionary<string, HeartbeatRuntimeEntry>(StringComparer.OrdinalIgnoreCase);
		private bool _disposed;

		public void Start(CommunicationConfig config)
		{
			List<HeartbeatRuntimeEntry> entries = BuildHeartbeatEntries(config);

			lock (_syncRoot)
			{
				DisposeEntriesLocked();

				if (_disposed)
				{
					return;
				}

				foreach (HeartbeatRuntimeEntry entry in entries)
				{
					if (entry == null || entry.Heartbeat == null)
					{
						continue;
					}

					int interval = Math.Max(entry.Heartbeat.IntervalMs, 50);
					entry.Active = true;
					entry.Timer = new Timer(TimerCallback, entry, interval, interval);
					_entries[entry.Key] = entry;
				}
			}
		}

		public void Stop()
		{
			lock (_syncRoot)
			{
				DisposeEntriesLocked();
			}
		}

		private void TimerCallback(object state)
		{
			HeartbeatRuntimeEntry entry = state as HeartbeatRuntimeEntry;
			if (entry == null || !entry.Active || entry.Heartbeat == null)
			{
				return;
			}

			if (Interlocked.Exchange(ref entry.Sending, 1) == 1)
			{
				return;
			}

			try
			{
				CommunicationHeartbeatConfig heartbeat = entry.Heartbeat;
				string protocolName = entry.ProtocolName;
				string instanceName = entry.InstanceName;

				if (!entry.Active ||
					!heartbeat.Enabled ||
					string.IsNullOrWhiteSpace(heartbeat.OutputName))
				{
					return;
				}

				if (protocolName.Equals("TCP/IP", StringComparison.OrdinalIgnoreCase) &&
					!CommunicationRuntimeManager.Instance.IsConnected(instanceName))
				{
					return;
				}

				bool sent = _outputService.SendHeartbeatOutput(
					protocolName,
					instanceName,
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
				Interlocked.Exchange(ref entry.Sending, 0);
			}
		}

		private void DisposeEntriesLocked()
		{
			foreach (HeartbeatRuntimeEntry entry in _entries.Values)
			{
				if (entry == null)
				{
					continue;
				}

				entry.Active = false;
				if (entry.Timer == null)
				{
					continue;
				}

				try
				{
					entry.Timer.Dispose();
				}
				catch
				{
				}

				entry.Timer = null;
			}

			_entries.Clear();
		}

		private static List<HeartbeatRuntimeEntry> BuildHeartbeatEntries(CommunicationConfig config)
		{
			List<HeartbeatRuntimeEntry> entries = new List<HeartbeatRuntimeEntry>();
			if (config == null)
			{
				return entries;
			}

			HashSet<string> usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			bool hasTcpInstances = false;

			if (config.Instances != null)
			{
				foreach (CommunicationInstanceConfig instance in config.Instances)
				{
					if (instance == null || instance.CommunicationType != CommunicationType.TcpIp)
					{
						continue;
					}

					hasTcpInstances = true;

					TcpIpConfig tcpIp = instance.TcpIp;
					if (!instance.Enabled || tcpIp == null || !tcpIp.Enabled)
					{
						continue;
					}

					CommunicationHeartbeatConfig heartbeat = tcpIp.Heartbeat;
					if (!IsHeartbeatEnabled(heartbeat))
					{
						continue;
					}

					string instanceName = string.IsNullOrWhiteSpace(instance.InstanceName)
						? CommunicationRuntimeNaming.GetDefaultInstanceName("TCP/IP", config)
						: instance.InstanceName.Trim();

					AddHeartbeatEntry(entries, usedKeys, "TCP/IP", instanceName, heartbeat);
				}
			}

			if (!hasTcpInstances &&
				entries.Count == 0 &&
				config.TcpIp != null &&
				config.TcpIp.Enabled &&
				IsHeartbeatEnabled(config.TcpIp.Heartbeat))
			{
				AddHeartbeatEntry(
					entries,
					usedKeys,
					"TCP/IP",
					CommunicationRuntimeNaming.GetDefaultInstanceName("TCP/IP", config),
					config.TcpIp.Heartbeat);
			}

			return entries;
		}

		private static void AddHeartbeatEntry(
			List<HeartbeatRuntimeEntry> entries,
			HashSet<string> usedKeys,
			string protocolName,
			string instanceName,
			CommunicationHeartbeatConfig heartbeat)
		{
			if (entries == null ||
				usedKeys == null ||
				string.IsNullOrWhiteSpace(protocolName) ||
				string.IsNullOrWhiteSpace(instanceName) ||
				!IsHeartbeatEnabled(heartbeat))
			{
				return;
			}

			string key = CommunicationRuntimeNaming.FormatCommunicationName(protocolName, instanceName);
			if (usedKeys.Contains(key))
			{
				return;
			}

			usedKeys.Add(key);

			HeartbeatRuntimeEntry entry = new HeartbeatRuntimeEntry();
			entry.Key = key;
			entry.ProtocolName = CommunicationRuntimeNaming.NormalizeProtocolName(protocolName);
			entry.InstanceName = instanceName.Trim();
			entry.Heartbeat = CloneHeartbeat(heartbeat);
			entries.Add(entry);
		}

		private static bool IsHeartbeatEnabled(CommunicationHeartbeatConfig heartbeat)
		{
			return heartbeat != null &&
				   heartbeat.Enabled &&
				   !string.IsNullOrWhiteSpace(heartbeat.OutputName);
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

		private sealed class HeartbeatRuntimeEntry
		{
			public string Key;
			public string ProtocolName;
			public string InstanceName;
			public CommunicationHeartbeatConfig Heartbeat;
			public Timer Timer;
			public volatile bool Active;
			public int Sending;
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
