using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXTableDefinitionConverter
    {
        public static DXTableDefinition ToDXTableDefinition(this DXElementDefinitionUnit dxElement, string dxUnitTypeName, IEnumerable<DXRelationDefinitionUnit> relations, bool isRequired)
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
            JObject jObject = new JObject();

            var properties = dxElementType.GetProperties()
                .Where(x => AttributeReader.GetAttribute<DXColumnAttribute>(x) != null);

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetAttribute<DXColumnAttribute>(property);

                DXColumnDefinition item = new DXColumnDefinition(property.Name, attribute);

                dxElementDefinition.AddPropertyDefinition(item);
            }

            return dxElementDefinition;
        }
    }
}
