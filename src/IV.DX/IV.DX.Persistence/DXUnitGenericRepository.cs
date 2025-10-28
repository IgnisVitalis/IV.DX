using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal class DXUnitGenericRepository : IDXUnitGenericRepository
    {
        private readonly IDXCoreRepository _coreRepo;

        public DXUnitGenericRepository(IDXCoreRepository coreRepo)
        {
            this._coreRepo = coreRepo;
        }

        public bool Delete(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var dxTypeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());

            return this._coreRepo.Delete(dxTypeName, dxUnit.ID);
        }

        public T GetDXUnit<T>(Guid id) where T : DXUnit
        {
            var result = this._coreRepo.GetItem(DXModelDefinitionConverter.Get(typeof(T)), id, DXLoadingType.Full);

            return DXUnitHelper.CreateInstance<T>(result);
        }

        public IEnumerable<T> GetDXUnits<T>() where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXModelDefinitionConverter.Get<T>(), DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public IEnumerable<T> GetDXUnits<T>(IEnumerable<Guid> ids) where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXModelDefinitionConverter.Get<T>(), ids, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public IEnumerable<T> GetDXUnits<T>(string dxsqlWhereExpression) where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXModelDefinitionConverter.Get<T>(), dxsqlWhereExpression, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitHelper.CreateInstance<T>(x)).ToList();
        }

        public Guid Insert(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var dxModel = dxUnit.ConvertToDXModel();

            return this._coreRepo.Insert(dxModel);
        }

        public Guid InsertOrUpdate(DXUnit dxUnit)
        {
            var definition = DXModelDefinitionConverter.Get(dxUnit.GetType());

            var existingDXUnit = this._coreRepo.GetItem(definition, dxUnit.ID, DXLoadingType.Base);

            if (existingDXUnit == null)
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

            var dxModel = dxUnit.ConvertToDXModel();

            return this._coreRepo.Update(dxModel);
        }

        public bool AddDXRelation(DXRelationItemUnit relationItem)
        {
            ArgumentNullException.ThrowIfNull(relationItem);

            return this._coreRepo.AddRelation(
                relationItem.DXRelationItemMainElement.ObjectTypeNameLeft,
                     relationItem.DXRelationItemMainElement.DXUnitIDLeft,
                     relationItem.DXRelationItemMainElement.RelationNameRight,
                     relationItem.DXRelationItemMainElement.ObjectTypeNameRight,
                     relationItem.DXRelationItemMainElement.DXUnitIDRight);
        }

        public bool RemoveDXRelation(DXRelationItemUnit relationItem)
        {
            ArgumentNullException.ThrowIfNull(relationItem);

            return this._coreRepo.RemoveRelation(
                relationItem.DXRelationItemMainElement.ObjectTypeNameLeft,
                relationItem.DXRelationItemMainElement.DXUnitIDLeft,
                relationItem.DXRelationItemMainElement.RelationNameRight,
                relationItem.DXRelationItemMainElement.ObjectTypeNameRight,
                relationItem.DXRelationItemMainElement.DXUnitIDRight);
        }
    }
}