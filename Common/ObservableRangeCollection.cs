using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common {
    public class ObservableRangeCollection<T> : ObservableCollection<T> {
        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public void AddRangeAtStart(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            foreach (var i in collection) {
                Items.Insert(0, i);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
