using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXEnumDefinitionUnit")]
    public class DXEnumDefinitionUnit : DXObjectDefinitionUnit
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = DXModelConverter.GetESQLModelDefinition<DXEnumDefinitionUnit>();
    }
}