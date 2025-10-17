using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Hosting
{
    public static class DXInitServiceCollectionExtensions
    {
        public static IServiceCollection AddDXInitializer(this IServiceCollection services)
        {
            services.AddScoped<IDXInitializer, DXInitializer>();
            return services;
        }
    }
}