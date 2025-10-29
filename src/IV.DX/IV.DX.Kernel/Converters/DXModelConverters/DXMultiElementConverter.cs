using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
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

            var announced = new HashSet<DXItem>();
            var deleted = new HashSet<DXItem>();
            var isRequired = JHelper.GetValue<bool?>(jProperty.Value as JObject, Constants.SystemPropertyIsRequired) ?? false;

            if (jProperty[Constants.Announced] != null)
            {
                announced = (jProperty[Constants.Announced] as JArray).Children()
                    .Select(x => x as JObject).Select(x => x.ToDXItem()).ToHashSet();
            }

            if (jProperty[Constants.Deleted] != null)
            {
                deleted = (jProperty[Constants.Deleted] as JArray).Children()
                    .Select(x => x as JObject).Select(x => x.ToDXItem()).ToHashSet();
            }

            var mode = (MultiElementsMode)jProperty[Constants.Mode].Value<int>();
            var attribute = new DXElementAttribute(jProperty[Constants.SystemPropertyTypeName] != null ? jProperty[Constants.SystemPropertyTypeName].Value<string>() : jProperty.Name);


            if (mode == MultiElementsMode.Full)
            {
                return DXMultiElement.CreateForFullMode(jProperty.Name, attribute, announced);
            }
            else
            {
                return DXMultiElement.CreateForTargetMode(jProperty.Name, attribute, announced, deleted);
            }
        }
    }
}