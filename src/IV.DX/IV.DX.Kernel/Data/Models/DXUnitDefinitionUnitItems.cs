using System.Reflection;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json;

namespace IV.DX.Kernel.Data.Models
{
    internal static class DXUnitDefinitionUnitItems
    {
        public static IEnumerable<DXUnitDefinitionUnit> Items { get; private set; }

        static DXUnitDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                 Assembly.GetAssembly(typeof(DXUnitAttribute)),
                 "Data/DXCore/01_01_0002_DXCore_DXUnitDefinitionUnit.unit");

            var blocks = JsonConvert.DeserializeObject<List<DXDataBlock<DXUnitRecord>>>(text)
                         ?? new List<DXDataBlock<DXUnitRecord>>();

            Items = DXRecordConverter.ToDXUnits<DXUnitDefinitionUnit>(blocks).ToList();
        }
    }
}
