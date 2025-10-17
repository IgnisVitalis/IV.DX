using IV.DX.Kernel.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Models
{
    public class DXModel
    {
        public DXMainItem OwnSingleItem { get; set; }
        public IEnumerable<DXSingleElement> SingleItems { get; set; }
        public IEnumerable<DXMultiElement> MultiItems { get; set; }

        public DXModel(DXMainItem ownSingleItem)
        {
            this.OwnSingleItem = ownSingleItem;
        }

        public static bool DeepEquals(DXModel item1, DXModel item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = true;

            result = result
                && DXMainItem.DeepEquals(item1.OwnSingleItem, item2.OwnSingleItem)
                && DXSingleElement.DeepEquals(item1.SingleItems, item2.SingleItems)
                && DXMultiElement.DeepEquals(item1.MultiItems, item2.MultiItems);

            return result;
        }

        public DXModel DeepClone()
        {
            var ownItemClone = this.OwnSingleItem.DeepClone();

            return new DXModel(ownItemClone)
            {
                SingleItems = this.SingleItems?.Select(x => x.DeepClone()).ToList(),
                MultiItems = this.MultiItems?.Select(x => x.DeepClone()).ToList()
            };
        }

        #region Convert to JObject
        public JObject ConvertToJObject()
        {
            JObject result = new JObject
            {
                { Constants.SystemPropertyTypeName, this.OwnSingleItem.ObjectInfo.ObjectName },
                { Constants.ID, this.OwnSingleItem.Item.ID }
            };

            if (this.SingleItems != null)
            {
                foreach (var item in this.SingleItems)
                {
                    result.Add(item.ConvertToJProperty());
                }
            }

            if (this.MultiItems != null)
            {
                foreach (var item in this.MultiItems)
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

            var ownItem = GetQwndxSingleItem(jObject);

            var singleItems = jProperties
                    .Where(x => !expressionToFilterMultiItems(x))
                    .Select(x => GetDXSingleItem(x, ownItem.Item.ID))
                    .ToList();

            var multiItems = jProperties
                    .Where(x => expressionToFilterMultiItems(x))
                    .Select(x => GetDXMutliItem(x, ownItem.Item.ID))
                    .ToList();

            DXModel dxModel = new DXModel(ownItem)
            {
                SingleItems = singleItems,
                MultiItems = multiItems
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
                Announced = (property.Value[Constants.Announced] as JArray)?.Children().Select(x => GetdxItem((JObject)x, objId)).ToList(),
                Deleted = (property.Value[Constants.Deleted] as JArray)?.Children().Select(x => GetdxItem((JObject)x, objId)).ToList(),
                Mode = (MultiElementsMode)property.Value[Constants.Mode].Value<int>()
            };

            return dxMultiItem;
        }

        private static DXMainItem GetQwndxSingleItem(JObject jObject)
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

            var result = new DXMainItem(new DXUnitAttribute(type))
            {
                Item = GetdxItem(jObjectCopy, id)
            };

            result.Item.ID = id;
            result.Item.ObjectID = id;

            return result;
        }

        private static DXItem GetdxItem(JObject jObject, Guid? objId)
        {
            DXItem dxItem = new DXItem
            {
                ID = jObject[Constants.ID] != null ? (Guid?)jObject[Constants.ID] : null,
                ObjectID = objId
            };

            var content = jObject.DeepClone() as JObject;

            if (content[Constants.ID] != null)
            {
                content.Remove(Constants.ID);
            }

            if (content[Constants.ObjectID] != null)
            {
                content.Remove(Constants.ObjectID);
            }

            dxItem.Content = content;

            return dxItem;
        }
        #endregion
    }
}