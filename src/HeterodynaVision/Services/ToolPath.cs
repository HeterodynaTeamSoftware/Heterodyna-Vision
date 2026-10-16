using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HeterodynaVision.Storage;

namespace HeterodynaVision.Services;

public static class ToolPath
{
	public static string FindFfmpeg()
	{
		return FindTool("ffmpeg.exe", "ffmpeg");
	}

	public static string FindFfprobe()
	{
		return FindTool("ffprobe.exe", "ffprobe");
	}

	public static bool ToolExists(string pathOrCommand)
	{
		if (string.IsNullOrWhiteSpace(pathOrCommand))
		{
			return false;
		}
		if (File.Exists(pathOrCommand))
		{
			return true;
		}
		return FindInPath(pathOrCommand) != null;
	}

	public static bool SetCustomFfmpegDirectory(string directory, out string message)
	{
		if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
		{
			message = "Folder nie istnieje.";
			return false;
		}
		string path = Path.Combine(directory, "ffmpeg.exe");
		string ffprobe = Path.Combine(directory, "ffprobe.exe");
		if (!File.Exists(path) || !File.Exists(ffprobe))
		{
			message = "W wybranym folderze muszą być ffmpeg.exe i ffprobe.exe.";
			return false;
		}
		ToolPathConfigStore.Save(new ToolPathConfig
		{
			FfmpegDirectory = directory
		});
		message = "Zapisano folder FFmpeg: " + directory;
		return true;
	}

	public static string GetDiagnostics()
	{
		List<string> candidates = (from x in BuildCandidates("ffmpeg.exe")
			select x + " | exists=" + File.Exists(x)).ToList();
		ToolPathConfig cfg = ToolPathConfigStore.Load();
		return "BaseDirectory: " + AppContext.BaseDirectory + Environment.NewLine + "CurrentDirectory: " + Environment.CurrentDirectory + Environment.NewLine + "Custom FFmpeg directory: " + (string.IsNullOrWhiteSpace(cfg.FfmpegDirectory) ? "(brak)" : cfg.FfmpegDirectory) + Environment.NewLine + "Config: " + ToolPathConfigStore.GetPath() + Environment.NewLine + "ffmpeg: " + FindFfmpeg() + " | exists=" + ToolExists(FindFfmpeg()) + Environment.NewLine + "ffprobe: " + FindFfprobe() + " | exists=" + ToolExists(FindFfprobe()) + Environment.NewLine + Environment.NewLine + "Sprawdzane kandydaty ffmpeg.exe:" + Environment.NewLine + string.Join(Environment.NewLine, candidates);
	}

	private static string FindTool(string exeName, string commandName)
	{
		foreach (string candidate in BuildCandidates(exeName))
		{
			if (File.Exists(candidate))
			{
				return candidate;
			}
		}
		return FindInPath(exeName) ?? FindInPath(commandName) ?? commandName;
	}

	private static IEnumerable<string> BuildCandidates(string exeName)
	{
		List<string> roots = new List<string>();
		AddRoot(roots, ToolPathConfigStore.Load().FfmpegDirectory);
		AddRoot(roots, AppContext.BaseDirectory);
		AddRoot(roots, Environment.CurrentDirectory);
		string[] array = new string[2]
		{
			AppContext.BaseDirectory,
			Environment.CurrentDirectory
		};
		foreach (string start in array)
		{
			try
			{
				DirectoryInfo dir = new DirectoryInfo(start);
				for (int j = 0; j < 10; j++)
				{
					if (dir == null)
					{
						break;
					}
					AddRoot(roots, dir.FullName);
					dir = dir.Parent;
				}
			}
			catch
			{
			}
		}
		foreach (string root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
		{
			yield return Path.Combine(root, exeName);
			yield return Path.Combine(root, "bin", exeName);
			yield return Path.Combine(root, "tools", "ffmpeg", exeName);
			yield return Path.Combine(root, "tools", "ffmpeg", "x64", exeName);
			yield return Path.Combine(root, "tools", "ffmpeg", "x86", exeName);
		}
	}

	private static void AddRoot(List<string> roots, string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return;
		}
		try
		{
			string full = Path.GetFullPath(path);
			if (!roots.Contains(full, StringComparer.OrdinalIgnoreCase))
			{
				roots.Add(full);
			}
		}
		catch
		{
		}
	}

	private static string? FindInPath(string tool)
	{
		try
		{
			string fileName = (tool.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? tool : (tool + ".exe"));
			string path = Environment.GetEnvironmentVariable("PATH");
			if (string.IsNullOrWhiteSpace(path))
			{
				return null;
			}
			string[] array = path.Split(Path.PathSeparator);
			foreach (string dir in array)
			{
				if (!string.IsNullOrWhiteSpace(dir))
				{
					string full = Path.Combine(dir.Trim(), fileName);
					if (File.Exists(full))
					{
						return full;
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}
}
