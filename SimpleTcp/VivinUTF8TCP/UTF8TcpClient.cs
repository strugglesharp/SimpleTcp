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
        /// <summary>
        /// Instantiates the TCP client without SSL.  Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        /// </summary>
        /// <param name="ipPort">The IP:port of the server.</param> 
        public UTF8TcpClient(string ipPort)
        {
            _client = new SimpleTcpClient(ipPort);
        }

        BuffManager _recvBuffer;

        /// <summary>
        /// Instantiates the TCP client without SSL.  Set the Connected, Disconnected, and DataReceived callbacks.  Once set, use Connect() to connect to the server.
        /// </summary>
        /// <param name="serverIpOrHostname">The server IP address or hostname.</param>
        /// <param name="port">The TCP port on which to connect.</param>
        public UTF8TcpClient(string serverIpOrHostname, int port)
        {
            _client = new SimpleTcpClient(serverIpOrHostname, port);
            _client.Settings.StreamBufferSize = StreamBuffSize;
            _client.Events.Connected += Events_Connected;
            _client.Events.Disconnected += Events_Disconnected;
            _client.Events.DataReceived += Events_DataReceived;
            _recvBuffer = new BuffManager(StreamBuffSize);
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


        private void Events_Connected(object sender, ClientConnectedEventArgs e)
        {
            _Events.HandleConnected(sender, e);
        }
        private void Events_Disconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _Events.HandleClientDisconnected(sender, e);
        }
        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data.ToList();
            var LEN = data.Count;
            int p = 0;

            while (p < LEN)
            {
                if (_recvBuffer.Counter == 0) //还未收到STX
                {
                    var idxSTX = data.IndexOf((byte)STX, p);

                    if (idxSTX < 0)
                    {//未找到STX, 全部忽略
                        return;
                    }
                    else
                    {//找到了STX
                        var idxETX = data.IndexOf((byte)ETX, idxSTX);
                        if (idxETX < 0)
                        {//未找到ETX,全部存储
                            _recvBuffer.Concat(data, idxSTX, LEN - idxSTX);
                            return;
                        }
                        else
                        {//找到了ETX
                            _recvBuffer.Concat(data, idxSTX, idxETX - idxSTX+1);                        
                            _Events.HandleDataReceived(sender, new UTF8ReceivedEventArgs(e.IpPort, _recvBuffer  ));
                            _recvBuffer.Clear();//处理完一笔就清空一笔.

                            p = idxETX + 1; //后续处理的起点
                            continue;
                        }
                    }
                }
                else //已经有STX,等待ETX
                {
                    var idxSTX = data.IndexOf((byte)STX, p);

                    if (idxSTX < 0)
                    {//未找到STX, 全部忽略
                        var idxETX = data.IndexOf((byte)ETX, p);
                        if (idxETX < 0)
                        {//未找到ETX,全部存储
                            _recvBuffer.Concat(data, p, LEN - p);
                            return;
                        }
                        else
                        {//找到了ETX
                            _recvBuffer.Concat(data, p, idxETX - p+1);
                            _Events.HandleDataReceived(sender, new UTF8ReceivedEventArgs(e.IpPort, _recvBuffer ));
                            _recvBuffer.Clear();//处理完一笔就清空一笔.

                            p = idxETX + 1; //后续处理的起点
                            continue;
                        }
                    }
                    else
                    {//找到了STX,发现重复的STX,清除buff所有信息,按照没有STX重新开始
                        _recvBuffer.Clear();
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
