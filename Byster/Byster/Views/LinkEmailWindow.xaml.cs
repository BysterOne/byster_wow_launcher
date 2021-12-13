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
using System.Windows.Shapes;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LinkEmailWindow.xaml
    /// </summary>
    public partial class LinkEmailWindow : Window
    {
        private SettingsViewModel Model { get; set; } = null;

        public LinkEmailWindow(string title, string infoText, SettingsViewModel model)
        {
            InitializeComponent();
            this.Title = title;
            infoTextBlock.Text = infoText;
            titleTextBlock.Text = title;
            Model = model;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            string email = emailTextBox.Text;
            if (string.IsNullOrEmpty(email)) return;
            if ((!(email.ToLower().EndsWith(".ru") ||
                email.ToLower().EndsWith(".com") ||
                email.ToLower().EndsWith(".net"))) ||
                    (email.Count(c => c == '@') != 1))
            {
                var w = new InfoWindow("Ошибка", "Введённый текст не является E-Mail");
                w.ShowDialog();
                return;
            }
            bool result = Model.MainViewModel.UserInfo.LinkEmail(email);
            if(result)
            {
                var w = new InfoWindow("Успех", "E-Mail привязан");
                w.ShowDialog();
                this.DialogResult = true;
            }
            else
            {
                var w = new InfoWindow("Ошибка", "Ошибка привязки E-Mail");
                w.ShowDialog();
            }

        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
