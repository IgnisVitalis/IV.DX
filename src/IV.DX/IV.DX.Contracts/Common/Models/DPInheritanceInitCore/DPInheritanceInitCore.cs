using IV.DataProvider.Persistence.Contracts.Models;

namespace IV.DX.Contracts.Common.Models
{
    public class DPInheritanceInitCore : ESQLObject
    {
        public string BaseEntity { get; set; }
        public string ChildEntity { get; set; }
    }
}