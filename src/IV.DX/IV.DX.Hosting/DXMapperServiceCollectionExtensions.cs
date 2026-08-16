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
        /// Registers a full CRUD mapper and its <see cref="IDXUnitDtoService{TRequest, TResponse}"/>.
        /// <para>TMapper must derive from <c>DXUnitMapper&lt;TRequest, TResponse, TUnit&gt;</c>.</para>
        /// </summary>
        public static IServiceCollection AddDXUnitMapper<TMapper>(this IServiceCollection services)
            where TMapper : class
        {
            var (requestType, responseType, unitType) = ResolveFullMapperTypeArgs(typeof(TMapper));

            services.AddTransient<TMapper>();
            RegisterDtoService(services, requestType, responseType, unitType, typeof(TMapper));

            return services;
        }

        /// <summary>
        /// Registers a read-only mapper and its <see cref="IDXUnitQueryService{TResponse}"/>.
        /// <para>TMapper must derive from <c>DXUnitReadMapper&lt;TResponse, TUnit&gt;</c>.</para>
        /// </summary>
        public static IServiceCollection AddDXUnitReadMapper<TMapper>(this IServiceCollection services)
            where TMapper : class
        {
            var (responseType, unitType) = ResolveReadMapperTypeArgs(typeof(TMapper));

            services.AddTransient<TMapper>();
            RegisterQueryService(services, responseType, unitType, typeof(TMapper));

            return services;
        }

        /// <summary>
        /// Registers a write-only mapper and its <see cref="IDXUnitCommandService{TRequest}"/>.
        /// <para>TMapper must derive from <c>DXUnitWriteMapper&lt;TRequest, TUnit&gt;</c>.</para>
        /// </summary>
        public static IServiceCollection AddDXUnitWriteMapper<TMapper>(this IServiceCollection services)
            where TMapper : class
        {
            var (requestType, unitType) = ResolveWriteMapperTypeArgs(typeof(TMapper));

            services.AddTransient<TMapper>();
            RegisterCommandService(services, requestType, unitType, typeof(TMapper));

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
            services.AddScoped<IDXUnitDtoService<TDto, TDto>,
                DXUnitDtoService<TDto, TDto, TUnit, DXConventionMapper<TDto, TUnit>>>();

            return services;
        }

        /// <summary>
        /// Registers a full CRUD element mapper and its <see cref="IDXElementDtoService{TRequest, TResponse}"/>.
        /// <para>TMapper must derive from <c>DXElementMapper&lt;TRequest, TResponse, TElement, TUnit&gt;</c>.</para>
        /// </summary>
        public static IServiceCollection AddDXElementMapper<TMapper>(this IServiceCollection services)
            where TMapper : class
        {
            var args = ResolveMapperTypeArgs(typeof(TMapper), typeof(DXElementMapper<,,,>),
                "DXElementMapper<TRequest, TResponse, TElement, TUnit>");

            services.AddTransient<TMapper>();

            var serviceType = typeof(IDXElementDtoService<,>).MakeGenericType(args[0], args[1]);
            var implType = typeof(DXElementDtoService<,,,,>).MakeGenericType(args[0], args[1], args[2], args[3], typeof(TMapper));
            services.AddScoped(serviceType, implType);

            return services;
        }

        /// <summary>
        /// Registers a read-only element mapper and its <see cref="IDXElementQueryService{TResponse}"/>.
        /// <para>TMapper must derive from <c>DXElementReadMapper&lt;TResponse, TElement, TUnit&gt;</c>.</para>
        /// </summary>
        public static IServiceCollection AddDXElementReadMapper<TMapper>(this IServiceCollection services)
            where TMapper : class
        {
            var args = ResolveMapperTypeArgs(typeof(TMapper), typeof(DXElementReadMapper<,,>),
                "DXElementReadMapper<TResponse, TElement, TUnit>");

            services.AddTransient<TMapper>();

            var serviceType = typeof(IDXElementQueryService<>).MakeGenericType(args[0]);
            var implType = typeof(DXElementQueryService<,,,>).MakeGenericType(args[0], args[1], args[2], typeof(TMapper));
            services.AddScoped(serviceType, implType);

            return services;
        }

        /// <summary>
        /// Registers a write-only element mapper and its <see cref="IDXElementCommandService{TRequest}"/>.
        /// <para>TMapper must derive from <c>DXElementWriteMapper&lt;TRequest, TElement, TUnit&gt;</c>.</para>
        /// </summary>
        public static IServiceCollection AddDXElementWriteMapper<TMapper>(this IServiceCollection services)
            where TMapper : class
        {
            var args = ResolveMapperTypeArgs(typeof(TMapper), typeof(DXElementWriteMapper<,,>),
                "DXElementWriteMapper<TRequest, TElement, TUnit>");

            services.AddTransient<TMapper>();

            var serviceType = typeof(IDXElementCommandService<>).MakeGenericType(args[0]);
            var implType = typeof(DXElementCommandService<,,,>).MakeGenericType(args[0], args[1], args[2], typeof(TMapper));
            services.AddScoped(serviceType, implType);

            return services;
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Walks up from a mapper class to the generic base it is expected to close, and returns that
        /// base's type arguments.
        /// </summary>
        private static Type[] ResolveMapperTypeArgs(Type mapperType, Type openBaseType, string expected)
        {
            var current = mapperType;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == openBaseType)
                    return current.GetGenericArguments();

                current = current.BaseType;
            }

            throw new InvalidOperationException(
                $"'{mapperType.Name}' must derive from {expected}. " +
                $"Ensure your mapper class inherits {expected} with concrete type arguments.");
        }

        private static (Type requestType, Type responseType, Type unitType) ResolveFullMapperTypeArgs(Type mapperType)
        {
            var current = mapperType;
            while (current != null)
            {
                if (current.IsGenericType &&
                    current.GetGenericTypeDefinition() == typeof(DXUnitMapper<,,>))
                {
                    var args = current.GetGenericArguments();
                    return (args[0], args[1], args[2]);
                }
                current = current.BaseType;
            }

            throw new InvalidOperationException(
                $"'{mapperType.Name}' must derive from DXUnitMapper<TRequest, TResponse, TUnit>. " +
                $"Ensure your mapper class inherits DXUnitMapper<TRequest, TResponse, TUnit> with concrete type arguments.");
        }

        private static (Type responseType, Type unitType) ResolveReadMapperTypeArgs(Type mapperType)
        {
            var current = mapperType;
            while (current != null)
            {
                if (current.IsGenericType &&
                    current.GetGenericTypeDefinition() == typeof(DXUnitReadMapper<,>))
                {
                    var args = current.GetGenericArguments();
                    return (args[0], args[1]);
                }
                current = current.BaseType;
            }

            throw new InvalidOperationException(
                $"'{mapperType.Name}' must derive from DXUnitReadMapper<TResponse, TUnit>. " +
                $"Ensure your mapper class inherits DXUnitReadMapper<TResponse, TUnit> with concrete type arguments.");
        }

        private static (Type requestType, Type unitType) ResolveWriteMapperTypeArgs(Type mapperType)
        {
            var current = mapperType;
            while (current != null)
            {
                if (current.IsGenericType &&
                    current.GetGenericTypeDefinition() == typeof(DXUnitWriteMapper<,>))
                {
                    var args = current.GetGenericArguments();
                    return (args[0], args[1]);
                }
                current = current.BaseType;
            }

            throw new InvalidOperationException(
                $"'{mapperType.Name}' must derive from DXUnitWriteMapper<TRequest, TUnit>. " +
                $"Ensure your mapper class inherits DXUnitWriteMapper<TRequest, TUnit> with concrete type arguments.");
        }

        private static void RegisterDtoService(
            IServiceCollection services,
            Type requestType,
            Type responseType,
            Type unitType,
            Type mapperType)
        {
            var serviceType = typeof(IDXUnitDtoService<,>).MakeGenericType(requestType, responseType);
            var implType = typeof(DXUnitDtoService<,,,>).MakeGenericType(requestType, responseType, unitType, mapperType);
            services.AddScoped(serviceType, implType);
        }

        private static void RegisterQueryService(
            IServiceCollection services,
            Type responseType,
            Type unitType,
            Type mapperType)
        {
            var serviceType = typeof(IDXUnitQueryService<>).MakeGenericType(responseType);
            var implType = typeof(DXUnitQueryService<,,>).MakeGenericType(responseType, unitType, mapperType);
            services.AddScoped(serviceType, implType);
        }

        private static void RegisterCommandService(
            IServiceCollection services,
            Type requestType,
            Type unitType,
            Type mapperType)
        {
            var serviceType = typeof(IDXUnitCommandService<>).MakeGenericType(requestType);
            var implType = typeof(DXUnitCommandService<,,>).MakeGenericType(requestType, unitType, mapperType);
            services.AddScoped(serviceType, implType);
        }
    }
}
