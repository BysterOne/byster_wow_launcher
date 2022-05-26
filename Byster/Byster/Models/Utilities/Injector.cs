using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RestSharp;
using System.Net;
using System.IO;
using Byster.Models.RestModels;
using System.Windows;
using static Byster.Models.Utilities.BysterLogger;
using Microsoft.Win32;

namespace Byster.Models.Utilities
{
    public class Injector
    {
        public static event Action PreInjection;
        public static event Action PostInjection;
        private static string FullLibPath
        {
            get => Path.Combine(Path.GetTempPath(), coreFolderName);
        }
        private static string coreFolderName;
        public static string Branch { get; set; } = "master";
        public static RestClient Rest { get; set; }

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(uint hProcess, uint lpBaseAddress, byte[] lpBuffer, int dwSize, uint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(UInt32 handle);

        [DllImport("kernel32.dll")]
        public static extern UInt32 OpenProcess(UInt32 dwDesiredAcess, bool bInheritHandle, UInt32 swProcessId);
        [DllImport("kernel32.dll")]
        static extern UInt32 VirtualAllocEx(UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 e);

        [DllImport("kernel32.dll")]
        static extern UInt32 CreateRemoteThread(UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 e, UInt32 f, UInt32 g);

        [DllImport("kernel32.dll")]
        static extern UInt32 GetProcAddress(UInt32 hModule, byte[] functionName);

        [DllImport("kernel32.dll")]
        static extern UInt32 GetModuleHandleA(byte[] moduleName);

        [DllImport("kernel32.dll")]
        static extern UInt32 WaitForSingleObject(UInt32 a, UInt32 b);

        [DllImport("kernel32.dll")]
        static extern bool GetExitCodeThread(UInt32 a, ref UInt32 b);

        [DllImport("kernel32.dll")]
        static extern bool VirtualFreeEx(UInt32 a, UInt32 b, UInt32 c, UInt32 d);

        static byte[] to_ascii(string utf16String)
        {
            return Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(utf16String));
        }

        static bool Inject(UInt32 processID, string full_path)
        {
            bool fRes;

            //получаем хэндл процесса, он не должен быть равен нулю
            UInt32 hProcess = OpenProcess(0x43A, false, processID);
            if (hProcess == 0)
                return false;

            //получаем адрес библиотеки kernel32, он не должен быть равен нулю
            UInt32 hKernel32 = GetModuleHandleA(to_ascii("kernel32.dll"));

            if (hKernel32 == 0)
            {
                fRes = CloseHandle(hProcess);
                return false;
            }

            //ищем в библиотеке kernel32 адрес функции LoadLibraryW, он не должен быть равен нулю
            UInt32 loadLibraryW = GetProcAddress(hKernel32, to_ascii("LoadLibraryW"));

            if (loadLibraryW == 0)
            {
                fRes = CloseHandle(hProcess);
                return false;
            }

            //выделяем страницу памяти в wow и получаем адрес этой страницы
            UInt32 malloc = VirtualAllocEx(hProcess, 0, 0x1000, 0x00001000, 0x04);

            if (malloc == 0)
            {
                fRes = CloseHandle(hProcess);
                return false;
            }

            //пишем путь к файлу в эту страницу, чтобы wow видел путь откуда ему грузить либу
            fRes = WriteProcessMemory(hProcess, malloc, Encoding.Unicode.GetBytes(full_path), full_path.Length * 2, 0);

            if (!fRes)
            {
                fRes = VirtualFreeEx(hProcess, malloc, 0, 0x8000);
                fRes = CloseHandle(hProcess);
                return false;
            }

            //запускаем поток внутри wow по адресу loadLibraryW, а в качестве аргумента для потока указываем путь к либе
            UInt32 hThread = CreateRemoteThread(hProcess, 0, 0, loadLibraryW, malloc, 0, 0);

            if (hThread == 0)
            {
                fRes = VirtualFreeEx(hProcess, malloc, 0, 0x8000);
                fRes = CloseHandle(hProcess);
                return false;
            }

            UInt32 exitCode = 0;

            //ждем завершения потока
            UInt32 wRes = WaitForSingleObject(hThread, 0xffffffff);
            //можно увидеть, что в exitCode лежит адрес загруженной либы byster
            fRes = GetExitCodeThread(hThread, ref exitCode);

            //необходимо закрывать хэндлы и чистить память
            fRes = CloseHandle(hThread);
            fRes = VirtualFreeEx(hProcess, malloc, 0, 0x8000);
            fRes = CloseHandle(hProcess);
            return true;
        }

        static private Queue<InjectInfo> injectQueue;

        public delegate void InjectQueueChangedDelegate(InjectInfo changedElement, InjectorStatusCode injectorStatusCode);

        public static event InjectQueueChangedDelegate InjectQueueUpdated;

        private static Thread injectionThread;



        public static void Init()
        {
            string _valueName = HashCalc.GetMD5Hash(Environment.MachineName);
            coreFolderName = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", _valueName, "") as string;
            if (string.IsNullOrWhiteSpace(coreFolderName))
            {
                Random random = new Random();
                string newFolderName = HashCalc.GetRandomString(random.Next(30) + 15);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", _valueName, (coreFolderName = newFolderName));
            }
            injectQueue = new Queue<InjectInfo>();
            injectionThread = new Thread(new ThreadStart(ThreadMethod));
            injectionThread.Start();
            Branch = "master";
            if (Directory.Exists(Path.Combine(Path.GetTempPath(), "Byster"))) Directory.Delete(Path.Combine(Path.GetTempPath(), "Byster"), true);
            if (!Directory.Exists(FullLibPath)) Directory.CreateDirectory(FullLibPath);
            foreach (var file in Directory.GetFiles(FullLibPath))
            {
                try
                {
                    File.Delete(Path.Combine(FullLibPath, file));
                }
                catch
                {
                }
            }
            InjectQueueUpdated += baseInjectQueueChangedHandler;
        }

        static public void Close()
        {
            injectionThread.Abort();
        }

        private static void baseInjectQueueChangedHandler(InjectInfo changedElement, InjectorStatusCode injectorStatusCode)
        {
            switch (injectorStatusCode)
            {
                default:
                case InjectorStatusCode.INJECTED_OK:
                    LogInfo("Injector", "Инжект завершён");
                    changedElement.InjectInfoStatusCode = InjectInfoStatusCode.INJECTED_OK;
                    Task taskToDelete = new Task(() =>
                    {
                        Thread.Sleep(60000);
                        changedElement.InjectInfoStatusCode = InjectInfoStatusCode.INACTIVE;
                    });
                    taskToDelete.Start();
                    break;
                case InjectorStatusCode.ERROR_WHILE_DOWNLOADING_LIB:
                case InjectorStatusCode.ERROR_WHILE_INJECTING:
                case InjectorStatusCode.ERROR_PROCESS_NOT_FOUND:
                case InjectorStatusCode.ERROR_PROCESS_NOT_DECLARED:
                    LogWarn("Injector", "Ошибка инжекта", "Статус-код:", injectorStatusCode.ToString(), $"ID Инжекта: {changedElement.InjectInfoId}");
                    changedElement.InjectInfoStatusCode = InjectInfoStatusCode.INACTIVE;
                    break;
                case InjectorStatusCode.ADDED_OK:
                    LogInfo("Injector", "Добавлен инжект в очередь", $"ID Инжекта: {changedElement.InjectInfoId}");
                    changedElement.InjectInfoStatusCode = InjectInfoStatusCode.ENEQUEUED;
                    break;
                case InjectorStatusCode.LIBRARY_DOWNLOADING_STARTED:
                    LogInfo("Injector", "Скачивание библиотеки начато", $"ID Инжекта: {changedElement.InjectInfoId}");
                    changedElement.InjectInfoStatusCode = InjectInfoStatusCode.DOWNLOADING;
                    break;
                case InjectorStatusCode.INJECTION_STARTED:
                    LogInfo("Injector", "Запуск инжекта", $"ID Инжекта: {changedElement.InjectInfoId}");
                    changedElement.InjectInfoStatusCode = InjectInfoStatusCode.INJECTING;
                    break;
            }
        }

        static public void AddProcessToInject(InjectInfo injectingProcessInfo)
        {
            if (injectingProcessInfo.InjectInfoStatusCode != InjectInfoStatusCode.INACTIVE) return;
            if (injectingProcessInfo.ProcessId == 0)
            {
                InjectQueueUpdated.Invoke(injectingProcessInfo, InjectorStatusCode.ERROR_PROCESS_NOT_DECLARED);
                return;
            }
            injectQueue.Enqueue(injectingProcessInfo);
            InjectQueueUpdated?.Invoke(injectingProcessInfo, InjectorStatusCode.ADDED_OK);
        }

        static public void ThreadMethod()
        {
            while (true)
            {
                if (injectQueue.Count > 0)
                {
                    InjectInfo injectingProcess = injectQueue.Dequeue();

                    InjectQueueUpdated?.Invoke(injectingProcess, InjectorStatusCode.LIBRARY_DOWNLOADING_STARTED);
                    var response = Rest.Post(new RestRequest("launcher/get_lib").AddJsonBody(new RestLibRequest()
                    {
                        branch = Branch,
                    }));
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        InjectQueueUpdated?.Invoke(injectingProcess, InjectorStatusCode.ERROR_WHILE_DOWNLOADING_LIB);
                        continue;
                    }
                    string dllPath = "";
                    do
                    {
                        dllPath = Path.Combine(FullLibPath, $"{HashCalc.GetRandomString(25)}.dll");
                    }
                    while (File.Exists(dllPath));
                    File.WriteAllBytes(dllPath, response.RawBytes);
                    InjectQueueUpdated?.Invoke(injectingProcess, InjectorStatusCode.INJECTION_STARTED);
                    PreInjection?.Invoke();
                    bool injectionResult = Inject(injectingProcess.ProcessId, dllPath);
                    PostInjection?.Invoke();
                    if (injectionResult)
                    {
                        InjectQueueUpdated?.Invoke(injectingProcess, InjectorStatusCode.INJECTED_OK);
                    }
                    else
                    {
                        InjectQueueUpdated?.Invoke(injectingProcess, InjectorStatusCode.ERROR_WHILE_INJECTING);
                    }
                }
                Thread.Sleep(100);
            }
        }
    }

    public class InjectInfo : INotifyPropertyChanged
    {
        private static int idOfInjectInfo = 0;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public int InjectInfoId { get; private set; }
        public InjectInfo()
        {
            InjectInfoId = idOfInjectInfo++;
        }
        private InjectInfoStatusCode injectInfoStatusCode;
        public InjectInfoStatusCode InjectInfoStatusCode
        {
            get => injectInfoStatusCode;
            set
            {
                injectInfoStatusCode = value;
                OnPropertyChanged("InjectInfoStatusCode");
            }
        }

        private UInt32 processId;
        public UInt32 ProcessId
        {
            get { return processId; }
            set
            {
                processId = value;
                OnPropertyChanged("ProcessId");
            }
        }
    }

    public enum InjectorStatusCode
    {
        INJECTED_OK = 0,
        ERROR_WHILE_INJECTING = 1,
        ERROR_WHILE_DOWNLOADING_LIB = 2,

        ADDED_OK = 3,
        ERROR_PROCESS_NOT_FOUND = 4,
        ERROR_PROCESS_NOT_DECLARED = 5,

        LIBRARY_DOWNLOADING_STARTED = 6,
        INJECTION_STARTED = 7,
    }

    public enum InjectInfoStatusCode
    {
        INACTIVE = 0,
        ENEQUEUED = 1,
        DOWNLOADING = 2,
        INJECTING = 3,
        INJECTED_OK = 4,

        ERROR = 5,
    }
}