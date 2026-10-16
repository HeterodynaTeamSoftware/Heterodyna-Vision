using System;
using System.Linq;
using HeterodynaVision.Models;

namespace HeterodynaVision.Services;

public static class StreamFormatHelper
{
	public static readonly string[] SupportedFormats = new string[9] { "AUTO", "RTSP", "ONVIF", "HTTP Snapshot", "MJPEG", "RTMP", "HLS", "HTTP/MP4", "USB/DirectShow" };

	public static string DetectFromUrl(string url)
	{
		url = (url ?? "").Trim();
		if (string.IsNullOrWhiteSpace(url))
		{
			return "AUTO";
		}
		if (url.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase))
		{
			return "RTSP";
		}
		if (url.StartsWith("rtmp://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("rtmps://", StringComparison.OrdinalIgnoreCase))
		{
			return "RTMP";
		}
		if (url.Contains(".m3u8", StringComparison.OrdinalIgnoreCase))
		{
			return "HLS";
		}
		if (url.Contains("mjpg", StringComparison.OrdinalIgnoreCase) || url.Contains("mjpeg", StringComparison.OrdinalIgnoreCase) || url.Contains("video.cgi", StringComparison.OrdinalIgnoreCase) || url.Contains("videostream", StringComparison.OrdinalIgnoreCase))
		{
			return "MJPEG";
		}
		if (url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || url.Contains("snapshot", StringComparison.OrdinalIgnoreCase) || url.Contains("image", StringComparison.OrdinalIgnoreCase))
		{
			return "HTTP Snapshot";
		}
		if (url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
		{
			return "HTTP/MP4";
		}
		return "AUTO";
	}

	public static string BuildFfmpegInput(CameraItem camera)
	{
		string url = RtspProbeService.BuildUrl(camera);
		string dshowName = ((!string.IsNullOrWhiteSpace(camera.StreamUrl)) ? camera.StreamUrl : camera.Ip);
		string format = camera.SourceFormat;
		if (string.Equals(format, "AUTO", StringComparison.OrdinalIgnoreCase))
		{
			format = DetectFromUrl(url);
		}
		return format switch
		{
			"RTSP" => "-rtsp_transport tcp -i \"" + url + "\"", 
			"RTMP" => "-i \"" + url + "\"", 
			"HLS" => "-i \"" + url + "\"", 
			"MJPEG" => "-f mjpeg -i \"" + url + "\"", 
			"HTTP Snapshot" => "-loop 1 -i \"" + url + "\"", 
			"HTTP/MP4" => "-i \"" + url + "\"", 
			"USB/DirectShow" => BuildDirectShowInput(camera, dshowName), 
			_ => "-rtsp_transport tcp -i \"" + url + "\"", 
		};
	}

	private static string BuildDirectShowInput(CameraItem camera, string dshowName)
	{
		string args = "-f dshow ";
		if (!string.IsNullOrWhiteSpace(camera.UsbResolution))
		{
			args = args + "-video_size " + camera.UsbResolution + " ";
		}
		if (camera.UsbFps > 0)
		{
			args += $"-framerate {camera.UsbFps} ";
		}
		return args + "-i video=\"" + dshowName + "\"";
	}

	public static string Explain()
	{
		return string.Join(Environment.NewLine, SupportedFormats.Select((string x) => "- " + x));
	}
}
