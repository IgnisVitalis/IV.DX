using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class EntityHandlerMigrationServiceContext : EntityHandlerBaseContext
    {
        public DPMigrationScriptsObject MigrationScript { get; set; }

        public EntityHandlerMigrationServiceContext(DPMigrationScriptsObject migrationScriptInfo)
        {
            MigrationScript = migrationScriptInfo;
        }
    }
}