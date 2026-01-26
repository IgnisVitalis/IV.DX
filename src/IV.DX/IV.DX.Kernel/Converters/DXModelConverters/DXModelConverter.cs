using System.Linq;
using System.Xml.Linq;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXModelConverter
    {
        public static DXModel? ToDXModel(this DXUnit? dxUnit)
        {
            if (dxUnit is null) return null;

            var unitAttr = DXReflectionHelper.GetAttr<DXUnitAttribute>(dxUnit.GetType());
            var item = new DXItem(unitAttr.Type, dxUnit.ID, dxUnit.ID, dxUnit.TimeStamp, GetContent(dxUnit));

            var ownItem = new DXMainElement(unitAttr, item);

            var dxSingleElements = GetDXSingleElements(dxUnit);
            var dxMultiElements = GetDXMultiElements(dxUnit);

            return new DXModel(ownItem, dxSingleElements, dxMultiElements);
        }

        private static HashSet<DXSingleElement> GetDXSingleElements(DXUnit dxUnit)
        {
            var singleInfos = AttributeReader.GetSingleItemInfos(dxUnit);
            return singleInfos.Select(pi =>
            {
                var element = pi.GetValue(dxUnit) as DXElement;

                var isRequiredAttr = AttributeReader.GetAttribute<DXRequiredAttribute>(pi.PropertyType);

                return element.ToDXSingleElement(isRequiredAttr != null, pi.Name);
            }).ToHashSet();
        }

        private static HashSet<DXMultiElement> GetDXMultiElements(DXUnit dxUnit)
        {
            var multiInfos = AttributeReader.GetMultiItemInfos(dxUnit);

            return multiInfos.Select(pi =>
            {
                var value = pi.GetValue(dxUnit);
                var multiType = pi.PropertyType;

                var mode = value is null
                    ? MultiElementsMode.Full
                    : (MultiElementsMode)(multiType.GetProperty("Mode")?.GetValue(value) ?? (int)MultiElementsMode.Full);

                var elementType = pi.PropertyType.GenericTypeArguments[0];
                var elementInfo = DXReflectionHelper.GetAttr<DXElementAttribute>(elementType);
                //var isRequiredAttr = DXReflectionHelper.GetAttr<DXRequiredAttribute>(elementType);

                var announcedList = new HashSet<DXItem>();
                var deletedList = new HashSet<DXItem>();

                if (value != null)
                {
                    void Fill(string propertyName, HashSet<DXItem> target)
                    {
                        var src = multiType.GetProperty(propertyName)?.GetValue(value) as IEnumerable<DXElement>;
                        if (src == null) return;

                        foreach (var e in src)
                        {
                            target.Add(new DXItem(elementInfo.Type, e.ID, dxUnit.ID, e.TimeStamp, e.ToDictionary()));
                        }
                    }

                    Fill(Constants.Announced, announcedList);
                    Fill(Constants.Deleted, deletedList);
                }

                if (mode == MultiElementsMode.Full)
                    return DXMultiElement.CreateForFullMode(pi.Name, elementInfo, announcedList);
                else
                    return DXMultiElement.CreateForTargetMode(pi.Name, elementInfo, announcedList, deletedList);

                    //var multi = DXMultiElement. new DXMultiElement(pi.Name, elementInfo, mode, announcedList, deletedList, isRequiredAttr != null);

                //return multi;
            }).ToHashSet();
        }

        private static IDictionary<string, object>? GetContent(DXUnit? obj)
        {
            if (obj is null) return null;

            var dict = new Dictionary<string, object>();
            dict[Constants.SystemPropertyTypeName] = DXReflectionHelper.GetAttr<DXUnitAttribute>(obj.GetType()).Type;

            foreach (var prop in DXReflectionHelper.GetPropsWithAttribute<DXColumnAttribute>(obj.GetType()))
            {
                var value = prop.GetValue(obj);
                dict[prop.Name] = value;
            }

            return dict;
        }

        public static DXModel ToDXModel(this string json)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(json);

            var jObject = JObject.Parse(json);

            return ToDXModel(jObject);
        }

        public static DXModel ToDXModel(this JObject jObject)
        {
            ArgumentNullException.ThrowIfNull(jObject);

            var jProperties = jObject.Properties()
                    .Where(x => x.Value.Type == JTokenType.Object).ToList();

            var expressionToFilterMultiItems = new Func<JProperty, bool>((x) =>
            {
                return x.Value[Constants.Mode] != null && (x.Value[Constants.Announced] != null || x.Value[Constants.Deleted] != null);
            });

            var jObjectCopy = jObject.DeepClone() as JObject;

            var ownItem = GetOwnDXSingleItem(jObjectCopy);

            var singleItems = jProperties
                    .Where(x => !expressionToFilterMultiItems(x))
                    .Select(x => GetDXSingleItem(x, ownItem.Item.ID))
                    .ToHashSet();

            var multiItems = jProperties
                    .Where(x => expressionToFilterMultiItems(x))
                    .Select(x => GetDXMutliItem(x, ownItem.Item.ID))
                    .ToHashSet();

            var dxSingleElements = singleItems;
            var dxMultiElements = multiItems;

            DXModel dxModel = new DXModel(ownItem, dxSingleElements, dxMultiElements);

            return dxModel;
        }

        static DXMainElement GetOwnDXSingleItem(JObject jObject)
        {
            if (jObject[Constants.ID] == null)
                throw new Exception($"JSON for DXMainElement should contain {Constants.ID} property");

            if (jObject[Constants.SystemPropertyTypeName] == null)
                throw new Exception($"JSON for DXMainElement should contain {Constants.SystemPropertyTypeName} property");

            if (jObject[Constants.TimeStamp] == null)
                throw new Exception($"JSON for DXMainElement should contain {Constants.TimeStamp} property");

            Guid id;

            if (jObject[Constants.ID].Type == JTokenType.Guid)
            {
                id = jObject.Value<Guid>(Constants.ID);
            }
            else
            {
                id = Guid.Parse(jObject.Value<string>(Constants.ID));
            }

            var type = jObject.Value<string>(Constants.SystemPropertyTypeName);
            var item = GetDXItem(jObject, id);
            var result = new DXMainElement(new DXUnitAttribute(type), item);

            return result;
        }

        static DXItem GetDXItem(JObject jObject, Guid objId)
        {
            var content = KeepScalarsOnly(jObject);

            DXItem dxItem = new DXItem(
                (string)jObject[Constants.SystemPropertyTypeName],
                (Guid)jObject[Constants.ID],
                objId,
                (DateTime)jObject[Constants.TimeStamp],
                content.ToDictionary());

            return dxItem;
        }

        static DXSingleElement GetDXSingleItem(JProperty property, Guid objId)
        {
            var name = property.Name;
            var attribute = new DXElementAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name);

            var jObject = (JObject)property.Value;

            bool isRequired = false;

            if (jObject.ContainsKey(Constants.SystemPropertyIsRequired))
            {
                isRequired = jObject.Value<bool>(Constants.SystemPropertyIsRequired);
            }

            var item = GetDXItem(jObject, objId);

            DXSingleElement singleItem = new DXSingleElement(name, attribute, item, isRequired);

            return singleItem;
        }

        static DXMultiElement GetDXMutliItem(JProperty property, Guid objId)
        {
            var isRequired = JHelper.GetValue<bool?>(property.Value as JObject, Constants.SystemPropertyIsRequired) ?? false;

            var mode = (MultiElementsMode)property.Value[Constants.Mode].Value<int>();
            var attribute = new DXElementAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name);

            if (mode == MultiElementsMode.Full)
            {
                var announced = (property.Value[Constants.Announced] as JArray)?.Children().Select(x => GetDXItem((JObject)x, objId)).ToHashSet();

                return DXMultiElement.CreateForFullMode(property.Name, attribute, announced);
            }
            else
            {
                var announced = (property.Value[Constants.Announced] as JArray)?.Children().Select(x => GetDXItem((JObject)x, objId)).ToHashSet();
                var deleted = (property.Value[Constants.Deleted] as JArray)?.Children().Select(x => GetDXItem((JObject)x, objId)).ToHashSet();

                return DXMultiElement.CreateForTargetMode(property.Name, attribute, announced, deleted);
            }
        }

        static JObject KeepScalarsOnly(JObject src)
        {
            if (src is null) return new JObject();

            var dst = new JObject();

            foreach (var prop in src.Properties())
            {
                if (prop.Value is JValue v)
                    dst.Add(prop.Name, v.DeepClone());
            }

            return dst;
        }
    }
}