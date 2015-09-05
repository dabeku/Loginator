using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Backend {

    public class UDPListener {
        private const int listenPort = 7071;
        public void Start() {
            bool done = false;
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listenPort);
            string received_data;
            byte[] receive_byte_array;
            try {
                while (!done) {
                    Console.WriteLine("Waiting for broadcast");
                    // this is the line of code that receives the broadcase message.
                    // It calls the receive function from the object listener (class UdpClient)
                    // It passes to listener the end point groupEP.
                    // It puts the data from the broadcast message into the byte array
                    // named received_byte_array.
                    // I don't know why this uses the class UdpClient and IPEndPoint like this.
                    // Contrast this with the talker code. It does not pass by reference.
                    // Note that this is a synchronous or blocking call.
                    receive_byte_array = listener.Receive(ref groupEP);
                    Console.WriteLine("Received a broadcast from {0}", groupEP.ToString());
                    received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                    Console.WriteLine("data follows \n{0}\n\n", received_data);
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            listener.Close();
        }
    } // end of class UDPListener
}
