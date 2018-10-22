using System.Net;
using System.Net.Sockets;

namespace Utils
{
    public static class NetworkUtil
    {
        public static string LocalIpAddress()
        {
            var localIp = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = ip.ToString();
                    break;
                }
            }
            return localIp;
        }
    }
}