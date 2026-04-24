using IV.DX.Kernel;
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

        IDXStructureCache _dxStructureCache;

        public DXUnitGenericRepository(IDXUnitCoreRepository coreRepo, IDXStructureCache dxStructureCache)
        {
            this._coreRepo = coreRepo;
            this._dxStructureCache = dxStructureCache;
        }

        public bool Delete(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var dxTypeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());

            return this._coreRepo.Delete(dxTypeName, dxUnit.ID);
        }

        public T GetDXUnit<T>(Guid id) where T : DXUnit
        {
            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance<T>();
            var dxModelDefinition = DXDataSetDefinitionConverter.ToDXModelDefinition(typeof(T), dxUnitInheritance);
            ApplyDXTitleExpression<T>(dxModelDefinition);

            var block = this._coreRepo.GetItemRecord(dxModelDefinition, id, DXLoadingType.Full);
            var record = block?.Data?.Items?.FirstOrDefault();
            return record == null ? default! : (T)DXRecordConverter.ToDXUnit(record, typeof(T));
        }

        public IEnumerable<T> GetDXUnits<T>() where T : DXUnit
        {
            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance<T>();
            var definition = DXDataSetDefinitionConverter.ToDXModelDefinition<T>(dxUnitInheritance);
            ApplyDXTitleExpression<T>(definition);

            var block = this._coreRepo.GetItemsRecord(definition, DXLoadingType.Full);
            return (block.Data?.Items ?? new List<DXUnitRecord>())
                .Select(x => (T)DXRecordConverter.ToDXUnit(x, typeof(T)))
                .ToList();
        }

        public IEnumerable<T> GetDXUnits<T>(IEnumerable<Guid> ids) where T : DXUnit
        {
            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance<T>();
            var definition = DXDataSetDefinitionConverter.ToDXModelDefinition<T>(dxUnitInheritance);
            ApplyDXTitleExpression<T>(definition);

            var block = this._coreRepo.GetItemsRecord(definition, ids, DXLoadingType.Full);
            return (block.Data?.Items ?? new List<DXUnitRecord>())
                .Select(x => (T)DXRecordConverter.ToDXUnit(x, typeof(T)))
                .ToList();
        }

        public IEnumerable<T> GetDXUnits<T>(string dxFilter) where T : DXUnit
        {
            var dxUnitInheritance = _dxStructureCache.GetDXUnitInheritance<T>();
            var definition = DXDataSetDefinitionConverter.ToDXModelDefinition<T>(dxUnitInheritance);
            ApplyDXTitleExpression<T>(definition);

            var block = this._coreRepo.GetItemsRecord(definition, dxFilter, DXLoadingType.Full);
            return (block.Data?.Items ?? new List<DXUnitRecord>())
                .Select(x => (T)DXRecordConverter.ToDXUnit(x, typeof(T)))
                .ToList();
        }

        private void ApplyDXTitleExpression<T>(DXDataSetDefinition definition) where T : DXUnit
        {
            var typeName = AttributeReader.GetDXUnitTypeName(typeof(T));
            var typeInfo = _dxStructureCache.GetDXUnit(typeName);
            definition.MainElement.DXTitleExpression =
                string.IsNullOrEmpty(typeInfo?.DXTitleExpression) ? Constants.ID : typeInfo.DXTitleExpression;
        }

        public Guid Insert(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var block = DXRecordWriter.ToBlock(dxUnit);
            return this._coreRepo.InsertOrUpdate(block);
        }

        public Guid InsertOrUpdate(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var block = DXRecordWriter.ToBlock(dxUnit);
            return this._coreRepo.InsertOrUpdate(block);
        }

        public Guid Update(DXUnit dxUnit)
        {
            ArgumentNullException.ThrowIfNull(dxUnit);

            var block = DXRecordWriter.ToBlock(dxUnit);
            return this._coreRepo.InsertOrUpdate(block);
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

