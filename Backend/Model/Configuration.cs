using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Model {
    public class Configuration {
        public LogType LogType { get; set; }
        public int PortChainsaw { get; set; }
        public int PortLogcat { get; set; }
        public LogTimeFormat LogTimeFormat { get; set; }
    }
}
