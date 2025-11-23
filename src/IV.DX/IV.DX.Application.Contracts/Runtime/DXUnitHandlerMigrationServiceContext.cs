using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Runtime
{
    internal class DXUnitHandlerMigrationServiceContext : IDXHandlerContext
    {
        public DXMigrationScriptsUnit MigrationScript { get; set; }

        public DXUnitHandlerMigrationServiceContext(DXMigrationScriptsUnit migrationScriptInfo)
        {
            MigrationScript = migrationScriptInfo;
        }
    }
}