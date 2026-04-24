using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace IV.DX.Kernel.Migration.Models
{
    internal static class DXUnitDefinitionUnitItems
    {
        public static IEnumerable<DXUnitDefinitionUnit> Items { get; private set; }

        static DXUnitDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                 Assembly.GetAssembly(typeof(DXUnitAttribute))!,
                 "Migration/DXCore/01_01_0002_DXCore_DXUnitDefinitionUnit.dx");

            var blocks = JsonConvert.DeserializeObject<List<DXDataBlock<DXUnitRecord>>>(text)
                         ?? new List<DXDataBlock<DXUnitRecord>>();

            Items = DXRecordConverter.ToDXUnits<DXUnitDefinitionUnit>(blocks).ToList();
        }
    }
}
