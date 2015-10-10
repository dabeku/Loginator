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
using System.Text.RegularExpressions;

namespace Backend.Converter {

    public class LogcatToLogConverter : ILogConverter {

        // https://regex101.com/
        private static readonly Regex Regex = new Regex(@"^(V|D|I|W|E|F|S)(\/)([ -~]+)(\()([0-9 ]+)(\))(\:)([ -~]+)$");
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

                // Example: I/ActivityManager(  585): Starting activity: Intent { action=android.intent.action...}
                // Namespace: Create "Logcat.585.ActivityManager" from "ActivityManager(  585)"
                foreach (string line in lines) {

                    if (!Regex.IsMatch(line)) {
                        continue;
                    }

                    Log log = new Log();

                    foreach (Match match in Regex.Matches(line)) {
                        var group = match.Groups;
                        log.Level = GetLogLevel(group[1].Value);
                        log.Namespace = group[3].Value.Trim();
                        log.Namespace = Constants.NAMESPACE_LOGCAT + Constants.NAMESPACE_SPLITTER + group[5].Value.Trim() + Constants.NAMESPACE_SPLITTER + log.Namespace;
                        log.Message = group[8].Value.Trim();
                    }
                    
                    if (!String.IsNullOrEmpty(log.Message)) {
                        logs.Add(log);
                    }
                }
                return logs;
            } catch (Exception e) {
                Logger.Error(e, "Could not read logcat data");
            }

            return new Log[] { Log.DEFAULT };
        }

        private LoggingLevel GetLogLevel(string logLevel) {
            LoggingLevel level = LoggingLevel.NOT_SET;
            if (logLevel == "V") {
                level = LoggingLevel.TRACE;
            } else if (logLevel == "D") {
                level = LoggingLevel.DEBUG;
            } else if (logLevel == "I") {
                level = LoggingLevel.INFO;
            } else if (logLevel == "W") {
                level = LoggingLevel.WARN;
            } else if (logLevel == "E") {
                level = LoggingLevel.ERROR;
            } else if (logLevel == "F") {
                level = LoggingLevel.FATAL;
            }
            return level;
        }
    }
}
