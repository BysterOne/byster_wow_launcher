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
using System.Collections.ObjectModel;
using Byster.Models.LocalRotationModels;
using System.IO;
using Newtonsoft.Json;
using Byster.Models.ViewModels;
using Microsoft.Win32;
using Byster.Models.BysterModels;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для RotationSettingsWindow.xaml
    /// </summary>
    public partial class RotationSettingsWindow : Window
    {
        public RotationSettingsWindow(MainWindowViewModel model)
        {
            InitializeComponent();
            this.DataContext = model;
            model.DeveloperRotations.PreAddRotationAction = () =>
            {
                var window = new AddRotationWindow(model);
                return window.ShowDialog() ?? false;
            };
            model.DeveloperRotations.AddRotationSuccess = () =>
            {
                var window = new InfoWindow("Успех", "Создание ротации успешно");
                window.ShowDialog();
            };
            model.DeveloperRotations.AddRotationFail = () =>
            {
                var window = new InfoWindow("Ошибка", "Ошибка при создании ротации\n" + model.LastError);
                window.ShowDialog();
            };
            model.DeveloperRotations.CloseDel = () =>
            {
                Dispatcher.Invoke(() =>
                {
                    Close();
                });
            };
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

    }
}
