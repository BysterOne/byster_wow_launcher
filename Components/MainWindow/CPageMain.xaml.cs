using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.LaunchExeHelperAny;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.DialogBox;
using Launcher.Components.MainWindow.Any.PageMain;
using Launcher.Components.MainWindow.Any.PageMain.Enums;
using Launcher.Components.MainWindow.Any.PageMain.Errors;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Components.PanelChanger;
using Launcher.Settings;
using Launcher.Settings.Enums;
using Launcher.Windows;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using static System.Net.Mime.MediaTypeNames;


namespace Launcher.Components.MainWindow
{
    #region Errors
    namespace Any.PageMain.Errors
    {
        #region EInitialization
        public enum EInitialization
        {
            FailLoadSubscriptions,
            FailLoadPanelChanger
        }
        #endregion
    }
    #endregion
    #region Errors
    namespace Any.PageMain.Enums
    {
        #region EPC_Subscription
        public enum EPC_Subscription
        {
            NoSubscriptions,
            SubscriptionsList
        }
        #endregion
    }
    #endregion
    /// <summary>
    /// Логика взаимодействия для CPageMain.xaml
    /// </summary>
    public partial class CPageMain : UserControl, ITranslatable
    {
        public CPageMain()
        {
            InitializeComponent();

            TranslationHub.Register(this);

            LaunchExeHelper.OnLaunchItemUpdate += ELaunchItemUpdate;
            GProp.LauncherUpdateEvent += ELauncherUpdateEvent;
            GProp.Subscriptions.CollectionChanged += ECollectionChanged;
            GProp.SelectedServerChanged += ESelectedServerChanged;
        }

        #region Переменные
        public static LogBox Pref { get; set; } = new("Main Page");
        private CPanelChanger<EPC_Subscription> PanelChanger { get; set; }
        #endregion

        #region Обработчики событий
        #region LP_addButton_MouseLeftButtonDown
        private void LP_addButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dlg = new CAddServerDialogBox();
            _ = Main.ShowModal(dlg);
        }
        #endregion
        #region ECollectionChanged
        private void ECollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action is NotifyCollectionChangedAction.Add || e.Action is NotifyCollectionChangedAction.Remove)
            {
                if (GProp.Subscriptions.Count is 0 && PanelChanger.SelectedPanel is not EPC_Subscription.NoSubscriptions)
                    _ = PanelChanger.ChangePanel(EPC_Subscription.NoSubscriptions);
                else
                if (GProp.Subscriptions.Count > 0 && PanelChanger.SelectedPanel is not EPC_Subscription.SubscriptionsList)
                    _ = PanelChanger.ChangePanel(EPC_Subscription.SubscriptionsList);
            }
        }
        #endregion
        #region launch_button_MouseLeftButtonDown
        private void launch_button_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => LaunchCheat();
        #endregion
        #region ESelectedServerChanged
        private void ESelectedServerChanged(CServer? server)
        {
            if (server is null) return;
            Dispatcher.Invoke(() => UpdateLaunchButtonState(LaunchExeHelper.GetItem(server)));
        }
        #endregion
        #region ELauncherUpdateEvent
        private async Task ELauncherUpdateEvent(ELauncherUpdate updates)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName()).AddTrace($"{ELauncherUpdate.Subscriptions}");

            if (updates.HasFlag(ELauncherUpdate.Subscriptions))
            {
                var tryGetSubscriptions = await CApi.GetUserSubscriptions();
                if (!tryGetSubscriptions.IsSuccess)
                {
                    var except = new UExcept(EInitialization.FailLoadSubscriptions, $"Ошибка загрузки ротаций пользователей", tryGetSubscriptions.Error);
                    Functions.Error(except, except.Error, "Ошибка загрузки ротаций", _proc);
                }
                else
                {
                    GProp.UpdateSubscriptions(tryGetSubscriptions.Response);
                }
            }
        }
        #endregion
        #region ELaunchItemUpdate
        private void ELaunchItemUpdate(LaunchItem item)
        {
            Dispatcher.Invoke(() => UpdateLaunchButtonState(item));
        }
        #endregion
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
                #region Инициация переключателя панелей
                PanelChanger = new
                (
                    AviableProducts,
                    [
                        new(EPC_Subscription.NoSubscriptions, AP_empty),
                        new(EPC_Subscription.SubscriptionsList, AP_items)
                    ],
                    EPC_Subscription.NoSubscriptions
                );
                PanelChanger.ShowElement += PanelChangerShow;
                PanelChanger.HideElement += PanelChangerHide;
                var initPanelChanger = await PanelChanger.Init();
                if (!initPanelChanger.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailLoadPanelChanger, $"Ошибка загрузки панели типа {nameof(EPC_Subscription)}", initPanelChanger.Error);
                }
                #endregion
                #region Делаем привязку серверов из настроек
                LP_servers.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = AppSettings.Instance.Servers });
                #endregion
                #region Загрузка списка ротаций
                var tryGetSubscriptions = await CApi.GetUserSubscriptions();
                if (!tryGetSubscriptions.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailLoadSubscriptions, $"Ошибка загрузки ротаций пользователей", tryGetSubscriptions.Error);
                }
                GProp.Subscriptions.Clear();
                foreach (var sub in tryGetSubscriptions.Response) GProp.Subscriptions.Add(sub);
                #endregion
                #region Обновление состояния кнопки запуска
                Dispatcher.Invoke(() => UpdateLaunchButtonState(null));
                #endregion
                #region Обновление языка
                _ = UpdateAllValues();
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
        #region UpdateLaunchButtonState
        private async void UpdateLaunchButtonState(LaunchItem? item)
        {
            if (GProp.SelectedServer is not null)
            {
                #region Если нет в компоненте запуска
                if (item is null)
                {
                    if (GProp.Subscriptions.Count == 0)
                    {
                        launch_button.IsEnabled = false;
                        HoverHint.SetText(launch_button, Dictionary.Translate($"Вам надо иметь хотя бы одну подписку"));
                        return;
                    }

                    launch_button.IsEnabled = true;
                    launch_button.Text = Dictionary.Translate("Запустить");
                    HoverHint.SetText(launch_button, "");
                    return;
                }
                #endregion

                #region Если это есть в компоненте запуска
                if (item.Server.Id == GProp.SelectedServer.Id || item.State is ELaunchState.ErrorOccurred)
                {                    
                    var enabled = false;
                    var text = string.Empty;

                    #region Разбор статусов                   
                    switch (item.State)
                    {
                        case ELaunchState.Downloading:
                            text = Dictionary.Translate("Загрузка") + "...";
                            break;
                        case ELaunchState.Verifying:
                            text = Dictionary.Translate("Проверка") + "...";
                            break;
                        case ELaunchState.Launching:
                            text = Dictionary.Translate("Запуск") + "...";
                            break;
                        case ELaunchState.Launched:
                            enabled = true;
                            text = Dictionary.Translate("Запустить");
                            break;                        
                        case ELaunchState.Saving:
                            text = Dictionary.Translate("Сохранение") + "...";
                            break;
                        case ELaunchState.ErrorOccurred:
                            text = Dictionary.Translate("Ошибка");

                            #region Выводим ошибку если есть текст
                            if (!String.IsNullOrWhiteSpace(item.Error))
                            {
                                _ = Main.ShowModal
                                (
                                    new BoxSettings
                                    (
                                        Dictionary.Translate($"Ошибка запуска"),
                                        $"{item.Server.Name}: {item.Error.ToString()}",
                                        [
                                            new (EResponse.Ok, "Ok")
                                        ]
                                    )
                                );
                            }
                            #endregion
                            #region Разрешаем заново запустить
                            await Task.Run(() =>
                            {
                                Thread.Sleep(2000);
                                Dispatcher.Invoke(() =>
                                {
                                    launch_button.Text = Dictionary.Translate("Запустить");
                                    launch_button.IsEnabled = true;
                                });
                            });
                            #endregion

                            break;
                    };
                    #endregion

                    launch_button.Text = text;
                    launch_button.IsEnabled = enabled;
                }
                #endregion                               
            }
            else
            {                
                launch_button.IsEnabled = false;
                launch_button.Text = Dictionary.Translate("Запустить");
                HoverHint.SetText(launch_button, Dictionary.Translate($"Для запуска выберите клиент"));
            }
        }
        #endregion
        #region UpdatePanelsVisibility
        private void UpdatePanelsVisibility()
        {

        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            APE_text_block.Text = Dictionary.Translate("На данный момент у Вас нет ротаций. Их можно приобрести в магазине");
            HoverHint.SetText(LP_addButton, Dictionary.Translate("Добавить клиент WoW"));

            UpdateLaunchButtonState(null);
        }
        #endregion
        #region LaunchCheat
        public void LaunchCheat()
        {
            if (GProp.SelectedServer is not null)
            {
                launch_button.IsEnabled = false;
                launch_button.Text = Dictionary.Translate($"Подготовка...");

                _ = Task.Run(() => LaunchExeHelper.Launch(GProp.SelectedServer));
            }
        }
        #endregion
        #endregion


    }
}
