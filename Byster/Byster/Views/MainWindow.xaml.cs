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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Byster.Models.BysterWOWModels;
using Byster.Utilities.WOWModels;
using RestSharp;
using Byster.Models.RestModels;
using System.Net;

namespace Byster
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<RotationWOW> RotationsToView { get; set; }
        public ObservableCollection<RotationWOW> AllRotations { get; set; }
        public ObservableCollection<WOWSession> WowSessions { get; set; }

        private WoWSearcher searcher;

        public WOWClasses FilterClass;

        public string UserName { get; set; }
        public MainWindow(string username)
        {
            InitializeComponent();
            UserName = username;

            RotationsToView = new ObservableCollection<RotationWOW>();
            rotationsView.ItemsSource = RotationsToView;
            WowSessions = new ObservableCollection<WOWSession>()
            {
                new WOWSession()
                {
                    ServerName = "testServerName",
                    UserName = "testUser1",
                    SessionClass = new WOWClass(WOWClasses.Warrior),
                },
                new WOWSession()
                {
                    ServerName = "testServerName",
                    UserName = "testUser2",
                    SessionClass = new WOWClass(WOWClasses.ANY),
                },
                new WOWSession()
                {
                    ServerName = "testServerName",
                    UserName = "testUser3",
                    SessionClass = new WOWClass(WOWClasses.Wizard),
                },
            };
            sessionsView.ItemsSource = WowSessions;

            updateRotations();
            FilterClass = WOWClasses.ANY;
            filter();

            searcher = new WoWSearcher("WoWSearcher");
            searcher.OnWowFounded += OnWowFounded;
            searcher.OnWowClosed += OnWowClosed;
            searcher.OnWowChanged += OnWowChanged;

            userNameTextBlock.Text = UserName;
        }

        private bool OnWowChanged(WoW p)
        {
            WOWSession changedsession = WowSessions.Single((session) => session.wowApp.Process == p.Process);
            changedsession.wowApp = p;
            changedsession.UserName = p.Name;
            changedsession.ServerName = p.RealmServer;
            changedsession.SessionClass = new WOWClass(WOWSession.ConverterOfClasses(p.Class));
            return true;
        }

        private bool OnWowClosed(WoW p)
        {
            
            WowSessions.Remove(WowSessions.Single((session) => session.wowApp.Process == p.Process));
            return true;
        }

        private bool OnWowFounded(WoW p)
        {
            WowSessions.Add(new WOWSession()
            {
                wowApp = p,
                UserName = p.Name,
                ServerName = p.RealmServer,
                SessionClass = new WOWClass(WOWSession.ConverterOfClasses(p.Class)),
            });
            return true;
        }

        private void sessionsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WOWSession selectedSession = sessionsView.SelectedItem as WOWSession;
            if(selectedSession == null)
            {
                FilterClass = WOWClasses.ANY;
            }
            else
            {
                FilterClass = selectedSession.SessionClass.EnumWOWClass;
            }
            filter();
        }

        private bool updateRotations()
        {
            var response = App.Rest.Post<List<RotationResponse>>(new RestRequest("shop/my_subscriptions"));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Запрос завершён с кодом: {(int)response.StatusCode}\nСообщение ошибки: {response.ErrorMessage}");
                return false;
            }

            List<RotationResponse> rotationResponses = response.Data;
            AllRotations = new ObservableCollection<RotationWOW>();
            foreach(var rotationResponse in rotationResponses)
            {
                AllRotations.Add(new RotationWOW(rotationResponse));
            }
            return true;
        }
        private void filter()
        {
            RotationsToView.Clear();
            string filterName = new WOWClass(FilterClass).NameOfClass;
            foreach (var rotation in AllRotations)
            {
                if(FilterClass == WOWClasses.ANY || rotation.RotationClass.NameOfClass == filterName)
                {
                    RotationsToView.Add(rotation);
                }
            }
        }



        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void titleBtn_Click(object sender, RoutedEventArgs e)
        {
            sessionsView.SelectedItem = null;
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
