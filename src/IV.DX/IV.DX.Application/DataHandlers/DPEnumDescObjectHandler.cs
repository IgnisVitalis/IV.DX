using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    internal class DXEnumDefinitionUnitHandler : DXObjectDefinitionUnitHandler<DXEnumDefinitionUnit>
    {
        private readonly IDXStructureRepository _dataStructureRepo;
        private readonly IDXGenericRepository _genericRepo;

        public DXEnumDefinitionUnitHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepo = serviceProvider.GetService<IDXStructureRepository>();
            this._genericRepo = serviceProvider.GetService<IDXGenericRepository>();            
        }

        public override Guid OnInserting(DXEnumDefinitionUnit entity, DXUnitHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            if (context is DXUnitHandlerPreInitCoreContext)
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                return Guid.Empty;
            }
            else if (context is DXUnitHandlerPostInitCoreContext)
            {
                return base.OnInserting(entity, context);
            }
            else
            {
                this._dataStructureRepo.CreateDataStructure(entity);

                return base.OnInserting(entity, context);
            }
        }

        public override Guid OnUpdating(DXEnumDefinitionUnit entity, DXUnitHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.UpdatedDataStructure(entity);

            //this._dataStructureRepo.AddOrUpdateEnumInfo(entity);

            return base.OnUpdating(entity, context);
        }

        public override bool OnDeleting(Guid id, DXUnitHandlerBaseContext context)
        {
            var entity = this._genericRepo.GetItem<DXEnumDefinitionUnit>(id);

            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.DropDataStructure(entity);

            switch (entity.DXUnitDefinitionMainElement.Kind)
            {
                case DXObjectKindEnum.Core:
                    return false;
                default:
                    return base.OnDeleting(id, context);
            }
        }
    }
}