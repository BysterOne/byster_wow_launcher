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
using Launcher.Settings;
using Launcher.Settings.Enums;
using Launcher.Windows;
using System.Diagnostics;
using System.Drawing;
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
            FailLoadServersList,
            FailLoadBranchList
        }

        public enum EToggle
        {
            Compilation,
            Protection,
            Encryption
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
            GProp.LauncherDataUpdatedEvent += ELauncherUpdatedEvent;
            Application.Current.Windows.OfType<Main>().First().PreviewKeyDown += EKeyDown;

            this.Opacity = 0;
            this.middleGrid.Opacity = 0;
        }       

        #region Переменные
        private bool AvailableAdvanced
        { 
            get => 
                GProp.User.Permissions.HasFlag(EUserPermissions.ExternalDeveloper) ||
                GProp.User.Permissions.HasFlag(EUserPermissions.Superuser);
        }
        private bool IsInit { get; set; } = false;
        private static LogBox Pref { get; set; } = new LogBox("SettingsDialogBox");
        private static TaskCompletionSource<EDialogResponse> TaskCompletion { get; set; } = null!;
        private CPanelChanger<EPC_Panels> PanelChanger { get; set; }
        #endregion

        #region Обработчики событий
        #region EKeyDown
        private void EKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape) 
            {
                e.Handled = true;
                Application.Current.Windows.OfType<Main>().First().PreviewKeyDown -= EKeyDown;
                TaskCompletion?.TrySetResult(EDialogResponse.Closed);
            }
        }
        #endregion
        #region ELauncherUpdatedEvent
        private async Task ELauncherUpdatedEvent(ELauncherUpdate updates, object? data)
        {
            if (updates.HasFlag(ELauncherUpdate.User))
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    UpdateUserPermissions();

                    MGPMCP_value.SelectedIndex = GProp.User.Compilation ? 1 : 0;
                    MGPMTEP_value.SelectedIndex = GProp.User.Encryption ? 1 : 0;
                    MGPMVP_value.SelectedIndex = GProp.User.Protection ? 1 : 0;
                });
            }
        }
        #endregion
        #region Background_MouseDown
        private void Background_MouseDown(object sender, MouseButtonEventArgs e) => Application.Current.Windows.OfType<Main>().FirstOrDefault()?.DragMove();
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
        #region EBranchList_NewSelectedItem
        private void EBranchList_NewSelectedItem(CList.CListItem item)
        {
            var branch = GProp.User.Branches[(int)item.Id];

            if (branch.ToLower() == AppSettings.Instance.Branch) return;

            AppSettings.Instance.Branch = branch;
            AppSettings.Save();
        }
        #endregion
        #region EEncryption_SelectedIndexChanged
        private void EEncryption_SelectedIndexChanged(object sender, int newIndex) => _ = TryChangeToggle(EToggle.Encryption, newIndex);
        #endregion
        #region EProtection_SelectedIndexChanged
        private void EProtection_SelectedIndexChanged(object sender, int newIndex) => _ = TryChangeToggle(EToggle.Protection, newIndex);
        #endregion
        #region ECompilation_SelectedIndexChanged
        private void ECompilation_SelectedIndexChanged(object sender, int newIndex) => _ = TryChangeToggle(EToggle.Compilation, newIndex);
        #endregion
        #region MGPM_admin_panel_button_MouseLeftButtonDown
        private void MGPM_admin_panel_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = $"https://admin.byster.one/", UseShellExecute = true });
        }
        #endregion
        #region MGPM_change_password_MouseDown
        private void MGPM_change_password_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ = Main.ShowModal(new CChangePasswordDialogBox());
        }
        #endregion
        #region EConsole_SelectedIndexChanged
        private void EConsole_SelectedIndexChanged(object sender, int newIndex)
        {
            var newValue = newIndex is 1;

            if (newValue == AppSettings.Instance.Console) return;
            AppSettings.Instance.Console = newValue;
            AppSettings.Save();
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
                #region Консоль
                MGPMTC_value.SetSelectedIndexFast(AppSettings.Instance.Console ? 1 : 0);
                MGPMTC_value.SelectedIndexChanged += EConsole_SelectedIndexChanged;
                #endregion
                #region Шифрование
                MGPMTEP_value.SetSelectedIndexFast(GProp.User.Encryption ? 1 : 0);
                MGPMTEP_value.SelectedIndexChanged += EEncryption_SelectedIndexChanged;
                #endregion
                #region Protection
                MGPMVP_value.SetSelectedIndexFast(GProp.User.Protection ? 1 : 0);
                MGPMVP_value.SelectedIndexChanged += EProtection_SelectedIndexChanged;
                #endregion
                #region Компиляция
                MGPMCP_value.SetSelectedIndexFast(GProp.User.Compilation ? 1 : 0);
                MGPMCP_value.SelectedIndexChanged += ECompilation_SelectedIndexChanged;
                #endregion
                #region Обновление разрешения пользователя
                UpdateUserPermissions();
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
                #region Установка списка веток
                var branchesList = new List<CList.CListItem>();
                foreach (var branch in GProp.User.Branches)
                {
                    branchesList.Add(new(GProp.User.Branches.IndexOf(branch), branch.ToUpper()));
                }
                var selected =
                    GProp.User.Branches.Any(x => x.Equals(AppSettings.Instance.Branch, StringComparison.CurrentCultureIgnoreCase)) ?
                    branchesList.IndexOf(branchesList.First(x => x.Name.Equals(AppSettings.Instance.Branch, StringComparison.CurrentCultureIgnoreCase))) :
                    0;

                var tryLoadBranchListTask =
                    branchesList.Count > 0 ?
                    MGPM_branch_list.LoadItems(branchesList, selected) :
                    new Task(() => { return; });
                #endregion
                #region Ожидаем завершения задача
                await Task.WhenAll
                (
                    tryInitPanelChangerTask,
                    tryLoadLangListTask,
                    tryLoadServerListTask,
                    tryLoadBranchListTask
                );
                #endregion
                #region Обработчик ответов
                #region Панели
                var tryInitPanelChanger = await tryInitPanelChangerTask;
                if (!tryInitPanelChanger.IsSuccess)
                {
                    throw new UExcept(EShow.FailInitPanelChanger, $"Ошибка инициализации панели {nameof(PanelChanger)}");
                }
                #endregion
                #region Список доступных языков
                var tryLoadLangList = await tryLoadLangListTask;
                if (!tryLoadLangList.IsSuccess)
                {
                    throw new UExcept(EShow.FailLoadLangList, $"Не удалось загрузить языки в список", tryLoadLangList.Error);
                }
                MGPM_localization_list.NewSelectedItem += ELocalizationList_NewSelectedItem;
                #endregion
                #region Список доступных серверов
                var tryLoadServerList = await tryLoadServerListTask;
                if (!tryLoadServerList.IsSuccess)
                {
                    throw new UExcept(EShow.FailLoadServersList, $"Не удалось загрузить сервера в список", tryLoadServerList.Error);
                }
                MGPM_server_list.NewSelectedItem += EServerList_NewSelectedItem;
                #endregion
                #region Список доступных веток
                if (tryLoadBranchListTask is Task<UResponse> task)
                {
                    var tryLoadBranchList = await task;
                    if (!tryLoadBranchList.IsSuccess)
                    {
                        throw new UExcept(EShow.FailLoadBranchList, $"Не удалось загрузить ветки в список", tryLoadBranchList.Error);
                    }
                    MGPM_branch_list.NewSelectedItem += EBranchList_NewSelectedItem;
                }
                #endregion
                #endregion
                #region Если доступны расширенные
                if (AvailableAdvanced)
                {
                    MGPA_git.Width = 150;
                    MGPA_git.Visibility = Visibility.Visible;
                    _ = MGPA_git.Initialization();
                }
                #endregion

                IsInit = false;

                return new(await TaskCompletion.Task);
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
            MGHH_value.Text = Dictionary.Translate($"Настройки");
            MGPMCP_header.Text = Dictionary.Translate($"Компиляция");
            MGPMTC_header.Text = Dictionary.Translate($"Консоль");
            MGPMVP_header.Text = Dictionary.Translate($"Защита");
            MGHBP_main.Text = Dictionary.Translate($"ОСНОВНЫЕ");
            MGHBP_advanced.Text = Dictionary.Translate($"РАСШИРЕННЫЕ");
            MGPM_redeem.Placeholder = Dictionary.Translate($"Активировать купон");
            MGPM_clear_cache.Text = Dictionary.Translate($"Очистить кэш");
            MGPM_change_password.Text = Dictionary.Translate($"Сменить пароль");
            MGPM_server_header.Text = Dictionary.Translate($"Сервер");
            MGPM_branch_header.Text = Dictionary.Translate($"Ветка");
            MGPM_localization_header.Text = Dictionary.Translate($"Локализация");
        }
        #endregion
        #region UpdateUserPermissions
        private void UpdateUserPermissions()
        {
            ///GProp.User.Permissions = EUserPermissions.None;
            var perms = GProp.User.Permissions;
            

            #region Выбор сервера
            var AvailableChooseServer =
                perms.HasFlag(EUserPermissions.Tester) ||
                perms.HasFlag(EUserPermissions.ExternalDeveloper) ||
                perms.HasFlag(EUserPermissions.Superuser);
            MGPM_server_panel.Visibility = AvailableChooseServer ? Visibility.Visible : Visibility.Collapsed;
            #endregion
            #region Расширенные настройки
            var AvailableAdvanced =
                GProp.User.Permissions.HasFlag(EUserPermissions.ExternalDeveloper) ||
                perms.HasFlag(EUserPermissions.Superuser);
            MGH_buttons_panel.Visibility = AvailableAdvanced ? Visibility.Visible : Visibility.Collapsed;
            MGH_header.Visibility = AvailableAdvanced ? Visibility.Collapsed : Visibility.Visible;
            #endregion
            #region Консоль
            var AvailableToggleConsole =
                perms.HasFlag(EUserPermissions.Tester) ||
                perms.HasFlag(EUserPermissions.ExternalDeveloper) ||
                perms.HasFlag(EUserPermissions.Superuser);
            MGPM_toggle_console.Visibility = AvailableToggleConsole ? Visibility.Visible : Visibility.Collapsed;
            #endregion
            #region Шифрование
            var AvailableToggleEncrypt = 
                perms.HasFlag(EUserPermissions.ToggleEncrypt) ||
                perms.HasFlag(EUserPermissions.Superuser);
            MGPM_toggle_encryption_panel.Visibility = AvailableToggleEncrypt ? Visibility.Visible : Visibility.Collapsed;
            #endregion
            #region Компиляция
            var AvailableToggleCompilation =
                perms.HasFlag(EUserPermissions.CanToggleCompilation) ||
                perms.HasFlag(EUserPermissions.Superuser);
            MGPM_compilation_panel.Visibility = AvailableToggleCompilation ? Visibility.Visible : Visibility.Collapsed;
            #endregion
            #region Защита
            var AvailableToggleProtection =
                perms.HasFlag(EUserPermissions.CanToggleProtection) ||
                perms.HasFlag(EUserPermissions.Superuser);
            MGPM_protection_panel.Visibility = AvailableToggleProtection ? Visibility.Visible : Visibility.Collapsed;
            #endregion
            #region Ветки
            var AvailableBranches = GProp.User.Branches.Count > 1;
            MGPM_branch_panel.Visibility = AvailableBranches ? Visibility.Visible : Visibility.Collapsed;
            #endregion
            #region Админ панель
            var AvailableAdminPanel =
                perms.HasFlag(EUserPermissions.AdminSiteAccess) ||
                perms.HasFlag(EUserPermissions.Superuser);
            MGPM_admin_panel_button.Visibility = AvailableAdminPanel ? Visibility.Visible : Visibility.Collapsed;
            #endregion
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
                await Main.Loader(ELoaderState.Hide);
                return;
            }

            Main.Notify(Dictionary.Translate($"Кэш успешно очищен"));
            await Main.Loader(ELoaderState.Hide);
        }
        #endregion
        #region TryChangeToggle
        private async Task TryChangeToggle(EToggle toggle, int newValue)
        {
            await Main.Loader(ELoaderState.Show);

            var tryExecute = toggle switch
            {
                EToggle.Compilation => await CApi.ToggleEncryption(newValue is 1),
                EToggle.Protection => await CApi.ToggleProtection(newValue is 1),
                EToggle.Encryption => await CApi.ToggleEncryption(newValue is 1),
                _ => throw new NotImplementedException()
            };
            if (!tryExecute.IsSuccess)
            {
                switch (toggle)
                {
                    case EToggle.Compilation: MGPMCP_value.SelectedIndex = GProp.User.Compilation ? 1 : 0; break;
                    case EToggle.Protection: MGPMVP_value.SelectedIndex = GProp.User.Protection ? 1 : 0; break;
                    case EToggle.Encryption: MGPMTEP_value.SelectedIndex = GProp.User.Encryption ? 1 : 0; break;
                }

                Main.Notify(Dictionary.Translate($"Не удалось переключить"));
                await Main.Loader(ELoaderState.Hide);
                return;
            }

            _ = GProp.Update(ELauncherUpdate.User, tryExecute.Response);
            _ = Main.Loader(ELoaderState.Hide);
        }
        #endregion
        #region ChangePanel
        private async void ChangePanel(EPC_Panels panel)
        {
            MGHBP_main.IsActive = panel is EPC_Panels.Main;
            MGHBP_advanced.IsActive = panel is EPC_Panels.Advanced;
            if (panel is EPC_Panels.Advanced && AvailableAdvanced)
            {
                _ = MGPA_git.StartWork();
                MGPA_git.Width = double.NaN;
            }

            await PanelChanger.ChangePanel(panel);

            if (panel is EPC_Panels.Main) MGPA_git.Width = 150;
        }
        #endregion

        #endregion

        
    }
}
