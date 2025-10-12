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

        public bool Delete(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var esqlTypeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());

            return this._coreRepo.Delete(esqlTypeName, dxUnit.ID);
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

        public IEnumerable<T> GetItems<T>(string dxsqlWhereExpression) where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXModelDefinitionHelper.GetESQLModelDefinition<T>(), dxsqlWhereExpression, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public Guid Insert(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var esqlModel = dxUnit.ConvertToESQLModel();

            return this._coreRepo.Insert(esqlModel);
        }

        public Guid InsertOrUpdate(DXUnit dxUnit)
        {
            var definition = DXModelDefinitionHelper.GetESQLModelDefinition(dxUnit.GetType());

            var existingEntity = this._coreRepo.GetItem(definition, dxUnit.ID, DXLoadingType.Base);

            if (existingEntity == null)
            {
                return this.Insert(dxUnit);
            }
            else
            {
                return this.Update(dxUnit);
            }
        }

        public Guid Update(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var esqlModel = dxUnit.ConvertToESQLModel();

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

        public Guid InsertBlock(string esqlModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleBlock = dxElement.ConvertToSingleItem();

            return this._coreRepo.InsertSingleBlock(esqlModelType, singleBlock);
        }

        public Guid UpdateBlock(string esqlModelType, DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(esqlModelType);
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleBlock = dxElement.ConvertToSingleItem();

            return this._coreRepo.UpdateSingleBlock(esqlModelType, singleBlock);
        }

        public bool DeleteBlock(DXElement dxElement)
        {
            ArgumentNullException.ThrowIfNull(dxElement);

            var singleBlock = dxElement.ConvertToSingleItem();

            return this._coreRepo.DeleteSingleBlock(singleBlock.Name, dxElement.ID);
        }

        public T GetBlock<T>(Guid id) where T : DXElement
        {
            var blockName = AttributeReader.GetDXBlockTypeName(typeof(T));

            var block = DXModelDefinitionHelper.GetDXElementDefinition(blockName, typeof(T));

            var result = this._coreRepo.GetSingleBlock(block, id);

            return DXUnitHelper.CreateBlockInstance<T>(result);
        }
    }
}