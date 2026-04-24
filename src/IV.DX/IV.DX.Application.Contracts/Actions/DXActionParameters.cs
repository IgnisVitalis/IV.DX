namespace IV.DX.Application.Contracts.Actions
{
    public sealed class DXActionParameters
    {
        private readonly Dictionary<string, object?> _values = new(StringComparer.OrdinalIgnoreCase);

        public DXActionParameters Set(string key, object? value)
        {
            _values[key] = value;
            return this;
        }

        public T? Get<T>(string key)
        {
            if (!_values.TryGetValue(key, out var value) || value is null)
                return default;

            if (value is T typed)
                return typed;

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (targetType == typeof(Guid) && value is string s)
                return (T)(object)Guid.Parse(s);

            return (T)Convert.ChangeType(value, targetType);
        }

        public object? Get(string key)
        {
            _values.TryGetValue(key, out var value);
            return value;
        }

        public bool ContainsKey(string key) => _values.ContainsKey(key);

        public IReadOnlyDictionary<string, object?> ToDictionary() => _values;
    }
}
