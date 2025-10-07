using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLBlockDefinition("DPEntityInheritanceBlock")]
    public class DPEntityInheritanceBlock : ESQLBlock
    {
        [ESQLColumnDefinition("BaseEntity")]
        public Guid BaseEntity { get; set; }
    }
}