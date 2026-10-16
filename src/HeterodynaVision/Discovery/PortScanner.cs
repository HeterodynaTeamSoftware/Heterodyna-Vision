using System.Net.Sockets;
using System.Threading.Tasks;

namespace HeterodynaVision.Discovery;

public static class PortScanner
{
	public static async Task<bool> IsPortOpenAsync(string ip, int port, int timeoutMs)
	{
		try
		{
			using TcpClient client = new TcpClient();
			Task connectTask = client.ConnectAsync(ip, port);
			Task timeoutTask = Task.Delay(timeoutMs);
			return await Task.WhenAny(connectTask, timeoutTask) == connectTask && client.Connected;
		}
		catch
		{
			return false;
		}
	}
}
