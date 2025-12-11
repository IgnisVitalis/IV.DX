using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IV.DX.Application.Helpers
{
    internal class IgnorePropertiesResolver : DefaultContractResolver
    {
        private readonly HashSet<string> _ignoredProps;

        public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
        {
            _ignoredProps = new HashSet<string>(propNamesToIgnore,
                System.StringComparer.OrdinalIgnoreCase);
        }

        protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);
            return props
                .Where(p => !_ignoredProps.Contains(p.PropertyName))
                .ToList();
        }
    }
}
