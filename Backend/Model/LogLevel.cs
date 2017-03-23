using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend.Model {
    public class LoggingLevel {

        public int Id { get; private set; }
        public string Name { get; private set; }

        public static readonly LoggingLevel NOT_SET = new LoggingLevel(-1, "[not set]");
        public static readonly LoggingLevel TRACE = new LoggingLevel(0, "TRACE");
        public static readonly LoggingLevel DEBUG = new LoggingLevel(1, "DEBUG");
        public static readonly LoggingLevel INFO = new LoggingLevel(2, "INFO");
        public static readonly LoggingLevel WARN = new LoggingLevel(3, "WARN");
        public static readonly LoggingLevel ERROR = new LoggingLevel(4, "ERROR");
        public static readonly LoggingLevel FATAL = new LoggingLevel(5, "FATAL");

        private static readonly IEnumerable<LoggingLevel> Levels = new[] { NOT_SET, TRACE, DEBUG, INFO, WARN, ERROR, FATAL };

        public LoggingLevel(int id, string name) {
            Id = id;
            Name = name;
        }

        public static LoggingLevel FromName(string name) {
            return Levels.FirstOrDefault(m => m.Name == name);
        }

        public static IEnumerable<LoggingLevel> GetAllLoggingLevels() {
            yield return LoggingLevel.TRACE;
            yield return LoggingLevel.DEBUG;
            yield return LoggingLevel.INFO;
            yield return LoggingLevel.WARN;
            yield return LoggingLevel.ERROR;
            yield return LoggingLevel.FATAL;
        }

        public static bool IsLogLevelAboveMin(LoggingLevel level, LoggingLevel minLevel) {
            if (level == null || minLevel == null) {
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

        /// <summary>
        /// Gets the log level incl. the toLevel if fromLevel != toLevel
        /// Example (remove): fromLevel = Debug, toLevel = Warn -> return = Debug, Info
        /// Example (add): fromLevel = Warn, toLevel = Debug -> return = Info, Debug
        /// </summary>
        public static LoggingLevel[] GetLogLevelsBetween(LoggingLevel fromLevel, LoggingLevel toLevel) {

            if (fromLevel == toLevel || fromLevel == null || toLevel == null) {
                return new LoggingLevel[] { };
            }

            IEnumerable<LoggingLevel> levelsBetween;
            if (fromLevel.Id < toLevel.Id) {
                levelsBetween = Levels.Where(m => m.Id >= fromLevel.Id && m.Id < toLevel.Id);
            } else {
                levelsBetween = Levels.Where(m => m.Id >= toLevel.Id && m.Id < fromLevel.Id);
            }

            return levelsBetween.ToArray();
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            LoggingLevel other = (LoggingLevel)obj;
            return Id == other.Id &&
                Name == other.Name;
        }

        public override int GetHashCode() {
            return new { Id, Name }.GetHashCode();
        }
    }
}
