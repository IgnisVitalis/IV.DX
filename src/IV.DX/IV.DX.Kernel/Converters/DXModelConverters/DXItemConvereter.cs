using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXItemConvereter
    {
        public static DXItem ToDXItem(this JObject jObject)
        {
            var jObjectCopy = jObject.DeepClone() as JObject;

            DXItem fragment = new DXItem((Guid)jObject[Constants.ID], (Guid)jObject[Constants.DXUnitID], jObjectCopy.ToDictionary());

            return fragment;
        }
    }
}