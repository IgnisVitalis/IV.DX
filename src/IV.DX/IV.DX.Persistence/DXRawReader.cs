using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Data;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXUnitCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader, IDXElementCoreRepository, IDXRawReader
    {
        public DXMultiElement Get(string typeName, IDictionary<string, string> columns, string? dxFilter = null)
        {
            if (typeName == null || columns == null)
                return null;

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

                var announced =
                    dataTable.Rows.Cast<DataRow>()
                    .Select(x => this.GetDXItem(typeName, x, columns)).ToHashSet();

                DXMultiElement multiItem = DXMultiElement.CreateForFullMode(typeName, new DXElementAttribute(typeName), announced);

                return multiItem;
            });
        }
    }
}