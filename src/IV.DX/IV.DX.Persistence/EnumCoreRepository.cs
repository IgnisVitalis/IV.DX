using IV.DX.Contracts.Common.Helpers;
using IV.DX.Contracts.Common.Models;
using IV.DX.Contracts.Persistence;
using System.Data;

namespace IV.DX.Persistence
{
    public partial class CoreRepository : ICoreRepository, IDataStructureRepository, IEnumCoreRepository
    {
        public ESQLMultiItem Get(ESQLBlockDefinition container)
        {
            if (container == null)
                return null;

            ESQLMultiItem result = null;

            this.RunRequest((conn) =>
            {
                DataSet dataSet = new DataSet(container.Type);

                this.PopulateTableToDataSet(conn, dataSet, container.Type,
                    columnNames: container.Select(x => x.ColumnDefinition.ESQLExpression));

                if (dataSet.Tables[container.Type].Rows.Count == 0)
                {
                    result = null;
                }
                else
                {
                    result = this.ConvertToESQLMultiItem(container);

                    this.PopulateESQLMultiItem(result, container, dataSet.Tables[result.Name].Rows.Cast<DataRow>());
                }
            });

            return result;
        }
    }
}
