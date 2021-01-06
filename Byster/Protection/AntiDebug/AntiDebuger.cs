using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Byster.Protection.Utils;
using System.Diagnostics;

namespace Byster.Protection.AntiDebug
{
    class AntiDebuger
    {
        [DllImport("ntdll.dll")]
        internal static extern NtStatus NtSetInformationThread(IntPtr ThreadHandle, ThreadInformationClass ThreadInformationClass, IntPtr ThreadInformation, int ThreadInformationLength);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        public static void Initialize()
        {
            ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
            foreach (ProcessThread thread in currentThreads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SET_INFORMATION, false, (uint)thread.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                HideFromDebugger(pOpenThread);
                CloseHandle(pOpenThread);
            }
        }

        /// <summary>
        /// Hide the thread from debug events.
        /// </summary>
        private static bool HideFromDebugger(IntPtr Handle)
        {
            NtStatus nStatus = NtSetInformationThread(Handle, ThreadInformationClass.ThreadHideFromDebugger, IntPtr.Zero, 0);
            return nStatus == NtStatus.Success;
        }
    }
}
