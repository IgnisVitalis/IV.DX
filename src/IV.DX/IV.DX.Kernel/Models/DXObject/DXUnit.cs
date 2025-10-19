using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace IV.DX.Kernel.Models
{
    public abstract class DXUnit
    {
        public Guid ID { get; set; }

        [DXColumn("TimeStamp", "TimeStamp", DXLoadingType.Base)]
        public DateTime TimeStamp { get; set; }


        private static readonly ConcurrentDictionary<Type, bool> _validated = new();

        public DXUnit()
        {
            var t = GetType();
          
            _validated.GetOrAdd(t, static type =>
            {
                if (!Attribute.IsDefined(type, typeof(DXUnitAttribute), inherit: true))
                    throw new InvalidOperationException(
                        $"Type {type.FullName} should have [{nameof(DXUnitAttribute)}] attribute.");
                return true;
            });
        }

        public JObject ToJObject()
        {
            return DXUnitHelper.ConvertToJObject(this);
        }

        public static T Parse<T>(JObject jObject) where T : DXUnit
        {
            ArgumentNullException.ThrowIfNull(jObject);

            return DXUnitHelper.CreateInstance<T>(jObject);
        }

        public static T Parse<T>(string jObjectStr) where T : DXUnit
        {
            ArgumentNullException.ThrowIfNullOrEmpty(jObjectStr);

            return DXUnitHelper.CreateInstance<T>(jObjectStr);
        }

        public static string GetTypeName(Type t) => DXUnitHelper.GetTypeName(t);
    }    
}