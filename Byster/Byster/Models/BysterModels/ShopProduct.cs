using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.RestModels;
using Byster.Models.BysterModels;

namespace Byster.Models.BysterModels
{
    public class ShopProduct
    {
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
            ImageUri = !string.IsNullOrEmpty(RestShopProduct.image_url) ? RestShopProduct.image_url :
                Rotations.Count == 1 ? Rotations[0].ImageUri :
                "/Resources/Images/image-placeholder.jpg";

        }
    }
}
