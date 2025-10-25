using IV.DX.Kernel.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class DXModel
    {
        public DXMainElement MainElement { get; set; }
        public HashSet<DXSingleElement> DXSingleElements { get; set; }
        public HashSet<DXMultiElement> DXMultiElements { get; set; }

        public DXModel(DXMainElement mainElement)
        {
            this.MainElement = mainElement;
        }

        public static bool DeepEquals(DXModel item1, DXModel item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = true;

            result = result
                && DXMainElement.DeepEquals(item1.MainElement, item2.MainElement)
                && DXSingleElement.DeepEquals(item1.DXSingleElements, item2.DXSingleElements)
                && DXMultiElement.DeepEquals(item1.DXMultiElements, item2.DXMultiElements);

            return result;
        }

        public DXModel DeepClone()
        {
            var ownItemClone = this.MainElement.DeepClone();

            return new DXModel(ownItemClone)
            {
                DXSingleElements = this.DXSingleElements?.Select(x => x.DeepClone()).ToHashSet(),
                DXMultiElements = this.DXMultiElements?.Select(x => x.DeepClone()).ToHashSet()
            };
        }

        #region Convert to JObject
        public JObject ConvertToJObject()
        {
            JObject result = new JObject
            {
                { Constants.SystemPropertyTypeName, this.MainElement.ObjectInfo.ObjectName },
                { Constants.ID, this.MainElement.Item.ID }
            };

            if (this.DXSingleElements != null)
            {
                foreach (var item in this.DXSingleElements)
                {
                    result.Add(item.ConvertToJProperty());
                }
            }

            if (this.DXMultiElements != null)
            {
                foreach (var item in this.DXMultiElements)
                {
                    result.Add(item.ConvertToJProperty());
                }
            }

            return result;
        }
        #endregion

        #region Create instance
        public static DXModel CreateInstance(JObject jObject)
        {
            if (jObject == null)
                return null;

            var jProperties = jObject.Properties()
                    .Where(x => x.Value.Type == JTokenType.Object).ToList();

            var expressionToFilterMultiItems = new Func<JProperty, bool>((x) =>
            {
                return x.Value[Constants.Mode] != null && (x.Value[Constants.Announced] != null || x.Value[Constants.Deleted] != null);
            });

            var ownItem = GetOwnDXSingleItem(jObject);

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

        public static DXModel CreateInstance(string json)
        {
            var jObject = JObject.Parse(json);

            return CreateInstance(jObject);
        }

        private static DXSingleElement GetDXSingleItem(JProperty property, Guid? objId)
        {
            DXSingleElement singleItem = new DXSingleElement()
            {
                Name = property.Name,
                ElementInfo = new DXElementAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name),
                Item = GetdxItem((JObject)property.Value, objId)
            };

            return singleItem;
        }

        private static DXMultiElement GetDXMutliItem(JProperty property, Guid? objId)
        {
            DXMultiElement dxMultiItem = new DXMultiElement()
            {
                Name = property.Name,
                DXElementInfo = new DXElementAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name),
                Announced = (property.Value[Constants.Announced] as JArray)?.Children().Select(x => GetdxItem((JObject)x, objId)).ToHashSet(),
                Deleted = (property.Value[Constants.Deleted] as JArray)?.Children().Select(x => GetdxItem((JObject)x, objId)).ToHashSet(),
                Mode = (MultiElementsMode)property.Value[Constants.Mode].Value<int>()
            };

            return dxMultiItem;
        }

        private static DXMainElement GetOwnDXSingleItem(JObject jObject)
        {
            var jObjectCopy = jObject.DeepClone() as JObject;
            string type = null;

            Guid? id = null;

            if (jObjectCopy[Constants.ID] != null)
            {
                id = (Guid?)jObjectCopy[Constants.ID];

                jObjectCopy.Remove(Constants.ID);
            }

            if (jObjectCopy[Constants.SystemPropertyTypeName] != null)
            {
                type = (string)jObjectCopy[Constants.SystemPropertyTypeName];
                jObjectCopy.Remove(Constants.SystemPropertyTypeName);
            }

            foreach (var item in jObjectCopy.Properties()
                    .Where(x => x.Value.Type == JTokenType.Object).ToList())
            {
                jObjectCopy.Remove(item.Name);
            }

            var result = new DXMainElement(new DXUnitAttribute(type))
            {
                Item = GetdxItem(jObjectCopy, id)
            };

            result.Item.ID = id;
            result.Item.DXUnitID = id;

            return result;
        }

        private static DXItem GetdxItem(JObject jObject, Guid? objId)
        {
            DXItem dxItem = new DXItem
            {
                ID = jObject[Constants.ID] != null ? (Guid?)jObject[Constants.ID] : null,
                DXUnitID = objId
            };

            var content = jObject.DeepClone() as JObject;

            if (content[Constants.ID] != null)
            {
                content.Remove(Constants.ID);
            }

            if (content[Constants.DXUnitID] != null)
            {
                content.Remove(Constants.DXUnitID);
            }

            dxItem.Content = content;

            return dxItem;
        }
        #endregion
    }
}