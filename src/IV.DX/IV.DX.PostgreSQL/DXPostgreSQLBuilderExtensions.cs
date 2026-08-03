using IV.DX.Hosting;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.PostgreSQL
{
    /// <summary>
    /// Selects PostgreSQL as the database provider for IV.DX.
    /// </summary>
    public static class DXPostgreSQLBuilderExtensions
    {
        /// <summary>
        /// Registers the PostgreSQL implementations of the IV.DX database provider contracts.
        /// Call this on the builder returned by <c>AddDX</c>, before <c>Build</c> or
        /// <c>RegisterHostedService</c>.
        /// </summary>
        public static DXBuilder UsePostgreSQL(this DXBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<PGSQLQueryDXHelper>();

            services.AddSingleton<ISQLDialect>(sp => sp.GetRequiredService<PGSQLQueryDXHelper>());
            services.AddSingleton<ISQLSchemaHelper>(sp => sp.GetRequiredService<PGSQLQueryDXHelper>());
            services.AddSingleton<ISQLDbProvider>(sp => sp.GetRequiredService<PGSQLQueryDXHelper>());
            services.AddSingleton<ISQLMigrationLockHelper>(sp => sp.GetRequiredService<PGSQLQueryDXHelper>());

            return builder;
        }
    }
}
