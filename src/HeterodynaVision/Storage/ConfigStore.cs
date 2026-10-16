using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HeterodynaVision.Models;

namespace HeterodynaVision.Storage;

public static class ConfigStore
{
	private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");

	private static readonly string ConfigPath = Path.Combine(DataDir, "cameras.json");

	public static List<CameraItem> Load()
	{
		try
		{
			if (!File.Exists(ConfigPath))
			{
				return new List<CameraItem>();
			}
			List<CameraItem> list = JsonSerializer.Deserialize<List<CameraItem>>(File.ReadAllText(ConfigPath)) ?? new List<CameraItem>();
			foreach (CameraItem camera in list)
			{
				if (string.IsNullOrWhiteSpace(camera.Id))
				{
					camera.Id = Guid.NewGuid().ToString("N");
				}
			}
			return list;
		}
		catch
		{
			return new List<CameraItem>();
		}
	}

	public static void Save(List<CameraItem> cameras)
	{
		Directory.CreateDirectory(DataDir);
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cameras, options));
	}

	public static void ExportTo(string path, List<CameraItem> cameras)
	{
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		File.WriteAllText(path, JsonSerializer.Serialize(cameras, options));
	}

	public static List<CameraItem> ImportFrom(string path)
	{
		List<CameraItem> list = JsonSerializer.Deserialize<List<CameraItem>>(File.ReadAllText(path)) ?? new List<CameraItem>();
		foreach (CameraItem camera in list)
		{
			if (string.IsNullOrWhiteSpace(camera.Id))
			{
				camera.Id = Guid.NewGuid().ToString("N");
			}
		}
		return list;
	}

	public static string GetConfigPath()
	{
		Directory.CreateDirectory(DataDir);
		return ConfigPath;
	}
}
