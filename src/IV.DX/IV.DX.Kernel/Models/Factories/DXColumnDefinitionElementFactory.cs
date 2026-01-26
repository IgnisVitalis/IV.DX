using System;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    public static class DXColumnDefinitionElementFactory
    {
        public static DXColumnDefinitionElement GetIntColumn(Guid id, string columnName, bool allowNull, string defaultValue)
        {
            return new DXColumnDefinitionElement()
            {
                ID = id,
                Name = columnName,
                AllowNull = allowNull,
                ColumnType = DXColumnTypeEnum.Int,
                Scale = null,
                Precision = null,
                DefaultValue = defaultValue,
                Length = null
            };
        }

        public static DXColumnDefinitionElement GetIntColumn(string columnName, bool allowNull, string defaultValue)
        {
            return GetIntColumn(Guid.NewGuid(), columnName, allowNull, defaultValue);
        }

        public static DXColumnDefinitionElement GetStringColumn(Guid id, string columnName, int length, bool allowNull, string defaultValue)
        {
            return new DXColumnDefinitionElement()
            {
                ID = id,
                Name = columnName,
                AllowNull = allowNull,
                ColumnType = DXColumnTypeEnum.String,
                Scale = null,
                Precision = null,
                DefaultValue = defaultValue,
                Length = length
            };
        }

        public static DXColumnDefinitionElement GetStringColumn(string columnName, int length, bool allowNull, string defaultValue)
        {
            return GetStringColumn(Guid.NewGuid(), columnName, length, allowNull, defaultValue);
        }

        public static DXColumnDefinitionElement GetIntColumn(string columnName, bool allowNull)
        {
            return GetIntColumn(Guid.NewGuid(), columnName, allowNull, null);
        }
    }
}