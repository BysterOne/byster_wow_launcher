using Cls;
using Cls.Any;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Api;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components.MainWindow.GitControlAny;
using Launcher.Components.PanelChanger;
using Launcher.Settings;
using Launcher.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Launcher.Components.MainWindow
{
    namespace GitControlAny
    {
        #region enums
        #region EGitControl
        public enum EGitControl
        {
            FailInitialization,
            FailStartWork,
            FailSelectFolder
        }
        #endregion
        #region EInitialization
        public enum EInitialization
        {
            FailInitPanelChanger
        }
        #endregion
        #region EPanelChanger
        public enum EPanelChanger
        {
            Loader,
            Content
        }
        #endregion
        #region EStartWork
        public enum EStartWork
        {
            FailLoadList
        }
        #endregion
        #region EStartStatus
        public enum EStartStatus 
        {
            NotStarted,
            Starting,
            ErrorOccurred,
            Started
        }
        #endregion
        #region EUpdateRepList
        public enum EUpdateRepList
        {
            FailLoadDirs
        }
        #endregion
        #region ESelectFolder
        public enum ESelectFolder
        {
            FailLoadList
        }
        #endregion
        #endregion
    }
    /// <summary>
    /// Логика взаимодействия для CGitDirectories.xaml
    /// </summary>
    public partial class CGitDirectories : UserControl, ITranslatable
    {
        public CGitDirectories()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region Переменные
        private EStartStatus StartStatus { get; set; } = EStartStatus.NotStarted;        
        private static LogBox Pref { get; set; } = new("Git Control");
        private CPanelChanger<EPanelChanger> PanelChanger { get; set; }
        public ObservableCollection<CGitDirectory> Repositories { get; set; } = [];
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

        #region Обработчики событий
        #region TPSFP_button_MouseLeftButtonDown
        private void TPSFP_button_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => _ = SelectFolder();
        #endregion
        #endregion

        #region Функции
        #region Инициализация
        public async Task Initialization()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось инициализировать компонент";

            #region try
            try
            {
                #region Переключатель панелей
                PanelChanger = new
                (
                    MainGrid,
                    [
                        new(EPanelChanger.Loader, Preloader),
                        new(EPanelChanger.Content, ContentPanel)
                    ],
                    EPanelChanger.Content,
                    IsHitTestMonitor: false
                );
                PanelChanger.ShowElement += PanelChangerShow;
                PanelChanger.HideElement += PanelChangerHide;
                var tryInitPanel = await PanelChanger.Init();
                if (!tryInitPanel.IsSuccess)
                {
                    throw new UExcept(EInitialization.FailInitPanelChanger, $"Не удалось инициализировать переключатель панелей", tryInitPanel.Error);
                }
                #endregion
                #region Обновление шаблона
                UpdateView();
                #endregion
                #region Обновление языка
                await UpdateAllValues();
                #endregion
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var glerror = new UExcept(EGitControl.FailInitialization, _failinf, ex);
                Functions.Error(ex, glerror.Message, _proc);
            }
            #endregion
        }
        #endregion
        #region SelectFolder
        private async Task SelectFolder()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выбрать рабочую папку";

            #region try
            try
            {
                var dlg = new OpenFolderDialog
                {
                    Title = Dictionary.Translate("Рабочая папка"),
                    DefaultDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    RootDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    Multiselect = false
                };

                if (dlg.ShowDialog() == true)
                {
                    var isEmpty = String.IsNullOrEmpty(AppSettings.Instance.WorkDirectory);
                    AppSettings.Instance.WorkDirectory = dlg.FolderName;
                    AppSettings.Save();
                    if (isEmpty) 
                    {
                        await Loader(ELoaderState.Show);
                        #region Обновляем вид
                        UpdateView();
                        #endregion
                        #region Грузим список
                        var tryLoadList = await UpdateRepsList();
                        if (!tryLoadList.IsSuccess)
                        {
                            throw new UExcept(ESelectFolder.FailLoadList, $"Не удалось загрузить список", tryLoadList.Error);
                        }
                        #endregion
                    }
                }
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(EGitControl.FailSelectFolder, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);
            }
            #endregion
            #region finally
            finally
            {
                await Loader(ELoaderState.Hide);
            }
            #endregion
        }
        #endregion
        #region UpdateView
        private void UpdateView()
        {
            var isFullState = !String.IsNullOrWhiteSpace(AppSettings.Instance.WorkDirectory) && Directory.Exists(AppSettings.Instance.WorkDirectory);

            SelectDirectory.Visibility = isFullState ? Visibility.Collapsed : Visibility.Visible;
            CenterPanel.Visibility = isFullState ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion
        #region UpdateAllValues
        public async Task UpdateAllValues()
        {
            TPSFP_text.Text = Dictionary.Translate("Для начала работы выберите папку, где будут храниться директории");
            if (String.IsNullOrWhiteSpace(AppSettings.Instance.WorkDirectory)) TPSFP_path.Text = Dictionary.Translate("Выберите папку");
        }
        #endregion
        #region Loader
        private async Task Loader(ELoaderState state)
        {
            if (state is ELoaderState.Show)
            {
                Preloader.StartAnimation();
                await PanelChanger.ChangePanel(EPanelChanger.Loader);
            }
            else
            {
                await PanelChanger.ChangePanel(EPanelChanger.Content);
                Preloader.StopAnimation();
            }
        }
        #endregion
        #region StartWork
        public async Task StartWork()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось начать работу компонента";

            #region try
            try
            {
                #region Если уже запущено то ничего не делаем
                if 
                (
                    StartStatus is EStartStatus.Starting ||
                    StartStatus is EStartStatus.Starting ||
                    StartStatus is EStartStatus.Started
                ) 
                return;
                #endregion
                #region Если директория еще не выбрана
                if (String.IsNullOrWhiteSpace(AppSettings.Instance.WorkDirectory)) return;
                #endregion
                #region Loader
                await Loader(ELoaderState.Show);
                #endregion
                #region Грузим и ставим список
                var tryLoadList = await UpdateRepsList();
                if (!tryLoadList.IsSuccess)
                {
                    throw new UExcept(EStartWork.FailLoadList, $"Не удалось загрузить список", tryLoadList.Error);
                }
                #endregion


                StartStatus = EStartStatus.Started;
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(EGitControl.FailStartWork, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);
                ShowError(Dictionary.Translate($"При запуске компонента произошла ошибка. Детали в логах"));
                StartStatus = EStartStatus.ErrorOccurred;
            }
            #endregion
            #region finally
            finally
            {
                await Loader(ELoaderState.Hide);
            }
            #endregion
        }
        #endregion
        #region UpdateRepsList
        private async Task<UResponse> UpdateRepsList()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось обновить и установить список";

            #region try
            try
            {
                #region Загрузка списка директорий
                var tryGetDirs = await CApi.GetGitDirectories();
                if (!tryGetDirs.IsSuccess)
                {
                    throw new UExcept(EUpdateRepList.FailLoadDirs, $"Не удалось загрузить список репозиториев", tryGetDirs.Error);
                }
                GProp.GitRepositories = tryGetDirs.Response.OrderBy(x => x.Name).ToList();
                #endregion
                #region Добавление в список
                Functions.SyncCollections(Repositories, GProp.GitRepositories, (x) => x.Id);
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
                return new(new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex));
            }
            #endregion
        }
        #endregion
        #region ShowError
        private void ShowError(string error)
        {
            _ = Main.ShowModal(new DialogBox.BoxSettings
                (
                    Dictionary.Translate("Ошибка"), 
                    error,
                    [
                        new(DialogBox.EResponse.Ok)
                    ]
                ));
        }
        #endregion

        #endregion
    }
}
