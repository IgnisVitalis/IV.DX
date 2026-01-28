using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXItemConvereter
    {
        public static DXItem FromDXUnitToDXItem(this JObject jObject)
        {
            var jObjectCopy = jObject.DeepClone() as JObject;

            DXItem fragment = new DXItem(
                (string)jObject[Constants.SystemPropertyTypeName],
                (Guid)jObject[Constants.ID], 
                (Guid)jObject[Constants.DXUnitID], 
                (DateTime)jObject[Constants.TimeStamp], 
                jObjectCopy.ToDictionary());

            return fragment;
        }

        public static DXItem FromDXElementToDXItem(this JObject jObject)
        {
            var jObjectCopy = jObject.DeepClone() as JObject;

            DXItem fragment = new DXItem(
                (string)jObject[Constants.SystemPropertyTypeName],
                (Guid)jObject[Constants.ID], 
                (Guid)jObject[Constants.DXUnitID], 
                (DateTime)jObject[Constants.TimeStamp], 
                jObjectCopy.ToDictionary());

            return fragment;
        }
    }
}