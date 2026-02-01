using System;
using System.Collections.Generic;
using System.Linq;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXModelConverters
{
    internal static class DXModelRecordConverter
    {
        public static DXDataBlock<DXUnitRecord> ToBlock(DXModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return ToBlock(new[] { model }, model.DXMainElement.Attribute.Type);
        }

        public static DXDataBlock<DXUnitRecord> ToBlock(IEnumerable<DXModel> models, string typeName)
        {
            ArgumentNullException.ThrowIfNull(models);

            var records = models.Select(ToRecord).ToList();

            return new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = typeName
                },
                Data = new DXData<DXUnitRecord>
                {
                    Upsert = records
                }
            };
        }

        public static DXUnitRecord ToRecord(DXModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var record = new DXUnitRecord
            {
                ID = model.DXMainElement.Item.ID,
                TimeStamp = model.DXMainElement.Item.TimeStamp,
                Fields = ConvertFields(model.DXMainElement.Item.Content),
                DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
            };

            foreach (var single in model.DXSingleElements)
            {
                var elementTypeName = string.IsNullOrWhiteSpace(single.Attribute?.Type)
                    ? single.Name
                    : single.Attribute.Type;

                record.DXElements[elementTypeName] = BuildSingleBlock(single, elementTypeName);
            }

            foreach (var multi in model.DXMultiElements)
            {
                var elementTypeName = string.IsNullOrWhiteSpace(multi.Attribute?.Type)
                    ? multi.Name
                    : multi.Attribute.Type;

                record.DXElements[elementTypeName] = BuildMultiBlock(multi, elementTypeName);
            }

            return record;
        }

        private static DXDataBlock<DXElementRecord> BuildSingleBlock(DXSingleElement single, string elementTypeName)
        {
            return new DXDataBlock<DXElementRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXElement",
                    Type = elementTypeName,
                    Op = "Patch",
                    IsMulti = false,
                    IsRequired = single.IsRequired
                },
                Data = new DXData<DXElementRecord>
                {
                    Upsert = new List<DXElementRecord> { BuildElementRecord(single.Item) }
                }
            };
        }

        private static DXDataBlock<DXElementRecord> BuildMultiBlock(DXMultiElement multi, string elementTypeName)
        {
            var upsert = multi.Announced.Select(BuildElementRecord).ToList();
            var delete = multi.Deleted.Select(BuildDeleteRef).ToList();

            return new DXDataBlock<DXElementRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXElement",
                    Type = elementTypeName,
                    Op = MapMode(multi.Mode),
                    IsMulti = true,
                    IsRequired = multi.IsRequired
                },
                Data = new DXData<DXElementRecord>
                {
                    Upsert = upsert.Count == 0 ? null : upsert,
                    Delete = delete.Count == 0 ? null : delete
                }
            };
        }

        private static DXElementRecord BuildElementRecord(DXItem item)
        {
            return new DXElementRecord
            {
                ID = item.ID,
                TimeStamp = item.TimeStamp,
                DXUnitID = item.DXUnitID,
                Fields = ConvertFields(item.Content)
            };
        }

        private static DXDeleteRef BuildDeleteRef(DXItem item)
        {
            var deleteRef = new DXDeleteRef { ID = item.ID };

            if (item.DXUnitID != Guid.Empty)
            {
                deleteRef.Fields = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase)
                {
                    { Constants.DXUnitID, JToken.FromObject(item.DXUnitID) }
                };
            }

            return deleteRef;
        }

        private static Dictionary<string, JToken>? ConvertFields(IDictionary<string, object> content)
        {
            if (content == null || content.Count == 0)
                return null;

            var result = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in content)
            {
                if (IsSystemField(kvp.Key))
                    continue;

                result[kvp.Key] = kvp.Value == null
                    ? JValue.CreateNull()
                    : JToken.FromObject(kvp.Value);
            }

            return result.Count == 0 ? null : result;
        }

        private static bool IsSystemField(string fieldName)
        {
            return Constants.SystemProperties.Any(p =>
                string.Equals(p, fieldName, StringComparison.OrdinalIgnoreCase));
        }

        private static string MapMode(MultiElementsMode mode)
        {
            return mode == MultiElementsMode.Target ? "Patch" : "Sync";
        }
    }
}
