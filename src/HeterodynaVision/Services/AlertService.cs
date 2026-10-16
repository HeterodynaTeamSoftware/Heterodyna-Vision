using System;
using System.Media;

namespace HeterodynaVision.Services;

public static class AlertService
{
	private static DateTime _lastBeep = DateTime.MinValue;

	public static bool SoundEnabled { get; set; }

	public static void MotionBeep()
	{
		if (!SoundEnabled || (DateTime.Now - _lastBeep).TotalSeconds < 5.0)
		{
			return;
		}
		_lastBeep = DateTime.Now;
		try
		{
			SystemSounds.Exclamation.Play();
		}
		catch
		{
		}
	}
}
