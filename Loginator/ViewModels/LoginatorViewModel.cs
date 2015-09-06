﻿using System;
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

        private bool isScrollingToBottom;
        public bool IsScrollingToBottom {
            get { return isScrollingToBottom; }
            set {
                isScrollingToBottom = value;
                OnPropertyChanged("IsScrollingToBottom");
            }
        }

        private int numberOfLogsPerLevelInternal;

        private int numberOfLogsPerLevel;
        public int NumberOfLogsPerLevel {
            get { return numberOfLogsPerLevel; }
            set {
                numberOfLogsPerLevel = value;
                OnPropertyChanged("IsScrollingToBottom");
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

        public event EventHandler<EventArgs> Updated;

        private LoginatorViewModel() {
            IsActive = true;
            IsScrollingToBottom = true;
            NumberOfLogsPerLevel = DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL;
            numberOfLogsPerLevelInternal = DEFAULT_MAX_NUMBER_OF_LOGS_PER_LEVEL;
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
                    //if (Updated != null) {
                    //    Updated(this, null);
                    //}
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
                    if (!IsActive) {
                        return;
                    }
                    LogsToInsert.Add(ToLogViewModel(e.Log));
                    //Logs.Insert(0, ToLogViewModel(e.Log));
                    //UpdateLogs();
                }
            });
        }

        private void UpdateLogs() {

            var logsToInsert = LogsToInsert.OrderBy(m => m.Timestamp);
            Logs.AddRangeAtStart(logsToInsert);
            LogsToInsert.Clear();
            
            var levels = new List<string>(Logs.Select(m => m.Level).Distinct());
            foreach (var level in levels) {
                var logsByLevel = Logs.Where(m => m.Level == level);
                int logCountByLevel = logsByLevel.Count();
                while (logCountByLevel > numberOfLogsPerLevelInternal) {
                    Logs.Remove(logsByLevel.ElementAt(logsByLevel.Count() - 1));
                    //Logs.Remove(logsByLevel.ElementAt(0));
                    logsByLevel = Logs.Where(m => m.Level == level);
                    logCountByLevel = logsByLevel.Count();
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
            foreach (var child in ns.Children) {
                ResetAllCount(child);
            }
        }

        private void HandleLogVisibilityByNamespace(LogViewModel log, NamespaceViewModel ns, string currentNamespace) {
            foreach (var child in ns.Children) {
                string nsAbsolute = currentNamespace + Constants.NAMESPACE_SPLITTER + child.Name;
                if (log.Namespace == nsAbsolute) {
                    child.Count++;
                    // Only active if namespace AND application are active
                    if (child.IsChecked) {
                        var application = Applications.FirstOrDefault(m => m.Name == log.Application);
                        if (application != null && application.IsActive && LogLevel.IsLogLevelAboveMin(log.Level, application.SelectedMinLogLevel)) {
                            log.IsVisible = true;
                        } else {
                            log.IsVisible = false;
                        }
                    }
                    else {
                        log.IsVisible = false;
                    }
                }
                else {
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

        private ICommand clearLogCommand;
        public ICommand ClearLogCommand {
            get {
                if (clearLogCommand == null) {
                    clearLogCommand = new DelegateCommand(HandleClearLogCommand);
                }
                return clearLogCommand;
            }
        }
        public void HandleClearLogCommand(object one) {
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
                    numberOfLogsPerLevelInternal = NumberOfLogsPerLevel;
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
