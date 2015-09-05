using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Backend.Model;

namespace Backend {

    public class LogcatToLogConverter {

        public Log Convert(string text) {
            if (text == null) {
                return Log.DEFAULT;
            }

            // Example: I/ActivityManager(  585): Starting activity: Intent { action=android.intent.action...}
            int indexOfTag = text.IndexOf('/');
            int indexOfMessage = text.IndexOf(':');

            if (indexOfTag == -1 && indexOfMessage == -1) {
                return Log.DEFAULT;
            }

            Log log = new Log();
            string level = text.Substring(0, indexOfTag);
            if (level == "V") {
                level = LogLevel.TRACE;
            } else if (level == "D") {
                level = LogLevel.DEBUG;
            } else if (level == "I") {
                level = LogLevel.INFO;
            } else if (level == "W") {
                level = LogLevel.WARN;
            } else if (level == "E") {
                level = LogLevel.ERROR;
            } else if (level == "F") {
                level = LogLevel.FATAL;
            }
            log.Level = level;
            log.Namespace = text.Substring(indexOfTag + 1, indexOfMessage - indexOfTag);
            log.Message = text.Substring(indexOfMessage + 1);
            return log;
        }

    }
}
