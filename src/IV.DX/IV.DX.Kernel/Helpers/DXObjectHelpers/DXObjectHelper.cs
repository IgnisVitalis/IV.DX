using Newtonsoft.Json.Linq;
using IV.DX.Kernel.Helpers;

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
    }
}
