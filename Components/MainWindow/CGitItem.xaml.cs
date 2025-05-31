using Cls;
using Cls.Any;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.CGitHelperAny;
using Launcher.Api.Models;
using Launcher.Components.MainWindow.GitItemAny;
using Launcher.Settings;
using Launcher.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Launcher.Components.MainWindow
{
    namespace GitItemAny
    {
        #region EGitItem
        public enum EGitItem
        {
            FailUpdateView,
            FailSync,
        }
        #endregion
        #region ESync
        public enum ESync
        {
            FailSendRequest
        }
        #endregion
    }
    /// <summary>
    /// Логика взаимодействия для CGitItem.xaml
    /// </summary>
    public partial class CGitItem : UserControl
    {
        public CGitItem()
        {
            InitializeComponent();

            Unloaded += EUnloaded;

            CGitHelper.GitTaskStatusChanged += EGitTaskStatusChanged;
            CGitHelper.GitTaskCompletionStageChanged += EGitTaskCompletionStageChanged;
        }

       

        #region Переменные
        private static LogBox Pref { get; set; } = new("Git Item");
        private CGitTask? SyncTask { get; set; }
        #endregion

        #region Свойства
        #region GitLib
        public CGitDirectory? GitLib
        {
            get => (CGitDirectory)GetValue(GitLibProperty);
            set => SetValue(GitLibProperty, value);
        }

        public static readonly DependencyProperty GitLibProperty =
            DependencyProperty.Register(nameof(GitLib), typeof(CGitDirectory), typeof(CGitItem), 
                new PropertyMetadata(null, OnLibChanged));

        private static void OnLibChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CGitItem sender) _ = sender.UpdateView();            
        }
        #endregion
        #endregion

        #region Обработчики событий
        #region CPSP_sync_button_MouseDown
        private void CPSP_sync_button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => Sync();
        #endregion
        #region EGitTaskCompletionStageChanged
        private void EGitTaskCompletionStageChanged(CGitTaskCompletion completion, int completedCount, int totalCount, EGitTaskCompletionStage stage, int queueCount)
        {
            Dispatcher.Invoke(() =>
            {
                if (SyncTask is not null || GitLib is null) return;

                if (stage is EGitTaskCompletionStage.Added)
                {
                    SyncTask = completion.Tasks.FirstOrDefault(x => x.Repository.Id == GitLib.Id);
                    if (SyncTask is not null) UpdateSyncState();
                }
            });
        }
        #endregion
        #region EGitTaskStatusChanged
        private void EGitTaskStatusChanged(CGitTaskCompletion completion, CGitTask task)
        {
            Dispatcher.Invoke(() =>
            {
                if (GitLib is not null && task.Repository.Id != GitLib.Id) return;

                if (task.State is EGitTaskState.Finished || task.State is EGitTaskState.ErrorOccurred)
                {
                    SyncTask = null;
                    UpdateSwitcher();
                }
                else SyncTask ??= task;

                UpdateSyncState();
            });
        }
        #endregion
        #region EUnloaded
        private void EUnloaded(object sender, RoutedEventArgs e)
        {
            CGitHelper.GitTaskStatusChanged -= EGitTaskStatusChanged;
            CGitHelper.GitTaskCompletionStageChanged -= EGitTaskCompletionStageChanged;
        }
        #endregion       
        #region CPSP_open_folder_MouseLeftButtonDown
        private void CPSP_open_folder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => _ = OpenFolder();
        #endregion

        #region CPSP_switch_SelectedIndexChanged
        private async void CPSP_switch_SelectedIndexChanged(object sender, int newIndex)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                if (GitLib is null) return;

                var newValue = newIndex is 1;

                var syncData = AppSettings.Instance.SyncData;
                var has = syncData.TryGetValue(GitLib.Id, out CGitSyncData? data);
                if (has && data is not null && data.IsIncluded != newValue)
                {
                    data = syncData.AddOrUpdate
                    (
                        GitLib.Id,
                        new CGitSyncData() { IsIncluded = newValue },
                        (_, x) => x with { IsIncluded = newValue }
                    );
                    AppSettings.Save();
                    _ = CGitHelper.UpdateGitConfig();
                }
            });
        }
        #endregion
        #endregion

        #region Функции
        #region UpdateView
        private async Task UpdateView()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось обновить вид компонента";

            #region try
            try
            {
                #region Если пустой
                if (GitLib is null) return;
                #endregion
                #region Проверка наличия директории
                var path = Path.Combine(AppSettings.Instance.WorkDirectory, GitLib.FilePath);
                Directory.CreateDirectory(path);
                #endregion
                #region Установка данных
                CP_name.Text = GitLib.Name;
                CP_path.Text = $"../{GitLib.FilePath}";
                CP_type.Text = GitLib.Type;
                CP_class.Text = GStatic.GetRotationClassName(GitLib.Class);
                #endregion
                #region Обновление статуса синхронизации
                UpdateSyncState();
                #endregion
                #region Обновление переключателя
                UpdateSwitcher();
                #endregion
                #region Async
                await Task.Run(() => { return; });
                #endregion
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(EGitItem.FailUpdateView, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);               
            }
            #endregion
        }
        #endregion
        #region OpenFolder
        private async Task OpenFolder()
        {
            if (String.IsNullOrWhiteSpace(AppSettings.Instance.WorkDirectory) || GitLib is null) return;

            var folder = Path.Combine(AppSettings.Instance.WorkDirectory, GitLib.FilePath.Replace("/", "\\"));
            try { Directory.CreateDirectory(folder); } catch { }

            var psi = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folder}\"",
                UseShellExecute = true
            };

            Process.Start(psi);

            await Task.Run(() => { return; });
        }
        #endregion
        #region UpdateSyncState
        private void UpdateSyncState()
        {
            if (GitLib is null) return;

            EGitTaskState? state = SyncTask is not null ? SyncTask.State : null;
            if (state is null) 
            {
                var lastState = CGitHelper.GetTaskLastState(GitLib);
                if (lastState is not null) state = lastState;
            }

            #region Если нет статуса
            if (state is null || state is EGitTaskState.Finished)
            {
                CPSP_sync_button.Visibility = Visibility.Visible;
                CPSP_status_icon.Visibility = Visibility.Collapsed;
            }
            #endregion
            #region Обработка для иконки
            else
            {
                var stateIcon = state switch
                {
                    EGitTaskState.WaitQueue => "Media/waiting_icon.png",
                    EGitTaskState.Processing => "Media/Main/update_icon.png",
                    EGitTaskState.ErrorOccurred => "Media/error_icon.png"
                };

                CPSP_sync_button.Visibility = Visibility.Collapsed;
                CPSP_status_icon.Visibility = Visibility.Visible;
                CPSP_status_icon.Source = BitmapFrame.Create(Functions.GetSourceFromResource(stateIcon));

                if (state is EGitTaskState.ErrorOccurred)
                {
                    _ = Task.Run(() =>
                    {
                        Thread.Sleep(2000);
                        Dispatcher.Invoke(() =>
                        {
                            CPSP_sync_button.Visibility = Visibility.Visible;
                            CPSP_status_icon.Visibility = Visibility.Collapsed;
                        });
                    });
                }
            }
            #endregion
        }
        #endregion
        #region UpdateSwitcher
        private void UpdateSwitcher()
        {
            if (String.IsNullOrWhiteSpace(AppSettings.Instance.WorkDirectory) || GitLib is null) return;

            var folderPath = Path.Combine(AppSettings.Instance.WorkDirectory!, GitLib.FilePath);
            Directory.CreateDirectory(folderPath);

            string[] patterns = { "*.toc", "*.tocb" };

            var files = patterns.SelectMany(p => Directory.EnumerateFiles(folderPath, p, SearchOption.TopDirectoryOnly)).ToList();
            var file = string.Empty;
            #region Если один файл
            if (files.Count is 1) 
            {
                file = Path.GetFileNameWithoutExtension(files[0]); 
            }
            else if (files.Count > 1)
            {
                #region Сравниваем все файлы
                var isEq = true;
                var fName = Path.GetFileNameWithoutExtension(files[0]);
                for (int i = 1; i < files.Count; i++)
                {
                    var cmFileName = Path.GetFileNameWithoutExtension(files[i]);
                    if (fName.Equals(cmFileName)) isEq = false;
                }
                #endregion
                #region Если хотя бы один отличается, то выдаем предупреждение
                if (isEq)
                {
                    _ = Main.ShowModal(
                        new
                        (
                            Dictionary.Translate($"Предупреждение"),
                            Dictionary.Translate("В репозитории <repository> существует более одного требуемого файла с разными именами")
                                .Replace("<repository>", $"{GitLib.Name} ({GitLib.SshUrl})"),
                            [
                                new (DialogBox.EResponse.Ok, Dictionary.Translate("Ок"))
                            ]
                        )
                    );

                    CPSP_switch.Visibility = Visibility.Collapsed;
                    return;
                }
                #endregion
                #region Если все ок, то сохраняем
                file = fName;
                #endregion
            }
            else
            {
                CPSP_switch.Visibility = Visibility.Collapsed;
                return;
            }
            #endregion

            #region Обновляем или добавляем
            var syncData = AppSettings.Instance.SyncData;
            var has = syncData.TryGetValue(GitLib.Id, out CGitSyncData? data);
            if (!has || data is null || String.IsNullOrWhiteSpace(data.FileName) || !data.FileName.Equals(file))
            {
                data = syncData.AddOrUpdate
                (
                    GitLib.Id,
                    new CGitSyncData() { FileName = file },
                    (_, x) => x with { FileName = file }
                );
                AppSettings.Save();
                _ = CGitHelper.UpdateGitConfig();
            }
            #endregion
            #region Показываем переключатель и устанавливаем ему значение
            CPSP_switch.Visibility = Visibility.Visible;
            CPSP_switch.SelectedIndex = data.IsIncluded ? 1 : 0;
            #endregion
        }
        #endregion
        #region Sync
        private async void Sync()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось запустить синхронизацию";

            #region try
            try
            {
                if (GitLib is null) return;

                var tryAddSync = await CGitHelper.Sync([GitLib]);
                if (!tryAddSync.IsSuccess)
                {
                    throw new UExcept(ESync.FailSendRequest, Dictionary.Translate($"Не удалось добавить запрос в очередь. Детали в логах"), tryAddSync.Error);
                }
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(EGitItem.FailSync, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);
                Main.Notify(ex.Message);
            }
            #endregion
        }

        #endregion
        #endregion

        
    }
}
