using Backend;
using Backend.Model;
using Common.Configuration;
using GalaSoft.MvvmLight.Threading;
using LogApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private const string TEMPLATE_APP_NAME = "Loginator v{0}";
        private const string STRING_EQUALS = "=";
        private const string VERSION_CODE = "versionCode";
        private const string VERSION_NAME = "versionName";

        public MainWindow() {
            InitializeComponent();
            SetTitleVersionFromFile();
            LoginatorViewModel vm = DataContext as LoginatorViewModel;
            if (vm != null) {
                try {
                    vm.StartListener();
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                }
            }
        }

        private void SetTitleVersionFromFile() {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Loginator.Version.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    string[] splitted = reader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var line in splitted) {
                        string[] splittedLine = line.Split(new string[] { STRING_EQUALS }, StringSplitOptions.None);
                        if (splittedLine[0].Trim() == VERSION_NAME) {
                            Title = String.Format(TEMPLATE_APP_NAME, splittedLine[1].Trim());
                            break;
                        }
                    }
                }
            }
        }
    }
}
