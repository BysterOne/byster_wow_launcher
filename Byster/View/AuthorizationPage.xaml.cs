using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Byster.View
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationPage.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void FogotPassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            App.WindowMain.Owner = App.WindowLogin.Owner;
            App.WindowLogin.Hide();
            App.WindowMain.Show();
            App.WindowLogin.Close();
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void txtLogin_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
