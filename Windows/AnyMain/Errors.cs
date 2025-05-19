using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Windows.AnyMain.Errors
{
    #region EInitialization
    public enum EInitialization
    {
        FailInitPanelChanger,
        FailGetUserInfo,
        FailInitPage,
        FailLoadTranslations
    }
    #endregion
    #region EShowModal
    public enum EShowModal
    {
        MainWindowWasNull
    }
    #endregion
}
