using System.Data.Common;

namespace IV.DX.Persistence.Abstractions
{
    internal interface ISQLDbProvider
    {
        DbCommandBuilder GetDbCommandBuilder(DbDataAdapter dataAdapter);
        DbDataAdapter GetDbDataAdapter(DbConnection dbconnection, string query);
        DbConnection GetDBConnection(string connectionStr);
        void RunSQLQuery(string connectionString, string query);
    }
}
