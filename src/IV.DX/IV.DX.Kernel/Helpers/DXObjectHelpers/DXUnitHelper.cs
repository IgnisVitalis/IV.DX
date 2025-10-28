using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Helpers.DXObjectHelpers
{
    internal static class DXUnitHelper
    {
        public static string GetTypeName(string json) => GetTypeName(JObject.Parse(json));

        public static string? GetTypeName(JObject jObject) => (string?)jObject[Constants.SystemPropertyTypeName];

        public static string GetTypeName(Type type) => AttributeReader.GetDXUnitTypeName(type);

        public static Guid GetID(JObject jObject) => (Guid)jObject[Constants.ID];
    }
}
