using Cls;
using Cls.Any;
using Cls.Exceptions;
using Launcher.Any.CGitHelperAny;
using Launcher.Api.Models;
using Launcher.Cls;
using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        /// <summary>
        /// Стадия выполнения пачки <see cref="CGitTaskCompletion"/>.
        /// </summary>
        public enum EGitTaskCompletionStage
        {
            Started,
            Progress,
            Completed
        }

        /// <summary>
        /// Делегат события изменения статуса пачки.
        /// </summary>
        /// <param name="completion">Пачка задач.</param>
        /// <param name="completedCount">Сколько под‑задач уже завершено.</param>
        /// <param name="totalCount">Общее количество под‑задач.</param>
        /// <param name="stage">Стадия.</param>
        public delegate void GitTaskCompletionHandler(CGitTaskCompletion completion, int completedCount, int totalCount, EGitTaskCompletionStage stage);

        /// <summary>
        /// Делегат события изменения статуса отдельной Git‑задачи.
        /// </summary>
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
        #endregion

        #region События        
        public static event GitTaskCompletionHandler? GitTaskCompletionStageChanged;
        public static event GitTaskStatusChangedHandler? GitTaskStatusChanged;
        #endregion

        #region Функции
        #region Sync
        public static async Task<UResponse> Sync(IReadOnlyCollection<CGitDirectory> directories)
        {
            var proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = "Не удалось запустить синхронизацию";

            #region try
            try
            {
                #region Удаляем репозитории, которые уже находятся в очереди/работе
                var toAdd = directories
                    .Where(dir => !_queue.SelectMany(c => c.Tasks).Any(t => t.Repository.Id == dir.Id)
                                     && (CurrentTask == null || CurrentTask.Tasks.All(t => t.Repository.Id != dir.Id)))
                    .ToList();
                #endregion
                #region Если нечего добавлять
                if (toAdd.Count == 0) return new() { IsSuccess = true };
                #endregion
                #region Создаём пачку и кладём в очередь
                var newCompletion = new CGitTaskCompletion(toAdd.Select(r => new CGitTask(r)));
                _queue.Enqueue(newCompletion);
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
            GitTaskCompletionStageChanged?.Invoke(completion, completed, total, EGitTaskCompletionStage.Started);
            #endregion
            #region Создаем и ожидаем задачи
            var tasks = completion.Tasks.Select(t => ProcessTaskAsync(completion, t, () =>
            {
                var done = Interlocked.Increment(ref completed);
                GitTaskCompletionStageChanged?.Invoke(completion, done, total, EGitTaskCompletionStage.Progress);
            }));

            await Task.WhenAll(tasks);
            #endregion
            #region Оповещаем о завершении пачки
            GitTaskCompletionStageChanged?.Invoke(completion, total, total, EGitTaskCompletionStage.Completed);
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
                GitTaskStatusChanged?.Invoke(completion, task);
                #endregion

                #region Иммитация
                await Task.Run(() => Thread.Sleep(1000));
                #endregion

                #region Успешное выполнение
                task.State = EGitTaskState.Finished;
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
                GitTaskStatusChanged?.Invoke(completion, task);
            }
            #endregion
            #region finally
            finally
            {
                _taskSlim.Release();
                onCompleted();
            }
            #endregion
        }
        #endregion
        #endregion

    }
}
