using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXElementDefinitionConverter
    {
        public static DXElementDefinition ToDXElementDefinition(this DXElementDefinitionUnit dxElement, IEnumerable<DXRelationDefinitionUnit> relations, bool isRequired)
        {
            var props = dxElement.DXColumnDefinitionElement.Announced
                        .Select(y => new DXPropertyDefinition(y.Name, new DXColumnAttribute(y.Name)));
            
            var singleFragmentDefinition =
                new DXElementDefinition(
                    dxElement.Name,
                    dxElement.Name,
                    isRequired);

            singleFragmentDefinition.AddPropertyDefinitions(props);            

            //var relationsAsProperties = relations.ToDXPropertyDefinitions(dxElement.Name);

            //singleFragmentDefinition.AddPropertyDefinitions(relationsAsProperties);

            return singleFragmentDefinition;
        }

        public static DXElementDefinition ToDXElementDefinition(string type, Type dxElementType, bool isRequired)
        {
            DXElementDefinition dxElementDefinition = new DXElementDefinition(type, type, isRequired);
            JObject jObject = new JObject();

            var properties = dxElementType.GetProperties()
                .Where(x => AttributeReader.GetAttribute<DXColumnAttribute>(x) != null);

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetAttribute<DXColumnAttribute>(property);

                DXPropertyDefinition item = new DXPropertyDefinition(property.Name, attribute);

                dxElementDefinition.AddPropertyDefinition(item);
            }

            return dxElementDefinition;
        }

        public static DXElementDefinition ToDXElementDefinition(this DXEnumDefinitionUnit enumDesc, bool isRequired)
        {
            DXElementDefinition dxElementDefinition = new DXElementDefinition(enumDesc.Name, enumDesc.Name, isRequired);

            JObject jObject = new JObject();

            foreach (var column in enumDesc.DXColumnDefinitionElement.Announced)
            {
                DXPropertyDefinition item = new DXPropertyDefinition(column.Name, new DXColumnAttribute(column.Name));

                dxElementDefinition.AddPropertyDefinition(item);
            }

            if (dxElementDefinition.SingleOrDefault(x => x.ColumnDefinition.DXExpression == Constants.ID) == null)
            {
                DXPropertyDefinition item = new DXPropertyDefinition(Constants.ID, new DXColumnAttribute(Constants.ID));

                dxElementDefinition.AddPropertyDefinition(item);
            }

            return dxElementDefinition;
        }
    }
}
