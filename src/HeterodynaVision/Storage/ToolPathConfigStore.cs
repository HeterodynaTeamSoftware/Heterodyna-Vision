using System;
using System.IO;
using System.Text.Json;

namespace HeterodynaVision.Storage;

public static class ToolPathConfigStore
{
	private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");

	private static readonly string ConfigPath = Path.Combine(DataDir, "toolpaths.json");

	public static ToolPathConfig Load()
	{
		try
		{
			if (!File.Exists(ConfigPath))
			{
				return new ToolPathConfig();
			}
			return JsonSerializer.Deserialize<ToolPathConfig>(File.ReadAllText(ConfigPath)) ?? new ToolPathConfig();
		}
		catch
		{
			return new ToolPathConfig();
		}
	}

	public static void Save(ToolPathConfig config)
	{
		Directory.CreateDirectory(DataDir);
		File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, new JsonSerializerOptions
		{
			WriteIndented = true
		}));
	}

	public static string GetPath()
	{
		Directory.CreateDirectory(DataDir);
		return ConfigPath;
	}
}
