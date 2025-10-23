using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace IV.DX.Kernel.Models
{
    public abstract class DXElement
    {
        [DXColumn("ID", "ID", DXLoadingType.Base)]
        public Guid ID { get; set; }
        [DXColumn("DXUnitID", "DXUnitID", DXLoadingType.Base)]
        public Guid DXUnitID { get; set; }
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

        //public JObject ToJObject()
        //{
        //    return DXUnitHelper.ConvertToJObject(this);
        //}
    }
}