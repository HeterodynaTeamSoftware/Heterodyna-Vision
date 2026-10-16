using System.Diagnostics;
using System.IO;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public class RecordingService
{
	private Process? _process;

	public bool IsRecording
	{
		get
		{
			if (_process != null)
			{
				return !_process.HasExited;
			}
			return false;
		}
	}

	public void Start(CameraItem camera, string outputFile)
	{
		Stop();
		string ffmpeg = ToolPath.FindFfmpeg();
		string inputArgs = StreamFormatHelper.BuildFfmpegInput(camera);
		if (!ToolPath.ToolExists(ffmpeg))
		{
			throw new FileNotFoundException("Nie znaleziono ffmpeg.exe. Kliknij Biblioteki i sprawdź ścieżki.", ffmpeg);
		}
		Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
		_process = new Process();
		_process.StartInfo.FileName = ffmpeg;
		_process.StartInfo.Arguments = inputArgs + " -c copy -f mp4 \"" + outputFile + "\" -y";
		_process.StartInfo.UseShellExecute = false;
		_process.StartInfo.CreateNoWindow = true;
		_process.Start();
	}

	public void Stop()
	{
		try
		{
			if (_process != null && !_process.HasExited)
			{
				_process.Kill(entireProcessTree: true);
			}
		}
		catch
		{
		}
		finally
		{
			_process = null;
		}
	}
}
