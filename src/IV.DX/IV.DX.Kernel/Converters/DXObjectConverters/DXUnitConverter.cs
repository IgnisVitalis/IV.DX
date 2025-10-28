using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Converters.JObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXObjectConverters
{
    internal static class DXUnitConverter
    {
        #region Create instance
        public static T ToDXUnit<T>(string json) where T : DXUnit =>
            ToDXUnits<T>(DXModelConverter.Parse(json));

        public static IEnumerable<T> ToDXUnits<T>(string json) where T : DXUnit =>
            ToDXUnits<T>(JArray.Parse(json));

        public static IEnumerable<T> ToDXUnits<T>(JArray jArray) where T : DXUnit
        {
            foreach (JObject jObject in jArray)
                yield return ToDXUnits<T>(jObject);
        }

        public static T? ToDXUnits<T>(JObject jObject) where T : DXUnit =>
            ToDXUnits<T>(DXModelConverter.Parse(jObject));

        public static DXUnit? ToDXUnits(string json, Type type) =>
            ToDXUnits(DXModelConverter.Parse(json), type);

        public static DXUnit? ToDXUnits(JObject jObject, Type type) =>
            ToDXUnits(DXModelConverter.Parse(jObject), type);

        public static T? ToDXUnits<T>(DXModel dxModel) where T : DXUnit =>
            (T?)ToDXUnitPrivate(dxModel, typeof(T));

        public static DXUnit? ToDXUnits(DXModel dxModel, Type type) =>
            ToDXUnitPrivate(dxModel, type);

        private static DXUnit? ToDXUnitPrivate(DXModel? dxModel, Type? type)
        {
            if (dxModel is null || type is null)
                return null;

            var own = dxModel.DXMainElement.ToJProperty();
            var obj = (own?.Value?.ToObject(type)) ?? Activator.CreateInstance(type)!;

            var idProp = type.GetProperty(Constants.ID);
            idProp?.SetValue(obj, dxModel.DXMainElement.Item.ID);

            var singleProps = AttributeReader.GetSingleItemInfos(type);
            if (dxModel.DXSingleElements != null)
            {
                foreach (var sp in singleProps)
                {
                    var modelItem = dxModel.DXSingleElements.SingleOrDefault(x => x.Name == sp.Name);
                    if (modelItem is null) continue;

                    var jProp = modelItem.ConvertToJPropertyWithoutSystemProperties();
                    if (jProp?.Value == null) continue;

                    var instance = jProp.Value.ToObject(sp.PropertyType);
                    sp.SetValue(obj, instance);
                }
            }

            var multiProps = AttributeReader.GetMultiItemInfos(type);
            if (dxModel.DXMultiElements != null)
            {
                foreach (var mp in multiProps)
                {
                    var modelItem = dxModel.DXMultiElements.SingleOrDefault(x => x.Name == mp.Name);
                    if (modelItem is null) continue;

                    var jProp = modelItem.ToJProperty();
                    if (jProp?.Value == null) continue;

                    var instance = jProp.Value.ToObject(mp.PropertyType);
                    mp.SetValue(obj, instance);
                }
            }

            return (DXUnit)obj;
        }
        #endregion      
    }
}