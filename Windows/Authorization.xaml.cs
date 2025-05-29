using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Api;
using Launcher.Api.Errors;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components;
using Launcher.Components.PanelChanger;
using Launcher.PanelChanger.Enums;
using Launcher.Settings;
using Launcher.Windows.AnyAuthorization.Enums;
using Launcher.Windows.AnyAuthorization.Errors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Launcher.Windows
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window, ITranslatable
    {
        public Authorization()
        {
            InitializeComponent();
            TranslationHub.Register(this);

            Loaded += ALoaded;
            //Closing += (a, e) => { Application.Current.Shutdown(); };
        }

        #region Переменные
        private static LogBox Pref { get; set; } = new LogBox("Authorization");
        private CPanelChanger<EPC_LoginReg> EPC_MainPanelChanger { get; set; }
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
        public async Task<UResponse> Inititalization()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить инициализацию";

            #region try
            try
            {
                #region Переключатель
                EPC_MainPanelChanger = new
                (
                    mainGrid,
                    [
                        new (EPC_LoginReg.Login, LoginPanel),
                        new (EPC_LoginReg.Registration, RegistrationPanel)
                    ],
                    EPC_LoginReg.Registration,
                    EPanelState.Showen
                );
                EPC_MainPanelChanger.ShowElement += PanelChangerShow;
                EPC_MainPanelChanger.HideElement += PanelChangerHide;
                var tryInitMainChanger = await EPC_MainPanelChanger.Init();
                if (!tryInitMainChanger.IsSuccess) throw new UExcept(EInitialization.FailInitPanelChanger, $"Ошибка запуска '{nameof(EPC_MainPanelChanger)}'", tryInitMainChanger.Error);
                #endregion
                #region Проверка источника регистрации
                if (GProp.ReferralSource is not null)
                {
                    RegistrationPanel.Children.Remove(RP_ref_code_input);
                }
                #endregion
                #region Установка списка языков
                var LangList = new List<CList.CListItem>();
                foreach (var lang in Enum.GetValues<ELang>())
                {
                    LangList.Add(new ((int)lang, Dictionary.GetLanguageName(lang)));
                }
                var tryLoadLangList = await CLP_list.LoadItems(LangList);
                if (!tryLoadLangList.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailLoadLangList, $"Не удалось загрузить языки в список", tryLoadLangList.Error);
                }
                CLP_list.NewSelectedItem += CLP_list_NewSelectedItem;
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
        #region Registration
        private async Task Registration()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось зарегистрировать пользователя";
            var hideLoader = true;

            #region try
            try
            {
                #region Кнопка
                RP_button_input.IsEnabled = false;
                #endregion
                #region Сбор и проверка данных
                #region Логин 
                var login = RP_login_input.Text.Trim();
                if (String.IsNullOrWhiteSpace(login)) throw new UExcept(ERegistration.NonComplianceData, "Логин не может быть пустым");
                #endregion
                #region Пароль 
                var password = RP_password_input.Text.Trim();
                var password_confirm = RP_repeat_password_input.Text.Trim();
                if (String.IsNullOrWhiteSpace(password)) throw new UExcept(ERegistration.NonComplianceData, "Укажите пароль");
                if (password.Length < 8) throw new UExcept(ERegistration.NonComplianceData, "Минимальная длина пароля 8 символов");
                if (String.IsNullOrWhiteSpace(password_confirm)) throw new UExcept(ERegistration.NonComplianceData, "Повторите пароль");
                if (password != password_confirm) throw new UExcept(ERegistration.NonComplianceData, "Пароли не совпадают");
                #endregion
                #region Создание объекта запроса
                var regData = new RegistrationRequestBody()
                {
                    Login = login,
                    Password = Functions.GetMd5Hash(password),
                };
                #endregion
                #region Наличие рефкода
                if (GProp.ReferralSource is null)
                {
                    regData.ReferralCode = String.IsNullOrWhiteSpace(RP_ref_code_input.Text) ? null : RP_ref_code_input.Text.Trim();
                }
                else
                {
                    regData.ReferralCode = GProp.ReferralSource.ReferralCode;
                    regData.RegisterSource = GProp.ReferralSource.RegisterSource;
                }
                #endregion
                #endregion
                #region Запрос
                await Loader(ELoaderState.Show);
                var tryReg = await CApi.Registration(regData);
                if (!tryReg.IsSuccess)
                {
                    if (tryReg.Error.Code is ERequest.BadRequest) Notify(Dictionary.Translate(tryReg.Error.Message));
                    else Notify(Dictionary.Translate("Ошибка регистрации. Обратитесь к администрации"));
                    return;
                }
                hideLoader = false;
                #endregion
                #region Сохранение данных
                CApi.Session = tryReg.Response.Session;
                AppSettings.Instance.Login = regData.Login;
                AppSettings.Instance.Password = regData.Password;
                AppSettings.Save();
                #endregion
                #region Открытие основного окна
                OpenMainWindow();
                #endregion               
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                if (ex.Code is ERegistration.NonComplianceData)
                {
                    Notify(Dictionary.Translate(ex.Message));
                }

                var uex = new UExcept(ERegistration.FailProcRegister, _failinf, ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
            }
            #endregion
            #region finally
            finally
            {
                if (hideLoader)
                {
                    RP_button_input.IsEnabled = true;
                    _ = Loader(ELoaderState.Hide);
                }
            }
            #endregion
        }
        #endregion        
        #region Login
        public async Task Login()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось войти в систему";
            var hideLoader = true;

            #region try
            try
            {
                #region Кнопка
                LP_button_input.IsEnabled = false;
                #endregion
                #region Сбор и проверка данных
                #region Логин 
                var login = LP_login_input.Text.Trim();
                if (String.IsNullOrWhiteSpace(login)) throw new UExcept(ELogin.NonComplianceData, "Логин не может быть пустым");
                #endregion
                #region Пароль 
                var password = LP_password_input.Text.Trim();
                if (String.IsNullOrWhiteSpace(password)) throw new UExcept(ELogin.NonComplianceData, "Укажите пароль");               
                #endregion
                #region Создание объекта запроса
                var loginData = new LoginRequestBody()
                {
                    Login = login,
                    Password = Functions.GetMd5Hash(password),
                };
                #endregion
                #endregion
                #region Запрос
                await Loader(ELoaderState.Show);
                var tryLogin = await CApi.Login(loginData);
                if (!tryLogin.IsSuccess)
                {
                    if (tryLogin.Error.Code is ERequest.BadRequest) Notify(Dictionary.Translate(tryLogin.Error.Message));
                    else Notify(Dictionary.Translate("Ошибка входа. Обратитесь к администрации"));
                    return;
                }
                #endregion
                #region Сохранение данных
                CApi.Session = tryLogin.Response.Session;
                AppSettings.Instance.Login = loginData.Login;
                AppSettings.Instance.Password = loginData.Password;
                AppSettings.Save();
                #endregion
                #region Открытие основного окна
                OpenMainWindow();
                #endregion
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                if (ex.Code is ELogin.NonComplianceData)
                {
                    Notify(Dictionary.Translate(ex.Message));
                }

                var uex = new UExcept(ELogin.FailProcLogin, _failinf, ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
            }
            #endregion
            #region finally
            finally
            {
                if (hideLoader)
                {
                    LP_button_input.IsEnabled = true;
                    _ = Loader(ELoaderState.Hide);
                }
            }
            #endregion
        }
        #endregion
        #region EnableButtonAndHideLoader
        public void EnableButtonAndHideLoader(CButton button)
        {
            button.IsEnabled = true;
            _ = Loader(ELoaderState.Hide);
        }
        #endregion        
        #region OpenMainWindow
        private void OpenMainWindow() => Application.Current.Dispatcher.Invoke(() => Functions.OpenWindow(this, new Main()));
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            LP_login_input.Placeholder = Dictionary.Translate("Логин");
            LP_password_input.Placeholder = Dictionary.Translate("Пароль");
            LP_button_input.Text = Dictionary.Translate("Авторизоваться");
            LP_change_to_sign.Content = $"{Dictionary.Translate("Регистрация")}?";

            RP_login_input.Placeholder = Dictionary.Translate("Логин");
            RP_password_input.Placeholder = Dictionary.Translate("Пароль");
            RP_repeat_password_input.Placeholder = Dictionary.Translate("Повторите пароль");
            RP_ref_code_input.Placeholder = Dictionary.Translate("Реферальный код");
            RP_button_input.Text = Dictionary.Translate("Зарегистрироваться");
            RP_change_to_login.Content = $"{Dictionary.Translate("Авторизация")}?";

            ChangeLangButton.Text = Dictionary.GetLanguageName(AppSettings.Instance.Language);
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
        public async Task Loader(ELoaderState newState)
        {
            var tcs = new TaskCompletionSource<object?>();            

            if (newState == ELoaderState.Show)
            {
                LoaderPanel.Visibility = Visibility.Visible;
                var animation = AnimationHelper.OpacityAnimationStoryBoard(LoaderPanel, 1);
                animation.Completed += (s, e) => tcs.SetResult(null);
                animation.Begin(LoaderPanel, HandoffBehavior.SnapshotAndReplace, true);
                LP_loader.StartAnimation();
            }
            else
            {
                var animation = AnimationHelper.OpacityAnimationStoryBoard(LoaderPanel, 0);
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
        #region Загрузка
        private async void ALoaded(object sender, RoutedEventArgs e)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());

            #region Инициализация
            var tryInit = await Inititalization();
            if (!tryInit.IsSuccess)
            {
                Functions.Error(tryInit.Error, $"Ошибка инициализации. Выход из приложения...", _proc);
                Application.Current.Shutdown();
            }
            #endregion
        }
        #endregion
        #region LP_change_to_sign_MouseDown
        private void LP_change_to_sign_MouseDown(object sender, MouseButtonEventArgs e)
        {

            _ = EPC_MainPanelChanger.ChangePanel(EPC_LoginReg.Registration);
        }
        #endregion
        #region RP_change_to_login_MouseDown
        private void RP_change_to_login_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ = EPC_MainPanelChanger.ChangePanel(EPC_LoginReg.Login);
        }
        #endregion
        #region RP_button_input_MouseDown
        private void RP_button_input_MouseDown(object sender, MouseButtonEventArgs e) => _ = Registration();
        #endregion
        #region CLP_list_NewSelectedItem
        private void CLP_list_NewSelectedItem(CList.CListItem item)
        {
            var newSelectedLanguage = (ELang)item.Id;

            #region Если выбран тот же язык
            if (newSelectedLanguage == AppSettings.Instance.Language)
            {
                var animation = AnimationHelper.OpacityAnimationStoryBoard(ChangeLanguagePanel, 0);
                animation.Completed += (s, e) => ChangeLanguagePanel.Visibility = Visibility.Hidden;
                animation.Begin(ChangeLanguagePanel, HandoffBehavior.SnapshotAndReplace, true);

                return;
            }
            #endregion
            #region Если другой язык
            var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
            var fadeOut = AnimationHelper.OpacityAnimationStoryBoard(this, 0);

            fadeOut.Completed += async (e, f) =>
            {
                AppSettings.Instance.Language = newSelectedLanguage;
                AppSettings.Save();
                await UpdateAllValues();
                ChangeLanguagePanel.Opacity = 0;
                ChangeLanguagePanel.Visibility = Visibility.Hidden;
                await Task.Run(() => Thread.Sleep(500));
                fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
            };

            fadeOut.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
            #endregion
        }
        #endregion
        #region ChangeLangButton_MouseDown
        private void ChangeLangButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeLanguagePanel.Visibility = Visibility.Visible;
            CLP_list.IsOpened = true;
            AnimationHelper.OpacityAnimationStoryBoard(ChangeLanguagePanel, 1).Begin(ChangeLanguagePanel, HandoffBehavior.SnapshotAndReplace, true);
        }
        #endregion
        #region LP_button_input_MouseDown
        private void LP_button_input_MouseDown(object sender, MouseButtonEventArgs e) => _ = Login();
        #endregion
        #region BackRectangle_MouseDown
        private void BackRectangle_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
        #endregion
        #region LP_password_input_KeyDown
        private void LP_password_input_KeyDown(object sender, KeyEventArgs e) { if (e.Key is Key.Enter) _ = Login(); }
        #endregion
        #region RP_repeat_password_input_KeyDown
        private void RP_repeat_password_input_KeyDown(object sender, KeyEventArgs e) { if (e.Key is Key.Enter) _ = Registration(); }
        #endregion
        #region RP_ref_code_input_KeyDown
        private void RP_ref_code_input_KeyDown(object sender, KeyEventArgs e) { if (e.Key is Key.Enter) _ = Registration(); }
        #endregion
        #endregion

    }
}
