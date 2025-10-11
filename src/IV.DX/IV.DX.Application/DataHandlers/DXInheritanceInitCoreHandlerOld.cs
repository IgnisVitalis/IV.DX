using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXInheritanceInitCoreHandlerOld : BaseEntityHandler<DXInheritanceInitCore>
    {
        private readonly IDXStructureRepository _dataStructureRepository;

        public DXInheritanceInitCoreHandlerOld(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepository = serviceProvider.GetRequiredService<IDXStructureRepository>();
        }

        public override Guid OnInserting(DXInheritanceInitCore entity, IDXHandlerContext context)
        {
            this._dataStructureRepository.SetEntityInheritance(entity.ChildEntity, entity.BaseEntity);

            return Guid.Empty;
        }
    }
}