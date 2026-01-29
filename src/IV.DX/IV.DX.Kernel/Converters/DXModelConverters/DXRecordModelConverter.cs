using System;
using System.Collections.Generic;
using System.Linq;
using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXRecordModelConverter
    {
        public static IEnumerable<DXModel> ToDXModels(DXDataBlock<DXUnitRecord> block)
        {
            if (block?.Data?.Upsert == null)
                return Array.Empty<DXModel>();

            return block.Data.Upsert.Select(record => ToDXModel(block, record)).ToList();
        }

        public static DXModel ToDXModel(DXDataBlock<DXUnitRecord> block, DXUnitRecord record)
        {
            ArgumentNullException.ThrowIfNull(block);
            ArgumentNullException.ThrowIfNull(record);

            var typeName = block.Meta?.Type;
            if (string.IsNullOrWhiteSpace(typeName))
                throw new InvalidOperationException("DXUnitRecord block Meta.Type is required.");

            var mainItem = new DXItem(
                typeName,
                record.ID,
                record.ID,
                record.TimeStamp,
                ConvertFields(record.Fields));

            var mainElement = new DXMainElement(new DXUnitAttribute(typeName), mainItem);

            var singleElements = new HashSet<DXSingleElement>();
            var multiElements = new HashSet<DXMultiElement>();

            if (record.DXElements != null)
            {
                foreach (var kvp in record.DXElements)
                {
                    var elementKey = kvp.Key;
                    var elementBlock = kvp.Value;
                    if (elementBlock == null) continue;

                    var elementTypeName = elementBlock.Meta?.Type;
                    if (string.IsNullOrWhiteSpace(elementTypeName))
                        elementTypeName = elementKey;

                    var elementName = elementTypeName ?? elementKey;
                    var isMulti = elementBlock.Meta?.IsMulti ?? InferIsMulti(elementBlock);

                    if (isMulti)
                    {
                        var mode = MapMode(elementBlock.Meta?.Op);
                        var announced = BuildElementItems(elementName, elementBlock.Data?.Upsert, record.ID);
                        var deleted = BuildDeleteItems(elementName, elementBlock.Data?.Delete, record.ID);

                        var multiElement = mode == MultiElementsMode.Target
                            ? DXMultiElement.CreateForTargetMode(elementName, new DXElementAttribute(elementName), announced, deleted)
                            : DXMultiElement.CreateForFullMode(elementName, new DXElementAttribute(elementName), announced);

                        multiElements.Add(multiElement);
                    }
                    else
                    {
                        var first = elementBlock.Data?.Upsert?.FirstOrDefault();
                        if (first == null) continue;

                        var item = BuildElementItem(elementName, first, record.ID);
                        var isRequired = elementBlock.Meta?.IsRequired ?? false;

                        singleElements.Add(new DXSingleElement(elementName, new DXElementAttribute(elementName), item, isRequired));
                    }
                }
            }

            return new DXModel(mainElement, singleElements, multiElements);
        }

        private static HashSet<DXItem> BuildElementItems(string typeName, IEnumerable<DXElementRecord>? records, Guid parentId)
        {
            if (records == null)
                return new HashSet<DXItem>();

            var items = new HashSet<DXItem>();
            foreach (var record in records)
            {
                items.Add(BuildElementItem(typeName, record, parentId));
            }

            return items;
        }

        private static HashSet<DXItem> BuildDeleteItems(string typeName, IEnumerable<DXDeleteRef>? deletes, Guid parentId)
        {
            if (deletes == null)
                return new HashSet<DXItem>();

            var items = new HashSet<DXItem>();
            foreach (var deleteRef in deletes)
            {
                var dxUnitId = GetDxUnitId(deleteRef.Fields, parentId);
                var content = ConvertFields(deleteRef.Fields);
                items.Add(new DXItem(typeName, deleteRef.ID, dxUnitId, DateTime.MinValue, content));
            }

            return items;
        }

        private static DXItem BuildElementItem(string typeName, DXElementRecord record, Guid parentId)
        {
            var dxUnitId = record.DXUnitID == Guid.Empty ? parentId : record.DXUnitID;

            return new DXItem(
                typeName,
                record.ID,
                dxUnitId,
                record.TimeStamp,
                ConvertFields(record.Fields));
        }

        private static Dictionary<string, object> ConvertFields(IDictionary<string, JToken>? fields)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (fields == null || fields.Count == 0)
                return result;

            foreach (var kvp in fields)
            {
                result[kvp.Key] = ConvertTokenToObject(kvp.Value);
            }

            return result;
        }

        private static object? ConvertTokenToObject(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            return token.ToObject<object>();
        }

        private static Guid GetDxUnitId(IDictionary<string, JToken>? fields, Guid fallback)
        {
            if (fields == null)
                return fallback;

            foreach (var kvp in fields)
            {
                if (!string.Equals(kvp.Key, Constants.DXUnitID, StringComparison.OrdinalIgnoreCase))
                    continue;

                var token = kvp.Value;
                if (token == null || token.Type == JTokenType.Null)
                    return fallback;

                try
                {
                    var value = token.ToObject<Guid>();
                    return value == Guid.Empty ? fallback : value;
                }
                catch
                {
                    return fallback;
                }
            }

            return fallback;
        }

        private static bool InferIsMulti(DXDataBlock<DXElementRecord> block)
        {
            var count = block.Data?.Upsert?.Count ?? 0;
            if (count > 1)
                return true;

            var deleteCount = block.Data?.Delete?.Count ?? 0;
            return deleteCount > 1;
        }

        private static MultiElementsMode MapMode(string? op)
        {
            if (string.IsNullOrWhiteSpace(op))
                return MultiElementsMode.Full;

            return op.Equals("Patch", StringComparison.OrdinalIgnoreCase)
                ? MultiElementsMode.Target
                : MultiElementsMode.Full;
        }
    }
}
