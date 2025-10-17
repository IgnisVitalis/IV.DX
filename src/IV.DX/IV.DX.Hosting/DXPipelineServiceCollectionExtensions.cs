using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Hosting
{
    public static class DXPipelineServiceCollectionExtensions
    {
        public static IServiceCollection AddDXPipeline(this IServiceCollection services)
        {         
            services.AddSingleton<IDXUnitGetHandlerProvider, DXUnitGetHandlerProvider>();
            services.AddSingleton<IDXUnitInsertHandlerProvider, DXUnitInsertHandlerProvider>();
            services.AddSingleton<IDXUnitUpdateHandlerProvider, DXUnitUpdateHandlerProvider>();
            services.AddSingleton<IDXUnitDeleteHandlerProvider, DXUnitDeleteHandlerProvider>();

            services.AddScoped<IDXPipelineExecutor, DXPipelineExecutor>();

            return services;
        }
    }
}
