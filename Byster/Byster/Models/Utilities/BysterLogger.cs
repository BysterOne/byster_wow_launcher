using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLog;
using System.Windows;
namespace Byster.Models.Utilities
{
    public class BysterLogger
    {
        public static void Log(string msg, params object[] p)
        {
            if (!LogManager.IsLoggingEnabled()) MessageBox.Show("Логгер не активирован");
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Error(msg);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(DateTime.Now.ToString("[dd.MM.yyyy HH:MM:ss.ff] - ["));
            stringBuilder.Append(msg);
            stringBuilder.Append("]\t");
            foreach(object o in p)
                stringBuilder.Append(o?.ToString() ?? "{Ошибка преобразования}");
            stringBuilder.Append("\n");
            logger.Info(stringBuilder.ToString());
        }
    }
}
