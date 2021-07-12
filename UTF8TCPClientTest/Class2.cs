using SimpleTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UTF8TCPClientTest
{
    class Class2
    {
        static void Main1(string[] args)
        {
            
        }

        private static void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"Events_DataReceived From:{ e.IpPort} {e.Data}");
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
