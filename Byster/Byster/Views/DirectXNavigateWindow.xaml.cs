using Byster.Localizations.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для DialogWindow.xaml
    /// </summary>
    public partial class DirectXNavigateWindow : Window
    {
        private string navUri = "https://s3.byster.ru/public/directx.7z";
        public DirectXNavigateWindow(string title, string infoText)
        {
            InitializeComponent();
            this.Title = title;
            titleTextBlock.Text = title;
            infoTextBlock.Text = infoText;
            okBtn.Content = Localizator.GetLocalizationResourceByKey("Download").Value;
            cancelBtn.Content = Localizator.GetLocalizationResourceByKey("Cancel").Value;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(navUri);
            this.DialogResult = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
