using System.Reflection;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Data.Models
{
    internal static class DXUnitDefinitionUnitItems
    {
        public static IEnumerable<DXUnitDefinitionUnit> Items { get; private set; }

        static DXUnitDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                 Assembly.GetAssembly(typeof(DXUnitAttribute)),
                 "Data/DXCore/01_01_0002_DXCore_DXUnitDefinitionUnit.dat");

            Items = DXUnitConverter.ToDXUnits<DXUnitDefinitionUnit>(text).ToList();
        }
    }
}