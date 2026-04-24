using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXMainTableDefinitionConverter
    {
        public static DXMainTableDefinition ToDXTableDefinition(string dxUnitTypeName, Type dxElementType)
        {
            DXMainTableDefinition dxElementDefinition = new DXMainTableDefinition(dxUnitTypeName, dxUnitTypeName);
        
            var properties = dxElementType.GetProperties()
                .Where(x => AttributeReader.GetAttribute<DXColumnAttribute>(x) != null);

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetAttribute<DXColumnAttribute>(property);

                DXColumnDefinition item = new DXColumnDefinition(property.Name, attribute!);

                dxElementDefinition.AddPropertyDefinition(item);
            }

            return dxElementDefinition;
        }
        
        
        public static DXMainTableDefinition ToDXTableDefinition(string dxUnitTypeName, HashSet<string> columnNames)
        {
            DXMainTableDefinition dxElementDefinition = new DXMainTableDefinition(dxUnitTypeName, dxUnitTypeName);
         
            foreach (var columnName in columnNames)
            {
                DXColumnDefinition item = new DXColumnDefinition(columnName, new DXColumnAttribute(columnName));

                dxElementDefinition.AddPropertyDefinition(item);
            }

            return dxElementDefinition;
        }
    }
}
