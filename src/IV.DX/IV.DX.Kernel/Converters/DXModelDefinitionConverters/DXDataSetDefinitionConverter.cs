using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXDataSetDefinitionConverter
    {
        public static DXDataSetDefinition ToDXModelDefinition<T>() where T : DXUnit
        {
            Type type = typeof(T);

            return ToDXModelDefinition(type);
        }

        public static DXDataSetDefinition ToDXModelDefinition(this DXModel dxModel, DXUnitInheritance dxUnitHierarchy)
        {
            var typeName = dxModel.DXMainElement.Attribute.Type;

            var mainItemDefinition = new DXMainTableDefinition(typeName, typeName, true);

            mainItemDefinition.AddPropertyDefinitions(dxModel.DXMainElement.Item.Content.Select(x =>
                new DXColumnDefinition(x.Key, new DXColumnAttribute(x.Key))).ToList());

            var result = new DXDataSetDefinition(mainItemDefinition);

            foreach (var dxUnitHierarchyItem in dxUnitHierarchy.Items)
            {
                typeName = dxUnitHierarchyItem.DXUnit.Name;

                foreach (var dxSingleElement in dxModel.DXSingleElements)
                {
                    if (dxUnitHierarchyItem.ContainsSingleMandatory(dxSingleElement.Name) || dxUnitHierarchyItem.ContainsSingleOptional(dxSingleElement.Name))
                    {
                        result.AddToSingleItemDefinitions(dxSingleElement.ToDXElementDefinition(typeName));
                    }
                }
                
                foreach (var dxMultiElement in dxModel.DXMultiElements)
                {
                    if (dxUnitHierarchyItem.ContainsMultiMandatory(dxMultiElement.Name) || dxUnitHierarchyItem.ContainsMultiOptional(dxMultiElement.Name))
                    {
                        result.AddToMultiItemDefinitions(dxMultiElement.ToDXElementDefinition(typeName));
                    }
                }            
            }

            return result;
        }

        public static DXDataSetDefinition ToDXModelDefinition(this DXModel dxModel, DXEnumDefinitionUnit dxEnum)
        {
            var typeName = dxModel.DXMainElement.Attribute.Type;

            var mainItemDefinition = new DXMainTableDefinition(typeName, typeName, true);

            mainItemDefinition.AddPropertyDefinitions(dxModel.DXMainElement.Item.Content.Select(x =>
                new DXColumnDefinition(x.Key, new DXColumnAttribute(x.Key))).ToList());

            var singleItemDefinitions =
                dxModel.DXSingleElements.Select(x => x.ToDXElementDefinition(typeName)).ToList();

            var multiItemDefinitions =
                  dxModel.DXMultiElements.Select(x => x.ToDXElementDefinition(typeName)).Where(x => x != null).ToList();

            var result = new DXDataSetDefinition(mainItemDefinition);
            result.AddToSingleItemDefinitions(singleItemDefinitions);
            result.AddToMultiItemDefinitions(multiItemDefinitions);

            return result;
        }

        public static DXDataSetDefinition ToDXModelDefinition(this Type type)
        {
            var dxUnitTypeName = AttributeReader.GetDXUnitTypeName(type);

            var ownItem = DXMainTableDefinitionConverter.ToDXTableDefinition(dxUnitTypeName, type, true);

            DXDataSetDefinition result = new DXDataSetDefinition(ownItem);

            var singleItemInfos = AttributeReader.GetSingleItemInfos(type);
            var multiItemInfos = AttributeReader.GetMultiItemInfos(type);

            var singleItemDefinitions = singleItemInfos.Select(x =>
            {
                var isReqruired = AttributeReader.GetAttribute<DXRequiredAttribute>(x);

                return DXTableDefinitionConverter.ToDXTableDefinition(dxUnitTypeName, x.Name, x.PropertyType, isReqruired != null);
            }).ToList();

            var mutliItemDefinitions = multiItemInfos.Select(x =>
            {
                var isReqruired = AttributeReader.GetAttribute<DXRequiredAttribute>(x);

                return DXTableDefinitionConverter.ToDXTableDefinition(dxUnitTypeName, x.Name, x.PropertyType.GenericTypeArguments[0], isReqruired != null);
            }).ToList();


            result.AddToSingleItemDefinitions(singleItemDefinitions);
            result.AddToMultiItemDefinitions(mutliItemDefinitions);

            return result;
        }
    }
}