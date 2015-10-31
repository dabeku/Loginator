using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Common;
using Loginator.ViewModels;

namespace LogApplication.ViewModels {

    public class NamespaceViewModel : INotifyPropertyChanged {

        private bool isChecked;
        public bool IsChecked {
            get { return isChecked; }
            set {
                isChecked = value;
                lock (ViewModelConstants.SYNC_OBJECT) {
                    if (ApplicationViewModel != null) {
                        ApplicationViewModel.UpdateByNamespaceChange(this);
                    }
                }
                
                if (Children != null) {
                    foreach (var child in Children) {
                        child.IsChecked = isChecked;
                    }
                }
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        private bool isExpanded;
        public bool IsExpanded {
            get {
                return isExpanded;
            }
            set {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        private int count;
        public int Count {
            get {
                return count;
            }
            set {
                count = value;
                OnPropertyChanged(nameof(Count));
            }
        }

        private int countTrace;
        public int CountTrace {
            get {
                return countTrace;
            }
            set {
                countTrace = value;
                OnPropertyChanged(nameof(CountTrace));
            }
        }

        private int countDebug;
        public int CountDebug {
            get {
                return countDebug;
            }
            set {
                countDebug = value;
                OnPropertyChanged(nameof(CountDebug));
            }
        }

        private int countInfo;
        public int CountInfo {
            get {
                return countInfo;
            }
            set {
                countInfo = value;
                OnPropertyChanged(nameof(CountInfo));
            }
        }

        private int countWarn;
        public int CountWarn {
            get {
                return countWarn;
            }
            set {
                countWarn = value;
                OnPropertyChanged(nameof(CountWarn));
            }
        }

        private int countError;
        public int CountError {
            get {
                return countError;
            }
            set {
                countError = value;
                OnPropertyChanged(nameof(CountError));
            }
        }

        private int countFatal;

        public int CountFatal {
            get {
                return countFatal;
            }
            set {
                countFatal = value;
                OnPropertyChanged(nameof(CountFatal));
            }
        }

        private bool isHighlighted;
        public bool IsHighlighted
        {
            get
            {
                return isHighlighted;
            }
            set
            {
                isHighlighted = value;
                OnPropertyChanged(nameof(IsHighlighted));
            }
        }

        public string Name { get; set; }
        public NamespaceViewModel Parent { get; set; }
        public ObservableCollection<NamespaceViewModel> Children { get; set; }

        private ApplicationViewModel ApplicationViewModel { get; set; }

        public NamespaceViewModel(string name, ApplicationViewModel applicationViewModel) {
            IsChecked = true;
            IsExpanded = true;
            Name = name;
            Children = new ObservableCollection<NamespaceViewModel>();
            ApplicationViewModel = applicationViewModel;
        }

        public string Fullname {
            get {
                string fullname = Name;
                var parent = Parent;
                while (parent != null) {
                    fullname = parent.Name + Constants.NAMESPACE_SPLITTER + fullname;
                    parent = parent.Parent;
                }
                return fullname;
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
