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

namespace Byster.Models.Utilities
{
    public class BackgroundPhotoDownloader
    {
        public static string ImageRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterImages\\";
        public static string InfoFile = ImageRootPath + "info.json";
        public static Queue<ImageItem> ItemsToDownload { get; set; }
        public static List<ImageItem> DownloadedItems { get; set; }

        private static Thread downloadingThread;

        public static void Init()
        {
            ItemsToDownload = new Queue<ImageItem>();
            DownloadedItems = new List<ImageItem>();

            if (!Directory.Exists(ImageRootPath)) Directory.CreateDirectory(ImageRootPath);
            if (!File.Exists(InfoFile)) File.Create(InfoFile).Close();

            readInfoFile();

            downloadingThread = new Thread(threadMethod);
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
                if(ItemsToDownload.Count > 0)
                {
                    var downloadingItem = ItemsToDownload.Dequeue();
                    
                    WebClient client = new WebClient();
                    byte[] buffer = client.DownloadData(downloadingItem.PathOfNetworkSource);
                    File.WriteAllBytes(downloadingItem.PathOfLocalSource, buffer);
                    downloadingItem.IsDownLoaded = true;
                    downloadingItem.PathOfCurrentLocalSource = downloadingItem.PathOfLocalSource;
                    DownloadedItems.Add(downloadingItem);
                    JsonManager.UpdatePropertyInInfoFile(downloadingItem.PathOfNetworkSource, "downloaded", true);
                    JsonManager.UpdatePropertyInInfoFile(downloadingItem.PathOfNetworkSource, "local", downloadingItem.PathOfLocalSource);
                    client.Dispose();
                }
            }
        }

        private static void readInfoFile()
        {
            List<ImageItem> allImages = JsonManager.ReadAllImagesInInfoFile();

            

            foreach(var image in allImages)
            {
                if(image.IsDownLoaded)
                {
                    DownloadedItems.Add(image);
                }
                else
                {
                    ItemsToDownload.Enqueue(image);
                }
            }
        }

        public static ImageItem GetImageItemByNetworkPath(string networkPath)
        {
            if(!string.IsNullOrEmpty(networkPath))
            {
                ImageItem imageItem = DownloadedItems.Find((item) => item.PathOfNetworkSource == networkPath);
                if(imageItem != null)
                {
                    return imageItem;
                }
                else
                {
                    imageItem = ItemsToDownload.FirstOrDefault((item) => item.PathOfNetworkSource == networkPath);
                    if(imageItem != null)
                    {
                        return imageItem;
                    }
                    else
                    {
                        return AddToDownloadQueueOfNetworkSource(networkPath);
                    }
                }
            }
            return null;
        }

        public static ImageItem AddToDownloadQueueOfNetworkSource(string pathToImage)
        {
            if (string.IsNullOrEmpty(pathToImage)) return null;
            string newLocalPath = JsonManager.AddPathToInfoFile(pathToImage);
            File.Create(newLocalPath).Close();
            ImageItem creatingItem = new ImageItem()
            {
                PathOfNetworkSource = pathToImage,
                PathOfCurrentLocalSource = ImageRootPath + "placeholder.jpg",
                PathOfLocalSource = newLocalPath,
                IsDownLoaded = false,
            };
            ItemsToDownload.Enqueue(creatingItem);
            return creatingItem;
        }
    }

    public class JsonManager
    {
        public static string ImageRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterImages\\";
        public static string InfoFile = ImageRootPath + "info.json";
        
        private static List<JsonImage> openFileInfo()
        {
            string infoText = File.ReadAllText(InfoFile);
            List<JsonImage> existingImages = JsonConvert.DeserializeObject<List<JsonImage>>(infoText) ?? new List<JsonImage>();
            return existingImages;
        }

        private static void saveFileInfo(object newInfoFileContent)
        {
            File.WriteAllText(InfoFile, JsonConvert.SerializeObject(newInfoFileContent));
        }
        
        public static string AddPathToInfoFile(string pathOfNetworkSource)
        {

            List<JsonImage> images = openFileInfo();

            string partOfImagePath = ImageRootPath;
            int i = 0;
            while (File.Exists(partOfImagePath + i + ".png")) i++;
            string imagePath = partOfImagePath + i + ".png";

            images.Add(new JsonImage()
            {
                localAwaited = imagePath,
                local = ImageRootPath + "placeholder.jpg",
                netSource = pathOfNetworkSource,
                downloaded = false,
            });

            saveFileInfo(images);
            return imagePath;
        }

        public static bool UpdatePropertyInInfoFile(string pathOfNetworkSource, string propertyName, object propertyValue)
        {
            List<JsonImage> images = openFileInfo();

            JsonImage selectedImage = images.Find((image) => image.netSource == pathOfNetworkSource);

            switch(propertyName)
            {
                case "local":
                    selectedImage.local = (string)propertyValue;
                    break;
                case "downloaded":
                    selectedImage.downloaded = (bool)propertyValue;
                    break;
                case "netSource":
                    selectedImage.netSource = (string)propertyValue;
                    break;
                default:
                    return false;
            }
            saveFileInfo(images);
            return true;
        }

        public static List<ImageItem> ReadAllImagesInInfoFile()
        {
            List<JsonImage> jsonImages = openFileInfo();

            List<ImageItem> images = new List<ImageItem>();

            foreach(var jsonImage in jsonImages)
            {
                images.Add(new ImageItem()
                {
                    IsDownLoaded = jsonImage.downloaded,
                    PathOfCurrentLocalSource = jsonImage.local,
                    PathOfLocalSource = jsonImage.localAwaited,
                    PathOfNetworkSource = jsonImage.netSource,
                });
            }
            return images;
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

    public class JsonImage
    {
        public string local;
        public string netSource;
        public string localAwaited;
        public bool downloaded;
    }
}
