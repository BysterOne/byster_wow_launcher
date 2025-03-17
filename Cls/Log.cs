using Cls.Enums;
using System.Diagnostics;

namespace Cls.Enums
{
    public enum ELogType { Message, Warning, Error, Trace }
}

namespace Cls
{
    public class Log
    {
        #region Делегат
        public delegate void NewMessageDelegate(string message, ELogType type);
        #endregion

        #region События
        public static event NewMessageDelegate NewMessage;
        #endregion

        #region Функции
        #region Add
        public static void Add(string message, ELogType type = ELogType.Message)
        {
            Debug.WriteLine($"{type} {message}");
            NewMessage?.Invoke(message, type);
        }
        #endregion        
        #endregion
    }

    public class LogBox
    {
        #region Компоненты
        private List<string> Trace { get; set; } = new List<string>();
        #endregion

        #region Конструкторы
        public LogBox(string startTrace)
        {
            Trace.Add(startTrace);
        }
        public LogBox(LogBox logBox, string addTrace = "")
        {
            Trace = new List<string>();
            Trace.AddRange(logBox.Trace);
            if (!String.IsNullOrWhiteSpace(addTrace)) Trace.Add(addTrace.Trim());
        }
        #endregion

        #region Функции
        #region AddTrace
        public LogBox AddTrace(string trace)
        {
            if (!String.IsNullOrWhiteSpace(trace)) Trace.Add(trace.Trim());
            return this;
        }
        #endregion
        #region Log
        public void Log(string message, ELogType type = ELogType.Message)
        {
            var trace = String.Join(" -> ", Trace);
            foreach (var item in message.Split('\n'))
            {
                var log = $"{trace} -> {item}";
                Cls.Log.Add(log, type);
            }
        }
        #endregion
        #region CloneAs
        public LogBox CloneAs(string addTrace = "")
        {
            return new LogBox(this, addTrace);
        }
        #endregion
        #endregion
    }

    public interface LoggedObject
    {
        public delegate void NewLogDelegate(string message, ELogType type = ELogType.Message);
    }
}
