using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Windows;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Cls.Any
{
    public static class Functions
    {
        #region TimeStampToDateTime - Конвертация миллисекунд в объект даты и времени
        /// <summary>
        /// Конвертация миллисекунд в объект даты и времени
        /// </summary>
        /// <param name="timeStamp">Миллисекунды</param>
        /// <returns></returns>
        public static DateTime TimeStampToDateTime(long timeStamp)
        {
            var zero = DateTime.UnixEpoch;
            return zero.AddMilliseconds(timeStamp);
        }
        #endregion

        #region DateTimeToTimeStamp - Конвертация даты и времени в миллисекунды
        /// <summary>
        /// Конвертация даты и времени в миллисекунды
        /// </summary>
        /// <param name="dateTime">Дата и время</param>
        /// <returns></returns>
        public static long DateTimeToTimeStamp(DateTime dateTime)
        {
            var zero = DateTime.UnixEpoch;
            var milliseconds = (long)Math.Round((dateTime - zero).TotalMilliseconds);
            return milliseconds;
        }
        #endregion

        #region CreateId - Создает Id по количеству блоков и их длины
        /// <summary>
        /// Создает Id по количеству блоков и их длины
        /// </summary>
        /// <param name="countBlocks">Количество блоков</param>
        /// <param name="blockLength">Количество символов в блоке</param>
        /// <returns></returns>
        public static string CreateId(int countBlocks = 4, int blockLength = 6)
        {
            var rd = new Random();
            string h = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890";
            var blocks = new List<string>();
            while (blocks.Count < countBlocks)
            {
                var b = "";
                while (b.Length < blockLength) { b += h[rd.Next(h.Length)]; }
                blocks.Add(b);
            }
            var r = string.Join('-', blocks);
            return r;
        }
        #endregion

        #region GetMethodName - Возвращает имя функции
        /// <summary>
        /// Возвращает имя функции. Используется для логирования
        /// </summary>
        /// <param name="methodName">Параметр с названием метода. Устанавливается при вызове</param>
        /// <returns></returns>
        public static string GetMethodName([CallerMemberName] string methodName = "")
        {
            return methodName;
        }
        #endregion

        #region Error - Разные конструкторы ошибок
        public static void Error(UExcept exception, string message, LogBox proc)
        {
            proc.Log(message, Enums.ELogType.Error);
            exception.Error.ToLog(proc);
            proc.Log($"trace -> {exception.StackTrace}", Enums.ELogType.Trace);
        }
        public static void Error(UError error, string message, LogBox proc)
        {
            proc.Log(message, Enums.ELogType.Error);
            error.ToLog(proc);
        }
        public static void Error(Exception exception, UError error, string message, LogBox proc)
        {
            proc.Log(message, Enums.ELogType.Error);
            error.ToLog(proc);
            proc.Log($"error -> {exception.Message}", Enums.ELogType.Error);
            proc.Log($"trace -> {exception.StackTrace?.Replace("\n", "")}", Enums.ELogType.Trace);
            if (exception.InnerException is not null) { SaveInnerException(exception.InnerException, proc, 0); }
        }
        public static void Error(Exception exception, string message, LogBox proc)
        {
            proc.Log(message, Enums.ELogType.Message);
            proc.Log($"error -> {exception.Message}", Enums.ELogType.Error);
            proc.Log($"trace -> {exception.StackTrace?.Replace("\n", "")}", Enums.ELogType.Trace);
            if (exception.InnerException is not null) { SaveInnerException(exception.InnerException, proc, 0); }
        }
        private static void SaveInnerException(Exception exception, LogBox proc, int indexInner)
        {
            proc.Log($"inEx[{indexInner}] -> error -> {exception.Message}", Enums.ELogType.Error);
            proc.Log($"inEx[{indexInner}] -> trace -> {exception.StackTrace?.Replace("\n", "")}", Enums.ELogType.Trace);
            if (exception.InnerException is not null) { SaveInnerException(exception.InnerException, proc, ++indexInner); }
        }
        #endregion

        #region IsDebugMode - Возвращает true если приложение запущено в режиме дебага
        public static bool IsDebugMode()
        {
            var location = Assembly.GetEntryAssembly()?.Location;
            return !String.IsNullOrEmpty(location);
        }
        #endregion

        #region MD5
        public static string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
        #endregion

        #region OpenWindow
        public static void OpenWindow<T>(Window inner, T window) where T : Window
        {
            var animation = AnimationHelper.OpacityAnimation(inner, 0);
            animation.Completed += async (s, e) =>
            {
                WindowAnimations.ApplyFadeAnimations(ref window);
                window.Topmost = true;
                window.Show();
                await Task.Run(() => Thread.Sleep(AnimationHelper.AnimationDuration));
                window.Topmost = false;

                inner.Close();
            };
            animation.Begin();
        }
        #endregion

        #region ToOut
        public static string ToOut(this double value)
        {
            return value.ToString
            (
                "#,0.##",
                new NumberFormatInfo
                {
                    NumberGroupSeparator = " ",
                    NumberDecimalSeparator = "."
                }
            );
        }
        #endregion

        #region GetSourceFromResource
        public static Uri GetSourceFromResource(string resourceName)
        {            
            return new Uri("pack://application:,,,/" + resourceName, UriKind.RelativeOrAbsolute);
        }
        #endregion

        #region GlobalResources
        public static ResourceDictionary GlobalResources()
        {
            return new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Styles/Global.xaml", UriKind.RelativeOrAbsolute)
            };
        }
        #endregion
    }
}
