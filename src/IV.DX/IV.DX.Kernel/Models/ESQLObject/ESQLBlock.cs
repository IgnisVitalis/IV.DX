using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    public abstract class ESQLBlock
    {
        [ESQLColumnDefinition("ID", "ID", TypeOfEntityLoading.Base)]
        public Guid ID { get; set; }
        [ESQLColumnDefinition("ObjectID", "ObjectID", TypeOfEntityLoading.Base)]
        public Guid ObjectID { get; set; }
        [ESQLColumnDefinition("TimeStamp", "TimeStamp", TypeOfEntityLoading.Base)]
        public DateTime TimeStamp { get; set; }
    }
}