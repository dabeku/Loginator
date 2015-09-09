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
using Common;
using System.Net.NetworkInformation;
using Common.Exceptions;
using Backend.Converter;
using Common.Configuration;

namespace Backend {

    public class Receiver {

        private LogType LogType { get; set; }
        private UdpClient Client { get; set; }
        private ILogConverter Converter { get; set; }

        public event EventHandler<LogReceivedEventArgs> LogReceived;

        public void Initialize(Configuration configuration) {

            LogType = configuration.LogType;
            int port = 0;
            if (LogType == LogType.CHAINSAW) {
                port = configuration.PortChainsaw;
                Converter = IoC.Get<ChainsawToLogConverter>();
            } else if (LogType == LogType.LOGCAT) {
                port = configuration.PortLogcat;
                Converter = IoC.Get<LogcatToLogConverter>();
            }
            if (Client != null) {
                Client.Close();
            }

            bool isPortAlreadyInUse = (from p
                                 in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
                                 where p.Port == port
                                 select p).Count() == 1;

            if (isPortAlreadyInUse) {
                throw new LoginatorException("Port " + port + " is already in use.");
            }

            Client = new UdpClient(port);
            UdpState state = new UdpState(Client, new IPEndPoint(IPAddress.Any, 0));
            Client.BeginReceive(new AsyncCallback(DataReceived), state);
        }

        private void DataReceived(IAsyncResult ar) {

            try {
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
                    IEnumerable<Log> logs = Converter.Convert(receivedText);
                    if (LogReceived != null) {
                        foreach (Log log in logs) {
                            if (log == Log.DEFAULT) {
                                continue;
                            }
                            LogReceived(this, new LogReceivedEventArgs(log));
                        }
                    }
                }

                // Restart listening for udp data packages
                c.BeginReceive(new AsyncCallback(DataReceived), ar.AsyncState);
            } catch (Exception e) {
                Console.WriteLine("Could not read package: " + e);
                //throw new LoginatorException("Could not read data. Please restart application.");
            }
        }
    }
}
