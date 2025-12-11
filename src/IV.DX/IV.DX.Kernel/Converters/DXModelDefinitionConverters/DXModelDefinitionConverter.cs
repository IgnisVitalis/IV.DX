using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

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
            var mainItemDefinition = new DXElementDefinition(dxModel.DXMainElement.Attribute.Type, dxModel.DXMainElement.Attribute.Type, true);

            mainItemDefinition.AddPropertyDefinitions(dxModel.DXMainElement.Item.Content.Select(x =>
                new DXPropertyDefinition(x.Key, new DXColumnAttribute(x.Key))).ToList());

            var singleItemDefinitions =
                dxModel.DXSingleElements.Select(x => x.ToDXElementDefinition()).ToList();

            var multiItemDefinitions =
                  dxModel.DXMultiElements.Select(x => x.ToDXElementDefinition()).Where(x => x != null).ToList();

            var result = new DXModelDefinition(mainItemDefinition);
            result.AddToSingleItemDefinitions(singleItemDefinitions);
            result.AddToMultiItemDefinitions(multiItemDefinitions);

            return result;
        }

        public static DXModelDefinition ToDXModelDefinition(this Type type)
        {
            var asqlTypeName = AttributeReader.GetDXUnitTypeName(type);

            var ownItem = DXElementDefinitionConverter.ToDXElementDefinition(asqlTypeName, type, true);

            DXModelDefinition result = new DXModelDefinition(ownItem);

            var singleItemInfos = AttributeReader.GetSingleItemInfos(type);
            var multiItemInfos = AttributeReader.GetMultiItemInfos(type);

            var singleItemDefinitions = singleItemInfos.Select(x =>
            {
                var isReqruired = AttributeReader.GetAttribute<DXRequiredAttribute>(x);

                return DXElementDefinitionConverter.ToDXElementDefinition(x.Name, x.PropertyType, isReqruired != null);
            }).ToList();

            var mutliItemDefinitions = multiItemInfos.Select(x =>
            {
                var isReqruired = AttributeReader.GetAttribute<DXRequiredAttribute>(x);

                return DXElementDefinitionConverter.ToDXElementDefinition(x.Name, x.PropertyType.GenericTypeArguments[0], isReqruired != null);
            }).ToList();


            result.AddToSingleItemDefinitions(singleItemDefinitions);
            result.AddToMultiItemDefinitions(mutliItemDefinitions);

            return result;
        }
    }
}