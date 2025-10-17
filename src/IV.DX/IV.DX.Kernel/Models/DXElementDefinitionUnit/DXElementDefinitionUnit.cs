using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXElementDefinitionUnit")]
    public class DXElementDefinitionUnit : DXObjectDefinitionUnit
    {
        public static DXModelDefinition DXModelDefinition { get; } 
            = DXModelDefinitionHelper.GetDXModelDefinition<DXElementDefinitionUnit>();
    }
}