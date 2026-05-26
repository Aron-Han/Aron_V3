using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Aron_V3
{
	public enum GlobalVariableDataType
	{
		Int16 = 0,
		Int32 = 1,
		Byte = 2,
		Bit = 3,
		String = 4,
		Float = 5,
		Double = 6,
		Bool = 100,
		Int = 101,
		Decimal = 102
	}

	[Serializable]
	public class GlobalVariableItem
	{
		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public GlobalVariableDataType DataType { get; set; }

		[XmlAttribute]
		public bool RememberValue { get; set; }

		[XmlAttribute]
		public string CurrentValue { get; set; }

		[XmlAttribute]
		public string Mark { get; set; }

		public GlobalVariableItem()
		{
			Name = string.Empty;
			DataType = GlobalVariableDataType.String;
			RememberValue = false;
			CurrentValue = string.Empty;
			Mark = string.Empty;
		}
	}

	[XmlRoot("GlobalVariableConfig")]
	public class GlobalVariableConfig
	{
		[XmlArray("Variables")]
		[XmlArrayItem("Variable")]
		public List<GlobalVariableItem> Variables { get; set; }

		public GlobalVariableConfig()
		{
			Variables = new List<GlobalVariableItem>();
		}
	}

	public static class GlobalVariableStore
	{
		private static readonly object _syncRoot = new object();
		private static readonly Dictionary<string, string> _runtimeValues =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private static bool _initialized;

		public static event EventHandler VariablesChanged;

		public static string ConfigFile
		{
			get { return Path.Combine(ProjectPathStore.SystemConfigRoot, "GlobalVariables.xml"); }
		}

		public static GlobalVariableConfig LoadForEditing()
		{
			lock (_syncRoot)
			{
				GlobalVariableConfig config = LoadPersistedConfig();
				Normalize(config);

				if (!_initialized)
				{
					_runtimeValues.Clear();
					foreach (GlobalVariableItem item in config.Variables)
					{
						_runtimeValues[item.Name] = item.RememberValue ? item.CurrentValue : string.Empty;
					}
					_initialized = true;
				}

				foreach (GlobalVariableItem item in config.Variables)
				{
					string runtimeValue;
					if (_runtimeValues.TryGetValue(item.Name, out runtimeValue))
					{
						item.CurrentValue = runtimeValue;
					}
				}

				return config;
			}
		}

		public static void Save(GlobalVariableConfig config)
		{
			if (config == null)
			{
				config = new GlobalVariableConfig();
			}

			Normalize(config);

			lock (_syncRoot)
			{
				Dictionary<string, string> newValues =
					new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				foreach (GlobalVariableItem item in config.Variables)
				{
					newValues[item.Name] = item.CurrentValue ?? string.Empty;
				}

				_runtimeValues.Clear();
				foreach (KeyValuePair<string, string> pair in newValues)
				{
					_runtimeValues[pair.Key] = pair.Value;
				}
				_initialized = true;

				GlobalVariableConfig persisted = CloneForPersistence(config);
				XmlConfigHelper.Save(ConfigFile, persisted);
			}

			EventHandler handler = VariablesChanged;
			if (handler != null)
			{
				handler(null, EventArgs.Empty);
			}
		}

		public static List<string> GetVariableNames()
		{
			GlobalVariableConfig config = LoadForEditing();
			return config.Variables
				.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
				.Select(x => x.Name)
				.ToList();
		}

		public static bool TryGetValue(string name, out object value)
		{
			value = null;
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			GlobalVariableConfig config = LoadForEditing();
			GlobalVariableItem item = config.Variables.FirstOrDefault(x =>
				x != null && string.Equals(x.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

			if (item == null)
			{
				return false;
			}

			value = ConvertValue(item.CurrentValue, item.DataType);
			return true;
		}

		public static string GetValueText(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return string.Empty;
			}

			LoadForEditing();
			lock (_syncRoot)
			{
				string value;
				return _runtimeValues.TryGetValue(name.Trim(), out value) ? value : string.Empty;
			}
		}

		public static void SetValue(string name, object value)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return;
			}

			GlobalVariableConfig config = LoadForEditing();
			GlobalVariableItem item = config.Variables.FirstOrDefault(x =>
				x != null && string.Equals(x.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

			if (item == null)
			{
				return;
			}

			string text = FormatValue(value, item.DataType);
			lock (_syncRoot)
			{
				_runtimeValues[item.Name] = text;
				item.CurrentValue = text;

				if (item.RememberValue)
				{
					GlobalVariableConfig persisted = LoadPersistedConfig();
					Normalize(persisted);
					GlobalVariableItem savedItem = persisted.Variables.FirstOrDefault(x =>
						string.Equals(x.Name, item.Name, StringComparison.OrdinalIgnoreCase));
					if (savedItem != null)
					{
						savedItem.CurrentValue = text;
						XmlConfigHelper.Save(ConfigFile, CloneForPersistence(persisted));
					}
				}
			}

			EventHandler handler = VariablesChanged;
			if (handler != null)
			{
				handler(null, EventArgs.Empty);
			}
		}

		private static GlobalVariableConfig LoadPersistedConfig()
		{
			if (!File.Exists(ConfigFile))
			{
				return new GlobalVariableConfig();
			}

			return XmlConfigHelper.Load<GlobalVariableConfig>(ConfigFile);
		}

		private static void Normalize(GlobalVariableConfig config)
		{
			if (config.Variables == null)
			{
				config.Variables = new List<GlobalVariableItem>();
			}

			HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = config.Variables.Count - 1; i >= 0; i--)
			{
				GlobalVariableItem item = config.Variables[i];
				if (item == null || string.IsNullOrWhiteSpace(item.Name))
				{
					config.Variables.RemoveAt(i);
					continue;
				}

				item.Name = item.Name.Trim();
				item.CurrentValue = item.CurrentValue ?? string.Empty;
				item.Mark = item.Mark ?? string.Empty;
				if (item.DataType == GlobalVariableDataType.Bool)
				{
					item.DataType = GlobalVariableDataType.Bit;
				}
				else if (item.DataType == GlobalVariableDataType.Int)
				{
					item.DataType = GlobalVariableDataType.Int32;
				}
				else if (item.DataType == GlobalVariableDataType.Decimal)
				{
					item.DataType = GlobalVariableDataType.Double;
				}
				if (!names.Add(item.Name))
				{
					config.Variables.RemoveAt(i);
				}
			}
		}

		private static GlobalVariableConfig CloneForPersistence(GlobalVariableConfig source)
		{
			GlobalVariableConfig target = new GlobalVariableConfig();
			foreach (GlobalVariableItem item in source.Variables)
			{
				target.Variables.Add(new GlobalVariableItem
				{
					Name = item.Name,
					DataType = item.DataType,
					RememberValue = item.RememberValue,
					CurrentValue = item.RememberValue ? item.CurrentValue : string.Empty,
					Mark = item.Mark
				});
			}
			return target;
		}

		private static object ConvertValue(string value, GlobalVariableDataType type)
		{
			value = value ?? string.Empty;
			bool b;
			short s;
			int i;
			byte by;
			float f;
			double d;

			switch (type)
			{
				case GlobalVariableDataType.Bit:
					return bool.TryParse(value, out b) ? b : value == "1";
				case GlobalVariableDataType.Int16:
					return short.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out s) ? s : (short)0;
				case GlobalVariableDataType.Int32:
					return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out i) ? i : 0;
				case GlobalVariableDataType.Byte:
					return byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out by) ? by : (byte)0;
				case GlobalVariableDataType.Float:
					return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out f) ? f : 0F;
				case GlobalVariableDataType.Double:
					return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out d) ? d : 0.0;
				default:
					return value;
			}
		}

		private static string FormatValue(object value, GlobalVariableDataType type)
		{
			if (value == null)
			{
				return string.Empty;
			}

			if (type == GlobalVariableDataType.Bit)
			{
				bool b;
				if (value is bool)
				{
					return ((bool)value) ? "1" : "0";
				}
				if (bool.TryParse(Convert.ToString(value), out b))
				{
					return b ? "1" : "0";
				}
			}

			return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
		}
	}
}
