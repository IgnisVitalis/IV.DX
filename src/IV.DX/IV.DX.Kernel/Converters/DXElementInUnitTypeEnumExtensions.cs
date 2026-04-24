using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Converters
{
    internal static class DXElementInUnitTypeEnumExtensions
    {
        public static DXRelationTypeEnum ToDXRelationTypeEnum(this DXElementInUnitTypeEnum value)
        {
            return value switch
            {
                DXElementInUnitTypeEnum.SingleMandatory => DXRelationTypeEnum.OneToZeroOne,
                DXElementInUnitTypeEnum.SingleOptional => DXRelationTypeEnum.ZeroOneToOne,
                DXElementInUnitTypeEnum.MultiMandatory => DXRelationTypeEnum.OneToMany,
                DXElementInUnitTypeEnum.MultiOptional => DXRelationTypeEnum.ZeroOneToMany,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown DXElementInUnitTypeEnum value.")
            };
        }
    }
}
