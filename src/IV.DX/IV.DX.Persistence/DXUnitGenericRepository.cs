using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal class DXUnitGenericRepository : IDXUnitGenericRepository
    {
        private readonly IDXUnitCoreRepository _coreRepo;

        public DXUnitGenericRepository(IDXUnitCoreRepository coreRepo)
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
            var result = this._coreRepo.GetItem(DXDataSetDefinitionConverter.ToDXModelDefinition(typeof(T)), id, DXLoadingType.Full);

            return DXUnitConverter.ToDXUnits<T>(result);
        }

        public IEnumerable<T> GetDXUnits<T>() where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXDataSetDefinitionConverter.ToDXModelDefinition<T>(), DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitConverter.ToDXUnits<T>(x)).ToList();
        }

        public IEnumerable<T> GetDXUnits<T>(IEnumerable<Guid> ids) where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXDataSetDefinitionConverter.ToDXModelDefinition<T>(), ids, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitConverter.ToDXUnits<T>(x)).ToList();
        }

        public IEnumerable<T> GetDXUnits<T>(string dxFilter) where T : DXUnit
        {
            var result = this._coreRepo.GetItems(DXDataSetDefinitionConverter.ToDXModelDefinition<T>(), dxFilter, DXLoadingType.Full).ToList();

            return result.Select(x => DXUnitConverter.ToDXUnits<T>(x)).ToList();
        }

        public Guid Insert(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var dxModel = dxUnit.ToDXModel();

            return this._coreRepo.Insert(dxModel);
        }

        public Guid InsertOrUpdate(DXUnit dxUnit)
        {
            var definition = DXDataSetDefinitionConverter.ToDXModelDefinition(dxUnit.GetType());

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

            var dxModel = dxUnit.ToDXModel();

            return this._coreRepo.Update(dxModel);
        }

        public bool AddDXRelation(DXRelationItemUnit relationItem)
        {
            ArgumentNullException.ThrowIfNull(relationItem);

            return this._coreRepo.AddRelation(
                relationItem.ObjectTypeNameLeft,
                     relationItem.DXUnitIDLeft,
                     relationItem.RelationNameRight,
                     relationItem.ObjectTypeNameRight,
                     relationItem.DXUnitIDRight);
        }

        public bool RemoveDXRelation(DXRelationItemUnit relationItem)
        {
            ArgumentNullException.ThrowIfNull(relationItem);

            return this._coreRepo.RemoveRelation(
                relationItem.ObjectTypeNameLeft,
                relationItem.DXUnitIDLeft,
                relationItem.RelationNameRight,
                relationItem.ObjectTypeNameRight,
                relationItem.DXUnitIDRight);
        }
    }
}