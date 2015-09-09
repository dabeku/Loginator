using Common.Configuration;
using Common.Exceptions;
using GalaSoft.MvvmLight.Threading;
using Loginator.Bootstrapper;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Loginator {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private ILogger Logger { get; set; }

        private Exception GetInnerException(Exception exception) {
            if (exception.InnerException == null) {
                return exception;
            }
            return GetInnerException(exception.InnerException);
        }

        private void HandleException(Exception exception) {
            if (exception is LoginatorException) {
                MessageBox.Show(exception.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Stop,
                MessageBoxResult.OK);
            }
            else {
                MessageBox.Show(exception.ToString(),
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Stop,
                MessageBoxResult.OK);
            }
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            Logger = LogManager.GetCurrentClassLogger();
            try {
                // Exception handlers
                DispatcherUnhandledException += (m, n) => {
                    Exception exception = n.Exception;
                    Exception innerException = GetInnerException(exception);
                    Logger.Error(exception, "[OnStartup] An unhandled dispatcher exception occurred.");
                    HandleException(innerException);
                    n.Handled = true;
                    Current.Shutdown();
                };
                AppDomain.CurrentDomain.UnhandledException += (m, n) => {
                    Exception exception = n.ExceptionObject as Exception;
                    if (exception == null) {
                        Logger.Fatal("[OnStartup] Unknow error killed application");
                    } else {
                        Logger.Fatal(exception, "[OnStartup] An unhandled exception occurred and the application is terminating");
                    }
                    HandleException(exception);
                };
                // Initialize dispatcher helper so we can access UI thread in view model
                DispatcherHelper.Initialize();
                // Bootstrapping
                DiBootstrapperFrontend.Initialize(IoC.Container);
                Logger.Info("[OnStartup] Application successfully started");
            } catch (Exception exception) {
                Logger.Fatal(exception, "[OnStartup] Error during starting Application");
                HandleException(exception);
                Current.Shutdown();
            }
        }
    }
}
