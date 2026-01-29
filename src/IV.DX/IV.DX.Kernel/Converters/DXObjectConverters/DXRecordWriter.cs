using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXObjectConverters
{
    internal static class DXRecordWriter
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ColumnPropsCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> SingleElementPropsCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> MultiElementPropsCache = new();

        public static DXUnitRecord ToRecord(DXUnit unit, DXRecordWriteOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(unit);

            return BuildUnitRecord(unit, options);
        }

        public static DXDataBlock<DXUnitRecord> ToBlock(DXUnit unit, DXRecordWriteOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(unit);

            var record = BuildUnitRecord(unit, options);
            var meta = BuildUnitMeta(unit, options);

            return new DXDataBlock<DXUnitRecord>
            {
                Meta = meta,
                Data = new DXData<DXUnitRecord>
                {
                    Upsert = new List<DXUnitRecord> { record }
                }
            };
        }

        public static DXDataBlock<DXElementRecord> ToBlock(DXElement element, DXRecordWriteOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(element);

            options ??= new DXRecordWriteOptions();
            var context = GetUnitContext(null, element.GetType()) ?? options.DXUnitContext;
            if (string.IsNullOrWhiteSpace(context))
            {
                throw new InvalidOperationException(
                    "DXUnitContext is required when serializing a standalone DXElement block.");
            }

            options.DXUnitContext = context;

            var record = BuildElementRecord(element, element.DXUnitID);
            var meta = BuildSingleElementMeta(element.GetType(), options, isStandalone: true);

            return new DXDataBlock<DXElementRecord>
            {
                Meta = meta,
                Data = new DXData<DXElementRecord>
                {
                    Upsert = new List<DXElementRecord> { record }
                }
            };
        }

        public static DXDataBlock<DXEnumRecord> ToBlock(IEnumerable<DXEnumItem> items, DXRecordWriteOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(items);

            var records = items.Select(ToEnumRecord).ToList();
            var meta = BuildEnumMeta(records, options);

            return new DXDataBlock<DXEnumRecord>
            {
                Meta = meta,
                Data = new DXData<DXEnumRecord>
                {
                    Upsert = records
                }
            };
        }

        private static DXUnitRecord BuildUnitRecord(DXUnit unit, DXRecordWriteOptions? options)
        {
            var record = new DXUnitRecord
            {
                ID = unit.ID,
                TimeStamp = unit.TimeStamp,
                Fields = ReadScalarFields(unit),
                DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
            };

            ApplySingleElements(unit, record, options);
            ApplyMultiElements(unit, record, options);

            return record;
        }

        private static void ApplySingleElements(DXUnit unit, DXUnitRecord record, DXRecordWriteOptions? options)
        {
            foreach (var prop in GetSingleElementProps(unit.GetType()))
            {
                var element = prop.GetValue(unit) as DXElement;
                if (element == null) continue;

                var elementTypeName = AttributeReader.GetDXElementTypeName(element.GetType());
                if (string.IsNullOrWhiteSpace(elementTypeName))
                    elementTypeName = prop.Name;

                record.DXElements[elementTypeName] = BuildSingleElementBlock(element, options, prop);
            }
        }

        private static void ApplyMultiElements(DXUnit unit, DXUnitRecord record, DXRecordWriteOptions? options)
        {
            foreach (var prop in GetMultiElementProps(unit.GetType()))
            {
                var container = prop.GetValue(unit);
                if (container == null) continue;

                var elementType = prop.PropertyType.GetGenericArguments()[0];
                var elementTypeName = AttributeReader.GetDXElementTypeName(elementType);
                if (string.IsNullOrWhiteSpace(elementTypeName))
                    elementTypeName = prop.Name;

                var block = BuildMultiElementBlock(container, elementType, unit.ID, options, prop, elementTypeName);
                record.DXElements[elementTypeName] = block;
            }
        }

        private static DXDataBlock<DXElementRecord> BuildSingleElementBlock(
            DXElement element,
            DXRecordWriteOptions? options,
            PropertyInfo? property)
        {
            var meta = BuildSingleElementMeta(element.GetType(), options, isStandalone: false, property);

            return new DXDataBlock<DXElementRecord>
            {
                Meta = meta,
                Data = new DXData<DXElementRecord>
                {
                    Upsert = new List<DXElementRecord> { BuildElementRecord(element, element.DXUnitID) }
                }
            };
        }

        private static DXDataBlock<DXElementRecord> BuildMultiElementBlock(
            object container,
            Type elementType,
            Guid unitId,
            DXRecordWriteOptions? options,
            PropertyInfo? property,
            string elementTypeName)
        {
            var mode = GetContainerMode(container);
            var meta = BuildMultiElementMeta(elementType, mode, options, isStandalone: false, property, elementTypeName);

            var upsert = new List<DXElementRecord>();
            foreach (var element in GetContainerElements(container, Constants.Announced))
            {
                upsert.Add(BuildElementRecord(element, unitId));
            }

            var delete = new List<DXDeleteRef>();
            foreach (var element in GetContainerElements(container, Constants.Deleted))
            {
                delete.Add(BuildDeleteRef(element, unitId, options));
            }

            return new DXDataBlock<DXElementRecord>
            {
                Meta = meta,
                Data = new DXData<DXElementRecord>
                {
                    Upsert = upsert.Count == 0 ? null : upsert,
                    Delete = delete.Count == 0 ? null : delete
                }
            };
        }

        private static DXElementRecord BuildElementRecord(DXElement element, Guid unitId)
        {
            return new DXElementRecord
            {
                ID = element.ID,
                TimeStamp = element.TimeStamp,
                DXUnitID = element.DXUnitID == Guid.Empty ? unitId : element.DXUnitID,
                Fields = ReadScalarFields(element)
            };
        }

        private static DXDeleteRef BuildDeleteRef(DXElement element, Guid unitId, DXRecordWriteOptions? options)
        {
            var delete = new DXDeleteRef
            {
                ID = element.ID
            };

            if (options?.IncludeDeleteFields == true)
            {
                var fields = ReadScalarFields(element);
                if (fields != null)
                    delete.Fields = fields;
                else if (element.DXUnitID != Guid.Empty || unitId != Guid.Empty)
                    delete.Fields = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase)
                    {
                        { Constants.DXUnitID, JToken.FromObject(element.DXUnitID == Guid.Empty ? unitId : element.DXUnitID) }
                    };
            }

            return delete;
        }

        private static DXEnumRecord ToEnumRecord(DXEnumItem item)
        {
            return new DXEnumRecord
            {
                ID = item.ID,
                TimeStamp = item.TimeStamp,
                Type = item.Type,
                Key = item.Key == null ? null : JToken.FromObject(item.Key),
                Value = item.Value == null ? null : JToken.FromObject(item.Value)
            };
        }

        private static DXMeta BuildUnitMeta(DXUnit unit, DXRecordWriteOptions? options)
        {
            var meta = new DXMeta
            {
                Kind = "DXUnit",
                Type = AttributeReader.GetDXUnitTypeName(unit.GetType()),
                Op = options?.Op,
                IsMulti = options?.IsMulti,
                IsRequired = options?.IsRequired,
                DXFilter = options?.DXFilter
            };

            var requiredAttr = AttributeReader.GetAttribute<DXRequiredAttribute>(unit.GetType());
            if (requiredAttr != null && options?.IsRequired.HasValue != true)
                meta.IsRequired = requiredAttr.IsRequired;

            ApplyOverride(meta, options?.UnitMetaOverride);
            return meta;
        }

        private static DXMeta BuildSingleElementMeta(
            Type elementType,
            DXRecordWriteOptions? options,
            bool isStandalone,
            PropertyInfo? property = null)
        {
            var elementTypeName = AttributeReader.GetDXElementTypeName(elementType);

            var meta = new DXMeta
            {
                Kind = "DXElement",
                Type = elementTypeName,
                Op = options?.Op ?? "Patch",
                IsMulti = false
            };

            meta.IsRequired = GetRequired(property, elementType) ?? options?.IsRequired ?? false;

            if (isStandalone)
            {
                meta.DXUnitContext = GetUnitContext(property, elementType) ?? options?.DXUnitContext;
            }

            ApplyElementOverrides(meta, options, elementTypeName, property?.Name);
            return meta;
        }

        private static DXMeta BuildMultiElementMeta(
            Type elementType,
            MultiElementsMode mode,
            DXRecordWriteOptions? options,
            bool isStandalone,
            PropertyInfo? property,
            string elementTypeName)
        {
            var meta = new DXMeta
            {
                Kind = "DXElement",
                Type = elementTypeName,
                Op = MapMode(mode),
                IsMulti = true
            };

            meta.IsRequired = GetRequired(property, elementType) ?? options?.IsRequired ?? false;

            if (isStandalone)
            {
                meta.DXUnitContext = GetUnitContext(property, elementType) ?? options?.DXUnitContext;
            }

            ApplyElementOverrides(meta, options, elementTypeName, property?.Name);
            return meta;
        }

        private static DXMeta BuildEnumMeta(IReadOnlyCollection<DXEnumRecord> records, DXRecordWriteOptions? options)
        {
            string? commonType = null;
            if (records.Count > 0)
            {
                var types = records
                    .Select(x => x.Type)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (types.Count == 1)
                    commonType = types[0];
            }

            var meta = new DXMeta
            {
                Kind = "DXEnum",
                Type = options?.EnumMetaType ?? commonType,
                Op = options?.Op,
                IsMulti = options?.IsMulti,
                IsRequired = options?.IsRequired
            };

            ApplyOverride(meta, options?.EnumMetaOverride);
            return meta;
        }

        private static void ApplyElementOverrides(
            DXMeta meta,
            DXRecordWriteOptions? options,
            string elementTypeName,
            string? propertyName)
        {
            if (options?.ElementMetaOverrides == null)
                return;

            if (options.ElementMetaOverrides.TryGetValue(elementTypeName, out var overrideMeta))
            {
                ApplyOverride(meta, overrideMeta);
                return;
            }

            if (!string.IsNullOrWhiteSpace(propertyName)
                && options.ElementMetaOverrides.TryGetValue(propertyName, out overrideMeta))
            {
                ApplyOverride(meta, overrideMeta);
            }
        }

        private static void ApplyOverride(DXMeta target, DXMeta? overrideMeta)
        {
            if (overrideMeta == null) return;

            if (!string.IsNullOrWhiteSpace(overrideMeta.Kind)) target.Kind = overrideMeta.Kind;
            if (!string.IsNullOrWhiteSpace(overrideMeta.Type)) target.Type = overrideMeta.Type;
            if (overrideMeta.Op != null) target.Op = overrideMeta.Op;
            if (overrideMeta.IsMulti.HasValue) target.IsMulti = overrideMeta.IsMulti;
            if (overrideMeta.IsRequired.HasValue) target.IsRequired = overrideMeta.IsRequired;
            if (overrideMeta.DXFilter != null) target.DXFilter = overrideMeta.DXFilter;
            if (overrideMeta.DXUnitContext != null) target.DXUnitContext = overrideMeta.DXUnitContext;
        }

        private static bool? GetRequired(PropertyInfo? property, Type elementType)
        {
            var propAttr = property == null ? null : AttributeReader.GetAttribute<DXRequiredAttribute>(property);
            if (propAttr != null)
                return propAttr.IsRequired;

            var typeAttr = AttributeReader.GetAttribute<DXRequiredAttribute>(elementType);
            return typeAttr?.IsRequired;
        }

        private static string? GetUnitContext(PropertyInfo? property, Type elementType)
        {
            var propAttr = property == null ? null : AttributeReader.GetAttribute<DXUnitContextAttribute>(property);
            if (propAttr != null)
                return propAttr.ContextTypeName;

            var typeAttr = AttributeReader.GetAttribute<DXUnitContextAttribute>(elementType);
            return typeAttr?.ContextTypeName;
        }

        private static Dictionary<string, JToken>? ReadScalarFields(object obj)
        {
            var props = GetColumnProps(obj.GetType());
            if (props.Length == 0) return null;

            Dictionary<string, JToken>? result = null;

            foreach (var prop in props)
            {
                var attr = AttributeReader.GetAttribute<DXColumnAttribute>(prop);
                if (attr == null) continue;

                if (IsSystemField(attr.Name))
                    continue;

                var value = prop.GetValue(obj);

                result ??= new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
                result[attr.Name] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
            }

            return result;
        }

        private static bool IsSystemField(string fieldName)
        {
            return fieldName == Constants.ID
                   || fieldName == Constants.TimeStamp
                   || fieldName == Constants.DXUnitID;
        }

        private static MultiElementsMode GetContainerMode(object container)
        {
            var prop = container.GetType().GetProperty(Constants.Mode);
            if (prop?.GetValue(container) is MultiElementsMode mode)
                return mode;

            return MultiElementsMode.Full;
        }

        private static IEnumerable<DXElement> GetContainerElements(object container, string propertyName)
        {
            var prop = container.GetType().GetProperty(propertyName);
            if (prop?.GetValue(container) is not IEnumerable items)
                yield break;

            foreach (var item in items)
            {
                if (item is DXElement element)
                    yield return element;
            }
        }

        private static PropertyInfo[] GetColumnProps(Type type) =>
            ColumnPropsCache.GetOrAdd(type, t =>
                t.GetProperties().Where(p => AttributeReader.GetAttribute<DXColumnAttribute>(p) != null).ToArray());

        private static PropertyInfo[] GetSingleElementProps(Type type) =>
            SingleElementPropsCache.GetOrAdd(type, t => AttributeReader.GetSingleItemInfos(t).ToArray());

        private static PropertyInfo[] GetMultiElementProps(Type type) =>
            MultiElementPropsCache.GetOrAdd(type, t => AttributeReader.GetMultiItemInfos(t).ToArray());

        private static string? MapMode(MultiElementsMode mode)
        {
            return mode == MultiElementsMode.Target ? "Patch" : "Sync";
        }
    }

    internal sealed class DXRecordWriteOptions
    {
        public string? Op { get; set; }
        public bool? IsMulti { get; set; }
        public bool? IsRequired { get; set; }
        public string? DXFilter { get; set; }
        public string? DXUnitContext { get; set; }

        public string? EnumMetaType { get; set; }

        public DXMeta? UnitMetaOverride { get; set; }
        public DXMeta? EnumMetaOverride { get; set; }
        public Dictionary<string, DXMeta>? ElementMetaOverrides { get; set; }

        public bool IncludeDeleteFields { get; set; }
    }
}
