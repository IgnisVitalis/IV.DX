using IV.DX.Contracts.Application;
using IV.DX.Contracts.Common.Helpers;
using IV.DX.Contracts.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    public class DPInheritanceInitCoreHandler : BaseEntityHandler<DPInheritanceInitCore>
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