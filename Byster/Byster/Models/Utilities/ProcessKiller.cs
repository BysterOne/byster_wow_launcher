using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Byster.Models.Utilities
{
    public static class ProcessKiller
    {
        private static readonly string[] processesToKill = { "Sirus Launcher" };
        private static Thread seekerThread;
        private static bool isStopping = false;

        public static event Action ProcessKilled;

        public static void StartKiller()
        {
            seekerThread = new Thread(threadTick);
            seekerThread.Start();
        }

        public static void StopKiller()
        {
            isStopping = true;
        }

        private static void threadTick()
        {
            while(!isStopping)
            {
                if(KillProcesses())
                {
                    ProcessKilled?.Invoke();
                }
                Thread.Sleep(1000);
            }
        }

        public static bool KillProcesses()
        {
            var processes = getProcessesToKill();
            if (processes.Length == 0) return false;
            foreach (var processToKill in processes)
            {
                processToKill.Kill();
            }
            return true;
        }

        private static Process[] getProcessesToKill()
        {
            List<Process> processes = new List<Process>();
            foreach(var processName in processesToKill)
            {
                var foundProcesses = Process.GetProcessesByName(processName);
                processes.AddRange(foundProcesses);
            }
            return processes.ToArray();
        }
    }
}
