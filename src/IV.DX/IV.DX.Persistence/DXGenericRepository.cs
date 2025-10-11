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

        public bool Delete(DXUnit esqlObject)
        {
            ArgumentNullException.ThrowIfNull(esqlObject);

            var esqlTypeName = AttributeReader.GetESQLObjectTypeName(esqlObject.GetType());

            return this._coreRepo.Delete(esqlTypeName, esqlObject.ID);
        }

        public T GetItem<T>(Guid id) where T : DXUnit
        {
            var result = this._coreRepo.GetItem(DXModelDefinitionHelper.GetESQLModelDefinition(typeof(T)), id, DXLoadingType.Full);

            return DXUnitHelper.CreateInstance<T>(result);
        }

        public IEnumerable<T> GetItems<T>() where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXModelDefinitionHelper.GetESQLModelDefinition<T>(), DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids) where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXModelDefinitionHelper.GetESQLModelDefinition<T>(), ids, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public IEnumerable<T> GetItems<T>(string esqlWhereExpression) where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXModelDefinitionHelper.GetESQLModelDefinition<T>(), esqlWhereExpression, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public Guid Insert(DXUnit esqlObject)
        {
            ArgumentNullException.ThrowIfNull(esqlObject);

            var esqlModel = esqlObject.ConvertToESQLModel();

            return this._coreRepo.Insert(esqlModel);
        }

        public Guid InsertOrUpdate(DXUnit esqlObject)
        {
            var definition = DXModelDefinitionHelper.GetESQLModelDefinition(esqlObject.GetType());

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

        public Guid Update(DXUnit esqlObject)
        {
            ArgumentNullException.ThrowIfNull(esqlObject);

            var esqlModel = esqlObject.ConvertToESQLModel();

            return this._coreRepo.Update(esqlModel);
        }

        public bool AddRelation(DXRelationItemUnit relationItem)
        {
            ArgumentNullException.ThrowIfNull(relationItem);

            return this._coreRepo.AddRelation(
                relationItem.DXRelationItemMainElement.ObjectTypeNameLeft,
                     relationItem.DXRelationItemMainElement.ObjectIDLeft,
                     relationItem.DXRelationItemMainElement.RelationNameRight,
                     relationItem.DXRelationItemMainElement.ObjectTypeNameRight,
                     relationItem.DXRelationItemMainElement.ObjectIDRight);
        }

        public bool RemoveRelation(DXRelationItemUnit relationItem)
        {
            ArgumentNullException.ThrowIfNull(relationItem);

            return this._coreRepo.RemoveRelation(
                relationItem.DXRelationItemMainElement.ObjectTypeNameLeft,
                relationItem.DXRelationItemMainElement.ObjectIDLeft,
                relationItem.DXRelationItemMainElement.RelationNameRight,
                relationItem.DXRelationItemMainElement.ObjectTypeNameRight,
                relationItem.DXRelationItemMainElement.ObjectIDRight);
        }

        public Guid InsertBlock(string esqlModelType, DXElement esqlBlock)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(esqlBlock);

            var singleBlock = esqlBlock.ConvertToSingleItem();

            return this._coreRepo.InsertSingleBlock(esqlModelType, singleBlock);
        }

        public Guid UpdateBlock(string esqlModelType, DXElement esqlBlock)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(esqlBlock);

            var singleBlock = esqlBlock.ConvertToSingleItem();

            return this._coreRepo.UpdateSingleBlock(esqlModelType, singleBlock);
        }

        public bool DeleteBlock(DXElement esqlBlock)
        {
            ArgumentNullException.ThrowIfNull(esqlBlock);

            var singleBlock = esqlBlock.ConvertToSingleItem();

            return this._coreRepo.DeleteSingleBlock(singleBlock.Name, esqlBlock.ID);
        }

        public T GetBlock<T>(Guid id) where T : DXElement
        {
            var blockName = AttributeReader.GetESQLBlockTypeName(typeof(T));

            var block = DXModelDefinitionHelper.GetESQLBlockDefinition(blockName, typeof(T));

            var result = this._coreRepo.GetSingleBlock(block, id);

            return DXUnitHelper.CreateBlockInstance<T>(result);
        }
    }
}