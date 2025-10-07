using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    public abstract class ESQLObject
    {
        public Guid ID { get; set; }

        [ESQLColumnDefinition("TimeStamp", "TimeStamp", TypeOfEntityLoading.Base)]
        public DateTime TimeStamp { get; set; }

        public ESQLObject()
        {

        }
    }
}