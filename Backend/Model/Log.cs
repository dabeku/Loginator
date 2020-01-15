using Common;
using System;
using System.Collections.Generic;

namespace Backend.Model {

    public class Log {
        /// <summary>
        /// The date and time the log happened. Either this comes from the logging source or is set when received.
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The log level in the form "INFO", "ERROR", etc. This should always be available.
        /// </summary>
        public LoggingLevel Level { get; set; }
        /// <summary>
        /// The log message. Can be anything the logging source writes. This should always be available.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The exception details including the stacktrace. May not be available.
        /// </summary>
        public string Exception { get; set; }
        /// <summary>
        /// Gets or sets the machine name of the log. May not be available.
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// The namespace of the log. May be set to "global" if no namespace is available.
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// The application of the log. May be set to "global" if no application is available.
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// The thread id of the logging application. May not be available.
        /// </summary>
        public string Thread { get; set; }
        /// <summary>
        /// The context of the logging application. May not be available.
        /// </summary>
        public string Context { get; set; }
        /// <summary>
        /// Additional properties. May not be available.
        /// </summary>
        public IEnumerable<Property> Properties { get; set; }

        public Log() {
            Properties = new List<Property>();
            Timestamp = DateTime.Now;
            Namespace = Constants.NAMESPACE_GLOBAL;
            Application = Constants.APPLICATION_GLOBAL;
        }

        private static Log def = new Log();
        public static Log DEFAULT {
            get {
                return def;
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
