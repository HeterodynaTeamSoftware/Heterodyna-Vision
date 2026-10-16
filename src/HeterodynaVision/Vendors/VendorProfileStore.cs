using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace HeterodynaVision.Vendors;

public static class VendorProfileStore
{
	public static string ProfilesPath => Path.Combine(AppContext.BaseDirectory, "profiles", "vendors.json");

	public static List<VendorProfile> Load()
	{
		try
		{
			EnsureDefaultFile();
			return JsonSerializer.Deserialize<List<VendorProfile>>(File.ReadAllText(ProfilesPath)) ?? Defaults();
		}
		catch
		{
			return Defaults();
		}
	}

	public static VendorProfile FindBest(string vendor)
	{
		List<VendorProfile> profiles = Load();
		VendorProfile vendorProfile = profiles.FirstOrDefault((VendorProfile p) => string.Equals(p.Vendor, vendor, StringComparison.OrdinalIgnoreCase));
		if (vendorProfile == null)
		{
			vendorProfile = profiles.FirstOrDefault((VendorProfile p) => p.Vendor == "Generic");
			if (vendorProfile == null)
			{
				vendorProfile = new VendorProfile
				{
					Vendor = "Generic"
				};
				VendorProfile vendorProfile2 = vendorProfile;
				int num = 3;
				List<string> list = new List<string>(num);
				CollectionsMarshal.SetCount(list, num);
				Span<string> span = CollectionsMarshal.AsSpan(list);
				span[0] = "RTSP";
				span[1] = "ONVIF";
				span[2] = "HTTP/MJPEG";
				vendorProfile2.Protocols = list;
			}
		}
		return vendorProfile;
	}

	public static void EnsureDefaultFile()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(ProfilesPath));
		if (!File.Exists(ProfilesPath))
		{
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			File.WriteAllText(ProfilesPath, JsonSerializer.Serialize(Defaults(), options));
		}
	}

	public static List<VendorProfile> Defaults()
	{
		List<VendorProfile> list = new List<VendorProfile>();
		VendorProfile vendorProfile = new VendorProfile
		{
			Vendor = "Generic",
			Family = "Universal"
		};
		int num = 4;
		List<int> list2 = new List<int>(num);
		CollectionsMarshal.SetCount(list2, num);
		Span<int> span = CollectionsMarshal.AsSpan(list2);
		span[0] = 554;
		span[1] = 80;
		span[2] = 8080;
		span[3] = 8554;
		vendorProfile.Ports = list2;
		num = 3;
		List<string> list3 = new List<string>(num);
		CollectionsMarshal.SetCount(list3, num);
		Span<string> span2 = CollectionsMarshal.AsSpan(list3);
		span2[0] = "RTSP";
		span2[1] = "ONVIF";
		span2[2] = "HTTP/MJPEG";
		vendorProfile.Protocols = list3;
		num = 3;
		List<string> list4 = new List<string>(num);
		CollectionsMarshal.SetCount(list4, num);
		Span<string> span3 = CollectionsMarshal.AsSpan(list4);
		span3[0] = "rtsp://{auth}{ip}:{port}/";
		span3[1] = "rtsp://{auth}{ip}:{port}/live";
		span3[2] = "rtsp://{auth}{ip}:{port}/stream1";
		vendorProfile.RtspTemplates = list4;
		vendorProfile.Notes = "Uniwersalne próby.";
		list.Add(vendorProfile);
		VendorProfile vendorProfile2 = new VendorProfile
		{
			Vendor = "Hikvision",
			Family = "Hikvision/HiWatch/OEM"
		};
		num = 4;
		List<int> list5 = new List<int>(num);
		CollectionsMarshal.SetCount(list5, num);
		Span<int> span4 = CollectionsMarshal.AsSpan(list5);
		span4[0] = 554;
		span4[1] = 8000;
		span4[2] = 80;
		span4[3] = 443;
		vendorProfile2.Ports = list5;
		num = 2;
		List<string> list6 = new List<string>(num);
		CollectionsMarshal.SetCount(list6, num);
		Span<string> span5 = CollectionsMarshal.AsSpan(list6);
		span5[0] = "RTSP";
		span5[1] = "ONVIF";
		vendorProfile2.Protocols = list6;
		num = 4;
		List<string> list7 = new List<string>(num);
		CollectionsMarshal.SetCount(list7, num);
		Span<string> span6 = CollectionsMarshal.AsSpan(list7);
		span6[0] = "rtsp://{auth}{ip}:554/Streaming/Channels/101";
		span6[1] = "rtsp://{auth}{ip}:554/Streaming/Channels/102";
		span6[2] = "rtsp://{auth}{ip}:554/h264/ch1/main/av_stream";
		span6[3] = "rtsp://{auth}{ip}:554/h264/ch1/sub/av_stream";
		vendorProfile2.RtspTemplates = list7;
		vendorProfile2.Notes = "Hikvision/HiWatch/OEM.";
		list.Add(vendorProfile2);
		VendorProfile vendorProfile3 = new VendorProfile
		{
			Vendor = "Dahua",
			Family = "Dahua/BCS/Imou/OEM"
		};
		num = 4;
		List<int> list8 = new List<int>(num);
		CollectionsMarshal.SetCount(list8, num);
		Span<int> span7 = CollectionsMarshal.AsSpan(list8);
		span7[0] = 554;
		span7[1] = 37777;
		span7[2] = 80;
		span7[3] = 443;
		vendorProfile3.Ports = list8;
		num = 2;
		List<string> list9 = new List<string>(num);
		CollectionsMarshal.SetCount(list9, num);
		Span<string> span8 = CollectionsMarshal.AsSpan(list9);
		span8[0] = "RTSP";
		span8[1] = "ONVIF";
		vendorProfile3.Protocols = list9;
		num = 4;
		List<string> list10 = new List<string>(num);
		CollectionsMarshal.SetCount(list10, num);
		Span<string> span9 = CollectionsMarshal.AsSpan(list10);
		span9[0] = "rtsp://{auth}{ip}:554/cam/realmonitor?channel=1&subtype=0";
		span9[1] = "rtsp://{auth}{ip}:554/cam/realmonitor?channel=1&subtype=1";
		span9[2] = "rtsp://{auth}{ip}:554/live";
		span9[3] = "rtsp://{auth}{ip}:554/stream1";
		vendorProfile3.RtspTemplates = list10;
		vendorProfile3.Notes = "Dahua/BCS/Imou/OEM.";
		list.Add(vendorProfile3);
		VendorProfile vendorProfile4 = new VendorProfile
		{
			Vendor = "XM",
			Family = "Xiongmai/XM"
		};
		num = 3;
		List<int> list11 = new List<int>(num);
		CollectionsMarshal.SetCount(list11, num);
		Span<int> span10 = CollectionsMarshal.AsSpan(list11);
		span10[0] = 554;
		span10[1] = 34567;
		span10[2] = 80;
		vendorProfile4.Ports = list11;
		num = 2;
		List<string> list12 = new List<string>(num);
		CollectionsMarshal.SetCount(list12, num);
		Span<string> span11 = CollectionsMarshal.AsSpan(list12);
		span11[0] = "RTSP";
		span11[1] = "HTTP";
		vendorProfile4.Protocols = list12;
		num = 2;
		List<string> list13 = new List<string>(num);
		CollectionsMarshal.SetCount(list13, num);
		Span<string> span12 = CollectionsMarshal.AsSpan(list13);
		span12[0] = "rtsp://{auth}{ip}:554/user={user}&password={pass}&channel=1&stream=0.sdp";
		span12[1] = "rtsp://{auth}{ip}:554/11";
		vendorProfile4.RtspTemplates = list13;
		vendorProfile4.Notes = "Tanie DVR/NVR.";
		list.Add(vendorProfile4);
		VendorProfile vendorProfile5 = new VendorProfile
		{
			Vendor = "Reolink",
			Family = "Reolink"
		};
		num = 3;
		List<int> list14 = new List<int>(num);
		CollectionsMarshal.SetCount(list14, num);
		Span<int> span13 = CollectionsMarshal.AsSpan(list14);
		span13[0] = 554;
		span13[1] = 80;
		span13[2] = 443;
		vendorProfile5.Ports = list14;
		num = 2;
		List<string> list15 = new List<string>(num);
		CollectionsMarshal.SetCount(list15, num);
		Span<string> span14 = CollectionsMarshal.AsSpan(list15);
		span14[0] = "RTSP";
		span14[1] = "ONVIF";
		vendorProfile5.Protocols = list15;
		num = 4;
		List<string> list16 = new List<string>(num);
		CollectionsMarshal.SetCount(list16, num);
		Span<string> span15 = CollectionsMarshal.AsSpan(list16);
		span15[0] = "rtsp://{auth}{ip}:554/h264Preview_01_main";
		span15[1] = "rtsp://{auth}{ip}:554/h264Preview_01_sub";
		span15[2] = "rtsp://{auth}{ip}:554/h265Preview_01_main";
		span15[3] = "rtsp://{auth}{ip}:554/h265Preview_01_sub";
		vendorProfile5.RtspTemplates = list16;
		vendorProfile5.Notes = "Reolink.";
		list.Add(vendorProfile5);
		VendorProfile vendorProfile6 = new VendorProfile
		{
			Vendor = "TP-Link Tapo",
			Family = "Tapo/VIGI"
		};
		num = 3;
		List<int> list17 = new List<int>(num);
		CollectionsMarshal.SetCount(list17, num);
		Span<int> span16 = CollectionsMarshal.AsSpan(list17);
		span16[0] = 554;
		span16[1] = 2020;
		span16[2] = 80;
		vendorProfile6.Ports = list17;
		num = 2;
		List<string> list18 = new List<string>(num);
		CollectionsMarshal.SetCount(list18, num);
		Span<string> span17 = CollectionsMarshal.AsSpan(list18);
		span17[0] = "RTSP";
		span17[1] = "ONVIF";
		vendorProfile6.Protocols = list18;
		num = 2;
		List<string> list19 = new List<string>(num);
		CollectionsMarshal.SetCount(list19, num);
		Span<string> span18 = CollectionsMarshal.AsSpan(list19);
		span18[0] = "rtsp://{auth}{ip}:554/stream1";
		span18[1] = "rtsp://{auth}{ip}:554/stream2";
		vendorProfile6.RtspTemplates = list19;
		vendorProfile6.Notes = "Tapo/VIGI.";
		list.Add(vendorProfile6);
		VendorProfile vendorProfile7 = new VendorProfile
		{
			Vendor = "Axis",
			Family = "Axis"
		};
		num = 3;
		List<int> list20 = new List<int>(num);
		CollectionsMarshal.SetCount(list20, num);
		Span<int> span19 = CollectionsMarshal.AsSpan(list20);
		span19[0] = 554;
		span19[1] = 80;
		span19[2] = 443;
		vendorProfile7.Ports = list20;
		num = 3;
		List<string> list21 = new List<string>(num);
		CollectionsMarshal.SetCount(list21, num);
		Span<string> span20 = CollectionsMarshal.AsSpan(list21);
		span20[0] = "RTSP";
		span20[1] = "ONVIF";
		span20[2] = "HTTP/MJPEG";
		vendorProfile7.Protocols = list21;
		num = 1;
		List<string> list22 = new List<string>(num);
		CollectionsMarshal.SetCount(list22, num);
		CollectionsMarshal.AsSpan(list22)[0] = "rtsp://{auth}{ip}:554/axis-media/media.amp";
		vendorProfile7.RtspTemplates = list22;
		vendorProfile7.Notes = "Axis.";
		list.Add(vendorProfile7);
		VendorProfile vendorProfile8 = new VendorProfile
		{
			Vendor = "Foscam",
			Family = "Foscam"
		};
		num = 3;
		List<int> list23 = new List<int>(num);
		CollectionsMarshal.SetCount(list23, num);
		Span<int> span21 = CollectionsMarshal.AsSpan(list23);
		span21[0] = 554;
		span21[1] = 88;
		span21[2] = 80;
		vendorProfile8.Ports = list23;
		num = 3;
		List<string> list24 = new List<string>(num);
		CollectionsMarshal.SetCount(list24, num);
		Span<string> span22 = CollectionsMarshal.AsSpan(list24);
		span22[0] = "RTSP";
		span22[1] = "ONVIF";
		span22[2] = "HTTP/MJPEG";
		vendorProfile8.Protocols = list24;
		num = 2;
		List<string> list25 = new List<string>(num);
		CollectionsMarshal.SetCount(list25, num);
		Span<string> span23 = CollectionsMarshal.AsSpan(list25);
		span23[0] = "rtsp://{auth}{ip}:554/videoMain";
		span23[1] = "rtsp://{auth}{ip}:554/videoSub";
		vendorProfile8.RtspTemplates = list25;
		vendorProfile8.Notes = "Foscam.";
		list.Add(vendorProfile8);
		VendorProfile vendorProfile9 = new VendorProfile
		{
			Vendor = "Samsung Hanwha",
			Family = "Wisenet/Hanwha"
		};
		num = 2;
		List<int> list26 = new List<int>(num);
		CollectionsMarshal.SetCount(list26, num);
		Span<int> span24 = CollectionsMarshal.AsSpan(list26);
		span24[0] = 554;
		span24[1] = 80;
		vendorProfile9.Ports = list26;
		num = 2;
		List<string> list27 = new List<string>(num);
		CollectionsMarshal.SetCount(list27, num);
		Span<string> span25 = CollectionsMarshal.AsSpan(list27);
		span25[0] = "RTSP";
		span25[1] = "ONVIF";
		vendorProfile9.Protocols = list27;
		num = 2;
		List<string> list28 = new List<string>(num);
		CollectionsMarshal.SetCount(list28, num);
		Span<string> span26 = CollectionsMarshal.AsSpan(list28);
		span26[0] = "rtsp://{auth}{ip}:554/profile2/media.smp";
		span26[1] = "rtsp://{auth}{ip}:554/profile1/media.smp";
		vendorProfile9.RtspTemplates = list28;
		vendorProfile9.Notes = "Wisenet/Hanwha.";
		list.Add(vendorProfile9);
		return list;
	}
}
