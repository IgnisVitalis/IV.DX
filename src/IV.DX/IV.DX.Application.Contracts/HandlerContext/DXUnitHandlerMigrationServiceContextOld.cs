using IV.DX.Kernel.Models;

namespace IV.DX.Application.Contracts.HandlerContext
{
    public class DXUnitHandlerMigrationServiceContextOld : DXUnitHandlerBaseContextOld
    {
        public DXMigrationScriptsUnit MigrationScript { get; set; }

        public DXUnitHandlerMigrationServiceContextOld(DXMigrationScriptsUnit migrationScriptInfo)
        {
            MigrationScript = migrationScriptInfo;
        }
    }
}