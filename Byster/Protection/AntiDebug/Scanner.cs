using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace Byster.Protection.AntiDebug
{
    class Scanner
    {
        private static List<string> BadProcessNames = new List<string>();
        private static List<string> BadWindowTexts  = new List<string>();
        private static List<Process> FoundedBads    = new List<Process>();


        /// <summary>
        /// Simple scanner for "bad" processes (debuggers) using .NET code only. (for now)
        /// </summary>
        public static IEnumerable<Process> Scan()
        {
            if(BadProcessNames.Count == 0 && BadWindowTexts.Count == 0) {
                Init();
            }

            var bads = Process.GetProcesses().Where(x => 
                !FoundedBads.Contains(x) && 
                (BadProcessNames.Contains(x.ProcessName) || 
                BadWindowTexts.Contains(x.MainWindowTitle))
            );

            foreach (Process p in bads)
            {
                FoundedBads.Add(p);
            }

            return bads;
        }

        /// <summary>
        /// Populate "database" with process names/window names.
        /// </summary>
        private static int Init()
        {
            if (BadProcessNames.Count > 0 && BadWindowTexts.Count > 0)
            {
                return 1;
            }

            BadProcessNames.Add("ollydbg");
            BadProcessNames.Add("ida");
            BadProcessNames.Add("ida64");
            BadProcessNames.Add("idag");
            BadProcessNames.Add("idag64");
            BadProcessNames.Add("idaw");
            BadProcessNames.Add("idaw64");
            BadProcessNames.Add("idaq");
            BadProcessNames.Add("idaq64");
            BadProcessNames.Add("idau");
            BadProcessNames.Add("idau64");
            BadProcessNames.Add("scylla");
            BadProcessNames.Add("scylla_x64");
            BadProcessNames.Add("scylla_x86");
            BadProcessNames.Add("protection_id");
            BadProcessNames.Add("x64dbg");
            BadProcessNames.Add("x32dbg");
            BadProcessNames.Add("windbg");
            BadProcessNames.Add("reshacker");
            BadProcessNames.Add("ImportREC");
            BadProcessNames.Add("IMMUNITYDEBUGGER");
            BadProcessNames.Add("MegaDumper");

            BadWindowTexts.Add("OLLYDBG");
            BadWindowTexts.Add("ida");
            BadWindowTexts.Add("disassembly");
            BadWindowTexts.Add("scylla");
            BadWindowTexts.Add("Debug");
            BadWindowTexts.Add("[CPU");
            BadWindowTexts.Add("Immunity");
            BadWindowTexts.Add("WinDbg");
            BadWindowTexts.Add("x32dbg");
            BadWindowTexts.Add("x64dbg");
            BadWindowTexts.Add("Import reconstructor");
            BadWindowTexts.Add("MegaDumper");
            BadWindowTexts.Add("MegaDumper 1.0 by CodeCracker / SnD");

            return 0;
        }

    }
}
