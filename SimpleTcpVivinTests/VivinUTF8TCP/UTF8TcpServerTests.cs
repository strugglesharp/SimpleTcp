using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleTcp.VivinUTF8TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTcp.VivinUTF8TCP.Tests
{
    [TestClass()]
    public class UTF8TcpServerTests
    {
        static int PORT_START = 9000;
        const string MSG1 = "ABCDE";
        const string MSG2 = "行生生生生产";
        const string MSG3 = "1234567890";
        const string STX = "\x02";
        const string ETX = "\x03";
        static void CreateTCP(int port, out SimpleTcpServer tcpsvr, out UTF8TcpClient client)
        {
            tcpsvr = new SimpleTcpServer($"127.0.0.1:{port}");
            client = new UTF8TcpClient($"127.0.0.1:{port}");
            client.Keepalive.EnableTcpKeepAlives = true;
            client.Settings.MutuallyAuthenticate = false;
            client.Settings.AcceptInvalidCertificates = true;
            client.Settings.ConnectTimeoutMs = 5000;

            Console.WriteLine($"使用端口 {port}");
        }

        [TestMethod()]
        public void 基本通信测试()
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

            Assert.AreEqual(sendMsg ,recvMsg);   
        }

        [TestMethod()]
        public void 粘包测试()
        {
            CreateTCP(PORT_START++, out SimpleTcpServer tcpsvr, out UTF8TcpClient client);

            List<string> RecvArr = new List<string>();
 
            client.Events.DataReceived += (sender, e) =>
            {
               RecvArr.Add(  e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

 
            tcpsvr.Events.ClientConnected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                tcpsvr.Send(e.IpPort, STX + MSG1 + ETX+ STX+MSG2+ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);
            Task.Delay(500).Wait();

            Assert.IsTrue(RecvArr.Count() == 2);
            Assert.AreEqual(RecvArr[0], MSG1);
            Assert.AreEqual(RecvArr[1], MSG2);
        }

        [TestMethod()]
        public void 半包测试()
        {
            CreateTCP(PORT_START++, out SimpleTcpServer tcpsvr, out UTF8TcpClient client);


            List<string> RecvArr = new List<string>();

            client.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };
 
            tcpsvr.Events.ClientConnected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                tcpsvr.Send(e.IpPort, STX + MSG1 + ETX);
                Task.Delay(1000).Wait();
                tcpsvr.Send(e.IpPort, STX + MSG2 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();


            Assert.IsTrue(RecvArr.Count() == 2);
            Assert.AreEqual(RecvArr[0], MSG1);
            Assert.AreEqual(RecvArr[1], MSG2);
        }

        [TestMethod()]
        public void 连续STX测试()
        {
            CreateTCP(PORT_START++, out SimpleTcpServer tcpsvr, out UTF8TcpClient client);


            List<string> RecvArr = new List<string>();

            client.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            tcpsvr.Events.ClientConnected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                tcpsvr.Send(e.IpPort, STX + MSG1 );
                Task.Delay(1000).Wait();
                tcpsvr.Send(e.IpPort, STX + MSG2 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();


            Assert.IsTrue(RecvArr.Count() ==1);
            Assert.AreEqual(RecvArr[0], MSG2); 
        }
        [TestMethod()]
        public void 只有STX_数据_ETX分离测试()
        {
            CreateTCP(PORT_START++, out SimpleTcpServer tcpsvr, out UTF8TcpClient client);


            List<string> RecvArr = new List<string>();

            client.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            tcpsvr.Events.ClientConnected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                tcpsvr.Send(e.IpPort, STX + MSG1);
                Task.Delay(300).Wait();
                tcpsvr.Send(e.IpPort, MSG2);
                Task.Delay(300).Wait();
                tcpsvr.Send(e.IpPort, MSG3+ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();


            Assert.IsTrue(RecvArr.Count() == 1);
            Assert.AreEqual(RecvArr[0], MSG1+MSG2+MSG3); 
        }
        [TestMethod()]
        public void 半包加完整包测试()
        {
            CreateTCP(PORT_START++, out SimpleTcpServer tcpsvr, out UTF8TcpClient client);


            List<string> RecvArr = new List<string>();

            client.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            tcpsvr.Events.ClientConnected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                tcpsvr.Send(e.IpPort, STX + MSG1);
                Task.Delay(1000).Wait();
                tcpsvr.Send(e.IpPort, ETX+STX + MSG2 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();


            Assert.IsTrue(RecvArr.Count() == 2);
            Assert.AreEqual(RecvArr[0], MSG1);
            Assert.AreEqual(RecvArr[1], MSG2);
        }


        [TestMethod()]
        public void 纯ASCII半包加完整包测试()
        {
            CreateTCP(PORT_START++, out SimpleTcpServer tcpsvr, out UTF8TcpClient client);


            List<string> RecvArr = new List<string>();

            client.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            tcpsvr.Events.ClientConnected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                tcpsvr.Send(e.IpPort, STX + MSG1);
                Task.Delay(1000).Wait();
                tcpsvr.Send(e.IpPort,  ETX + STX + MSG3 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();


            Assert.IsTrue(RecvArr.Count() == 2);
            Assert.AreEqual(RecvArr[0], MSG1);
            Assert.AreEqual(RecvArr[1], MSG3);
        }
    }
}