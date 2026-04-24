using System.Threading.Tasks;

namespace IV.DX.Shared.IntTests.Schema
{
    public interface IDXTestSchemaHelper
    {
        Task<bool> UniqueConstraintExistsAsync(string tableName, string constraintName);
    }
}
