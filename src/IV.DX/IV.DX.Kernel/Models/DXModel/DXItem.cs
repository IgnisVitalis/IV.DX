using System.Numerics;

namespace IV.DX.Kernel.Models
{
    public class DXItem
    {
        public string Type { get; }
        public Guid ID { get; }
        public Guid DXUnitID { get; }
        public DateTime TimeStamp { get; }
        public IDictionary<string, object> Content { get; }       

        public DXItem(string type, Guid id, Guid dxUnitID, DateTime timeStamp, IDictionary<string, object> content)
        {
            this.Type = type;
            this.ID = id;
            this.DXUnitID = dxUnitID;
            this.TimeStamp = timeStamp;
            this.Content = content;

            this.SetValue(Constants.SystemPropertyTypeName, this.Type);
            this.SetValue(Constants.ID, this.ID);

            if (this.DXUnitID != this.ID)
            {
                this.SetValue(Constants.DXUnitID, this.DXUnitID);
            }

            this.SetValue(Constants.TimeStamp, this.TimeStamp);
        }

        public void SetValue(string propertyName, object value)
        {
            if (!IsSimpleTypeOrByteArray(value))
            {
                throw new Exception($"Value {value} is not simple type or byte[]");
            }

            if (HasValue(propertyName))
            {
                this.Content[propertyName] = value;
            }
            else
            {
                this.Content.Add(propertyName, value);
            }
        }

        public bool HasValue(string propertyName)
        {
            return Content.ContainsKey(propertyName);
        }

        public T? GetValue<T>(string propertyName)
        {
            if (this.HasValue(propertyName))
                return (T)this.Content[propertyName];
            else
                return default(T?);
        }

        public object GetValue(string propertyName)
        {
            if (this.HasValue(propertyName))
                return this.Content[propertyName];
            else
                return null;
        }

        public static bool DeepEquals(DXItem item1, DXItem item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = true;

            result = result && (item1.ID == item2.ID);
            result = result && (item1.DXUnitID == item2.DXUnitID);

            result = result && DeepEquals(item1.Content, item2.Content);

            return result;
        }

        private static bool DeepEquals(IDictionary<string, object> dict1, IDictionary<string, object> dict2)
        {
            if (ReferenceEquals(dict1, dict2))
                return true;
            if (dict1 == null || dict2 == null)
                return false;
            if (dict1.Count != dict2.Count)
                return false;

            foreach (var kvp in dict1)
            {
                if (!dict2.ContainsKey(kvp.Key))
                    return false;

                var v1 = kvp.Value;
                var v2 = dict2[kvp.Key];

                if (!AreValuesEqual(v1, v2))
                    return false;
            }

            return true;
        }

        public static bool AreValuesEqual(object v1, object v2)
        {
            if (ReferenceEquals(v1, v2)) return true;
            if (v1 is null || v2 is null) return false;

            if (v1 is Guid g1 && v2 is string s2)
                return Guid.TryParse(s2, out var parsed2) && g1.Equals(parsed2);

            if (v2 is Guid g2 && v1 is string s1)
                return Guid.TryParse(s1, out var parsed1) && g2.Equals(parsed1);

            if (TryToBigInteger(v1, out var b1) && TryToBigInteger(v2, out var b2))
                return b1 == b2;

            if (TryToDecimal(v1, out var d1) && TryToDecimal(v2, out var d2))
                return d1 == d2;

            return Equals(v1, v2);
        }

        private static bool TryToBigInteger(object v, out BigInteger result)
        {
            switch (v)
            {
                case sbyte x: result = x; return true;
                case byte x: result = x; return true;
                case short x: result = x; return true;
                case ushort x: result = x; return true;
                case int x: result = x; return true;
                case uint x: result = x; return true;
                case long x: result = x; return true;
                case ulong x: result = x; return true;
                case Enum e: result = new BigInteger(Convert.ToInt64(e)); return true;
                default: result = default; return false;
            }
        }

        private static bool TryToDecimal(object v, out decimal result)
        {
            switch (v)
            {
                case decimal x: result = x; return true;
                case float x: if (float.IsNaN(x) || float.IsInfinity(x)) break; result = (decimal)x; return true;
                case double x: if (double.IsNaN(x) || double.IsInfinity(x)) break; result = (decimal)x; return true;
                case sbyte x: result = x; return true;
                case byte x: result = x; return true;
                case short x: result = x; return true;
                case ushort x: result = x; return true;
                case int x: result = x; return true;
                case uint x: result = x; return true;
                case long x: result = x; return true;
                case ulong x: result = x; return true;
                case Enum e: result = Convert.ToInt64(e); return true;
            }
            result = default;
            return false;
        }

        public static bool DeepEquals(IEnumerable<DXItem> list1, IEnumerable<DXItem> list2)
        {
            if (list1 == null || list2 == null)
                return true;

            if (list1.Count() != list2.Count())
                return false;

            foreach (var item1 in list1)
            {
                var item2 = list2.SingleOrDefault(x => x.ID == item1.ID);

                if (item2 == null)
                    return false;

                if (!DXItem.DeepEquals(item1, item2))
                    return false;
            }

            return true;
        }

        public DXItem DeepClone()
        {
            return new DXItem(this.Type, this.ID, this.DXUnitID, this.TimeStamp, DeepClone(this.Content));
        }

        public static IDictionary<string, object> DeepClone(
            IDictionary<string, object> source,
            IEqualityComparer<string>? comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var clone = new Dictionary<string, object>(source.Count, comparer ?? StringComparer.Ordinal);

            foreach (var kvp in source)
            {
                clone[kvp.Key] = kvp.Value is byte[] bytes
                    ? CloneBytes(bytes)
                    : kvp.Value;
            }

            return clone;
        }

        private static byte[] CloneBytes(byte[] bytes)
        {
            var copy = new byte[bytes.Length];
            Buffer.BlockCopy(bytes, 0, copy, 0, bytes.Length);
            return copy;
        }

        public static bool IsSimpleTypeOrByteArray(object value)
        {
            if (value is null)
                return false;

            var type = value.GetType();

            if (type.IsPrimitive || type.IsEnum)
                return true;

            if (type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) ||
                type == typeof(Guid) || type == typeof(TimeSpan) || type == typeof(byte[]))
                return true;

            return false;
        }
    }
}