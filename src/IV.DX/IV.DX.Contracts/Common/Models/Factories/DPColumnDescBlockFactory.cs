using IV.DX.Contracts.Common.Enums;

namespace IV.DX.Contracts.Common.Models
{
    public static class DPColumnDescBlockFactory
    {
        public static DPColumnDescBlock GetIntColumn(Guid id, string columnName, bool allowNull, string defaultValue)
        {
            return new DPColumnDescBlock()
            {
                ID = id,
                Name = columnName,
                AllowNull = allowNull,
                ColumnType = DPColumnTypeEnum.Int,
                Scale = null,
                Precision = null,
                DefaultValue = defaultValue,
                Length = null
            };
        }

        public static DPColumnDescBlock GetIntColumn(string columnName, bool allowNull, string defaultValue)
        {
            return GetIntColumn(Guid.NewGuid(), columnName, allowNull, defaultValue);
        }

        public static DPColumnDescBlock GetStringColumn(Guid id, string columnName, int length, bool allowNull, string defaultValue)
        {
            return new DPColumnDescBlock()
            {
                ID = id,
                Name = columnName,
                AllowNull = allowNull,
                ColumnType = DPColumnTypeEnum.String,
                Scale = null,
                Precision = null,
                DefaultValue = defaultValue,
                Length = length
            };
        }

        public static DPColumnDescBlock GetStringColumn(string columnName, int length, bool allowNull, string defaultValue)
        {
            return GetStringColumn(Guid.NewGuid(), columnName, length, allowNull, defaultValue);
        }

        public static DPColumnDescBlock GetIntColumn(string columnName, bool allowNull)
        {
            return GetIntColumn(Guid.NewGuid(), columnName, allowNull, null);
        }
    }
}