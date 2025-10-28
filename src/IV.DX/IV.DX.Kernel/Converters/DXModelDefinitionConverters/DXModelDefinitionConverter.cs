using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXModelDefinitionConverter
    {
        public static DXModelDefinition Get<T>() where T : DXUnit
        {
            Type type = typeof(T);

            return Get(type);
        }

        public static DXModelDefinition Get(DXModel dxModel)
        {
            var mainItemDefinition = new DXElementDefinition(dxModel.DXMainElement.Attribute.Type, dxModel.DXMainElement.Attribute.Type);

            mainItemDefinition.AddPropertyDefinitions(dxModel.DXMainElement.Item.Content.Children().Select(x => x as JProperty).Where(x => x != null).Select(x =>
                new DXPropertyDefinition(x.Name, new DXColumnAttribute(x.Name))).ToList());

            var singleItemDefinitions =
                dxModel.DXSingleElements.Select(x =>
                {
                    var item = new DXElementDefinition(x.Attribute.Type, x.Name);

                    var propertyNames = x.Item.Content.Children().Select(y => y as JProperty).Select(y => y.Name).ToList();

                    if (!propertyNames.Contains(Constants.ID))
                    {
                        propertyNames.Add(Constants.ID);
                    }

                    if (!propertyNames.Contains(Constants.DXUnitID))
                    {
                        propertyNames.Add(Constants.DXUnitID);
                    }

                    item.AddPropertyDefinitions(propertyNames.Select(y => new DXPropertyDefinition(y, new DXColumnAttribute(y))).ToList());

                    return item;
                }).ToList();

            var multiItemDefinitions =
                  dxModel.DXMultiElements.Select(x =>
                  {
                      var item = new DXElementDefinition(x.Attribute.Type, x.Name);

                      var existingElement = x.Announced.Count() > 0 ? x.Announced.First() : x.Deleted.Count() > 0 ? x.Deleted.First() : null;

                      if (existingElement == null)
                          return null;

                      var propertyNames = existingElement.Content.Children().Select(y => y as JProperty).Select(y => y.Name).ToList();

                      if (!propertyNames.Contains(Constants.ID))
                      {
                          propertyNames.Add(Constants.ID);
                      }

                      if (!propertyNames.Contains(Constants.DXUnitID))
                      {
                          propertyNames.Add(Constants.DXUnitID);
                      }

                      item.AddPropertyDefinitions(propertyNames.Select(y => new DXPropertyDefinition(y, new DXColumnAttribute(y))).ToList());

                      return item;
                  }).Where(x => x != null).ToList();


            var result = new DXModelDefinition(mainItemDefinition);
            result.AddToSingleItemDefinitions(singleItemDefinitions);
            result.AddToMultiItemDefinitions(multiItemDefinitions);

            return result;
        }

        public static DXModelDefinition Get(Type type)
        {
            var asqlTypeName = AttributeReader.GetDXUnitTypeName(type);

            var ownItem = Get(asqlTypeName, type);

            DXModelDefinition result = new DXModelDefinition(ownItem);

            var singleItemInfos = AttributeReader.GetSingleItemInfos(type);
            var multiItemInfos = AttributeReader.GetMultiItemInfos(type);

            var singleItemDefinitions = singleItemInfos.Select(x => Get(x.Name, x.PropertyType)).ToList();
            var mutliItemDefinitions = multiItemInfos.Select(x => Get(x.Name, x.PropertyType.GenericTypeArguments[0])).ToList();

            result.AddToSingleItemDefinitions(singleItemDefinitions);
            result.AddToMultiItemDefinitions(mutliItemDefinitions);

            return result;
        }

        public static DXElementDefinition Get(string type, Type dxElementType)
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

        public static DXElementDefinition Get(DXEnumDefinitionUnit enumDesc)
        {
            DXElementDefinition dxElementDefinition = new DXElementDefinition(enumDesc.DXObjectDefinitionMainElement.Name, enumDesc.DXObjectDefinitionMainElement.Name);

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