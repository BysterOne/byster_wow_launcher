using Launcher.Any.GlobalEnums;
using Launcher.Cls.ModelConverters;
using Launcher.Windows.AnyMain.Enums;
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
    #region User
    public class User
    {
        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("user_email")]
        public string? Email { get; set; }

        [JsonProperty("max_computers")]
        public int MaxComputers { get; set; } = 0;

        [JsonProperty("balance")]
        public double Balance { get; set; } = 0;

        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonProperty("referral_code")]
        public string ReferralCode { get; set; } = string.Empty;

        [JsonProperty("encryption")]
        public bool Encryption { get; set; } = false;

        [JsonProperty("test_duration")]
        public int TestDuration { get; set; } = 0;
    }
    #endregion
    #region Product
    public class Product
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [JsonProperty("is_bundle")]
        public bool IsBundle { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("description_en")]
        public string DescriptionEn { get; set; } = string.Empty;

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("currency")]
        [JsonConverter(typeof(CurrencyConverter))]
        public ECurrency Currency { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("can_test")]
        public bool CanTest { get; set; }

        [JsonProperty("media")]
        public List<Media> Media { get; set; } = [];

        [JsonProperty("rotations")]
        public List<Rotation> Rotations { get; set; } = [];
    }
    #endregion
    #region Rotation
    public class Rotation
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("description_en")]
        public string DescriptionEn { get; set; } = string.Empty;

        [JsonProperty("type")]
        [JsonConverter(typeof(RotationTypeConverter))]
        public ERotationType Type { get; set; }

        [JsonProperty("klass")]
        [JsonConverter(typeof(RotationClassConverter))]
        public ERotationClass Class { get; set; }

        [JsonProperty("specialization")]
        public string Specialization { get; set; } = string.Empty; // Enum

        [JsonProperty("role_type")]
        [JsonConverter(typeof(RotationRolesConverter))]
        public ERotationRole RoleType { get; set; }

        [JsonProperty("media")]
        public List<Media> Media { get; set; } = [];
    }
    #endregion
    #region Media
    public class Media
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(MediaTypeConverter))]
        public EMediaType Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
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
