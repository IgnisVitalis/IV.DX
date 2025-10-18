using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public abstract class DXUnit
    {
        public Guid ID { get; set; }

        [DXColumn("TimeStamp", "TimeStamp", DXLoadingType.Base)]
        public DateTime TimeStamp { get; set; }

        public DXUnit()
        {

        }

        public JObject ToJObject()
        {
            return DXUnitHelper.ConvertToJObject(this);
        }
    }
}