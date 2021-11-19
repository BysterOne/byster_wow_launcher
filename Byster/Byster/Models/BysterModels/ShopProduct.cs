using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.RestModels;
using Byster.Models.BysterModels;
using Byster.Models.Utilities;
using System.ComponentModel;

namespace Byster.Models.BysterModels
{
    public class ShopProduct : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUri { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Duration { get; set; }

        public string Currency { get; set; }
        public bool IsTestable { get; set; }
        public List<Media> Medias { get; set; }
        public List<ShopRotation>Rotations { get; set; }
        public bool IsPack { get; set; }

        public ShopProduct() { }

        public ShopProduct(RestShopProduct RestShopProduct)
        {
            Name = RestShopProduct.name;
            Description = RestShopProduct.description;
            Price = RestShopProduct.price;
            Currency = RestShopProduct.currency;
            Duration = RestShopProduct.duration;
            Medias = new List<Media>();
            foreach(var restMedia in RestShopProduct.media)
            {
                Medias.Add(new Media(restMedia.url, Media.GetMediaTypeByName(restMedia.type)));
            }
            Rotations = new List<ShopRotation>();
            foreach(var restRotation in RestShopProduct.rotations)
            {
                Rotations.Add(new ShopRotation(restRotation));
            }
            IsPack = Rotations.Count > 1;
            IsTestable = RestShopProduct.can_test;


            if (!string.IsNullOrEmpty(RestShopProduct.image_url))
            {
                ImageItem item = BackgroundPhotoDownloader.GetImageItemByNetworkPath(RestShopProduct.image_url);
                if (item != null)
                {
                    item.PropertyChanged += ImageUriPropChanged;
                    ImageUri = item.PathOfCurrentLocalSource;
                }
            }
            else
            {
                ImageUri = Rotations.Count == 1 ? Rotations[0].ImageUri :
                "/Resources/Images/image-placeholder.jpg";
            }

        }

        private void ImageUriPropChanged(object sender, PropertyChangedEventArgs e)
        {
            ImageItem item = (ImageItem)sender;
            ImageUri = item.PathOfCurrentLocalSource;
            OnPropertyChanged("ImageUri");
        }
    }
}
