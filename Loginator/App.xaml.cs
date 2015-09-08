using Common.Exceptions;
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

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Logger = LogManager.GetCurrentClassLogger();

            try {

                DispatcherUnhandledException += (m, n) => {
                    Exception exception = n.Exception;
                    Exception innerException = GetInnerException(exception);
                    Logger.Error(exception, "[OnStartup] An unhandled dispatcher exception occurred.");
                    if (innerException is LoginatorException) {
                        var ex = innerException as LoginatorException;
                        MessageBox.Show(ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Stop,
                        MessageBoxResult.OK);
                    } else {
                        MessageBox.Show(n.Exception.ToString(),
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Stop,
                        MessageBoxResult.OK);
                    }

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

                    if (exception is LoginatorException) {
                        MessageBox.Show(exception.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Stop,
                        MessageBoxResult.OK);
                    } else { 
                        MessageBox.Show(exception.ToString(),
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Stop,
                        MessageBoxResult.OK);
                    }
                };

                Logger.Info("[OnStartup] Application successfully started");
            } catch (Exception exception) {
                Logger.Fatal(exception, "[OnStartup] Error during starting Application");

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

                Current.Shutdown();
            }
        }
    }
}
