using System;
using System.Diagnostics;
using System.IO;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public sealed class MotionRecordingService : IDisposable
{
	private Process? _process;

	private DateTime _lastMotion = DateTime.MinValue;

	private string _currentFile = "";

	private readonly TimeSpan _keepRecordingAfterMotion = TimeSpan.FromSeconds(15.0);

	public bool Enabled { get; set; }

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

	public string CurrentFile => _currentFile;

	public void OnMotion(CameraItem camera, int slotIndex, double score, Action<string>? log = null)
	{
		if (Enabled)
		{
			_lastMotion = DateTime.Now;
			if (!IsRecording)
			{
				Start(camera, slotIndex, score, log);
			}
		}
	}

	public void Tick(Action<string>? log = null)
	{
		if (IsRecording && DateTime.Now - _lastMotion > _keepRecordingAfterMotion)
		{
			Stop(log);
		}
	}

	private void Start(CameraItem camera, int slotIndex, double score, Action<string>? log)
	{
		string ffmpeg = ToolPath.FindFfmpeg();
		if (!ToolPath.ToolExists(ffmpeg))
		{
			log?.Invoke("REC ruch: brak ffmpeg.exe.");
			return;
		}
		string inputArgs = StreamFormatHelper.BuildFfmpegInput(camera);
		string safeName = MakeSafe(camera.Name);
		string day = DateTime.Now.ToString("yyyy-MM-dd");
		string dir = Path.Combine(AppContext.BaseDirectory, "data", "recordings", safeName, day);
		Directory.CreateDirectory(dir);
		_currentFile = Path.Combine(dir, DateTime.Now.ToString("HH-mm-ss") + $"_slot{slotIndex + 1}_motion.mp4");
		_process = new Process();
		_process.StartInfo.FileName = ffmpeg;
		_process.StartInfo.Arguments = "-hide_banner -loglevel error " + inputArgs + " -c copy -movflags +faststart -f mp4 \"" + _currentFile + "\" -y";
		_process.StartInfo.UseShellExecute = false;
		_process.StartInfo.CreateNoWindow = true;
		_process.StartInfo.RedirectStandardError = true;
		_process.StartInfo.RedirectStandardOutput = true;
		try
		{
			_process.Start();
			log?.Invoke($"REC ruch START: {_currentFile} | score={score:F1}");
		}
		catch (Exception ex)
		{
			log?.Invoke("REC ruch błąd startu FFmpeg: " + ex.Message);
			_process?.Dispose();
			_process = null;
			_currentFile = "";
		}
	}

	public void Stop(Action<string>? log = null)
	{
		try
		{
			if (_process != null && !_process.HasExited)
			{
				try
				{
					_process.Kill(entireProcessTree: true);
					_process.WaitForExit(2500);
				}
				catch
				{
				}
			}
			if (!string.IsNullOrWhiteSpace(_currentFile))
			{
				log?.Invoke("REC ruch STOP: " + _currentFile);
			}
		}
		finally
		{
			_process?.Dispose();
			_process = null;
			_currentFile = "";
		}
	}

	public void Dispose()
	{
		Stop();
	}

	private static string MakeSafe(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return "kamera";
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
		return "kamera";
	}
}
