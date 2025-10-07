using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Converters;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLObjectDefinition("DPEnumDescObject")]
    public class DPEnumDescObject : DPObjectDescObject
    {
        public static ESQLModelDefinition ESQLModelDefinition { get; } = ModelConverter.GetESQLModelDefinition<DPEnumDescObject>();
    }
}