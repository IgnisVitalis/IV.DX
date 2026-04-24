using IV.DX.Application.Actions;
using IV.DX.Application.Contracts.Actions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace IV.DX.Hosting
{
    internal static class DXActionServiceCollectionExtensions
    {
        public static IServiceCollection AddDXActions(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            var actionTypes = DXActionScanner.FindActionTypes(assemblies).ToList();

            var registry = new DXActionRegistry();
            foreach (var type in actionTypes)
            {
                registry.Register(type);
                services.TryAddScoped(type);
            }

            services.TryAddSingleton<IDXActionRegistry>(registry);
            services.TryAddScoped<IDXActionExecutor, DXActionExecutor>();

            return services;
        }
    }
}
