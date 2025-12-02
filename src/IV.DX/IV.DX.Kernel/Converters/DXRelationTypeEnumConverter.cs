using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Converters
{
    internal static class DXRelationTypeEnumConverter
    {
        public static DXRelationTypeEnum ToDXRelationTypeEnum(this DXElementInUnitTypeEnum relationType)
        {
            switch (relationType)
            {
                case DXElementInUnitTypeEnum.SingleMandatory:
                    return DXRelationTypeEnum.ZeroOneToZeroOne;
                case DXElementInUnitTypeEnum.SingleOptional:
                    return DXRelationTypeEnum.ZeroOneToZeroOne;
                case DXElementInUnitTypeEnum.MultiMandatory:
                    return DXRelationTypeEnum.ZeroOneToMany;
                case DXElementInUnitTypeEnum.MultiOptional:
                    return DXRelationTypeEnum.ZeroOneToMany;
                default:
                    throw new Exception($"DXElementInUnitTypeEnum doesn't contain '{relationType}' value");
            }
        }
    }
}