using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public static class RtspProbeService
{
	public static async Task<RtspProbeResult> ProbeAsync(CameraItem camera)
	{
		string url = BuildUrl(camera);
		string ffprobe = ToolPath.FindFfprobe();
		RtspProbeResult result = new RtspProbeResult
		{
			Command = ffprobe + " -v error -rtsp_transport tcp -i \"" + url + "\" -show_streams -select_streams v:0"
		};
		if (!ToolPath.ToolExists(ffprobe))
		{
			result.Success = false;
			result.Message = "Nie znaleziono ffprobe.exe. Kliknij Biblioteki i sprawdź ścieżki.";
			return result;
		}
		try
		{
			using Process process = new Process();
			process.StartInfo.FileName = ffprobe;
			process.StartInfo.Arguments = "-v error -rtsp_transport tcp -i \"" + url + "\" -show_streams -select_streams v:0";
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
				result.Message = "Nie udało się uruchomić ffprobe: " + ex.Message;
				result.Output = ToolPath.GetDiagnostics();
				return result;
			}
			Task task = process.WaitForExitAsync();
			Task timeoutTask = Task.Delay(8000);
			if (await Task.WhenAny(task, timeoutTask) == timeoutTask)
			{
				try
				{
					process.Kill(entireProcessTree: true);
				}
				catch
				{
				}
				result.Success = false;
				result.Message = "Timeout testu RTSP po 8 sekundach.";
				return result;
			}
			string stdout = await process.StandardOutput.ReadToEndAsync();
			string stderr = await process.StandardError.ReadToEndAsync();
			result.Output = stdout + Environment.NewLine + stderr;
			result.Success = process.ExitCode == 0 && stdout.Contains("[STREAM]");
			result.Message = (result.Success ? "RTSP działa - wykryto strumień wideo." : "Nie udało się potwierdzić strumienia RTSP. Sprawdź URL, login, hasło, port albo ścieżkę.");
		}
		catch (Exception ex2)
		{
			result.Success = false;
			result.Message = "Błąd testu RTSP: " + ex2.Message;
		}
		return result;
	}

	public static string BuildUrl(CameraItem camera)
	{
		if (!string.IsNullOrWhiteSpace(camera.StreamUrl))
		{
			return camera.StreamUrl.Trim();
		}
		string auth = "";
		if (!string.IsNullOrWhiteSpace(camera.Username))
		{
			auth = Uri.EscapeDataString(camera.Username) + ":" + Uri.EscapeDataString(camera.Password) + "@";
		}
		string ip = (string.IsNullOrWhiteSpace(camera.Ip) ? "127.0.0.1" : camera.Ip.Trim());
		int port = ((camera.Port <= 0) ? 554 : camera.Port);
		return $"rtsp://{auth}{ip}:{port}/Streaming/Channels/101";
	}
}
