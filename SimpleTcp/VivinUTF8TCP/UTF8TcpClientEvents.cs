using System;

namespace SimpleTcp.VivinUTF8TCP
{
    /// <summary>
    /// UTF8TcpClientEvents
    /// </summary>
    public class UTF8TcpClientEvents

    {
        #region Public-Members

        /// <summary>
        /// Event to call when the connection is established.
        /// </summary>
        public event EventHandler<ClientConnectedEventArgs> Connected;

        /// <summary>
        /// Event to call when the connection is destroyed.
        /// </summary>
        public event EventHandler<ClientDisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Event to call when byte data has become available from the server.
        /// </summary>
        public event EventHandler<UTF8ReceivedEventArgs> DataReceived;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public UTF8TcpClientEvents()
        {

        }

        #endregion

        #region Public-Methods

        internal void HandleConnected(object sender, ClientConnectedEventArgs args)
        {
            Connected?.Invoke(sender, args);
        }

        internal void HandleClientDisconnected(object sender, ClientDisconnectedEventArgs args)
        {
            Disconnected?.Invoke(sender, args);
        }

        internal void HandleDataReceived(object sender, UTF8ReceivedEventArgs args)
        {
            DataReceived?.Invoke(sender, args);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}