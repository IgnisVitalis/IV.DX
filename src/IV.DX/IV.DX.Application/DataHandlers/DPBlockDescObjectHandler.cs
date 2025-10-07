using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXElementDefinitionUnitHandler : DXObjectDefinitionUnitHandler<DXElementDefinitionUnit>
    {
        private readonly IDataStructureRepository _dataStructureRepo;
        private readonly IGenericRepository _genericRepo;

        public DXElementDefinitionUnitHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();
        }

        public override Guid OnInserting(DXElementDefinitionUnit entity, EntityHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            if (context is EntityHandlerPreInitCoreContext)
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                return Guid.Empty;
            }
            else if (context is EntityHandlerPostInitCoreContext)
            {
                return base.OnInserting(entity, context);
            }
            else
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                this.ProcessRelations(entity);
                return base.OnInserting(entity, context);
            }
        }

        public override Guid OnUpdating(DXElementDefinitionUnit entity, EntityHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.UpdatedDataStructure(entity);

            this.ProcessRelations(entity);
            return base.OnUpdating(entity, context);
        }

        public override bool OnDeleting(Guid id, EntityHandlerBaseContext context)
        {
            var entity = this._genericRepo.GetItem<DXElementDefinitionUnit>(id);

            if (entity == null)
                return false;

            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.DropDataStructure(entity);

            return base.OnDeleting(id, context);
        }

        private void ProcessRelations(DXElementDefinitionUnit entity)
        {
            this.ProcessEnumRelations(entity);
        }
    }
}