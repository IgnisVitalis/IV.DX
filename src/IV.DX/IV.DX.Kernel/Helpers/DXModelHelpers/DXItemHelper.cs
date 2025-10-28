using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Helpers.DXModelHelpers
{
    internal static class DXItemHelper
    {
        //public static DXItem Combine(params DXItem[] items)
        //{
        //    DXItem result = new DXItem()
        //    {
        //        Content = new JObject()
        //    };

        //    foreach (var fragment in items.Where(x => x != null).ToList())
        //    {
        //        result.ID = fragment.ID;

        //        JObject jObject = fragment.Content;

        //        if (jObject == null)
        //            continue;

        //        // Copy properties
        //        foreach (var item in jObject.Properties().Where(x => x.Value is JValue).ToList())
        //        {
        //            if (result.Content.ContainsKey(item.Name))
        //            {
        //                result.Content[item.Name] = item.Value;
        //            }
        //            else
        //            {
        //                result.Content.Add(item.DeepClone());
        //            }
        //        }

        //        // Copy relations
        //        foreach (var item in jObject.Properties().Where(x => x.Value is JObject).ToList())
        //        {
        //            if (!result.Content.ContainsKey(item.Name))
        //            {
        //                JObject jObjectForRel = new JObject
        //                {
        //                    { Constants.Announced, new JArray() },
        //                    { Constants.Deleted, new JArray() }
        //                };

        //                result.Content.Add(item.Name, jObjectForRel);
        //            }

        //            var addedRelations = item.Value[Constants.Announced];

        //            if (addedRelations != null && addedRelations is JArray)
        //            {
        //                var idsFromIncomeObj = (item.Value[Constants.Announced] as JArray).ToObject<IEnumerable<Guid>>();
        //                var idsFromCurrentObj = (result.Content[item.Name][Constants.Announced] as JArray).ToObject<IEnumerable<Guid>>();

        //                result.Content[item.Name][Constants.Announced] = new JArray(idsFromIncomeObj.Concat(idsFromCurrentObj).ToList());
        //            }

        //            var removedRelations = item.Value[Constants.Deleted];

        //            if (removedRelations != null && removedRelations is JArray)
        //            {
        //                var idsFromIncomeObj = (item.Value[Constants.Deleted] as JArray).ToObject<IEnumerable<Guid>>();
        //                var idsFromCurrentObj = (result.Content[item.Name][Constants.Deleted] as JArray).ToObject<IEnumerable<Guid>>();

        //                result.Content[item.Name][Constants.Deleted] = new JArray(idsFromIncomeObj.Concat(idsFromCurrentObj).ToList());
        //            }
        //        }
        //    }

        //    return result;
        //}
    }
}