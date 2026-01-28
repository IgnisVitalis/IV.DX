using IV.DX.Application.Contracts.Models;

namespace IV.DX.Application.Contracts.Abstractions;

public interface IDXMigrationParser
{
    IReadOnlyList<DXParsedItem> Parse(string json);
    IReadOnlyList<DXParsedItem> ParseFile(string path);
}

