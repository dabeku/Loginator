using Backend.Model;
using GalaSoft.MvvmLight.Threading;
using LogApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Loginator {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            DispatcherHelper.Initialize();
            DataContext = LoginatorViewModel.Instance;
            //LoginatorViewModel.Instance.Updated += Updated;
        }

        //private void Updated(object sender, EventArgs e) {
        //    ListBoxAutomationPeer svAutomation = (ListBoxAutomationPeer)ScrollViewerAutomationPeer.CreatePeerForElement(lvLogs);
        //    IScrollProvider scrollInterface = (IScrollProvider)svAutomation.GetPattern(PatternInterface.Scroll);
        //    System.Windows.Automation.ScrollAmount scrollVertical = System.Windows.Automation.ScrollAmount.LargeIncrement;
        //    System.Windows.Automation.ScrollAmount scrollHorizontal = System.Windows.Automation.ScrollAmount.NoAmount;
        //    //If the vertical scroller is not available, the operation cannot be performed, which will raise an exception. 
        //    if (scrollInterface.VerticallyScrollable)
        //        scrollInterface.Scroll(scrollHorizontal, scrollVertical);
        //}
    }
}
