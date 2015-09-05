using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Model {

    public class Log {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Namespace { get; set; }
        public string Thread { get; set; }
        public IEnumerable<Property> Properties { get; set; }

        public bool IsVisible { get; set; }

        public Log() {
            Properties = new List<Property>();
            Timestamp = DateTime.Now;
        }

        public static Log DEFAULT {
            get {
                return new Log();
            }
        }
        
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            Log other = (Log)obj;
            return Timestamp.Ticks == other.Timestamp.Ticks &&
                Level == other.Level &&
                Message == other.Message &&
                Namespace == other.Namespace &&
                Thread == other.Thread;
        }

        public override int GetHashCode() {
            return new { Timestamp, Level, Message, Namespace, Thread }.GetHashCode();
        }
    }
}
