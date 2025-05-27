using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Any.GlobalEnums
{
    public enum ELang
    {
        Ru,
        En,
        ZhCn
    }

    public enum ELoaderState
    {
        Show,
        Hide
    }

    public enum ECurrency
    {
        Rub,
        Usd
    }

    [Flags]
    public enum EUserPermissions
    {        
        None,
        Superuser,
        ToggleEncrypt,
        AdminSiteAccess,
        CanToggleCompilation,
        ClosedServer,
        CanToggleVmprotect,
        ExternalDeveloper,
        DoNotBan,
        Tester,
    }

    public enum EBranch
    {
        Master,
        Dev,
        Test,
        NewThread
    }
}
