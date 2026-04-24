using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace IV.DX.Kernel.Migration.Models
{
    internal static class DXElementDefinitionUnitItems
    {
        public static IEnumerable<DXElementDefinitionUnit> Items { get; private set; }

        static DXElementDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                Assembly.GetAssembly(typeof(DXUnitAttribute))!,
                "Migration/DXCore/01_01_0001_DXCore_DXElementDefinitionUnit.dx");

            var blocks = JsonConvert.DeserializeObject<List<DXDataBlock<DXUnitRecord>>>(text)
                         ?? new List<DXDataBlock<DXUnitRecord>>();

            Items = DXRecordConverter.ToDXUnits<DXElementDefinitionUnit>(blocks).ToList();
        }
    }
}
