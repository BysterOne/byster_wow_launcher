using Cls.Any;
using Launcher.Any;
using Launcher.Components.DialogBox;
using Launcher.Components.MainWindow.Any.PageMain.AddServerDialogBoxAny;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using Launcher.Windows;
using System.Diagnostics;
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

            this.MouseEnter += EMouseEnter;
            this.MouseLeave += EMouseLeave;
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
            sender.SetServer();
        }
        #endregion
        #endregion

        #region Функции
        #region SetServer
        private void SetServer()
        {
            #region Ставим иконку
            border.Background = new ImageBrush() { ImageSource = GStatic.GetServerIcon(Server.Icon) };
            #endregion
            #region Проверка на выбранный
            IsSelected = GProp.SelectedServer is not null && GProp.SelectedServer.Id == Server.Id;
            #endregion
            #region Ставим текст
            ServerName.Text = Server.Name;
            #endregion
        }
        #endregion
        #region UpdateSelectedView
        public void UpdateSelectedView()
        {
            var opcityAnim = AnimationHelper.OpacityAnimationStoryBoard(Selection_Background, IsSelected ? 0.2 : 0);
            opcityAnim.Begin(border, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region ChangeHoverState
        private void ChangeHoverState()
        {
            var storyboard = new Storyboard();
            var ease = new PowerEase() { EasingMode = EasingMode.EaseInOut };

            var nameFullWidth = this.ActualWidth - 65;
            var needFreeWidth = (nameFullWidth - ServerName.ActualWidth) - ButtonsPanel.ActualWidth;
            var newHideOffset = (ServerName.ActualWidth + needFreeWidth) / ServerName.ActualWidth;
            var newOffset = this.IsMouseOver ? needFreeWidth < 0 ? newHideOffset : 5 : 5;

            var gradientStopAnim = new DoubleAnimation(newOffset, AnimationHelper.AnimationDuration) { EasingFunction = ease };
            Storyboard.SetTarget(gradientStopAnim, ServerName);
            Storyboard.SetTargetProperty(gradientStopAnim, new PropertyPath("OpacityMask.GradientStops[1].Offset"));
            storyboard.Children.Add(gradientStopAnim);

            ButtonsPanel.IsHitTestVisible = this.IsMouseOver;
            var animationOpacity = AnimationHelper.DoubleAnimation
            (
                ButtonsPanel, this.IsMouseOver ? 1 : 0,
                new PropertyPath(UIElement.OpacityProperty),
                duration: AnimationHelper.AnimationDuration
            );
            storyboard.Children.Add(animationOpacity);

            storyboard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region RemoveItem
        private async Task RemoveItem()
        {
            var accept = await Main.ShowModal(new BoxSettings
            (
                Dictionary.Translate("Удаление"),
                Dictionary.Translate($"Вы точно хотите удалить данный клиент игры?"),
                [
                    new(EResponse.Yes, Dictionary.Translate("Да")),
                    new(EResponse.Cancel, Dictionary.Translate("Отмена")),                    
                ]
            ));

            if (!accept.IsSuccess || accept.Response is EResponse.Cancel) return;

            _ = Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    AppSettings.Instance.Servers.Remove(Server);
                    AppSettings.Save();
                });
            });
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
        #region EMouseEnter
        private void EMouseEnter(object sender, MouseEventArgs e) => ChangeHoverState();
        #endregion
        #region EMouseLeave
        private void EMouseLeave(object sender, MouseEventArgs e) => ChangeHoverState();
        #endregion
        #region BP_edit_MouseLeftButtonDown
        private async void BP_edit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            await Main.ShowModal(new CAddServerDialogBox(), EShowType.EditServer, Server);
            SetServer();
        }
        #endregion
        #region BP_remove_MouseLeftButtonDown
        private void BP_remove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _ = RemoveItem(); 
        }
        #endregion
        #endregion


    }
}
