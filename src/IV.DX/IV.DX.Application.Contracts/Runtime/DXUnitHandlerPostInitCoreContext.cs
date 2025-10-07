using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Runtime
{
    public class DXUnitHandlerPostInitCoreContext : DXUnitHandlerMigrationServiceContext
    {
        public DXUnitHandlerPostInitCoreContext(DXMigrationScriptsUnit migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}