using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Model {

    public class LogViewModel : INotifyPropertyChanged {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Namespace { get; set; }
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
            IsVisible = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
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
