using NLog;
using System;
using System.Collections.Generic;
using System.Text;
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
            var logerrorfile = new NLog.Targets.FileTarget("logerrorfile")
            {
                FileName = "${specialfolder:folder=ApplicationData:cached=true}/BysterConfig/BysterErrors.log",
                Layout = "[${level}][${longdate}] ${message}${exception:format=ToString}",
                KeepFileOpen = true,
                Encoding = Encoding.UTF8,
                CreateDirs = true,
                ArchiveFileName = "${specialfolder:folder=ApplicationData:cached=true}/BysterConfig/BysterErrorLogsArchive/${longdate}-BysterLogs.zip",
                EnableArchiveFileCompression = true,
                ArchiveOldFileOnStartupAboveSize = 5000,
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            config.AddRule(LogLevel.Warn, LogLevel.Fatal, logerrorfile);
            NLog.LogManager.Configuration = config;
        }

        public static void BaseLog(string messageClass, string msg, LogLevel level, params object[] p)
        {
            if (!LogManager.IsLoggingEnabled()) MessageBox.Show("Логгер не активирован");
            Logger logger = LogManager.GetCurrentClassLogger();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append(messageClass);
            stringBuilder.Append("]\t");
            stringBuilder.Append("[");
            stringBuilder.Append(msg);
            stringBuilder.Append("]\t");
            foreach (object o in p)
                stringBuilder.Append(o?.ToString() ?? "{Ошибка преобразования}");
            if(level == LogLevel.Info)
            {
                logger.Info(stringBuilder.ToString());
            }
            else if(level == LogLevel.Warn)
            {
                logger.Warn(stringBuilder.ToString());
            }
            else if(level == LogLevel.Error)
            {
                logger.Error(stringBuilder.ToString());
            }
            else if(level == LogLevel.Fatal)
            {
                logger.Fatal(stringBuilder.ToString());
            }
            else
            {
                logger.Debug(stringBuilder.ToString());
            }

        }


        public static void LogInfo(string messageClass, string msg, params object[] p)
        {
            BaseLog(messageClass, msg, LogLevel.Info, p);
        }
        public static void LogWarn(string messageClass, string msg, params object[] p)
        {
            BaseLog(messageClass, msg, LogLevel.Warn, p);
        }
        public static void LogError(string messageClass, string msg, params object[] p)
        {
            BaseLog(messageClass, msg, LogLevel.Error, p);
        }
        public static void LogFatal(string messageClass, string msg, params object[] p)
        {
            BaseLog(messageClass, msg, LogLevel.Fatal, p);
        }
    }
}
