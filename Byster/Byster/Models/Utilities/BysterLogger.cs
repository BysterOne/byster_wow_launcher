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
        public static void Init()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "${specialfolder:folder=ApplicationData:cached=true}/BysterConfig/Byster.log",
                Layout = "[${level}][${longdate}] ${message}${exception:format=ToString}",
                KeepFileOpen = true,
                Encoding = Encoding.UTF8,
                CreateDirs = true,
                ArchiveFileName = "${specialfolder:folder=ApplicationData:cached=true}/BysterConfig/BysterLogsArchive/${longdate}-BysterLogs.zip",
                EnableArchiveFileCompression = true,
                ArchiveOldFileOnStartupAboveSize = 5000,
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
        }


        public static void Log(string msg, params object[] p)
        {
            if (!LogManager.IsLoggingEnabled()) MessageBox.Show("Логгер не активирован");
            Logger logger = LogManager.GetCurrentClassLogger();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append(msg);
            stringBuilder.Append("]\t");
            foreach(object o in p)
                stringBuilder.Append(o?.ToString() ?? "{Ошибка преобразования}");
            logger.Info(stringBuilder.ToString());
        }
    }
}
