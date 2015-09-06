using Backend.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogApplication.ViewModels {

    public class ApplicationViewModel : INotifyPropertyChanged {
        
        public string Name { get; set; }
        public IList<LogLevel> LogLevels { get; set; }

        private string selectedMinLogLevel;
        public string SelectedMinLogLevel {
            get {
                return selectedMinLogLevel;
            }
            set {
                selectedMinLogLevel = value;
                OnPropertyChanged("SelectedMinLogLevel");
            }
        }

        private bool isActive;
        public bool IsActive {
            get {
                return isActive;
            }
            set {
                isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        public ApplicationViewModel(string name) {
            Name = name;
            IsActive = true;
            LogLevels = new List<LogLevel>();
            LogLevels.Add(new LogLevel(LogLevel.TRACE));
            LogLevels.Add(new LogLevel(LogLevel.DEBUG));
            LogLevels.Add(new LogLevel(LogLevel.INFO));
            LogLevels.Add(new LogLevel(LogLevel.WARN));
            LogLevels.Add(new LogLevel(LogLevel.ERROR));
            LogLevels.Add(new LogLevel(LogLevel.FATAL));
            SelectedMinLogLevel = LogLevels.ElementAt(2).Id;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
