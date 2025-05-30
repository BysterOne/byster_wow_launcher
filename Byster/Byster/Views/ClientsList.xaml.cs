using Byster.Models.BysterModels.Settings;
using Byster.Views.ModelsTemp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Byster.Views
{
    namespace ModelsTemp
    {
        public enum ClientIcon 
        {
            Wow,
            Wizard,
            Warrior,
            Warlock
        }

        public class ClientModel
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; }
            public string Path { get; set; }
            public ClientIcon Icon { get; set; }

            public ClientModel(string name, string path, ClientIcon icon)
            {
                Name = name; 
                Path = path;    
                Icon = icon;
            }
        }
    }
    /// <summary>
    /// Логика взаимодействия для ClientsList.xaml
    /// </summary>
    public partial class ClientsList : UserControl
    {
        public ClientsList()
        {
            InitializeComponent();

            Settings.Load();
        }

        #region События
        public event EventHandler<ClientModel> OnClientSelected;
        #endregion

        #region Переменные
        public ClientModel Selected { get; set; }
        public ClientsListItem SelectedClientItem { get; set; }
        public ObservableCollection<ClientModel> Clients { get; set; } = Settings.I.Clients;
        #endregion

        #region Обработчики событий
        private void FRemoveClient(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить данный объект?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result is MessageBoxResult.Yes)
            {
                try
                {
                    Settings.I.Clients.Remove((sender as ClientsListItem).Client);
                    Settings.Load();
                }
                catch { MessageBox.Show("Не удалось удалить. Попробуйте снова", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }
        private void FSelectClient(object sender, EventArgs e)
        {
            if (SelectedClientItem != null) SelectedClientItem.IsSelected = false;

            var newSelected = sender as ClientsListItem;
            newSelected.IsSelected = true;
            SelectedClientItem = newSelected;
            Selected = SelectedClientItem.Client;

            OnClientSelected?.Invoke(this, Selected);
        }
        private void FAddButtonClick(object sender, RoutedEventArgs e)
        {
            var n = new AddNewClient();
            n.ShowDialog();
        }
        #endregion
    }
}
