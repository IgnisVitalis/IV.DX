using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLObjectDefinition("DPRelationItemObject")]
    public class DPRelationItemObject : ESQLObject
    {
        public DPRelationItemGenBlock DPRelationItemGenBlock { get; set; }
    }
}