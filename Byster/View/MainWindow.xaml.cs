using Byster.Models;
using Byster.RestModels;
using RestSharp;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Byster.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<Process, WoWProcess> Wows { get; set; } = new Dictionary<Process, WoWProcess>();

        public MainWindow()
        {
            InitializeComponent();
            var characters = new List<Character>
            {
                new Character
                {
                    Name = "Druid",
                    Icon = "https://i.imgur.com/4gkqjvq.jpg"
                },
                new Character
                {
                    Name = "Druid",
                    Icon = "https://i.imgur.com/4gkqjvq.jpg"
                }
            };
            charactersList.ItemsSource = characters;


            var rotations = new List<Rotation>
            {
                new Rotation
                {
                    IsInject = true,
                    Name = "Авто Хил",
                    ExpireDate = DateTime.Now,
                    Specialization = "Исцеление",
                    Type = "PvE"
                },
                new Rotation
                {
                    IsInject = false,
                    Name = "Авто Хил2",
                    ExpireDate = DateTime.Now,
                    Specialization = "Исцеление2",
                    Type = "PvP"
                },
                new Rotation
                {
                    IsInject = false,
                    Name = "Авто Хилa3",
                    ExpireDate = DateTime.Now,
                    Specialization = "Исцеление3",
                    Type = "Utilitty"
                },
                new Rotation
                {
                    IsInject = true,
                    Name = "Авто Хил4",
                    ExpireDate = DateTime.Now,
                    Specialization = "Исцеление4",
                    Type = "Bots"
                },
            };
            tableRotations.ItemsSource = rotations;
        }

        class Rotation
        {
            public bool IsInject { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Specialization { get; set; }
            public DateTime ExpireDate { get; set; }
        }

        class Character
        {
            public string Name { get; set; }

            public string Icon { get; set; }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void NewWoW_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("new wow");
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLevel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnYoutube_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnVK_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddMoney_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBalance_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDiscord_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            if (contentMenu != null)
                contentMenu.SelectedIndex = headerList.Items.IndexOf(sender);
        }

        private void ContentPresenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var response = App.Rest.Post<UserInfoResponse>(new RestRequest("launcher/info"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                txtLogin.Text = response.Data.username;
                txtBonuses.Text = response.Data.balance.ToString();
            }

            
        }

        private void ListBoxItem_Selected_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
