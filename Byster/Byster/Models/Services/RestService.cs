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
using Byster.Localizations.Tools;
using Byster.Models.ViewModels;
using Byster.Views;
using Newtonsoft.Json;
using System.Threading;

namespace Byster.Models.Services
{
    public class RestService
    {
        private RestClient client;
        public string LastError { get; set; }
        public event Action MultipleConnectionErrorsDetected;
        private int connectionErrorCounter = 0;
        private Timer resetConnectionErrorAccidentCounterTimer;
        private bool multipleConnectionErrorEventCalled = false;
        public RestService(RestClient _client)
        {
            if(_client == null)
            {
                throw new ArgumentNullException(nameof(_client));
            }
            client = _client;
            resetConnectionErrorAccidentCounterTimer = new Timer((obj) =>
            {
                resetConnectionErrorAccidentsCounter();
            }, null, 0, 60000);

        }

        public IEnumerable<ActiveRotationViewModel> GetActiveRotationCollection()
        {
            var response = client.Post<List<RestRotationWOW>>(new RestRequest("shop/my_subscriptions"));
            if (checkHTTPStatusCode(response.StatusCode)) return null;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = JsonConvert.DeserializeObject<BaseResponse>(response.Content)?.error ?? "No Error Received";
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
            if (checkHTTPStatusCode(response.StatusCode)) return null;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = JsonConvert.DeserializeObject<BaseResponse>(response?.Content ?? "")?.error ?? "No Error Received";
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
            IRestResponse<RestBuyResponse> response;
            if(cart.Bonuses >= cart.Sum)
            {
                response = client.Post<RestBuyResponse>(new RestRequest("shop/buy").AddJsonBody(new RestBuyRequest()
                {
                    bonuses = cart.Bonuses,
                    items = products,
                    payment_system_id = 0,
                }));
            }
            else
            {
                response = client.Post<RestBuyResponse>(new RestRequest("shop/buy").AddJsonBody(new RestBuyRequest()
                {
                    bonuses = cart.Bonuses,
                    payment_system_id = cart.PaymentSystemId,
                    items = products,
                }));
            }
            
            if (checkHTTPStatusCode(response.StatusCode)) return (false, null);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = response.Data.error;
                Log("Ошибка при выполнении запроса покупки - ", response.Data?.error ?? "Нет ответа сервера", " - ", response.ErrorMessage);
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
            if (checkHTTPStatusCode(response.StatusCode)) return false;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = response.Data.error;
                Log("Ошибка при выполнении запроса на тест - ", response.Data.error, " - ", response.ErrorMessage);
                return false;
            }
            return true;
        }

        public (string ,string, int, string, bool?) GetUserInfo()
        {
            var response = client.Post<RestUserInfoResponse>(new RestRequest("launcher/info"));
            if (checkHTTPStatusCode(response.StatusCode)) return (null, null, 0, null, null);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = response.Data.error;
                Log("Ошибка обновления данных пользователя - ", response.Data.error, " - ", response.ErrorMessage);
                return (null, null, 0, null, null);
            }
            Log("Обновлены данные пользователя");
            return (response.Data.username, response.Data.referral_code, Convert.ToInt32(Math.Floor(response.Data.balance)), response.Data.currency, response.Data.encryption);
        }

        public bool? GetEncryptStatus()
        {
            var response = client.Get<RestUserInfoResponse>(new RestRequest("launcher/info"));
            if (checkHTTPStatusCode(response.StatusCode)) return null;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = response.Data.error;
                Log("Ошибка получения статуса encryption пользователя - ", response.Data.error, " - ", response.ErrorMessage);
                return null;
            }
            Log("Получен статус encryption");
            return response.Data.encryption;
        }

        public void SetEncryptStatus(bool newStatus)
        {
            var response = client.Post<RestUserInfoResponse>(new RestRequest("launcher/toggle_encryption").AddJsonBody(new RestEncryptionRequest()
            {
                enable = newStatus,
            }));
            if (checkHTTPStatusCode(response.StatusCode)) return;
            if (response.StatusCode != System.Net.HttpStatusCode.OK || !string.IsNullOrEmpty(response.Data.error))
            {
                LastError = response.Data.error;
                Log("Ошибка установки статуса encryption пользователя - ", response.Data.error, " - ", response.ErrorMessage);
                return;
            }
            Log("Установлен новый encryption");
            return;
        }

        public IEnumerable<PaymentSystem> GetAllPaymentSystemList()
        {
            bool isTesterOrDeveloper = !(GetUserType() == BranchType.MASTER);
            var response = client.Get<List<RestPaymentSystem>>(new RestRequest("shop/payment_systems"));
            if (checkHTTPStatusCode(response.StatusCode)) return null;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = JsonConvert.DeserializeObject<BaseResponse>(string.IsNullOrEmpty(response?.Content ?? "") ? "" : response.Content)?.error ?? "No Error Received";
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
                    Name = Localizator.LoadedLocalizationInfo.Language == "Русский" ? item.name : item.name_en,
                    Description = Localizator.LoadedLocalizationInfo.Language == "Русский" ? item.description : item.description_en,
                });
            }
            Log("Получены данные платёжных систем");
            return result;
        }
        List<string> updatedActionIds = new List<string>();

        public bool GetActionState(string sessionId)
        {
            var response = client.Get<List<RestAction>>(new RestRequest("launcher/ping"));
            if (checkHTTPStatusCode(response.StatusCode)) return false;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = JsonConvert.DeserializeObject<BaseResponse>(string.IsNullOrEmpty(response?.Content ?? "") ? "" : response.Content)?.error ?? "No Error Received";
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
            if (checkHTTPStatusCode(response.StatusCode)) return BranchType.UNKNOWN;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = response.Data.error;
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
            if (checkHTTPStatusCode(response.StatusCode)) return false;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = response.Data.error;
                Log("Ошибка изменения пароля", response.Data.error, " - ", response.ErrorMessage ?? "{Ошибка преобразования}");
                return false;
            }
            Log("Пароль изменён");
            return true;
        }

        public List<RestDeveloperRotation> ExecuteDeveloperRotationRequest()
        {
            var response = client.Post<List<RestDeveloperRotation>>(new RestRequest("launcher/git_pull"));
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = JsonConvert.DeserializeObject<BaseResponse>(response.Content).error;
                Log("Ошибка получения данных ротаций для разработчиков", LastError, " - ", response.ErrorMessage);
                return null;
            }
            return response.Data;
        }

        public RestDeveloperRotation ExecuteAddRtationRequest(string name,
                                            string description,
                                            int type,
                                            string klass,
                                            string spec,
                                            string roletype)
        {
            var response = client.Post<RestAddRotationResponse>(new RestRequest("launcher/add_rotation").AddJsonBody(new RestAddRotationRequest()
            {
                name = name,
                description = description,
                type = type,
                klass = klass,
                specialization = spec,
                role_type = roletype,
            }));
            if (checkHTTPStatusCode(response.StatusCode)) return null;
            if (!string.IsNullOrEmpty(response.Data?.error))
            {
                LastError = response.Data.error;
                Log("Ошибка создания ротации разработчиков", LastError, " - ", response.ErrorMessage);
                return null;
            }
            return response.Data;
        }

        public bool ExecuteLinkEmailRequest(string email)
        {
            //var response = client.Post<>()
            Log("ПРЕДУПРЕЖДЕНИЕ: Использована нереализованная функция");
            return true;
        }

        public bool ExecuteCouponRequest(string couponCode)
        {
            var response = client.Post<RestCouponResponse>(new RestRequest("launcher/redeem_coupon").AddJsonBody(new RestCouponRequest()
            {
                coupon_code = couponCode,
            }));
            if (checkHTTPStatusCode(response.StatusCode)) return false;
            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LastError = response.Data.error;
                Log("Ошибка активации купона", response.Data.error);
                return false;
            }
            return true;
        }

        private void addConnectoinErrorAccident()
        {
            if (++connectionErrorCounter >= 3 && !multipleConnectionErrorEventCalled)
            {
                multipleConnectionErrorEventCalled = true;
                MultipleConnectionErrorsDetected?.Invoke();
            }
        }

        private void resetConnectionErrorAccidentsCounter()
        {
            connectionErrorCounter = 0;
            multipleConnectionErrorEventCalled = false;
        }

        private bool checkHTTPStatusCode(System.Net.HttpStatusCode statusCode)
        {
            System.Net.HttpStatusCode[] restrictedCodes = new System.Net.HttpStatusCode[]
            {
                System.Net.HttpStatusCode.Forbidden,
                System.Net.HttpStatusCode.BadGateway,
                System.Net.HttpStatusCode.GatewayTimeout,
                System.Net.HttpStatusCode.RequestTimeout,
                System.Net.HttpStatusCode.HttpVersionNotSupported,
                System.Net.HttpStatusCode.ServiceUnavailable,
            };
            if(restrictedCodes.Contains(statusCode) || (int)statusCode == 0)
            {
                LastError = "Ошибка соединения с сервером";
                Log("Сервер недоступен", statusCode.ToString());
                addConnectoinErrorAccident();
                return true;
            }
            else
            {
                //resetConnectionErrorAccidentsCounter();
                return false;
            }
        }
    }
}
