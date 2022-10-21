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
using System.Net;

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
            if (_client == null)
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
            if (!checkAndLogResponseToError(response, "Получение данных активных ротаций")) return null;
            List<RestRotationWOW> responseRotations = response.Data;
            List<ActiveRotationViewModel> res = new List<ActiveRotationViewModel>();
            foreach (var restRotation in responseRotations)
            {
                res.Add(new ActiveRotationViewModel(restRotation));
            }
            LogInfo("Rest Service", "Обновлены данные активных ротаций");
            return res;
        }

        public IEnumerable<ShopProductInfoViewModel> GetAllProductCollection()
        {
            var response = client.Post<List<RestShopProduct>>(new RestRequest("shop/product_list"));
            if (!checkAndLogResponseToError(response, "Получение данных продуктов магазина")) return null;
            List<RestShopProduct> responseProducts = response.Data;
            List<ShopProductInfoViewModel> res = new List<ShopProductInfoViewModel>();
            foreach (var product in responseProducts)
            {
                res.Add(new ShopProductInfoViewModel(new ShopProduct(product)));
            }
            LogInfo("Rest Service", "Обновлены данные списка продуктов магазина");
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
            if (cart.Bonuses >= cart.Sum)
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
            if (!checkAndLogResponseToError(response, "Запрос на покупку")) return (false, null);
            LogInfo("Rest Service", "Выполнен запрос на покупку продукта - ", response.Data.payment_url);
            return (true, response.Data.payment_url);
        }
        public bool ExecuteTestRequest(int id)
        {
            var response = client.Post<BaseResponse>(new RestRequest("shop/test").AddJsonBody(new RestTestRequest()
            {
                product_id = id,
            }));
            if (!checkAndLogResponseToError(response, "Запрос на тест")) return false;
            return true;
        }

        public (string, string, int, string, bool?) GetUserInfo()
        {
            var response = client.Post<RestUserInfoResponse>(new RestRequest("launcher/info"));
            if (!checkAndLogResponseToError(response, "Получение данных пользователя")) return (null, null, 0, null, null);
             LogInfo("Rest Service", "Обновлены данные пользователя");
            return (response.Data.username, response.Data.referral_code, Convert.ToInt32(Math.Floor(response.Data.balance)), response.Data.currency, response.Data.encryption);
        }

        public bool? GetEncryptStatus()
        {
            var response = client.Get<RestUserInfoResponse>(new RestRequest("launcher/info"));
            if (!checkAndLogResponseToError(response, "Получение данных encryption")) return null;
            LogInfo("Rest Service", "Получен статус encryption");
            return response.Data.encryption;
        }

        public void SetEncryptStatus(bool newStatus)
        {
            var response = client.Post<RestUserInfoResponse>(new RestRequest("launcher/toggle_encryption").AddJsonBody(new RestEncryptionRequest()
            {
                enable = newStatus,
            }));
            if (!checkAndLogResponseToError(response, "Установка нового encryption")) return;
            LogInfo("Rest Service", "Установлен новый encryption");
            return;
        }

        public IEnumerable<PaymentSystem> GetAllPaymentSystem()
        {
            bool isTesterOrDeveloper = !(GetUserType() == BranchType.MASTER);
            var response = client.Get<List<RestPaymentSystem>>(new RestRequest("shop/payment_systems"));
            if (!checkAndLogResponseToError(response, "Получение списка платёжных систем")) return null;
            List<PaymentSystem> result = new List<PaymentSystem>();
            foreach (var item in response.Data)
            {
                if (item.name.ToLower().Contains("тест") && !isTesterOrDeveloper) continue;
                result.Add(new PaymentSystem()
                {
                    Id = item.id,
                    Name = Localizator.LoadedLocalizationInfo.Language == "Русский" ? item.name : item.name_en,
                    Description = Localizator.LoadedLocalizationInfo.Language == "Русский" ? item.description : item.description_en,
                });
            }
            LogInfo("Rest Service", "Получены данные платёжных систем");
            return result;
        }
        List<string> updatedActionIds = new List<string>();

        public bool GetActionState(string sessionId)
        {
            var response = client.Get<List<RestAction>>(new RestRequest("launcher/ping"));
            if (!checkAndLogResponseToError(response, "Проверка запросов на обновление данных")) return false;
             bool res = false;
            List<RestAction> actions = response.Data;
            foreach (var action in actions)
            {
                if (!updatedActionIds.Contains(action.action_id) && action.session == sessionId && (action.action_type == 1 || action.action_type == 11))
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
            if (!checkAndLogResponseToError(response, "Получение типа аккаунта")) return BranchType.UNKNOWN;
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
            if (!checkAndLogResponseToError(response, "Изменение пароля")) return false;
            LogInfo("Rest Service", "Пароль изменён");
            return true;
        }

        public List<RestDeveloperRotation> ExecuteDeveloperRotationRequest()
        {
            var response = client.Post<List<RestDeveloperRotation>>(new RestRequest("launcher/git_pull"));
            if (!checkAndLogResponseToError(response, "Получение данных ротаций разработчиков")) return null;
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
            if(!checkAndLogResponseToError(response, "Добавление ротации разработчика")) return null;
            return response.Data;
        }

        public bool ExecuteLinkEmailRequest(string email)
        {
            //var response = client.Post<>("launcher/link_email", new Rest);
            LogWarn("Rest Service", "Использована нереализованная функция привязки почты");
            return true;
        }

        public bool ExecuteCouponRequest(string couponCode)
        {
            var response = client.Post<RestCouponResponse>(new RestRequest("launcher/redeem_coupon").AddJsonBody(new RestCouponRequest()
            {
                coupon_code = couponCode,
            }));
            if (!checkAndLogResponseToError(response, "Активация купона")) return false;
            return true;
        }

        public async Task<bool> ExecuteAsyncClearCacheRequest()
        {
            var response = await client.ExecutePostAsync(new RestRequest("laucnher/clear_cache"));
            return response.StatusCode == HttpStatusCode.OK;
        }
        private void addConnectionErrorAccident()
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

        public bool ExecuteSpecifiedRequest<TRequest, TResponse>(string url, string operationName, TRequest requestObject, out TResponse responseObject) where TResponse : BaseResponse
        {
            var rawResponse = client.Execute<TResponse>(new RestRequest(url).AddJsonBody(requestObject), Method.POST);
            if(checkAndLogResponseToError(rawResponse, operationName))
            {
                logSuccessOfOperation(operationName);
                responseObject = rawResponse.Data;
                return true;
            }
            responseObject = null;
            return false;

        }
        public bool ExecuteSpecifiedRequest<TRequest>(string url, string operationName, TRequest requestObject)
        {
            var rawResponse = client.Execute(new RestRequest(url).AddJsonBody(requestObject), Method.POST);
            if (checkAndLogResponseToError(rawResponse, operationName))
            {
                logSuccessOfOperation(operationName);
                return true;
            }
            return false;
        }

        public bool ExecuteSpecifiedRequest<TResponse>(string url, string operationName, Method method, out TResponse responseObject) where TResponse : BaseResponse
        {
            var rawResponse = client.Execute<TResponse>(new RestRequest(url), method);
            if (checkAndLogResponseToError(rawResponse, operationName))
            {
                logSuccessOfOperation(operationName);
                responseObject = rawResponse.Data;
                return true;
            }
            responseObject = null;
            return false;
        }

        private void logSuccessOfOperation(string operationName)
        {
            LogInfo("Rest Service", $"Выполнена операция:{operationName}");
        }
        private bool checkAndLogResponseToError(IRestResponse response, string operationName)
        {
            if(!checkHTTPStatusCodeToServerTrouble(response.StatusCode)) return false;
            if (string.IsNullOrWhiteSpace(response.Content))
            {
                LastError = "Получен пустой ответ от сервера";
                LogError("Rest Service", $"Получен пустой ответ от сервера при выполнении: {operationName}");
                return false;
            }
            BaseResponse baseResponse;
            try
            {
                baseResponse = JsonConvert.DeserializeObject<BaseResponse>(response.Content);
            }
            catch
            {
                baseResponse = null;
            }
            if(response.StatusCode != HttpStatusCode.OK
                || baseResponse != null && !string.IsNullOrEmpty(baseResponse.error))
            {
                LastError = baseResponse?.error ?? "Сервер не передал сообщение ошибки";
                LogError("Rest Service", $"Ошибка при выполнении: {operationName}", $"Сообщение ошибки: {LastError}");
                return false;
            }
            return true;
        }

        private bool checkHTTPStatusCodeToServerTrouble(HttpStatusCode statusCode)
        {
            HttpStatusCode[] restrictedCodes = new HttpStatusCode[]
            {
                HttpStatusCode.Forbidden,
                HttpStatusCode.BadGateway,
                HttpStatusCode.GatewayTimeout,
                HttpStatusCode.RequestTimeout,
                HttpStatusCode.HttpVersionNotSupported,
                HttpStatusCode.ServiceUnavailable,
            };
            if (restrictedCodes.Contains(statusCode) || (int)statusCode == 0)
            {
                LastError = "Ошибка соединения с сервером";
                LogError("Rest Service", "Сервер недоступен", statusCode.ToString());
                addConnectionErrorAccident();
                return false;
            }
            //resetConnectionErrorAccidentsCounter();
            return true;
        }
    }
}
