using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Hosting
{
    internal static class DXInitServiceCollectionExtensions
    {
        internal static IServiceCollection AddDXInitializer(this IServiceCollection services)
        {
            services.AddScoped<IDXInitializer, DXInitializer>();
            return services;
        }
    }
}