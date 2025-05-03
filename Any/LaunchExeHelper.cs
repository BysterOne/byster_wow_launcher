using Cls.Any;
using Launcher.Any.LaunchExeHelperAny;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Any
{
    #region LaunchExeHelperAny
    namespace LaunchExeHelperAny
    {
        public enum ELaunchState
        {
            None,
            Downloading,
            Launching,
            Launched,
        }

        public class LaunchItem
        {
            public CServer Server { get; set; } = null!;
            public ELaunchState State { get; set; } = ELaunchState.None;
        }
    }
    #endregion

    public class LaunchExeHelper
    {
        #region Переменные
        private static ConcurrentDictionary<CServer, ELaunchState> Items { get; set; } = [];
        #endregion

        #region Функции
        #region CanLaunch
        public static bool CanLaunch(CServer server)
        {
            if (Items.Any(x => x.Key.Id == server.Id)) return false;
            return true;
        }
        #endregion
        #region Launch
        public static async Task Launch(CServer server)
        {
            if (!CanLaunch(server)) return;

            #region Создаем в очередь
            var tryAdd = Items.TryAdd(server, ELaunchState.None);
            #endregion
            #region Проверяем скачан ли
            var pathName = $"{Functions.GetMd5Hash(GProp.User.Username)[13..]}.exe";
            var pathExe = Path.Combine(AppSettings.TempBin, pathName);
            #endregion sdf
            Dictionary.Translate("dd");
        }
        #endregion
        #endregion



    }
}
