using System;
using System.IO;

namespace Aron_V3
{
	public static class LanguagePreferenceStore
	{
		private const string EnglishValue = "English";
		private const string ChineseValue = "Chinese";

		private static string ConfigFile
		{
			get { return Path.Combine(ProjectPathStore.SystemConfigRoot, "Language.config"); }
		}

		public static bool LoadIsEnglish()
		{
			try
			{
				string file = ConfigFile;
				if (!File.Exists(file))
				{
					return false;
				}

				string value = (File.ReadAllText(file) ?? string.Empty).Trim();
				if (value.Equals(EnglishValue, StringComparison.OrdinalIgnoreCase) ||
					value.Equals("EN", StringComparison.OrdinalIgnoreCase) ||
					value.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			catch
			{
			}

			return false;
		}

		public static void SaveIsEnglish(bool isEnglish)
		{
			try
			{
				File.WriteAllText(ConfigFile, isEnglish ? EnglishValue : ChineseValue);
			}
			catch
			{
			}
		}
	}
}
