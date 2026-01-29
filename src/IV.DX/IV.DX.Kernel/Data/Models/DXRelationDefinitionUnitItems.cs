using System.Reflection;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json;

namespace IV.DX.Kernel.Data.Models
{
    internal static class DXRelationDefinitionUnitItems
    {
        public static IEnumerable<DXRelationDefinitionUnit> Items { get; private set; }

        static DXRelationDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                Assembly.GetAssembly(typeof(DXUnitAttribute)),
                "Data/DXCore/01_01_0003_DXCore_DXRelationDefinitionUnit.unit");

            var blocks = JsonConvert.DeserializeObject<List<DXDataBlock<DXUnitRecord>>>(text)
                         ?? new List<DXDataBlock<DXUnitRecord>>();

            var items = DXRecordConverter.ToDXUnits<DXRelationDefinitionUnit>(blocks).ToList();
            var revertedItems = items.Select(x => x.CreateInvertedRelationObject()).ToList();

            Items = items.Concat(revertedItems).ToList();
        }
    }
}
