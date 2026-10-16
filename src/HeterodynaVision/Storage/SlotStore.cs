using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HeterodynaVision.Models;

namespace HeterodynaVision.Storage;

public static class SlotStore
{
	private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "data");

	private static readonly string SlotsPath = Path.Combine(DataDir, "slots.json");

	public static List<SlotAssignment> Load()
	{
		try
		{
			Directory.CreateDirectory(DataDir);
			if (!File.Exists(SlotsPath))
			{
				return new List<SlotAssignment>();
			}
			return JsonSerializer.Deserialize<List<SlotAssignment>>(File.ReadAllText(SlotsPath)) ?? new List<SlotAssignment>();
		}
		catch
		{
			return new List<SlotAssignment>();
		}
	}

	public static void Save(IEnumerable<SlotAssignment> assignments)
	{
		Directory.CreateDirectory(DataDir);
		List<SlotAssignment> normalized = (from x in assignments
			where !string.IsNullOrWhiteSpace(x.CameraId)
			group x by x.SlotIndex into g
			select g.Last() into x
			orderby x.SlotIndex
			select x).ToList();
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		File.WriteAllText(SlotsPath, JsonSerializer.Serialize(normalized, options));
	}

	public static string GetSlotsPath()
	{
		Directory.CreateDirectory(DataDir);
		return SlotsPath;
	}
}
