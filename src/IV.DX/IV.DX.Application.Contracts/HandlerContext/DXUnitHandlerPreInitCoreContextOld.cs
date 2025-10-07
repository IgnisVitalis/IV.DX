using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class DXUnitHandlerPreInitCoreContextOld : DXUnitHandlerMigrationServiceContextOld
    {
        public DXUnitHandlerPreInitCoreContextOld(DXMigrationScriptsUnit migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}