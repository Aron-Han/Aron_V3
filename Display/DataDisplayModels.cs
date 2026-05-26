using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Aron_V3
{
	[Serializable]
	public class DataDisplayItem
	{
		[XmlAttribute]
		public string GroupName { get; set; }

		[XmlAttribute]
		public string ItemName { get; set; }

		[XmlAttribute]
		public string GlobalVariableName { get; set; }

		public DataDisplayItem()
		{
			GroupName = string.Empty;
			ItemName = string.Empty;
			GlobalVariableName = string.Empty;
		}
	}

	[XmlRoot("DataDisplayConfig")]
	public class DataDisplayConfig
	{
		[XmlArray("Items")]
		[XmlArrayItem("Item")]
		public List<DataDisplayItem> Items { get; set; }

		public DataDisplayConfig()
		{
			Items = new List<DataDisplayItem>();
		}
	}

	public static class DataDisplayStore
	{
		public static event EventHandler ConfigChanged;

		public static string ConfigFile
		{
			get { return Path.Combine(ProjectPathStore.SystemConfigRoot, "DataDisplayConfig.xml"); }
		}

		public static DataDisplayConfig LoadOrCreateDefault()
		{
			DataDisplayConfig config = File.Exists(ConfigFile)
				? XmlConfigHelper.Load<DataDisplayConfig>(ConfigFile)
				: new DataDisplayConfig();
			if (config == null)
			{
				config = new DataDisplayConfig();
			}
			Normalize(config);
			return config;
		}

		public static void Save(DataDisplayConfig config)
		{
			Normalize(config);
			XmlConfigHelper.Save(ConfigFile, config);
			EventHandler handler = ConfigChanged;
			if (handler != null) handler(null, EventArgs.Empty);
		}

		private static void Normalize(DataDisplayConfig config)
		{
			if (config == null)
			{
				return;
			}
			if (config.Items == null)
			{
				config.Items = new List<DataDisplayItem>();
			}
			foreach (DataDisplayItem item in config.Items)
			{
				if (item == null) continue;
				item.GroupName = item.GroupName ?? string.Empty;
				item.ItemName = item.ItemName ?? string.Empty;
				item.GlobalVariableName = item.GlobalVariableName ?? string.Empty;
			}
		}
	}
}
