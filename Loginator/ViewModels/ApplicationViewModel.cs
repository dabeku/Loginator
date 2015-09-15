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
        public IList<LoggingLevel> LogLevels { get; set; }

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
            LogLevels = new List<LoggingLevel>();
            LogLevels.Add(new LoggingLevel(LoggingLevel.TRACE));
            LogLevels.Add(new LoggingLevel(LoggingLevel.DEBUG));
            LogLevels.Add(new LoggingLevel(LoggingLevel.INFO));
            LogLevels.Add(new LoggingLevel(LoggingLevel.WARN));
            LogLevels.Add(new LoggingLevel(LoggingLevel.ERROR));
            LogLevels.Add(new LoggingLevel(LoggingLevel.FATAL));
            SelectedMinLogLevel = LogLevels.ElementAt(0).Id;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
