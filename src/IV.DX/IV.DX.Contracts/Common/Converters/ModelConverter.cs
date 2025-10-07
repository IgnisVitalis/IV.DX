using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Helpers;
using IV.DX.Contracts.Common.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Contracts.Common.Converters
{
    public static class ModelConverter
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
                .Where(x => AttributeReader.GetAttribute<ESQLColumnDefinitionAttribute>(x) != null);

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetAttribute<ESQLColumnDefinitionAttribute>(property);

                ESQLPropertyDefinition item = new ESQLPropertyDefinition(property.Name, attribute);

                esqlBlockDefinition.AddPropertyDefinition(item);
            }

            return esqlBlockDefinition;
        }

        public static ESQLBlockDefinition GetESQLBlockDefinition(DPEnumDescObject enumDesc)
        {
            ESQLBlockDefinition esqlBlockDefinition = new ESQLBlockDefinition(enumDesc.DPObjectDescGenBlock.Name, enumDesc.DPObjectDescGenBlock.Name);

            JObject jObject = new JObject();

            foreach (var column in enumDesc.DPColumnDescBlock.Announced)
            {
                ESQLPropertyDefinition item = new ESQLPropertyDefinition(column.Name, new ESQLColumnDefinitionAttribute(column.Name));

                esqlBlockDefinition.AddPropertyDefinition(item);
            }

            if (esqlBlockDefinition.SingleOrDefault(x => x.ColumnDefinition.ESQLExpression == Constants.ID) == null)
            {
                ESQLPropertyDefinition item = new ESQLPropertyDefinition(Constants.ID, new ESQLColumnDefinitionAttribute(Constants.ID));

                esqlBlockDefinition.AddPropertyDefinition(item);
            }

            return esqlBlockDefinition;
        }
    }
}