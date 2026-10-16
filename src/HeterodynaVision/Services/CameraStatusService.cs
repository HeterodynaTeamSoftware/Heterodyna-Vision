using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public static class CameraStatusService
{
	public static async Task<CameraStatusResult> TestAsync(CameraItem camera, CancellationToken token = default(CancellationToken))
	{
		_ = 3;
		try
		{
			if (camera.SourceFormat == "USB/DirectShow" || camera.ProtocolMode == "USB/DirectShow")
			{
				DirectShowTestResult usb = await DirectShowTestService.CaptureTestFrameAsync(camera, token);
				return new CameraStatusResult
				{
					Success = usb.Success,
					StatusIcon = (usb.Success ? "\ud83d\udfe2" : "\ud83d\udd34"),
					StatusText = (usb.Success ? "USB OK" : "USB błąd"),
					Details = usb.Message + Environment.NewLine + usb.Output
				};
			}
			if (camera.SourceFormat == "HTTP Snapshot")
			{
				string frame = Path.Combine(AppContext.BaseDirectory, "cache", "status", camera.Id + ".jpg");
				Directory.CreateDirectory(Path.GetDirectoryName(frame));
				FrameCaptureResult capture = await FrameCaptureService.CaptureFrameAsync(camera, frame, token);
				return new CameraStatusResult
				{
					Success = capture.Success,
					StatusIcon = (capture.Success ? "\ud83d\udfe2" : "\ud83d\udd34"),
					StatusText = (capture.Success ? "Snapshot OK" : "Snapshot błąd"),
					Details = capture.Message + Environment.NewLine + capture.Output
				};
			}
			RtspProbeResult probe = await RtspProbeService.ProbeAsync(camera);
			if (probe.Success)
			{
				return new CameraStatusResult
				{
					Success = true,
					StatusIcon = "\ud83d\udfe2",
					StatusText = "Stream OK",
					Details = probe.Message + Environment.NewLine + probe.Output
				};
			}
			string framePath = Path.Combine(AppContext.BaseDirectory, "cache", "status", camera.Id + ".jpg");
			Directory.CreateDirectory(Path.GetDirectoryName(framePath));
			FrameCaptureResult fallback = await FrameCaptureService.CaptureFrameAsync(camera, framePath, token);
			CameraStatusResult cameraStatusResult = new CameraStatusResult();
			cameraStatusResult.Success = fallback.Success;
			cameraStatusResult.StatusIcon = (fallback.Success ? "\ud83d\udfe2" : "\ud83d\udd34");
			cameraStatusResult.StatusText = (fallback.Success ? "Klatka OK" : "Stream błąd");
			cameraStatusResult.Details = probe.Message + Environment.NewLine + probe.Output + Environment.NewLine + fallback.Message + Environment.NewLine + fallback.Output;
			return cameraStatusResult;
		}
		catch (Exception ex)
		{
			return new CameraStatusResult
			{
				Success = false,
				StatusIcon = "\ud83d\udd34",
				StatusText = "Błąd testu",
				Details = ex.Message
			};
		}
	}
}
