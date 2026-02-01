using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Converters.DXModelDefinitionConverters
{
    internal static class DXDataSetDefinitionConverter
    {
        public static DXDataSetDefinition ToDXModelDefinition<T>(DXUnitInheritance dxUnitHierarchy) where T : DXUnit
        {
            Type type = typeof(T);

            return ToDXModelDefinition(type, dxUnitHierarchy);
        }

        public static DXDataSetDefinition ToDXModelDefinition(this Type type, DXUnitInheritance dxUnitHierarchy)
        {
            var dxUnitTypeName = AttributeReader.GetDXUnitTypeName(type);

            var ownItem = DXMainTableDefinitionConverter.ToDXTableDefinition(dxUnitTypeName, type);

            DXDataSetDefinition result = new DXDataSetDefinition(ownItem);

            var singleItemInfos = AttributeReader.GetSingleItemInfos(type);
            var multiItemInfos = AttributeReader.GetMultiItemInfos(type);

            foreach (var dxUnitHierarchyItem in dxUnitHierarchy.Items)
            {
                dxUnitTypeName = dxUnitHierarchyItem.DXUnit.Name;

                foreach (var dxSingleElement in singleItemInfos)
                {
                    if (dxUnitHierarchyItem.ContainsSingleMandatory(dxSingleElement.Name) || dxUnitHierarchyItem.ContainsSingleOptional(dxSingleElement.Name))
                    {
                        var isReqruired = AttributeReader.GetAttribute<DXRequiredAttribute>(dxSingleElement);

                        var singleItemDefinition =
                            DXTableDefinitionConverter.ToDXTableDefinition(dxSingleElement.Name, dxUnitTypeName, dxSingleElement.PropertyType, isReqruired != null);

                        result.AddToSingleItemDefinitions(singleItemDefinition);
                    }
                }

                foreach (var dxMultiElement in multiItemInfos)
                {
                    if (dxUnitHierarchyItem.ContainsMultiMandatory(dxMultiElement.Name) || dxUnitHierarchyItem.ContainsMultiOptional(dxMultiElement.Name))
                    {
                        var isReqruired = AttributeReader.GetAttribute<DXRequiredAttribute>(dxMultiElement);

                        var mutliItemDefinition = DXTableDefinitionConverter.ToDXTableDefinition(dxMultiElement.Name, dxUnitTypeName, dxMultiElement.PropertyType.GenericTypeArguments[0], isReqruired != null);

                        // var mutliItemDefinition = dxMultiElement.ToDXElementDefinition(typeName);

                        if (mutliItemDefinition != null)
                        {
                            result.AddToMultiItemDefinitions(mutliItemDefinition);
                        }
                    }
                }
            }

            return result;
        }

        public static DXDataSetDefinition ToDXModelDefinition(this string dxUnitTypeName, DXUnitInheritance dxUnitHierarchy)
        {
            var topLevelItem = dxUnitHierarchy.Items.First();

            var dxUnitColumnNames = dxUnitHierarchy.Items.SelectMany(x => x.DXUnit.DXColumnDefinitionElement.Announced.Select(y => y.Name)).Distinct().ToHashSet();

            var ownItem = DXMainTableDefinitionConverter.ToDXTableDefinition(dxUnitTypeName, dxUnitColumnNames);

            DXDataSetDefinition result = new DXDataSetDefinition(ownItem);

            foreach (var dxUnitHierarchyItem in dxUnitHierarchy.Items)
            {
                var dxUnitTypeNameLocal = dxUnitHierarchyItem.DXUnit.Name;

                var dxUnit = dxUnitHierarchyItem.DXUnit;

                foreach (var item in dxUnitHierarchyItem.SingleMandatory)
                {
                    var singleItemDefinition =
                          DXTableDefinitionConverter.ToDXTableDefinition(
                              item.Name,
                              dxUnitTypeNameLocal,
                              item.DXColumnDefinitionElement.Announced.Select(x => x.Name).ToHashSet(),
                              true);

                    result.AddToSingleItemDefinitions(singleItemDefinition);
                }

                foreach (var item in dxUnitHierarchyItem.SingleOptional)
                {
                    var singleItemDefinition =
                          DXTableDefinitionConverter.ToDXTableDefinition(
                              item.Name,
                              dxUnitTypeNameLocal,
                              item.DXColumnDefinitionElement.Announced.Select(x => x.Name).ToHashSet(),
                              false);

                    result.AddToSingleItemDefinitions(singleItemDefinition);
                }

                foreach (var item in dxUnitHierarchyItem.MultiMandatory)
                {
                    var singleItemDefinition =
                          DXTableDefinitionConverter.ToDXTableDefinition(
                              item.Name,
                              dxUnitTypeNameLocal,
                              item.DXColumnDefinitionElement.Announced.Select(x => x.Name).ToHashSet(),
                              true);

                    result.AddToMultiItemDefinitions(singleItemDefinition);
                }


                foreach (var item in dxUnitHierarchyItem.MultiOptional)
                {
                    var singleItemDefinition =
                          DXTableDefinitionConverter.ToDXTableDefinition(
                              item.Name,
                              dxUnitTypeNameLocal,
                              item.DXColumnDefinitionElement.Announced.Select(x => x.Name).ToHashSet(),
                              false);

                    result.AddToMultiItemDefinitions(singleItemDefinition);
                }

            }

            return result;
        }
    }
}
