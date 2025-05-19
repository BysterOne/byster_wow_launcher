using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.UDialogBox;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using Launcher.Windows;
using Launcher.Windows.AnyMain.Enums;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Launcher.Components.MainWindow.Any.PageMain
{
    /// <summary>
    /// Логика взаимодействия для CAddServerDialogBox.xaml
    /// </summary>
    public partial class CAddServerDialogBox : UserControl, IUDialogBox, ITranslatable
    {
        public CAddServerDialogBox()
        {
            InitializeComponent();
            TranslationHub.Register(this);

            this.middleGrid.Opacity = 0;
            this.Opacity = 0;
        }

        #region Переменные
        public static LogBox Pref { get; set; } = new("Add Server Dialog Box");
        private TaskCompletionSource<EDialogResponse> TaskCompletionSource { get; set; } = new();
        private List<CFilterChanger> IconsChangers { get; set; } = [];
        private CFilterChanger SelectedIcon { get; set; }
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
                #region Появление окна
                var fadeInMiddle = AnimationHelper.OpacityAnimationStoryBoard(middleGrid, 1);
                var fadeIn = AnimationHelper.OpacityAnimationStoryBoard(this, 1);
                fadeIn.Completed += (s, e) => { fadeInMiddle.Begin(middleGrid, HandoffBehavior.SnapshotAndReplace, true); };
                fadeIn.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                #endregion
                #region Установка иконок
                SetIcons();
                #endregion
                #region Обновление текстов
                await UpdateAllValues();
                #endregion

                return new(await TaskCompletionSource.Task);
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
        #region SetIcons
        private void SetIcons()
        {
            IconsChangers.Clear();
            MGCI_icons_panel.Children.Clear();

            foreach (var icon in Enum.GetValues<EServerIcon>())
            {
                var imageSource = GStatic.GetServerIcon(icon);
                var changer = new CFilterChanger()
                {
                    Text = "",
                    ChangerType = EFilterChangerType.Icon,
                    MarkerHeight = 50,
                    MarkerWidth = 50,
                    Icon = imageSource,
                    IconBorderRadius = 50,
                    IsAutoChangeState = false,
                    IconBorderThickness = 3,
                    Value = icon,
                    Margin = new Thickness(5)
                };
                changer.Clicked += EIconChangerClicked;
                IconsChangers.Add(changer);
                MGCI_icons_panel.Children.Add(changer);
            }

            SelectedIcon = IconsChangers[0];
            SelectedIcon.IsActive = true;
        }
        #endregion
        #region AddServer
        private void AddServer(string? name, string? path, EServerIcon? icon)
        {
            #region Проверки
            if (String.IsNullOrWhiteSpace(name)) { Main.Notify(Dictionary.Translate("Укажите название клиента")); return; }
            if (String.IsNullOrWhiteSpace(path)) { Main.Notify(Dictionary.Translate("Укажите путь к wow.exe")); return; }
            if (!File.Exists(path)) { Main.Notify(Dictionary.Translate("Выбранный файл не найден")); return; }            
            if (icon is null) { Main.Notify(Dictionary.Translate("Выберите иконку из доступных")); return; }
            if (AppSettings.Instance.Servers.Any(x => x.PathToExe == path)) { Main.Notify(Dictionary.Translate("Выбранный клиент уже добавлен")); return; }
            #endregion
            #region Сохранение
            var server = new CServer()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name!,
                PathToExe = path!,
                Icon = (EServerIcon)icon!
            };
            AppSettings.Instance.Servers.Add(server);
            AppSettings.Save();
            #endregion
            #region Выбор нового сервера как основного
            GProp.SelectedServer = server;
            #endregion
            #region Скрытие
            TaskCompletionSource.SetResult(EDialogResponse.Ok);
            #endregion
        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            MGH_value.Text = Dictionary.Translate("Добавление клиента");
            MGC_server_name.Placeholder = Dictionary.Translate("Название");
            MGC_path.Text = Dictionary.Translate("Путь к WoW.exe");
            MGCI_header.Text = Dictionary.Translate("Выберите иконку");
            MGC_save.Text = Dictionary.Translate("Сохранить");
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region EIconChangerClicked
        private void EIconChangerClicked(CFilterChanger sender, bool newValue)
        {
            if ((EServerIcon)sender.Value == (EServerIcon)SelectedIcon.Value) return;

            if (!sender.IsActive) sender.IsActive = true;            
            SelectedIcon.IsActive = false;
            SelectedIcon = sender;
        }
        #endregion
        #region MG_close_button_MouseLeftButtonDown
        private void MG_close_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TaskCompletionSource.SetResult(EDialogResponse.Closed);
        }
        #endregion       
        #region MGC_select_path_MouseLeftButtonDown
        private void MGC_select_path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = Dictionary.Translate("Выберите исполняемый файл wow.exe"),
                Filter = $"{Dictionary.Translate("Исполняемые файлы")} (*.exe)|*.exe",
                DefaultExt = ".exe",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dlg.ShowDialog() == true) MGC_path.Text = dlg.FileName;
        }
        #endregion
        #region MGC_save_MouseLeftButtonDown
        private void MGC_save_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => 
            AddServer
            (
                MGC_server_name.Text.Trim(),
                MGC_path.Text.Trim(),
                (EServerIcon)SelectedIcon.Value
            );
        #endregion
        #endregion


    }
}
