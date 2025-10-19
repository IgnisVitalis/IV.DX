using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace IV.DX.Kernel.Converters
{
    internal static class DXUnitHelper
    {
        public static string GetTypeName(string json)
        {
            var jObject = JObject.Parse(json);

            return GetTypeName(jObject);
        }

        public static string GetTypeName(JObject jObject)
        {
            return (string)jObject[Constants.SystemPropertyTypeName];
        }

        public static string GetTypeName(Type type)
        {
            return AttributeReader.GetDXUnitTypeName(type);
        }        

        public static Guid GetID(JObject jObject)
        {
            return (Guid)jObject[Constants.ID];
        }

        #region Convert to JObject       
        public static JObject ConvertToJObject(this DXUnit dxUnit)
        {
            var result = dxUnit.ConvertToDXModel().ConvertToJObject();

            return result;
        }

        public static string ConvertToString(this DXUnit dxUnit)
        {
            var jObject = dxUnit.ConvertToJObject();
            var str = jObject.ToString();

            return str;
        }
        #endregion

        #region Convert to DXModel
        public static DXModel? ConvertToDXModel(this DXUnit dxUnit)
        {
            if (dxUnit == null)
                return null;

            var objectInfo = AttributeReader.GetSingleAttribute<DXUnitAttribute>
                   (dxUnit.GetType());

            var ownItem = new DXMainItem(objectInfo)
            {
                Item = new DXItem()
                {
                    ID = dxUnit.ID,
                    ObjectID = dxUnit.ID,
                    Content = GetContent(dxUnit)
                }
            };

            DXModel dxModel = new DXModel(ownItem)
            {
                SingleItems = GetDXSingleElements(dxUnit),
                MultiItems = GetDXMutliElements(dxUnit)
            };

            return dxModel;
        }

        private static IEnumerable<DXSingleElement> GetDXSingleElements(DXUnit dxUnit)
        {
            var singleItemInfos = AttributeReader.GetSingleItemInfos(dxUnit);

            var result = singleItemInfos.Select(x =>
            {
                var singleItem = x.GetValue(dxUnit) as DXElement;

                DXSingleElement dxSingleItem = new DXSingleElement()
                {
                    ElementInfo = AttributeReader.GetSingleAttribute<DXElementAttribute>(x.PropertyType),
                    Item = new DXItem()
                    {
                        ID = singleItem?.ID,
                        ObjectID = dxUnit.ID,
                        Content = GetContent(singleItem),
                    },
                    Name = x.Name
                };

                return dxSingleItem;
            }).ToList();

            return result;
        }

        public static DXSingleElement ConvertToSingleItem(this DXElement dxElement)
        {
            var dxElementInfo = AttributeReader.GetSingleAttribute<DXElementAttribute>(dxElement.GetType());

            DXSingleElement singleItem = new DXSingleElement()
            {
                ElementInfo = dxElementInfo,
                Item = new DXItem()
                {
                    ID = dxElement.ID,
                    ObjectID = dxElement.ObjectID,
                    Content = GetContent(dxElement)
                },
                Name = dxElementInfo.Name
            };
            return singleItem;
        }

        private static IEnumerable<DXMultiElement> GetDXMutliElements(DXUnit dxUnit)
        {
            var multiItemsInfos = AttributeReader.GetMultiItemInfos(dxUnit);

            var result = multiItemsInfos.Select(x =>
            {
                var multiItemType = x.PropertyType;
                var multiItemValue = x.GetValue(dxUnit);

                MultiElementsMode mode = MultiElementsMode.Full;

                if (multiItemValue != null)
                    mode = (MultiElementsMode)multiItemType.GetProperty("Mode").GetValue(multiItemValue);

                DXMultiElement multiItem = new DXMultiElement()
                {
                    DXElementInfo = AttributeReader.GetSingleAttribute<DXElementAttribute>(x.PropertyType.GenericTypeArguments[0]),
                    Name = x.Name,
                    Mode = mode
                };

                if (multiItemValue != null)
                {
                    var announcedArray = multiItemType.GetProperty(Constants.Announced).GetValue(multiItemValue) as IEnumerable<DXElement>;

                    if (announcedArray != null)
                    {
                        multiItem.Announced = announcedArray.Select(y =>
                        {
                            var content = GetContent(y);

                            var dxItem = new DXItem()
                            {
                                ID = y.ID,
                                ObjectID = dxUnit.ID,
                                Content = content
                            };

                            return dxItem;
                        }).ToList();
                    }
                    else
                    {
                        multiItem.Announced = new List<DXItem>();
                    }

                    var destroyedArray = multiItemType.GetProperty(Constants.Deleted).GetValue(multiItemValue) as IEnumerable<DXElement>;

                    if (destroyedArray != null)
                    {
                        multiItem.Deleted = destroyedArray.Select(y =>
                        {
                            var content = GetContent(y);

                            var dxItem = new DXItem()
                            {
                                ID = y.ID,
                                ObjectID = dxUnit.ID,
                                Content = content
                            };

                            return dxItem;
                        }).ToList();
                    }
                    else
                    {
                        multiItem.Deleted = new List<DXItem>();
                    }
                }

                return multiItem;
            }).ToList();

            return result;
        }

        private static JObject GetContent(DXElement dxElement)
        {
            if (dxElement == null)
                return null;

            JObject jObject = new JObject();

            var properties = dxElement.GetType().GetProperties()
                .Where(x => AttributeReader.GetSinglePropertyAttribute<DXColumnAttribute>(x) != null)
                .ToList();

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetSinglePropertyAttribute<DXColumnAttribute>(property);

                jObject[property.Name] = new JValue(property.GetValue(dxElement));
            }

            return jObject;
        }

        private static JObject GetContent(DXUnit obj)
        {
            if (obj == null)
                return null;

            JObject jObject = new JObject();

            var properties = obj.GetType().GetProperties()
                .Where(x => AttributeReader.GetSinglePropertyAttribute<DXColumnAttribute>(x) != null)
                .ToList();

            foreach (var property in properties)
            {
                var attribute = AttributeReader.GetSinglePropertyAttribute<DXColumnAttribute>(property);

                jObject[property.Name] = new JValue(property.GetValue(obj));
            }

            return jObject;
        }
        #endregion

        #region Create instance
        public static T CreateInstance<T>(string json) where T : DXUnit
        {
            var dxModel = DXModel.CreateInstance(json);

            T dxUnit = CreateInstance<T>(dxModel);

            return dxUnit;
        }

        public static IEnumerable<T> CreateInstances<T>(string json) where T : DXUnit
        {
            var jArray = JArray.Parse(json);

            return CreateInstances<T>(jArray);
        }

        public static IEnumerable<T> CreateInstances<T>(JArray jArray) where T : DXUnit
        {
            foreach (JObject jObject in jArray)
            {
                yield return CreateInstance<T>(jObject);
            }
        }

        public static T CreateInstance<T>(JObject jObject) where T : DXUnit
        {
            var dxModel = DXModel.CreateInstance(jObject);

            T dxUnit = CreateInstance<T>(dxModel);

            return dxUnit;
        }

        public static DXUnit CreateInstance(string json, Type type)
        {
            var dxModel = DXModel.CreateInstance(json);

            DXUnit dxUnit = CreateInstance(dxModel, type);

            return dxUnit;
        }

        public static DXUnit CreateInstance(JObject jObject, Type type)
        {
            var dxModel = DXModel.CreateInstance(jObject);

            DXUnit dxUnit = CreateInstance(dxModel, type);

            return dxUnit;
        }

        public static T CreateInstance<T>(DXModel dxModel) where T : DXUnit
        {
            return ConvertTodxUnitect(dxModel, typeof(T)) as T;
        }

        public static DXUnit CreateInstance(DXModel dxModel, Type type)
        {
            return ConvertTodxUnitect(dxModel, type);
        }

        public static T CreateDXElementInstance<T>(DXSingleElement item) where T : DXElement
        {
            if (item == null)
                return null;

            var singleItemName = item.Name;
            var asqlModelSingleItem = singleItemName;

            var jProperty = item.ConvertToJPropertyWithoutSystemProperties();

            var singleFragmetInstance = jProperty.Value.ToObject(typeof(T));

            return singleFragmetInstance as T;
        }

        private static DXUnit ConvertTodxUnitect(DXModel dxModel, Type type)
        {
            if (dxModel == null)
                return null;

            if (type == null)
                return null;

            var obj = dxModel.OwnSingleItem.ConvertToJPropertyWithoutSystemProperties().Value.ToObject(type);

            var objIdProperty = type.GetProperty(Constants.ID);
            objIdProperty.SetValue(obj, dxModel.OwnSingleItem.Item.ID);

            var singleItemProperties = AttributeReader.GetSingleItemInfos(type);

            if (dxModel.SingleItems != null)
            {
                foreach (var singleItemProperty in singleItemProperties)
                {
                    var singleItemName = singleItemProperty.Name;
                    var asqlModelSingleItem = dxModel.SingleItems.SingleOrDefault(x => x.Name == singleItemName);

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

            if (dxModel.MultiItems != null)
            {
                foreach (var multiItemProperty in multiItemProperties)
                {
                    var multiItemName = multiItemProperty.Name;
                    var asqlModelMultiItem = dxModel.MultiItems.SingleOrDefault(x => x.Name == multiItemName);

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

            return (DXUnit)obj;
        }
        #endregion

        public static string ConvertToJArrayString(this IEnumerable<DXUnit> objects)
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