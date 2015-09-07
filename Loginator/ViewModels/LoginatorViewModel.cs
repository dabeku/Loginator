using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogApplication.ViewModels {
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using System.Windows.Threading;
    using GalaSoft.MvvmLight.Threading;
    using Backend;
    using Backend.Events;
    using System.Threading;
    using Backend.Model;
    using Common;

    public class LoginatorViewModel : INotifyPropertyChanged {

        private readonly static LoginatorViewModel instance = new LoginatorViewModel();

        private const int TIME_INTERVAL_IN_MILLISECONDS = 1000;
        private const int DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL = 1000;
        private static object SYNC_OBJECT = new Object();
        private Receiver Receiver { get; set; }
        private Timer Timer { get; set; }

        public static LoginatorViewModel Instance {
            get { return instance; }
        }

        private bool isActive;
        public bool IsActive {
            get { return isActive; }
            set {
                isActive = value;
                OnPropertyChanged("IsActive");
            }
        }
        
        private int numberOfLogsPerApplicationAndLevelInternal;
        private int numberOfLogsPerLevel;
        public int NumberOfLogsPerLevel {
            get { return numberOfLogsPerLevel; }
            set {
                numberOfLogsPerLevel = value;
                OnPropertyChanged("IsScrollingToBottom");
            }
        }

        private string searchCriteriaInternal;
        private string searchCriteria;
        public string SearchCriteria {
            get { return searchCriteria; }
            set {
                searchCriteria = value;
                OnPropertyChanged("SearchCriteria");
            }
        }

        private LogViewModel selectedLog;
        public LogViewModel SelectedLog {
            get { return selectedLog; }
            set {
                selectedLog = value;
                OnPropertyChanged("SelectedLog");
            }
        }

        private List<LogViewModel> LogsToInsert { get; set; }

        public ObservableRangeCollection<LogViewModel> Logs { get; set; }
        public ObservableCollection<NamespaceViewModel> Namespaces { get; set; }
        public ObservableCollection<ApplicationViewModel> Applications { get; set; }

        private LoginatorViewModel() {
            IsActive = true;
            NumberOfLogsPerLevel = DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL;
            numberOfLogsPerApplicationAndLevelInternal = DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL;
            Logs = new ObservableRangeCollection<LogViewModel>();
            LogsToInsert = new List<LogViewModel>();
            Namespaces = new ObservableCollection<NamespaceViewModel>();
            Applications = new ObservableCollection<ApplicationViewModel>();
            Receiver = new Receiver();
            Receiver.Initialize();
            Receiver.LogReceived += Receiver_LogReceived;
            Timer = new Timer(Callback, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
        }

        private void Callback(Object state) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (SYNC_OBJECT) {
                    UpdateLogs();
                    UpdateNamespaces();
                    UpdateApplications();
                    Timer.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
                }
            });
        }

        private LogViewModel ToLogViewModel(Log log) {
            return new LogViewModel() {
                Exception = log.Exception,
                Level = log.Level,
                Message = log.Message,
                Namespace = log.Namespace,
                Application = log.Application,
                Properties = log.Properties,
                Thread = log.Thread,
                Timestamp = log.Timestamp
            };
        }

        private void Receiver_LogReceived(object sender, LogReceivedEventArgs e) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (SYNC_OBJECT) {
                    // Add a log entry only to the list if
                    // * global logging is active (checkbox)
                    // * no application was found (assume that it's the first log and we need this to populate the application list)
                    // * an application was found, is active (checkbox at application) and the log level is above the min level (dropdown at application)
                    if (!IsActive) {
                        return;
                    }
                    LogViewModel log = ToLogViewModel(e.Log);
                    var application = Applications.FirstOrDefault(m => m.Name == log.Application);
                    if (application != null && (!application.IsActive || !LogLevel.IsLogLevelAboveMin(log.Level, application.SelectedMinLogLevel))) {
                        return;
                    }
                    LogsToInsert.Add(log);
                }
            });
        }

        private void UpdateLogs() {

            var logsToInsert = LogsToInsert.OrderBy(m => m.Timestamp);
            Logs.AddRangeAtStart(logsToInsert);
            LogsToInsert.Clear();

            var applications = new List<string>(Logs.Select(m => m.Application).Distinct());
            var levels = new List<string>(Logs.Select(m => m.Level).Distinct());
            foreach (var application in applications) {
                foreach (var level in levels) {
                    var logsByApplicationAndLevel = Logs.Where(m => m.Level == level && m.Application == application);
                    int logCountByApplicationAndLevel = logsByApplicationAndLevel.Count();
                    while (logCountByApplicationAndLevel > numberOfLogsPerApplicationAndLevelInternal) {
                        Logs.Remove(logsByApplicationAndLevel.ElementAt(logsByApplicationAndLevel.Count() - 1));
                        logsByApplicationAndLevel = Logs.Where(m => m.Level == level && m.Application == application);
                        logCountByApplicationAndLevel = logsByApplicationAndLevel.Count();
                    }
                }
            }
        }

        private void UpdateNamespaces() {
            foreach (var log in Logs) {
                // Example: Verbosus.VerbTeX.View
                string nsLogFull = log.Namespace;
                // Example: Verbosus
                string nsLogPart = nsLogFull.Split(new string[] { Constants.NAMESPACE_SPLITTER }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                // Try to get existing namespace with name Verbosus
                var ns = Namespaces.FirstOrDefault(m => m.Name == nsLogPart);
                if (ns == null) {
                    ns = new NamespaceViewModel(nsLogPart);
                    Namespaces.Add(ns);
                }
                HandleNamespace(ns, nsLogFull.Substring(nsLogFull.IndexOf(Constants.NAMESPACE_SPLITTER) + 1));
            }

            foreach (var ns in Namespaces) {
                ResetAllCount(ns);
            }

            foreach (var log in Logs) {
                foreach (var ns in Namespaces) {
                    HandleLogVisibilityByNamespace(log, ns, ns.Name);
                }
            }
        }

        private void HandleNamespace(NamespaceViewModel parent, string suffix) {
            // Example: VerbTeX.View (Verbosus was processed before)
            string nsLogFull = suffix;
            // Example: VerbTeX
            string nsLogPart = nsLogFull.Split(new string[] { Constants.NAMESPACE_SPLITTER }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            // Try to get existing namespace with name VerbTeX
            var nsChild = parent.Children.FirstOrDefault(m => m.Name == nsLogPart);
            if (nsChild == null) {
                nsChild = new NamespaceViewModel(nsLogPart);
                nsChild.IsChecked = parent.IsChecked;
                parent.Children.Add(nsChild);
                nsChild.Parent = parent;
            }

            if (suffix.Contains(Constants.NAMESPACE_SPLITTER)) {
                HandleNamespace(nsChild, suffix.Substring(suffix.IndexOf(Constants.NAMESPACE_SPLITTER) + 1));
            }
        }

        private void UpdateApplications() {
            foreach (var log in Logs) {
                var application = Applications.FirstOrDefault(m => m.Name == log.Application);
                if (application == null) {
                    application = new ApplicationViewModel(log.Application);
                    Applications.Add(application);
                }
            }
        }

        private void ResetAllCount(NamespaceViewModel ns) {
            ns.Count = 0;
            ns.CountTrace = 0;
            ns.CountDebug = 0;
            ns.CountInfo = 0;
            ns.CountWarn = 0;
            ns.CountError = 0;
            ns.CountFatal = 0;
            foreach (var child in ns.Children) {
                ResetAllCount(child);
            }
        }

        private void HandleLogVisibilityByNamespace(LogViewModel log, NamespaceViewModel ns, string currentNamespace) {
            foreach (var child in ns.Children) {
                string nsAbsolute = currentNamespace + Constants.NAMESPACE_SPLITTER + child.Name;
                if (log.Namespace == nsAbsolute) {
                    child.Count++;
                    switch (log.Level) {
                        case LogLevel.TRACE:
                            child.CountTrace++;
                            break;
                        case LogLevel.DEBUG:
                            child.CountDebug++;
                            break;
                        case LogLevel.INFO:
                            child.CountInfo++;
                            break;
                        case LogLevel.WARN:
                            child.CountWarn++;
                            break;
                        case LogLevel.ERROR:
                            child.CountError++;
                            break;
                        case LogLevel.FATAL:
                            child.CountFatal++;
                            break;
                    }
                    // Only active if namespace AND application are active
                    if (child.IsChecked) {
                        var application = Applications.FirstOrDefault(m => m.Name == log.Application);
                        if (application != null &&
                            application.IsActive &&
                            LogLevel.IsLogLevelAboveMin(log.Level, application.SelectedMinLogLevel) &&
                            (String.IsNullOrEmpty(searchCriteriaInternal) ||
                                log.Application.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant()) ||
                                log.Namespace.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant()) ||
                                log.Message.ToLowerInvariant().Contains(searchCriteriaInternal) ||
                                (!String.IsNullOrEmpty(log.Exception) && log.Exception.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant())))) {
                            log.IsVisible = true;
                        } else {
                            log.IsVisible = false;
                        }
                    } else {
                        log.IsVisible = false;
                    }
                } else {
                    HandleLogVisibilityByNamespace(log, child, currentNamespace + Constants.NAMESPACE_SPLITTER + child.Name);
                }
            }
        }

        public void Update() {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (SYNC_OBJECT) {
                    UpdateLogs();
                }
            });
        }

        private ICommand clearLogsCommand;
        public ICommand ClearLogsCommand {
            get {
                if (clearLogsCommand == null) {
                    clearLogsCommand = new DelegateCommand(HandleClearLogsCommand);
                }
                return clearLogsCommand;
            }
        }
        public void HandleClearLogsCommand(object one) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (SYNC_OBJECT) {
                    Logs.Clear();
                }
            });
        }

        private ICommand clearAllCommand;
        public ICommand ClearAllCommand {
            get {
                if (clearAllCommand == null) {
                    clearAllCommand = new DelegateCommand(HandleClearAllCommand);
                }
                return clearAllCommand;
            }
        }
        public void HandleClearAllCommand(object one) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (SYNC_OBJECT) {
                    Logs.Clear();
                    Namespaces.Clear();
                    Applications.Clear();
                }
            });
        }

        private ICommand updateNumberOfLogsPerLevelCommand;
        public ICommand UpdateNumberOfLogsPerLevelCommand {
            get {
                if (updateNumberOfLogsPerLevelCommand == null) {
                    updateNumberOfLogsPerLevelCommand = new DelegateCommand(HandleUpdateNumberOfLogsPerLevelCommand);
                }
                return updateNumberOfLogsPerLevelCommand;
            }
        }
        public void HandleUpdateNumberOfLogsPerLevelCommand(object one) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (SYNC_OBJECT) {
                    numberOfLogsPerApplicationAndLevelInternal = NumberOfLogsPerLevel;
                    Update();
                }
            });
        }

        private ICommand updateSearchCriteriaCommand;
        public ICommand UpdateSearchCriteriaCommand {
            get {
                if (updateSearchCriteriaCommand == null) {
                    updateSearchCriteriaCommand = new DelegateCommand(HandleUpdateSearchCriteriaCommand);
                }
                return updateSearchCriteriaCommand;
            }
        }
        public void HandleUpdateSearchCriteriaCommand(object one) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (SYNC_OBJECT) {
                    searchCriteriaInternal = SearchCriteria;
                    Update();
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
