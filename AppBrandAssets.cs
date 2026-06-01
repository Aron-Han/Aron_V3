using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Aron_V3
{
	internal static class AppBrandAssets
	{
		public const string BrandLogoFileName = "brand_logo_transparent.png";
		public const string AppIconFileName = "app.ico";

		public static string AssetsFolder
		{
			get
			{
				string path = Path.Combine(Application.StartupPath, "Assets");
				return path;
			}
		}

		public static Image LoadImage(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return null;
			}

			Image embeddedImage = LoadImageFromEmbeddedResource(fileName);
			if (embeddedImage != null)
			{
				return embeddedImage;
			}

			string path = FindAssetPath(fileName);
			if (path == null)
			{
				return null;
			}

			using (Image image = Image.FromFile(path))
			{
				return new Bitmap(image);
			}
		}

		public static Icon LoadAppIcon()
		{
			Icon embeddedIcon = LoadIconFromEmbeddedResource(AppIconFileName);
			if (embeddedIcon != null)
			{
				return embeddedIcon;
			}

			string path = FindAssetPath(AppIconFileName);
			if (path == null)
			{
				return null;
			}

			return new Icon(path);
		}

		private static Image LoadImageFromEmbeddedResource(string fileName)
		{
			using (Stream stream = OpenEmbeddedResource(fileName))
			{
				if (stream == null)
				{
					return null;
				}

				using (Image image = Image.FromStream(stream))
				{
					return new Bitmap(image);
				}
			}
		}

		private static Icon LoadIconFromEmbeddedResource(string fileName)
		{
			using (Stream stream = OpenEmbeddedResource(fileName))
			{
				if (stream == null)
				{
					return null;
				}

				using (Icon icon = new Icon(stream))
				{
					return (Icon)icon.Clone();
				}
			}
		}

		private static Stream OpenEmbeddedResource(string fileName)
		{
			Assembly assembly = typeof(AppBrandAssets).Assembly;
			string namespacePrefix = typeof(AppBrandAssets).Namespace ?? "Aron_V3";
			string expectedName = namespacePrefix + ".Assets." + fileName;
			Stream stream = assembly.GetManifestResourceStream(expectedName);
			if (stream != null)
			{
				return stream;
			}

			string expectedSuffix = ".Assets." + fileName;
			string[] resourceNames = assembly.GetManifestResourceNames();
			for (int i = 0; i < resourceNames.Length; i++)
			{
				string resourceName = resourceNames[i];
				if (resourceName.EndsWith(expectedSuffix, StringComparison.OrdinalIgnoreCase))
				{
					return assembly.GetManifestResourceStream(resourceName);
				}
			}

			return null;
		}

		private static string FindAssetPath(string fileName)
		{
			string[] roots = new[]
			{
				Application.StartupPath,
				AppDomain.CurrentDomain.BaseDirectory,
				Directory.GetCurrentDirectory()
			};

			for (int i = 0; i < roots.Length; i++)
			{
				string root = roots[i];
				if (string.IsNullOrWhiteSpace(root))
				{
					continue;
				}

				string assetsPath = Path.Combine(root, "Assets", fileName);
				if (File.Exists(assetsPath))
				{
					return assetsPath;
				}

				string directPath = Path.Combine(root, fileName);
				if (File.Exists(directPath))
				{
					return directPath;
				}
			}

			return null;
		}
	}
}
