using System;

namespace SimpleTcp.VivinUTF8TCP
{
    /// <summary>
    /// UTF8TcpServerEvents
    /// </summary>
    public class UTF8TcpServerEvents
    {
        #region Public-Members

        /// <summary>
        /// Event to call when the connection is established.
        /// </summary>
        public event Action<UTF8TcpServer, ClientConnectedEventArgs> Connected;

        /// <summary>
        /// Event to call when the connection is destroyed.
        /// </summary>
        public event Action<UTF8TcpServer, ClientDisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Event to call when byte data has become available from the server.
        /// </summary>
        public event Action<UTF8TcpServer, UTF8ReceivedEventArgs> DataReceived;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public UTF8TcpServerEvents()
        {

        }

        #endregion

        #region Public-Methods

        internal void HandleConnected(UTF8TcpServer sender, ClientConnectedEventArgs args)
        {
            Connected?.Invoke(sender, args);
        }

        internal void HandleClientDisconnected(UTF8TcpServer sender, ClientDisconnectedEventArgs args)
        {
            Disconnected?.Invoke(sender, args);
        }

        internal void HandleDataReceived(UTF8TcpServer sender, UTF8ReceivedEventArgs args)
        {
            DataReceived?.Invoke(sender, args);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}