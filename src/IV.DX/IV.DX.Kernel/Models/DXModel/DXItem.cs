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

        private static bool AreValuesEqual(object v1, object v2)
        {
            if (ReferenceEquals(v1, v2))
                return true;

            if (v1 == null || v2 == null)
                return false;

            if (v1 is Guid && v2 is string)
            {
                Guid v2Guid;

                Guid.TryParse((string)v2, out v2Guid);

                return Equals(v1, v2Guid);
            }

            if (v2 is Guid && v1 is string)
            {
                Guid v1Guid;

                Guid.TryParse((string)v1, out v1Guid);

                return Equals(v2, v1Guid);
            }

            // Простые типы
            return Equals(v1, v2);
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
    }
}