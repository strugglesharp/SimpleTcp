using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTcp.VivinUTF8TCP
{
    /// <summary>
    /// 收发以STX开头,ETX结尾的UTF的字符串
    /// </summary>
    public class UTF8TcpServer : IDisposable
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



        SimpleTcp.SimpleTcpServer _svr;

        /// <summary>
        /// Instantiates the TCP server without SSL.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        /// </summary>
        /// <param name="ipPort">The IP:port of the server.</param> 
        public UTF8TcpServer(string ipPort)
        {
            _svr = new SimpleTcpServer(ipPort);
            InitBind();
        }
        /// <summary>
        /// Stop accepting new connections.
        /// </summary>
        public void Stop() => _svr.Stop();

        /// <summary>
        /// Instantiates the TCP server without SSL.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        /// </summary>
        /// <param name="listenerIp">The listener IP address or hostname.</param>
        /// <param name="port">The TCP port on which to listen.</param>
        public UTF8TcpServer(string listenerIp, int port)
        {
            _svr = new SimpleTcpServer(listenerIp, port);
            InitBind();
        }
        /// <summary>
        /// Start accepting connections.
        /// </summary>
        public void Start() => _svr.Start();

        /// <summary>
        /// Start accepting connections.
        /// </summary>
        /// <returns>Task.</returns>
        public void StartAsync() => _svr.StartAsync();
        /// <summary>
        /// SimpleTcp statistics.
        /// </summary>
        public SimpleTcpStatistics Statistics => _svr.Statistics;

        /// <summary>
        /// SimpleTcp keepalive settings.
        /// </summary>
        public SimpleTcpKeepaliveSettings Keepalive => _svr.Keepalive;
        /// <summary>
        /// SimpleTcp client settings.
        /// </summary>
        public SimpleTcpServerSettings Settings => _svr.Settings;
        /// <summary>
        /// Method to invoke to send a log message.
        /// </summary>
        public Action<string> Logger { get => _svr.Logger; set { _svr.Logger = value; } }


        //Dictionary<string, UTF8TcpClient> _dictClients = new Dictionary<string, UTF8TcpClient>();
        void InitBind()
        {
            _svr.Settings.StreamBufferSize = StreamBuffSize;
            _svr.Events.ClientConnected += Events_ClientConnected;
            _svr.Events.ClientDisconnected += Events_ClientDisconnected;
            _svr.Events.DataReceived += Events_DataReceived;
        }

        private void Events_DataReceived(SimpleTcpServer svr, DataReceivedEventArgs e)
        {
            if (svr != _svr)
            {
                throw new NotImplementedException();
            }
            CutToSentenceInBuff(e.IpPort, e.Data.ToList());
            Console.WriteLine("throw not impletment!");
        }

        private void Events_ClientDisconnected(SimpleTcpServer svr, ClientDisconnectedEventArgs e)
        {
            if (svr != _svr)
            {
                throw new NotImplementedException();
            }   
            Events.HandleClientDisconnected(this, e);
        }

        private void Events_ClientConnected(SimpleTcpServer svr, ClientConnectedEventArgs e)
        { 
            if(svr!= _svr)
            {
                throw new NotImplementedException();
            }

            _svr.GetClientMeta(e.IpPort).RecvBuff = new BuffManager(StreamBuffSize);
            Events.HandleConnected(this, e);
        }

        /// <summary>
        /// Dispose of the TCP client.
        /// </summary>
        public void Dispose()
        {
            _svr.Events.ClientConnected -= Events_ClientConnected;
            _svr.Events.ClientDisconnected -= Events_ClientDisconnected;
            _svr.Events.DataReceived -= Events_DataReceived;
            _svr.Dispose();
        }



        UTF8TcpServerEvents _Events = new UTF8TcpServerEvents();

        /// <summary>
        /// UTF8Tcp client events.
        /// </summary>
        public UTF8TcpServerEvents Events
        {
            get
            {
                return _Events;
            }
            set
            {
                if (value == null) _Events = new UTF8TcpServerEvents();
                else _Events = value;
            }
        }

        /// <summary>
        /// Send data to the specified client by IP:port.
        /// </summary>
        /// <param name="ipPort">The client IP:port string.</param>
        /// <param name="data">String containing data to send.</param>
        public void Send(string ipPort, string data)
        {
            _svr.Send(ipPort, STX + data + ETX);
        }

        /// <summary>
        /// Send data to the specified client by IP:port asynchronously.
        /// </summary>
        /// <param name="ipPort">The client IP:port string.</param>
        /// <param name="data">String containing data to send.</param>
        /// <param name="token">Cancellation token for canceling the request.</param>
        public async Task SendAsync(string ipPort, string data, CancellationToken token = default)
        {
            await _svr.SendAsync(ipPort, STX + data + ETX, token);
        }


        void CutToSentenceInBuff(string ipPort, List<byte> data)
        {
            var recvBuff = _svr.GetClientMeta(ipPort).RecvBuff;
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
                        _Events.HandleDataReceived(this, new UTF8ReceivedEventArgs(ipPort , recvBuff));
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

    }
}
