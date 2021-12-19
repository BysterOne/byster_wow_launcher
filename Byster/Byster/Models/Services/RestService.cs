using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.RestModels;
using RestSharp;
using System.Collections.ObjectModel;
using Byster.Models.BysterModels;
using static Byster.Models.Utilities.BysterLogger;
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

        public IEnumerable<ActiveRotationViewModel> GetActiveRotationCollection()
        {
            var response = client.Post<List<RestRotationWOW>>(new RestRequest("shop/my_subscriptions"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка получения данных с сервера - ", response.Data, " - ", response.ErrorMessage);
                return null;
            }
            List<RestRotationWOW> responseRotations = response.Data;
            List<ActiveRotationViewModel> res = new List<ActiveRotationViewModel>();
            foreach(var restRotation in responseRotations)
            {
                res.Add(new ActiveRotationViewModel(restRotation));
            }
            Log("Обновлены данные активных ротаций");
            return res;
        }

        public IEnumerable<ShopProductInfoViewModel> GetAllProductCollection()
        {
            var response = client.Post<List<RestShopProduct>>(new RestRequest("shop/product_list"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка получения данных с сервера - ", response.Data, " - ", response.ErrorMessage);
                return null;
            }
            List<RestShopProduct> responseProducts = response.Data;
            List<ShopProductInfoViewModel> res = new List<ShopProductInfoViewModel>();
            foreach(var product in responseProducts)
            {
                res.Add(new ShopProductInfoViewModel(new ShopProduct(product)));
            }
            Log("Обновлены данные списка продуктов магазина");
            return res;
        }
        
        public (bool, string) ExecuteBuyRequest(Cart cart, int paymentSystemId)
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
                payment_system_id = paymentSystemId,
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
        //"{\"payment_url\": \"https://api.byster.ru/shop/test_payment_webhook?id=21&username=byster_service&token=6f22f01ee45c45f3bca856a604664d6d\", \"status\": \"init\"}"
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

        public IEnumerable<PaymentSystem> GetAllPaymentSystemList()
        {
            bool isTesterOrDeveloper = !(GetUserType() == BranchType.MASTER);
            var response = client.Get<List<RestPaymentSystem>>(new RestRequest("shop/payment_systems"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка получения списка платёжных систем", response?.Content ?? "{Ошибка преобразования}", " - ", response?.ErrorMessage ?? "{Ошибка преобразования}");
                return null;
            }
            List<PaymentSystem> result = new List<PaymentSystem>();
            foreach(var item in response.Data)
            {
                if (item.name.ToLower().Contains("тест") && !isTesterOrDeveloper) continue;
                result.Add(new PaymentSystem()
                {
                    Id = item.id,
                    Name = item.name,
                    Description = item.description,
                });
            }
            Log("Получены данные платёжных систем");
            return result;
        }
        List<string> updatedActionIds = new List<string>();
        public bool GetActionState(string sessionId)
        {
            var response = client.Get<List<RestAction>>(new RestRequest("launcher/ping"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка обновления данных", response.Content.ToString(), " - ", response.ErrorMessage);
                return false;
            }
            bool res = false;
            List<RestAction> actions = response.Data;
            foreach(var action in actions)
            {
                if(!updatedActionIds.Contains(action.action_id) && action.session == sessionId && (action.action_type == 1 || action.action_type == 11))
                {
                    updatedActionIds.Add(action.action_id);
                    res = true;
                }
            }
            return res;
        }

        public BranchType GetUserType()
        {
            var response = client.Get<RestBranchResponse>(new RestRequest("launcher/branch_choices"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка получения данных пользователя", response.Data.error, " - ", response.ErrorMessage ?? "{Ошибка преобразования}");
                return BranchType.UNKNOWN;
            }
            return response.Data.dev ? BranchType.DEVELOPER :
                response.Data.test ? BranchType.TEST :
                response.Data.master ? BranchType.MASTER : BranchType.UNKNOWN;
        }

        public bool ExecuteChangePasswordRequest(string newPwdHash)
        {
            var response = client.Post<RestChangePasswordResponse>(new RestRequest("launcher/change_password").AddJsonBody(new RestChangePasswordRequest()
            {
                new_password = newPwdHash,
            }));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log("Ошибка изменения пароля", response.Data.error, " - ", response.ErrorMessage ?? "{Ошибка преобразования}");
                return false;
            }
            Log("Пароль изменён");
            return true;
        }

        public bool ExecuteLinkEmailRequest(string email)
        {
            //var response = client.Post<>()
            Log("ПРЕДУПРЕЖДЕНИЕ Использована нереализованная функция");
            return true;
        }
    }
}
