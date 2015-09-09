using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend.Model {
    public class LoggingLevel {

        public string Id { get; private set; }
        public string Name { get; private set; }

        public const string NOT_SET = "[not set]";
        public const string TRACE = "TRACE";
        public const string DEBUG = "DEBUG";
        public const string INFO = "INFO";
        public const string WARN = "WARN";
        public const string ERROR = "ERROR";
        public const string FATAL = "FATAL";

        public LoggingLevel(string name) {
            Id = name;
            Name = name;
        }

        public static bool IsLogLevelAboveMin(string level, string minLevel) {
            if (String.IsNullOrEmpty(level) || String.IsNullOrEmpty(minLevel)) {
                return false;
            }

            if (minLevel == TRACE) {
                return true;
            }
            if (minLevel == DEBUG && (level == DEBUG || level == INFO || level == WARN || level == ERROR || level == FATAL)) {
                return true;
            }
            if (minLevel == INFO && (level == INFO || level == WARN || level == ERROR || level == FATAL)) {
                return true;
            }
            if (minLevel == WARN && (level == WARN || level == ERROR || level == FATAL)) {
                return true;
            }
            if (minLevel == ERROR && (level == ERROR || level == FATAL)) {
                return true;
            }
            if (minLevel == FATAL && (level == FATAL)) {
                return true;
            }
            return false;
        }
    }
}
