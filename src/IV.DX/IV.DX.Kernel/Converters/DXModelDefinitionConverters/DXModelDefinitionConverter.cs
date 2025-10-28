using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXModelDefinitionConverter
    {
        public static DXModelDefinition ToDXModelDefinition<T>() where T : DXUnit
        {
            Type type = typeof(T);

            return ToDXModelDefinition(type);
        }

        public static DXModelDefinition ToDXModelDefinition(this DXModel dxModel)
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

        public static DXModelDefinition ToDXModelDefinition(this Type type)
        {
            var asqlTypeName = AttributeReader.GetDXUnitTypeName(type);

            var ownItem = DXElementDefinitionConverter.ToDXElementDefinition(asqlTypeName, type);

            DXModelDefinition result = new DXModelDefinition(ownItem);

            var singleItemInfos = AttributeReader.GetSingleItemInfos(type);
            var multiItemInfos = AttributeReader.GetMultiItemInfos(type);

            var singleItemDefinitions = singleItemInfos.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x.Name, x.PropertyType)).ToList();
            var mutliItemDefinitions = multiItemInfos.Select(x => DXElementDefinitionConverter.ToDXElementDefinition(x.Name, x.PropertyType.GenericTypeArguments[0])).ToList();

            result.AddToSingleItemDefinitions(singleItemDefinitions);
            result.AddToMultiItemDefinitions(mutliItemDefinitions);

            return result;
        }
    }
}