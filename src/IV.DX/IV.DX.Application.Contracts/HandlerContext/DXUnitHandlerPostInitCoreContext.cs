using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class DXUnitHandlerPostInitCoreContext : DXUnitHandlerMigrationServiceContext
    {
        public DXUnitHandlerPostInitCoreContext(DXMigrationScriptsUnit migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}