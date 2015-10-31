using Backend;
using Backend.Dao;
using Backend.Manager;
using Backend.Model;
using Common;
using Common.Configuration;
using GalaSoft.MvvmLight.Command;
using Loginator.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Loginator.ViewModels {

    public class ConfigurationViewModel : INotifyPropertyChanged {

        private IConfigurationDao ConfigurationDao { get; set; }

        private LogType logType;
        public LogType LogType {
            get { return logType; }
            set {
                logType = value;
                OnPropertyChanged(nameof(LogType));
            }
        }

        private string portChainsaw;
        public string PortChainsaw {
            get { return portChainsaw; }
            set {
                portChainsaw = value;
                OnPropertyChanged(nameof(PortChainsaw));
            }
        }

        private string portLogcat;
        public string PortLogcat {
            get { return portLogcat; }
            set {
                portLogcat = value;
                OnPropertyChanged(nameof(PortLogcat));
            }
        }

        public Action CloseAction { get; set; }

        public ConfigurationViewModel(IConfigurationDao configurationDao) {
            ConfigurationDao = configurationDao;
            Configuration configuration = ConfigurationDao.Read();
            LogType = configuration.LogType;
            PortChainsaw = configuration.PortChainsaw.ToString();
            PortLogcat = configuration.PortLogcat.ToString();
        }

        private ICommand cancelChangesCommand;
        public ICommand CancelChangesCommand {
            get {
                return cancelChangesCommand ?? (cancelChangesCommand = new RelayCommand<ConfigurationViewModel>(CancelChanges, CanCancelChanges));
            }
        }
        private bool CanCancelChanges(ConfigurationViewModel configuration) {
            return true;
        }
        private void CancelChanges(ConfigurationViewModel configuration) {
            try {
                CloseAction();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
            }
        }

        private ICommand acceptChangesCommand;
        public ICommand AcceptChangesCommand {
            get {
                return acceptChangesCommand ?? (acceptChangesCommand = new RelayCommand<ConfigurationViewModel>(AcceptChanges, CanAcceptChanges));
            }
        }
        private bool CanAcceptChanges(ConfigurationViewModel serverRule) {
            return true;
        }
        private void AcceptChanges(ConfigurationViewModel serverRule) {
            try {
                Configuration configuration = new Configuration() {
                    LogType = LogType,
                    PortChainsaw = Convert.ToInt32(PortChainsaw),
                    PortLogcat = Convert.ToInt32(PortLogcat)
                };
                ConfigurationDao.Write(configuration);
                IoC.Get<Receiver>().Initialize(configuration);
                CloseAction();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
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
