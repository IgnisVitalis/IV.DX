using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Kernel.Data.Models
{
    internal static class DXEnumDefinitionUnitItems
    {
        public static IEnumerable<DXEnumDefinitionUnit> Items { get; private set; }

        static DXEnumDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                Assembly.GetAssembly(typeof(DXUnitAttribute)),
                "Data/Core/01_01_0000_Core_DXEnumDefinitionUnit.dat");

            Items = DXUnitConverter.ToDXUnits<DXEnumDefinitionUnit>(text).ToList();
        }
    }
}
