using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DXEnumDefinitionUnit")]
    public class DXEnumDefinitionUnit : DXObjectDefinitionUnit
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DXEnumDefinitionUnit>();
    }
}