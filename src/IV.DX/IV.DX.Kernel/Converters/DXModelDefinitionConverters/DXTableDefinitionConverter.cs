using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXTableDefinitionConverter
    {
        public static DXTableDefinition ToDXTableDefinition(
            this DXElementDefinitionUnit dxElement,
            string dxUnitTypeName,
            IEnumerable<DXRelationDefinitionUnit> relations, bool isRequired)
        {
            var props = dxElement.DXColumnDefinitionElement.Announced
                        .Select(y => new DXColumnDefinition(y.Name, new DXColumnAttribute(y.Name)));

            var singleFragmentDefinition =
                new DXTableDefinition(
                    dxUnitTypeName,
                    dxElement.Name,
                    dxElement.Name,
                    isRequired);

            singleFragmentDefinition.AddPropertyDefinitions(props);

            //var relationsAsProperties = relations.ToDXPropertyDefinitions(dxElement.Name);

            //singleFragmentDefinition.AddPropertyDefinitions(relationsAsProperties);

            return singleFragmentDefinition;
        }

        public static DXTableDefinition ToDXTableDefinition(string dxElementTypeName, string dxUnitTypeName, Type dxElementType, bool isRequired)
        {
            DXTableDefinition dxElementDefinition = new DXTableDefinition(dxUnitTypeName, dxElementTypeName, dxElementTypeName, isRequired);

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

        public static DXTableDefinition ToDXTableDefinition(
            string dxElementTypeName,
            string dxUnitTypeName,
            HashSet<string> columnNames, 
            bool isRequired)
        {
            DXTableDefinition dxElementDefinition = new DXTableDefinition(dxUnitTypeName, dxElementTypeName, dxElementTypeName, isRequired);

            foreach (var columnName in columnNames)
            {          
                DXColumnDefinition item = new DXColumnDefinition(columnName, new DXColumnAttribute(columnName));

                dxElementDefinition.AddPropertyDefinition(item);
            }

            return dxElementDefinition;
        }
    }
}
