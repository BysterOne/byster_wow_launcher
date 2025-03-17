using Newtonsoft.Json;
using RestSharp;

namespace Launcher.Api.Models
{
    #region RReferralSource
    public class RReferralSource
    {
        [JsonProperty("referral_code")]
        public string ReferralCode { get; set; } = string.Empty;

        [JsonProperty("register_source")]
        public int RegisterSource { get; set; } = -1;
    }
    #endregion
    #region RegistrationRequestBody
    public class RegistrationRequestBody
    {
        [JsonProperty("login")]
        public string Login { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        [JsonProperty("referal")]
        public string? ReferralCode { get; set; }

        [JsonProperty("register_source")]
        public int? RegisterSource { get; set; }
    }
    #endregion
    #region LoginRequestBody
    public class LoginRequestBody
    {
        [JsonProperty("login")]
        public string Login { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;
    }
    #endregion
    #region SessionData
    public class SessionData
    {
        [JsonProperty("session")]
        public string Session { get; set; } = string.Empty;
    }
    #endregion



    #region RCreateClient
    public class RCreateClient(RestRequest request, RestClient? client = null)
    {
        public RestClient? Client { get; set; } = client;
        public RestRequest Request { get; set; } = request;
    }
    #endregion
    #region RError
    public class RError
    {
        [JsonProperty("error")]
        public string Error { get; set; } = string.Empty;
    }
    #endregion
}
