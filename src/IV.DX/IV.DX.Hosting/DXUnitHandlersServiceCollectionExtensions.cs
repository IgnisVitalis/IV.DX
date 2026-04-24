using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IV.DX.Hosting
{
    internal static class DXUnitHandlersServiceCollectionExtensions
    {
        public static IServiceCollection AddDxHandlers(
            this IServiceCollection services,
            params Assembly[] scanAssemblies)
        {
            services.AddSingleton<DXUnitGetHandlerStore>();
            services.AddSingleton<DXUnitInsertHandlerStore>();
            services.AddSingleton<DXUnitUpdateHandlerStore>();
            services.AddSingleton<DXUnitDeleteHandlerStore>();

            services.AddScoped<IDXUnitInsertHandlerProvider, DXUnitInsertHandlerProvider>();
            services.AddScoped<IDXUnitUpdateHandlerProvider, DXUnitUpdateHandlerProvider>();
            services.AddScoped<IDXUnitDeleteHandlerProvider, DXUnitDeleteHandlerProvider>();
            services.AddScoped<IDXUnitGetHandlerProvider, DXUnitGetHandlerProvider>();

            var handlerTypes = DXUnitHandlerScanner.FindHandlerTypes(scanAssemblies);
            foreach (var ht in handlerTypes)
            {
                services.AddTransient(ht);
            }

            services.AddSingleton<IEnumerable<Assembly>>(scanAssemblies);
            services.AddHostedService<DXUnitHandlerRegistryInitializer>();

            return services;
        }
    }
}
