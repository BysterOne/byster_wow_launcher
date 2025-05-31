using Cls;
using Cls.Any;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.CGitHelperAny;
using Launcher.Any.GlobalEnums;
using Launcher.Api;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components.DialogBox;
using Launcher.Components.MainWindow.GitControlAny;
using Launcher.Components.PanelChanger;
using Launcher.Settings;
using Launcher.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            FailSelectFolder,
            FailSyncAll,
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
        #region ESyncAll
        public enum ESyncAll
        {
            FailSendRequest
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
            this.MGPACP_search.TextChangedDelay = TimeSpan.FromMilliseconds(300);
            CGitHelper.GitTaskCompletionStageChanged += EGitTaskCompletionStageChanged;
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
        #region MGPACP_sync_all_MouseLeftButtonDown
        private void MGPACP_sync_all_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _ = SyncAll();
        #endregion
        #region EGitTaskCompletionStageChanged
        private void EGitTaskCompletionStageChanged(CGitTaskCompletion completion, int completedCount, int totalCount, EGitTaskCompletionStage stage, int queueCount)
        {
            if (stage is EGitTaskCompletionStage.Completed)
            {
                Dispatcher.Invoke(() =>
                {
                    MGPACP_sync_all.IsEnabled = true;
                    MGPACP_sync_all.Text = Dictionary.Translate($"Синхронизировать все");
                });

                var countFail = completion.Tasks.Where(x => x.State == EGitTaskState.ErrorOccurred).ToList();
                if (countFail.Count > 0)
                {
                    var text =
                        totalCount is 1 ?
                        Dictionary.Translate($"При синхронизации репозитория произошла ошибка. Детали в логах. Репозиторий:") + $" {countFail[0].Repository.Name}" :
                        Dictionary.Translate($"При синхронизации репозиториев произошло несколько ошибок. Детали в логах. Количество ошибок:") + $" {countFail.Count}";

                    Application.Current.Dispatcher.Invoke
                    (
                        () =>
                            Main.ShowModal(new BoxSettings
                            (
                                Dictionary.Translate("Ошибка синхронизации"),
                                text,
                                [new(EResponse.Ok, Dictionary.Translate("Ок"))]
                            ))
                    );
                }
            }
        }
        #endregion
        #region MGPACP_search_OnTextChanged
        private async Task MGPACP_search_OnTextChanged(CTextInput sender, string text) => _ = ApplySearchFilter(text);
        #endregion
        #region MGPACP_load_type_NewSelectedItem
        private void MGPACP_load_type_NewSelectedItem(CList.CListItem item)
        {
            var newValue = (ELoadType)item.Id;

            if (AppSettings.Instance.LoadType != newValue)
            {
                AppSettings.Instance.LoadType = newValue;
                AppSettings.Save();
            }
        }
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
                #region Загрузка типов загрузки
                var listItem = new List<CList.CListItem>();
                foreach (var loadType in Enum.GetValues<ELoadType>()) listItem.Add(new ((int)loadType, loadType.ToString().ToUpper()));
                _ = MGPACP_load_type.LoadItems(listItem, listItem.IndexOf(listItem.First(x => x.Id == (int)AppSettings.Instance.LoadType)));
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

            MGPACP_sync_all.Text = Dictionary.Translate($"Синхронизировать все");
            MGPACP_search.Placeholder = Dictionary.Translate($"Поиск");
        }
        #endregion
        #region SyncAll
        private async Task SyncAll()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось запустить синхронизацию всех репозиториев";

            #region try
            try
            {
                #region Меняем кнопку
                await Dispatcher.BeginInvoke(() => { MGPACP_sync_all.Text = Dictionary.Translate($"Синхронизация..."); MGPACP_sync_all.IsEnabled = false; });
                #endregion
                #region Отправка запроса
                var tryRunSync = await CGitHelper.Sync(Repositories);
                if (!tryRunSync.IsSuccess)
                {
                    throw new UExcept(ESyncAll.FailSendRequest, Dictionary.Translate($"Не удалось добавить запрос в очередь. Детали в логах"), tryRunSync.Error);
                }
                #endregion
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {                
                var uex = new UExcept(EGitControl.FailSyncAll, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);
                Main.Notify(ex.Message);
            }
            #endregion
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
        #region ApplySearchFilter
        private async Task ApplySearchFilter(string searchText)
        {
            var results = GProp.GitRepositories.Where
            (
                x =>
                    x.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    x.Type.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    x.FilePath.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    GStatic.GetRotationClassName(x.Class).Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
            ).ToList();

            if (results.Count > 0)
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    Functions.SyncCollections(Repositories, results, (x) => x.Id);
                });
            }
            else
            {
                Repositories.Clear();
                Main.Notify(Dictionary.Translate($"Нет результатов"));
            }            
        }
        #endregion
        #endregion

        
    }
}
