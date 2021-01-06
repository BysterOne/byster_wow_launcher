using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Byster.Protection.AntiDebug;
using Byster.Protection.AntiDump;

namespace Byster.Protection
{
    internal class Protector
    {
        public static void Initialize()
        {
            //AntiDumper1.Initialize();
            //AntiDebuger.Initialize();

            //Scan();

            new Thread(() =>
            {
                Scan();
                //App.Current.Dispatcher.Invoke(() => Scan());
                Thread.Sleep(1000);
            }).Start();

        }

        public static void Scan()
        {
            IEnumerable<Process> bads = Scanner.Scan();
            if (bads.Any())
            {
                foreach(var p in bads)
                    Console.WriteLine($"{p.Id}:{p.ProcessName}");
            }
            //new changes

            AntiDebugFlags debugFlags = AntiDebugFlags.None;
            debugFlags = DebugProtect1.PerformChecks(debugFlags);
            debugFlags = DebugProtect2.PerformChecks(debugFlags);
            if (debugFlags > 0)
            {
                Console.WriteLine($"Debug index: {(int)debugFlags}");
            }
        }
    }

    public enum AntiDebugFlags
    {
        None                        = 0,
        DebuggerManaged             = 1,
        DebuggerUnmanaged           = 2,
        RemoteDebugger              = 4,
        DebugPort                   = 8,
        KernelDebug                 = 16,
        DetachFromDebuggerProcess   = 32
    }
}
