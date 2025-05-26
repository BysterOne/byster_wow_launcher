using Cls.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Any
{
    public class SentryExtensions
    {

        #region SendException
        public static void SendException(UExcept ex)
        {
            var sentryEvent = new SentryEvent(ex);
            sentryEvent.SetExtra("Advanced Data", ex.GetFullInfo(""));
            SentrySdk.CaptureEvent(sentryEvent);
        }
        public static void SendException(Exception ex) => SentrySdk.CaptureException(ex);
        #endregion
    }
}
