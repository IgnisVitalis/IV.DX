using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Converters.JObjectConverters;
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

            var ownItem = new DXMainElement(unitAttr)
            {
                Item = new DXItem
                {
                    ID = dxUnit.ID,
                    DXUnitID = dxUnit.ID,
                    Content = GetContent(dxUnit)
                }
            };

            return new DXModel(ownItem)
            {
                DXSingleElements = GetDXSingleElements(dxUnit),
                DXMultiElements = GetDXMultiElements(dxUnit)
            };
        }

        private static HashSet<DXSingleElement> GetDXSingleElements(DXUnit dxUnit)
        {
            var singleInfos = AttributeReader.GetSingleItemInfos(dxUnit);
            return singleInfos.Select(pi =>
            {
                var element = pi.GetValue(dxUnit) as DXElement;
                return element.ToDXSingleElement(pi.Name);
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
                            target.Add(new DXItem
                            {
                                ID = e.ID,
                                DXUnitID = dxUnit.ID,
                                Content = e.ToJObject()
                            });
                        }
                    }

                    Fill(Constants.Announced, announcedList);
                    Fill(Constants.Deleted, deletedList);
                }

                var multi = new DXMultiElement
                {
                    Attribute = elementInfo,
                    Name = pi.Name,
                    Mode = mode,
                    Announced = announcedList,
                    Deleted = deletedList
                };

                return multi;
            }).ToHashSet();
        }

        private static JObject? GetContent(DXUnit? obj)
        {
            if (obj is null) return null;

            var jObject = new JObject();
            jObject[Constants.SystemPropertyTypeName] = DXReflectionHelper.GetAttr<DXUnitAttribute>(obj.GetType()).Type;

            foreach (var prop in DXReflectionHelper.GetPropsWithAttribute<DXColumnAttribute>(obj.GetType()))
            {
                var value = prop.GetValue(obj);
                jObject[prop.Name] = new JValue(value);
            }
            return jObject;
        }

        public static DXModel? ToDXModel(this string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            var jObject = JObject.Parse(json);

            return ToDXModel(jObject);
        }

        public static DXModel? ToDXModel(this JObject? jObject)
        {
            if (jObject == null)
                return null;

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

            DXModel dxModel = new DXModel(ownItem)
            {
                DXSingleElements = singleItems,
                DXMultiElements = multiItems
            };

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

            var result = new DXMainElement(new DXUnitAttribute(type))
            {
                Item = GetDXItem(jObject, id)
            };

            result.Item.ID = id;
            result.Item.DXUnitID = id;

            return result;
        }

        static DXItem GetDXItem(JObject jObject, Guid? objId)
        {
            DXItem dxItem = new DXItem
            {
                ID = jObject[Constants.ID] != null ? (Guid?)jObject[Constants.ID] : null,
                DXUnitID = objId
            };

            var content = KeepScalarsOnly(jObject);

            dxItem.Content = content;

            return dxItem;
        }       

        static DXSingleElement GetDXSingleItem(JProperty property, Guid? objId)
        {
            DXSingleElement singleItem = new DXSingleElement()
            {
                Name = property.Name,
                Attribute = new DXElementAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name),
                Item = GetDXItem((JObject)property.Value, objId)
            };

            return singleItem;
        }

        static DXMultiElement GetDXMutliItem(JProperty property, Guid? objId)
        {
            DXMultiElement dxMultiItem = new DXMultiElement()
            {
                Name = property.Name,
                Attribute = new DXElementAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name),
                Announced = (property.Value[Constants.Announced] as JArray)?.Children().Select(x => GetDXItem((JObject)x, objId)).ToHashSet(),
                Deleted = (property.Value[Constants.Deleted] as JArray)?.Children().Select(x => GetDXItem((JObject)x, objId)).ToHashSet(),
                Mode = (MultiElementsMode)property.Value[Constants.Mode].Value<int>()
            };

            return dxMultiItem;
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