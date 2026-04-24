using Npgsql;
using System;
using System.Threading.Tasks;

namespace IV.DX.Shared.IntTests.Schema
{
    public sealed class PGSQLTestSchemaHelper(string connectionString) : IDXTestSchemaHelper
    {
        public async Task<bool> UniqueConstraintExistsAsync(string tableName, string constraintName)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM information_schema.table_constraints " +
                "WHERE table_name = @t AND constraint_type = 'UNIQUE' AND constraint_name = @c",
                conn);
            cmd.Parameters.AddWithValue("t", tableName);
            cmd.Parameters.AddWithValue("c", constraintName);
            return Convert.ToInt64(await cmd.ExecuteScalarAsync()) > 0;
        }
    }
}
