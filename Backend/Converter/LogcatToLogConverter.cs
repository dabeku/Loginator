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
using NLog;

namespace Backend.Converter {

    public class LogcatToLogConverter : ILogConverter {

        private ILogger Logger { get; set; }

        public LogcatToLogConverter() {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public IEnumerable<Log> Convert(string text) {
            if (text == null) {
                return new Log[] { Log.DEFAULT };
            }

            try {
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
                    if (indexOfTag == -1) {
                        log.Level = LoggingLevel.NOT_SET;
                    }
                    else {
                        string level = line.Substring(0, indexOfTag);
                        if (level == "V") {
                            level = LoggingLevel.TRACE;
                        }
                        else if (level == "D") {
                            level = LoggingLevel.DEBUG;
                        }
                        else if (level == "I") {
                            level = LoggingLevel.INFO;
                        }
                        else if (level == "W") {
                            level = LoggingLevel.WARN;
                        }
                        else if (level == "E") {
                            level = LoggingLevel.ERROR;
                        }
                        else if (level == "F") {
                            level = LoggingLevel.FATAL;
                        }
                        log.Level = level;
                    }

                    
                    string ns = line.Substring(indexOfTag + 1, indexOfMessage - indexOfTag - 1);

                    int braceOpenIndex = ns.IndexOf("(");
                    int braceCloseIndex = ns.IndexOf(")");
                    if (braceOpenIndex != -1 && braceCloseIndex != -1 && braceOpenIndex < braceCloseIndex) {
                        // Create the structure "Logcat.585.ActivityManager" out of "ActivityManager(  585)"
                        log.Namespace = Constants.NAMESPACE_LOGCAT + Constants.NAMESPACE_SPLITTER + ns.Substring(braceOpenIndex + 1, braceCloseIndex - braceOpenIndex - 1).Trim() + Constants.NAMESPACE_SPLITTER + ns.Substring(0, braceOpenIndex);
                    } else {
                        // Leave the structure as it is: "ActivityManager(  585)"
                        log.Namespace = Constants.NAMESPACE_LOGCAT + Constants.NAMESPACE_SPLITTER + ns;
                    }
                    
                    log.Message = line.Substring(indexOfMessage + 1).Trim();
                    if (!String.IsNullOrEmpty(log.Message)) {
                        logs.Add(log);
                    } else {
                        //Console.WriteLine("Do not log message as text is empty");
                    }
                }
                return logs;
            } catch (Exception e) {
                Logger.Error(e, "Could not read logcat data");
            }

            return new Log[] { Log.DEFAULT };
        }
    }
}
