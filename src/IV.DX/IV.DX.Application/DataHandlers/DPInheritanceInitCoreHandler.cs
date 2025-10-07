using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXInheritanceInitCoreHandler : BaseEntityHandler<DXInheritanceInitCore>
    {
        private readonly IDXStructureRepository _dataStructureRepository;

        public DXInheritanceInitCoreHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepository = serviceProvider.GetService<IDXStructureRepository>();
        }

        public override Guid OnInserting(DXInheritanceInitCore entity, DXUnitHandlerBaseContext context)
        {
            this._dataStructureRepository.SetEntityInheritance(entity.ChildEntity, entity.BaseEntity);

            return Guid.Empty;
        }
    }
}