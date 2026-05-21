using System;
using System.Collections.Generic;

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
			RegisterRuntime(new TcpIpCommunicationService());
		}

		private void RegisterRuntime(ICommunicationRuntime runtime)
		{
			if (runtime == null)
			{
				return;
			}

			_runtimes[runtime.CommunicationType] = runtime;

			runtime.StatusChanged += Runtime_StatusChanged;
			runtime.DataReceived += Runtime_DataReceived;
			runtime.ErrorOccurred += Runtime_ErrorOccurred;
		}

		public ICommunicationRuntime GetRuntime(CommunicationType type)
		{
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

			ICommunicationRuntime tcp = GetRuntime(CommunicationType.TcpIp);

			if (tcp != null)
			{
				tcp.Start(config);
			}

			// 后续扩展：
			// ICommunicationRuntime profinet = GetRuntime(CommunicationType.Profinet);
			// if (profinet != null) profinet.Start(config);
			//
			// ICommunicationRuntime s7 = GetRuntime(CommunicationType.S7);
			// if (s7 != null) s7.Start(config);
		}

		public void Restart()
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			Stop();
			Start(config);
		}

		public void Stop()
		{
			foreach (KeyValuePair<CommunicationType, ICommunicationRuntime> pair in _runtimes)
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

			foreach (KeyValuePair<CommunicationType, ICommunicationRuntime> pair in _runtimes)
			{
				if (pair.Value != null)
				{
					pair.Value.StatusChanged -= Runtime_StatusChanged;
					pair.Value.DataReceived -= Runtime_DataReceived;
					pair.Value.ErrorOccurred -= Runtime_ErrorOccurred;
					pair.Value.Dispose();
				}
			}

			_runtimes.Clear();
		}
	}
}
