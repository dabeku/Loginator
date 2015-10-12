using Backend;
using Backend.Model;
using Common;
using Common.Configuration;
using GalaSoft.MvvmLight.Threading;
using LogApplication.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        private const string FILE_VERSION = "Loginator.Version.txt";
        private const string VERSION_URL = "https://raw.githubusercontent.com/dabeku/Loginator/master/Loginator/Version.txt";
        private const string DOWNLOAD_URL = "https://github.com/dabeku/Loginator/releases";

        private ILogger Logger { get; set; }

        private int Version { get; set; }

        public MainWindow() {
            Logger = LogManager.GetCurrentClassLogger();

            InitializeComponent();
            SetTitleVersionFromFile();
            CheckForNewVersion();
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
            using (Stream stream = assembly.GetManifestResourceStream(FILE_VERSION)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    string text = reader.ReadToEnd();
                    Title = GetVersionName(text);
                    Version = GetVersionCode(text);
                }
            }
        }

        private int GetVersionCode(string text) {
            string[] splitted = text.Split(new string[] { Environment.NewLine, Constants.STRING_NEWLINE }, StringSplitOptions.None);
            foreach (var line in splitted) {
                string[] splittedLine = line.Split(new string[] { STRING_EQUALS }, StringSplitOptions.None);
                if (splittedLine[0].Trim() == VERSION_CODE) {
                    return Convert.ToInt32(splittedLine[1].Trim());
                }
            }
            return 1;
        }

        private string GetVersionName(string text) {
            string[] splitted = text.Split(new string[] { Environment.NewLine, Constants.STRING_NEWLINE }, StringSplitOptions.None);
            foreach (var line in splitted) {
                string[] splittedLine = line.Split(new string[] { STRING_EQUALS }, StringSplitOptions.None);
                if (splittedLine[0].Trim() == VERSION_NAME) {
                    return String.Format(TEMPLATE_APP_NAME, splittedLine[1].Trim());
                }
            }
            return String.Empty;
        }

        private void CheckForNewVersion() {
            try {
                using (var webClient = new WebClient()) {
                    string text = webClient.DownloadString(VERSION_URL);
                    int latestVersion = GetVersionCode(text);
                    if (Version < latestVersion) {
                        Logger.Info("New version available. Current: '{0}'. Latest: '{1}'", Version, latestVersion);
                        MessageBoxResult messageBoxResult = MessageBox.Show(L10n.Language.NewVersionAvailable, L10n.Language.UpdateAvailable, MessageBoxButton.YesNo);
                        if (messageBoxResult == MessageBoxResult.Yes) {
                            Process.Start(DOWNLOAD_URL);
                        }
                    } else {
                        Logger.Info("No new version available. Current: '{0}'", Version);
                    }
                }
            } catch (Exception e) {
                Logger.Error(e, "Could not check for new version");
            }
        }
    }
}
