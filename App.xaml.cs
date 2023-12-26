using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Concursus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;
        public const string APP_VERSION = "3.1.0";
        public const string APP_UPDATE_ENDPOINT = "https://api.github.com/repos/MadMax1960/Concursus/releases";
        public const string OLD_FOLDER = "old";
        protected override void OnStartup(StartupEventArgs e)
        {

            if (System.IO.Directory.Exists(OLD_FOLDER))
                System.IO.Directory.Delete(OLD_FOLDER, true);

            Utils.RegisterProtocol(Utils.MM_PROTOCOL, "Concursus");
            if (e.Args.Length == 1)
                this.StartupUri = new Uri("/Concursus;component/GBModPrompt.xaml", UriKind.Relative);
            else
            {
                const string appName = "Concursus";
                bool createdNew;

                _mutex = new Mutex(true, appName, out createdNew);

                if (!createdNew)
                {
                    //app is already running! Exiting the application
                    Application.Current.Shutdown();
                }
            }
            base.OnStartup(e);
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (!(e is System.Windows.Markup.XamlParseException))
                e.Handled = true;
            Environment.Exit(-1);
        }
    }
}
