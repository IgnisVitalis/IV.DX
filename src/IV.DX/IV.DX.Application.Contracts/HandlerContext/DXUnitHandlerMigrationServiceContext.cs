using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class DXUnitHandlerMigrationServiceContext : DXUnitHandlerBaseContext
    {
        public DXMigrationScriptsUnit MigrationScript { get; set; }

        public DXUnitHandlerMigrationServiceContext(DXMigrationScriptsUnit migrationScriptInfo)
        {
            MigrationScript = migrationScriptInfo;
        }
    }
}