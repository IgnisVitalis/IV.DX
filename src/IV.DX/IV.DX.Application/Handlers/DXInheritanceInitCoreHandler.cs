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

        public Task<DXResult<DXInheritanceInitCore>> BeforeInsertAsync(DXInheritanceInitCore dxUnit, IDXHandlerContext ctx, CancellationToken ct)
        {
            dataStructureRepo.SetDXUnitInheritance(dxUnit.ChildEntity, dxUnit.BaseEntity);

            return Task.Run(() => DXResult<DXInheritanceInitCore>.OkSkipProcess(dxUnit));
        }
    }
}
