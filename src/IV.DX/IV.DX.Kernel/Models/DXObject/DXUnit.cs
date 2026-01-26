using System.Collections.Concurrent;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Converters.JObjectConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers.DXObjectHelpers;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public abstract class DXUnit
    {
        [DXColumn("ID", "ID", DXLoadingType.Base)]
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
            return JObjectConverter.ToJObject(this);
        }

        public DXModel ToDXModel()
        {
            return DXModelConverter.ToDXModel(this);
        }

        public static T Parse<T>(JObject jObject) where T : DXUnit
        {
            ArgumentNullException.ThrowIfNull(jObject);

            return DXUnitConverter.ToDXUnits<T>(jObject);
        }

        public static T Parse<T>(DXModel dxModel) where T : DXUnit
        {
            ArgumentNullException.ThrowIfNull(dxModel);

            return DXUnitConverter.ToDXUnits<T>(dxModel);
        }

        public static T Parse<T>(string jObjectStr) where T : DXUnit
        {
            ArgumentNullException.ThrowIfNullOrEmpty(jObjectStr);

            return DXUnitConverter.ToDXUnit<T>(jObjectStr);
        }

        public static IEnumerable<T> ParseItems<T>(string jArrayStr) where T : DXUnit
        {
            return DXUnitConverter.ToDXUnits<T>(jArrayStr);
        }

        public static IEnumerable<T> ParseItems<T>(JArray jArray) where T : DXUnit
        {
            return DXUnitConverter.ToDXUnits<T>(jArray);
        }

        public static string GetTypeName<T>() where T : DXUnit => DXUnitHelper.GetTypeName(typeof(T));
    }    
}