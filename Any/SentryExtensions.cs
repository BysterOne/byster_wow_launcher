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
        #region Глобальные транзакции
        public static ITransactionTracer? FirstLoadTransaction { get; set; }
        public static ITransactionTracer? MainFromAuthTransaction { get; set; }
        public static ISpan? AuthorizationWindowLoadingTransaction { get; set; }
        public static ISpan? MainWindowLoadingTransaction { get; set; }
        public static ITransactionTracer? BuyTransaction { get; set; }
        public static ITransactionTracer? LaunchExeTransaction { get; set; }
        #endregion

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
