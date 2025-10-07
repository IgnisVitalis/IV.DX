using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    public abstract class DXElement
    {
        [DXColumn("ID", "ID", DXLoadingType.Base)]
        public Guid ID { get; set; }
        [DXColumn("ObjectID", "ObjectID", DXLoadingType.Base)]
        public Guid ObjectID { get; set; }
        [DXColumn("TimeStamp", "TimeStamp", DXLoadingType.Base)]
        public DateTime TimeStamp { get; set; }
    }
}