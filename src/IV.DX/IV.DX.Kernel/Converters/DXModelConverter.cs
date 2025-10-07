using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters
{
    internal static class DXModelConverter
    {
        public static ESQLModelDefinition GetESQLModelDefinition<T>() where T : ESQLObject
        {
            Type type = typeof(T);

            return GetESQLModelDefinition(type);
        }

        public static ESQLModelDefinition GetESQLModelDefinition(Type type)
        {
            var asqlTypeName = AttributeReader.GetESQLObjectTypeName(type);

            var ownItem = GetESQLBlockDefinition(asqlTypeName, type);

            ESQLModelDefinition result = new ESQLModelDefinition(ownItem);

            var singleItemInfos = AttributeReader.GetSingleItemInfos(type);
            var multiItemInfos = AttributeReader.GetMultiItemInfos(type);

            var singleItemDefinitions = singleItemInfos.Select(x => GetESQLBlockDefinition(x.Name, x.PropertyType)).ToList();
            var mutliItemDefinitions = multiItemInfos.Select(x => GetESQLBlockDefinition(x.Name, x.PropertyType.GenericTypeArguments[0])).ToList();

            result.AppendToSingleItemDefinitions(singleItemDefinitions);
            result.AppendToMultiItemDefinitions(mutliItemDefinitions);

            return result;
        }

        public static ESQLBlockDefinition GetESQLBlockDefinition(string type, Type esqlBlockType)
        {
            ESQLBlockDefinition esqlBlockDefinition = new ESQLBlockDefinition(type, type);
            JObject jObject = new JObject();

            var properties = esqlBlockType.GetProperties()
                .Where(x => AttributeReader.GetAttribute<DXColumnAttribute>(x) != null);

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetAttribute<DXColumnAttribute>(property);

                ESQLPropertyDefinition item = new ESQLPropertyDefinition(property.Name, attribute);

                esqlBlockDefinition.AddPropertyDefinition(item);
            }

            return esqlBlockDefinition;
        }

        public static ESQLBlockDefinition GetESQLBlockDefinition(DXEnumDefinitionUnit enumDesc)
        {
            ESQLBlockDefinition esqlBlockDefinition = new ESQLBlockDefinition(enumDesc.DXUnitDefinitionMainElement.Name, enumDesc.DXUnitDefinitionMainElement.Name);

            JObject jObject = new JObject();

            foreach (var column in enumDesc.DXColumnDefinitionElement.Announced)
            {
                ESQLPropertyDefinition item = new ESQLPropertyDefinition(column.Name, new DXColumnAttribute(column.Name));

                esqlBlockDefinition.AddPropertyDefinition(item);
            }

            if (esqlBlockDefinition.SingleOrDefault(x => x.ColumnDefinition.ESQLExpression == Constants.ID) == null)
            {
                ESQLPropertyDefinition item = new ESQLPropertyDefinition(Constants.ID, new DXColumnAttribute(Constants.ID));

                esqlBlockDefinition.AddPropertyDefinition(item);
            }

            return esqlBlockDefinition;
        }
    }
}