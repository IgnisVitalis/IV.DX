using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Handlers
{
    internal class DXInheritanceInitCoreHandler(IDXStructureRepository dataStructureRepo) :
        IDXBeforeInsertHandler<DXInheritanceInitCore>, IDXUniqueBeforeInsertHandler
    {
        public int BeforeOrder => 1;

        public Task<DXResult<DXInheritanceInitCore>> BeforeInsertAsync(DXInheritanceInitCore dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
        {
            dataStructureRepo.SetDXUnitInheritance(dxUnit.ChildDXUnit, dxUnit.BaseDXUnit);

            return Task.FromResult(DXResult<DXInheritanceInitCore>.OkSkipProcess(dxUnit));
        }
    }
}
