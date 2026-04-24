using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace IV.DX.Kernel.Migration.Models
{
    internal static class DXEnumDefinitionUnitItems
    {
        public static IEnumerable<DXEnumDefinitionUnit> Items { get; private set; }

        static DXEnumDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                Assembly.GetAssembly(typeof(DXUnitAttribute))!,
                "Migration/DXCore/01_01_0000_DXCore_DXEnumDefinitionUnit.dx");

            var blocks = JsonConvert.DeserializeObject<List<DXDataBlock<DXUnitRecord>>>(text)
                         ?? new List<DXDataBlock<DXUnitRecord>>();

            Items = DXRecordConverter.ToDXUnits<DXEnumDefinitionUnit>(blocks).ToList();
        }
    }
}
