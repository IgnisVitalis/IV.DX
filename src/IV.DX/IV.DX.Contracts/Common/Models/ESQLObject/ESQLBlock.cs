using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Enums;

namespace IV.DataProvider.Persistence.Contracts.Models
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