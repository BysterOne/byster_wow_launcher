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
using Byster.Models.BysterWOWModels;
using Byster.Models.RestModels;
using Byster.Utilities.WOWModels;
using RestSharp;
using System.Net;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindowReworked.xaml
    /// </summary>
    public partial class MainWindowReworked : Window
    {
        private WoWSearcher searcher;
        MainWindowManager Manager { get; set; }
        public MainWindowReworked(string login)
        {
            InitializeComponent();
            Manager = new MainWindowManager();
            this.DataContext = Manager;

            searcher = new WoWSearcher("World of Warcraft");
            searcher.OnWowFounded += OnWOWFound;
            searcher.OnWowChanged += OnWOWChanged;
            searcher.OnWowClosed += OnWOWClosed;
        }

        private bool OnWOWClosed(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                Manager.SessionsCollection.Remove(Manager.SessionsCollection.First((session) => session.wowApp.Process == p.Process));
            });
            return true;
        }

        private bool OnWOWChanged(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                WOWSession changedSession = new WOWSession();
                changedSession.wowApp = p;
                changedSession.SessionClass = new WOWClass(WOWSession.ConverterOfClasses(p.Class));
                changedSession.UserName = p.Name;
                changedSession.ServerName = p.Version;
                Manager.SessionsCollection.Remove(Manager.SessionsCollection.First((session) => session.wowApp.Process == p.Process));
                Manager.SessionsCollection.Add(changedSession);
            });
            return true;
        }

        private bool OnWOWFound(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                Manager.SessionsCollection.Add(new WOWSession()
                {
                    SessionClass = new WOWClass(WOWSession.ConverterOfClasses(p.Class)),
                    UserName = p.Name,
                    ServerName = p.Version,
                    wowApp = p,
                });
            });
            return true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void sessionsViewBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Manager.FilterClass = Manager.selectedSession != null ? Manager.selectedSession.SessionClass.EnumWOWClass : WOWClasses.ANY;
            Manager.FilterRotations();
        }

        private void titleButton_Click(object sender, RoutedEventArgs e)
        {
            sessionsViewBox.SelectedItem = null;
        }

        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }


    public class MainWindowManager
    {
        public string UserName { get; set; }
        public WOWSession selectedSession { get; set; }
        public ObservableCollection<WOWSession> SessionsCollection { get; set; }
        public ObservableCollection<RotationWOW> AllRotations { get; set; }
        public ObservableCollection<RotationWOW> RotationViewCollection { get; set; }
        public WOWClasses FilterClass { get; set; }
        public bool IsInjecting { get; set; }
        public bool IsNotInjecting { get; set; }

        public List<Visibility> ControlVisibilities { get; private set; }
        public List<Visibility> PageVisibilities { get; private set; }

        public MainWindowManager()
        {
            SessionsCollection = new ObservableCollection<WOWSession>();
            AllRotations = new ObservableCollection<RotationWOW>();
            RotationViewCollection = new ObservableCollection<RotationWOW>();

            FilterClass = WOWClasses.ANY;

            ControlVisibilities = new List<Visibility>()
            {
                Visibility.Visible,
                Visibility.Collapsed,
                Visibility.Collapsed,
            };

            PageVisibilities = new List<Visibility>()
            {
                Visibility.Visible,
                Visibility.Collapsed,
            };

            UserName = "Default";
            UpdateRotations();
            FilterRotations();
        }

        public void SelectPage(int pageIndex, bool condition)
        {
            if(condition)
            {
                PageVisibilities[PageVisibilities.IndexOf(Visibility.Visible)] = Visibility.Collapsed;
                PageVisibilities[pageIndex] = Visibility.Visible;
            }
        }

        public void SelectControls(int controlIndex, bool condition)
        {
            if(condition)
            {
                ControlVisibilities[ControlVisibilities.IndexOf(Visibility.Visible)] = Visibility.Collapsed;
                ControlVisibilities[controlIndex] = Visibility.Visible;
            }
        }

        public void FilterRotations()
        {
            RotationViewCollection.Clear();
            foreach(var rotation in AllRotations)
            {
                if(rotation.RotationClass.EnumWOWClass == FilterClass || rotation.RotationClass.EnumWOWClass == WOWClasses.ANY || FilterClass == WOWClasses.ANY)
                {
                    RotationViewCollection.Add(rotation);
                }
            }
            CheckAvailableRotations();
        }

        public void UpdateRotations()
        {
            var rotationsResponse = App.Rest.Post<List<RotationResponse>>(new RestRequest("shop/my_subscriptions"));
            if(rotationsResponse.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Запрос завершён с кодом: {(int)rotationsResponse.StatusCode}\nСообщение ошибки: {rotationsResponse.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            foreach( var rotationResponse in rotationsResponse.Data)
            {
                AllRotations.Add(new RotationWOW(rotationResponse));
            }

        }

        public void CheckAvailableRotations()
        {
            if(RotationViewCollection.Count == 0)
            {
                SelectControls(2, !IsInjecting);
            }
            else
            {
                SelectControls(0, true);
            }
        }
    }
}
