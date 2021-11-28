using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Byster.Models.Utilities
{
    public class Logger
    {
        private static string logPath = $"{Directory.GetCurrentDirectory()}\\log.txt";
        public static void Log(string msg, params object[] p)
        {
            if (!File.Exists(logPath)) File.Create(logPath).Close();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(DateTime.Now.ToString("[dd.MM.yyyy HH:MM:ss.ff] - ["));
            stringBuilder.Append(msg);
            stringBuilder.Append("]\t");
            foreach(object o in p)
                stringBuilder.Append(o?.ToString() ?? "{Ошибка преобразования}");
            stringBuilder.Append("\n");
            File.AppendAllText(logPath, stringBuilder.ToString());
        }
    }
}
