using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXModelConverter
    {
        public static JObject ConvertToJObject(this DXModel dxModel)
        {
            JObject result = dxModel.DXMainElement.Item.Content.DeepClone() as JObject;

            if (dxModel.DXSingleElements != null)
            {
                foreach (var item in dxModel.DXSingleElements)
                {
                    result.Add(item.ConvertToJProperty());
                }
            }

            if (dxModel.DXMultiElements != null)
            {
                foreach (var item in dxModel.DXMultiElements)
                {
                    result.Add(item.ConvertToJProperty());
                }
            }

            return result;
        }

        public static DXModel Parse(string json)
        {
            var jObject = JObject.Parse(json);

            return Parse(jObject);
        }

        public static DXModel Parse(JObject jObject)
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
                Item = GetdxItem(jObject, id)
            };

            result.Item.ID = id;
            result.Item.DXUnitID = id;

            return result;
        }

        static DXItem GetdxItem(JObject jObject, Guid? objId)
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
                Item = GetdxItem((JObject)property.Value, objId)
            };

            return singleItem;
        }

        static DXMultiElement GetDXMutliItem(JProperty property, Guid? objId)
        {
            DXMultiElement dxMultiItem = new DXMultiElement()
            {
                Name = property.Name,
                Attribute = new DXElementAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name),
                Announced = (property.Value[Constants.Announced] as JArray)?.Children().Select(x => GetdxItem((JObject)x, objId)).ToHashSet(),
                Deleted = (property.Value[Constants.Deleted] as JArray)?.Children().Select(x => GetdxItem((JObject)x, objId)).ToHashSet(),
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
