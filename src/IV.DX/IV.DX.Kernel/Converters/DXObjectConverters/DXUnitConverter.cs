using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXObjectConverters
{
    internal static class DXUnitConverter
    {
        #region Create instance
        public static T ToDXUnit<T>(this string json) where T : DXUnit
        {
            return ToDXUnits<T>(json).FirstOrDefault()!;
        }

        public static IEnumerable<T> ToDXUnits<T>(this string json) where T : DXUnit =>
            ToDXUnits<T>(ParseBlocks(json));

        public static IEnumerable<T> ToDXUnits<T>(this JArray jArray) where T : DXUnit =>
            ToDXUnits<T>(ParseBlocks(jArray));

        public static T? ToDXUnits<T>(this JObject jObject) where T : DXUnit =>
            ToDXUnits<T>(ParseBlocks(jObject)).FirstOrDefault();

        public static DXUnit? ToDXUnits(this string json, Type type) =>
            ToDXUnits(ParseBlocks(json), type).FirstOrDefault();

        public static DXUnit? ToDXUnits(this JObject jObject, Type type) =>
            ToDXUnits(ParseBlocks(jObject), type).FirstOrDefault();

        private static IEnumerable<T> ToDXUnits<T>(IEnumerable<DXDataBlock<DXUnitRecord>> blocks) where T : DXUnit
        {
            return DXRecordConverter.ToDXUnits<T>(blocks);
        }

        private static IEnumerable<DXUnit> ToDXUnits(IEnumerable<DXDataBlock<DXUnitRecord>> blocks, Type type)
        {
            foreach (var block in blocks)
            {
                var items = block?.Data?.Items;
                if (items == null) continue;

                foreach (var record in items)
                {
                    yield return DXRecordConverter.ToDXUnit(record, type);
                }
            }
        }

        private static IEnumerable<DXDataBlock<DXUnitRecord>> ParseBlocks(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<DXDataBlock<DXUnitRecord>>();

            var token = JToken.Parse(json);
            return ParseBlocks(token);
        }

        private static IEnumerable<DXDataBlock<DXUnitRecord>> ParseBlocks(JToken token)
        {
            if (token is JArray jArray)
            {
                return ParseBlocks(jArray);
            }

            if (token is JObject jObject)
            {
                var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
                return block == null ? Array.Empty<DXDataBlock<DXUnitRecord>>() : new[] { block };
            }

            return Array.Empty<DXDataBlock<DXUnitRecord>>();
        }

        private static IEnumerable<DXDataBlock<DXUnitRecord>> ParseBlocks(JArray jArray)
        {
            var blocks = new List<DXDataBlock<DXUnitRecord>>();
            foreach (var item in jArray)
            {
                var block = item.ToObject<DXDataBlock<DXUnitRecord>>();
                if (block != null)
                    blocks.Add(block);
            }

            return blocks;
        }
        #endregion      
    }
}

