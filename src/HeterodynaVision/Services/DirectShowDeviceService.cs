using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeterodynaVision.Services;

public static class DirectShowDeviceService
{
	public static async Task<List<DirectShowDevice>> GetVideoDevicesAsync()
	{
		List<DirectShowDevice> result = new List<DirectShowDevice>();
		string text = await RunFfmpegAsync("-hide_banner -list_devices true -f dshow -i dummy");
		bool inVideoSection = false;
		string[] array = text.Split('\n');
		foreach (string obj in array)
		{
			string line = obj.Trim();
			if (line.Contains("DirectShow video devices", StringComparison.OrdinalIgnoreCase))
			{
				inVideoSection = true;
			}
			else if (line.Contains("DirectShow audio devices", StringComparison.OrdinalIgnoreCase))
			{
				inVideoSection = false;
			}
			else
			{
				if (!inVideoSection)
				{
					continue;
				}
				Match match = Regex.Match(line, "\"(?<name>.+?)\"");
				if (match.Success)
				{
					string name = match.Groups["name"].Value.Trim();
					if (!string.IsNullOrWhiteSpace(name) && !result.Any((DirectShowDevice x) => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
					{
						result.Add(new DirectShowDevice
						{
							Name = name,
							Kind = "video"
						});
					}
				}
			}
		}
		return result;
	}

	public static async Task<List<DirectShowVideoOption>> GetVideoOptionsAsync(string deviceName)
	{
		List<DirectShowVideoOption> list = new List<DirectShowVideoOption>
		{
			new DirectShowVideoOption
			{
				Resolution = "",
				Fps = 0,
				Raw = "AUTO"
			}
		};
		if (string.IsNullOrWhiteSpace(deviceName))
		{
			return list;
		}
		string safeName = deviceName.Replace("\"", "\\\"");
		string[] array = (await RunFfmpegAsync("-hide_banner -list_options true -f dshow -i video=\"" + safeName + "\"")).Split('\n');
		foreach (string obj in array)
		{
			string line = obj.Trim();
			if (line.Contains("min s=", StringComparison.OrdinalIgnoreCase) || line.Contains("max s=", StringComparison.OrdinalIgnoreCase) || line.Contains("pixel_format", StringComparison.OrdinalIgnoreCase) || line.Contains("vcodec", StringComparison.OrdinalIgnoreCase))
			{
				Match sizeMatch = Regex.Match(line, "s=(?<w>\\d+)x(?<h>\\d+)", RegexOptions.IgnoreCase);
				Match fpsMatch = Regex.Match(line, "fps=(?<fps>\\d+)", RegexOptions.IgnoreCase);
				string resolution = (sizeMatch.Success ? (sizeMatch.Groups["w"].Value + "x" + sizeMatch.Groups["h"].Value) : "");
				int fps = ((fpsMatch.Success && int.TryParse(fpsMatch.Groups["fps"].Value, out var parsed)) ? parsed : 0);
				if (!string.IsNullOrWhiteSpace(resolution) && !list.Any((DirectShowVideoOption x) => x.Resolution == resolution && x.Fps == fps))
				{
					list.Add(new DirectShowVideoOption
					{
						Resolution = resolution,
						Fps = fps,
						Raw = line
					});
				}
			}
		}
		return list;
	}

	private static async Task<string> RunFfmpegAsync(string args)
	{
		string ffmpeg = ToolPath.FindFfmpeg();
		if (!ToolPath.ToolExists(ffmpeg))
		{
			return "";
		}
		using Process process = new Process();
		process.StartInfo.FileName = ffmpeg;
		process.StartInfo.Arguments = args;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.RedirectStandardOutput = true;
		try
		{
			process.Start();
			string stderr = await process.StandardError.ReadToEndAsync();
			string stdout = await process.StandardOutput.ReadToEndAsync();
			await process.WaitForExitAsync();
			return stderr + Environment.NewLine + stdout;
		}
		catch
		{
			return "";
		}
	}
}
