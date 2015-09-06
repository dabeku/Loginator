using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Linq;
using Backend.Events;
using Backend.Model;

namespace Backend {

    public class Receiver {

        // TODO: Make configurable
        private const string LOG_TYPE = "chainsaw";

        private const string LOG_TYPE_CHAINSAW = "chainsaw";
        private const string LOG_TYPE_LOGCAT = "logcat";

        private const int LOCAL_PORT = 7071;

        private ChainsawToLogConverter ChainsawConverter { get; set; }
        private LogcatToLogConverter LogcatConverter { get; set; }

        public event EventHandler<LogReceivedEventArgs> LogReceived;

        public void Initialize() {
            ChainsawConverter = new ChainsawToLogConverter();
            LogcatConverter = new LogcatToLogConverter();

            UdpClient Client = new UdpClient(LOCAL_PORT);
            UdpState state = new UdpState(Client, new IPEndPoint(IPAddress.Any, 0));
            Client.BeginReceive(new AsyncCallback(DataReceived), state);

            // Wait for any key to terminate application
            //Console.ReadKey();
            //client.Close();
        }
        
        private void DataReceived(IAsyncResult ar) {

            UdpClient c = (UdpClient)((UdpState)ar.AsyncState).u;
            IPEndPoint wantedIpEndPoint = (IPEndPoint)((UdpState)(ar.AsyncState)).e;
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] receiveBytes = c.EndReceive(ar, ref receivedIpEndPoint);

            // Check sender
            bool isRightHost = (wantedIpEndPoint.Address.Equals(receivedIpEndPoint.Address)
                               || wantedIpEndPoint.Address.Equals(IPAddress.Any));
            bool isRightPort = (wantedIpEndPoint.Port == receivedIpEndPoint.Port)
                               || wantedIpEndPoint.Port == 0;
            if (isRightHost && isRightPort) {
                string receivedText = Encoding.ASCII.GetString(receiveBytes);

                IEnumerable<Log> logs;

                if (LOG_TYPE == LOG_TYPE_CHAINSAW) {
                    logs = ChainsawConverter.Convert(receivedText);
                } else if (LOG_TYPE == LOG_TYPE_LOGCAT) {
                    logs = LogcatConverter.Convert(receivedText);
                }

                //Console.WriteLine("Parsed (Level): " + log.Level);
                //Console.WriteLine("Parsed (Message): " + log.Message);

                if (LogReceived != null) {
                    foreach (Log log in logs) {
                        LogReceived(this, new LogReceivedEventArgs(log));
                    }
                }
            }

            // Restart listening for udp data packages
            c.BeginReceive(new AsyncCallback(DataReceived), ar.AsyncState);
        }
    }
}
