using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public static class DirectShowTestService
{
	public static async Task<DirectShowTestResult> CaptureTestFrameAsync(CameraItem camera, CancellationToken token = default(CancellationToken))
	{
		DirectShowTestResult result = new DirectShowTestResult();
		string ffmpeg = ToolPath.FindFfmpeg();
		if (!ToolPath.ToolExists(ffmpeg))
		{
			result.Message = "Nie znaleziono ffmpeg.exe. Kliknij Biblioteki i wskaż folder FFmpeg.";
			return result;
		}
		string dir = Path.Combine(AppContext.BaseDirectory, "cache", "usb-tests");
		Directory.CreateDirectory(dir);
		string safeName = MakeSafe(camera.Name);
		string outputPath = Path.Combine(dir, safeName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg");
		string inputArgs = StreamFormatHelper.BuildFfmpegInput(camera);
		string args = "-hide_banner -loglevel error " + inputArgs + " -frames:v 1 -q:v 3 \"" + outputPath + "\" -y";
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
			Task task = process.WaitForExitAsync(token);
			Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(8.0), token);
			if (await Task.WhenAny(task, timeoutTask) == timeoutTask)
			{
				try
				{
					process.Kill(entireProcessTree: true);
				}
				catch
				{
				}
				result.Message = "Test USB przekroczył limit 8 sekund.";
				return result;
			}
			string stderr = await process.StandardError.ReadToEndAsync();
			string stdout = await process.StandardOutput.ReadToEndAsync();
			result.Output = stderr + Environment.NewLine + stdout;
			if (File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
			{
				result.Success = true;
				result.FramePath = outputPath;
				result.Message = "Test USB OK. Zapisano klatkę: " + outputPath;
				return result;
			}
			result.Message = "Nie udało się pobrać klatki USB. Sprawdź rozdzielczość/FPS albo wybierz AUTO.";
			return result;
		}
		catch (Exception ex)
		{
			result.Message = "Błąd testu USB: " + ex.Message;
			return result;
		}
	}

	private static string MakeSafe(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return "usb_camera";
		}
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char c in invalidFileNameChars)
		{
			value = value.Replace(c, '_');
		}
		value = value.Replace(' ', '_').Trim('_');
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		return "usb_camera";
	}
}
