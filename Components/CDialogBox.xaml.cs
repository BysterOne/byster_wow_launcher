using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Components.DialogBox;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Launcher.Components
{
    namespace DialogBox
    {
        public enum EResponse { Yes, No, Cancel, Ok }
        public enum EShow { Exception }
        public record DialogButton(EResponse Response, string Text = "");
        public record BoxSettings(string Header, string Message, List<DialogButton> Buttons)
        {
            public BoxSettings(string message, List<DialogButton> buttons) : this("", message, buttons) { }
        }
    }
    /// <summary>
    /// Логика взаимодействия для DialogBox.xaml
    /// </summary>
    public partial class CDialogBox : UserControl
    {
        public CDialogBox()
        {
            InitializeComponent();
            Opacity = 0;
            middleGrid.Opacity = 0;
        }

        #region Переменные
        private LogBox Pref { get; set; } = new("Dialog Box");
        private TaskCompletionSource<EResponse> TaskCompletion { get; set; }
        #endregion

        #region Свойства
        #region Background
        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static new readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(CDialogBox),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));
        #endregion
        #endregion

        #region Функции
        #region Show
        public async Task<UResponse<EResponse>> Show(BoxSettings settings)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось показать диалоговое окно";

            #region try
            try
            {
                #region Устанавливаем данные
                #region Заголовок
                if (String.IsNullOrWhiteSpace(settings.Header))
                {
                    headerBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    headerBlock.Visibility = Visibility.Visible;
                    header.Text = settings.Header;
                }
                #endregion
                #region Текст
                message.Text = settings.Message;
                #endregion
                #region Кнопки
                buttons.Children.Clear();
                foreach (var button in settings.Buttons)
                {
                    var bComponent = new CButton();
                    bComponent.Margin = new Thickness(5, 0, 5, 0);
                    bComponent.MinWidth = 100;
                    bComponent.Padding = new Thickness(10, 5, 10, 5);
                    bComponent.BorderRadius = new CornerRadius(20);

                    bComponent.Text =
                        !String.IsNullOrWhiteSpace(button.Text) ?
                        button.Text.Trim() :
                        button.Response switch
                        {
                            EResponse.Yes => Dictionary.Translate("Да"),
                            EResponse.No => Dictionary.Translate("Нет"),
                            EResponse.Cancel => Dictionary.Translate("Отмена"),
                            _ => ""
                        };
                    bComponent.Tag = button.Response;
                    bComponent.PreviewMouseLeftButtonDown += FResponseButtonClicked;
                    buttons.Children.Add(bComponent);
                }
                #endregion
                #endregion
                #region Анимация появления
                await Dispatcher.InvokeAsync(() =>
                {
                    var fadeInMiddle = AnimationHelper.OpacityAnimationStoryBoard(middleGrid, 1);
                    var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                    fadeIn.Completed += (s, e) => { fadeInMiddle.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true); };
                    fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                });
                #endregion
                #region Создание задачи и ожидание ответа
                TaskCompletion = new TaskCompletionSource<EResponse>();
                var result = await TaskCompletion.Task;
                return new(result);
                #endregion
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
                var uerror = new UError(EShow.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region Hide
        public async Task Hide()
        {
            var ts = new TaskCompletionSource<object>();
            await Dispatcher.InvokeAsync(() =>
            {
                var fadeOut = AnimationHelper.OpacityAnimationStoryBoard(this, 0);
                fadeOut.Completed += (s, e) => { ts.SetResult(true); };               

                var fadeOutMiddle = AnimationHelper.OpacityAnimationStoryBoard(middleGrid, 0);
                fadeOutMiddle.Completed += (s, e) => { fadeOut.Begin(this, HandoffBehavior.SnapshotAndReplace, true); };
                fadeOutMiddle.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
            });
            await ts.Task;
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region ButtonClicked
        private void FResponseButtonClicked(object sender, MouseButtonEventArgs e)
        {
            if (TaskCompletion is not null) { TaskCompletion.SetResult((EResponse)((CButton)sender).Tag); }
        }
        #endregion
        #endregion
    }
}
