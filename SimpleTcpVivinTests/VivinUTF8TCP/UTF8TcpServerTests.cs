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
        const string MSG1 = "ABCDE";
        const string MSG2 = "行生生生生产";
        const string MSG3 = "1234567890";
        const string STX = "\x02";
        const string ETX = "\x03";
        static void CreateTCPPair(  out UTF8TcpServer tcpsvr, out SimpleTcpClient client)
        {
         var   port = 9000;
            tcpsvr = new UTF8TcpServer($"127.0.0.1:{port}");
            client = new SimpleTcpClient($"127.0.0.1:{port}");
            client.Keepalive.EnableTcpKeepAlives = true;
            client.Settings.MutuallyAuthenticate = false;
            client.Settings.AcceptInvalidCertificates = true;
            client.Settings.ConnectTimeoutMs = 5000;

            Console.WriteLine($"使用端口 {port}");
        }
        static void CloseTCPPair(UTF8TcpServer tcpsvr,   SimpleTcpClient client)
        {
            tcpsvr.Stop();
            tcpsvr.Dispose();
            client.Disconnect();
            client.Dispose();
        }
     
        public void 手动测试测试()
        {
            CreateTCPPair(out UTF8TcpServer tcpsvr, out SimpleTcpClient client);

            string recvMsg = null;
            tcpsvr.Events.DataReceived += (sender, e) =>
            {
                recvMsg = e.Sentence;
                Console.WriteLine($"Recv {recvMsg}");
            };

            tcpsvr.Start(); 
            Task.Delay(500000).Wait();
        }
        [TestMethod()]
        public void 基本通信测试()
        {
            CreateTCPPair(out UTF8TcpServer tcpsvr, out SimpleTcpClient client);

            string recvMsg = null;
            tcpsvr.Events.DataReceived += (sender, e) =>
            {
                recvMsg = e.Sentence;
                Console.WriteLine($"Recv {recvMsg}");
            };

            var sendMsg = MSG1;
            client.Events.Connected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                client.Send(  STX + sendMsg + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);
            Task.Delay(500).Wait();
            CloseTCPPair(tcpsvr, client);

            Assert.AreEqual(sendMsg ,recvMsg);   
        }

        [TestMethod()]
        public void 粘包测试()
        {
            CreateTCPPair(out UTF8TcpServer tcpsvr, out SimpleTcpClient client);

            List<string> RecvArr = new List<string>();
 
            tcpsvr.Events.DataReceived += (sender, e) =>
            {
               RecvArr.Add(  e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };


            client.Events.Connected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                client.Send(STX + MSG1 + ETX+ STX+MSG2+ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);
            Task.Delay(500).Wait();
            CloseTCPPair(tcpsvr, client);
            Assert.IsTrue(RecvArr.Count() == 2);
            Assert.AreEqual(RecvArr[0], MSG1);
            Assert.AreEqual(RecvArr[1], MSG2);
        }

        [TestMethod()]
        public void 半包测试()
        {
            CreateTCPPair(out UTF8TcpServer tcpsvr, out SimpleTcpClient client);


            List<string> RecvArr = new List<string>();

            tcpsvr.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            client.Events.Connected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                client.Send(STX + MSG1 + ETX);
                Task.Delay(1000).Wait();
                client.Send(STX + MSG2 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();
            CloseTCPPair(tcpsvr, client);

            Assert.IsTrue(RecvArr.Count() == 2);
            Assert.AreEqual(RecvArr[0], MSG1);
            Assert.AreEqual(RecvArr[1], MSG2);
        }

        [TestMethod()]
        public void 连续STX测试()
        {
            CreateTCPPair(out UTF8TcpServer tcpsvr, out SimpleTcpClient client);


            List<string> RecvArr = new List<string>();

            tcpsvr.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            client.Events.Connected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                client.Send(STX + MSG1 );
                Task.Delay(1000).Wait();
                client.Send(STX + MSG2 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();
            CloseTCPPair(tcpsvr, client);

            Assert.IsTrue(RecvArr.Count() ==1);
            Assert.AreEqual(RecvArr[0], MSG2); 
        }
        [TestMethod()]
        public void 只有STX_数据_ETX分离测试()
        {
            CreateTCPPair(out UTF8TcpServer tcpsvr, out SimpleTcpClient client);


            List<string> RecvArr = new List<string>();

            tcpsvr.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            client.Events.Connected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                client.Send(  STX + MSG1);
                Task.Delay(300).Wait();
                client.Send(MSG2);
                Task.Delay(300).Wait();
                client.Send(MSG3 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();
            CloseTCPPair(tcpsvr, client);

            Assert.IsTrue(RecvArr.Count() == 1);
            Assert.AreEqual(RecvArr[0], MSG1+MSG2+MSG3); 
        }
        [TestMethod()]
        public void 半包加完整包测试()
        {
            CreateTCPPair(out UTF8TcpServer tcpsvr, out SimpleTcpClient client);


            List<string> RecvArr = new List<string>();

            tcpsvr.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            client.Events.Connected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                client.Send(STX + MSG1);
                Task.Delay(1000).Wait();
                client.Send(ETX + STX + MSG2 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();
            CloseTCPPair(tcpsvr, client);

            Assert.IsTrue(RecvArr.Count() == 2);
            Assert.AreEqual(RecvArr[0], MSG1);
            Assert.AreEqual(RecvArr[1], MSG2);
        }


        [TestMethod()]
        public void 纯ASCII半包加完整包测试()
        {
            CreateTCPPair(out UTF8TcpServer tcpsvr, out SimpleTcpClient client);


            List<string> RecvArr = new List<string>();

            tcpsvr.Events.DataReceived += (sender, e) =>
            {
                RecvArr.Add(e.Sentence);
                Console.WriteLine($"recv {e.Sentence}");
            };

            client.Events.Connected += (sender, e) =>
            {
                Console.WriteLine($"{e.IpPort} is connected!");
                client.Send(STX + MSG1);
                Task.Delay(1000).Wait();
                client.Send(ETX + STX + MSG3 + ETX);
            };

            tcpsvr.Start();
            client.ConnectWithRetries(5000);

            Task.Delay(1500).Wait();
            CloseTCPPair(tcpsvr, client);

            Assert.IsTrue(RecvArr.Count() == 2);
            Assert.AreEqual(RecvArr[0], MSG1);
            Assert.AreEqual(RecvArr[1], MSG3);
        }
    }
}