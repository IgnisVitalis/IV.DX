using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Runtime
{
    public class DXUnitHandlerPreInitCoreContext : DXUnitHandlerMigrationServiceContext
    {
        public DXUnitHandlerPreInitCoreContext(DXMigrationScriptsUnit migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}