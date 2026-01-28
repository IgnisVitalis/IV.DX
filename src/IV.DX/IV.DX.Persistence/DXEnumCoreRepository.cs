using System.Data;
using IV.DX.Kernel.Converters.DXModelDefinitionConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers.DXModelDefinitionHelpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        IEnumerable<DXModel> IDXEnumCoreRepository.GetItems(string enumType)
        {
            var modelDefinition = this.GetEnumModelDefinition(enumType);

            if (modelDefinition == null)
                return null;

            return this.GetItems(modelDefinition, DXLoadingType.Full);
        }

        bool IDXEnumCoreRepository.IsItemExisting(string typeName, Guid objectId)
        {
            DXDataSetDefinition dd = new DXDataSetDefinition(new DXMainTableDefinition(typeName, typeName, false));

            var item = this.GetItem(dd, objectId, DXLoadingType.Base);

            return item != null;
        }

        private DXDataSetDefinition GetEnumModelDefinition(string type)
        {
            var mainDXUnit = this.GetDXEnumDefinition(type);

            if (mainDXUnit == null)
                return null;

            var modelDefinition = DXModelDefinitionHelper.BuildModelDefinition(mainDXUnit, _dxStructureCache.DXRelations);

            return modelDefinition;
        }

        DXModel? IDXEnumCoreRepository.GetItem(string typeName, Guid objectId)
        {
            var modelDefinition = this.GetEnumModelDefinition(typeName);

            if (modelDefinition == null)
                return null;

            return this.GetItem(modelDefinition, objectId, DXLoadingType.Full);
        }

        Guid IDXEnumCoreRepository.Insert(DXModel dxModel)
        {
            var typeName = dxModel.DXMainElement.Attribute.Type;
            var enumInfo = this.GetDXEnumDefinition(typeName);

            return this.InsertOrUpdateDXEnum(enumInfo, dxModel, ProcessingType.Insert);

            throw new Exception($"Enum type '{dxModel.DXMainElement.Attribute.Type}' is not registered.");
        }

        Guid IDXEnumCoreRepository.Update(DXModel dxModel)
        {
            var typeName = dxModel.DXMainElement.Attribute.Type;
            var enumInfo = this.GetDXEnumDefinition(typeName);

            return this.InsertOrUpdateDXEnum(enumInfo, dxModel, ProcessingType.Update);

            throw new Exception($"Enum type '{dxModel.DXMainElement.Attribute.Type}' is not registered.");
        }

        Guid IDXEnumCoreRepository.InsertOrUpdate(DXModel dxModel)
        {
            var objId = dxModel.DXMainElement.Item.ID;
            var type = dxModel.DXMainElement.Attribute.Type;

            if (!string.IsNullOrEmpty(type)
                && this.IsItemExisting(type, objId))
            {
                return this.Update(dxModel);
            }
            else
            {
                return this.Insert(dxModel);
            }
        }

        bool IDXEnumCoreRepository.Delete(string typeName, Guid id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(typeName);

            if (id == Guid.Empty)
                throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

            var mainDXUnitInfo = this.GetDXEnumDefinition(typeName);

            return this.RunRequestInTransaction((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                this.DeleteDXUnitFromDataSet(typeName, id, dataSet, conn);

                dataSet.AcceptChanges();

                return true;
            });
        }

        private Guid InsertOrUpdateDXEnum(DXEnumDefinitionUnit enumInfo, DXModel dxModel, ProcessingType processingType)
        {
            this.RunRequestInTransaction(conn =>
            {
                var dataSet = new DataSet(enumInfo.Name);
                var enumTable = enumInfo.Name;

                var dxModelDefinition = dxModel.ToDXModelDefinition(enumInfo);

                var adapter = this.PopulateTableToDataSet(
                    conn,
                    dataSet,
                    enumTable,
                    dxFilter:
                    this.GetWhereExpressionForID(dxModel.DXMainElement.Item.ID),
                    columns: dxModelDefinition.MainElement.GetColumns());

                UpsertOwnRow(dxModel, dataSet.Tables[enumTable], enumTable, processingType);

                SaveTable(adapter, conn, dataSet, dataSet.Tables[enumTable], false);

                dataSet.AcceptChanges();
                return true;
            });

            return dxModel.DXMainElement.Item.ID;
        }
    }
}