using IV.DX.Kernel;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Collections.Generic;
using System.Data;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        public DXDataBlock<DXUnitRecord> Get(string typeName, IDictionary<string, string> columns, string? dxFilter = null)
        {
            if (typeName == null || columns == null)
                return new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta { Kind = "DXUnit", Type = typeName, Op = "Sync", IsMulti = true },
                    Data = new DXData<DXUnitRecord> { Upsert = new List<DXUnitRecord>() }
                };

            columns[Constants.ID] = Constants.ID;
            columns[Constants.TimeStamp] = Constants.TimeStamp;

            return this.RunRequest((conn) =>
            {
                DataSet dataSet = new DataSet(typeName);

                this.PopulateTableToDataSet(conn, dataSet, typeName,
                    columns: columns,
                    dxFilter: dxFilter, fillSchema: false);

                var dataTable = dataSet.Tables[typeName];

                if (dataTable == null)
                    throw new Exception($"Table '{typeName}' wouldn't load");

                var records = dataTable.Rows.Cast<DataRow>()
                    .Select(x => BuildRecordFromRow(typeName, x, columns))
                    .ToList();

                return new DXDataBlock<DXUnitRecord>
                {
                    Meta = new DXMeta
                    {
                        Kind = "DXUnit",
                        Type = typeName,
                        Op = "Sync",
                        IsMulti = true
                    },
                    Data = new DXData<DXUnitRecord>
                    {
                        Upsert = records
                    }
                };
            });
        }

        private DXUnitRecord BuildRecordFromRow(string typeName, DataRow row, IDictionary<string, string> columns)
        {
            var content = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (DataColumn dataColumn in row.Table.Columns)
            {
                if (!columns.ContainsKey(dataColumn.ColumnName))
                    continue;

                if (row[dataColumn.ColumnName] != DBNull.Value)
                {
                    content[dataColumn.ColumnName] = GetValueFromRow(row, dataColumn);
                }
                else
                {
                    content[dataColumn.ColumnName] = null;
                }
            }

            var id = ConvertHelper.ParseGuid(row[Constants.ID]);
            var timeStamp = ConvertHelper.ParseDateTime(row[Constants.TimeStamp]);

            return new DXUnitRecord
            {
                ID = id,
                TimeStamp = timeStamp,
                Fields = ConvertContentToFields(content)
            };
        }
    }
}
