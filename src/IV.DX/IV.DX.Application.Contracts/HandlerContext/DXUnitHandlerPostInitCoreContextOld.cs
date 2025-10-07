using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class DXUnitHandlerPostInitCoreContextOld : DXUnitHandlerMigrationServiceContextOld
    {
        public DXUnitHandlerPostInitCoreContextOld(DXMigrationScriptsUnit migrationScriptInfo)
            : base(migrationScriptInfo)
        {
        }
    }
}