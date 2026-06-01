using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Aron_V3
{
	public sealed class DatabaseRecordWriter : IDisposable
	{
		private static readonly Lazy<DatabaseRecordWriter> LazyInstance =
			new Lazy<DatabaseRecordWriter>(delegate { return new DatabaseRecordWriter(); });

		private readonly object _syncRoot = new object();
		private readonly BlockingCollection<DatabaseWriteRequest> _queue;
		private Thread _worker;
		private bool _started;
		private bool _disposed;

		public static DatabaseRecordWriter Instance
		{
			get { return LazyInstance.Value; }
		}

		private DatabaseRecordWriter()
		{
			_queue = new BlockingCollection<DatabaseWriteRequest>(
				new ConcurrentQueue<DatabaseWriteRequest>(),
				10000);
		}

		public void Start()
		{
			lock (_syncRoot)
			{
				if (_disposed || _started)
				{
					return;
				}

				_worker = new Thread(WorkerLoop);
				_worker.IsBackground = true;
				_worker.Name = "DatabaseRecordWriter";
				_started = true;
				_worker.Start();
			}
		}

		public bool Enqueue(IDictionary<string, object> values)
		{
			return Enqueue(new DatabaseWriteRequest(values));
		}

		public bool Enqueue(DatabaseWriteRequest request)
		{
			if (request == null)
			{
				return false;
			}

			try
			{
				Start();
				if (_disposed || _queue.IsAddingCompleted)
				{
					return false;
				}

				if (!_queue.TryAdd(request))
				{
					RuntimeLogStore.Append(
						DateTime.Now,
						RuntimeLogCategory.Step,
						"Database async write skipped. Error=Background queue is full.",
						true);
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				RuntimeLogStore.Append(
					DateTime.Now,
					RuntimeLogCategory.Step,
					"Database async write enqueue failed. Error=" + ex.Message,
					true);
				return false;
			}
		}

		private void WorkerLoop()
		{
			foreach (DatabaseWriteRequest request in _queue.GetConsumingEnumerable())
			{
				try
				{
					DatabaseConfig config = DatabaseConfigStore.LoadOrCreateDefault();
					DatabaseLocalRecordStore.AppendRecord(config, request);
				}
				catch (Exception ex)
				{
					RuntimeLogStore.Append(
						DateTime.Now,
						RuntimeLogCategory.Step,
						"Database async write failed. Error=" + ex.Message,
						true);
				}
			}
		}

		public void Dispose()
		{
			lock (_syncRoot)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;
				try
				{
					_queue.CompleteAdding();
				}
				catch
				{
				}
			}
		}
	}
}
