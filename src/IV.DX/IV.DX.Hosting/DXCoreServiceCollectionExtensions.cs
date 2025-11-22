using IV.DX.Application;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Handlers;
using IV.DX.Application.Services;
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

                var typeLowerCase = o.Type.ToLower().Trim();            

                switch (typeLowerCase)
                {
                    case "postgresql": return sp.GetRequiredService<PGSQLQueryDXHelper>();
                    case "mysql": return sp.GetRequiredService<MySQLQueryDXHelper>();
                    default: return sp.GetRequiredService<PGSQLQueryDXHelper>();
                }
            });

            services.AddSingleton<MySQLQueryDXHelper>();
            services.AddSingleton<PGSQLQueryDXHelper>();

            services.AddSingleton<IDXStructureCache, DXStructureCache>();

            var func = (IServiceProvider sp) =>
            {
                var cache = sp.GetRequiredService<IDXStructureCache>();
                var helper = sp.GetRequiredService<ISQLQueryDXHelper>();

                var o = sp.GetRequiredService<IOptions<DXDatabaseOptions>>().Value;

                return new DXCoreRepository(new DXDatabaseOptions() { ConnectionString = o.ConnectionString }, cache, helper);
            };
           
            services.AddScoped<IDXCoreRepository, DXCoreRepository>(func);
            services.AddScoped<IDXStructureRawReader, DXCoreRepository>(func);
            services.AddScoped<IDXStructureRepository, DXCoreRepository>(func);
            services.AddScoped<IDXEnumCoreRepository, DXCoreRepository>(func);

            services.AddScoped<IDXUnitGenericRepository, DXUnitGenericRepository>();
            services.AddScoped<IDXElementGenericRepository, DXElementGenericRepository>();
            services.AddScoped<IDXUnitDataService, DXUnitDataService>();
            services.AddScoped<IDXEnumDataService, DXEnumDataService>();
            services.AddScoped<IDXElementDataService, DXElementDataService>();            
            services.AddScoped<IDXMigrationService, MigrationService>();
            services.AddScoped<IDXStructureService, DXStructureService>();

            services.RegisterCoreHandlers();

            return services;
        }

        private static void RegisterCoreHandlers(this IServiceCollection services)
        {
            services.AddDXHandlers(typeof(DXElementDefinitionUnitHandler).Assembly);
        }
    }
}
