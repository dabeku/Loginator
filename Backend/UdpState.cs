using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Backend {
    
    public class UdpState {

        public UdpState(UdpClient client, IPEndPoint endpoint) {
            u = client;
            e = endpoint;
        }

        public IPEndPoint e;
        public UdpClient u;
    }
}
