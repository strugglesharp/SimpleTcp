using SimpleTcp;
using SimpleTcp.VivinUTF8TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UTF8TCPClientTest
{
    class Class1
    {
        static void Main(string[] args)
        {
            Test01();
        }
        static public void Test01()
        {
            CreateTCP(PORT_START++, out SimpleTcpServer tcpsvr, out UTF8TcpClient client);


            string recvMsg = null;
            client.Events.DataReceived += (sender, e) =>
            {
                recvMsg = e.Sentence;
                Console.WriteLine($"Recv {recvMsg}");
            };

            var sendMsg = MSG1;
            tcpsvr.Events.ClientConnected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                tcpsvr.Send(e.IpPort, STX + sendMsg + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);




            Task.Delay(500).Wait();


            Console.WriteLine(sendMsg == recvMsg);
            Console.ReadKey();
        }

        static int PORT_START = 9000;
        const string MSG1 = "ABCDEFGGGGGGGGGGGGGG00";
        const string MSG2 = "行星架生产";
        const string MSG3 = "12323行生产444";
        const char STX = (char)0x02;
        const char ETX = (char)0x03;
        static void CreateTCP(int port, out SimpleTcpServer tcpsvr, out UTF8TcpClient client)
        {
            tcpsvr = new SimpleTcpServer($"0.0.0.0:{port}");
            client = new UTF8TcpClient($"127.0.0.1:{port}");
            client.Keepalive.EnableTcpKeepAlives = true;
            client.Settings.MutuallyAuthenticate = false;
            client.Settings.AcceptInvalidCertificates = true;
            client.Settings.ConnectTimeoutMs = 5000;
        }


    }
}
