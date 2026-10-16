using System;
using System.IO;

namespace HeterodynaVision.Storage;

public static class MotionEventStore
{
	private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");

	private static readonly string LogPath = Path.Combine(DataDir, "motion-events.log");

	public static void Append(string message)
	{
		Directory.CreateDirectory(DataDir);
		File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
	}

	public static string GetPath()
	{
		Directory.CreateDirectory(DataDir);
		return LogPath;
	}
}
