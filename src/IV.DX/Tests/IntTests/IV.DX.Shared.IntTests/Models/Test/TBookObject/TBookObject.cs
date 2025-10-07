using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [DXUnit("TBookUnit")]
    public class TBookUnit : DXUnit
    {
        public TBookMainElement TBookMainElement { get; set; }
        public DXMultiElementsContainer<TBookChapterElement> TBookChapterElement { get; set; }
    }
}