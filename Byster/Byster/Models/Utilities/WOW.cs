using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Jupiter;

namespace Byster.Models.Utilities
{
    public class WoW
    {
        public Process Process { get; set; }
        public MemoryModule Memory { get; set; }
        public string Name { get; set; }
        public Classes Class { get; set; }
        public string RealmName { get; set; }
        public string RealmServer { get; set; }
        public string Version { get; set; }
        public bool WorldLoaded { get; set; }

        public override string ToString()
        {
            return $"[{Process.Id}][{(WorldLoaded ? "+" : "-")}]{(WorldLoaded ? $" {Name} - {Class} - {RealmName}" : "Not loaded")}";
        }

        public static bool operator ==(WoW item1, WoW item2)
        {
            if (item1?.Process?.Id == item2?.Process?.Id) return true;
            return false;
        }

        public static bool operator !=(WoW item1, WoW item2)
        {
            if (item1?.Process?.Id == item2?.Process?.Id) return false;
            return true;
        }
    }

    public enum Classes
    {
        WARRIOR = 1,
        PALADIN = 2,
        HUNTER = 3,
        ROGUE = 4,
        PRIEST = 5,
        DEATHKNIGHT = 6,
        SHAMAN = 7,
        MAGE = 8,
        WARLOCK = 9,
        DRUID = 11
    }

    public class WoWSearcher : IDisposable
    { 
        public string Title { get; set; }
        public List<WoW> Wows { get; set; } = new List<WoW>();
        public WoWSearcher(string title)
        {
            Title = title;

            TimerWatcher = new Timer(new TimerCallback(TimerTick), null, 0, 1000);
            
        }

        public void Dispose()
        {
            TimerWatcher.Dispose();
        }

        public delegate bool ProcessDelegate(WoW p);

        public event ProcessDelegate OnWowFounded;
        public event ProcessDelegate OnWowClosed;
        public event ProcessDelegate OnWowChanged;

        Timer TimerWatcher;

        private void TimerTick(object state)
        {
            var windows = GetOpenedWindows();

            foreach (var w in windows)
            {
                uint pid;
                GetWindowThreadProcessId(w.Key, out pid);

                if (GetWowByPid((int)pid) != null)
                    continue;

                WoW wow = new WoW
                {
                    Process = Process.GetProcessById((int)pid),
                    Memory = new MemoryModule((int)pid)
                };

                Wows.Add(wow);

                OnWowFounded?.Invoke(wow);
                Update(wow);
            }

            for (int i = 0; i < Wows.Count; i++)
            {
                var w = Wows[i];
                if (w.Process.HasExited)
                {
                    Exited(w);
                    i--;
                }
                else
                    Update(w);
            }
        }

        private void Exited(WoW w)
        {
            Wows.Remove(w);
            OnWowClosed.Invoke(w);
        }

        private void Update(WoW w)
        {
            bool updated = false;
            try
            {
                w.WorldLoaded = _(ref updated, w.WorldLoaded, w.Memory.ReadVirtualMemory((IntPtr)0xBEBA40, 1)[0] == 1);

                w.Version = _(ref updated, w.Version, StringFromBytes(w.Memory.ReadVirtualMemory((IntPtr)0xCAD851, 30)));
                w.RealmName = _(ref updated, w.RealmName, StringFromBytes(w.Memory.ReadVirtualMemory((IntPtr)0xC79B9E, 30)));
                w.RealmServer = _(ref updated, w.RealmServer, StringFromBytes(w.Memory.ReadVirtualMemory((IntPtr)0x879B9E, 30)));
                if (w.WorldLoaded)
                {
                    w.Name = _(ref updated, w.Name, StringFromBytes(w.Memory.ReadVirtualMemory((IntPtr)0xC79D18, 30)));
                    w.Class = _(ref updated, w.Class, (Classes)w.Memory.ReadVirtualMemory<byte>((IntPtr)0xC79E89));
                }
            }
            catch { }
            

            if (updated)
                OnWowChanged?.Invoke(w);
        }

        private T _<T>(ref bool updated, T oldValue, T newValue)
        {
            updated = !newValue.Equals(oldValue) | updated;

            return newValue;
        }

        private WoW GetWowByPid(int pid)
        {
            return Wows.Where(w => w.Process.Id == pid).FirstOrDefault();
        }

        private static string StringFromBytes(byte[] myBuffer)
        {
            string text = Encoding.UTF8.GetString(myBuffer, 0, myBuffer.Length);

            if (!text.Contains('\0'))
                return "";

            return text.Split('\0')[0];
        }

        class InfoWindow
        {
            public IntPtr Handle = IntPtr.Zero;
            public FileInfo File = null;
            public string Title = null;
            public override string ToString()
            {
                return File.Name + "\t>\t" + Title;
            }
        }

        private IDictionary<IntPtr, InfoWindow> GetOpenedWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, InfoWindow> windows = new Dictionary<IntPtr, InfoWindow>();

            EnumWindows(new EnumWindowsProc(delegate (IntPtr hWnd, int lParam) {
                if (hWnd == shellWindow)
                    return true;

                if (!IsWindowVisible(hWnd))
                    return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0)
                    return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                var info = new InfoWindow()
                {
                    Handle = hWnd,
                    Title = builder.ToString()
                };

                if (info.Title != Title)
                    return true;

                windows[hWnd] = info;

                return true;
            }), 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);
    }
}
