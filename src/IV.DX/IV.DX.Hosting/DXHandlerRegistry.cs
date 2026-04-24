using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Pipeline;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace IV.DX.Hosting
{
    public static class DXHandlerRegistry
    {
        public static IServiceCollection AddDXHandlers(
            this IServiceCollection services,
            params Assembly[] scanAssemblies)
        {
            var handlerTypes = DXUnitHandlerScanner.FindHandlerTypes(scanAssemblies);
            foreach (var ht in handlerTypes)
                services.AddTransient(ht); // AddScoped(ht)
                     
            services.AddSingleton<IReadOnlyList<Assembly>>(scanAssemblies);
            return services;
        }

        /// <summary>Drops the DX database. Intended for test teardown scenarios.</summary>
        public static Task DropDXDatabaseAsync(this IServiceProvider root)
        {
            return Task.Run(() =>
            {
                using var scope = root.CreateScope();
                var coreRepo = scope.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
                coreRepo.DropDataBase();
            });
        }

        /// <summary>Runs embedded migration scripts from the given assembly and refreshes the structure cache.</summary>
        public static async Task InitCustomEmbeddedDataAsync(this IServiceProvider root, Assembly assembly, string listPath, CancellationToken ct = default)
        {
            using var scope = root.CreateScope();
            var migration = scope.ServiceProvider.GetRequiredService<IDXMigrationService>();
            var cache = scope.ServiceProvider.GetRequiredService<IDXStructureCache>();

            await migration.MigrateCustomEmbeddedAsync(assembly, listPath, ct);
            await cache.RefreshAsync(ct);
        }

        public static async Task StartDXAsync(this IServiceProvider root, CancellationToken ct = default)
        {
            root.InitializeDXHandlers();

            var options = root.GetRequiredService<IOptions<DXStartupOptions>>().Value;

            using var scope = root.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IDXInitializer>();

            await initializer.InitAsync(ct);

            if (options.SecurityEnabled)
                await initializer.InitDXSecurityDataAsync(ct);

            foreach (var path in options.CustomDataPaths)
                await initializer.InitCustomDataAsync(path, ct);
        }

        public static void InitializeDXHandlers(this IServiceProvider root, params Assembly[] scanAssemblies)
        {
            var assemblies = (scanAssemblies is { Length: > 0 })
                ? scanAssemblies
                : root.GetServices<IReadOnlyList<Assembly>>().SelectMany(x => x).Distinct().ToArray();

            var handlerTypes = DXUnitHandlerScanner.FindHandlerTypes(assemblies);
            using var scope = root.CreateScope();

            var sp = scope.ServiceProvider;
            var insertProv = sp.GetRequiredService<IDXUnitInsertHandlerProvider>();
            var updateProv = sp.GetRequiredService<IDXUnitUpdateHandlerProvider>();
            var deleteProv = sp.GetRequiredService<IDXUnitDeleteHandlerProvider>();
            var getProv = sp.GetRequiredService<IDXUnitGetHandlerProvider>();

            foreach (var hType in handlerTypes)
            {
                var handler = sp.GetRequiredService(hType);

                foreach (var (_, openIface, unitType) in DXUnitHandlerScanner.EnumerateDxHandlerInterfaces(hType))
                {
                    var closed = openIface.MakeGenericType(unitType);

                    RegisterOnProperProvider(insertProv, nameof(insertProv.Register), closed, handler);
                    RegisterOnProperProvider(updateProv, nameof(updateProv.Register), closed, handler);
                    RegisterOnProperProvider(deleteProv, nameof(deleteProv.Register), closed, handler);
                    RegisterOnProperProvider(getProv, nameof(getProv.Register), closed, handler);
                }
            }

            static void RegisterOnProperProvider(object provider, string registerName, Type handlerIface, object handlerInstance)
            {
                var mi = provider.GetType().GetMethods()
                    .FirstOrDefault(m => m.Name == registerName &&
                                         m.IsGenericMethodDefinition &&
                                         m.GetParameters().Length == 1 &&
                                         m.GetParameters()[0].ParameterType.IsGenericType &&
                                         m.GetParameters()[0].ParameterType.GetGenericTypeDefinition()
                                            == handlerIface.GetGenericTypeDefinition());
                if (mi is null) return;

                var t = handlerIface.GetGenericArguments()[0];
                mi.MakeGenericMethod(t).Invoke(provider, new[] { handlerInstance });
            }
        }
    }
}
