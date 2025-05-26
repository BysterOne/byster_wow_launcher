using Launcher.Any.GlobalEnums;
using Launcher.Cls.ModelConverters;
using Launcher.Windows.AnyMain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System.Runtime.Serialization;

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

        [JsonProperty("vmprotect")]
        public bool VMProtect { get; set; } = false;

        [JsonProperty("compilation")]
        public bool Compilation { get; set; } = false;

        [JsonProperty("test_duration")]
        public int TestDuration { get; set; } = 0;

        [JsonProperty("permissions")]
        [JsonConverter(typeof(UserPermissionsConverter))]
        public EUserPermissions Permissions { get; set; }

        [JsonProperty("branches")]
        [JsonConverter(typeof(BranchesConverter))]
        public List<EBranch> Branches { get; set; } = [];
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

        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;

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
        public ERotationRole Role { get; set; }

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
    #region ULocDictionary
    public class ULocDictionary
    { 
        public ELang Language { get; set; }         
        public Dictionary<string, string> Translations { get; set; } = [];
    }
    #endregion
    #region TranslatedDictionary
    public class TranslatedDictionary
    {
        [JsonProperty("language")]
        public string Language { get; set; } = string.Empty;

        [JsonProperty("translations")]
        public Dictionary<string, string> Translations { get; set; } = [];
    }
    #endregion
    #region LocalDictionary
    public class LocalDictionary
    {
        [JsonProperty("language")]
        public string Language { get; set; } = string.Empty;

        [JsonProperty("Associations")]
        public Dictionary<string, string> Translations { get; set; } = [];
    }
    #endregion
    #region PaymentSystem
    public class PaymentSystem
    {
        [JsonProperty("id")]
        public int Id { get; set; } = -1;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("name_en")]
        public string NameEn { get; set; } = string.Empty;
    }
    #endregion
    #region PaymentDetails
    public class PaymentDetails
    {
        [JsonProperty("status")]
        public int Status { get; set; } = -1;

        [JsonProperty("payment_url")]
        public string PaymentUrl { get; set; } = string.Empty;
    }
    #endregion
    #region PingUpdate
    public class PingUpdate
    {
        // TODO: Разобраться с обновлениями с сервера
    }
    #endregion
    #region CVersion
    public class CVersion
    {
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("hash")]
        public string Hash { get; set; } = string.Empty;
    }
    #endregion

    #region Subscription
    public class Subscription
    {
        [JsonProperty("expired_date")]
        public DateTime ExpiredDate { get; set; }

        [JsonProperty("rotation")]
        public Rotation Rotation { get; set; } = null!;
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
