using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IV.DX.Kernel.Converters.DXObjectConverters
{
    internal static class DXRecordConverter
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ColumnPropsCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> SingleElementPropsCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> MultiElementPropsCache = new();

        private static readonly MethodInfo BuildContainerMethod =
            typeof(DXRecordConverter).GetMethod(nameof(BuildContainerGeneric), BindingFlags.NonPublic | BindingFlags.Static)!;

        public static IEnumerable<T> ToDXUnits<T>(string json) where T : DXUnit
        {
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<T>();

            var blocks = JsonConvert.DeserializeObject<List<DXDataBlock<DXUnitRecord>>>(json);
            return ToDXUnits<T>(blocks);
        }

        public static IEnumerable<T> ToDXUnits<T>(IEnumerable<DXDataBlock<DXUnitRecord>>? blocks) where T : DXUnit
        {
            if (blocks == null)
                yield break;

            foreach (var block in blocks)
            {
                var items = block?.Data?.Upsert;
                if (items == null) continue;

                foreach (var record in items)
                {
                    var unit = (T)ToDXUnit(record, typeof(T));
                    yield return unit;
                }
            }
        }

        public static DXUnit ToDXUnit(DXUnitRecord record, Type unitType)
        {
            var unit = (DXUnit)Activator.CreateInstance(unitType)!;
            unit.ID = record.ID;
            unit.TimeStamp = record.TimeStamp;

            ApplyFields(unit, record.Fields);
            ApplyElements(unit, record.DXElements);

            return unit;
        }

        public static IEnumerable<DXEnumItem> ToDXEnums(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<DXEnumItem>();

            var blocks = JsonConvert.DeserializeObject<List<DXDataBlock<DXEnumRecord>>>(json);
            return ToDXEnums(blocks);
        }

        public static IEnumerable<DXEnumItem> ToDXEnums(IEnumerable<DXDataBlock<DXEnumRecord>>? blocks)
        {
            if (blocks == null)
                yield break;

            foreach (var block in blocks)
            {
                var items = block?.Data?.Upsert;
                if (items == null) continue;

                foreach (var record in items)
                {
                    yield return ToDXEnum(record, block.Meta?.Type);
                }
            }
        }

        private static DXEnumItem ToDXEnum(DXEnumRecord record, string? fallbackType)
        {
            return new DXEnumItem
            {
                ID = record.ID,
                TimeStamp = record.TimeStamp,
                Type = record.Type ?? fallbackType ?? string.Empty,
                Key = ConvertTokenToObject(record.Key),
                Value = ConvertTokenToObject(record.Value)
            };
        }

        private static void ApplyFields(object target, IDictionary<string, JToken>? fields)
        {
            if (fields == null || fields.Count == 0)
                return;

            var fieldMap = fields is Dictionary<string, JToken> dict && dict.Comparer == StringComparer.OrdinalIgnoreCase
                ? dict
                : new Dictionary<string, JToken>(fields, StringComparer.OrdinalIgnoreCase);

            foreach (var prop in GetColumnProps(target.GetType()))
            {
                var attr = AttributeReader.GetAttribute<DXColumnAttribute>(prop);
                if (attr == null) continue;

                if (!fieldMap.TryGetValue(attr.Name, out var token)) continue;

                if (token.Type == JTokenType.Null)
                {
                    if (IsNullable(prop.PropertyType))
                        prop.SetValue(target, null);
                    continue;
                }

                var value = token.ToObject(prop.PropertyType);
                prop.SetValue(target, value);
            }
        }

        private static void ApplyElements(DXUnit unit, IDictionary<string, DXDataBlock<DXElementRecord>>? elements)
        {
            if (elements == null || elements.Count == 0)
                return;

            var elementMap = elements is Dictionary<string, DXDataBlock<DXElementRecord>> dict
                && dict.Comparer == StringComparer.OrdinalIgnoreCase
                    ? dict
                    : new Dictionary<string, DXDataBlock<DXElementRecord>>(elements, StringComparer.OrdinalIgnoreCase);

            ApplySingleElements(unit, elementMap);
            ApplyMultiElements(unit, elementMap);
        }

        private static void ApplySingleElements(DXUnit unit, Dictionary<string, DXDataBlock<DXElementRecord>> elementMap)
        {
            foreach (var prop in GetSingleElementProps(unit.GetType()))
            {
                var elementType = prop.PropertyType;
                var elementTypeName = AttributeReader.GetDXElementTypeName(elementType);

                if (!TryGetElementBlock(elementMap, elementTypeName, prop.Name, out var block))
                    continue;

                var record = block?.Data?.Upsert?.FirstOrDefault();
                if (record == null) continue;

                var element = (DXElement)CreateElement(elementType, record, unit.ID);
                prop.SetValue(unit, element);
            }
        }

        private static void ApplyMultiElements(DXUnit unit, Dictionary<string, DXDataBlock<DXElementRecord>> elementMap)
        {
            foreach (var prop in GetMultiElementProps(unit.GetType()))
            {
                var elementType = prop.PropertyType.GetGenericArguments()[0];
                var elementTypeName = AttributeReader.GetDXElementTypeName(elementType);

                if (!TryGetElementBlock(elementMap, elementTypeName, prop.Name, out var block))
                    continue;

                var container = BuildContainer(elementType, block!, unit.ID);
                prop.SetValue(unit, container);
            }
        }

        private static bool TryGetElementBlock(
            Dictionary<string, DXDataBlock<DXElementRecord>> elementMap,
            string elementTypeName,
            string propertyName,
            out DXDataBlock<DXElementRecord>? block)
        {
            if (elementMap.TryGetValue(elementTypeName, out block))
                return true;

            if (elementMap.TryGetValue(propertyName, out block))
                return true;

            block = null;
            return false;
        }

        private static object BuildContainer(Type elementType, DXDataBlock<DXElementRecord> block, Guid unitId)
        {
            var method = BuildContainerMethod.MakeGenericMethod(elementType);
            return method.Invoke(null, new object[] { block, unitId })!;
        }

        private static DXMultiElementsContainer<TElement> BuildContainerGeneric<TElement>(
            DXDataBlock<DXElementRecord> block,
            Guid unitId) where TElement : DXElement
        {
            var container = new DXMultiElementsContainer<TElement>();
            var mode = MapMode(block.Meta?.Op);
            if (mode != null)
                container.Mode = mode.Value;

            if (block.Data?.Upsert != null)
            {
                foreach (var record in block.Data.Upsert)
                {
                    var element = CreateElementGeneric<TElement>(record, unitId);
                    container.Announced.Add(element);
                }
            }

            if (block.Data?.Delete != null)
            {
                foreach (var deleteRef in block.Data.Delete)
                {
                    var element = CreateElementFromDeleteGeneric<TElement>(deleteRef, unitId);
                    container.Deleted.Add(element);
                }
            }

            return container;
        }

        private static DXElement CreateElement(Type elementType, DXElementRecord record, Guid unitId)
        {
            var element = (DXElement)Activator.CreateInstance(elementType)!;
            element.ID = record.ID;
            element.TimeStamp = record.TimeStamp;
            element.DXUnitID = record.DXUnitID == Guid.Empty ? unitId : record.DXUnitID;

            ApplyFields(element, record.Fields);
            return element;
        }

        private static TElement CreateElementGeneric<TElement>(DXElementRecord record, Guid unitId) where TElement : DXElement
        {
            var element = (TElement)Activator.CreateInstance(typeof(TElement))!;
            element.ID = record.ID;
            element.TimeStamp = record.TimeStamp;
            element.DXUnitID = record.DXUnitID == Guid.Empty ? unitId : record.DXUnitID;

            ApplyFields(element, record.Fields);
            return element;
        }

        private static TElement CreateElementFromDeleteGeneric<TElement>(DXDeleteRef deleteRef, Guid unitId) where TElement : DXElement
        {
            var element = (TElement)Activator.CreateInstance(typeof(TElement))!;
            element.ID = deleteRef.ID;

            ApplyFields(element, deleteRef.Fields);

            if (element.DXUnitID == Guid.Empty)
                element.DXUnitID = unitId;

            return element;
        }

        private static PropertyInfo[] GetColumnProps(Type type) =>
            ColumnPropsCache.GetOrAdd(type, t =>
                t.GetProperties().Where(p => AttributeReader.GetAttribute<DXColumnAttribute>(p) != null).ToArray());

        private static PropertyInfo[] GetSingleElementProps(Type type) =>
            SingleElementPropsCache.GetOrAdd(type, t => AttributeReader.GetSingleItemInfos(t).ToArray());

        private static PropertyInfo[] GetMultiElementProps(Type type) =>
            MultiElementPropsCache.GetOrAdd(type, t => AttributeReader.GetMultiItemInfos(t).ToArray());

        private static MultiElementsMode? MapMode(string? op)
        {
            if (string.IsNullOrWhiteSpace(op))
                return null;

            if (op.Equals("Patch", StringComparison.OrdinalIgnoreCase))
                return MultiElementsMode.Target;

            if (op.Equals("Sync", StringComparison.OrdinalIgnoreCase))
                return MultiElementsMode.Full;

            return null;
        }

        private static bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true;
            return Nullable.GetUnderlyingType(type) != null;
        }

        private static object? ConvertTokenToObject(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            return token.ToObject<object>();
        }
    }
}
