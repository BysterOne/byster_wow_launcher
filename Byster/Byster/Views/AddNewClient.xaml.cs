using Byster.Models.BysterModels.Settings;
using Byster.Models.ViewModels;
using Byster.Views.ModelsTemp;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    namespace ModelsTemp
    {
        public class IconSelect
        {
            public ClientIcon Icon { get; set; }
            public ImageSource Source { get; set; }
            public bool IsSelected { get; set; }
        }
    }
    /// <summary>
    /// Логика взаимодействия для AddNewClient.xaml
    /// </summary>
    public partial class AddNewClient : Window
    {
        public AddNewClient()
        {
            InitializeComponent();

            Loaded += FLoaded;
        }

        private string SelectedFile = null;
        private ObservableCollection<IconSelect> Icons
        {
            get => (ObservableCollection<IconSelect>)GetValue(IconsProperty);
            set => SetValue(IconsProperty, value);
        }

        public static readonly DependencyProperty IconsProperty =
            DependencyProperty.Register(nameof(Icons), typeof(ObservableCollection<IconSelect>), typeof(AddNewClient),
                new PropertyMetadata(new ObservableCollection<IconSelect>()));

        private void FChooseProgramButton(object sender, MouseButtonEventArgs e)
        {
            var fDialog = new OpenFileDialog
            {
                Filter = "Executable (*.exe)|*.exe",
                Title = "Выберите файл",
                Multiselect = false
            };

            if (fDialog.ShowDialog() == true)
            {
                var fileName = fDialog.FileName;
                SelectedFile = fileName;
                pathToFile.Text = fileName;
            }
        }

        #region Обработчики событий
        private void closeButton_Click(object sender, RoutedEventArgs e) => Close();
        private void FLoaded(object sender, RoutedEventArgs e)
        {
            Icons.Clear();
            foreach (ClientIcon item in Enum.GetValues(typeof(ClientIcon)))
            {
                Icons.Add(new IconSelect() { Icon = item, Source = ClientImageSourceConverter.IconTypeToImageSource(item) });
            }
            Icons[0].IsSelected = true;
        }
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = serverName.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
            {
                Error("Укажите название сервера");
                return;
            }

            if (String.IsNullOrWhiteSpace(SelectedFile))
            {
                Error("Выберите .exe файл");
                return;
            }

            var icon = Icons.FirstOrDefault(x => x.IsSelected);
            if (icon == null)
            {
                Error("Выберите иконку");
                return;
            }

            Settings.I.Clients.Add(new ClientModel(name, SelectedFile, icon.Icon));
            Settings.I.Save();

            Close();
        }
        private void ItemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SizeToContent = SizeToContent.Height;
        }
        #endregion

        #region Функции
        private void Error(string error)
        {
            MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        
    }
}
