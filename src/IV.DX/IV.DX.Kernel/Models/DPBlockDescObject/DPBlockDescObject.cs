using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DPBlockDescObject")]
    public class DPBlockDescObject : DPObjectDescObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } 
            = ModelConverter.GetESQLModelDefinition<DPBlockDescObject>();
    }
}