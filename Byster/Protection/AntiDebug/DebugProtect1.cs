using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Byster.Protection.Utils;

namespace Byster.Protection.AntiDebug
{
    class DebugProtect1
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] ref bool isDebuggerPresent);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsDebuggerPresent();

        /// <summary>
        /// Peform basic checks, method 1
        /// Checks are very fast, there is no CPU overhead.
        /// </summary>
        public static AntiDebugFlags PerformChecks(AntiDebugFlags flags)
        {
            if(CheckDebuggerManagedPresent())
                flags = flags.SetFlag(AntiDebugFlags.DebuggerManaged, true);

            if (CheckDebuggerUnmanagedPresent())
                flags = flags.SetFlag(AntiDebugFlags.DebuggerUnmanaged, true); 

            if (CheckRemoteDebugger())
                flags = flags.SetFlag(AntiDebugFlags.RemoteDebugger, true); 

            return flags;
        }

        /// <summary>
        /// Asks the CLR for the presence of an attached managed debugger, and never even bothers to check for the presence of a native debugger.
        /// </summary>
        private static bool CheckDebuggerManagedPresent()
        {
            return System.Diagnostics.Debugger.IsAttached;
        }

        /// <summary>
        /// Asks the kernel for the presence of an attached native debugger, and has no knowledge of managed debuggers.
        /// </summary>
        private static bool CheckDebuggerUnmanagedPresent()
        {
            return IsDebuggerPresent();
        }

        /// <summary>
        /// Checks whether a process is being debugged.
        /// </summary>
        /// <remarks>
        /// The "remote" in CheckRemoteDebuggerPresent does not imply that the debugger
        /// necessarily resides on a different computer; instead, it indicates that the 
        /// debugger resides in a separate and parallel process.
        /// </remarks>
        private static bool CheckRemoteDebugger()
        {
            var isDebuggerPresent = (bool)false;

            var bApiRet = CheckRemoteDebuggerPresent(System.Diagnostics.Process.GetCurrentProcess().Handle, ref isDebuggerPresent);

            return bApiRet && isDebuggerPresent;
        }
    }
}
