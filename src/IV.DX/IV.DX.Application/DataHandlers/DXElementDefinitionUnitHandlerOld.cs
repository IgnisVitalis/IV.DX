using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXElementDefinitionUnitHandlerOld : DXObjectDefinitionUnitHandlerOld<DXElementDefinitionUnit>
    {
        private readonly IDXStructureRepository _dataStructureRepo;
        private readonly IDXGenericRepository _genericRepo;

        public DXElementDefinitionUnitHandlerOld(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepo = serviceProvider.GetRequiredService<IDXStructureRepository>();
            this._genericRepo = serviceProvider.GetRequiredService<IDXGenericRepository>();
        }

        public override Guid OnInserting(DXElementDefinitionUnit entity, IDXHandlerContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            if (context is DXUnitHandlerPreInitCoreContextOld)
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                return Guid.Empty;
            }
            else if (context is DXUnitHandlerPostInitCoreContextOld)
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

        public override Guid OnUpdating(DXElementDefinitionUnit entity, IDXHandlerContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.UpdatedDataStructure(entity);

            this.ProcessRelations(entity);
            return base.OnUpdating(entity, context);
        }

        public override bool OnDeleting(Guid id, IDXHandlerContext context)
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