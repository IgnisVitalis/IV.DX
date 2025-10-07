using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class EntityHandlerPostInitCoreContext : EntityHandlerMigrationServiceContext
    {
        public EntityHandlerPostInitCoreContext(DPMigrationScriptsObject migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}