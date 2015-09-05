using Backend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Events {

    public class LogReceivedEventArgs : EventArgs {

        public Log Log { get; private set; }

        public LogReceivedEventArgs(Log log) {
            Log = log;
        }
    }
}
