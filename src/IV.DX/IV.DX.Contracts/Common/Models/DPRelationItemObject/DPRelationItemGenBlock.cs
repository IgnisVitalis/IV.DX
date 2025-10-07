using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLBlockDefinition("DPRelationItemGenBlock")]
    public class DPRelationItemGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("ObjectTypeNameLeft")]
        public string ObjectTypeNameLeft { get; set; }
        [ESQLColumnDefinition("ObjectIDLeft")]
        public Guid ObjectIDLeft { get; set; }
        [ESQLColumnDefinition("RelationNameRight")]
        public string RelationNameRight { get; set; }
        [ESQLColumnDefinition("ObjectTypeNameRight")]
        public string ObjectTypeNameRight { get; set; }
        [ESQLColumnDefinition("ObjectIDRight")]
        public Guid ObjectIDRight { get; set; }
    }
}