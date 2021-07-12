using SimpleTcp.VivinUTF8TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UTF8TCPClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            UTF8TcpClient _Client = new UTF8TcpClient("127.0.0.1", 8000);
            _Client.Events.Connected += Events_Connected; ;
            _Client.Events.Disconnected += Events_Disconnected; ;
            _Client.Events.DataReceived += Events_DataReceived; ;
            _Client.Keepalive.EnableTcpKeepAlives = true;
            _Client.Settings.MutuallyAuthenticate = false;
            _Client.Settings.AcceptInvalidCertificates = true;
            _Client.Settings.ConnectTimeoutMs = 5000;
            _Client.Logger = Logger;

            _Client.ConnectWithRetries(5000);

            _Client.Send("Hello, world!");
            Console.ReadKey();
        }

        private static void Events_DataReceived(object sender, UTF8ReceivedEventArgs e)
        {
            Console.WriteLine($"Events_DataReceived From:{ e.IpPort} {e.Sentence}");
        }

        private static void Events_Disconnected(object sender, SimpleTcp.ClientDisconnectedEventArgs e)
        {
            Console.WriteLine($"Disconnected() from {sender.ToString()}");
        }

        private static void Events_Connected(object sender, SimpleTcp.ClientConnectedEventArgs e)
        {
            Console.WriteLine($"Events_Connected() from {sender.ToString()}");
        }
        private static void Logger(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
