using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters
{
    public static class ESQLObjectHelper
    {
        #region Convert to JObject       
        public static JObject ConvertToJObject(this ESQLObject esqlObject)
        {
            var result = esqlObject.ConvertToESQLModel().ConvertToJObject();

            return result;
        }

        public static string ConvertToString(this ESQLObject esqlObject)
        {
            var jObject = esqlObject.ConvertToJObject();
            var str = jObject.ToString();

            return str;
        }
        #endregion

        #region Convert to ESQLModel
        public static ESQLModel ConvertToESQLModel(this ESQLObject esqlObject)
        {
            var objectInfo = AttributeReader.GetSingleAttribute<ESQLObjectDefinitionAttribute>
                   (esqlObject.GetType());

            var ownItem = new ESQLMainItem(objectInfo)
            {
                Item = new ESQLItem()
                {
                    ID = esqlObject.ID,
                    ObjectID = esqlObject.ID,
                    Content = GetContent(esqlObject)
                }
            };

            ESQLModel model = new ESQLModel(ownItem)
            {
                SingleItems = GetESQLSingleItems(esqlObject),
                MultiItems = GetESQLMutliItems(esqlObject)
            };

            return model;
        }

        private static IEnumerable<ESQLSingleItem> GetESQLSingleItems(ESQLObject esqlObject)
        {
            var singleItemInfos = AttributeReader.GetSingleItemInfos(esqlObject);

            var result = singleItemInfos.Select(x =>
            {
                var singleItem = x.GetValue(esqlObject) as ESQLBlock;

                ESQLSingleItem esqlSingleItem = new ESQLSingleItem()
                {
                    BlockInfo = AttributeReader.GetSingleAttribute<ESQLBlockDefinitionAttribute>(x.PropertyType),
                    Item = new ESQLItem()
                    {
                        ID = singleItem?.ID,
                        ObjectID = esqlObject.ID,
                        Content = GetContent(singleItem),
                    },
                    Name = x.Name
                };

                return esqlSingleItem;
            }).ToList();

            return result;
        }

        public static ESQLSingleItem ConvertToSingleItem(this ESQLBlock block)
        {
            var blockInfo = AttributeReader.GetSingleAttribute<ESQLBlockDefinitionAttribute>(block.GetType());

            ESQLSingleItem singleItem = new ESQLSingleItem()
            {
                BlockInfo = blockInfo,
                Item = new ESQLItem()
                {
                    ID = block.ID,
                    ObjectID = block.ObjectID,
                    Content = GetContent(block)
                },
                Name = blockInfo.BlockName
            };
            return singleItem;
        }

        private static IEnumerable<ESQLMultiItem> GetESQLMutliItems(ESQLObject esqlObject)
        {
            var multiItemsInfos = AttributeReader.GetMultiItemInfos(esqlObject);

            var result = multiItemsInfos.Select(x =>
            {
                var multiItemType = x.PropertyType;
                var multiItemValue = x.GetValue(esqlObject);

                ModeForMultiItems mode = ModeForMultiItems.Full;

                if (multiItemValue != null)
                    mode = (ModeForMultiItems)multiItemType.GetProperty("Mode").GetValue(multiItemValue);

                ESQLMultiItem multiItem = new ESQLMultiItem()
                {
                    BlockInfo = AttributeReader.GetSingleAttribute<ESQLBlockDefinitionAttribute>(x.PropertyType.GenericTypeArguments[0]),
                    Name = x.Name,
                    Mode = mode
                };

                if (multiItemValue != null)
                {
                    var announcedArray = multiItemType.GetProperty(Constants.Announced).GetValue(multiItemValue) as IEnumerable<ESQLBlock>;

                    if (announcedArray != null)
                    {
                        multiItem.Announced = announcedArray.Select(y =>
                        {
                            var content = GetContent(y);

                            var esqlItem = new ESQLItem()
                            {
                                ID = y.ID,
                                ObjectID = esqlObject.ID,
                                Content = content
                            };

                            return esqlItem;
                        }).ToList();
                    }
                    else
                    {
                        multiItem.Announced = new List<ESQLItem>();
                    }

                    var destroyedArray = multiItemType.GetProperty(Constants.Deleted).GetValue(multiItemValue) as IEnumerable<ESQLBlock>;

                    if (destroyedArray != null)
                    {
                        multiItem.Deleted = destroyedArray.Select(y =>
                        {
                            var content = GetContent(y);

                            var esqlItem = new ESQLItem()
                            {
                                ID = y.ID,
                                ObjectID = esqlObject.ID,
                                Content = content
                            };

                            return esqlItem;
                        }).ToList();
                    }
                    else
                    {
                        multiItem.Deleted = new List<ESQLItem>();
                    }
                }

                return multiItem;
            }).ToList();

            return result;
        }

        private static JObject GetContent(ESQLBlock block)
        {
            if (block == null)
                return null;

            JObject jObject = new JObject();

            var properties = block.GetType().GetProperties()
                .Where(x => AttributeReader.GetSinglePropertyAttribute<ESQLColumnDefinitionAttribute>(x) != null)
                .ToList();

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetSinglePropertyAttribute<ESQLColumnDefinitionAttribute>(property);

                jObject[property.Name] = new JValue(property.GetValue(block));
            }

            return jObject;
        }

        private static JObject GetContent(ESQLObject obj)
        {
            if (obj == null)
                return null;

            JObject jObject = new JObject();

            var properties = obj.GetType().GetProperties()
                .Where(x => AttributeReader.GetSinglePropertyAttribute<ESQLColumnDefinitionAttribute>(x) != null)
                .ToList();

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetSinglePropertyAttribute<ESQLColumnDefinitionAttribute>(property);

                jObject[property.Name] = new JValue(property.GetValue(obj));
            }

            return jObject;
        }
        #endregion

        #region Create instance
        public static T CreateInstance<T>(string json) where T : ESQLObject
        {
            var esqlModel = ESQLModel.CreateInstance(json);

            T esqlObj = CreateInstance<T>(esqlModel);

            return esqlObj;
        }

        public static IEnumerable<T> CreateInstances<T>(string json) where T : ESQLObject
        {
            var jArray = JArray.Parse(json);

            return CreateInstances<T>(jArray);
        }

        public static IEnumerable<T> CreateInstances<T>(JArray jArray) where T : ESQLObject
        {
            foreach (JObject jObject in jArray)
            {
                yield return CreateInstance<T>(jObject);
            }
        }

        public static T CreateInstance<T>(JObject jObject) where T : ESQLObject
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);

            T esqlObj = CreateInstance<T>(esqlModel);

            return esqlObj;
        }

        public static ESQLObject CreateInstance(string json, Type type)
        {
            var esqlModel = ESQLModel.CreateInstance(json);

            ESQLObject esqlObj = CreateInstance(esqlModel, type);

            return esqlObj;
        }

        public static ESQLObject CreateInstance(JObject jObject, Type type)
        {
            var esqlModel = ESQLModel.CreateInstance(jObject);

            ESQLObject esqlObj = CreateInstance(esqlModel, type);

            return esqlObj;
        }

        public static T CreateInstance<T>(ESQLModel model) where T : ESQLObject
        {
            return ConvertToESQLObject(model, typeof(T)) as T;
        }

        public static ESQLObject CreateInstance(ESQLModel model, Type type)
        {
            return ConvertToESQLObject(model, type);
        }

        public static T CreateBlockInstance<T>(ESQLSingleItem item) where T : ESQLBlock
        {
            if (item == null)
                return null;

            var singleItemName = item.Name;
            var asqlModelSingleItem = singleItemName;

            var jProperty = item.ConvertToJPropertyWithoutSystemProperties();

            var singleFragmetInstance = jProperty.Value.ToObject(typeof(T));

            return singleFragmetInstance as T;
        }

        private static ESQLObject ConvertToESQLObject(ESQLModel model, Type type)
        {
            if (model == null)
                return null;

            if (type == null)
                return null;

            var obj = model.OwnSingleItem.ConvertToJPropertyWithoutSystemProperties().Value.ToObject(type);

            var objIdProperty = type.GetProperty(Constants.ID);
            objIdProperty.SetValue(obj, model.OwnSingleItem.Item.ID);

            var singleItemProperties = AttributeReader.GetSingleItemInfos(type);

            if (model.SingleItems != null)
            {
                foreach (var singleItemProperty in singleItemProperties)
                {
                    var singleItemName = singleItemProperty.Name;
                    var asqlModelSingleItem = model.SingleItems.SingleOrDefault(x => x.Name == singleItemName);

                    if (asqlModelSingleItem == null)
                    {
                        continue;
                    }

                    var singleItemPropertyType = singleItemProperty.PropertyType;

                    var jProperty = asqlModelSingleItem.ConvertToJPropertyWithoutSystemProperties();

                    if (jProperty == null)
                        continue;

                    var singleFragmetInstance = jProperty.Value.ToObject(singleItemPropertyType);

                    singleItemProperty.SetValue(obj, singleFragmetInstance);
                }
            }

            var multiItemProperties = AttributeReader.GetMultiItemInfos(type);

            if (model.MultiItems != null)
            {
                foreach (var multiItemProperty in multiItemProperties)
                {
                    var multiItemName = multiItemProperty.Name;
                    var asqlModelMultiItem = model.MultiItems.SingleOrDefault(x => x.Name == multiItemName);

                    if (asqlModelMultiItem == null)
                    {
                        continue;
                    }

                    var multiItemPropertyType = multiItemProperty.PropertyType;

                    var jProperty = asqlModelMultiItem.ConvertToJProperty();

                    if (jProperty == null)
                        continue;

                    var multiFragmetInstance = jProperty.Value.ToObject(multiItemPropertyType);

                    multiItemProperty.SetValue(obj, multiFragmetInstance);
                }
            }

            return (ESQLObject)obj;
        }
        #endregion

        public static string ConvertToJArrayString(this IEnumerable<ESQLObject> objects)
        {
            if (objects == null)
                return null;

            JArray array = new JArray();

            var jObjects = objects.Select(x => x.ConvertToJObject());

            foreach (var jObject in jObjects)
            {
                array.Add(jObject);
            }

            return array.ToString();
        }
    }
}