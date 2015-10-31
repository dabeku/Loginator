using Backend.Model;
using Common;
using LogApplication.Collections;
using Loginator.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogApplication.ViewModels {

    /// <summary>
    /// If you add a new function / filter assure the following
    /// * Check that application is active: IsActive
    /// * Check that you got logs from selected loglevel: GetLogsByLevel()
    /// * Check if the namespace is active: IsNamespaceActive()
    /// * Check if the search criteria match: IsSearchCriteriaMatch()
    /// </summary>
    public class ApplicationViewModel : INotifyPropertyChanged {

        public string Name { get; set; }
        public IList<LoggingLevel> LogLevels { get; set; }

        private int MaxNumberOfLogsPerLevel { get; set; }
        private string SearchCriteria { get; set; }
        private bool IsSearchCriteriaInverted { get; set; }

        private OrderedObservableCollection Logs { get; set; }
        private List<LogViewModel> LogsTrace { get; set; }
        private List<LogViewModel> LogsDebug { get; set; }
        private List<LogViewModel> LogsInfo { get; set; }
        private List<LogViewModel> LogsWarn { get; set; }
        private List<LogViewModel> LogsError { get; set; }
        private List<LogViewModel> LogsFatal { get; set; }
        private ObservableCollection<NamespaceViewModel> Namespaces { get; set; }

        private LoggingLevel selectedMinLogLevel;
        public LoggingLevel SelectedMinLogLevel {
            get {
                return selectedMinLogLevel;
            }
            set {
                lock (ViewModelConstants.SYNC_OBJECT) {
                    UpdateByLogLevelChange(selectedMinLogLevel, value);
                    selectedMinLogLevel = value;
                    OnPropertyChanged(nameof(SelectedMinLogLevel));
                }
            }
        }

        private bool isActive;
        public bool IsActive {
            get {
                return isActive;
            }
            set {
                lock (ViewModelConstants.SYNC_OBJECT) {
                    UpdateByActiveChange(isActive, value);
                    isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public ApplicationViewModel(string name, OrderedObservableCollection logs, ObservableCollection<NamespaceViewModel> namespaces) {
            Name = name;
            IsActive = true;
            LogLevels = new List<LoggingLevel>();
            LogLevels.Add(LoggingLevel.TRACE);
            LogLevels.Add(LoggingLevel.DEBUG);
            LogLevels.Add(LoggingLevel.INFO);
            LogLevels.Add(LoggingLevel.WARN);
            LogLevels.Add(LoggingLevel.ERROR);
            LogLevels.Add(LoggingLevel.FATAL);
            SelectedMinLogLevel = LogLevels.ElementAt(0);
            Logs = logs;
            LogsTrace = new List<LogViewModel>();
            LogsDebug = new List<LogViewModel>();
            LogsInfo = new List<LogViewModel>();
            LogsWarn = new List<LogViewModel>();
            LogsError = new List<LogViewModel>();
            LogsFatal = new List<LogViewModel>();
            Namespaces = namespaces;
            MaxNumberOfLogsPerLevel = Constants.DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL;
        }

        public void ClearLogs() {
            LogsTrace = new List<LogViewModel>();
            LogsDebug = new List<LogViewModel>();
            LogsInfo = new List<LogViewModel>();
            LogsWarn = new List<LogViewModel>();
            LogsError = new List<LogViewModel>();
            LogsFatal = new List<LogViewModel>();
        }

        private void UpdateByLogLevelChange(LoggingLevel oldLogLevel, LoggingLevel newLogLevel) {
            if (!IsActive) {
                return;
            }
            if (oldLogLevel == null || newLogLevel == null || oldLogLevel.Id == newLogLevel.Id) {
                return;
            }
            bool isAdd = oldLogLevel.Id > newLogLevel.Id;
            var levelsBetween = LoggingLevel.GetLogLevelsBetween(oldLogLevel, newLogLevel);
            foreach (var levelBetween in levelsBetween) {
                if (levelBetween == LoggingLevel.TRACE) {
                    if (isAdd) {
                        LogsTrace.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.AddOrdered(m); });
                    } else {
                        LogsTrace.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.Remove(m); });
                    }
                } else if (levelBetween == LoggingLevel.DEBUG) {
                    if (isAdd) {
                        LogsDebug.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.AddOrdered(m); });
                    } else {
                        LogsDebug.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.Remove(m); });
                    }
                } else if (levelBetween == LoggingLevel.INFO) {
                    if (isAdd) {
                        LogsInfo.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.AddOrdered(m); });
                    } else {
                        LogsInfo.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.Remove(m); });
                    }
                } else if (levelBetween == LoggingLevel.WARN) {
                    if (isAdd) {
                        LogsWarn.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.AddOrdered(m); });
                    } else {
                        LogsWarn.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.Remove(m); });
                    }
                } else if (levelBetween == LoggingLevel.ERROR) {
                    if (isAdd) {
                        LogsError.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.AddOrdered(m); });
                    } else {
                        LogsError.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.Remove(m); });
                    }
                } else if (levelBetween == LoggingLevel.FATAL) {
                    if (isAdd) {
                        LogsFatal.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.AddOrdered(m); });
                    } else {
                        LogsFatal.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.Remove(m); });
                    }
                }
            }
        }

        private void UpdateByActiveChange(bool oldIsActive, bool newIsActive) {
            if (oldIsActive == newIsActive) {
                return;
            }
            if (newIsActive) {
                var levels = GetLogsByLevel(SelectedMinLogLevel);
                levels.ForEach((m) => { if (IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) Logs.AddOrdered(m); });
            } else {
                LogsTrace.ForEach((m) => { Logs.Remove(m); });
                LogsDebug.ForEach((m) => { Logs.Remove(m); });
                LogsInfo.ForEach((m) => { Logs.Remove(m); });
                LogsWarn.ForEach((m) => { Logs.Remove(m); });
                LogsError.ForEach((m) => { Logs.Remove(m); });
                LogsFatal.ForEach((m) => { Logs.Remove(m); });
            }
        }

        private List<LogViewModel> GetLogsByLevel(LoggingLevel level) {
            if (level == LoggingLevel.TRACE) {
                return LogsTrace.Union(LogsDebug).Union(LogsInfo).Union(LogsWarn).Union(LogsError).Union(LogsFatal).ToList();
            }
            else if (level == LoggingLevel.DEBUG) {
                return LogsDebug.Union(LogsInfo).Union(LogsWarn).Union(LogsError).Union(LogsFatal).ToList();
            }
            else if (level == LoggingLevel.INFO) {
                return LogsInfo.Union(LogsWarn).Union(LogsError).Union(LogsFatal).ToList();
            }
            else if (level == LoggingLevel.WARN) {
                return LogsWarn.Union(LogsError).Union(LogsFatal).ToList();
            }
            else if (level == LoggingLevel.ERROR) {
                return LogsError.Union(LogsFatal).ToList();
            }
            else if (level == LoggingLevel.FATAL) {
                return LogsFatal.ToList();
            }
            return new List<LogViewModel>();
        }

        public void UpdateByNamespaceChange(NamespaceViewModel ns) {
            if (!IsActive) {
                return;
            }
            var logs = GetLogsByLevel(SelectedMinLogLevel);
            if (ns.IsChecked) {
                List<LogViewModel> logsToAdd = logs.Where(m => Name + Constants.NAMESPACE_SPLITTER + m.Namespace == ns.Fullname).ToList();
                logsToAdd.ForEach((m) => { if (IsSearchCriteriaMatch(m)) Logs.AddOrdered(m); });
            } else {
                List<LogViewModel> logsToRemove = logs.Where(m => Name + Constants.NAMESPACE_SPLITTER + m.Namespace == ns.Fullname).ToList();
                logsToRemove.ForEach((m) => { Logs.Remove(m); });
            }
        }

        public void UpdateMaxNumberOfLogs(int maxNumberOfLogs) {

            if (MaxNumberOfLogsPerLevel <= maxNumberOfLogs) {
                MaxNumberOfLogsPerLevel = maxNumberOfLogs;
                return;
            }

            MaxNumberOfLogsPerLevel = maxNumberOfLogs;
            List<LogViewModel> logsToRemove = new List<LogViewModel>();
            List<LogViewModel> logsToRemoveTrace = LogsTrace.Take(LogsTrace.Count - maxNumberOfLogs).ToList();
            logsToRemoveTrace.ForEach((m) => { LogsTrace.Remove(m); });
            List<LogViewModel> logsToRemoveDebug = LogsDebug.Take(LogsDebug.Count - maxNumberOfLogs).ToList();
            logsToRemoveDebug.ForEach((m) => { LogsDebug.Remove(m); });
            List<LogViewModel> logsToRemoveInfo = LogsInfo.Take(LogsInfo.Count - maxNumberOfLogs).ToList();
            logsToRemoveInfo.ForEach((m) => { LogsInfo.Remove(m); });
            List<LogViewModel> logsToRemoveWarn = LogsWarn.Take(LogsWarn.Count - maxNumberOfLogs).ToList();
            logsToRemoveWarn.ForEach((m) => { LogsWarn.Remove(m); });
            List<LogViewModel> logsToRemoveError = LogsError.Take(LogsError.Count - maxNumberOfLogs).ToList();
            logsToRemoveError.ForEach((m) => { LogsError.Remove(m); });
            List<LogViewModel> logsToRemoveFatal = LogsFatal.Take(LogsFatal.Count - maxNumberOfLogs).ToList();
            logsToRemoveFatal.ForEach((m) => { LogsFatal.Remove(m); });

            logsToRemove.AddRange(logsToRemoveTrace);
            logsToRemove.AddRange(logsToRemoveDebug);
            logsToRemove.AddRange(logsToRemoveInfo);
            logsToRemove.AddRange(logsToRemoveWarn);
            logsToRemove.AddRange(logsToRemoveError);
            logsToRemove.AddRange(logsToRemoveFatal);

            logsToRemove.ForEach((m) => {
                if (IsActive && LoggingLevel.IsLogLevelAboveMin(m.Level, SelectedMinLogLevel) && IsNamespaceActive(m) && IsSearchCriteriaMatch(m)) {
                    Logs.Remove(m);
                }
            });
        }

        public void UpdateSearchCriteria(string criteria, bool isSearchCriteriaInverted) {
            SearchCriteria = criteria;
            IsSearchCriteriaInverted = isSearchCriteriaInverted;

            if (!IsActive) {
                return;
            }
            var logs = GetLogsByLevel(SelectedMinLogLevel);

            List<LogViewModel> logsToAdd = logs.Where(m => IsSearchCriteriaMatch(m)).ToList();
            logsToAdd.ForEach((m) => { if (IsNamespaceActive(m)) Logs.AddOrdered(m); });
            List<LogViewModel> logsToRemove = logs.Where(m => !IsSearchCriteriaMatch(m)).ToList();
            logsToRemove.ForEach((m) => { Logs.Remove(m); });
        }

        public void AddLog(LogViewModel log) {
            LogViewModel logToRemove = null;
            if (log.Level == LoggingLevel.TRACE) {
                LogsTrace.Add(log);
                if (LogsTrace.Count > MaxNumberOfLogsPerLevel) {
                    var last = LogsTrace.First();
                    LogsTrace.Remove(last);
                    logToRemove = last;
                }
            } else if (log.Level == LoggingLevel.DEBUG) {
                LogsDebug.Add(log);
                if (LogsDebug.Count > MaxNumberOfLogsPerLevel) {
                    var last = LogsDebug.First();
                    LogsDebug.Remove(last);
                    logToRemove = last;
                }
            } else if (log.Level == LoggingLevel.INFO) {
                LogsInfo.Add(log);
                if (LogsInfo.Count > MaxNumberOfLogsPerLevel) {
                    var last = LogsInfo.First();
                    LogsInfo.Remove(last);
                    logToRemove = last;
                }
            } else if (log.Level == LoggingLevel.WARN) {
                LogsWarn.Add(log);
                if (LogsWarn.Count > MaxNumberOfLogsPerLevel) {
                    var last = LogsWarn.First();
                    LogsWarn.Remove(last);
                    logToRemove = last;
                }
            } else if (log.Level == LoggingLevel.ERROR) {
                LogsError.Add(log);
                if (LogsError.Count > MaxNumberOfLogsPerLevel) {
                    var last = LogsError.First();
                    LogsError.Remove(last);
                    logToRemove = last;
                }
            } else if (log.Level == LoggingLevel.FATAL) {
                LogsFatal.Add(log);
                if (LogsFatal.Count > MaxNumberOfLogsPerLevel) {
                    var last = LogsFatal.First();
                    LogsFatal.Remove(last);
                    logToRemove = last;
                }
            }

            if (IsActive && LoggingLevel.IsLogLevelAboveMin(log.Level, SelectedMinLogLevel) && IsNamespaceActive(log) && IsSearchCriteriaMatch(log)) {
                Logs.Insert(0, log);
                if (logToRemove != null) {
                    Logs.Remove(logToRemove);
                }
            }
        }

        private bool IsSearchCriteriaMatch(LogViewModel log) {
            try {
                // Default
                if (String.IsNullOrEmpty(SearchCriteria)) {
                    return true;
                }

                // Search
                if (!IsSearchCriteriaInverted) {
                    if ((!String.IsNullOrEmpty(log.Application) && log.Application.ToLowerInvariant().Contains(SearchCriteria.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Namespace) && log.Namespace.ToLowerInvariant().Contains(SearchCriteria.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Message) && log.Message.ToLowerInvariant().Contains(SearchCriteria.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Exception) && log.Exception.ToLowerInvariant().Contains(SearchCriteria.ToLowerInvariant()))) {
                        return true;
                    }
                    return false;
                }
                else {
                    if ((!String.IsNullOrEmpty(log.Application) && log.Application.ToLowerInvariant().Contains(SearchCriteria.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Namespace) && log.Namespace.ToLowerInvariant().Contains(SearchCriteria.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Message) && log.Message.ToLowerInvariant().Contains(SearchCriteria.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Exception) && log.Exception.ToLowerInvariant().Contains(SearchCriteria.ToLowerInvariant()))) {
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception e) {
                Console.WriteLine("Invalid search criteria: " + e);
                return false;
            }
        }

        private bool IsNamespaceActive(LogViewModel log) {
            // Try to get existing root namespace with name of application
            var nsApplication = Namespaces.FirstOrDefault(m => m.Name == log.Application);
            if (nsApplication == null) {
                return false;
            }

            // Example: Verbosus.VerbTeX.View
            string nsLogFull = log.Namespace;
            // Example: Verbosus
            string nsLogPart = nsLogFull.Split(new string[] { Constants.NAMESPACE_SPLITTER }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            // Try to get existing namespace with name Verbosus
            var ns = nsApplication.Children.FirstOrDefault(m => m.Name == nsLogPart);
            if (ns == null) {
                return false;
            }
            if (nsLogFull.Contains(Constants.NAMESPACE_SPLITTER)) {
                return IsNamespaceActive(ns, nsLogFull.Substring(nsLogFull.IndexOf(Constants.NAMESPACE_SPLITTER) + 1));
            }
            else {
                return ns.IsChecked;
            }
        }

        private bool IsNamespaceActive(NamespaceViewModel parent, string suffix) {
            // Example: VerbTeX.View (Verbosus was processed before)
            string nsLogFull = suffix;
            // Example: VerbTeX
            string nsLogPart = nsLogFull.Split(new string[] { Constants.NAMESPACE_SPLITTER }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            // Try to get existing namespace with name VerbTeX
            var nsChild = parent.Children.FirstOrDefault(m => m.Name == nsLogPart);
            if (nsChild == null) {
                return false;
            }
            if (suffix.Contains(Constants.NAMESPACE_SPLITTER)) {
                return IsNamespaceActive(nsChild, suffix.Substring(suffix.IndexOf(Constants.NAMESPACE_SPLITTER) + 1));
            }
            else {
                return nsChild.IsChecked;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
