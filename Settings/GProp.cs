using Launcher.Any;
using Launcher.Api.Models;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings.Enums;
using Newtonsoft.Json;

namespace Launcher.Settings
{
    public class GProp
    {
        public static User User { get; set; } = null!;
        public static List<Product> Products { get; set; } = [];
        public static CFilters Filters { get; set; } = new();
        public static Cart Cart { get; set; } = new();
        public static RReferralSource? ReferralSource { get; set; } = null;
        public static EServer Server { get; set; } = EServer.Prod;
        public static JsonSerializerSettings JsonSeriSettings { get => new() { NullValueHandling = NullValueHandling.Ignore }; }        
    }
}
