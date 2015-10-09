using LogApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogApplication.Collections {
    public class OrderedObservableCollection : ObservableCollection<LogViewModel> {
        public void AddOrdered(LogViewModel toAdd) {
            if (toAdd == null) {
                throw new ArgumentNullException("The paramater must be set.");
            }
            if (Items.Contains(toAdd)) {
                return;
            }
            int i = 0;
            for (; i < Items.Count; i++) {
                if (Items.ElementAt(i).Timestamp < toAdd.Timestamp) {
                    break;
                }
            }
            Items.Insert(i, toAdd);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
