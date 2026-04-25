using System.Data;
using System.Data.Common;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    public interface IDXBulkInsertCapable
    {
        void BulkInsert(DbConnection connection, DataTable table, string tableName);
        void BulkUpsert(DbConnection connection, DataTable table, string tableName, string keyColumn = "Id");
    }
}
