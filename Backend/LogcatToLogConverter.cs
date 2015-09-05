using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Backend.Model;
using Common;

namespace Backend {

    public class LogcatToLogConverter {

        public IEnumerable<Log> Convert(string text) {
            if (text == null) {
                return new Log[] { Log.DEFAULT };
            }

            string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 1) {
                lines = text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }

            IList<Log> logs = new List<Log>();

            foreach (string line in lines) {

                // Example: I/ActivityManager(  585): Starting activity: Intent { action=android.intent.action...}
                int indexOfTag = line.IndexOf('/');
                int indexOfMessage = line.IndexOf(':');

                if (indexOfTag == -1 && indexOfMessage == -1) {
                    return new Log[] { Log.DEFAULT };
                }

                Log log = new Log();
                string level = line.Substring(0, indexOfTag);
                if (level == "V") {
                    level = LogLevel.TRACE;
                }
                else if (level == "D") {
                    level = LogLevel.DEBUG;
                }
                else if (level == "I") {
                    level = LogLevel.INFO;
                }
                else if (level == "W") {
                    level = LogLevel.WARN;
                }
                else if (level == "E") {
                    level = LogLevel.ERROR;
                }
                else if (level == "F") {
                    level = LogLevel.FATAL;
                }
                log.Level = level;
                string ns = line.Substring(indexOfTag + 1, indexOfMessage - indexOfTag - 1);
                log.Namespace = Constants.NAMESPACE_LOGCAT + Constants.NAMESPACE_SPLITTER + ns;
                log.Message = line.Substring(indexOfMessage + 1).Trim();
                logs.Add(log);
            }
            return logs;
        }
    }
}
