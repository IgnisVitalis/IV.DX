using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXUnit("TBookObject")]
    public class TBookObject : DXUnit
    {
        public TBookGenBlock TBookGenBlock { get; set; }
        public DXMultiElementsContainer<TBookChapterBlock> TBookChapterBlock { get; set; }
    }
}