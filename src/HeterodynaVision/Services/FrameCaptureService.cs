using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public static class FrameCaptureService
{
	public static async Task<FrameCaptureResult> CaptureFrameAsync(CameraItem camera, string targetPath, CancellationToken token)
	{
		string ffmpeg = ToolPath.FindFfmpeg();
		string url = RtspProbeService.BuildUrl(camera);
		FrameCaptureResult result = new FrameCaptureResult
		{
			FramePath = targetPath
		};
		if (!ToolPath.ToolExists(ffmpeg))
		{
			result.Success = false;
			result.Message = "Nie znaleziono ffmpeg.exe. Kliknij Biblioteki i sprawdź ścieżki.";
			return result;
		}
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
			using Process process = new Process();
			process.StartInfo.FileName = ffmpeg;
			process.StartInfo.Arguments = $"-hide_banner -loglevel error -rtsp_transport tcp -y -i \"{url}\" -frames:v 1 -q:v 4 \"{targetPath}\"";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			try
			{
				process.Start();
			}
			catch (Exception ex)
			{
				result.Success = false;
				result.Message = "Nie udało się uruchomić FFmpeg: " + ex.Message;
				result.Output = ToolPath.GetDiagnostics();
				return result;
			}
			using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
			timeoutCts.CancelAfter(TimeSpan.FromSeconds(8.0));
			try
			{
				await process.WaitForExitAsync(timeoutCts.Token);
			}
			catch (OperationCanceledException)
			{
				try
				{
					process.Kill(entireProcessTree: true);
				}
				catch
				{
				}
				result.Success = false;
				result.Message = "Timeout pobierania klatki.";
				return result;
			}
			string stdout = await process.StandardOutput.ReadToEndAsync();
			string stderr = await process.StandardError.ReadToEndAsync();
			result.Output = stdout + Environment.NewLine + stderr;
			result.Success = process.ExitCode == 0 && File.Exists(targetPath) && new FileInfo(targetPath).Length > 0;
			result.Message = (result.Success ? "Klatka pobrana." : "Nie udało się pobrać klatki.");
		}
		catch (Exception ex3)
		{
			result.Success = false;
			result.Message = "Błąd FFmpeg: " + ex3.Message;
		}
		return result;
	}
}
