using Newtonsoft.Json.Linq;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Helpers.DXObjectHelpers
{
    public static class DXObjectHelper
    {
        public static Guid GetId(JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject);

            var token = jObject.GetValue(Constants.Id, StringComparison.OrdinalIgnoreCase);

            if (token == null || token.Type == JTokenType.Null)
                throw new ArgumentException($"Property '{Constants.Id}' is missing or null.", nameof(jObject));

            if (token.Type == JTokenType.Guid)
                return token.Value<Guid>();

            if (Guid.TryParse(token.ToString(), out var id))
                return id;

            throw new ArgumentException($"Property '{Constants.Id}' could not be parsed as Guid.", nameof(jObject));
        }

        public static bool TryGetId(JObject jObject, out Guid id)
        {
            id = default;

            if (jObject is null)
                return false;

            var token = jObject.GetValue(Constants.Id, StringComparison.OrdinalIgnoreCase);

            if (token is null || token.Type == JTokenType.Null)
                return false;

            if (token.Type == JTokenType.Guid)
            {
                id = token.Value<Guid>();
                return id != default;
            }

            if (Guid.TryParse(token.ToString(), out id))
                return id != default;

            return false;
        }

        public static string GetDXType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return AttributeReader.GetDXUnitTypeName(type);
        }

        public static string? GetDXTitle(JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject);

            return jObject.GetValue(Constants.DXTitle, StringComparison.OrdinalIgnoreCase)
                          ?.Value<string>();
        }

        /// <summary>
        /// The owning unit declared by an element record. Reads the <see cref="DXElementRecord.DXUnitId"/>
        /// property first, then falls back to the dynamic fields, where a writer may have spelled the
        /// owner as <c>DXUnitId</c> or as <c>&lt;UnitType&gt;Id</c>.
        /// </summary>
        /// <remarks>
        /// Shared so that the access check and the write that follows it resolve the same owner.
        /// Reading it two different ways would let a record pass a check against one unit and then
        /// land under another.
        /// </remarks>
        public static Guid GetDeclaredDXUnitId(DXElementRecord record, string? dxUnitTypeName)
        {
            ArgumentNullException.ThrowIfNull(record);

            if (record.DXUnitId != Guid.Empty)
                return record.DXUnitId;

            if (record.Fields == null)
                return Guid.Empty;

            if (TryReadGuidField(record.Fields, Constants.DXUnitId, out var value))
                return value;

            return !string.IsNullOrWhiteSpace(dxUnitTypeName)
                && TryReadGuidField(record.Fields, $"{dxUnitTypeName}Id", out value)
                    ? value
                    : Guid.Empty;
        }

        private static bool TryReadGuidField(IDictionary<string, JToken> fields, string key, out Guid value)
        {
            value = Guid.Empty;

            if (!fields.TryGetValue(key, out var token))
            {
                foreach (var kvp in fields)
                {
                    if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        token = kvp.Value;
                        break;
                    }
                }
            }

            if (token == null || token.Type == JTokenType.Null)
                return false;

            if (token.Type == JTokenType.Guid)
            {
                value = token.ToObject<Guid>();
                return value != Guid.Empty;
            }

            return Guid.TryParse(token.ToString(), out value) && value != Guid.Empty;
        }
    }
}
