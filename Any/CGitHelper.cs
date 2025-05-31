using Cls;
using Cls.Any;
using Cls.Exceptions;
using Launcher.Any.CGitHelperAny;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Settings;
using Microsoft.VisualBasic.ApplicationServices;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Launcher.Any
{
    #region CGitHelperAny
    namespace CGitHelperAny
    {
        #region EGitHelper
        public enum EGitHelper
        {
            FailProcessTaskAsync
        }
        #endregion
        #region EGitTaskState
        public enum EGitTaskState
        {
            WaitQueue,
            Processing,
            Finished,
            ErrorOccurred,
        }
        #endregion
        #region EGitTaskCompletionStage
        public enum EGitTaskCompletionStage
        {
            Added,
            Started,
            Progress,
            Completed
        }
        #endregion
        #region EProcessTaskAsync
        public enum EProcessTaskAsync
        {
            TestError,
            WorkDirectoryIsNull,
            FailCloneRepository,
            FailPullRepository
        }
        #endregion
        #region ERunGitCommand
        public enum ERunGitCommand
        {
            Exception,
            FailExecuteGitCommand
        }
        #endregion

        public delegate void GitTaskCompletionHandler(CGitTaskCompletion completion, int completedCount, int totalCount, EGitTaskCompletionStage stage, int queueCount);
        public delegate void GitTaskStatusChangedHandler(CGitTaskCompletion completion, CGitTask task);
    }
    #endregion

    #region Models
    public class CGitTask
    {
        public string Id { get; init; }
        public EGitTaskState State { get; internal set; }
        public CGitDirectory Repository { get; init; }

        internal CGitTask(CGitDirectory repository)
        {
            Id = Guid.NewGuid().ToString();
            Repository = repository;
            State = EGitTaskState.WaitQueue;
        }
    }

    public class CGitTaskCompletion
    {
        public string Id { get; init; }
        public IReadOnlyList<CGitTask> Tasks { get; }
        public bool IsFullSync { get; set; } = false;

        public CGitTaskCompletion(IEnumerable<CGitTask> tasks)
        {
            Id = Guid.NewGuid().ToString();
            Tasks = tasks.ToList();
        }
    }
    #endregion


    public class CGitHelper
    {
        #region Поля / свойства
        public static LogBox Pref { get; set; } = new("Git Helper");
        public static CGitTaskCompletion? CurrentTask { get; private set; }

        private static readonly ConcurrentQueue<CGitTaskCompletion> _queue = new();
        private static readonly SemaphoreSlim _taskSlim = new(initialCount: 2, maxCount: 2);
        private static readonly object _runLock = new();

        private static readonly ConcurrentDictionary<int, EGitTaskState> cache = new();
        #endregion

        #region События        
        public static event GitTaskCompletionHandler? GitTaskCompletionStageChanged;
        public static event GitTaskStatusChangedHandler? GitTaskStatusChanged;
        #endregion

        #region Функции
        #region GetGitTask
        public static EGitTaskState? GetTaskLastState(CGitDirectory directory)
        {
            return cache.TryGetValue(directory.Id, out var state) ? state : null;
        }
        #endregion
        #region Sync
        public static async Task<UResponse> Sync(IReadOnlyCollection<CGitDirectory> directories, bool IsFullSync = false)
        {
            var proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = "Не удалось запустить синхронизацию";

            #region try
            try
            {
                #region Удаляем репозитории, которые уже находятся в очереди/работе
                var toAdd = 
                    directories.Where
                    (
                        dir => 
                            !_queue.SelectMany(c => c.Tasks).Any(t => t.Repository.Id == dir.Id) &&
                            (
                                CurrentTask == null || 
                                CurrentTask.Tasks.All(t => t.Repository.Id != dir.Id || t.State is EGitTaskState.Finished || t.State is EGitTaskState.ErrorOccurred)
                            )
                    )
                    .ToList();
                #endregion
                #region Если нечего добавлять
                if (toAdd.Count == 0) return new() { IsSuccess = true };
                #endregion
                #region Создаём пачку и кладём в очередь
                var newCompletion = new CGitTaskCompletion
                (
                    toAdd.Select
                    (
                        r =>
                        {
                            var t = new CGitTask(r);
                            cache.TryAdd(t.Repository.Id, EGitTaskState.WaitQueue);
                            return t;
                        }
                    )
                )
                {
                    IsFullSync = IsFullSync
                };
                _queue.Enqueue(newCompletion);
                GitTaskCompletionStageChanged?.Invoke(newCompletion, 0, newCompletion.Tasks.Count, EGitTaskCompletionStage.Added, _queue.Count);
                #endregion
                #region Пробуем запустить обработчик
                _ = RunNextAsync();
                #endregion

                await Task.Run(() => { return; });

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
                return new(new UExcept(GlobalErrors.Exception, $"{_failinf}: исключение", ex));
            }
            #endregion
        }
        #endregion
        #region RunNextAsync
        private static Task RunNextAsync()
        {
            lock (_runLock)
            {
                if (CurrentTask != null) return Task.CompletedTask;
                if (!_queue.TryDequeue(out var next)) return Task.CompletedTask;

                CurrentTask = next;
                return Task.Run(() => ProcessCurrentAsync());
            }
        }
        #endregion
        #region ProcessCurrentAsync
        private static async Task ProcessCurrentAsync()
        {
            if (CurrentTask == null) return;

            var completion = CurrentTask;
            var total = completion.Tasks.Count;
            var completed = 0;

            #region Оповещаем о старте пачки
            GitTaskCompletionStageChanged?.Invoke(completion, completed, total, EGitTaskCompletionStage.Started, _queue.Count);
            #endregion
            #region Создаем и ожидаем задачи
            var tasks = completion.Tasks.Select(t => ProcessTaskAsync(completion, t, () =>
            {
                var done = Interlocked.Increment(ref completed);
                GitTaskCompletionStageChanged?.Invoke(completion, done, total, EGitTaskCompletionStage.Progress, _queue.Count);
            }));

            await Task.WhenAll(tasks);
            #endregion
            #region Оповещаем о завершении пачки
            GitTaskCompletionStageChanged?.Invoke(completion, total, total, EGitTaskCompletionStage.Completed, _queue.Count);
            #endregion
            #region Освобождаем текущую пачку и запускаем следующую
            lock (_runLock)
            {
                CurrentTask = null;
            }
            await RunNextAsync();
            #endregion
        }
        #endregion
        #region ProcessTaskAsync
        private static async Task ProcessTaskAsync(CGitTaskCompletion completion, CGitTask task, Action onCompleted)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить задачу на синхронизацию";

            #region Ждем            
            await _taskSlim.WaitAsync();
            #endregion

            #region try
            try
            {
                #region Меняем статус
                task.State = EGitTaskState.Processing;
                Upsert(task.Repository.Id, task.State);
                GitTaskStatusChanged?.Invoke(completion, task);
                #endregion

                #region Клонирование или синхрон
                #region Проверка на пустую директорию
                if (String.IsNullOrWhiteSpace(AppSettings.Instance.WorkDirectory))
                {
                    throw new UExcept(EProcessTaskAsync.WorkDirectoryIsNull, $"Рабочая директория не указана");
                }
                #endregion
                var path = Path.Combine(AppSettings.Instance.WorkDirectory, task.Repository.FilePath);
                string gitFolder = Path.Combine(path, ".git");
                if (!Directory.Exists(gitFolder))
                {
                    var tryClone = await RunGitCommand(
                        $"clone --remote-submodules --recursive --branch=dev \"{task.Repository.SshUrl}\" \"{path}\"",
                        Directory.GetParent(path)?.FullName ?? ".");

                    if (!tryClone.IsSuccess)
                    {
                        var ex = new UExcept(EProcessTaskAsync.FailCloneRepository, $"Не удалось клонировать репозиторий", tryClone.Error);
                        ex.Data["name"] = task.Repository.Name;
                        ex.Data["ssh_url"] = task.Repository.SshUrl;
                        throw ex;
                    }
                }
                else
                {
                    var tryPull = await RunGitCommand(
                        "pull origin dev",
                        path
                    );

                    if (!tryPull.IsSuccess)
                    {
                        var ex = new UExcept(EProcessTaskAsync.FailPullRepository, $"Не удалось синхронизировать репозиторий", tryPull.Error);
                        ex.Data["name"] = task.Repository.Name;
                        ex.Data["ssh_url"] = task.Repository.SshUrl;
                        throw ex;
                    }                     
                }
                #endregion

                #region Успешное выполнение
                task.State = EGitTaskState.Finished;
                Upsert(task.Repository.Id, task.State);
                GitTaskStatusChanged?.Invoke(completion, task);                
                #endregion
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(EGitHelper.FailProcessTaskAsync, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);

                task.State = EGitTaskState.ErrorOccurred;
                Upsert(task.Repository.Id, task.State);
                GitTaskStatusChanged?.Invoke(completion, task);
            }
            #endregion
            #region finally
            finally
            {
                Remove(task.Repository.Id);
                _taskSlim.Release();
                onCompleted();
            }
            #endregion
        }
        #endregion
        #region Upsert
        private static EGitTaskState Upsert(int id, EGitTaskState newState)
        {
            return cache.AddOrUpdate(id,
                addValueFactory: _ => newState,
                updateValueFactory: (_, _) => newState);
        }
        #endregion
        #region Remove
        private static bool Remove(int id)
        { 
            return cache.TryRemove(id, out _);
        }
        #endregion
        #region RunGitCommand
        private static async Task<UResponse> RunGitCommand(string arguments, string workingDirectory)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить git команду";

            #region try
            try
            {
                var psi = new ProcessStartInfo("git")
                {
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0) return new() { IsSuccess = true };
                else throw new UExcept(ERunGitCommand.FailExecuteGitCommand, $"Ошибка процесса: {error}");
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
                return new(new UExcept(ERunGitCommand.Exception, $"Исключение: {ex.Message}", ex));
            }
            #endregion
        }
        #endregion
        #endregion

    }
}
