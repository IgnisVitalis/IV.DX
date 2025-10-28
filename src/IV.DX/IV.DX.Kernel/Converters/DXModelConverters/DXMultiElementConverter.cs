using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXMultiElementConverter
    {
        public static JProperty ConvertToJProperty(this DXMultiElement dxMultiElement)
        {
            JObject jObject = new JObject
            {
                [Constants.SystemPropertyTypeName] = dxMultiElement.Attribute.Type,
                [Constants.Mode] = (int)dxMultiElement.Mode
            };

            JArray announced = new JArray();
            JArray Deleted = new JArray();

            if (dxMultiElement.Announced != null)
            {
                foreach (var item in dxMultiElement.Announced)
                {
                    announced.Add(item.Parse());
                }
            }

            if (dxMultiElement.Deleted != null)
            {
                foreach (var item in dxMultiElement.Deleted)
                {
                    Deleted.Add(item.Parse());
                }
            }

            jObject[Constants.Announced] = announced;
            jObject[Constants.Deleted] = Deleted;

            JProperty jProperty = new JProperty(dxMultiElement.Name, jObject);

            return jProperty;
        }

        public static JProperty ConvertToJPropertyWithoutSystemProperties(this DXMultiElement dxMultiElement)
        {
            JObject jObject = new JObject
            {
                [Constants.Mode] = (int)dxMultiElement.Mode
            };

            JArray announced = new JArray();
            JArray Deleted = new JArray();

            if (dxMultiElement.Announced != null)
            {
                foreach (var item in dxMultiElement.Announced)
                {
                    announced.Add(item.Parse(true));
                }
            }

            if (dxMultiElement.Deleted != null)
            {
                foreach (var item in dxMultiElement.Deleted)
                {
                    Deleted.Add(item.Parse(true));
                }
            }

            jObject[Constants.Announced] = announced;
            jObject[Constants.Deleted] = Deleted;

            JProperty jProperty = new JProperty(dxMultiElement.Name, jObject);

            return jProperty;
        }

        public static DXMultiElement Parse(JProperty jProperty)
        {
            if (jProperty == null)
                return null;

            DXMultiElement multiFragment = new DXMultiElement
            {
                Attribute = new DXElementAttribute(jProperty[Constants.SystemPropertyTypeName] != null ? jProperty[Constants.SystemPropertyTypeName].Value<string>() : jProperty.Name),
                Name = jProperty.Name,
                Mode = (MultiElementsMode)jProperty[Constants.Mode].Value<int>()
            };

            if (jProperty[Constants.Announced] == null)
            {
                multiFragment.Announced = new HashSet<DXItem>();
            }
            else
            {
                multiFragment.Announced = (jProperty[Constants.Announced] as JArray).Children()
                    .Select(x => x as JObject).Select(x => DXItemConvereter.ConvertFromJObject(x)).ToHashSet();
            }

            if (jProperty[Constants.Deleted] == null)
            {
                multiFragment.Deleted = new HashSet<DXItem>();
            }
            else
            {
                multiFragment.Deleted = (jProperty[Constants.Deleted] as JArray).Children()
                    .Select(x => x as JObject).Select(x => DXItemConvereter.ConvertFromJObject(x)).ToHashSet();
            }

            return multiFragment;
        }
    }
}