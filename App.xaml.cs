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
                o.Dsn = "https://6c3217f76f7106e660083d117fb16c21@o4509390529822720.ingest.us.sentry.io/4509390537490432";                
                o.Debug = true;
                o.EnableTracing = true;
                o.TracesSampleRate = 1.0;                
            });
        }


        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);
            e.Handled = true;
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SentrySdk.CaptureMessage($"App launched");
            var loader = new Loader();
            WindowAnimations.ApplyFadeAnimations(ref loader);
            loader.Show();
        }
    }

}
