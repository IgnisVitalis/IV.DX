using System.Reflection;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json;

namespace IV.DX.Kernel.Data.Models
{
    internal static class DXElementDefinitionUnitItems
    {
        public static IEnumerable<DXElementDefinitionUnit> Items { get; private set; }

        static DXElementDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                Assembly.GetAssembly(typeof(DXUnitAttribute)),
                "Data/DXCore/01_01_0001_DXCore_DXElementDefinitionUnit.unit");

            var blocks = JsonConvert.DeserializeObject<List<DXDataBlock<DXUnitRecord>>>(text)
                         ?? new List<DXDataBlock<DXUnitRecord>>();

            Items = DXRecordConverter.ToDXUnits<DXElementDefinitionUnit>(blocks).ToList();
        }
    }
}
