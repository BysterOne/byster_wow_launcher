using Cls.Any;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Windows.AnyMain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Launcher.Settings
{
    class GStatic
    {
        #region Переменные
        public static Dictionary<ELang, string> Langs = new Dictionary<ELang, string>
        {
           { ELang.Ru, "ru" },
           { ELang.En, "en" },
           { ELang.ZhCn, "zh" },
        };    

        public static Dictionary<EUserPermissions, string> PermissionsStrings = new Dictionary<EUserPermissions, string>
        {
            { EUserPermissions.Superuser, "superuser" },
            { EUserPermissions.ToggleEncrypt, "shop.toggle_encrypt" },
            { EUserPermissions.AdminSiteAccess, "launcher.admin_site_access" },
            { EUserPermissions.CanToggleCompilation, "launcher.can_toggle_compilation" },
            { EUserPermissions.ClosedServer, "shop.closed_server" },
            { EUserPermissions.CanToggleVmprotect, "launcher.can_toggle_vmprotect" },
            { EUserPermissions.ExternalDeveloper, "shop.external_developer" },
            { EUserPermissions.DoNotBan, "protection.donotban" },
            { EUserPermissions.Tester, "shop.tester" },
        };

        public static Dictionary<EBranch, string> BranchStrings = new Dictionary<EBranch, string>
        {
            { EBranch.Test, "test" },
            { EBranch.Dev, "dev" },
            { EBranch.NewThread, "new-thread" },
            { EBranch.Master, "master" }
        };
        #endregion


        #region GetServerIcon
        public static ImageSource? GetServerIcon(EServerIcon serverIcon)
        {
            var name = serverIcon switch
            {         
                EServerIcon.Sirus => "server_sirus.png",
                EServerIcon.Circle => "server_circle.png",
                EServerIcon.Warmane => "server_warmane.png",
                EServerIcon.WarmaneDuplicate => "server_warmane_1.png",
                _ => "null_image_icon.png"
            };

            if (name is null) return null;

            return BitmapFrame.Create(Functions.GetSourceFromResource($"Media/ServersIcons/{name}"));
        }
        #endregion
        #region GetRotationClassIcon
        public static ImageSource? GetRotationClassIcon(ERotationClass rotationClass)
        {
            var name = rotationClass switch
            {
                ERotationClass.Any => "any_icon.png",
                ERotationClass.Warrior => "warrior_icon.png",
                ERotationClass.Druid => "druid_icon.png",
                ERotationClass.Priest => "priest_icon.png",
                ERotationClass.Mage => "mage_icon.png",
                ERotationClass.Hunter => "hunter_icon.png",
                ERotationClass.Paladin => "paladin_icon.png",
                ERotationClass.Rogue => "roque_icon.png",
                ERotationClass.DeathKnight => "death_knight_icon.png",
                ERotationClass.Warlock => "warlock_icon.png",
                ERotationClass.Shaman => "shaman_icon.png",
                _ => null
            };

            if (name is null) return null;

            return BitmapFrame.Create(Functions.GetSourceFromResource($"Media/Shop/ClassIcons/{name}"));
        }
        #endregion
        #region GetRotationTypeName
        public static string GetRotationTypeName(ERotationType type)
        {
            return type switch
            {
                ERotationType.Bot => Dictionary.Translate("Бот"),
                _ => type.ToString()
            };
        }
        #endregion
        #region GetRotationClassName
        public static string GetRotationClassName(ERotationClass rotationClass)
        {
            var key = rotationClass switch
            {
                ERotationClass.Warrior     => "Воин",
                ERotationClass.Druid       => "Друид",
                ERotationClass.Priest      => "Жрец",
                ERotationClass.Mage        => "Маг",
                ERotationClass.Hunter      => "Охотник",
                ERotationClass.Paladin     => "Паладин",
                ERotationClass.Rogue       => "Разбойник",
                ERotationClass.DeathKnight => "Рыцарь смерти",
                ERotationClass.Warlock     => "Чернокнижник",
                ERotationClass.Shaman      => "Шаман",
                ERotationClass.Any         => "Любой",
                _ => throw new Exception($"Необрабатываемый тип ERotationClass: {rotationClass}")
            };

            Dictionary.Translate("Любой");
            return Dictionary.Translate(key);
        }
        #endregion
        #region GetRotationTypeIcon
        public static ImageSource? GetRotationTypeIcon(ERotationType rotationType)
        {
            var name = rotationType switch
            {
                ERotationType.Bot => "bot_icon.png",
                ERotationType.PvE => "pve_icon.png",
                ERotationType.PvP => "pvp_icon.png",
                ERotationType.Utility => "utility_icon.png",
                _ => null
            };

            if (name is null) return null;

            return BitmapFrame.Create(Functions.GetSourceFromResource($"Media/Shop/TypesIcons/{name}"));
        }
        #endregion
        #region GetRotationRoleIcon
        public static ImageSource? GetRotationRoleIcon(ERotationRole rotationType)
        {
            var name = rotationType switch
            {
                ERotationRole.Any => "any_icon.png",
                ERotationRole.Dps => "dps_icon.png",
                ERotationRole.Heal => "heal_icon.png",
                ERotationRole.Tank => "tank_icon.png",
                _ => null
            };

            if (name is null) return null;

            return BitmapFrame.Create(Functions.GetSourceFromResource($"Media/Shop/RolesIcons/{name}"));
        }
        #endregion
        #region GetLangCode
        public static string GetLangCode(ELang lang) => Langs[lang];
        #endregion
        #region GetLangFromCode
        public static ELang? GetLangFromCode(string lang) => Langs.GetKeyOrNull(lang);
        #endregion
    }
}
