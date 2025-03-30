using Launcher.Any.GlobalEnums;
using Launcher.Windows.AnyMain.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Cls.ModelConverters
{
    #region CurrencyConverter
    public class CurrencyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(ECurrency);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString()?.ToLower();
            return value switch
            {
                "rub" => ECurrency.Rub,
                "usd" => ECurrency.Usd,
                _ => throw new JsonSerializationException($"Неизвестный {nameof(ECurrency)}: {value}")
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
    #endregion
    #region RotationTypeConverter
    public class RotationTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(ERotationType);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString()?.ToLower();

            return value switch
            {
                "pve" => ERotationType.PvE,
                "pvp" => ERotationType.PvP,
                "bot" => ERotationType.Bot,
                "utility" => ERotationType.Utility,
                _ => throw new JsonSerializationException($"Неизвестный {nameof(ERotationType)}: {value}")
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
    #endregion
    #region RotationClassConverter
    public class RotationClassConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(ERotationClass);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString()?.ToLower();

            var aviableValues = 
                Enum.GetValues<ERotationClass>()
                .Where(x => x.ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
            if (aviableValues.Count is 1) return aviableValues[0]; 

            return value switch
            {
                _ => throw new JsonSerializationException($"Неизвестный {nameof(ERotationClass)}: {value}")
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
    #endregion
    #region RotationRolesConverter
    public class RotationRolesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(ERotationRole);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString()?.ToLower();

            return value switch
            {
                "tank" => ERotationRole.Tank,
                "heal" => ERotationRole.Heal,
                "dps"  => ERotationRole.Dps,
                null   => ERotationRole.Any,
                _ => throw new JsonSerializationException($"Неизвестный {nameof(ERotationRole)}: {value}")
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
    #endregion
    #region MediaTypeConverter
    public class MediaTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(EMediaType);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString()?.ToLower();

            return value switch
            {
                "img"   => EMediaType.Image,
                "video" => EMediaType.Video,
                _ => throw new JsonSerializationException($"Неизвестный {nameof(EMediaType)}: {value}")
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
    #endregion
}
