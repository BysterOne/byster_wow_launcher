using Cls.Any;
using Launcher.Any;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Launcher.Components.MainWindow.Any.PageMain
{
    /// <summary>
    /// Логика взаимодействия для CServerItem.xaml
    /// </summary>
    public partial class CServerItem : UserControl
    {
        public CServerItem()
        {
            InitializeComponent();

            Background = new SolidColorBrush(Colors.Transparent);
            Cursor = Cursors.Hand;
            MouseLeftButtonDown += EMouseLeftButtonDown;
            GProp.SelectedServerChanged += ESelectedServerChanged;
        }      

        #region Свойства
        #region IsSelected
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(CServerItem),
                new PropertyMetadata(false, OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CServerItem)d;
            sender.UpdateSelectedView();
        }
        #endregion
        #region Server
        public CServer Server
        {
            get { return (CServer)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }

        public static readonly DependencyProperty ServerProperty =
            DependencyProperty.Register("Server", typeof(CServer), typeof(CServerItem),
                new PropertyMetadata(null, OnServerChanged));

        private static void OnServerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CServerItem)d;
            sender.SetServer((CServer)e.NewValue);
        }
        #endregion
        #endregion

        #region Функции
        #region SetServer
        private void SetServer(CServer server)
        {
            #region Ставим иконку
            border.Background = new ImageBrush() { ImageSource = GStatic.GetServerIcon(server.Icon) };
            #endregion
            #region Проверка на выбранный
            IsSelected = GProp.SelectedServer is not null && GProp.SelectedServer.Id == server.Id;
            #endregion
            #region Подсказка
            HoverHint.SetText(this, server.Name);
            #endregion
        }
        #endregion
        #region UpdateSelectedView
        public void UpdateSelectedView()
        {
            var story = new Storyboard();
            var newColor = IsSelected ? ((SolidColorBrush)Functions.GlobalResources()["orange_main"]).Color : Color.FromArgb(0, 255, 255, 255);
            var animation = new ColorAnimation(newColor, AnimationHelper.AnimationDuration)
            {
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animation, border);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Border.BorderBrush).(SolidColorBrush.Color)"));

            story.Children.Add(animation);

            story.Begin(border, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region EMouseLeftButtonDown
        private void EMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GProp.SelectedServer = Server;
        }
        #endregion
        #region ESelectedServerChanged
        private void ESelectedServerChanged(CServer? server)
        {
            if (server is not null)
            {
                if (server.Id == Server.Id) IsSelected = true;
                else IsSelected = false;
            }          
        }
        #endregion
        #endregion
    }
}
