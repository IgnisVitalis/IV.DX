using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using System.Reflection;

namespace IV.DX.Kernel.Data.Models
{
    internal static class DXRelationDefinitionUnitItems
    {
        public static IEnumerable<DXRelationDefinitionUnit> Items { get; private set; }

        static DXRelationDefinitionUnitItems()
        {
            var text = ResourceReader.ReadEmbeddedText(
                Assembly.GetAssembly(typeof(DXUnitAttribute)),
                "Data/DXCore/01_01_0003_DXCore_DXRelationDefinitionUnit.dat");

            var items = DXUnitConverter.ToDXUnits<DXRelationDefinitionUnit>(text);
            var revertedItems = items.Select(x => x.CreateInvertedRelationObject()).ToList();
            
            Items = items.Concat(revertedItems).ToList();
        }
    }
}