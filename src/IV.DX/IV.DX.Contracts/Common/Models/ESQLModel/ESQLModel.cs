using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using Newtonsoft.Json.Linq;

namespace IV.DX.Contracts.Common.Models
{
    public class ESQLModel
    {
        public ESQLMainItem OwnSingleItem { get; set; }
        public IEnumerable<ESQLSingleItem> SingleItems { get; set; }
        public IEnumerable<ESQLMultiItem> MultiItems { get; set; }

        public ESQLModel(ESQLMainItem ownSingleItem)
        {
            this.OwnSingleItem = ownSingleItem;
        }

        public static bool DeepEquals(ESQLModel item1, ESQLModel item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = true;

            result = result
                && ESQLMainItem.DeepEquals(item1.OwnSingleItem, item2.OwnSingleItem)
                && ESQLSingleItem.DeepEquals(item1.SingleItems, item2.SingleItems)
                && ESQLMultiItem.DeepEquals(item1.MultiItems, item2.MultiItems);

            return result;
        }

        public ESQLModel DeepClone()
        {
            var ownItemClone = this.OwnSingleItem.DeepClone();

            return new ESQLModel(ownItemClone)
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
        public static ESQLModel CreateInstance(JObject jObject)
        {
            if (jObject == null)
                return null;

            var jProperties = jObject.Properties()
                    .Where(x => x.Value.Type == JTokenType.Object).ToList();

            var expressionToFilterMultiItems = new Func<JProperty, bool>((x) =>
            {
                return x.Value[Constants.Mode] != null && (x.Value[Constants.Announced] != null || x.Value[Constants.Deleted] != null);
            });

            var ownItem = GetQwnESQLSingleItem(jObject);

            var singleItems = jProperties
                    .Where(x => !expressionToFilterMultiItems(x))
                    .Select(x => GetESQLSingleItem(x, ownItem.Item.ID))
                    .ToList();

            var multiItems = jProperties
                    .Where(x => expressionToFilterMultiItems(x))
                    .Select(x => GetESQLMutliItem(x, ownItem.Item.ID))
                    .ToList();

            ESQLModel model = new ESQLModel(ownItem)
            {
                SingleItems = singleItems,
                MultiItems = multiItems
            };

            return model;
        }

        public static ESQLModel CreateInstance(string json)
        {
            var jObject = JObject.Parse(json);

            return CreateInstance(jObject);
        }

        private static ESQLSingleItem GetESQLSingleItem(JProperty property, Guid? objId)
        {
            ESQLSingleItem singleItem = new ESQLSingleItem()
            {
                Name = property.Name,
                BlockInfo = new ESQLBlockDefinitionAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name),
                Item = GetESQLItem((JObject)property.Value, objId)
            };

            return singleItem;
        }

        private static ESQLMultiItem GetESQLMutliItem(JProperty property, Guid? objId)
        {
            ESQLMultiItem esqlMultiItem = new ESQLMultiItem()
            {
                Name = property.Name,
                BlockInfo = new ESQLBlockDefinitionAttribute(property.Value[Constants.SystemPropertyTypeName] != null ? property.Value[Constants.SystemPropertyTypeName].Value<string>() : property.Name),
                Announced = (property.Value[Constants.Announced] as JArray)?.Children().Select(x => GetESQLItem((JObject)x, objId)).ToList(),
                Deleted = (property.Value[Constants.Deleted] as JArray)?.Children().Select(x => GetESQLItem((JObject)x, objId)).ToList(),
                Mode = (ModeForMultiItems)property.Value[Constants.Mode].Value<int>()
            };

            return esqlMultiItem;
        }

        private static ESQLMainItem GetQwnESQLSingleItem(JObject jObject)
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

            var result = new ESQLMainItem(new ESQLObjectDefinitionAttribute(type))
            {
                Item = GetESQLItem(jObjectCopy, id)
            };

            result.Item.ID = id;
            result.Item.ObjectID = id;

            return result;
        }

        private static ESQLItem GetESQLItem(JObject jObject, Guid? objId)
        {
            ESQLItem esqlItem = new ESQLItem
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

            esqlItem.Content = content;

            return esqlItem;
        }
        #endregion
    }
}