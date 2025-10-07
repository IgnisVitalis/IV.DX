using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IV.DX.Application.DataHandlers
{
    public class DPEnumDescObjectHandler : DPObjectDescObjectHandler<DPEnumDescObject>
    {
        private readonly IDataStructureRepository _dataStructureRepo;
        private readonly IGenericRepository _genericRepo;

        public DPEnumDescObjectHandler(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this._dataStructureRepo = serviceProvider.GetService<IDataStructureRepository>();
            this._genericRepo = serviceProvider.GetService<IGenericRepository>();            
        }

        public override Guid OnInserting(DPEnumDescObject entity, EntityHandlerBaseContext context)
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

                return base.OnInserting(entity, context);
            }
        }

        public override Guid OnUpdating(DPEnumDescObject entity, EntityHandlerBaseContext context)
        {
            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.UpdatedDataStructure(entity);

            //this._dataStructureRepo.AddOrUpdateEnumInfo(entity);

            return base.OnUpdating(entity, context);
        }

        public override bool OnDeleting(Guid id, EntityHandlerBaseContext context)
        {
            var entity = this._genericRepo.GetItem<DPEnumDescObject>(id);

            base.Validate(entity);
            base.Process(entity);

            this._dataStructureRepo.DropDataStructure(entity);

            switch (entity.DPObjectDescGenBlock.Kind)
            {
                case DPObjectKindEnum.Core:
                    return false;
                default:
                    return base.OnDeleting(id, context);
            }
        }
    }
}