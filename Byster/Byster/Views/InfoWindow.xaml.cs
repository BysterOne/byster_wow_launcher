using Byster.Localizations.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class InfoWindow : Window
    {
        public static void ShowWindow(string title, string infoText)
        {
            new InfoWindow(title, infoText).Show();
        }

        public InfoWindow(string title, string infoText)
        {
            InitializeComponent();
            this.Title = title;
            titleTextBlock.Text = title;
            infoTextBlock.Text = infoText;
            okBtn.Content = Localizator.GetLocalizationResourceByKey("OK").Value;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = true;
            }
            finally
            {
                this.Close();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
