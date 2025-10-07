using IV.DX.Contracts.Common.Models;

namespace IV.DX.Contracts.Application
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