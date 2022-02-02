using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using Byster.Models.Utilities;
using static Byster.Models.Utilities.BysterLogger;


namespace Byster.Models.Utilities
{
    public class BackgroundImageDownloader
    {
        public static string ImageRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterImages\\";
        public static Queue<ImageItem> ItemsToDownload { get; set; }
        public static List<ImageItem> DownloadedItems { get; set; }

        private static Thread downloadingThread;

        //private static SuspendToken suspendToken = new SuspendToken();
        public static Mutex SuspendMutex { get; set; } = new Mutex();
        public static void Init()
        {
            ItemsToDownload = new Queue<ImageItem>();
            DownloadedItems = new List<ImageItem>();

            if (!Directory.Exists(ImageRootPath)) Directory.CreateDirectory(ImageRootPath);

            readImageDirectory();


            downloadingThread = new Thread(threadMethod);
            downloadingThread.IsBackground = true;
            downloadingThread.Name = "Downloader Of Images";
            downloadingThread.Start();
        }

        public static void Close()
        {
            downloadingThread.Abort();
        }

        private static void threadMethod()
        {
            while(true)
            {
                SuspendMutex.WaitOne();
                //suspendToken.mutex.WaitOne();
                //suspendToken.AcceptSuspend();
                //if(!suspendToken.GetSuspendStatus())
                //{
                    if (ItemsToDownload.Count > 0)
                    {
                        try
                        {
                            var downloadingItem = ItemsToDownload.Dequeue();
                            Log("Попытка скачивания", "Url", downloadingItem.PathOfNetworkSource);
                            WebClient client = new WebClient();
                            byte[] buffer = client.DownloadData(downloadingItem.PathOfNetworkSource);
                            File.WriteAllBytes(downloadingItem.PathOfLocalSource, buffer);
                            downloadingItem.IsDownLoaded = true;
                            downloadingItem.PathOfCurrentLocalSource = downloadingItem.PathOfLocalSource;
                            DownloadedItems.Add(downloadingItem);
                            client.Dispose();
                            Log("Скачан файл изображения", "Путь:", downloadingItem.PathOfCurrentLocalSource, "Url:", downloadingItem.PathOfNetworkSource);
                        }
                        catch (Exception ex)
                        {
                            Log("Ошибка при скачивании", ex.Message, " - ", ex.ToString());
                        }
                    }
                SuspendMutex.ReleaseMutex();
                //}
                Thread.Sleep(100);
            }
        }

        private static void readImageDirectory()
        {
            List<string> files = Directory.GetFiles(ImageRootPath).ToList();
            foreach(string file in files)
            {
                DownloadedItems.Add(new ImageItem()
                {
                    PathOfCurrentLocalSource = file,
                    PathOfLocalSource = file,
                    IsDownLoaded = true,
                });
            }
        }

        public static ImageItem GetImageItemByNetworkPath(string networkPath)
        {
            if(!string.IsNullOrEmpty(networkPath))
            {
                if(getExtensionOfNetworkSource(networkPath) == ".mp4")
                {
                    ImageItem imageItem = new ImageItem()
                    {
                        PathOfNetworkSource = networkPath,
                        PathOfCurrentLocalSource = "/Resources/Images/video-placeholder.png",
                        PathOfLocalSource = "/Resources/Images/video-placeholder.png",
                    };
                    return imageItem;
                }
                else
                {
                    ImageItem imageItem = DownloadedItems.FirstOrDefault((item) => item.PathOfLocalSource.Split('\\').Contains(HashCalc.GetMD5Hash(networkPath) + getExtensionOfNetworkSource(networkPath)));
                    if (imageItem != null)
                    {
                        return imageItem;
                    }
                    else
                    {
                        imageItem = ItemsToDownload.FirstOrDefault((item) => item.PathOfLocalSource.Split('\\').Contains(HashCalc.GetMD5Hash(networkPath) + getExtensionOfNetworkSource(networkPath)));
                        if (imageItem != null)
                        {
                            return imageItem;
                        }
                        else
                        {
                            return AddToDownloadQueueOfNetworkSource(networkPath);
                        }
                    }
                }
            }
            return null;
        }

        public static ImageItem AddToDownloadQueueOfNetworkSource(string pathToImage)
        {
            if (string.IsNullOrEmpty(pathToImage)) return null;
            string newLocalPath = ImageRootPath + HashCalc.GetMD5Hash(pathToImage) + getExtensionOfNetworkSource(pathToImage);
            //File.Create(newLocalPath).Close();
            ImageItem creatingItem = new ImageItem()
            {
                PathOfNetworkSource = pathToImage,
                PathOfCurrentLocalSource = "/Resources/Images/image-placeholder.png",
                PathOfLocalSource = newLocalPath,
                IsDownLoaded = false,
            };
            ItemsToDownload.Enqueue(creatingItem);
            return creatingItem;
        }

        public static void Suspend()
        {
            SuspendMutex.WaitOne();
        }

        public static void Resume()
        {
            SuspendMutex.ReleaseMutex();
        }

        private static string getExtensionOfNetworkSource(string netPath)
        {
            if(!string.IsNullOrEmpty(netPath))
            {
                string ext = ".";
                string[] parts = netPath.Split('.');
                if(parts.Last().Length <= 3 && parts.Last().Length > 0)
                {
                    ext += parts.Last();
                    return ext;
                }
                return ".png";
            }
            return ".png";
        }
    }

    public class ImageItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private string pathOfCurrentLocalSource;
        public string PathOfCurrentLocalSource
        {
            get => pathOfCurrentLocalSource;
            set
            {
                pathOfCurrentLocalSource = value;
                OnPropertyChanged("PathOfCurrentLocalSource");
            }
        }
        private string pathOfNetworkSource;
        public string PathOfNetworkSource
        {
            get => pathOfNetworkSource;
            set
            {
                pathOfNetworkSource = value;
                OnPropertyChanged("PathOfNetworkSource");
            }
        }
        private string pathOfLocalSource;
        public string PathOfLocalSource
        {
            get => pathOfLocalSource;
            set
            {
                pathOfLocalSource = value;
                OnPropertyChanged("PathOfLocalSource");
            }
        }
        private bool isDownloaded;
        public bool IsDownLoaded
        {
            get => isDownloaded;
            set
            {
                isDownloaded = value;
                OnPropertyChanged("IsDownloaded");
            }
        }

        public ImageItem()
        {
            IsDownLoaded = false;
        }
    }

    class SuspendToken
    {
        Mutex mutex = new Mutex();
        bool isSuspendRequestCreated = false;
        bool isSuspended = false;
        public void RequestSuspend()
        {
            mutex.WaitOne();
            if (isSuspendRequestCreated || isSuspended) return;
            isSuspendRequestCreated = true;
            mutex.ReleaseMutex();
            while (!isSuspended)
            {
               
            }
            return;
        }

        public bool GetSuspendRequestStatus()
        {
            return isSuspendRequestCreated;
        }

        public bool GetSuspendStatus()
        {
            return isSuspended;
        }

        public void AcceptSuspend()
        {
            if(isSuspendRequestCreated)
            {
                isSuspendRequestCreated = false;
                isSuspended = true;
            }
        }

        public void Resume()
        {
            if(isSuspended) isSuspended = false;
        }
    }
}
