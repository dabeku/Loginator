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
    using Loginator;
    using Backend.Manager;
    using GalaSoft.MvvmLight.Command;
    using Common.Configuration;
    using Backend.Dao;
    using System.Diagnostics;
using NLog;
    using AutoMapper;

    public class LoginatorViewModel : INotifyPropertyChanged {

        internal IConfigurationDao ConfigurationDao { get; set; }
        internal IApplicationConfiguration ApplicationConfiguration { get; set; }

        private const int TIME_INTERVAL_IN_MILLISECONDS = 1000;
        private const int DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL = 1000;
        private static object SYNC_OBJECT = new Object();
        private ILogger Logger { get; set; }
        private Receiver Receiver { get; set; }
        private Timer Timer { get; set; }

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

        private bool isInverted;
        public bool IsInverted {
            get { return isInverted; }
            set {
                isInverted = value;
                OnPropertyChanged("IsInverted");
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

        public LoginatorViewModel(IApplicationConfiguration applicationConfiguration, IConfigurationDao configurationDao) {
            ApplicationConfiguration = applicationConfiguration;
            ConfigurationDao = configurationDao;
            IsActive = true;
            NumberOfLogsPerLevel = DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL;
            numberOfLogsPerApplicationAndLevelInternal = DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL;
            Logger = LogManager.GetCurrentClassLogger();
            Logs = new ObservableRangeCollection<LogViewModel>();
            LogsToInsert = new List<LogViewModel>();
            Namespaces = new ObservableCollection<NamespaceViewModel>();
            Applications = new ObservableCollection<ApplicationViewModel>();
        }

        public void StartListener() {
            Receiver = IoC.Get<Receiver>();
            Receiver.LogReceived += Receiver_LogReceived;
            Timer = new Timer(Callback, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            Receiver.Initialize(ConfigurationDao.Read());
        }

        private void Callback(Object state) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                // TODO: Refactor this so we can use using(...)
                Stopwatch sw = new Stopwatch();
                lock (SYNC_OBJECT) {
                    sw.Start();
                    UpdateLogs();
                    sw.Stop();
                    if (ApplicationConfiguration.IsTimingTraceEnabled) {
                        Logger.Trace("[UpdateLogs] " + sw.ElapsedMilliseconds + "ms");
                    }
                    sw.Restart();
                    UpdateNamespaces();
                    sw.Stop();
                    if (ApplicationConfiguration.IsTimingTraceEnabled) {
                        Logger.Trace("[UpdateNamespaces] " + sw.ElapsedMilliseconds + "ms");
                    }
                    sw.Restart();
                    UpdateApplications();
                    sw.Stop();
                    if (ApplicationConfiguration.IsTimingTraceEnabled) {
                        Logger.Trace("[UpdateApplications] " + sw.ElapsedMilliseconds + "ms");
                    }
                    Timer.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
                }
            });
        }

        private LogViewModel ToLogViewModel(Log log) {
            return Mapper.Map<Log, LogViewModel>(log);
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
                    if (application != null && (!application.IsActive || !LoggingLevel.IsLogLevelAboveMin(log.Level, application.SelectedMinLogLevel))) {
                        return;
                    }
                    LogsToInsert.Add(log);
                }
            });
        }

        private void UpdateLogs() {
            try {
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
            } catch (Exception e) {
                Console.WriteLine("Could not update logs: " + e);
            }
        }

        private void UpdateNamespaces() {
            try {
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
            } catch (Exception e) {
                Console.WriteLine("Could not update namespaces: " + e);
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
            try {
                foreach (var log in Logs) {
                    var application = Applications.FirstOrDefault(m => m.Name == log.Application);
                    if (application == null) {
                        application = new ApplicationViewModel(log.Application);
                        Applications.Add(application);
                    }
                }
            } catch (Exception e) {
                Console.WriteLine("Could not update applications: " + e);
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
                        case LoggingLevel.TRACE:
                            child.CountTrace++;
                            break;
                        case LoggingLevel.DEBUG:
                            child.CountDebug++;
                            break;
                        case LoggingLevel.INFO:
                            child.CountInfo++;
                            break;
                        case LoggingLevel.WARN:
                            child.CountWarn++;
                            break;
                        case LoggingLevel.ERROR:
                            child.CountError++;
                            break;
                        case LoggingLevel.FATAL:
                            child.CountFatal++;
                            break;
                    }
                    // Only active if namespace AND application are active
                    if (child.IsChecked) {
                        var application = Applications.FirstOrDefault(m => m.Name == log.Application);
                        if (application != null &&
                            application.IsActive &&
                            LoggingLevel.IsLogLevelAboveMin(log.Level, application.SelectedMinLogLevel) &&
                            IsLogInSearchCriteria(log)) {
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

        private bool IsLogInSearchCriteria(LogViewModel log) {
            try {
                // Default
                if (String.IsNullOrEmpty(searchCriteriaInternal)) {
                    return true;
                }

                // Search
                if (!IsInverted) {
                    if ((!String.IsNullOrEmpty(log.Application) && log.Application.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Namespace) && log.Namespace.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Message) && log.Message.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Exception) && log.Exception.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant()))) {
                        return true;
                    }
                    return false;
                } else {
                    if ((!String.IsNullOrEmpty(log.Application) && log.Application.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Namespace) && log.Namespace.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Message) && log.Message.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant())) ||
                        (!String.IsNullOrEmpty(log.Exception) && log.Exception.ToLowerInvariant().Contains(searchCriteriaInternal.ToLowerInvariant()))) {
                        return false;
                    }
                    return true;
                }
            } catch (Exception e) {
                Console.WriteLine("Invalid search criteria: " + e);
                return false;
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
                    clearLogsCommand = new RelayCommand<LoginatorViewModel>(ClearLogs, CanClearLogs);
                }
                return clearLogsCommand;
            }
        }
        private bool CanClearLogs(LoginatorViewModel loginator) {
            return true;
        }
        public void ClearLogs(LoginatorViewModel loginator) {
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
                    clearAllCommand = new RelayCommand<LoginatorViewModel>(ClearAll, CanClearAll);
                }
                return clearAllCommand;
            }
        }
        private bool CanClearAll(LoginatorViewModel loginator) {
            return true;
        }
        public void ClearAll(LoginatorViewModel loginator) {
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
                    updateNumberOfLogsPerLevelCommand = new RelayCommand<LoginatorViewModel>(UpdateNumberOfLogsPerLevel, CanUpdateNumberOfLogsPerLevel);
                }
                return updateNumberOfLogsPerLevelCommand;
            }
        }
        private bool CanUpdateNumberOfLogsPerLevel(LoginatorViewModel loginator) {
            return true;
        }
        public void UpdateNumberOfLogsPerLevel(LoginatorViewModel loginator) {
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
                    updateSearchCriteriaCommand = new RelayCommand<LoginatorViewModel>(UpdateSearchCriteria, CanUpdateSearchCriteria);
                }
                return updateSearchCriteriaCommand;
            }
        }
        private bool CanUpdateSearchCriteria(LoginatorViewModel loginator) {
            return true;
        }
        public void UpdateSearchCriteria(LoginatorViewModel loginator) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (SYNC_OBJECT) {
                    searchCriteriaInternal = SearchCriteria;
                    Update();
                }
            });
        }

        private ICommand openConfigurationCommand;
        public ICommand OpenConfigurationCommand {
            get {
                if (openConfigurationCommand == null) {
                    openConfigurationCommand = new RelayCommand<LoginatorViewModel>(OpenConfiguration, CanOpenConfiguration);
                }
                return openConfigurationCommand;
            }
        }
        private bool CanOpenConfiguration(LoginatorViewModel loginator) {
            return true;
        }
        public void OpenConfiguration(LoginatorViewModel loginator) {
            new ConfigurationWindow().Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
