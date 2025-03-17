using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Api.Errors
{
    #region ERequest
    public enum ERequest
    {
        FailExecuteRequest,
        Unauthorized,
        UnprocessedMethod,
        BadRequest
    }
    #endregion
}
