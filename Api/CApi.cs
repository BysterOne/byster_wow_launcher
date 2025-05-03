using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Api.Errors;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Settings;
using Launcher.Settings.Enums;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Launcher.Api
{
    public class CApi
    {
        #region Переменные
        private static LogBox Pref { get; set; } = new("Api");
        public static string Session { get; set; } = string.Empty;
        #endregion


        #region Функции
        #region MyRegion
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
                    BaseUrl = new Uri(GProp.Server switch
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
