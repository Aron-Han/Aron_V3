using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Aron_V3
{
	[Serializable]
	public class DisplayLayoutConfig
	{
		public int DisplayCount { get; set; }
		public string LayoutMode { get; set; }
		public List<DisplaySlotConfig> Displays { get; set; }

		public DisplayLayoutConfig()
		{
			DisplayCount = 4;
			LayoutMode = "AutoGrid";
			Displays = new List<DisplaySlotConfig>();
		}
	}

	[Serializable]
	public class DisplaySlotConfig
	{
		public string SlotName { get; set; }
		public string Title { get; set; }
		public bool Enable { get; set; }

		public DisplaySlotConfig()
		{
			SlotName = "";
			Title = "";
			Enable = true;
		}
	}

	public static class DisplayLayoutStore
	{
		public static event EventHandler DisplayLayoutSaved;

		public static string ConfigFilePath
		{
			get
			{
				string folder = ProjectPathStore.SystemConfigRoot;
				Directory.CreateDirectory(folder);
				return Path.Combine(folder, "DisplayLayoutConfig.xml");
			}
		}

		public static DisplayLayoutConfig LoadOrCreateDefault()
		{
			try
			{
				if (File.Exists(ConfigFilePath))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(DisplayLayoutConfig));

					using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						DisplayLayoutConfig config = serializer.Deserialize(fs) as DisplayLayoutConfig;

						if (config != null)
						{
							Normalize(config);
							return config;
						}
					}
				}
			}
			catch
			{
			}

			DisplayLayoutConfig defaultConfig = CreateDefault();
			Save(defaultConfig);
			return defaultConfig;
		}

		public static DisplayLayoutConfig CreateDefault()
		{
			DisplayLayoutConfig config = new DisplayLayoutConfig();
			config.DisplayCount = 4;
			config.LayoutMode = "AutoGrid";

			config.Displays.Add(new DisplaySlotConfig { SlotName = "Display1", Title = "Display1", Enable = true });
			config.Displays.Add(new DisplaySlotConfig { SlotName = "Display2", Title = "Display2", Enable = true });
			config.Displays.Add(new DisplaySlotConfig { SlotName = "Display3", Title = "Display3", Enable = true });
			config.Displays.Add(new DisplaySlotConfig { SlotName = "Display4", Title = "Display4", Enable = true });

			return config;
		}

		public static void Save(DisplayLayoutConfig config)
		{
			if (config == null)
			{
				config = CreateDefault();
			}

			Normalize(config);

			string folder = Path.GetDirectoryName(ConfigFilePath);

			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(DisplayLayoutConfig));

			using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				serializer.Serialize(fs, config);
			}

			EventHandler handler = DisplayLayoutSaved;
			if (handler != null)
			{
				handler(null, EventArgs.Empty);
			}

			DiagnosticLogStore.Append(
				DiagnosticLogLevel.Info,
				"Config",
				"Display layout config saved.",
				new Dictionary<string, string> { { "path", ConfigFilePath } });
		}

		public static List<string> GetDisplaySlotNames()
		{
			DisplayLayoutConfig config = LoadOrCreateDefault();
			List<string> result = new List<string>();
			result.Add("Not Show");

			foreach (DisplaySlotConfig slot in config.Displays)
			{
				if (slot == null || !slot.Enable)
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(slot.SlotName))
				{
					continue;
				}

				result.Add(slot.SlotName);
			}

			return result;
		}

		private static void Normalize(DisplayLayoutConfig config)
		{
			if (config.Displays == null)
			{
				config.Displays = new List<DisplaySlotConfig>();
			}

			if (config.DisplayCount <= 0)
			{
				config.DisplayCount = 1;
			}

			if (config.DisplayCount > 16)
			{
				config.DisplayCount = 16;
			}

			if (string.IsNullOrWhiteSpace(config.LayoutMode))
			{
				config.LayoutMode = "AutoGrid";
			}

			while (config.Displays.Count < config.DisplayCount)
			{
				int index = config.Displays.Count + 1;
				config.Displays.Add(new DisplaySlotConfig
				{
					SlotName = "Display" + index,
					Title = "Display" + index,
					Enable = true
				});
			}

			for (int i = 0; i < config.Displays.Count; i++)
			{
				if (config.Displays[i] == null)
				{
					config.Displays[i] = new DisplaySlotConfig();
				}

				if (string.IsNullOrWhiteSpace(config.Displays[i].SlotName))
				{
					config.Displays[i].SlotName = "Display" + (i + 1);
				}

				if (string.IsNullOrWhiteSpace(config.Displays[i].Title))
				{
					config.Displays[i].Title = config.Displays[i].SlotName;
				}
			}
		}
	}
}
