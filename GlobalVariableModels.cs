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

			config.Variables = config.Variables
				.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
				.ThenBy(x => x.Name, StringComparer.Ordinal)
				.ToList();
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

	public static class GlobalVariableReferenceUpdater
	{
		public static void Apply(
			Dictionary<string, string> renameMap,
			HashSet<string> deletedNames)
		{
			if ((renameMap == null || renameMap.Count <= 0) &&
				(deletedNames == null || deletedNames.Count <= 0))
			{
				return;
			}

			if (renameMap == null)
			{
				renameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}

			if (deletedNames == null)
			{
				deletedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}

			UpdateCommunicationConfig(renameMap, deletedNames);
			UpdateDataDisplayConfig(renameMap, deletedNames);
			UpdateFlowConfig(renameMap, deletedNames);
			UpdateScriptConfigs(renameMap, deletedNames);
		}

		private static string Remap(string value, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			string key = value.Trim();
			string renamed;
			if (renameMap.TryGetValue(key, out renamed))
			{
				return renamed ?? string.Empty;
			}

			if (deletedNames.Contains(key))
			{
				return string.Empty;
			}

			return value;
		}

		private static bool UpdateValue(ref string value, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			string newValue = Remap(value, renameMap, deletedNames);
			if (string.Equals(value ?? string.Empty, newValue ?? string.Empty, StringComparison.Ordinal))
			{
				return false;
			}

			value = newValue;
			return true;
		}

		private static bool UpdateProperty(string value, Action<string> setValue, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			string newValue = Remap(value, renameMap, deletedNames);
			if (string.Equals(value ?? string.Empty, newValue ?? string.Empty, StringComparison.Ordinal))
			{
				return false;
			}

			setValue(newValue);
			return true;
		}

		private static void UpdateCommunicationConfig(Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			CommunicationConfig config = CommunicationConfigStore.LoadOrCreateDefault();
			bool changed = false;

			changed |= UpdateCommInputs(config.TcpIp == null ? null : config.TcpIp.InputVariables, renameMap, deletedNames);
			changed |= UpdateCommOutputs(config.TcpIp == null ? null : config.TcpIp.OutputVariables, renameMap, deletedNames);
			changed |= UpdateChannels(config.TcpIp == null ? null : config.TcpIp.Channels, renameMap, deletedNames);

			changed |= UpdateCommInputs(config.Profinet == null ? null : config.Profinet.InputVariables, renameMap, deletedNames);
			changed |= UpdateCommOutputs(config.Profinet == null ? null : config.Profinet.OutputVariables, renameMap, deletedNames);
			changed |= UpdateChannels(config.Profinet == null ? null : config.Profinet.Channels, renameMap, deletedNames);

			changed |= UpdateCommInputs(config.S7 == null ? null : config.S7.InputVariables, renameMap, deletedNames);
			changed |= UpdateCommOutputs(config.S7 == null ? null : config.S7.OutputVariables, renameMap, deletedNames);
			changed |= UpdateChannels(config.S7 == null ? null : config.S7.Channels, renameMap, deletedNames);

			if (changed)
			{
				CommunicationConfigStore.Save(config);
			}
		}

		private static bool UpdateCommInputs(List<CommInputVariable> list, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			bool changed = false;
			if (list == null) return false;
			foreach (CommInputVariable item in list)
			{
				if (item == null) continue;
				string value = item.GlobalVariableName;
				if (UpdateValue(ref value, renameMap, deletedNames))
				{
					item.GlobalVariableName = value;
					changed = true;
				}
			}
			return changed;
		}

		private static bool UpdateCommOutputs(List<CommOutputVariable> list, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			bool changed = false;
			if (list == null) return false;
			foreach (CommOutputVariable item in list)
			{
				if (item == null) continue;
				string value = item.GlobalVariableName;
				if (UpdateValue(ref value, renameMap, deletedNames))
				{
					item.GlobalVariableName = value;
					changed = true;
				}
			}
			return changed;
		}

		private static bool UpdateChannels(List<CommunicationChannelConfig> channels, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			bool changed = false;
			if (channels == null) return false;
			foreach (CommunicationChannelConfig channel in channels)
			{
				if (channel == null) continue;
				changed |= UpdateProperty(channel.TriggerGlobalVariableName, x => channel.TriggerGlobalVariableName = x, renameMap, deletedNames);
				changed |= UpdateProperty(channel.CustomTriggerGlobalVariableName, x => channel.CustomTriggerGlobalVariableName = x, renameMap, deletedNames);
				changed |= UpdateProperty(channel.PositionGlobalVariableName, x => channel.PositionGlobalVariableName = x, renameMap, deletedNames);
				changed |= UpdateProperty(channel.ProgramNoAddressName, x => channel.ProgramNoAddressName = x, renameMap, deletedNames);
				changed |= UpdateProperty(channel.ProgramSwitchEnableName, x => channel.ProgramSwitchEnableName = x, renameMap, deletedNames);
				changed |= UpdateProperty(channel.ProgramSwitchDoneName, x => channel.ProgramSwitchDoneName = x, renameMap, deletedNames);
				changed |= UpdateProperty(channel.ProgramSwitchFailName, x => channel.ProgramSwitchFailName = x, renameMap, deletedNames);
				changed |= UpdateProperty(channel.TriggerName, x => channel.TriggerName = x, renameMap, deletedNames);
				changed |= UpdateProperty(channel.PositionSourceName, x => channel.PositionSourceName = x, renameMap, deletedNames);
			}
			return changed;
		}

		private static void UpdateDataDisplayConfig(Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			DataDisplayConfig config = DataDisplayStore.LoadOrCreateDefault();
			bool changed = false;
			if (config.Items != null)
			{
				foreach (DataDisplayItem item in config.Items)
				{
					if (item == null) continue;
					changed |= UpdateProperty(item.GlobalVariableName, x => item.GlobalVariableName = x, renameMap, deletedNames);
				}
			}
			if (changed)
			{
				DataDisplayStore.Save(config);
			}
		}

		private static void UpdateFlowConfig(Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			ProjectFlowConfig config = FlowConfigStore.LoadOrCreateDefault();
			bool changed = false;
			if (config.Jobs != null)
			{
				foreach (JobConfig job in config.Jobs)
				{
					if (job == null || job.Tasks == null) continue;
					foreach (TaskConfig task in job.Tasks)
					{
						if (task == null) continue;
						changed |= UpdateProperty(task.TriggerName, x => task.TriggerName = x, renameMap, deletedNames);
						changed |= UpdateProperty(task.PositionName, x => task.PositionName = x, renameMap, deletedNames);
						changed |= UpdateProperty(task.PositionOptionName, x => task.PositionOptionName = x, renameMap, deletedNames);
						changed |= UpdateStepPins(task.Steps, renameMap, deletedNames);
					}
				}
			}
			if (changed)
			{
				FlowConfigStore.Save(config);
			}
		}

		private static bool UpdateStepPins(List<StepConfig> steps, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			bool changed = false;
			if (steps == null) return false;
			foreach (StepConfig step in steps)
			{
				if (step == null) continue;
				changed |= UpdatePins(step.InputPins, renameMap, deletedNames);
				changed |= UpdatePins(step.OutputPins, renameMap, deletedNames);
			}
			return changed;
		}

		private static bool UpdatePins(List<PinConfig> pins, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			bool changed = false;
			if (pins == null) return false;
			foreach (PinConfig pin in pins)
			{
				if (pin == null) continue;
				changed |= UpdateProperty(pin.GlobalVariableName, x => pin.GlobalVariableName = x, renameMap, deletedNames);
			}
			return changed;
		}

		private static void UpdateScriptConfigs(Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			string root = ProjectPathStore.ProjectRoot;
			if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
			{
				return;
			}

			foreach (string file in Directory.GetFiles(root, "*.script.xml", SearchOption.AllDirectories))
			{
				CSharpScriptStepConfig config = CSharpScriptStepStore.Load(file);
				bool changed = false;
				changed |= UpdateScriptPins(config.Inputs, renameMap, deletedNames);
				changed |= UpdateScriptPins(config.Outputs, renameMap, deletedNames);
				if (changed)
				{
					CSharpScriptStepStore.Save(file, config);
				}
			}
		}

		private static bool UpdateScriptPins(List<ScriptPinConfig> pins, Dictionary<string, string> renameMap, HashSet<string> deletedNames)
		{
			bool changed = false;
			if (pins == null) return false;
			foreach (ScriptPinConfig pin in pins)
			{
				if (pin == null) continue;
				changed |= UpdateProperty(pin.GlobalVariableName, x => pin.GlobalVariableName = x, renameMap, deletedNames);
			}
			return changed;
		}
	}
}
