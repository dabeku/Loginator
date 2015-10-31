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
        public LoggingLevel Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Namespace { get; set; }
        public string Application { get; set; }
        public string Thread { get; set; }
        public string Context { get; set; }
        
        public LogViewModel() { }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public override string ToString() {
            // TODO: Localize this with .resx
            StringBuilder sb = new StringBuilder();
            sb.Append("Application: ");
            sb.AppendLine(Application);
            sb.Append("Namespace: ");
            sb.AppendLine(Namespace);
            if (!String.IsNullOrEmpty(Context)) {
                sb.Append("Context: ");
                sb.AppendLine(Context);
            }
            if (!String.IsNullOrEmpty(Thread)) {
                sb.Append("Thread: ");
                sb.AppendLine(Thread);
            }
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
