using System;
using System.IO;
using System.Linq;

namespace HeterodynaVision.Services;

public static class RecordingCleanupService
{
	public static RecordingCleanupResult DeleteOlderThanDays(int days)
	{
		RecordingCleanupResult result = new RecordingCleanupResult();
		if (days < 1)
		{
			result.Message = "Liczba dni musi być większa od zera.";
			return result;
		}
		string root = RecordingIndexService.RecordingsRoot;
		DateTime cutoff = DateTime.Now.AddDays(-days);
		if (!Directory.Exists(root))
		{
			result.Message = "Folder nagrań jeszcze nie istnieje.";
			return result;
		}
		foreach (string file in Directory.EnumerateFiles(root, "*.mp4", SearchOption.AllDirectories))
		{
			try
			{
				FileInfo info = new FileInfo(file);
				if (!(info.CreationTime >= cutoff))
				{
					long size = info.Length;
					File.Delete(file);
					result.DeletedFiles++;
					result.FreedBytes += size;
				}
			}
			catch
			{
			}
		}
		TryRemoveEmptyDirs(root);
		result.Message = $"Usunięto plików: {result.DeletedFiles}, zwolniono: {RecordingIndexService.FormatSize(result.FreedBytes)}";
		return result;
	}

	private static void TryRemoveEmptyDirs(string root)
	{
		try
		{
			foreach (string dir in from x in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
				orderby x.Length descending
				select x)
			{
				if (!Directory.EnumerateFileSystemEntries(dir).Any())
				{
					Directory.Delete(dir);
				}
			}
		}
		catch
		{
		}
	}
}
