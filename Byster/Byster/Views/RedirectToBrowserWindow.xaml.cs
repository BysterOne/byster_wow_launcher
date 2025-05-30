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
    public partial class RedirectToBrowserWindow : Window
    {
        private string navUri = "https:/public.byster.one/directx.7z";
        public RedirectToBrowserWindow(string title, string infoText, string uri)
        {
            InitializeComponent();
            this.Title = title;
            navUri = uri;
            titleTextBlock.Text = title;
            infoTextBlock.Text = infoText;
            okBtn.Content = Localizator.GetLocalizationResourceByKey("Open").Value;
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
