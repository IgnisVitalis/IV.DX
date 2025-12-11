using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.JObjectConverters
{
    internal static class JPropertyConverter
    {
        public static JProperty ToJProperty(this DXMultiElement dxMultiElement)
        {
            JObject jObject = new JObject
            {
                [Constants.SystemPropertyTypeName] = dxMultiElement.Attribute.Type,
                [Constants.Mode] = (int)dxMultiElement.Mode
            };

            JArray announced = new JArray();
            JArray deleted = new JArray();

            if (dxMultiElement.Announced != null)
            {
                announced = dxMultiElement.Announced.ToJArray();
            }

            if (dxMultiElement.Deleted != null)
            {
                deleted = dxMultiElement.Deleted.ToJArray();
            }

            jObject[Constants.Announced] = announced;
            jObject[Constants.Deleted] = deleted;

            var name = dxMultiElement.Name;

            JProperty jProperty = new JProperty(name, jObject);

            return jProperty;
        }



        //public static JProperty ToJProperty(this DXMultiElement dxMultiElement)
        //{
        //    JObject jObject = new JObject
        //    {
        //        [Constants.Mode] = (int)dxMultiElement.Mode
        //    };

        //    JArray announced = new JArray();
        //    JArray Deleted = new JArray();

        //    if (dxMultiElement.Announced != null)
        //    {
        //        foreach (var item in dxMultiElement.Announced)
        //        {
        //            announced.Add(item.ToJObject(true));
        //        }
        //    }

        //    if (dxMultiElement.Deleted != null)
        //    {
        //        foreach (var item in dxMultiElement.Deleted)
        //        {
        //            Deleted.Add(item.ToJObject(true));
        //        }
        //    }

        //    jObject[Constants.Announced] = announced;
        //    jObject[Constants.Deleted] = Deleted;

        //    JProperty jProperty = new JProperty(dxMultiElement.Name, jObject);

        //    return jProperty;
        //}

        public static JProperty ToJProperty(this DXMainElement mainElement)
        {
            JObject jObject = new JObject(mainElement.Item.ToJObject(true));

            JProperty jProperty = new JProperty(mainElement.Attribute.Type, jObject);

            return jProperty;
        }

        public static JProperty ConvertToJProperty(this DXSingleElement dxSingleElement)
        {
            JObject jObject = null;

            if (dxSingleElement.Item != null)
            {
                jObject = new JObject(dxSingleElement.Item.ToJObject());
            }

            JProperty jProperty = new JProperty(dxSingleElement.Name, jObject);

            return jProperty;
        }

        public static JProperty ConvertToJPropertyWithoutSystemProperties(this DXSingleElement dxSingleElement)
        {
            if (dxSingleElement.Item == null)
                return null;

            JObject jObject = new JObject(dxSingleElement.Item.ToJObject(true));

            JProperty jProperty = new JProperty(dxSingleElement.Name, jObject);

            return jProperty;
        }
    }
}
