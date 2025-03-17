using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Windows.AnyAuthorization.Errors
{
    public enum EInitialization
    {
        FailInitPanelChanger,
        FailLoadLangList,
    }

    public enum ERegistration
    {
        FailProcRegister,
        NonComplianceData
    }

    public enum ELogin
    {
        FailProcLogin,
        NonComplianceData
    }
}
