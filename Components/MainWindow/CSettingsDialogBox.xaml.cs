using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Any.UDialogBox;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.MainWindow.SettingsDialogBoxAny;
using Launcher.Components.PanelChanger;
using Launcher.PanelChanger.Enums;
using Launcher.PanelChanger.Errors;
using Launcher.Settings;
using Launcher.Settings.Enums;
using Launcher.Windows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Launcher.Components.MainWindow
{
    namespace SettingsDialogBoxAny
    {
        public enum EPC_Panels
        {
            Main,
            Advanced
        }

        public enum EShow
        {
            FailInitPanelChanger,
            FailLoadLangList,
            FailLoadServersList
        }
    }
    /// <summary>
    /// Логика взаимодействия для CSettingsDialogBox.xaml
    /// </summary>
    public partial class CSettingsDialogBox : UserControl, IUDialogBox, ITranslatable
    {
        public CSettingsDialogBox()
        {
            InitializeComponent();

            TranslationHub.Register(this);
            GProp.LauncherUpdateEvent += ELauncherUpdateEvent;
        }        

        #region Переменные
        private bool IsInit { get; set; } = false;
        private static LogBox Pref { get; set; } = new LogBox("SettingsDialogBox");
        private static TaskCompletionSource<EDialogResponse> TaskCompletion { get; set; }
        private CPanelChanger<EPC_Panels> PanelChanger { get; set; }
        #endregion

        #region Обработчики событий
        #region ELauncherUpdateEvent
        private async Task ELauncherUpdateEvent(ELauncherUpdate updates)
        {
            if (updates.HasFlag(ELauncherUpdate.User)) await Dispatcher.InvokeAsync(() => UpdateUserPermissions());
        }
        #endregion
        #region ELocalizationList_NewSelectedItem
        private void ELocalizationList_NewSelectedItem(CList.CListItem item)
        {
            var newSelectedLanguage = (ELang)item.Id;

            if (newSelectedLanguage == AppSettings.Instance.Language) return;

            #region Если другой язык
            var main = Application.Current.Windows.OfType<Main>().FirstOrDefault();
            if (main is not null)
            {
                var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(main, 1);
                var fadeOut = AnimationHelper.OpacityAnimationStoryBoard(main, 0);

                fadeOut.Completed += async (e, f) =>
                {
                    AppSettings.Instance.Language = newSelectedLanguage;
                    AppSettings.Save();

                    await Task.Run(() => Thread.Sleep(500));
                    fadeIn.Begin(main, HandoffBehavior.SnapshotAndReplace, true);
                };
                fadeOut.Begin(main, HandoffBehavior.SnapshotAndReplace, true);
            }            
            #endregion
        }
        #endregion
        #region MGPM_clear_cache_MouseDown
        private void MGPM_clear_cache_MouseDown(object sender, MouseButtonEventArgs e) => _ = TryClearCache();
        #endregion
        #region MGPM_redeem_PreviewKeyDown
        private void MGPM_redeem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter) _ = TryRedeemCoupon();
        }
        #endregion
        #region MG_close_button_MouseLeftButtonDown
        private void MG_close_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TaskCompletion?.TrySetResult(EDialogResponse.Closed);
        }
        #endregion
        #region EServerList_NewSelectedItem
        private void EServerList_NewSelectedItem(CList.CListItem item)
        {
            var server = (EServer)item.Id;
            if (server == AppSettings.Instance.Server) return;

            AppSettings.Instance.Server = server;
            AppSettings.Save();

            #region Перезапуск
            var exePath = Process.GetCurrentProcess().MainModule!.FileName;
            Process.Start(exePath);
            Application.Current.Shutdown();
            #endregion
        }
        #endregion
        #region MGBP_main_MouseDown
        private void MGBP_main_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ChangePanel(EPC_Panels.Main);
        #endregion
        #region MGBP_advanced_MouseDown
        private void MGBP_advanced_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ChangePanel(EPC_Panels.Advanced);
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
        #region Show
        public async Task<UResponse<EDialogResponse>> Show(params object[] pars)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось инициализировать компонент";

            #region try
            try
            {
                IsInit = true;
                #region Анимация появления
                var fadeInMiddle = AnimationHelper.OpacityAnimationStoryBoard(middleGrid, 1);
                var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                fadeIn.Completed += (s, e) => { fadeInMiddle.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true); };
                fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                #endregion
                #region Задача
                TaskCompletion = new();
                #endregion           
                #region Язык
                _ = UpdateAllValues();
                #endregion
                #region Переключатель панелей
                PanelChanger = new
                (
                    MG_panels,
                    [
                        new(EPC_Panels.Main, MGP_main),
                        new(EPC_Panels.Advanced, MGP_advanced)
                    ],
                    defaultPanel: EPC_Panels.Main,
                    defaultState: EPanelState.Showen
                );
                PanelChanger.ShowElement += PanelChangerShow;
                PanelChanger.HideElement += PanelChangerHide;
                var tryInitPanelChangerTask = PanelChanger.Init();
                #endregion
                #region Установка списка языков
                var LangList = new List<CList.CListItem>();
                foreach (var lang in Enum.GetValues<ELang>())
                {
                    LangList.Add(new((int)lang, Dictionary.GetLanguageName(lang)));
                }
                var tryLoadLangListTask = MGPM_localization_list.LoadItems(LangList, LangList.IndexOf(LangList.First(x => x.Id == (int)AppSettings.Instance.Language)));
                #endregion
                #region Установка списка серверов
                var serversList = new List<CList.CListItem>();
                foreach (var server in Enum.GetValues<EServer>())
                {
                    serversList.Add(new((int)server, Dictionary.GetServerName(server)));
                }
                var tryLoadServerListTask = MGPM_server_list.LoadItems(serversList, serversList.IndexOf(serversList.First(x => x.Id == (int)AppSettings.Instance.Server)));
                #endregion
                #region Ожидаем завершения задача
                await Task.WhenAll
                (
                    tryInitPanelChangerTask,
                    tryLoadLangListTask,
                    tryLoadServerListTask
                );
                #endregion
                #region Обработчик ответов
                var tryInitPanelChanger = await tryInitPanelChangerTask;
                if (!tryInitPanelChanger.IsSuccess)
                {
                    throw new UExcept(EShow.FailInitPanelChanger, $"Ошибка инициализации панели {nameof(PanelChanger)}");
                }

                var tryLoadLangList = await tryLoadLangListTask;
                if (!tryLoadLangList.IsSuccess)
                {
                    var uerror = new UError(EShow.FailLoadLangList, $"Не удалось загрузить языки в список", tryLoadLangList.Error);
                    Functions.Error(uerror, uerror.Message, _proc);
                }
                MGPM_localization_list.NewSelectedItem += ELocalizationList_NewSelectedItem;

                var tryLoadServerList = await tryLoadServerListTask;
                if (!tryLoadServerList.IsSuccess)
                {
                    var uerror = new UError(EShow.FailLoadServersList, $"Не удалось загрузить сервера в список", tryLoadServerList.Error);
                    Functions.Error(uerror, uerror.Message, _proc);
                }
                MGPM_server_list.NewSelectedItem += EServerList_NewSelectedItem;
                #endregion


                #region Обновление разрешения пользователя
                UpdateUserPermissions();
                #endregion

                IsInit = false;

                return new(await TaskCompletion.Task);
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
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
        #region Hide
        public async Task Hide()
        {
            var tcs = new TaskCompletionSource<object?>();

            var storyboard = new Storyboard();
            var ease = new PowerEase() { EasingMode = EasingMode.EaseInOut };

            var animationHideGrid = new DoubleAnimation(0, AnimationHelper.AnimationDuration) { EasingFunction = ease };
            Storyboard.SetTarget(animationHideGrid, middleGrid);
            Storyboard.SetTargetProperty(animationHideGrid, new PropertyPath(OpacityProperty));

            var animationHide = new DoubleAnimation(0, AnimationHelper.AnimationDuration) { EasingFunction = ease };
            animationHide.BeginTime = AnimationHelper.AnimationDuration;
            Storyboard.SetTarget(animationHide, this);
            Storyboard.SetTargetProperty(animationHide, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(animationHideGrid);
            storyboard.Children.Add(animationHide);

            storyboard.Completed += (_, __) => { tcs.SetResult(null); };

            storyboard.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true);

            await tcs.Task;
        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            MGBP_main.Text = Dictionary.Translate($"ОСНОВНЫЕ");
            MGBP_advanced.Text = Dictionary.Translate($"РАСШИРЕННЫЕ");
            MGPA_text.Text = Dictionary.Translate($"Данный раздел находиться в разработке");
            MGPM_redeem.Placeholder = Dictionary.Translate($"Погасить купон");
            MGPM_clear_cache.Text = Dictionary.Translate($"Очистить кэш");
            MGPM_change_password.Text = Dictionary.Translate($"Сменить пароль");
            MGPM_server_header.Text = Dictionary.Translate($"Сервер");
            MGPM_localization_header.Text = Dictionary.Translate($"Локализация");
        }
        #endregion
        #region UpdateUserPermissions
        private void UpdateUserPermissions()
        {
            // TODO: Реализовать обновление прав пользователя

            //var canChangeToggleEncrypt = GProp.User.Permissions.Contains($"shop.toggle_encrypt");
            //MGPM_toggle_encryption_panel.Visibility = canChangeToggleEncrypt ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion
        #region TryRedeemCupon
        private async Task TryRedeemCoupon()
        {
            var coupon = MGPM_redeem.Text.Trim();

            var tryRedeem = await CApi.RedeemCoupon(coupon);
            if (!tryRedeem.IsSuccess)
            {
                Main.Notify(Dictionary.Translate($"Купон не найден или уже был активирован"));
                return;
            }

            MGPM_redeem.Text = "";
            Main.Notify(Dictionary.Translate($"Купон успешно активирован"));
            await GProp.Update(ELauncherUpdate.User);
        }
        #endregion
        #region TryClearCache
        private async Task TryClearCache()
        {
            await Main.Loader(ELoaderState.Show);

            var tryRedeem = await CApi.ClearCache();
            if (!tryRedeem.IsSuccess)
            {
                Main.Notify(Dictionary.Translate($"Не удалось очистить кэш"));
                return;
            }

            Main.Notify(Dictionary.Translate($"Кэш успешно очищен"));

            await Main.Loader(ELoaderState.Hide);
        }
        #endregion
        #region ChangePanel
        private async void ChangePanel(EPC_Panels panel)
        {
            MGBP_main.IsActive = panel is EPC_Panels.Main;
            MGBP_advanced.IsActive = panel is EPC_Panels.Advanced;

            await PanelChanger.ChangePanel(panel);
        }
        #endregion
        #endregion




        
    }
}
