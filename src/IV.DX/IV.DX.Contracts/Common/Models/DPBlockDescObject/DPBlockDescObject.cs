using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Converters;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLObjectDefinition("DPBlockDescObject")]
    public class DPBlockDescObject : DPObjectDescObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DPBlockDescObject>();
    }
}