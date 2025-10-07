using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Enums;

namespace IV.DataProvider.Persistence.Contracts.Models
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