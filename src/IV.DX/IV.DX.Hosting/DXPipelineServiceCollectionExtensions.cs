using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Hosting
{
    internal static class DXPipelineServiceCollectionExtensions
    {
        internal static IServiceCollection AddDXPipeline(this IServiceCollection services)
        {
            services.AddSingleton<DXUnitGetHandlerStore>();
            services.AddSingleton<DXUnitInsertHandlerStore>();
            services.AddSingleton<DXUnitUpdateHandlerStore>();
            services.AddSingleton<DXUnitDeleteHandlerStore>();

            services.AddScoped<IDXUnitGetHandlerProvider, DXUnitGetHandlerProvider>();
            services.AddScoped<IDXUnitInsertHandlerProvider, DXUnitInsertHandlerProvider>();
            services.AddScoped<IDXUnitUpdateHandlerProvider, DXUnitUpdateHandlerProvider>();
            services.AddScoped<IDXUnitDeleteHandlerProvider, DXUnitDeleteHandlerProvider>();

            services.AddScoped<IDXPipelineExecutor, DXPipelineExecutor>();

            return services;
        }
    }
}
