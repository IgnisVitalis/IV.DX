using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DXElementDefinitionUnit")]
    public class DXElementDefinitionUnit : DPObjectDescObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } 
            = ModelConverter.GetESQLModelDefinition<DXElementDefinitionUnit>();
    }
}