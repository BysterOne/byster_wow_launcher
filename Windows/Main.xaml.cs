using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.PanelChanger;
using Launcher.PanelChanger.Enums;
using Launcher.Settings;
using Launcher.Windows.AnyMain.Enums;
using Launcher.Windows.AnyMain.Errors;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Launcher.Windows
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Window, ITransferable
    {
        public Main()
        {
            InitializeComponent();

            Loaded += WLoaded;
        }

        #region Переменные
        public LogBox Pref { get; set; } = new("Main");
        private CPanelChanger<EPC_MainShop> EPC_MainPanelChanger { get; set; }
        #endregion

        

        #region Анимации
        #region PanelChangerHide
        private async Task PanelChangerHide(UIElement element, bool UseAnimation = true, bool Pending = true)
        {
            var duration = AnimationHelper.AnimationDuration;
            var tcs = new TaskCompletionSource<object?>();

            await Dispatcher.InvokeAsync(() =>
            {
                var animation = AnimationHelper.OpacityAnimation((FrameworkElement)element, 0, UseAnimation ? duration : TimeSpan.FromMilliseconds(1));
                animation.Completed += (s, e) => tcs.SetResult(null);
                animation.Begin((FrameworkElement)element, HandoffBehavior.SnapshotAndReplace, true);
            });

            if (Pending) { await tcs.Task; }
        }
        #endregion
        #region PanelChangerShow
        private async Task PanelChangerShow(UIElement element, bool UseAnimation = true, bool Pending = true)
        {
            var duration = AnimationHelper.AnimationDuration;
            var tcs = new TaskCompletionSource<object?>();

            await Dispatcher.InvokeAsync(() =>
            {
                element.Visibility = Visibility.Visible;

                var animation = AnimationHelper.OpacityAnimation((FrameworkElement)element, 1, UseAnimation ? duration : TimeSpan.FromMilliseconds(1));
                animation.Completed += (s, e) => tcs.SetResult(null);
                animation.Begin((FrameworkElement)element, HandoffBehavior.SnapshotAndReplace, true);
            });

            if (Pending) { await tcs.Task; }
        }
        #endregion
        #endregion

        #region Функции
        #region Инициализация
        public async Task<UResponse> Initialization()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить инициализацию";

            #region try
            try
            {
                #region Переключатель страниц магазина и главной
                EPC_MainPanelChanger = new
                (
                    MainPanels,
                    [
                        new (EPC_MainShop.Main, MP_main),
                        new (EPC_MainShop.Shop, MP_shop)
                    ],
                    EPC_MainShop.Main,
                    EPanelState.Showen
                );
                EPC_MainPanelChanger.ShowElement += PanelChangerShow;
                EPC_MainPanelChanger.HideElement += PanelChangerHide;
                var tryInitMainChanger = await EPC_MainPanelChanger.Init();
                if (!tryInitMainChanger.IsSuccess) throw new UExcept(EInitialization.FailInitPanelChanger, $"Ошибка запуска '{nameof(EPC_MainPanelChanger)}'", tryInitMainChanger.Error);
                #endregion
                #region Обновляем язык

                #endregion

                #region Инициализация страниц
                #region Главная

                #endregion
                #region Магазин
                var tryInitShopPage = await MP_shop.Initialization();
                if (!tryInitShopPage.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailInitPage, $"Не удалось загрузить страницу Shop", tryInitShopPage.Error);
                }
                #endregion
                #endregion

                #region Открываем главную
                await ChangeMainPanel(EPC_MainShop.Main);
                #endregion
                #region Загружаем информацию о пользователе
                var tryGetUserInfo = await CApi.GetUserInfo();
                if (!tryGetUserInfo.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailGetUserInfo, $"Не удалось загрузить данные пользователя", tryGetUserInfo.Error);
                }
                GProp.User = tryGetUserInfo.Response;
                UpdateUserDataView();
                #endregion

                return new() { IsSuccess = true };
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex.Error);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(GlobalErrors.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region UpdateUserDataView
        private void UpdateUserDataView()
        {
            #region Проверка на пустоту
            if (GProp.User is null) return;
            #endregion
            #region Установка данных
            TPIS_username.Text = GProp.User.Username;
            TPIS_balance.Text = $"{GProp.User.Balance.ToOut()} {GProp.User.Currency}";
            #endregion
        }
        #endregion
        #region ChangeMainPanel
        private async Task ChangeMainPanel(EPC_MainShop page)
        {
            var tcs = new TaskCompletionSource<object?>();

            await Dispatcher.Invoke(async () =>
            {
                TP_main_page_button.IsActive = page is EPC_MainShop.Main;
                TP_store_page_button.IsActive = page is EPC_MainShop.Shop;
                await EPC_MainPanelChanger.ChangePanel(page);
                
                tcs.SetResult(null);
            });

            await tcs.Task;
        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            TP_main_page_button.Text = Dictionary.Translate("ГЛАВНАЯ");
            TP_store_page_button.Text = Dictionary.Translate("МАГАЗИН");

            await MP_main.UpdateAllValues();
        }
        #endregion
        #region Notify
        public void Notify(string message)
        {
            NP_message_block.Text = message;

            #region Анимация
            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, TimeSpan.FromMilliseconds(150)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, TimeSpan.FromMilliseconds(3150)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, TimeSpan.FromMilliseconds(3300)));

            Storyboard.SetTarget(animation, NotifyPanel);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(animation);

            storyboard.Begin(NP_message_block, HandoffBehavior.SnapshotAndReplace, true);
            #endregion
        }
        #endregion
        #region Loader
        public async Task Loader(ELoaderState newState, bool isFullBlack = false)
        {
            var tcs = new TaskCompletionSource<object?>();

            if (newState == ELoaderState.Show)
            {
                if (isFullBlack) LP_fill.SetResourceReference(Shape.FillProperty, "back_main");
                else LP_fill.Fill = new SolidColorBrush(Color.FromArgb(254, 9, 12, 22));

                LoaderPanel.Visibility = Visibility.Visible;
                var animation = AnimationHelper.OpacityAnimation(LoaderPanel, 1);
                animation.Completed += (s, e) => tcs.SetResult(null);
                animation.Begin(LoaderPanel, HandoffBehavior.SnapshotAndReplace, true);
                LP_loader.StartAnimation();
            }
            else
            {
                var animation = AnimationHelper.OpacityAnimation(LoaderPanel, 0);
                animation.Completed += (s, e) =>
                {
                    Dispatcher.Invoke(() => LoaderPanel.Visibility = Visibility.Hidden);
                    tcs.SetResult(null);
                };
                LP_loader.StopAnimation();
                animation.Begin(LoaderPanel, HandoffBehavior.SnapshotAndReplace, true);
            }

            await tcs.Task;
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region WLoaded
        private async void WLoaded(object sender, RoutedEventArgs e)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());

            #region Прелоадер
            await Loader(ELoaderState.Show, true);
            #endregion
            #region Инициализация
            var tryInit = await Initialization();
            if (!tryInit.IsSuccess)
            {
                Functions.Error(tryInit.Error, $"Ошибка инициализации. Выход из приложения...", _proc);


                // TODO: Сделать вывод в отдельное окно
                this.Hide();
                MessageBox.Show("Ошибка инициализации. Приложение будет закрыто.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            #endregion

            #region Прелоадер
            await Loader(ELoaderState.Hide);
            #endregion
        }
        #endregion
        #region TP_main_page_button_MouseDown
        private void TP_main_page_button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!TP_main_page_button.IsActive)
                _ = ChangeMainPanel(EPC_MainShop.Main);
        }
        #endregion
        #region TP_store_page_button_MouseDown
        private void TP_store_page_button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!TP_store_page_button.IsActive)
                _ = ChangeMainPanel(EPC_MainShop.Shop);
        }
        #endregion
        #endregion


    }
}
