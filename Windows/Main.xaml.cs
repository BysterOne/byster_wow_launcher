using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Api;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components;
using Launcher.Components.DialogBox;
using Launcher.Components.MainWindow;
using Launcher.Components.PanelChanger;
using Launcher.PanelChanger.Enums;
using Launcher.Settings;
using Launcher.Settings.Enums;
using Launcher.Windows.AnyMain.Enums;
using Launcher.Windows.AnyMain.Errors;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Launcher.Windows
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Window, ITranslatable
    {
        public Main()
        {
            InitializeComponent();

            TranslationHub.Register(this);

            Loaded += WLoaded;
            this.PreviewKeyDown += EPreviewKeyDown;
            GProp.LauncherUpdateEvent += ELauncherUpdateEvent;
        }

        #region Переменные
        public static LogBox Pref { get; set; } = new("Main");
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
                var animation = AnimationHelper.OpacityAnimationStoryBoard((FrameworkElement)element, 0, UseAnimation ? duration : TimeSpan.FromMilliseconds(1));
                animation.Completed += (s, e) => { Panel.SetZIndex(element, -1); tcs.SetResult(null); };
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
            Panel.SetZIndex(element, 1);

            await Dispatcher.InvokeAsync(() =>
            {
                element.Visibility = Visibility.Visible;

                var animation = AnimationHelper.OpacityAnimationStoryBoard((FrameworkElement)element, 1, UseAnimation ? duration : TimeSpan.FromMilliseconds(1));
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
                #region Запуск пинг-помощника
                CPingHelper.Start();
                #endregion
                #region Загрузка словаря
                var tryLoadTranslations = await Dictionary.Load();
                if (!tryLoadTranslations.IsSuccess) throw new UExcept(EInitialization.FailLoadTranslations, $"Ошибка загрузка словарей", tryLoadTranslations.Error);
                #endregion
                #region Загружаем информацию о пользователе
                var tryGetUserInfo = await CApi.GetUserInfo();
                if (!tryGetUserInfo.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailGetUserInfo, $"Не удалось загрузить данные пользователя", tryGetUserInfo.Error);
                }
                GProp.User = tryGetUserInfo.Response;
                SentrySdk.ConfigureScope(scope => scope.User = new SentryUser() { Username = GProp.User.Username });
                UpdateUserDataView();
                #endregion
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
                #endregion
                #region Обновляем язык
                _ = UpdateAllValues();
                #endregion
                #region Создание и ожидание задач
                var tryInitMainPageTask = MP_main.Initialization();
                var tryInitShopPageTask = MP_shop.Initialization();
                var tryInitMdHelper = MdHelper.BuildDocMarkdigAsync("#m#", (r) => { });
                var tryInitMainChangerTask = EPC_MainPanelChanger.Init();

                await Task.WhenAll(tryInitMainPageTask, tryInitShopPageTask, tryInitMdHelper);
                #endregion
                #region Обработка ответов
                #region Страницы
                #region Главная
                var tryInitMainPage = await tryInitMainPageTask;
                if (!tryInitMainPage.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailInitPage, $"Не удалось загрузить страницу Main", tryInitMainPage.Error);
                }
                #endregion
                #region Магазин
                var tryInitShopPage = await tryInitShopPageTask;
                if (!tryInitShopPage.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailInitPage, $"Не удалось загрузить страницу Shop", tryInitShopPage.Error);
                }
                #endregion
                #endregion
                #region Переключатель страниц
                var tryInitMainChanger = await tryInitMainChangerTask;
                if (!tryInitMainChanger.IsSuccess) throw new UExcept(EInitialization.FailInitPanelChanger, $"Ошибка запуска '{nameof(EPC_MainPanelChanger)}'", tryInitMainChanger.Error);
                #endregion
                #endregion
                #region Обновляем язык у редкатора корзины
                _ = CartEditor.UpdateAllValues();
                #endregion

                #region Открываем главную
                await ChangeMainPanel(EPC_MainShop.Main);
                #endregion              

                return new() { IsSuccess = true };
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
                return new(uex);
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
        }
        #endregion
        #region Notify
        public static void Notify(string message)
        {
            var mainWindow = Application.Current.Windows.OfType<Main>().FirstOrDefault() ?? throw new UExcept(EShowModal.MainWindowWasNull, $"Основное окно было пустым");
            mainWindow.NP_message_block.Text = message;

            #region Анимация
            var storyboard = new Storyboard();
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, TimeSpan.FromMilliseconds(150)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, TimeSpan.FromMilliseconds(3150)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, TimeSpan.FromMilliseconds(3300)));

            Storyboard.SetTarget(animation, mainWindow.NotifyPanel);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(animation);

            storyboard.Begin(mainWindow.NP_message_block, HandoffBehavior.SnapshotAndReplace, true);
            #endregion
        }
        #endregion
        #region Loader
        public static async Task Loader(ELoaderState newState, bool isFullBlack = false)
        {
            #region Установка ожидания
            var tcs = new TaskCompletionSource<object?>();
            #endregion
            #region Главное окно
            var mainWindow = Application.Current.Windows.OfType<Main>().FirstOrDefault() ?? throw new Exception("Пустое окно");
            #endregion
            #region Показ
            if (newState == ELoaderState.Show)
            {
                if (isFullBlack) mainWindow.LP_fill.SetResourceReference(Shape.FillProperty, "back_main");
                else mainWindow.LP_fill.Fill = new SolidColorBrush(Color.FromArgb(254, 9, 12, 22));

                mainWindow.LoaderPanel.Visibility = Visibility.Visible;
                var animation = AnimationHelper.OpacityAnimationStoryBoard(mainWindow.LoaderPanel, 1);
                animation.Completed += (s, e) => tcs.SetResult(null);
                animation.Begin(mainWindow.LoaderPanel, HandoffBehavior.SnapshotAndReplace, true);
                mainWindow.LP_loader.StartAnimation();
            }
            #endregion
            #region Скрытие
            else
            {
                var animation = AnimationHelper.OpacityAnimationStoryBoard(mainWindow.LoaderPanel, 0);
                animation.Completed += (s, e) =>
                {
                    mainWindow.Dispatcher.Invoke(() => mainWindow.LoaderPanel.Visibility = Visibility.Hidden);
                    tcs.SetResult(null);
                };
                mainWindow.LP_loader.StopAnimation();
                animation.Begin(mainWindow.LoaderPanel, HandoffBehavior.SnapshotAndReplace, true);
            }
            #endregion
            await tcs.Task;
        }
        #endregion
        #region ShowModal
        public static async Task<UResponse<EResponse>> ShowModal(BoxSettings settings)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось отобразить модальное окно";

            #region try
            try
            {
                #region Установка компонента
                var mainWindow = Application.Current.Windows.OfType<Main>().FirstOrDefault() ?? throw new UExcept(EShowModal.MainWindowWasNull, $"Основное окно было пустым");
                #endregion
                #region Создание задачи на ожидание
                var taskWaiter = new TaskCompletionSource<UResponse<EResponse>>();
                #endregion
                #region Создание компонента
                await mainWindow.Dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        var modalComponent = new CDialogBox();
                        Panel.SetZIndex(modalComponent, 9);
                        mainWindow.modalGrid.Visibility = Visibility.Visible;
                        mainWindow.modalGrid.Children.Add(modalComponent);

                        var response = await modalComponent.Show(settings);
                        taskWaiter.SetResult(response);
                        await modalComponent.Hide();
                        mainWindow.modalGrid.Children.Remove(modalComponent);
                    }
                    catch (Exception ex) { taskWaiter.SetException(ex); }
                });
                #endregion
                #region Ожидание ответа
                return await taskWaiter.Task;
                #endregion
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
                return new(uex);
            }
            #endregion
        }
        #endregion
        #region ShowModal
        public static async Task<UResponse<Any.UDialogBox.EDialogResponse>> ShowModal<T>(T dialog, params object[] pars) where T : UIElement, IUDialogBox
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось отобразить модальное окно типа {typeof(T).Name}";

            #region try
            try
            {
                #region Установка компонента
                var mainWindow = Application.Current.Windows.OfType<Main>().FirstOrDefault() ?? throw new UExcept(EShowModal.MainWindowWasNull, $"Основное окно было пустым");
                #endregion
                #region Создание задачи на ожидание
                var taskWaiter = new TaskCompletionSource<UResponse<Any.UDialogBox.EDialogResponse>>();
                #endregion
                #region Создание компонента
                await mainWindow.Dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        Panel.SetZIndex(dialog, 9);
                        dialog.Opacity = 0;
                        mainWindow.modalGrid.Visibility = Visibility.Visible;
                        mainWindow.modalGrid.Children.Add(dialog);

                        var response = await dialog.Show(pars);
                        taskWaiter.SetResult(response);
                        await dialog.Hide();
                        mainWindow.modalGrid.Children.Remove(dialog);
                    }
                    catch (Exception ex) { taskWaiter.SetException(ex); }
                });
                #endregion
                #region Ожидание ответа
                return await taskWaiter.Task;
                #endregion
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
                return new(uex);
            }
            #endregion
        }
        #endregion
        #region ChangeCartEditorState
        public static void ChangeCartEditorState(bool show)
        {
            var mainWindow = Application.Current.Windows.OfType<Main>().FirstOrDefault() ?? throw new UExcept(EShowModal.MainWindowWasNull, $"Основное окно было пустым");
            #region Показать
            if (show)
            {
                mainWindow.CartEditor.Visibility = Visibility.Visible;
                AnimationHelper.OpacityAnimationStoryBoard(mainWindow.CartEditor, 1).
                    Begin(mainWindow.CartEditor, HandoffBehavior.SnapshotAndReplace, true);
                mainWindow.CartEditor.IsShowed = true;
            }
            #endregion
            #region Скрыть
            else
            {
                var anim = AnimationHelper.OpacityAnimationStoryBoard(mainWindow.CartEditor, 0);
                anim.Completed += (s, e) => mainWindow.Dispatcher.Invoke(() => mainWindow.CartEditor.Visibility = Visibility.Hidden);
                anim.Begin(mainWindow.CartEditor, HandoffBehavior.SnapshotAndReplace, true);
                mainWindow.CartEditor.IsShowed = false;
            }
            #endregion
        }
        #endregion
        #region GoToPayment
        public static async void GoToPayment()
        {
            if (GProp.Cart.Items.Count is 0)
            {
                Notify(Dictionary.Translate("Ваша корзина пуста"));
                return;
            }            

            _ = Main.ShowModal(new CPayDialogBox());

            await Task.Run(() => Thread.Sleep(300));
            Main.ChangeCartEditorState(false);
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
        #region ELauncherUpdateEvent
        private async Task ELauncherUpdateEvent(ELauncherUpdate updates, object? data)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName()).AddTrace($"{ELauncherUpdate.User}");

            if (updates.HasFlag(ELauncherUpdate.User))
            {
                #region Если есть данные
                if (data is User user)
                {
                    GProp.User = user;
                    _ = GProp.SendUpdated(ELauncherUpdate.User);
                    UpdateUserDataView();
                    return;
                }
                #endregion
                #region Если нет
                var tryGetUserInfo = await CApi.GetUserInfo();
                if (!tryGetUserInfo.IsSuccess)
                {
                    var exec = new UExcept(EInitialization.FailGetUserInfo, $"Не удалось загрузить данные пользователя", tryGetUserInfo.Error);
                    Functions.Error(exec, "Ошибка загрузки данных пользователя", _proc);
                    return;
                }

                GProp.User = tryGetUserInfo.Response;
                _ = GProp.SendUpdated(ELauncherUpdate.User);
                UpdateUserDataView();
                #endregion
            }                
        }
        #endregion
        #region EPreviewKeyDown
        private async void EPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    {
                        await Loader(ELoaderState.Show);
                        await GProp.Update(ELauncherUpdate.User | ELauncherUpdate.Shop | ELauncherUpdate.Subscriptions);
                        await Task.Run(() => Thread.Sleep(300));
                        await Loader(ELoaderState.Hide);
                    }
                    break;
                default: base.OnKeyDown(e); break;
            }
        }
        #endregion
        #region TPIS_settings_MouseLeftButtonDown
        private void TPIS_settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _ = Main.ShowModal(new CSettingsDialogBox());
        }
        #endregion
        #region Background_MouseDown
        private void Background_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
        #endregion
        #endregion


    }
}
