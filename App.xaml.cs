using Launcher.Any;
using Launcher.Windows;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            SentrySdk.Init(o =>
            {
                o.Dsn = "https://cd2f7190be9b3e31d3014ededfda7614@sentry.byster.one/19";                
                o.TracesSampleRate = 1.0;
                o.ProfilesSampleRate = 1.0;
                o.IsGlobalModeEnabled = true;
                o.SendDefaultPii = true;
                o.CaptureFailedRequests = true;
                o.AddProfilingIntegration();
            });

            SentryExtensions.FirstLoadTransaction = SentrySdk.StartTransaction("first-load", "app-creation");
            SentrySdk.ConfigureScope(scope => scope.Transaction = SentryExtensions.FirstLoadTransaction);
        }


        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loader = new Loader();
            WindowAnimations.ApplyFadeAnimations(ref loader);
            loader.Show();
        }
    }

}
