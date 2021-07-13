using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTcp.VivinUTF8TCP
{
    /// <summary>
    /// 收发以STX开头,ETX结尾的UTF的字符串
    /// </summary>
    public class UTF8TcpClient : IDisposable
    {
        /// <summary>
        /// 起始字符
        /// </summary>
        public const char STX = (char)0x02;
        /// <summary>
        /// 结束字符
        /// </summary>
        public const char ETX = (char)0x03;
        /// <summary>
        ///设定缓存大小,仅在初始化前有效!
        /// </summary>
        public static int StreamBuffSize { get; set; } = 65535;
        SimpleTcpClient _client;
        BuffManager _recvBuff;
        /// <summary>
        /// Instantiates the TCP client without SSL.  Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        /// </summary>
        /// <param name="ipPort">The IP:port of the server.</param> 
        public UTF8TcpClient(string ipPort)
        {
            _client = new SimpleTcpClient(ipPort);
            InitBind();
        }

        internal UTF8TcpClient(SimpleTcpClient client)
        {
            _client = client;
            InitBind();
        }


        /// <summary>
        /// Instantiates the TCP client without SSL.  Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        /// </summary>
        /// <param name="serverIpOrHostname">The server IP address or hostname.</param>
        /// <param name="port">The TCP port on which to connect.</param>
        public UTF8TcpClient(string serverIpOrHostname, int port)
        {
            _client = new SimpleTcpClient(serverIpOrHostname, port);
            InitBind();
        }

        void InitBind()
        {
            _client.Settings.StreamBufferSize = StreamBuffSize;
            _client.Events.Connected += Events_Connected;
            _client.Events.Disconnected += Events_Disconnected;
            _client.Events.DataReceived += Events_DataReceived;
            _recvBuff = new BuffManager(StreamBuffSize);
        }

        /// <summary>
        /// SimpleTcp keepalive settings.
        /// </summary>
        public SimpleTcpKeepaliveSettings Keepalive => _client.Keepalive;
        /// <summary>
        /// SimpleTcp client settings.
        /// </summary>
        public SimpleTcpClientSettings Settings => _client.Settings;
        /// <summary>
        /// Method to invoke to send a log message.
        /// </summary>
        public Action<string> Logger { get => _client.Logger; set { _client.Logger = value; } }


        private void Events_Connected(SimpleTcpClient client, ClientConnectedEventArgs e)
        {
            if (client != _client)
            {
                throw new NotImplementedException();
            }
            _Events.HandleConnected(this, e);
        }
        private void Events_Disconnected(SimpleTcpClient client, ClientDisconnectedEventArgs e)
        {
            if (client != _client)
            {
                throw new NotImplementedException();
            }
            _Events.HandleClientDisconnected(this, e);
        }
        private void Events_DataReceived(SimpleTcpClient client, DataReceivedEventArgs e)
        {
            if(client!= _client)
            {
                throw new NotImplementedException();
            }
            CutToSentenceInBuff(e.IpPort, e.Data.ToList(), _recvBuff);
        }
        // void CutToSentenceInBuff(SimpleTcpClient client, List<byte> data)
        void CutToSentenceInBuff(string ipPort, List<byte> data, BuffManager recvBuff)
        { 
            var LEN = data.Count;
            int p = 0;
            int idxSTX = 0, idxETX = 0;
            while (p < LEN)
            {
                if (recvBuff.Counter == 0) //还未收到STX
                {
                    idxSTX = data.IndexOf((byte)STX, p);

                    if (idxSTX < 0)
                    {//未找到STX, 全部忽略
                        return;
                    }
                    else
                    {//找到了STX
                        idxETX = data.IndexOf((byte)ETX, idxSTX);
                        if (idxETX < 0)
                        {//未找到ETX,全部存储
                            recvBuff.Concat(data, idxSTX, LEN - idxSTX);
                            return;
                        }
                        else
                        {//找到了ETX
                            recvBuff.Concat(data, idxSTX, idxETX - idxSTX + 1);
                            _Events.HandleDataReceived(this, new UTF8ReceivedEventArgs(ipPort, recvBuff));
                            recvBuff.Clear();//处理完一笔就清空一笔.

                            p = idxETX + 1; //后续处理的起点
                            continue;
                        }
                    }
                }
                else //前一帧已经有STX,等待ETX
                {
                    idxSTX = data.IndexOf((byte)STX, p);
                    idxETX = data.IndexOf((byte)ETX, p);
                    if (0 <= idxETX && idxETX < idxSTX)
                    {//第一帧STX+data1, 第二帧 data2+ETX+STX+data3+ETX 
                        recvBuff.Concat(data, p, idxETX + 1);
                        _Events.HandleDataReceived(this, new UTF8ReceivedEventArgs(ipPort, recvBuff));
                        recvBuff.Clear();//处理完一笔就清空一笔.

                        p = idxETX + 1; //后续处理的起点
                        continue;
                    }
                    if (idxSTX < 0)
                    {//未找到STX, 全部忽略
                        idxETX = data.IndexOf((byte)ETX, p);
                        if (idxETX < 0)
                        {//未找到ETX,全部存储
                            recvBuff.Concat(data, p, LEN - p);
                            return;
                        }
                        else
                        {//找到了ETX
                            recvBuff.Concat(data, p, idxETX - p + 1);
                            _Events.HandleDataReceived(this, new UTF8ReceivedEventArgs(ipPort, recvBuff));
                            recvBuff.Clear();//处理完一笔就清空一笔.

                            p = idxETX + 1; //后续处理的起点
                            continue;
                        }
                    }
                    else
                    {//找到了STX,发现重复的STX,清除buff所有信息,按照没有STX重新开始
                        recvBuff.Clear();
                        p = idxSTX;
                        continue;
                    }
                }
            }
        }

        UTF8TcpClientEvents _Events = new UTF8TcpClientEvents();


        /// <summary>
        /// UTF8Tcp client events.
        /// </summary>
        public UTF8TcpClientEvents Events
        {
            get
            {
                return _Events;
            }
            set
            {
                if (value == null) _Events = new UTF8TcpClientEvents();
                else _Events = value;
            }
        }

        /// <summary>
        /// Indicates whether or not the client is connected to the server.
        /// </summary>
        public bool IsConnected => _client.IsConnected;

        /// <summary>
        /// Send data to the server.
        /// </summary>
        /// <param name="UTF8Text">String containing data to send.</param>
        public void Send(string UTF8Text)
        {
            _client.Send(STX + UTF8Text + ETX);
        }
        /// <summary>
        /// Establish the connection to the server with retries up to either the timeout specified or the value in Settings.ConnectTimeoutMs.
        /// </summary>
        /// <param name="timeoutMs">The amount of time in milliseconds to continue attempting connections.</param>
        public void ConnectWithRetries(int? timeoutMs = null) => _client.ConnectWithRetries(timeoutMs);

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect()
        {
            _client.Events.Connected -= Events_Connected;
            _client.Events.Disconnected -= Events_Disconnected;
            _client.Events.DataReceived -= Events_DataReceived;
            _client.Disconnect();
        }
        /// <summary>
        /// Dispose of the TCP client.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            _client.Dispose();
        }
    }
}
