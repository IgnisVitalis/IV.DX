using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLObjectDefinition("TBookObject")]
    public class TBookObject : ESQLObject
    {
        public TBookGenBlock TBookGenBlock { get; set; }
        public ESQLMultiItemsContainer<TBookChapterBlock> TBookChapterBlock { get; set; }
    }
}