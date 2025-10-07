using IV.DX.Contracts.Common.Models;

namespace IV.DX.Contracts.Application
{
    public class EntityHandlerPostInitCoreContext : EntityHandlerMigrationServiceContext
    {
        public EntityHandlerPostInitCoreContext(DPMigrationScriptsObject migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}