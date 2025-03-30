using Cls.Any;
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
        #region GetRotationClassIcon
        public static ImageSource? GetRotationClassIcon(ERotationClass rotationClass)
        {
            var name = rotationClass switch
            {
                ERotationClass.DeathKnight => "death_knight_icon.png",
                _ => null
            };

            if (name is null) return null;

            return BitmapFrame.Create(Functions.GetSourceFromResource($"Media/Shop/{name}"));
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
                _ => throw new Exception($"Необрабатываемый тип ERotationClass: {rotationClass}")
            };

            return Dictionary.Translate(key);
        }
        #endregion
    }
}
