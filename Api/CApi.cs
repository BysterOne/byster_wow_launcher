using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any.GlobalEnums;
using Launcher.Api.Errors;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using Launcher.Settings.Enums;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Launcher.Api
{
    public class CApi
    {
        #region Переменные
        private static LogBox Pref { get; set; } = new("Api");
        public static string Session { get; set; } = string.Empty;
        #endregion


        #region Функции
        #region ToggleCompilation
        public static async Task<UResponse<string>> GetServerVersion()
        {
            return await Request<string>("/launcher/check_updates", Method.Get);
        }
        #endregion
        #region ToggleCompilation
        public static async Task<UResponse<User>> ToggleVMProtect(bool enable)
        {
            return await Request<User>("/launcher/toggle_vmprotect", Method.Post, body: new { enable });
        }
        #endregion
        #region ToggleCompilation
        public static async Task<UResponse<User>> ToggleCompilation(bool enable)
        {
            return await Request<User>("/launcher/toggle_compilation", Method.Post, body: new { enable });
        }
        #endregion
        #region ToggleEncryption
        public static async Task<UResponse<User>> ToggleEncryption(bool enable)
        {
            return await Request<User>("/launcher/toggle_encryption", Method.Post, body: new { enable });
        }
        #endregion
        #region ClearCache
        public static async Task<UResponse<object?>> ClearCache()
        {
            return await Request<object?>("/launcher/clear_cache", Method.Post);
        }
        #endregion
        #region RedeemCoupon
        public static async Task<UResponse<User>> RedeemCoupon(string coupon)
        {
            return await Request<User>("/launcher/ping", Method.Post, body: new { coupon_code = coupon });
        }
        #endregion
        #region GetExe
        public static async Task<UResponse<byte[]>> GetExe()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить запрос";

            var endPoint = "/launcher/get_lib";

            #region try
            try
            {
                #region База
                var createRequest = CreateRequest(endPoint);
                var client = createRequest.Client!;
                var request = createRequest.Request;
                request.Method = Method.Post;
                request.AddBody(JsonConvert.SerializeObject(new { branch = GStatic.BranchStrings[AppSettings.Instance.Branch] }, GProp.JsonSeriSettings));
                #endregion
                #region Выполнение и обработка
                var response = await client.ExecuteAsync(request);                
                if (!response.IsSuccessful || response.RawBytes is null)
                {
                    _proc.Log($"response: {response.Content}");
                    _proc.Log($"status: {response.StatusCode}");

                    throw new UExcept(ERequest.BadRequest, "Ошибка запроса");
                }
                #endregion
                #region Ответ
                return new(response.RawBytes);
                #endregion
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
                return new(ex.Error);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(GlobalErrors.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region Ping
        public static async Task<UResponse<List<PingUpdate>>> Ping()
        {
            return await Request<List<PingUpdate>>("/launcher/ping", Method.Get);
        }
        #endregion
        #region Buy
        public static async Task<UResponse<PaymentDetails>> Buy(List<CCartItem> items, PaymentSystem paymentSystem, double bonuses = 0)
        {
            var body = new
            {
                items = items.Select(x => new { product_id = x.Product.Id, amount = x.Count }),
                payment_system_id = paymentSystem.Id,
                bonuses
            };

            return await Request<PaymentDetails>("/shop/buy", Method.Post, body: body);
        }
        #endregion
        #region GetPaymentSystems
        public static async Task<UResponse<List<PaymentSystem>>> GetPaymentSystems()
        {
            return await Request<List<PaymentSystem>>("/shop/payment_systems", Method.Get);
        }
        #endregion
        #region Translate
        public static async Task<UResponse<TranslatedDictionary>> Translate(ELang lang, IEnumerable<string> keys)
        {
            return await Request<TranslatedDictionary>("/localization/v1/translate", Method.Post, body: new { language = GStatic.GetLangCode(lang), service = "core", items = keys });
        }
        #endregion
        #region GetUserSubscriptions
        public static async Task<UResponse<List<Subscription>>> GetUserSubscriptions()
        {
            return await Request<List<Subscription>>("/shop/my_subscriptions", Method.Post, body: new { });
        }
        #endregion
        #region GetTest
        public static async Task<UResponse<List<Subscription>>> GetTest(int productId)
        {
            return await Request<List<Subscription>>("/shop/test", Method.Post, body: new { product_id = productId });
        }
        #endregion
        #region GetProductsList
        public static async Task<UResponse<List<Product>>> GetProductsList()
        {
            return await Request<List<Product>>("/shop/product_list", Method.Post, body: new { });
        }
        #endregion
        #region GetUserInfo
        public static async Task<UResponse<User>> GetUserInfo()
        {
            return await Request<User>("/launcher/info", Method.Get);
        }
        #endregion
        #region GetReferralSource
        public static async Task<UResponse<RReferralSource>> GetReferralSource(string filename)
        {
            return await Request<RReferralSource>("/launcher/get_referal_source", Method.Post, body: new { filename });
        }
        #endregion
        #region Registration
        public static async Task<UResponse<SessionData>> Registration(RegistrationRequestBody data)
        {
            return await Request<SessionData>("/launcher/registration", Method.Post, body: data);
        }
        #endregion
        #region Login
        public static async Task<UResponse<SessionData>> Login(LoginRequestBody data)
        {
            return await Request<SessionData>("/launcher/login", Method.Post, body: data);
        }
        #endregion
        #endregion

        #region База       
        #region Request
        private static async Task<UResponse<T>> Request<T>(string endPoint, Method method, Dictionary<string, string>? parameters = null, object? body = null, [CallerMemberName] string methodName = "") where T : class
        {
            var _proc = Pref.CloneAs(methodName);
            var _failinf = $"Не удалось выполнить запрос";

            #region try
            try
            {
                #region Создание
                #region База
                var createRequest = CreateRequest(endPoint);
                var client = createRequest.Client!;
                var request = createRequest.Request;
                #endregion
                #region Тело
                request.Method = method;
                switch (method)
                {
                    case Method.Get:
                        if (parameters is not null) 
                            foreach (var par in parameters) 
                                request.AddParameter(par.Key, par.Value);
                        break;
                    case Method.Post:
                        request.AddBody(JsonConvert.SerializeObject(body, GProp.JsonSeriSettings));
                        break;
                    default: 
                        throw new UExcept(ERequest.UnprocessedMethod, $"Необрабатываемый метод '{method}'");
                }
                #endregion              
                #endregion
                #region Отправка
                var response = await client.ExecuteAsync(request);
                var data = response.Content != null ? JsonConvert.DeserializeObject<T>(response.Content.ToString()) : null;
                if (!response.IsSuccessStatusCode || response.Content is null)
                {
                    _proc.Log($"response: {response.Content}");
                    _proc.Log($"status: {response.StatusCode}");

                    var error = response.Content != null ? JsonConvert.DeserializeObject<RError>(response.Content!.ToString()) : null;

                    throw response.StatusCode switch
                    {
                        HttpStatusCode.BadRequest => new UExcept(ERequest.BadRequest, error is null ? "Ошибка запроса" : error.Error),
                        HttpStatusCode.Unauthorized => new UExcept(ERequest.BadRequest, error is null ? $"Ошибка авторизации" : error.Error),
                        _ => new UExcept(ERequest.FailExecuteRequest, $"Не удалось выполнить запрос")
                    };
                }
                #endregion

                return new(data);
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
                return new(ex.Error);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(GlobalErrors.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region CreateRequest
        private static RCreateClient CreateRequest(string requestUrl, bool withClient = true)
        {
            RestClient? client = null;
            if (withClient)
            {
                #region База
                var options = new RestClientOptions
                {
                    BaseUrl = new Uri(AppSettings.Instance.Server switch
                    {
                        EServer.Staging => "https://api.staging.byster.one/",
                        _ => "https://api.byster.one/",
                    })
                };
                #endregion
                client = new RestClient(options);
            }
            var request = new RestRequest(requestUrl);
            if (!String.IsNullOrWhiteSpace(Session)) request.AddHeader("Authorization", Session);
            return new(request, client);
        }
        #endregion
        #endregion
    }
}
