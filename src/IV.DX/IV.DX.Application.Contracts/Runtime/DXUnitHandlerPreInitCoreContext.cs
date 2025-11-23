using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.Runtime
{
    internal class DXUnitHandlerPreInitCoreContext : DXUnitHandlerMigrationServiceContext
    {
        public DXUnitHandlerPreInitCoreContext(DXMigrationScriptsUnit migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}