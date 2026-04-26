using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Mappers;
using IV.DX.Application.Services;
using IV.DX.Kernel.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Hosting
{
    public static class DXMapperServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a custom mapper and its <see cref="IDXUnitDtoService{TDto}"/>.
        /// <para>TMapper must derive from <c>DXUnitMapper&lt;TDto, TUnit&gt;</c> and implement both directions.</para>
        /// </summary>
        public static IServiceCollection AddDXUnitMapper<TMapper>(this IServiceCollection services)
            where TMapper : class
        {
            var (dtoType, unitType) = ResolveMapperTypeArgs(typeof(TMapper));

            services.AddTransient<TMapper>();
            RegisterDtoService(services, dtoType, unitType, typeof(TMapper));

            return services;
        }

        /// <summary>
        /// Registers a convention mapper for <typeparamref name="TDto"/> → <typeparamref name="TUnit"/>.
        /// <para>
        /// No mapper class needed. Properties are matched by name (case-insensitive).
        /// <c>List&lt;T&gt;</c> ↔ <c>DXMultiElementsContainer&lt;T&gt;</c> is supported when T matches exactly.
        /// All DTO properties must have a matching unit property — validation runs at startup.
        /// </para>
        /// </summary>
        public static IServiceCollection AddDXUnitMapper<TDto, TUnit>(this IServiceCollection services)
            where TUnit : DXUnit, new()
        {
            // Trigger static field init — throws InvalidOperationException at startup if mapping is invalid
            DXConventionMapper<TDto, TUnit>.Validate();

            services.AddTransient<DXConventionMapper<TDto, TUnit>>();
            services.AddScoped<IDXUnitDtoService<TDto>,
                DXUnitDtoService<TDto, TUnit, DXConventionMapper<TDto, TUnit>>>();

            return services;
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        private static (Type dtoType, Type unitType) ResolveMapperTypeArgs(Type mapperType)
        {
            var current = mapperType;
            while (current != null)
            {
                if (current.IsGenericType &&
                    current.GetGenericTypeDefinition() == typeof(DXUnitMapper<,>))
                {
                    var args = current.GetGenericArguments();
                    return (args[0], args[1]);
                }
                current = current.BaseType;
            }

            throw new InvalidOperationException(
                $"'{mapperType.Name}' must derive from DXUnitMapper<TDto, TUnit>. " +
                $"Ensure your mapper class inherits DXUnitMapper<TDto, TUnit> with concrete type arguments.");
        }

        private static void RegisterDtoService(
            IServiceCollection services,
            Type dtoType,
            Type unitType,
            Type mapperType)
        {
            var serviceType = typeof(IDXUnitDtoService<>).MakeGenericType(dtoType);
            var implType = typeof(DXUnitDtoService<,,>).MakeGenericType(dtoType, unitType, mapperType);
            services.AddScoped(serviceType, implType);
        }
    }
}
