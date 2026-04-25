using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.JObjectConverters;
using IV.DX.Kernel.Enums;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace IV.DX.Kernel.Models
{
    public abstract class DXElement
    {
        [DXColumn("Id", "Id", DXLoadingType.Base)]
        public Guid Id { get; set; }
        [DXColumn("DXUnitId", "DXUnitId", DXLoadingType.Base)]
        public Guid DXUnitId { get; set; }
        [DXColumn("TimeStamp", "TimeStamp", DXLoadingType.Base)]
        public DateTime TimeStamp { get; set; }


        private static readonly ConcurrentDictionary<Type, bool> _validated = new();

        public DXElement()
        {
            var t = GetType();

            _validated.GetOrAdd(t, static type =>
            {
                if (!Attribute.IsDefined(type, typeof(DXElementAttribute), inherit: true))
                    throw new InvalidOperationException(
                        $"Type {type.FullName} should have [{nameof(DXElementAttribute)}] attribute.");
                return true;
            });
        }

        public JObject? ToJObject()
        {
            return JObjectConverter.ToJObject(this);
        }
    }
}
