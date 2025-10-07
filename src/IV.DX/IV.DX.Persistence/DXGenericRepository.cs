using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal class DXGenericRepository : IDXGenericRepository
    {
        private readonly IDXCoreRepository _coreRepo;

        public DXGenericRepository(IDXCoreRepository coreRepo)
        {
            this._coreRepo = coreRepo;
        }

        public bool Delete(ESQLObject esqlObject)
        {
            ArgumentNullException.ThrowIfNull(esqlObject);

            var asqlTypeName = AttributeReader.GetESQLObjectTypeName(esqlObject.GetType());

            return this._coreRepo.Delete(asqlTypeName, esqlObject.ID);
        }

        public T GetItem<T>(Guid id) where T : ESQLObject
        {
            var result = this._coreRepo.GetItem(DXModelConverter.GetESQLModelDefinition(typeof(T)), id, DXLoadingType.Full);

            return DXUnitHelper.CreateInstance<T>(result);
        }

        public IEnumerable<T> GetItems<T>() where T : ESQLObject
        {
            var result = this._coreRepo.GetItems(DXModelConverter.GetESQLModelDefinition<T>(), DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids) where T : ESQLObject
        {
            var result = this._coreRepo.GetItems(DXModelConverter.GetESQLModelDefinition<T>(), ids, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public IEnumerable<T> GetItems<T>(string esqlWhereExpression) where T : ESQLObject
        {
            var result = this._coreRepo.GetItems(DXModelConverter.GetESQLModelDefinition<T>(), esqlWhereExpression, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public Guid Insert(ESQLObject esqlObject)
        {
            ArgumentNullException.ThrowIfNull(esqlObject);

            var esqlModel = esqlObject.ConvertToESQLModel();

            return this._coreRepo.Insert(esqlModel);
        }

        public Guid InsertOrUpdate(ESQLObject esqlObject)
        {
            var definition = DXModelConverter.GetESQLModelDefinition(esqlObject.GetType());

            var existingEntity = this._coreRepo.GetItem(definition, esqlObject.ID, DXLoadingType.Base);

            if (existingEntity == null)
            {
                return this.Insert(esqlObject);
            }
            else
            {
                return this.Update(esqlObject);
            }
        }

        public Guid Update(ESQLObject esqlObject)
        {
            ArgumentNullException.ThrowIfNull(esqlObject);

            var esqlModel = esqlObject.ConvertToESQLModel();

            return this._coreRepo.Update(esqlModel);
        }

        public bool AddRelation(DPRelationItemObject relationItem)
        {
            ArgumentNullException.ThrowIfNull(relationItem);

            return this._coreRepo.AddRelation(
                relationItem.DPRelationItemGenBlock.ObjectTypeNameLeft,
                     relationItem.DPRelationItemGenBlock.ObjectIDLeft,
                     relationItem.DPRelationItemGenBlock.RelationNameRight,
                     relationItem.DPRelationItemGenBlock.ObjectTypeNameRight,
                     relationItem.DPRelationItemGenBlock.ObjectIDRight);
        }

        public bool RemoveRelation(DPRelationItemObject relationItem)
        {
            ArgumentNullException.ThrowIfNull(relationItem);

            return this._coreRepo.RemoveRelation(
                relationItem.DPRelationItemGenBlock.ObjectTypeNameLeft,
                relationItem.DPRelationItemGenBlock.ObjectIDLeft,
                relationItem.DPRelationItemGenBlock.RelationNameRight,
                relationItem.DPRelationItemGenBlock.ObjectTypeNameRight,
                relationItem.DPRelationItemGenBlock.ObjectIDRight);
        }

        public Guid InsertBlock(string esqlModelType, ESQLBlock esqlBlock)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(esqlBlock);

            var singleBlock = esqlBlock.ConvertToSingleItem();

            return this._coreRepo.InsertSingleBlock(esqlModelType, singleBlock);
        }

        public Guid UpdateBlock(string esqlModelType, ESQLBlock esqlBlock)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(esqlBlock);

            var singleBlock = esqlBlock.ConvertToSingleItem();

            return this._coreRepo.UpdateSingleBlock(esqlModelType, singleBlock);
        }

        public bool DeleteBlock(ESQLBlock esqlBlock)
        {
            ArgumentNullException.ThrowIfNull(esqlBlock);

            var singleBlock = esqlBlock.ConvertToSingleItem();

            return this._coreRepo.DeleteSingleBlock(singleBlock.Name, esqlBlock.ID);
        }

        public T GetBlock<T>(Guid id) where T : ESQLBlock
        {
            var blockName = AttributeReader.GetESQLBlockTypeName(typeof(T));

            var block = DXModelConverter.GetESQLBlockDefinition(blockName, typeof(T));

            var result = this._coreRepo.GetSingleBlock(block, id);

            return DXUnitHelper.CreateBlockInstance<T>(result);
        }
    }
}