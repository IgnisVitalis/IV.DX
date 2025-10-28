using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXItemConvereter
    {
        public static JObject Parse(this DXItem dxItem, bool exlcudeSystemProperties = false)
        {
            JObject jObject = dxItem.Content != null ? new JObject(dxItem.Content) : new JObject();

            if (exlcudeSystemProperties)
            {
                var systemProperties = jObject.Properties().Where(x =>
                       x.Name.Length >= Constants.SystemPropertyPrefix.Length
                       && x.Name.Substring(0, Constants.SystemPropertyPrefix.Length) == Constants.SystemPropertyPrefix
                   ).ToList();

                foreach (var systemProperty in systemProperties)
                {
                    jObject.Remove(systemProperty.Name);
                }
            }

            return jObject;
        }

        public static DXItem ConvertFromJObject(JObject jObject)
        {
            DXItem fragment = new DXItem
            {
                ID = jObject[Constants.ID] != null ? (Guid?)jObject[Constants.ID] : null,
                DXUnitID = jObject[Constants.DXUnitID] != null ? (Guid?)jObject[Constants.DXUnitID] : null
            };

            var jObjectCopy = jObject.DeepClone() as JObject;

            jObjectCopy.Remove(Constants.ID);
            jObjectCopy.Remove(Constants.DXUnitID);

            fragment.Content = jObjectCopy;

            return fragment;
        }
    }
}