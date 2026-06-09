using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using System.Collections;
using System.Reflection;

namespace IV.DX.Application.Mappers
{
    internal sealed class DXConventionMapper<TDto, TUnit> : DXUnitMapper<TDto, TDto, TUnit>
        where TUnit : DXUnit, new()
    {
        // Built once per closed generic type; throws at startup if mapping is invalid
        private static readonly IReadOnlyList<PropertyMap> Mappings = BuildMappings();

        // Called by AddDXUnitMapper<TDto, TUnit>() at registration time.
        // Calls BuildMappings() directly so InvalidOperationException propagates cleanly —
        // accessing the static field would wrap the error in TypeInitializationException instead.
        internal static void Validate() => BuildMappings();

        public override Task<TDto> ToDtoAsync(TUnit unit, CancellationToken ct = default)
        {
            var dto = Activator.CreateInstance<TDto>();
            foreach (var map in Mappings)
                map.ApplyToDto(unit, dto!);
            return Task.FromResult(dto!);
        }

        public override Task<TUnit> ToUnitAsync(TDto dto, CancellationToken ct = default)
        {
            var unit = new TUnit();
            foreach (var map in Mappings)
                map.ApplyToUnit(dto!, unit);
            return Task.FromResult(unit);
        }

        private static IReadOnlyList<PropertyMap> BuildMappings()
        {
            var dtoProps = typeof(TDto).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();

            var unitPropsByName = typeof(TUnit)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            var maps = new List<PropertyMap>(dtoProps.Length);
            var errors = new List<string>();

            foreach (var dtoProp in dtoProps)
            {
                if (!unitPropsByName.TryGetValue(dtoProp.Name, out var unitProp))
                {
                    errors.Add(
                        $"  '{typeof(TDto).Name}.{dtoProp.Name}' ({dtoProp.PropertyType.Name}) " +
                        $"has no matching property in '{typeof(TUnit).Name}'.");
                    continue;
                }

                var map = TryBuildMap(dtoProp, unitProp);
                if (map is null)
                {
                    errors.Add(
                        $"  Cannot map '{typeof(TDto).Name}.{dtoProp.Name}' ({dtoProp.PropertyType.Name}) " +
                        $"to '{typeof(TUnit).Name}.{unitProp.Name}' ({unitProp.PropertyType.Name}): " +
                        $"types are incompatible. Use a custom DXUnitMapper<TDto, TDto, TUnit> instead.");
                    continue;
                }

                maps.Add(map);
            }

            if (errors.Count > 0)
                throw new InvalidOperationException(
                    $"Convention mapping from '{typeof(TDto).Name}' to '{typeof(TUnit).Name}' failed:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, errors));

            return maps;
        }

        private static PropertyMap? TryBuildMap(PropertyInfo dtoProp, PropertyInfo unitProp)
        {
            // Exact type match — scalar or any same-type property
            if (dtoProp.PropertyType == unitProp.PropertyType)
                return new ScalarMap(dtoProp, unitProp);

            // List<TElement> <-> DXMultiElementsContainer<TElement> where TElement : DXElement
            if (IsListOf(dtoProp.PropertyType, out var dtoElementType) &&
                IsContainerOf(unitProp.PropertyType, out var unitElementType) &&
                dtoElementType == unitElementType)
            {
                return new ContainerMap(dtoProp, unitProp, dtoElementType!);
            }

            return null;
        }

        private static bool IsListOf(Type type, out Type? elementType)
        {
            elementType = null;
            if (!type.IsGenericType) return false;
            if (type.GetGenericTypeDefinition() != typeof(List<>)) return false;
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        private static bool IsContainerOf(Type type, out Type? elementType)
        {
            elementType = null;
            if (!type.IsGenericType) return false;
            if (type.GetGenericTypeDefinition() != typeof(DXMultiElementsContainer<>)) return false;
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        // ── Mapping strategies ─────────────────────────────────────────────────────

        private abstract class PropertyMap
        {
            public abstract void ApplyToDto(TUnit unit, TDto dto);
            public abstract void ApplyToUnit(TDto dto, TUnit unit);
        }

        private sealed class ScalarMap(PropertyInfo dtoProp, PropertyInfo unitProp) : PropertyMap
        {
            public override void ApplyToDto(TUnit unit, TDto dto)
                => dtoProp.SetValue(dto, unitProp.GetValue(unit));

            public override void ApplyToUnit(TDto dto, TUnit unit)
                => unitProp.SetValue(unit, dtoProp.GetValue(dto));
        }

        private sealed class ContainerMap(
            PropertyInfo dtoProp,
            PropertyInfo unitProp,
            Type elementType) : PropertyMap
        {
            // Cached to avoid repeated GetProperty/GetMethod lookups per call
            private static readonly string AnnouncedPropName =
                nameof(DXMultiElementsContainer<DXElement>.Announced);
            private static readonly string AddMethodName =
                nameof(DXMultiElementsContainer<DXElement>.AddToAnnounced);

            public override void ApplyToDto(TUnit unit, TDto dto)
            {
                var container = unitProp.GetValue(unit);
                if (container is null) return;

                var announced = container.GetType()
                    .GetProperty(AnnouncedPropName)
                    ?.GetValue(container) as IEnumerable;
                if (announced is null) return;

                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
                foreach (var item in announced)
                    list.Add(item);

                dtoProp.SetValue(dto, list);
            }

            public override void ApplyToUnit(TDto dto, TUnit unit)
            {
                if (dtoProp.GetValue(dto) is not IEnumerable items) return;

                var container = unitProp.GetValue(unit);
                if (container is null) return;

                var addMethod = container.GetType().GetMethod(AddMethodName);
                if (addMethod is null) return;

                foreach (var item in items)
                    addMethod.Invoke(container, [item]);
            }
        }
    }
}
