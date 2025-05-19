using Cls.Any;
using Launcher.Any.UDialogBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher.Any
{
    #region UDialogBox
    namespace UDialogBox
    {
        public enum EDialogResponse
        {
            Ok, 
            Closed,
            ErrorOccurred
        }
    }
    #endregion

    public interface IUDialogBox
    {
        public Task<UResponse<EDialogResponse>> Show(params object[] p);

        public Task Hide();
    }
}
