using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Backend.Model {

    public class Namespace : INotifyPropertyChanged {

        private bool isChecked;
        public bool IsChecked {
            get { return isChecked; }
            set {
                isChecked = value;
                if (isChecked && Parent != null) {
                    // Bubble up
                    Parent.IsChecked = true;
                }
                if (!isChecked) {
                    // Bubble down
                    foreach (var child in Children) {
                        child.IsChecked = false;
                    }
                }
                OnPropertyChanged("IsChecked");
            }
        }

        private bool isExpanded;
        public bool IsExpanded {
            get {
                return isExpanded;
            }
            set {
                isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        private int count;
        public int Count {
            get {
                return count;
            }
            set {
                count = value;
                OnPropertyChanged("Count");
            }
        }

        //private bool isSelected;
        //public bool IsSelected {
        //    get {
        //        return isSelected;
        //    }
        //    set {
        //        isSelected = value;
        //        OnPropertyChanged("IsSelected");
        //    }
        //}

        public string Name { get; set; }
        public Namespace Parent { get; set; }
        public IList<Namespace> Children { get; set; }

        public Namespace(string name) {
            IsChecked = true;
            IsExpanded = true;
            Name = name;
            Children = new List<Namespace>();
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            Namespace other = (Namespace)obj;
            return Name == other.Name;
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
