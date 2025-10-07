using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class EntityHandlerPreInitCoreContext : EntityHandlerMigrationServiceContext
    {
        public EntityHandlerPreInitCoreContext(DXMigrationScriptsUnit migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}