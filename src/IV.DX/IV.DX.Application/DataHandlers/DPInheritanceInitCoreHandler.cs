using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DPInheritanceInitCoreHandler : BaseEntityHandler<DPInheritanceInitCore>
    {
        private readonly IDataStructureRepository _dataStructureRepository;

        public DPInheritanceInitCoreHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepository = serviceProvider.GetService<IDataStructureRepository>();
        }

        public override Guid OnInserting(DPInheritanceInitCore entity, EntityHandlerBaseContext context)
        {
            this._dataStructureRepository.SetEntityInheritance(entity.ChildEntity, entity.BaseEntity);

            return Guid.Empty;
        }
    }
}