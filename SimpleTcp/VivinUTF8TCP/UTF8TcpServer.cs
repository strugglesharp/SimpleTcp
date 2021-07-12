using System;
using System.Collections.Generic;
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



        SimpleTcp.SimpleTcpServer _tcp;

        /// <summary>
        /// Instantiates the TCP server without SSL.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        /// </summary>
        /// <param name="ipPort">The IP:port of the server.</param> 
        public UTF8TcpServer(string ipPort)
        {
            _tcp = new SimpleTcpServer(ipPort);
            InitBind();
        }

        /// <summary>
        /// Instantiates the TCP server without SSL.  Set the ClientConnected, ClientDisconnected, and DataReceived callbacks.  Once set, use Start() to begin listening for connections.
        /// </summary>
        /// <param name="listenerIp">The listener IP address or hostname.</param>
        /// <param name="port">The TCP port on which to listen.</param>
        public UTF8TcpServer(string listenerIp, int port)
        {
            _tcp = new SimpleTcpServer(listenerIp, port);
            InitBind();
        }
        /// <summary>
        /// Start accepting connections.
        /// </summary>
        public void Start() => _tcp.Start();

        /// <summary>
        /// Start accepting connections.
        /// </summary>
        /// <returns>Task.</returns>
        public void StartAsync() => _tcp.StartAsync();
        /// <summary>
        /// SimpleTcp statistics.
        /// </summary>
        public SimpleTcpStatistics Statistics => _tcp.Statistics;

        /// <summary>
        /// SimpleTcp keepalive settings.
        /// </summary>
        public SimpleTcpKeepaliveSettings Keepalive => _tcp.Keepalive;
        /// <summary>
        /// SimpleTcp client settings.
        /// </summary>
        public SimpleTcpServerSettings Settings => _tcp.Settings;
        /// <summary>
        /// Method to invoke to send a log message.
        /// </summary>
        public Action<string> Logger { get => _tcp.Logger; set { _tcp.Logger = value; } }

        void InitBind()
        {
            _tcp.Settings.StreamBufferSize = StreamBuffSize;
            _tcp.Events.ClientConnected += Events_ClientConnected;
            _tcp.Events.ClientDisconnected += Events_ClientDisconnected;
            _tcp.Events.DataReceived += Events_DataReceived;
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            //  Events.HandleDataReceived(sender, e);
            Console.WriteLine("throw not impletment!");
            1
        }

        private void Events_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Events.HandleClientDisconnected(sender, e);
        }

        private void Events_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Events.HandleConnected(sender, e);
        }

        /// <summary>
        /// Dispose of the TCP client.
        /// </summary>
        public void Dispose()
        {
            _tcp.Events.ClientConnected -= Events_ClientConnected;
            _tcp.Events.ClientDisconnected -= Events_ClientDisconnected;
            _tcp.Events.DataReceived -= Events_DataReceived;
            _tcp.Dispose();
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
            _tcp.Send(ipPort, STX + data + ETX);
        }

        /// <summary>
        /// Send data to the specified client by IP:port asynchronously.
        /// </summary>
        /// <param name="ipPort">The client IP:port string.</param>
        /// <param name="data">String containing data to send.</param>
        /// <param name="token">Cancellation token for canceling the request.</param>
        public async Task SendAsync(string ipPort, string data, CancellationToken token = default)
        {
            await _tcp.SendAsync(ipPort, STX + data + ETX, token);
        }
    }
}
