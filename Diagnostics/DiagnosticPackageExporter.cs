using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Aron_V3
{
	public static class DiagnosticPackageExporter
	{
		private const long MaxSingleFileBytes = 80L * 1024L * 1024L;

		public static string ExportPackage()
		{
			DiagnosticLogStore.Initialize();
			string snapshotPath = DiagnosticLogStore.WriteStateSnapshot("Manual diagnostic package export");

			string packageFolder = DiagnosticLogStore.PackageFolder;
			string zipPath = Path.Combine(
				packageFolder,
				"AronDiagnostics_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".zip");

			using (FileStream stream = new FileStream(zipPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
			using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
			{
				AddDirectory(archive, RuntimeLogStore.LogFolder, "Log");
				AddDirectory(archive, ProjectPathStore.ConfigRoot, "Config");
				AddDirectory(archive, ProjectPathStore.DatabaseRoot, "Database");
				AddFile(archive, snapshotPath, "snapshot.txt");
				AddRecentEvents(archive);
				AddSummary(archive);
			}

			Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			data["package"] = zipPath;
			DiagnosticLogStore.Append(DiagnosticLogLevel.Info, "Diagnostics", "Diagnostic package exported.", data);
			return zipPath;
		}

		private static void AddRecentEvents(ZipArchive archive)
		{
			if (archive == null)
			{
				return;
			}

			ZipArchiveEntry entry = archive.CreateEntry("recent_events.jsonl", CompressionLevel.Optimal);
			using (Stream stream = entry.Open())
			using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
			{
				foreach (string line in DiagnosticLogStore.GetRecentEventLines())
				{
					writer.WriteLine(line);
				}
			}
		}

		private static void AddSummary(ZipArchive archive)
		{
			if (archive == null)
			{
				return;
			}

			ZipArchiveEntry entry = archive.CreateEntry("summary.txt", CompressionLevel.Optimal);
			using (Stream stream = entry.Open())
			using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
			{
				writer.WriteLine("Aron_V3 Diagnostic Package");
				writer.WriteLine("ExportTime=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
				writer.WriteLine();
				writer.WriteLine("[Environment]");
				foreach (KeyValuePair<string, string> pair in DiagnosticLogStore.CollectEnvironmentData())
				{
					writer.WriteLine(pair.Key + "=" + pair.Value);
				}
				writer.WriteLine();
				writer.WriteLine("[ConfigFiles]");
				foreach (KeyValuePair<string, string> pair in DiagnosticLogStore.CollectConfigFingerprints())
				{
					writer.WriteLine(pair.Key + "=" + pair.Value);
				}
			}
		}

		private static void AddDirectory(ZipArchive archive, string folder, string entryRoot)
		{
			if (archive == null || string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
			{
				return;
			}

			foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
			{
				AddFile(archive, file, CombineEntry(entryRoot, GetRelativePath(folder, file)));
			}
		}

		private static void AddFile(ZipArchive archive, string filePath, string entryName)
		{
			if (archive == null ||
				string.IsNullOrWhiteSpace(filePath) ||
				string.IsNullOrWhiteSpace(entryName) ||
				!File.Exists(filePath))
			{
				return;
			}

			try
			{
				FileInfo info = new FileInfo(filePath);
				if (info.Length > MaxSingleFileBytes)
				{
					AddSkippedFileNote(archive, entryName, info.Length);
					return;
				}

				string normalizedEntry = NormalizeEntryName(entryName);
				if (archive.GetEntry(normalizedEntry) != null)
				{
					normalizedEntry = AddUniqueSuffix(normalizedEntry);
				}

				ZipArchiveEntry entry = archive.CreateEntry(normalizedEntry, CompressionLevel.Optimal);
				using (Stream input = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (Stream output = entry.Open())
				{
					input.CopyTo(output);
				}
			}
			catch
			{
			}
		}

		private static void AddSkippedFileNote(ZipArchive archive, string entryName, long length)
		{
			try
			{
				string noteName = NormalizeEntryName(entryName) + ".skipped.txt";
				ZipArchiveEntry entry = archive.CreateEntry(noteName, CompressionLevel.Optimal);
				using (Stream stream = entry.Open())
				using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
				{
					writer.WriteLine("File was skipped because it is larger than " + MaxSingleFileBytes.ToString(CultureInfo.InvariantCulture) + " bytes.");
					writer.WriteLine("OriginalSize=" + length.ToString(CultureInfo.InvariantCulture));
				}
			}
			catch
			{
			}
		}

		private static string GetRelativePath(string root, string file)
		{
			if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(file))
			{
				return Path.GetFileName(file ?? string.Empty);
			}

			string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			string fullFile = Path.GetFullPath(file);
			if (fullFile.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
			{
				return fullFile.Substring(fullRoot.Length);
			}

			return Path.GetFileName(file);
		}

		private static string CombineEntry(string left, string right)
		{
			if (string.IsNullOrWhiteSpace(left))
			{
				return right ?? string.Empty;
			}

			if (string.IsNullOrWhiteSpace(right))
			{
				return left;
			}

			return left.TrimEnd('/', '\\') + "/" + right.TrimStart('/', '\\');
		}

		private static string NormalizeEntryName(string entryName)
		{
			string value = (entryName ?? string.Empty).Replace('\\', '/').Trim('/');
			while (value.Contains("../"))
			{
				value = value.Replace("../", string.Empty);
			}

			if (string.IsNullOrWhiteSpace(value))
			{
				value = "file";
			}

			return value;
		}

		private static string AddUniqueSuffix(string entryName)
		{
			string folder = Path.GetDirectoryName(entryName);
			string name = Path.GetFileNameWithoutExtension(entryName);
			string ext = Path.GetExtension(entryName);
			string value = name + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ext;

			if (string.IsNullOrWhiteSpace(folder))
			{
				return value;
			}

			return folder.Replace('\\', '/') + "/" + value;
		}
	}
}
