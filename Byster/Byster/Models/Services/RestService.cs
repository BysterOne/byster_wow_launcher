using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.RestModels;
using RestSharp;
using System.Collections.ObjectModel;
using Byster.Models.BysterModels;
using static Byster.Models.Utilities.Logger;


namespace Byster.Models.Services
{
    public class RestService
    {
        private RestClient client;
        

        public RestService(RestClient _client)
        {
            if(client == null)
            {
                throw new ArgumentNullException(nameof(_client));
            }
            client = _client;
        }

        public ObservableCollection<ActiveRotation> GetActiveRotationCollection()
        {
            var response = client.Post<object>(new RestRequest("shop/my_subscriptions"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка получения данных с сервера - ", response.Data, " - ", response.ErrorMessage);
                return null;
            }
            List<RestRotationWOW> responseRotations = response.Data as List<RestRotationWOW>;
            ObservableCollection<ActiveRotation> res = new ObservableCollection<ActiveRotation>();
            foreach(var restRotation in responseRotations)
            {
                res.Add(new ActiveRotation(restRotation));
            }
            Log("Обновлены данные активных ротаций");
            return res;
        }

        public ObservableCollection<ShopProductInfo> GetAllProductCollection()
        {
            var response = client.Post<object>(new RestRequest("shop/product_list"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка получения данных с сервера - ", response.Data, " - ", response.ErrorMessage);
                return null;
            }
            List<RestShopProduct> responseProducts = response.Data as List<RestShopProduct>;
            ObservableCollection<ShopProductInfo> res = new ObservableCollection<ShopProductInfo>();
            foreach(var product in responseProducts)
            {
                res.Add(new ShopProductInfo(new ShopProduct(product)));
            }
            Log("Обновлены данные списка продуктов магазина");
            return res;
        }
    }
}
