using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Kernel.CoreData.Models
{
    internal static class DXRelationDefinitionUnitItems
    {
        public static IEnumerable<DXRelationDefinitionUnit> Items { get; private set; }

        static DXRelationDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                Assembly.GetAssembly(typeof(DXUnitAttribute)),
                "CoreData/Data/01_01_0003_Core_DXRelationDefinitionUnit.dat");

            Items = DXUnitConverter.ToDXUnits<DXRelationDefinitionUnit>(text);
        }
    }
}