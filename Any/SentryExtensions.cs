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
            #if DEBUG
            return;
            #endif

            var sentryEvent = new SentryEvent(ex);
            sentryEvent.SetExtra("Advanced Data", ex.GetFullInfo(""));
            SentrySdk.CaptureEvent(sentryEvent);
        }
        public static void SendException(Exception ex)
        {
            #if DEBUG
            return;
            #endif
            SentrySdk.CaptureException(ex);
        }
        #endregion
    }
}
