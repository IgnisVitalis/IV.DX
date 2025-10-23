using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace IV.DX.Kernel.Converters
{
    internal static class DXUnitHelper
    {
        public static string GetTypeName(string json) => GetTypeName(JObject.Parse(json));

        public static string? GetTypeName(JObject jObject) =>
            (string?)jObject[Constants.SystemPropertyTypeName];

        public static string GetTypeName(Type type) =>
            AttributeReader.GetDXUnitTypeName(type);

        public static Guid GetID(JObject jObject) => (Guid)jObject[Constants.ID];

        #region Convert to JObject / string
        public static JObject ConvertToJObject(this DXUnit dxUnit) =>
            dxUnit.ConvertToDXModel().ConvertToJObject();

        public static string ConvertToString(this DXUnit dxUnit) =>
            dxUnit.ConvertToJObject().ToString();
        #endregion

        #region Convert to DXModel
        public static DXModel? ConvertToDXModel(this DXUnit? dxUnit)
        {
            if (dxUnit is null) return null;

            var unitAttr = DXReflectionHelper.GetAttr<DXUnitAttribute>(dxUnit.GetType());

            var ownItem = new DXMainItem(unitAttr)
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
                SingleItems = GetDXSingleElements(dxUnit),
                MultiItems = GetDXMultiElements(dxUnit)
            };
        }

        private static HashSet<DXSingleElement> GetDXSingleElements(DXUnit dxUnit)
        {
            var singleInfos = AttributeReader.GetSingleItemInfos(dxUnit);
            return singleInfos.Select(pi =>
            {
                var element = pi.GetValue(dxUnit) as DXElement;
                return DXElementHelper.ConvertToSingleItem(element, pi.Name);
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

                var announcedList = new List<DXItem>();
                var deletedList = new List<DXItem>();

                if (value != null)
                {
                    void Fill(string propertyName, List<DXItem> target)
                    {
                        var src = multiType.GetProperty(propertyName)?.GetValue(value) as IEnumerable<DXElement>;
                        if (src == null) return;

                        foreach (var e in src)
                        {
                            target.Add(new DXItem
                            {
                                ID = e.ID,
                                DXUnitID = dxUnit.ID,
                                Content = e.GetContent()
                            });
                        }
                    }

                    Fill(Constants.Announced, announcedList);
                    Fill(Constants.Deleted, deletedList);
                }

                var multi = new DXMultiElement
                {
                    DXElementInfo = elementInfo,
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
            foreach (var prop in DXReflectionHelper.GetPropsWithAttribute<DXColumnAttribute>(obj.GetType()))
            {
                var value = prop.GetValue(obj);
                jObject[prop.Name] = new JValue(value);
            }
            return jObject;
        }
        #endregion

        #region Create instance
        public static T CreateInstance<T>(string json) where T : DXUnit =>
            CreateInstance<T>(DXModel.CreateInstance(json));

        public static IEnumerable<T> CreateInstances<T>(string json) where T : DXUnit =>
            CreateInstances<T>(JArray.Parse(json));

        public static IEnumerable<T> CreateInstances<T>(JArray jArray) where T : DXUnit
        {
            foreach (JObject jObject in jArray)
                yield return CreateInstance<T>(jObject);
        }

        public static T? CreateInstance<T>(JObject jObject) where T : DXUnit =>
            CreateInstance<T>(DXModel.CreateInstance(jObject));

        public static DXUnit? CreateInstance(string json, Type type) =>
            CreateInstance(DXModel.CreateInstance(json), type);

        public static DXUnit? CreateInstance(JObject jObject, Type type) =>
            CreateInstance(DXModel.CreateInstance(jObject), type);

        public static T? CreateInstance<T>(DXModel dxModel) where T : DXUnit =>
            (T?)ConvertToDxUnitObject(dxModel, typeof(T));

        public static DXUnit? CreateInstance(DXModel dxModel, Type type) =>
            ConvertToDxUnitObject(dxModel, type);

        public static T? CreateDXElementInstance<T>(DXSingleElement? item) where T : DXElement
        {
            if (item is null) return null;

            var jProp = item.ConvertToJPropertyWithoutSystemProperties();
            return (T?)jProp?.Value?.ToObject(typeof(T));
        }

        private static DXUnit? ConvertToDxUnitObject(DXModel? dxModel, Type? type)
        {
            if (dxModel is null || type is null)
                return null;

            var own = dxModel.OwnSingleItem.ConvertToJPropertyWithoutSystemProperties();
            var obj = (own?.Value?.ToObject(type)) ?? Activator.CreateInstance(type)!;

            var idProp = type.GetProperty(Constants.ID);
            idProp?.SetValue(obj, dxModel.OwnSingleItem.Item.ID);

            var singleProps = AttributeReader.GetSingleItemInfos(type);
            if (dxModel.SingleItems != null)
            {
                foreach (var sp in singleProps)
                {
                    var modelItem = dxModel.SingleItems.SingleOrDefault(x => x.Name == sp.Name);
                    if (modelItem is null) continue;

                    var jProp = modelItem.ConvertToJPropertyWithoutSystemProperties();
                    if (jProp?.Value == null) continue;

                    var instance = jProp.Value.ToObject(sp.PropertyType);
                    sp.SetValue(obj, instance);
                }
            }

            var multiProps = AttributeReader.GetMultiItemInfos(type);
            if (dxModel.MultiItems != null)
            {
                foreach (var mp in multiProps)
                {
                    var modelItem = dxModel.MultiItems.SingleOrDefault(x => x.Name == mp.Name);
                    if (modelItem is null) continue;

                    var jProp = modelItem.ConvertToJProperty();
                    if (jProp?.Value == null) continue;

                    var instance = jProp.Value.ToObject(mp.PropertyType);
                    mp.SetValue(obj, instance);
                }
            }

            return (DXUnit)obj;
        }
        #endregion

        public static string? ConvertToJArrayString(this IEnumerable<DXUnit>? objects)
        {
            if (objects is null) return null;
            var array = new JArray(objects.Select(o => o.ConvertToJObject()));
            return array.ToString();
        }
    }
}