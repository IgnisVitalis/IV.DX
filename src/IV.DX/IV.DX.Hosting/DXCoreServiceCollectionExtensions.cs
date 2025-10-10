using IV.DX.Application;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.DataHandlers;
using IV.DX.Persistence;
using IV.DX.Persistence.Abstractions;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.SQLQueryHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IV.DX.Hosting
{
    public static class DXCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddDXCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<DXDatabaseOptions>()
                .Bind(configuration.GetSection("Database"));

            services.AddSingleton<ISQLQueryDXHelper>(sp =>
            {
                var o = sp.GetRequiredService<IOptions<DXDatabaseOptions>>().Value;
                return o.Type.Equals("MySQL", StringComparison.OrdinalIgnoreCase)
                    ? sp.GetRequiredService<MySQLQueryDXHelper>()
                    : sp.GetRequiredService<PGSQLQueryDXHelper>();
            });

            services.AddSingleton<MySQLQueryDXHelper>();
            services.AddSingleton<PGSQLQueryDXHelper>();

            services.AddSingleton<IDXStructureCache, DXStructureCache>();

            services.AddScoped<IDXStructureRawReader, DXCoreRepository>();
            services.AddScoped<IDXCoreRepository, DXCoreRepository>();
            services.AddScoped<IDXStructureRepository, DXCoreRepository>();
            services.AddScoped<IDXEnumCoreRepository, DXCoreRepository>();

            services.AddScoped<IDXGenericRepository, DXGenericRepository>();
            services.AddScoped<IDXUnitDataService, DXUnitDataService>();
            services.AddScoped<IDXCoreHandler, CoreModelHandler>();
            services.AddScoped<IDXMigrationService, MigrationService>();

            return services;
        }
    }
}
