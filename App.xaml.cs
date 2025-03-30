using Launcher.Windows;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loader = new Loader();
            WindowAnimations.ApplyFadeAnimations(ref loader);
            loader.Show();
        }
    }

}
