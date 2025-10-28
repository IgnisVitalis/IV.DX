using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXMultiElementConverter
    {
        public static DXMultiElement ToDXMultiElement(this JProperty jProperty)
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
                    .Select(x => x as JObject).Select(x => x.ToDXItem()).ToHashSet();
            }

            if (jProperty[Constants.Deleted] == null)
            {
                multiFragment.Deleted = new HashSet<DXItem>();
            }
            else
            {
                multiFragment.Deleted = (jProperty[Constants.Deleted] as JArray).Children()
                    .Select(x => x as JObject).Select(x => x.ToDXItem()).ToHashSet();
            }

            return multiFragment;
        }
    }
}