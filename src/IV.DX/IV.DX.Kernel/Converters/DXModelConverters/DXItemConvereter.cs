using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXItemConvereter
    {
        public static DXItem ToDXItem(this JObject jObject)
        {
            DXItem fragment = new DXItem
            {
                ID = (Guid)jObject[Constants.ID],
                DXUnitID = (Guid)jObject[Constants.DXUnitID]
            };

            var jObjectCopy = jObject.DeepClone() as JObject;

            jObjectCopy.Remove(Constants.ID);
            jObjectCopy.Remove(Constants.DXUnitID);

            fragment.Content = jObjectCopy;

            return fragment;
        }
    }
}