using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Any.UDialogBox;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.MainWindow.ChangePasswordDialogBoxAny;
using Launcher.Settings;
using Launcher.Windows;
using Launcher.Windows.AnyAuthorization.Errors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Launcher.Components.MainWindow
{
    namespace ChangePasswordDialogBoxAny
    {
        public enum EChangePasswordDialogBox
        {
            FailChangePassword,
        }

        public enum EChangePassword
        {
            FailExecuteRequest
        }
    }
    /// <summary>
    /// Логика взаимодействия для CChangePasswordDialogBox.xaml
    /// </summary>
    public partial class CChangePasswordDialogBox : UserControl, IUDialogBox, ITranslatable
    {
        public CChangePasswordDialogBox()
        {
            InitializeComponent();
        }

        #region Переменные
        private LogBox Pref { get; set; } = new("Change Password Dialog Box");
        private TaskCompletionSource<EDialogResponse> TaskCompletionS { get; set; }
        #endregion

        #region Обработчики событий
        #region MG_close_button_MouseLeftButtonDown
        private void MG_close_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TaskCompletionS?.TrySetResult(EDialogResponse.Closed);
        }
        #endregion
        #region LP_password_input_KeyDown
        private void LP_password_input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Tab || e.Key is Key.Enter) RP_repeat_password_input.Focus(); 
        }
        #endregion
        #region RP_repeat_password_input_KeyDown
        private void RP_repeat_password_input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter) PrepareToChangePassword();
        }
        #endregion
        #region MG_save_MouseLeftButtonDown
        private void MG_save_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => PrepareToChangePassword();
        #endregion
        #region Background_MouseDown
        private void Background_MouseDown(object sender, MouseButtonEventArgs e) => Application.Current.Windows.OfType<Main>().FirstOrDefault()?.DragMove();
        #endregion
        #endregion

        #region Функции
        #region Show
        public async Task<UResponse<EDialogResponse>> Show(params object[] pars)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Ошибка при показе окна";

            #region try
            try
            {
                #region Задача
                TaskCompletionS = new();
                #endregion
                #region Появление окна
                var fadeInMiddle = AnimationHelper.OpacityAnimationStoryBoard(middleGrid, 1);
                var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                fadeIn.Completed += (s, e) => { fadeInMiddle.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true); };
                fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                #endregion                
                #region Обновление текстов
                await UpdateAllValues();
                #endregion

                return new(await TaskCompletionS.Task);
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
            MGH_value.Text = Dictionary.Translate($"Смена пароля");
            LP_password_input.Placeholder = Dictionary.Translate("Пароль");
            RP_repeat_password_input.Placeholder = Dictionary.Translate("Повторите пароль");
        }
        #endregion
        #region PrepareToChange
        private void PrepareToChangePassword() =>
            _ = ChangePassword
            (
                LP_password_input.Text.Trim(),
                RP_repeat_password_input.Text.Trim()
            );
        #endregion
        #region ChangePassword
        private async Task ChangePassword(string? password, string? passowrdConfirmation)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось изменить пароль";

            #region try
            try
            {
                #region Кнопка
                MG_save.IsEnabled = false;
                #endregion
                #region Проверки
                if (String.IsNullOrWhiteSpace(password)) throw new UExcept(ERegistration.NonComplianceData, Dictionary.Translate("Укажите пароль"));
                if (password.Length < 8) throw new UExcept(ERegistration.NonComplianceData, Dictionary.Translate("Минимальная длина пароля 8 символов"));
                if (String.IsNullOrWhiteSpace(passowrdConfirmation)) throw new UExcept(ERegistration.NonComplianceData, Dictionary.Translate("Повторите пароль"));
                if (password != passowrdConfirmation) throw new UExcept(ERegistration.NonComplianceData, Dictionary.Translate("Пароли не совпадают"));
                #endregion
                #region Лоадер
                await Main.Loader(ELoaderState.Show);
                #endregion
                #region Отправка запроса
                var tryChangePass = await CApi.ChangePassword(Functions.GetMd5Hash(password));
                if (!tryChangePass.IsSuccess)
                {                    
                    throw new UExcept(EChangePassword.FailExecuteRequest, $"Ошибка выполнения запроса", tryChangePass.Error);
                }
                #endregion
                #region Закрываем окно
                TaskCompletionS.TrySetResult(EDialogResponse.Ok);
                await Task.Run(() => Thread.Sleep(300));
                #endregion
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Main.Notify(Dictionary.Translate(ex.Message));

                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                Main.Notify(Dictionary.Translate($"Возникла внутренняя ошибка при смене пароля. Попробуйте позже"));

                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                var glerror = new UExcept(EChangePasswordDialogBox.FailChangePassword, $"{_failinf}: исключение", uex);
                Functions.Error(glerror, $"{_failinf}: исключение", _proc);                
            }
            #endregion
            #region finally
            finally
            {
                MG_save.IsEnabled = true;
                await Main.Loader(ELoaderState.Hide);
            }
            #endregion
        }
        #endregion

        #endregion

        
    }
}
