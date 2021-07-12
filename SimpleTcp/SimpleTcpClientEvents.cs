using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTcp
{
    /// <summary>
    /// SimpleTcp client events.
    /// </summary>
    public class SimpleTcpClientEvents
    {
        #region Public-Members

        /// <summary>
        /// Event to call when the connection is established.
        /// </summary>
        public event Action<SimpleTcpClient ,ClientConnectedEventArgs> Connected;

        /// <summary>
        /// Event to call when the connection is destroyed.
        /// </summary>
        public event Action<SimpleTcpClient, ClientDisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Event to call when byte data has become available from the server.
        /// </summary>
        public event Action<SimpleTcpClient, DataReceivedEventArgs> DataReceived;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SimpleTcpClientEvents()
        {

        }

        #endregion

        #region Public-Methods

        internal void HandleConnected(SimpleTcpClient client, ClientConnectedEventArgs args)
        {
            Connected?.Invoke(client, args);
        }

        internal void HandleClientDisconnected(SimpleTcpClient client, ClientDisconnectedEventArgs args)
        {
            Disconnected?.Invoke(client, args);
        }

        internal void HandleDataReceived(SimpleTcpClient client, DataReceivedEventArgs args)
        {
            DataReceived?.Invoke(client, args);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
