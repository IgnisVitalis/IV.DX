using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXEnumDefinitionUnit")]
    public class DXEnumDefinitionUnit : DXObjectDefinitionUnit
    {
        public DXColumnDefinitionElement GetColumnValue()
        {
            var uniqueColumnsWithSingleColumn = this.DXUniqueColumnsElement.Announced.Select(x => x.Columns).ToList();

            var columnsWithIntValue =
                this.DXColumnDefinitionElement
                .Announced.Where(x => x.ColumnType == Enums.DXColumnTypeEnum.Int && uniqueColumnsWithSingleColumn.Contains(x.Name)).ToList();

            if (columnsWithIntValue.Count > 1)
                throw new Exception($"DXUnit '{this.Name}' has more than 1 unique int value");

            if (columnsWithIntValue.Count == 0)
                throw new Exception($"DXUnit '{this.Name}' has 0 unique int value");

            return columnsWithIntValue.Single();
        }
    }
}