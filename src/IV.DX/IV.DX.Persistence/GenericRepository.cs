using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    public class GenericRepository : IGenericRepository
    {
        private readonly ICoreRepository _coreRepo;

        public GenericRepository(ICoreRepository coreRepo)
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
            var result = this._coreRepo.GetItem(ModelConverter.GetESQLModelDefinition(typeof(T)), id, TypeOfEntityLoading.Full);

            return ESQLObjectHelper.CreateInstance<T>(result);
        }

        public IEnumerable<T> GetItems<T>() where T : ESQLObject
        {
            var result = this._coreRepo.GetItems(ModelConverter.GetESQLModelDefinition<T>(), TypeOfEntityLoading.Full).ToList();

            return result.Select(x => ESQLObjectHelper.CreateInstance<T>(x)).ToList();
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids) where T : ESQLObject
        {
            var result = this._coreRepo.GetItems(ModelConverter.GetESQLModelDefinition<T>(), ids, TypeOfEntityLoading.Full).ToList();

            return result.Select(x => ESQLObjectHelper.CreateInstance<T>(x)).ToList();
        }

        public IEnumerable<T> GetItems<T>(string esqlWhereExpression) where T : ESQLObject
        {
            var result = this._coreRepo.GetItems(ModelConverter.GetESQLModelDefinition<T>(), esqlWhereExpression, TypeOfEntityLoading.Full).ToList();

            return result.Select(x => ESQLObjectHelper.CreateInstance<T>(x)).ToList();
        }

        public Guid Insert(ESQLObject esqlObject)
        {
            ArgumentNullException.ThrowIfNull(esqlObject);

            var esqlModel = esqlObject.ConvertToESQLModel();

            return this._coreRepo.Insert(esqlModel);
        }

        public Guid InsertOrUpdate(ESQLObject esqlObject)
        {
            var definition = ModelConverter.GetESQLModelDefinition(esqlObject.GetType());

            var existingEntity = this._coreRepo.GetItem(definition, esqlObject.ID, TypeOfEntityLoading.Base);

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

            var block = ModelConverter.GetESQLBlockDefinition(blockName, typeof(T));

            var result = this._coreRepo.GetSingleBlock(block, id);

            return ESQLObjectHelper.CreateBlockInstance<T>(result);
        }
    }
}