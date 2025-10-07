using IV.DX.Contracts.Common.Models;

namespace IV.DX.Contracts.Application
{
    public class EntityHandlerPreInitCoreContext : EntityHandlerMigrationServiceContext
    {
        public EntityHandlerPreInitCoreContext(DPMigrationScriptsObject migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}