﻿using Cls;
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
using System.Net.Http;
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
        #region GetGitDirectories
        public static async Task<UResponse<List<CGitDirectory>>> GetGitDirectories()
        {
            return await Request<List<CGitDirectory>>("/launcher/v1/git_pull", Method.Post);
        }
        #endregion
        #region ToggleCompilation
        public static async Task<UResponse<object?>> ChangePassword(string newPassword)
        {
            return await Request<object?> ("/launcher/v1/change_password", Method.Post, body: new { new_password = newPassword });
        }
        #endregion
        #region ToggleCompilation
        public static async Task<UResponse<CVersion>> GetServerVersion()
        {
            return await Request<CVersion>("/launcher/v2/check_updates", Method.Get);
        }
        #endregion
        #region ToggleCompilation
        public static async Task<UResponse<User>> ToggleProtection(bool enable)
        {
            return await Request<User>("/launcher/toggle_protection", Method.Post, body: new { enable });
        }
        #endregion
        #region ToggleCompilation
        public static async Task<UResponse<User>> ToggleCompilation(bool enable)
        {
            return await Request<User>("/launcher/v1/toggle_compilation", Method.Post, body: new { enable });
        }
        #endregion
        #region ToggleEncryption
        public static async Task<UResponse<User>> ToggleEncryption(bool enable)
        {
            return await Request<User>("/launcher/v1/toggle_encryption", Method.Post, body: new { enable });
        }
        #endregion
        #region ClearCache
        public static async Task<UResponse<object?>> ClearCache()
        {
            return await Request<object?>("/launcher/v1/clear_cache", Method.Post);
        }
        #endregion
        #region RedeemCoupon
        public static async Task<UResponse<User>> RedeemCoupon(string coupon)
        {
            return await Request<User>("/launcher/v1/redeem_coupon", Method.Post, body: new { coupon_code = coupon });
        }
        #endregion
        #region GetLauncher
        public static async Task<UResponse<byte[]>> GetLauncher()
        {
            return await GetExe("/launcher/v1/download", Method.Get);
        }
        #endregion
        #region GetLibVersion
        public static async Task<UResponse<CVersion>> GetLibVersion()
        {
            return await Request<CVersion>("/launcher/v2/get_lib_version", Method.Post, body: new { branch = AppSettings.Instance.Branch });
        }
        #endregion
        #region GetLib 
        public static async Task<UResponse<byte[]>> GetLib()
        {
            return await GetExe("/launcher/v1/get_lib", Method.Post);
        }
        #endregion
        #region GetExe
        private static async Task<UResponse<byte[]>> GetExe(string endPoint, Method method)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить запрос";

            #region try
            try
            {
                #region База
                var createRequest = CreateRequest(endPoint);
                var client = createRequest.Client!;
                var request = createRequest.Request;
                request.Method = method;
                request.AddBody(JsonConvert.SerializeObject(new { branch = AppSettings.Instance.Branch }, GProp.JsonSeriSettings));
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
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                return new(uex);
            }
            #endregion
        }
        #endregion
        #region Ping
        public static async Task<UResponse<List<PingUpdate>>> Ping()
        {
            return await Request<List<PingUpdate>>("/launcher/v1/ping", Method.Post);
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

            return await Request<PaymentDetails>("/shop/v1/buy", Method.Post, body: body);
        }
        #endregion
        #region GetPaymentSystems
        public static async Task<UResponse<List<PaymentSystem>>> GetPaymentSystems()
        {
            return await Request<List<PaymentSystem>>("/shop/v1/payment_systems", Method.Get);
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
            return await Request<List<Subscription>>("/shop/v1/my_subscriptions", Method.Post, body: new { });
        }
        #endregion
        #region GetTest
        public static async Task<UResponse<List<Subscription>>> GetTest(int productId)
        {
            return await Request<List<Subscription>>("/shop/v1/test", Method.Post, body: new { product_id = productId });
        }
        #endregion
        #region GetProductsList
        public static async Task<UResponse<List<Product>>> GetProductsList()
        {
            return await Request<List<Product>>("/shop/v1/product_list", Method.Post, body: new { });
        }
        #endregion
        #region GetUserInfo
        public static async Task<UResponse<User>> GetUserInfo()
        {
            return await Request<User>("/launcher/v1/info", Method.Get);
        }
        #endregion
        #region GetReferralSource
        public static async Task<UResponse<RReferralSource>> GetReferralSource(string filename)
        {
            return await Request<RReferralSource>("/launcher/v1/get_referal_source", Method.Post, body: new { filename });
        }
        #endregion
        #region Registration
        public static async Task<UResponse<SessionData>> Registration(RegistrationRequestBody data)
        {
            return await Request<SessionData>("/launcher/v1/registration", Method.Post, body: data);
        }
        #endregion
        #region Login
        public static async Task<UResponse<SessionData>> Login(LoginRequestBody data)
        {
            return await Request<SessionData>("/launcher/v1/login", Method.Post, body: data);
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
                if (!response.IsSuccessStatusCode)
                {
                    _proc.Log($"Response: {response.ErrorException?.Message}");
                    _proc.Log($"Status: {response.StatusCode}");
                    _proc.Log($"Exception: {response.Content}");
                    var uex = new UExcept(ERequest.FailExecuteRequest, $"Не удалось выполнить запрос");
                    uex.Data["Response"] = response.Content;
                    uex.Data["StatusCode"] = response.StatusCode;
                    uex.Data["ErrorException"] = response.ErrorException?.Message;
                    throw uex;
                }

                var data = response.Content != null ? JsonConvert.DeserializeObject<T>(response.Content.ToString()) : null;
                if (!response.IsSuccessStatusCode || (response.Content is null && typeof(T) != typeof(string)))
                {
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
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
                return new(uex);
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
                    }),
                    FollowRedirects = true,                    
                };
                #endregion
                client = new RestClient(new HttpClient(new SentryHttpMessageHandler()), options);
            }
            var request = new RestRequest(requestUrl);
            if (!String.IsNullOrWhiteSpace(Session)) request.AddHeader("Authorization", Session);
            return new(request, client);
        }
        #endregion
        #endregion
    }
}
