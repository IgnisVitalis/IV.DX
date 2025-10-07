using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class EntityHandlerMigrationServiceContext : EntityHandlerBaseContext
    {
        public DXMigrationScriptsUnit MigrationScript { get; set; }

        public EntityHandlerMigrationServiceContext(DXMigrationScriptsUnit migrationScriptInfo)
        {
            MigrationScript = migrationScriptInfo;
        }
    }
}