/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

using Newtonsoft.Json;

namespace LibreDiagnostics.Models.Configuration.Json
{
    internal sealed class EnumTypeEnumValueJsonConverter : JsonConverter
    {
        #region Fields

        const char EnumTokenSeparator = ':';

        #endregion

        #region Public

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Enum enumValue)
            {
                writer.WriteValue($"{enumValue.GetType().FullName}{EnumTokenSeparator}{enumValue}");
                return;
            }

            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var value = reader.Value?.ToString();

                if (TryParseEnumToken(value, out var enumValue))
                {
                    return enumValue;
                }

                return value;
            }

            if (reader.TokenType == JsonToken.Integer
             || reader.TokenType == JsonToken.Float
             || reader.TokenType == JsonToken.Boolean
             || reader.TokenType == JsonToken.Date
             || reader.TokenType == JsonToken.Bytes)
            {
                return reader.Value;
            }

            return serializer.Deserialize(reader);
        }

        #endregion

        #region Private

        static bool TryParseEnumToken(string token, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            int separatorIndex = token.LastIndexOf(EnumTokenSeparator);
            if (separatorIndex <= 0 || separatorIndex >= token.Length - 1)
            {
                return false;
            }

            string enumTypeName = token.Substring(0, separatorIndex);
            string enumValueName = token.Substring(separatorIndex + 1);

            if (string.IsNullOrWhiteSpace(enumTypeName) || string.IsNullOrWhiteSpace(enumValueName))
            {
                return false;
            }

            var enumType = FindEnumTypeByName(enumTypeName);
            if (enumType == null)
            {
                return false;
            }

            if (!Enum.TryParse(enumType, enumValueName, false, out var parsed))
            {
                return false;
            }

            if (!Enum.IsDefined(enumType, parsed))
            {
                return false;
            }

            value = parsed;
            return true;
        }

        static Type FindEnumTypeByName(string enumTypeName)
        {
            //Resolve by full type name across loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumTypeName, false, false);
                if (type?.IsEnum == true)
                {
                    return type;
                }
            }

            return null;
        }

        #endregion
    }
}
