using Backend.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogApplication.ViewModels {

    public class LogViewModel : INotifyPropertyChanged {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Namespace { get; set; }
        public string Application { get; set; }
        // TODO: Move this column to details textbox in xaml
        public string Thread { get; set; }
        public IEnumerable<Property> Properties { get; set; }

        private bool isVisible;
        public bool IsVisible {
            get {
                return isVisible;
            }
            set {
                isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        public LogViewModel() {
            Properties = new List<Property>();
            IsVisible = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("Application: ");
            sb.AppendLine(Application);
            sb.Append("Namespace: ");
            sb.AppendLine(Namespace);
            sb.Append("Message: ");
            sb.AppendLine(Message);
            if (!String.IsNullOrEmpty(Exception)) {
                sb.Append("Exception: ");
                sb.AppendLine(Exception);
            }
            return sb.ToString();
        }
    }
}
