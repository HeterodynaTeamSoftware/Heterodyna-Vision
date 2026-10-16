using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HeterodynaVision.Services;

public static class RecordingIndexService
{
	public static string RecordingsRoot => Path.Combine(AppContext.BaseDirectory, "data", "recordings");

	public static List<RecordingEntry> GetRecordings()
	{
		List<RecordingEntry> list = new List<RecordingEntry>();
		try
		{
			if (!Directory.Exists(RecordingsRoot))
			{
				return list;
			}
			foreach (string file in Directory.EnumerateFiles(RecordingsRoot, "*.mp4", SearchOption.AllDirectories))
			{
				FileInfo info = new FileInfo(file);
				list.Add(new RecordingEntry
				{
					FilePath = file,
					CameraName = TryGetCameraName(file),
					CreatedAt = info.CreationTime,
					SizeBytes = info.Length
				});
			}
		}
		catch
		{
			return list;
		}
		return list.OrderByDescending((RecordingEntry x) => x.CreatedAt).ToList();
	}

	public static string FormatSize(long bytes)
	{
		if (bytes < 1024)
		{
			return bytes + " B";
		}
		if (bytes < 1048576)
		{
			return ((double)bytes / 1024.0).ToString("F1") + " KB";
		}
		if (bytes < 1073741824)
		{
			return ((double)bytes / 1024.0 / 1024.0).ToString("F1") + " MB";
		}
		return ((double)bytes / 1024.0 / 1024.0 / 1024.0).ToString("F2") + " GB";
	}

	private static string TryGetCameraName(string file)
	{
		try
		{
			string dayDir = Path.GetDirectoryName(file);
			string camDir = ((dayDir == null) ? null : Path.GetDirectoryName(dayDir));
			if (string.IsNullOrWhiteSpace(camDir))
			{
				return "";
			}
			return new DirectoryInfo(camDir).Name.Replace('_', ' ');
		}
		catch
		{
			return "";
		}
	}
}
