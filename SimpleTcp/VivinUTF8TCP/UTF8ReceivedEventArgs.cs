using System.Text;

namespace SimpleTcp.VivinUTF8TCP
{
    /// <summary>
    /// UTF8ReceivedEventArgs
    /// </summary>
    public class UTF8ReceivedEventArgs
    {
        internal UTF8ReceivedEventArgs(string ipPort, byte[] OneSentenceBytes,int byteCnt)
        {
            IpPort = ipPort;
            Sentence = GetUTF8Sentence(OneSentenceBytes,byteCnt);
        }
        internal UTF8ReceivedEventArgs(string ipPort, BuffManager buff)
        {
            IpPort = ipPort;
            Sentence = GetUTF8Sentence(buff.Data,buff.Counter);
        }
        internal UTF8ReceivedEventArgs(string ipPort, string sentence)
        {
            IpPort = ipPort;
            Sentence = sentence;
        }

        /// <summary>
        /// The IP address and port number of the connected endpoint.
        /// </summary>
        public string IpPort { get; }

        /// <summary>
        /// The data received from the client.
        /// </summary>
        public string Sentence { get; }

        string GetUTF8Sentence(byte[] OneSentenceBytes,int byteCnt)
        {
            var rst = Encoding.UTF8.GetString(OneSentenceBytes,1, byteCnt-2);  //去掉开头STX和结尾ETX
            return rst;
        }
    }
}