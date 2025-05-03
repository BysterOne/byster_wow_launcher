using Launcher.Any;
using Launcher.Api.Models;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings.Enums;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace Launcher.Settings
{
    public class GProp
    {
        private static CServer? _selectedServer;

        public static User User { get; set; } = null!;
        public static List<Product> Products { get; set; } = [];
        public static ObservableCollection<Subscription> Subscriptions { get; set; } = [];
        public static CFilters Filters { get; set; } = new();
        public static Cart Cart { get; set; } = new();
        public static CServer? SelectedServer { get => _selectedServer; set { _selectedServer = value; SelectedServerChanged?.Invoke(value); } }
        public static RReferralSource? ReferralSource { get; set; } = null;
        public static EServer Server { get; set; } = EServer.Staging;
        public static JsonSerializerSettings JsonSeriSettings { get => new() { NullValueHandling = NullValueHandling.Ignore }; }


        #region События
        public delegate void SelectedServerChangedDelegate(CServer? server);
        public static event SelectedServerChangedDelegate? SelectedServerChanged;
        #endregion

        #region Функции
        #region UpdateSubscriptions
        public static void UpdateSubscriptions(List<Subscription> newList)
        {
            var toRemove = Subscriptions
                .Where(old => newList.All(n => n.Rotation.Id != old.Rotation.Id))
                .ToList();
            foreach (var old in toRemove)
                Subscriptions.Remove(old);

            foreach (var incoming in newList)
            {
                var existing = Subscriptions
                    .FirstOrDefault(old => old.Rotation.Id == incoming.Rotation.Id);

                if (existing != null)
                {
                    existing.ExpiredDate = incoming.ExpiredDate;
                    existing.Rotation = incoming.Rotation;
                }
                else
                {
                    Subscriptions.Add(incoming);
                }
            }
        }
        #endregion
        #endregion
    }
}
