using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Byster.Protection.Utils;

namespace Byster.Protection.AntiDebug
{
    class DebugProtect2
    {
        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtQueryInformationProcess([In] IntPtr ProcessHandle, [In] PROCESSINFOCLASS ProcessInformationClass, out IntPtr ProcessInformation, [In] int ProcessInformationLength, [Optional] out int ReturnLength);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtClose([In] IntPtr Handle);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtRemoveProcessDebug(IntPtr ProcessHandle, IntPtr DebugObjectHandle);

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtSetInformationDebugObject([In] IntPtr DebugObjectHandle, [In] DebugObjectInformationClass DebugObjectInformationClass, [In] IntPtr DebugObjectInformation, [In] int DebugObjectInformationLength, [Out] [Optional] out int ReturnLength );

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern NtStatus NtQuerySystemInformation([In] SYSTEM_INFORMATION_CLASS SystemInformationClass, IntPtr SystemInformation, [In] int SystemInformationLength, [Out] [Optional] out int ReturnLength);

        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        /// <summary>
        /// Peform basic checks, method 2
        /// Checks are very fast, there is a little CPU overhead.
        /// </summary>
        public static AntiDebugFlags PerformChecks(AntiDebugFlags flags)
        {
            if (CheckDebugPort())
                flags |= AntiDebugFlags.DebugPort;

            if (CheckKernelDebugInformation())
                flags |= AntiDebugFlags.KernelDebug;

            if (DetachFromDebuggerProcess())
                flags |= AntiDebugFlags.DetachFromDebuggerProcess;

            return flags;
        }

        private static bool CheckDebugPort()
        {
            NtStatus status;
            IntPtr DebugPort = new IntPtr(0);
            int ReturnLength;

            unsafe
            {
                status = NtQueryInformationProcess(System.Diagnostics.Process.GetCurrentProcess().Handle, PROCESSINFOCLASS.ProcessDebugPort, out DebugPort, Marshal.SizeOf(DebugPort), out ReturnLength);
                return status == NtStatus.Success && DebugPort == new IntPtr(-1);
            }
        }

        private static bool DetachFromDebuggerProcess()
        {
            IntPtr hDebugObject = INVALID_HANDLE_VALUE;
            var dwFlags = 0U;
            NtStatus ntStatus;
            int retLength_1;
            int retLength_2;

            unsafe
            {
                ntStatus = NtQueryInformationProcess(System.Diagnostics.Process.GetCurrentProcess().Handle, PROCESSINFOCLASS.ProcessDebugObjectHandle, out hDebugObject, IntPtr.Size, out retLength_1);
                if (ntStatus != NtStatus.Success)
                    return false;

                ntStatus = NtSetInformationDebugObject(hDebugObject, DebugObjectInformationClass.DebugObjectFlags, new IntPtr(&dwFlags), Marshal.SizeOf(dwFlags), out retLength_2);
                if (ntStatus != NtStatus.Success)
                    return false;

                ntStatus = NtRemoveProcessDebug(System.Diagnostics.Process.GetCurrentProcess().Handle, hDebugObject);
                if (ntStatus != NtStatus.Success)
                    return false;

                ntStatus = NtClose(hDebugObject);
                if (ntStatus != NtStatus.Success)
                    return false;
            }

            return true;
        }

        private static bool CheckKernelDebugInformation()
        {
            SYSTEM_KERNEL_DEBUGGER_INFORMATION pSKDI;

            int retLength;
            NtStatus ntStatus;

            unsafe
            {
                ntStatus = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemKernelDebuggerInformation, new IntPtr(&pSKDI), Marshal.SizeOf(pSKDI), out retLength);

                if (ntStatus == NtStatus.Success)
                {
                    if (pSKDI.KernelDebuggerEnabled && !pSKDI.KernelDebuggerNotPresent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
