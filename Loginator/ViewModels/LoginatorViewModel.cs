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
    using LogApplication.Collections;
    using Loginator.ViewModels;

    public class LoginatorViewModel : INotifyPropertyChanged {

        internal IConfigurationDao ConfigurationDao { get; set; }
        internal IApplicationConfiguration ApplicationConfiguration { get; set; }

        private const int TIME_INTERVAL_IN_MILLISECONDS = 1000;
        
        private ILogger Logger { get; set; }
        private Receiver Receiver { get; set; }
        private Timer Timer { get; set; }

        private bool isActive;
        public bool IsActive {
            get { return isActive; }
            set {
                isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
        
        private int numberOfLogsPerApplicationAndLevelInternal;
        private int numberOfLogsPerLevel;
        public int NumberOfLogsPerLevel {
            get { return numberOfLogsPerLevel; }
            set {
                numberOfLogsPerLevel = value;
                OnPropertyChanged(nameof(NumberOfLogsPerLevel));
            }
        }

        private string searchCriteriaInternal;
        private string searchCriteria;
        public string SearchCriteria {
            get { return searchCriteria; }
            set {
                searchCriteria = value;
                OnPropertyChanged(nameof(SearchCriteria));
            }
        }

        private bool isInverted;
        public bool IsInverted {
            get { return isInverted; }
            set {
                isInverted = value;
                OnPropertyChanged(nameof(IsInverted));
            }
        }

        private LogViewModel selectedLog;
        public LogViewModel SelectedLog {
            get { return selectedLog; }
            set {
                selectedLog = value;
                SetNamespaceHighlight(selectedLog);
                OnPropertyChanged(nameof(SelectedLog));
            }
        }

        private List<LogViewModel> LogsToInsert { get; set; }

        public OrderedObservableCollection Logs { get; set; }
        public ObservableCollection<NamespaceViewModel> Namespaces { get; set; }
        public ObservableCollection<ApplicationViewModel> Applications { get; set; }

        private NamespaceViewModel _selectedNamespaceViewModel;
        public NamespaceViewModel SelectedNamespaceViewModel {
            get { return _selectedNamespaceViewModel; }
            set {
                if (_selectedNamespaceViewModel != null) {
                    _selectedNamespaceViewModel.IsHighlighted = false;
                }
                _selectedNamespaceViewModel = value;
                if (_selectedNamespaceViewModel != null) {
                    _selectedNamespaceViewModel.IsHighlighted = true;
                }
                OnPropertyChanged(nameof(SelectedNamespaceViewModel));
            }
        }

        public LoginatorViewModel(IApplicationConfiguration applicationConfiguration, IConfigurationDao configurationDao) {
            ApplicationConfiguration = applicationConfiguration;
            ConfigurationDao = configurationDao;
            IsActive = true;
            NumberOfLogsPerLevel = Constants.DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL;
            Logger = LogManager.GetCurrentClassLogger();
            Logs = new OrderedObservableCollection();
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
                lock (ViewModelConstants.SYNC_OBJECT) {
                    var logsToInsert = LogsToInsert.OrderBy(m => m.Timestamp);

                    // 1. Add missing applications using incoming logs
                    sw.Start();
                    UpdateApplications(logsToInsert);
                    sw.Stop();
                    if (ApplicationConfiguration.IsTimingTraceEnabled) {
                        Logger.Trace("[UpdateApplications] " + sw.ElapsedMilliseconds + "ms");
                    }
                    // 2. Add missing namespaces using incoming logs
                    sw.Restart();
                    UpdateNamespaces(logsToInsert);
                    sw.Stop();
                    if (ApplicationConfiguration.IsTimingTraceEnabled) {
                        Logger.Trace("[UpdateNamespaces] " + sw.ElapsedMilliseconds + "ms");
                    }

                    sw.Restart();
                    AddLogs(logsToInsert);
                    sw.Stop();
                    if (ApplicationConfiguration.IsTimingTraceEnabled) {
                        Logger.Trace("[UpdateLogs] " + sw.ElapsedMilliseconds + "ms");
                    }

                    LogsToInsert.Clear();
                    
                    Timer.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
                }
            });
        }

        private LogViewModel ToLogViewModel(Log log) {
            return Mapper.Map<Log, LogViewModel>(log);
        }

        private void Receiver_LogReceived(object sender, LogReceivedEventArgs e) {
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                lock (ViewModelConstants.SYNC_OBJECT) {
                    // Add a log entry only to the list if global logging is active (checkbox)
                    if (!IsActive) {
                        return;
                    }
                    LogViewModel log = ToLogViewModel(e.Log);
                    LogsToInsert.Add(log);
                }
            });
        }

        private void AddLogs(IEnumerable<LogViewModel> logsToInsert) {
            try {
                foreach (var logToInsert in logsToInsert) {
                    var application = Applications.FirstOrDefault(m => m.Name == logToInsert.Application);
                    if (application == null) {
                        Logger.Error("[AddLogs] The application has to be set at this point.");
                        return;
                    }
                    application.AddLog(logToInsert);
                }
            }
            catch (Exception e) {
                Console.WriteLine("Could not update logs: " + e);
            }
        }
        
        private void UpdateNamespaces(IEnumerable<LogViewModel> logsToInsert) {
            try {
                foreach (var log in logsToInsert) {
                    var application = Applications.FirstOrDefault(m => m.Name == log.Application);
                    if (application == null) {
                        Logger.Error("[UpdateNamespaces] The application has to be set at this point.");
                        return;
                    }
                    // Try to get existing root namespace with name of application
                    var nsApplication = Namespaces.FirstOrDefault(m => m.Name == log.Application);
                    if (nsApplication == null) {
                        nsApplication = new NamespaceViewModel(log.Application, application);
                        Namespaces.Add(nsApplication);
                    }

                    // Example: Verbosus.VerbTeX.View
                    string nsLogFull = log.Namespace;
                    // Example: Verbosus
                    string nsLogPart = nsLogFull.Split(new string[] { Constants.NAMESPACE_SPLITTER }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    // Try to get existing namespace with name Verbosus
                    var nsChild = nsApplication.Children.FirstOrDefault(m => m.Name == nsLogPart);
                    if (nsChild == null) {
                        nsChild = new NamespaceViewModel(nsLogPart, application);
                        nsChild.IsChecked = nsApplication.IsChecked;
                        nsApplication.Children.Add(nsChild);
                        nsChild.Parent = nsApplication;
                    }
                    if (nsLogFull.Contains(Constants.NAMESPACE_SPLITTER)) {
                        HandleNamespace(nsChild, nsLogFull.Substring(nsLogFull.IndexOf(Constants.NAMESPACE_SPLITTER) + 1), application, log);
                    } else {
                        SetLogCountByLevel(log, nsChild);
                    }
                }

            } catch (Exception e) {
                Console.WriteLine("Could not update namespaces: " + e);
            }
        }

        private void HandleNamespace(NamespaceViewModel parent, string suffix, ApplicationViewModel application, LogViewModel log) {
            // Example: VerbTeX.View (Verbosus was processed before)
            string nsLogFull = suffix;
            // Example: VerbTeX
            string nsLogPart = nsLogFull.Split(new string[] { Constants.NAMESPACE_SPLITTER }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            // Try to get existing namespace with name VerbTeX
            var nsChild = parent.Children.FirstOrDefault(m => m.Name == nsLogPart);
            if (nsChild == null) {
                nsChild = new NamespaceViewModel(nsLogPart, application);
                nsChild.IsChecked = parent.IsChecked;
                parent.Children.Add(nsChild);
                nsChild.Parent = parent;
            }
            if (suffix.Contains(Constants.NAMESPACE_SPLITTER)) {
                HandleNamespace(nsChild, suffix.Substring(suffix.IndexOf(Constants.NAMESPACE_SPLITTER) + 1), application, log);
            } else {
                SetLogCountByLevel(log, nsChild);
            }
        }

        private void UpdateApplications(IEnumerable<LogViewModel> logsToInsert) {
            try {
                foreach (var log in logsToInsert) {
                    var application = Applications.FirstOrDefault(m => m.Name == log.Application);
                    if (application == null) {
                        application = new ApplicationViewModel(log.Application, Logs, Namespaces);
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

        private void SetNamespaceHighlight(LogViewModel log) {
            if (selectedLog != null) {
                SelectedNamespaceViewModel = Namespaces.Flatten(x => x.Children).FirstOrDefault(model => model.Fullname.Equals(selectedLog.Application + Constants.NAMESPACE_SPLITTER + selectedLog.Namespace));
            }
        }

        private void ClearNamespaceHighlight() {
            Namespaces.Flatten(x => x.Children).ToList().ForEach(m => m.IsHighlighted = false);
        }

        private void SetLogCountByLevel(LogViewModel log, NamespaceViewModel ns) {
            ns.Count++;
            if (log.Level == LoggingLevel.TRACE) {
                ns.CountTrace++;
            } else if (log.Level == LoggingLevel.DEBUG) {
                ns.CountDebug++;
            } else if (log.Level == LoggingLevel.INFO) {
                ns.CountInfo++;
            } else if (log.Level == LoggingLevel.WARN) {
                ns.CountWarn++;
            } else if (log.Level == LoggingLevel.ERROR) {
                ns.CountError++;
            } else if (log.Level == LoggingLevel.FATAL) {
                ns.CountFatal++;
            }
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
                lock (ViewModelConstants.SYNC_OBJECT) {
                    Logs.Clear();
                    foreach (var application in Applications) {
                        application.ClearLogs();
                    }
                    foreach (var ns in Namespaces) {
                        ResetAllCount(ns);
                    }
                    ClearNamespaceHighlight();
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
                lock (ViewModelConstants.SYNC_OBJECT) {
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
                lock (ViewModelConstants.SYNC_OBJECT) {
                    numberOfLogsPerApplicationAndLevelInternal = NumberOfLogsPerLevel;
                    foreach (var application in Applications) {
                        application.UpdateMaxNumberOfLogs(numberOfLogsPerApplicationAndLevelInternal);
                    }
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
                lock (ViewModelConstants.SYNC_OBJECT) {
                    searchCriteriaInternal = SearchCriteria;
                    foreach (var application in Applications) {
                        application.UpdateSearchCriteria(searchCriteriaInternal, IsInverted);
                    }
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
