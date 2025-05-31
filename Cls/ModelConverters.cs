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
    #region LangAsTextJsonConverter
    public class LangAsTextJsonConverter : JsonConverter<ELang>
    {
        public override void WriteJson(JsonWriter writer, ELang value, JsonSerializer serializer)
        {
            writer.WriteValue(GStatic.GetLangCode(value));
        }

        public override ELang ReadJson(JsonReader reader, Type objectType, ELang existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return (ELang)Convert.ToInt32(reader.Value);
            }
            if (reader.TokenType == JsonToken.String)
            {
                var tryP = GStatic.GetLangFromCode(reader.Value.ToString());
                return tryP ?? ELang.Ru;
            }
            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing boolean.");
        }
    }
    #endregion
    #region BoolAsIntJsonConverter
    public class BoolAsIntJsonConverter : JsonConverter<bool>
    {
        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value ? 1 : 0);
        }

        public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToInt32(reader.Value) != 0;
            }
            if (reader.TokenType == JsonToken.String)
            {
                return reader.Value.ToString() == "true";
            }
            if (reader.TokenType == JsonToken.Boolean)
            {
                return (bool)reader.Value;
            }
            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing boolean.");
        }
    }
    #endregion
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
                var permissions = new HashSet<EUserPermissions>();
                var list = arr.Select(x => x.ToString()).ToList();

                foreach (var permission in list)
                {
                    EUserPermissions? has = GStatic.PermissionsStrings.FirstOrDefault
                        (x => x.Value.Equals(permission, StringComparison.CurrentCultureIgnoreCase)).Key;
                    if (GStatic.PermissionsStrings.ContainsValue(permission))
                        permissions.Add((EUserPermissions)has);
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
