using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.RestModels;
using Byster.Models.BysterModels;
using Byster.Models.Utilities;
using System.ComponentModel;
using Byster.Localizations.Tools;

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
            Id = RestShopProduct.id;
            Name = Localizator.LoadedLocalizationInfo.Language == "Русский" ? RestShopProduct.name : RestShopProduct.name_en;
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
            if(Localizator.LoadedLocalizationInfo.Language == "Русский")
                Description = string.IsNullOrEmpty(RestShopProduct.description) ? Rotations[0].Description : RestShopProduct.description;
            else
                Description = string.IsNullOrEmpty(RestShopProduct.description_en) ? Rotations[0].Description : RestShopProduct.description_en;
            IsTestable = RestShopProduct.can_test;

            if(Rotations.Count == 1)
            {
                foreach(var media in Rotations[0].Medias)
                {
                    Medias.Add(media);
                }
            }

            if (!string.IsNullOrEmpty(RestShopProduct.image_url))
            {
                ImageItem item = BackgroundImageDownloader.GetImageItemByNetworkPath(RestShopProduct.image_url);
                if (item != null)
                {
                    item.PropertyChanged += ImageUriPropChanged;
                    ImageUri = item.PathOfCurrentLocalSource;
                }
            }
            else
            {
                ImageUri = Rotations.Count == 1 ? Rotations[0].ImageUri :
                "/Resources/Images/image-placeholder.png";
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
