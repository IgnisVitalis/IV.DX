using IV.DX.Kernel.Models;

namespace IV.DX.Persistence.Contracts.Abstractions
{
    internal interface IDXRawReader
    {
        DXDataBlock<DXUnitRecord> Get(string typeName, IDictionary<string, string> columns, string? dxFilter = null);
    }
}
