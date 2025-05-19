using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Settings.Enums
{
	#region EServer
	public enum EServer
	{
        Prod,
        Staging
    }
    #endregion
    #region ELauncherUpdate
    [Flags]
    public enum ELauncherUpdate
    {
        User,
        Shop,
        Subscriptions
    }
    #endregion

}
