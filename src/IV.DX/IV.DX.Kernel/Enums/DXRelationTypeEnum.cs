namespace IV.DX.Kernel.Enums
{
    public enum DXRelationTypeEnum
    {
        OneToZeroOne = 1,
        ZeroOneToOne = 2,
        OneToMany = 3,
        ManyToOne = 4,
        ZeroOneToMany = 5,
        ManyToZeroOne = 6,
        ManyToMany = 7,
        ZeroOneToZeroOne = 8
    }

    internal static class DXRelationTypeEnumHelper
    {
        public static DXRelationTypeEnum GetInvertedRelationType(DXRelationTypeEnum value)
        {
            switch (value)
            {
                case DXRelationTypeEnum.OneToZeroOne:
                    return DXRelationTypeEnum.ZeroOneToOne;
                case DXRelationTypeEnum.ZeroOneToOne:
                    return DXRelationTypeEnum.OneToZeroOne;
                case DXRelationTypeEnum.OneToMany:
                    return DXRelationTypeEnum.ManyToOne;
                case DXRelationTypeEnum.ManyToOne:
                    return DXRelationTypeEnum.OneToMany;
                case DXRelationTypeEnum.ZeroOneToMany:
                    return DXRelationTypeEnum.ManyToZeroOne;
                case DXRelationTypeEnum.ManyToZeroOne:
                    return DXRelationTypeEnum.ZeroOneToMany;
                case DXRelationTypeEnum.ManyToMany:
                    return DXRelationTypeEnum.ManyToMany;
                case DXRelationTypeEnum.ZeroOneToZeroOne:
                    return DXRelationTypeEnum.ZeroOneToZeroOne;
            }

            return value;
        }
    }
}