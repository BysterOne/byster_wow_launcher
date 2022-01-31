using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.Utilities;
using System.ComponentModel;
using static Byster.Models.Utilities.BysterLogger;


namespace Byster.Models.BysterModels
{
    public class Media : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ImageItem ImageItem { get; set; }

        private string uri;
        public string Uri
        {
            get => uri;
            set
            {
                uri = value;
                OnPropertyChanged("Uri");
                Log($"Получен новый Uri: {value}");
            }
        }
        public MediaTypes Type { get; set; }

        public static MediaTypes GetMediaTypeByName(string name)
        {
            List<string> names = new List<string>()
            {
                null,
                "img",
                "video"
            };
            return (MediaTypes)names.IndexOf(name.ToLower());
        }

        public Media(string uri, MediaTypes type)
        {
            ImageItem = BackgroundImageDownloader.GetImageItemByNetworkPath(uri);
            ImageItem.PropertyChanged += ImageItemChanged;
            Uri = ImageItem.PathOfCurrentLocalSource;
            Type = type;
        }

        private void ImageItemChanged(object sender, PropertyChangedEventArgs e)
        {
            ImageItem item = (ImageItem)sender;
            Uri = item.PathOfCurrentLocalSource;
        }
    }

    public enum MediaTypes
    {
        Unknown = 0,
        Image = 1,
        Video = 2,
    }
}
