using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters
{
    internal static class DXModelDefinitionHelper
    {
        public static DXModelDefinition GetESQLModelDefinition<T>() where T : DXUnit
        {
            Type type = typeof(T);

            return GetESQLModelDefinition(type);
        }

        public static DXModelDefinition GetESQLModelDefinition(DXModel dxModel)
        {
            var mainItemDefinition = new DXElementDefinition(dxModel.OwnSingleItem.ObjectInfo.ObjectName, dxModel.OwnSingleItem.ObjectInfo.ObjectName);

            mainItemDefinition.AddPropertyDefinitions(dxModel.OwnSingleItem.Item.Content.Children().Select(x => x as JProperty).Where(x => x != null).Select(x =>
                new DXPropertyDefinition(x.Name, new DXColumnAttribute(x.Name))).ToList());

            var singleItemDefinitions =
                dxModel.SingleItems.Select(x =>
                {
                    var item = new DXElementDefinition(x.BlockInfo.BlockName, x.Name);

                    var propertyNames = x.Item.Content.Children().Select(y => y as JProperty).Select(y => y.Name).ToList();

                    if (!propertyNames.Contains(Constants.ID))
                    {
                        propertyNames.Add(Constants.ID);
                    }

                    if (!propertyNames.Contains(Constants.ObjectID))
                    {
                        propertyNames.Add(Constants.ObjectID);
                    }

                    item.AddPropertyDefinitions(propertyNames.Select(y => new DXPropertyDefinition(y, new DXColumnAttribute(y))).ToList());

                    return item;
                }).ToList();

            var multiItemDefinitions =
                  dxModel.MultiItems.Select(x =>
                  {
                      var item = new DXElementDefinition(x.BlockInfo.BlockName, x.Name);

                      var existingElement = x.Announced.Count() > 0 ? x.Announced.First() : (x.Deleted.Count() > 0 ? x.Deleted.First() : null);

                      if (existingElement == null)
                          return null;

                      var propertyNames = existingElement.Content.Children().Select(y => y as JProperty).Select(y => y.Name).ToList();

                      if (!propertyNames.Contains(Constants.ID))
                      {
                          propertyNames.Add(Constants.ID);
                      }

                      if (!propertyNames.Contains(Constants.ObjectID))
                      {
                          propertyNames.Add(Constants.ObjectID);
                      }

                      item.AddPropertyDefinitions(propertyNames.Select(y => new DXPropertyDefinition(y, new DXColumnAttribute(y))).ToList());

                      return item;
                  }).Where(x => x != null).ToList();


            var result = new DXModelDefinition(mainItemDefinition);
            result.AppendToSingleItemDefinitions(singleItemDefinitions);
            result.AppendToMultiItemDefinitions(multiItemDefinitions);

            return result;
        }

        public static DXModelDefinition GetESQLModelDefinition(Type type)
        {
            var asqlTypeName = AttributeReader.GetESQLObjectTypeName(type);

            var ownItem = GetDXElementDefinition(asqlTypeName, type);

            DXModelDefinition result = new DXModelDefinition(ownItem);

            var singleItemInfos = AttributeReader.GetSingleItemInfos(type);
            var multiItemInfos = AttributeReader.GetMultiItemInfos(type);

            var singleItemDefinitions = singleItemInfos.Select(x => GetDXElementDefinition(x.Name, x.PropertyType)).ToList();
            var mutliItemDefinitions = multiItemInfos.Select(x => GetDXElementDefinition(x.Name, x.PropertyType.GenericTypeArguments[0])).ToList();

            result.AppendToSingleItemDefinitions(singleItemDefinitions);
            result.AppendToMultiItemDefinitions(mutliItemDefinitions);

            return result;
        }

        public static DXElementDefinition GetDXElementDefinition(string type, Type dxElementType)
        {
            DXElementDefinition dxElementDefinition = new DXElementDefinition(type, type);
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

        public static DXElementDefinition GetDXElementDefinition(DXEnumDefinitionUnit enumDesc)
        {
            DXElementDefinition dxElementDefinition = new DXElementDefinition(enumDesc.DXUnitDefinitionMainElement.Name, enumDesc.DXUnitDefinitionMainElement.Name);

            JObject jObject = new JObject();

            foreach (var column in enumDesc.DXColumnDefinitionElement.Announced)
            {
                DXPropertyDefinition item = new DXPropertyDefinition(column.Name, new DXColumnAttribute(column.Name));

                dxElementDefinition.AddPropertyDefinition(item);
            }

            if (dxElementDefinition.SingleOrDefault(x => x.ColumnDefinition.ESQLExpression == Constants.ID) == null)
            {
                DXPropertyDefinition item = new DXPropertyDefinition(Constants.ID, new DXColumnAttribute(Constants.ID));

                dxElementDefinition.AddPropertyDefinition(item);
            }

            return dxElementDefinition;
        }
    }
}