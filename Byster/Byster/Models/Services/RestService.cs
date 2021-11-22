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
using Byster.Models.ViewModels;

namespace Byster.Models.Services
{
    public class RestService
    {
        private RestClient client;

        public RestService(RestClient _client)
        {
            if(_client == null)
            {
                throw new ArgumentNullException(nameof(_client));
            }
            client = _client;
        }

        public ObservableCollection<ActiveRotationViewModel> GetActiveRotationCollection()
        {
            var response = client.Post<List<RestRotationWOW>>(new RestRequest("shop/my_subscriptions"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка получения данных с сервера - ", response.Data, " - ", response.ErrorMessage);
                return null;
            }
            List<RestRotationWOW> responseRotations = response.Data;
            ObservableCollection<ActiveRotationViewModel> res = new ObservableCollection<ActiveRotationViewModel>();
            foreach(var restRotation in responseRotations)
            {
                res.Add(new ActiveRotationViewModel(restRotation));
            }
            Log("Обновлены данные активных ротаций");
            return res;
        }

        public ObservableCollection<ShopProductInfoViewModel> GetAllProductCollection()
        {
            var response = client.Post<List<RestShopProduct>>(new RestRequest("shop/product_list"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка получения данных с сервера - ", response.Data, " - ", response.ErrorMessage);
                return null;
            }
            List<RestShopProduct> responseProducts = response.Data;
            ObservableCollection<ShopProductInfoViewModel> res = new ObservableCollection<ShopProductInfoViewModel>();
            foreach(var product in responseProducts)
            {
                res.Add(new ShopProductInfoViewModel(new ShopProduct(product)));
            }
            Log("Обновлены данные списка продуктов магазина");
            return res;
        }
        
        public (bool, string) ExecuteBuyRequest(Cart cart)
        {
            List<RestBuyProduct> products = new List<RestBuyProduct>();
            foreach (var product in cart.Products)
            {
                products.Add(new RestBuyProduct()
                {
                    product_id = product.Item1,
                    amount = product.Item2,
                });
            }
            var response = client.Post<RestBuyResponse>(new RestRequest("shop/buy").AddJsonBody(new RestBuyRequest()
            {
                bonuses = cart.Bonuses,
                payment_system_id = 3,
                items = products,
            }));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка при выполнении запроса покупки - ", response.Data.error, " - ", response.ErrorMessage);
                return (false, null);
            }
            Log("Выполнен запрос на покупку продукта - ", response.Data.payment_url);
            return (true, response.Data.payment_url);
        }

        public bool ExecuteTestRequest(int id)
        {
            var response = client.Post<BaseResponse>(new RestRequest("shop/test").AddJsonBody(new RestTestRequest()
            {
                product_id = id,
            }));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка при выполнении запроса на тест - ", response.Data.error, " - ", response.ErrorMessage);
                return false;
            }
            return true;
        }

        public (string ,string, int) GetUserInfo()
        {
            var response = client.Post<RestUserInfoResponse>(new RestRequest("launcher/info"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка обновления данных пользователя - ", response.Data.error, " - ", response.ErrorMessage);
                return (null, null, 0);
            }
            Log("Обновлены данные пользователя");
            return (response.Data.username, response.Data.referral_code, Convert.ToInt32(response.Data.balance));
        }

        public bool GetActionState(string sessionId)
        {
            var response = client.Get<object>(new RestRequest("launcher/ping"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка обновления данных - ", (response.Data as BaseResponse).error, " - ", response.ErrorMessage);
                return false;
            }
            List<RestAction> actions = response.Data as List<RestAction>;
            foreach(var action in actions)
            {
                if(action.session == sessionId && (action.action_type == 1 || action.action_type == 11))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
