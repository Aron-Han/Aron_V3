using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Aron_V3
{
	public enum DatabaseFieldDataFormat
	{
		String = 0,
		Text = 0,
		Int = 1,
		Double = 2,
		Bool = 3,
		DateTime = 4
	}

	[Serializable]
	public class DatabaseFieldConfig
	{
		[XmlAttribute]
		public bool Enabled { get; set; }

		[XmlAttribute]
		public string InputName { get; set; }

		[XmlAttribute]
		public DatabaseFieldDataFormat DataFormat { get; set; }

		[XmlAttribute]
		public string LengthPrecision { get; set; }

		[XmlAttribute]
		public string DefaultValue { get; set; }

		[XmlAttribute]
		public bool Required { get; set; }

		[XmlAttribute]
		public bool Indexed { get; set; }

		[XmlAttribute]
		public string Remark { get; set; }

		public DatabaseFieldConfig()
		{
			Enabled = true;
			InputName = string.Empty;
			DataFormat = DatabaseFieldDataFormat.String;
			LengthPrecision = string.Empty;
			DefaultValue = string.Empty;
			Required = false;
			Indexed = false;
			Remark = string.Empty;
		}
	}

	[XmlRoot("DatabaseConfig")]
	public class DatabaseConfig
	{
		[XmlAttribute]
		public string DatabasePath { get; set; }

		[XmlAttribute]
		public int RetentionDays { get; set; }

		[XmlAttribute]
		public string TableName { get; set; }

		[XmlArray("Fields")]
		[XmlArrayItem("Field")]
		public List<DatabaseFieldConfig> Fields { get; set; }

		public DatabaseConfig()
		{
			DatabasePath = string.Empty;
			RetentionDays = 365;
			TableName = "TaskRecord";
			Fields = new List<DatabaseFieldConfig>();
		}
	}

	public class DatabaseWriteRequest
	{
		public DateTime Time { get; private set; }
		public Dictionary<string, object> Values { get; private set; }

		public DatabaseWriteRequest(IDictionary<string, object> values)
			: this(DateTime.Now, values)
		{
		}

		public DatabaseWriteRequest(DateTime time, IDictionary<string, object> values)
		{
			Time = time;
			Values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			if (values == null)
			{
				return;
			}

			foreach (KeyValuePair<string, object> pair in values)
			{
				if (string.IsNullOrWhiteSpace(pair.Key))
				{
					continue;
				}

				Values[pair.Key.Trim()] = pair.Value;
			}
		}
	}

	public class DatabaseQueryOptions
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string FieldKeyword { get; set; }

		public DatabaseQueryOptions()
		{
			StartTime = DateTime.Today;
			EndTime = DateTime.Today.AddDays(1).AddMilliseconds(-1);
			FieldKeyword = string.Empty;
		}
	}

	public class DatabaseQueryResult
	{
		public List<string> Columns { get; private set; }
		public List<Dictionary<string, string>> Rows { get; private set; }

		public DatabaseQueryResult()
		{
			Columns = new List<string>();
			Rows = new List<Dictionary<string, string>>();
		}
	}

	public static class DatabaseConfigStore
	{
		public static event EventHandler ConfigChanged;

		public static string ConfigFile
		{
			get { return Path.Combine(ProjectPathStore.DatabaseRoot, "DatabaseConfig.xml"); }
		}

		public static DatabaseConfig LoadOrCreateDefault()
		{
			DatabaseConfig config = File.Exists(ConfigFile)
				? XmlConfigHelper.Load<DatabaseConfig>(ConfigFile)
				: CreateDefault();

			if (config == null)
			{
				config = CreateDefault();
			}

			Normalize(config);
			return config;
		}

		public static void Save(DatabaseConfig config)
		{
			if (config == null)
			{
				config = CreateDefault();
			}

			Normalize(config);
			XmlConfigHelper.Save(ConfigFile, config);

			EventHandler handler = ConfigChanged;
			if (handler != null)
			{
				handler(null, EventArgs.Empty);
			}

			DiagnosticLogStore.Append(
				DiagnosticLogLevel.Info,
				"Config",
				"Database config saved.",
				new Dictionary<string, string> { { "path", ConfigFile } });
		}

		private static DatabaseConfig CreateDefault()
		{
			DatabaseConfig config = new DatabaseConfig();
			config.DatabasePath = Path.Combine("Project", "database", "vision_records.db");
			config.RetentionDays = 365;
			config.TableName = "TaskRecord";
			config.Fields.Add(CreateField("TaskName", DatabaseFieldDataFormat.String, string.Empty, string.Empty, true, true, "当前 Task 名称"));
			config.Fields.Add(CreateField("JobID", DatabaseFieldDataFormat.Int, "4", "0", false, true, "程序号"));
			config.Fields.Add(CreateField("ResultOK", DatabaseFieldDataFormat.Bool, "1", "False", false, false, "最终结果"));
			config.Fields.Add(CreateField("CostMs", DatabaseFieldDataFormat.Double, "10,1", "0", false, false, "Task 总耗时"));
			config.Fields.Add(CreateField("Barcode", DatabaseFieldDataFormat.String, string.Empty, string.Empty, false, true, "条码或产品 ID"));
			config.Fields.Add(CreateField("PosID", DatabaseFieldDataFormat.Int, "4", "0", false, false, "位置/工位"));
			config.Fields.Add(CreateField("Sum_Result", DatabaseFieldDataFormat.Double, "10,3", string.Empty, false, false, "算法输出值"));
			config.Fields.Add(CreateField("ErrorCode", DatabaseFieldDataFormat.Int, "4", "0", false, false, "错误码"));
			return config;
		}

		private static DatabaseFieldConfig CreateField(
			string name,
			DatabaseFieldDataFormat format,
			string lengthPrecision,
			string defaultValue,
			bool required,
			bool indexed,
			string remark)
		{
			return new DatabaseFieldConfig
			{
				Enabled = true,
				InputName = name,
				DataFormat = format,
				LengthPrecision = lengthPrecision,
				DefaultValue = defaultValue,
				Required = required,
				Indexed = indexed,
				Remark = remark
			};
		}

		public static void Normalize(DatabaseConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(config.DatabasePath))
			{
				config.DatabasePath = Path.Combine("Project", "database", "vision_records.db");
			}

			if (config.RetentionDays <= 0)
			{
				config.RetentionDays = 365;
			}

			if (string.IsNullOrWhiteSpace(config.TableName))
			{
				config.TableName = "TaskRecord";
			}

			config.DatabasePath = config.DatabasePath.Trim();
			config.TableName = config.TableName.Trim();

			if (config.Fields == null)
			{
				config.Fields = new List<DatabaseFieldConfig>();
			}

			HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = config.Fields.Count - 1; i >= 0; i--)
			{
				DatabaseFieldConfig field = config.Fields[i];
				if (field == null || string.IsNullOrWhiteSpace(field.InputName))
				{
					config.Fields.RemoveAt(i);
					continue;
				}

				field.InputName = field.InputName.Trim();
				field.LengthPrecision = field.LengthPrecision ?? string.Empty;
				field.DefaultValue = field.DefaultValue ?? string.Empty;
				field.Remark = field.Remark ?? string.Empty;

				if (!names.Add(field.InputName))
				{
					config.Fields.RemoveAt(i);
				}
			}
		}
	}

	public static class DatabaseLocalRecordStore
	{
		private const string LinePrefix = "ARONDB1";
		private const char KeyValueSeparator = ':';
		private const char LegacyKeyValueSeparator = '=';
		private static readonly object WriteSyncRoot = new object();
		private static DateTime _lastCleanupDate = DateTime.MinValue;

		public static string ResolveDatabasePath(DatabaseConfig config)
		{
			string configuredPath = config == null ? string.Empty : config.DatabasePath;
			if (string.IsNullOrWhiteSpace(configuredPath))
			{
				configuredPath = Path.Combine("Project", "database", "vision_records.db");
			}

			configuredPath = configuredPath.Trim();

			if (Path.IsPathRooted(configuredPath))
			{
				return configuredPath;
			}

			string normalized = configuredPath.Replace('/', Path.DirectorySeparatorChar);
			string projectPrefix = "Project" + Path.DirectorySeparatorChar;
			if (normalized.StartsWith(projectPrefix, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(normalized, "Project", StringComparison.OrdinalIgnoreCase))
			{
				return Path.Combine(Application.StartupPath, normalized);
			}

			return Path.Combine(ProjectPathStore.DatabaseRoot, normalized);
		}

		public static void EnsureStorage(DatabaseConfig config)
		{
			string basePath = ResolveDatabasePath(config);
			string folder = Path.GetDirectoryName(basePath);
			if (!string.IsNullOrWhiteSpace(folder))
			{
				Directory.CreateDirectory(folder);
			}
		}

		public static void AppendRecord(DatabaseConfig config, DatabaseWriteRequest request)
		{
			if (request == null)
			{
				return;
			}

			if (config == null)
			{
				config = DatabaseConfigStore.LoadOrCreateDefault();
			}

			DatabaseConfigStore.Normalize(config);
			EnsureStorage(config);
			CleanupOldFiles(config, request.Time);

			Dictionary<string, string> values = BuildRecordValues(config, request);
			string line = BuildLine(request.Time, values);
			string path = GetDailyRecordFile(config, request.Time);

			lock (WriteSyncRoot)
			{
				using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
				using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
				{
					writer.WriteLine(line);
				}
			}
		}

		public static DatabaseQueryResult Query(DatabaseConfig config, DatabaseQueryOptions options)
		{
			if (config == null)
			{
				config = DatabaseConfigStore.LoadOrCreateDefault();
			}

			if (options == null)
			{
				options = new DatabaseQueryOptions();
			}

			DatabaseConfigStore.Normalize(config);
			DatabaseQueryResult result = new DatabaseQueryResult();

			DateTime start = options.StartTime <= DateTime.MinValue ? DateTime.Today : options.StartTime;
			DateTime end = options.EndTime <= start ? start.Date.AddDays(1).AddMilliseconds(-1) : options.EndTime;
			string keyword = (options.FieldKeyword ?? string.Empty).Trim();

			List<DatabaseFieldConfig> fields = config.Fields
				.Where(x => x != null && x.Enabled)
				.Where(x => string.IsNullOrWhiteSpace(keyword) ||
					x.InputName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
				.ToList();

			result.Columns.Add("RecordTime");
			foreach (DatabaseFieldConfig field in fields)
			{
				if (!result.Columns.Any(x => string.Equals(x, field.InputName, StringComparison.OrdinalIgnoreCase)))
				{
					result.Columns.Add(field.InputName);
				}
			}

			foreach (string file in GetRecordFilesForRange(config, start, end))
			{
				foreach (DatabaseRecordLine record in ReadRecords(file))
				{
					if (record.Time < start || record.Time > end)
					{
						continue;
					}

					Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
					row["RecordTime"] = record.Time.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
					foreach (DatabaseFieldConfig field in fields)
					{
						string value;
						row[field.InputName] = record.Values.TryGetValue(field.InputName, out value) ? value : string.Empty;
					}
					result.Rows.Add(row);
				}
			}

			List<Dictionary<string, string>> sortedRows = result.Rows
				.OrderByDescending(x =>
				{
					DateTime t;
					return DateTime.TryParse(x.ContainsKey("RecordTime") ? x["RecordTime"] : string.Empty, out t) ? t : DateTime.MinValue;
				})
				.ToList();
			result.Rows.Clear();
			result.Rows.AddRange(sortedRows);

			return result;
		}

		public static void ExportCsv(DatabaseQueryResult result, string filePath)
		{
			if (result == null)
			{
				result = new DatabaseQueryResult();
			}

			string dir = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrWhiteSpace(dir))
			{
				Directory.CreateDirectory(dir);
			}

			using (StreamWriter writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
			{
				writer.WriteLine(string.Join(",", result.Columns.Select(EscapeCsv)));
				foreach (Dictionary<string, string> row in result.Rows)
				{
					List<string> cells = new List<string>();
					foreach (string column in result.Columns)
					{
						string value;
						cells.Add(EscapeCsv(row != null && row.TryGetValue(column, out value) ? value : string.Empty));
					}
					writer.WriteLine(string.Join(",", cells));
				}
			}
		}

		public static string GetStorageFolder(DatabaseConfig config)
		{
			string path = ResolveDatabasePath(config);
			string folder = Path.GetDirectoryName(path);
			return string.IsNullOrWhiteSpace(folder) ? ProjectPathStore.DatabaseRoot : folder;
		}

		private static Dictionary<string, string> BuildRecordValues(DatabaseConfig config, DatabaseWriteRequest request)
		{
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (DatabaseFieldConfig field in config.Fields.Where(x => x != null && x.Enabled))
			{
				object raw;
				string text;
				if (request.Values != null && request.Values.TryGetValue(field.InputName, out raw))
				{
					text = FormatValue(raw, field);
				}
				else
				{
					text = field.DefaultValue ?? string.Empty;
				}

				result[field.InputName] = text;
			}

			return result;
		}

		private static string FormatValue(object raw, DatabaseFieldConfig field)
		{
			if (raw == null)
			{
				return string.Empty;
			}

			try
			{
				switch (field == null ? DatabaseFieldDataFormat.String : field.DataFormat)
				{
					case DatabaseFieldDataFormat.Int:
						int intValue;
						if (int.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out intValue))
						{
							return intValue.ToString(CultureInfo.InvariantCulture);
						}
						break;
					case DatabaseFieldDataFormat.Double:
						double doubleValue;
						if (double.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue))
						{
							return doubleValue.ToString("0.########", CultureInfo.InvariantCulture);
						}
						break;
					case DatabaseFieldDataFormat.Bool:
						bool boolValue;
						string boolText = Convert.ToString(raw, CultureInfo.InvariantCulture);
						if (bool.TryParse(boolText, out boolValue))
						{
							return boolValue ? "True" : "False";
						}
						if (boolText == "1")
						{
							return "True";
						}
						if (boolText == "0")
						{
							return "False";
						}
						break;
					case DatabaseFieldDataFormat.DateTime:
						DateTime dateTimeValue;
						if (raw is DateTime)
						{
							return ((DateTime)raw).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
						}
						if (DateTime.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), out dateTimeValue))
						{
							return dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
						}
						break;
				}
			}
			catch
			{
			}

			return Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty;
		}

		private static string BuildLine(DateTime time, Dictionary<string, string> values)
		{
			List<string> parts = new List<string>();
			parts.Add(LinePrefix);
			parts.Add(time.Ticks.ToString(CultureInfo.InvariantCulture));

			foreach (KeyValuePair<string, string> pair in values.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
			{
				parts.Add(ToBase64(pair.Key) + KeyValueSeparator + ToBase64(pair.Value ?? string.Empty));
			}

			return string.Join("\t", parts);
		}

		private static IEnumerable<DatabaseRecordLine> ReadRecords(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
			{
				yield break;
			}

			string[] lines;
			try
			{
				using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true))
				{
					List<string> list = new List<string>();
					while (!reader.EndOfStream)
					{
						list.Add(reader.ReadLine());
					}
					lines = list.ToArray();
				}
			}
			catch
			{
				yield break;
			}

			foreach (string line in lines)
			{
				DatabaseRecordLine record;
				if (TryParseLine(line, out record))
				{
					yield return record;
				}
			}
		}

		private static bool TryParseLine(string line, out DatabaseRecordLine record)
		{
			record = null;
			if (string.IsNullOrWhiteSpace(line))
			{
				return false;
			}

			string[] parts = line.Split('\t');
			if (parts.Length < 2 || parts[0] != LinePrefix)
			{
				return false;
			}

			long ticks;
			if (!long.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out ticks))
			{
				return false;
			}

			record = new DatabaseRecordLine();
			record.Time = new DateTime(ticks);

			for (int i = 2; i < parts.Length; i++)
			{
				string item = parts[i] ?? string.Empty;
				string key;
				string value;
				if (!TryParseRecordValuePart(item, out key, out value))
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(key))
				{
					continue;
				}
				record.Values[key] = value;
			}

			return true;
		}

		private static bool TryParseRecordValuePart(string item, out string key, out string value)
		{
			key = string.Empty;
			value = string.Empty;

			if (string.IsNullOrEmpty(item))
			{
				return false;
			}

			int separatorIndex = item.IndexOf(KeyValueSeparator);
			if (separatorIndex > 0)
			{
				return TryDecodeRecordValuePart(
					item.Substring(0, separatorIndex),
					item.Substring(separatorIndex + 1),
					out key,
					out value);
			}

			for (int i = 0; i < item.Length; i++)
			{
				if (item[i] != LegacyKeyValueSeparator)
				{
					continue;
				}

				if (TryDecodeRecordValuePart(
					item.Substring(0, i),
					item.Substring(i + 1),
					out key,
					out value))
				{
					return true;
				}
			}

			return false;
		}

		private static bool TryDecodeRecordValuePart(
			string encodedKey,
			string encodedValue,
			out string key,
			out string value)
		{
			key = string.Empty;
			value = string.Empty;

			string decodedKey;
			if (!TryFromBase64(encodedKey, out decodedKey) || string.IsNullOrWhiteSpace(decodedKey))
			{
				return false;
			}

			string decodedValue;
			if (!TryFromBase64(encodedValue, out decodedValue))
			{
				return false;
			}

			key = decodedKey;
			value = decodedValue;
			return true;
		}

		private static IEnumerable<string> GetRecordFilesForRange(DatabaseConfig config, DateTime start, DateTime end)
		{
			DateTime day = start.Date;
			DateTime lastDay = end.Date;
			while (day <= lastDay)
			{
				string path = GetDailyRecordFile(config, day);
				if (File.Exists(path))
				{
					yield return path;
				}
				day = day.AddDays(1);
			}
		}

		private static string GetDailyRecordFile(DatabaseConfig config, DateTime time)
		{
			string basePath = ResolveDatabasePath(config);
			string folder = Path.GetDirectoryName(basePath);
			if (string.IsNullOrWhiteSpace(folder))
			{
				folder = ProjectPathStore.DatabaseRoot;
			}

			string name = Path.GetFileNameWithoutExtension(basePath);
			if (string.IsNullOrWhiteSpace(name))
			{
				name = "vision_records";
			}

			string ext = Path.GetExtension(basePath);
			if (string.IsNullOrWhiteSpace(ext))
			{
				ext = ".db";
			}

			return Path.Combine(folder, name + "_" + time.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ext);
		}

		private static void CleanupOldFiles(DatabaseConfig config, DateTime now)
		{
			if (config == null || config.RetentionDays <= 0 || _lastCleanupDate.Date == now.Date)
			{
				return;
			}

			_lastCleanupDate = now.Date;
			try
			{
				string folder = GetStorageFolder(config);
				if (!Directory.Exists(folder))
				{
					return;
				}

				string basePath = ResolveDatabasePath(config);
				string name = Path.GetFileNameWithoutExtension(basePath);
				string ext = Path.GetExtension(basePath);
				DateTime cutoff = now.Date.AddDays(-config.RetentionDays);
				foreach (string file in Directory.GetFiles(folder, name + "_*" + ext))
				{
					if (File.GetLastWriteTime(file).Date < cutoff)
					{
						File.Delete(file);
					}
				}
			}
			catch (Exception ex)
			{
				RuntimeLogStore.Append(DateTime.Now, RuntimeLogCategory.Step, "Database cleanup failed. Error=" + ex.Message, true);
			}
		}

		private static string ToBase64(string text)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(text ?? string.Empty));
		}

		private static bool TryFromBase64(string text, out string value)
		{
			value = string.Empty;
			try
			{
				value = Encoding.UTF8.GetString(Convert.FromBase64String(text ?? string.Empty));
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static string EscapeCsv(string value)
		{
			value = value ?? string.Empty;
			if (value.IndexOfAny(new char[] { ',', '"', '\r', '\n' }) < 0)
			{
				return value;
			}

			return "\"" + value.Replace("\"", "\"\"") + "\"";
		}

		private sealed class DatabaseRecordLine
		{
			public DateTime Time { get; set; }
			public Dictionary<string, string> Values { get; private set; }

			public DatabaseRecordLine()
			{
				Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}
		}
	}
}
