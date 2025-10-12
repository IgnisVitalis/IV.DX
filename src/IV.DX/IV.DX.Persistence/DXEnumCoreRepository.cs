using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using System.Data;

namespace IV.DX.Persistence
{
    internal partial class DXCoreRepository : IDXCoreRepository, IDXStructureRepository, IDXEnumCoreRepository, IDXStructureRawReader
    {
        public DXMultiElement Get(DXElementDefinition container)
        {
            if (container == null)
                return null;

            DXMultiElement result = null;

            this.RunRequest((conn) =>
            {
                DataSet dataSet = new DataSet(container.Type);

                this.PopulateTableToDataSet(conn, dataSet, container.Type,
                    columnNames: container.Select(x => x.ColumnDefinition.DXExpression));

                if (dataSet.Tables[container.Type].Rows.Count == 0)
                {
                    result = null;
                }
                else
                {
                    result = this.ConvertToDXMultiItem(container);

                    this.PopulateDXMultiItem(result, container, dataSet.Tables[result.Name].Rows.Cast<DataRow>());
                }
            });

            return result;
        }
    }
}
