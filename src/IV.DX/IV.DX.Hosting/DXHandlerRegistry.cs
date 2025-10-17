using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;
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

        public static void InitializeDXHandlers(this IServiceProvider root, params Assembly[] scanAssemblies)
        {
            var assemblies = (scanAssemblies is { Length: > 0 })
                ? scanAssemblies
                : root.GetRequiredService<IReadOnlyList<Assembly>>()?.ToArray() ?? Array.Empty<Assembly>();

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
