using Launcher.Any.GlobalEnums;
using Launcher.Settings;
using Launcher.Windows.AnyMain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Cls.ModelConverters
{
    #region BranchesConverter
    public class BranchesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(EBranch);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray arr = JArray.Load(reader);
                var branches = new List<EBranch>();
                var list = arr.Select(x => x.ToString()).ToList();

                foreach (var branch in list)
                {
                    EBranch? has = GStatic.BranchStrings.FirstOrDefault
                        (x => x.Value.Equals(branch, StringComparison.CurrentCultureIgnoreCase)).Key;
                    if (GStatic.BranchStrings.ContainsValue(branch))
                        branches.Add((EBranch)has);
                }
                return branches;
            }
            throw new JsonSerializationException("Данный параметр принимает только Array<string>");
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
    #endregion
    #region UserPermissionsConverter
    public class UserPermissionsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(EUserPermissions);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {                
                throw new JsonSerializationException($"Данный параметр принимает только Array<string>");
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                int combined = 0;
                JArray arr = JArray.Load(reader);
                var permissions = EUserPermissions.None;
                var list = arr.Select(x => x.ToString()).ToList();

                foreach (var permission in list)
                {
                    EUserPermissions? has = GStatic.PermissionsStrings.FirstOrDefault
                        (x => x.Value.Equals(permission, StringComparison.CurrentCultureIgnoreCase)).Key;
                    if (GStatic.PermissionsStrings.ContainsValue(permission)) 
                        permissions |= (EUserPermissions)has;
                }
                return permissions;
            }
            throw new JsonSerializationException("Данный параметр принимает только Array<string>");
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
    #endregion
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
