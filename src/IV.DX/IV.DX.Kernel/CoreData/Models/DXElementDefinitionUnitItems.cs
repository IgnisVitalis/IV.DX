using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Kernel.CoreData.Models
{
    internal static class DXElementDefinitionUnitItems
    {
        public static IEnumerable<DXElementDefinitionUnit> Items { get; private set; }

        static DXElementDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                Assembly.GetAssembly(typeof(DXUnitAttribute)), 
                "CoreData/Data/01_01_0001_Core_DXElementDefinitionUnit.dat");

            Items = DXUnitConverter.ToDXUnits<DXElementDefinitionUnit>(text);
        }
    }
}
