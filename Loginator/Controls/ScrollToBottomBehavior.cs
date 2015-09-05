using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using LogApplication.ViewModels;

namespace Loginator.Controls {

    public class ScrollToBottomBehavior : Behavior<ListView> {

        protected override void OnAttached() {
            base.OnAttached();
            var collection = AssociatedObject.Items.SourceCollection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += collection_CollectionChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            var collection = AssociatedObject.Items.SourceCollection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged -= collection_CollectionChanged;
        }

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            //if (e.Action == NotifyCollectionChangedAction.Add) {
                ScrollToLastItem();
            //}
        }

        private void ScrollToLastItem() {
            lock (this) {
                if (!LoginatorViewModel.Instance.IsScrollingToBottom) {
                    return;
                }

                int count = AssociatedObject.Items.Count;
                if (count > 0) {
                    var last = AssociatedObject.Items[count - 1];
                    AssociatedObject.ScrollIntoView(last);
                }
            }
        }
    }
}
