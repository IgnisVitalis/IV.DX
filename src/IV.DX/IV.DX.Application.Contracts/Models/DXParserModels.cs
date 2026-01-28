using IV.DX.Application.Contracts.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Models;

public interface IDXMigrationItem
{
    DXMeta Meta { get; }
}

public sealed class DXMeta
{
    public string Kind { get; set; } = null!;          // "DXUnit" | "DXElement"
    public string Type { get; set; } = null!;          // DXCustomerDefinitionUnit / DXColumnDefinitionElement
    public string? Op { get; set; }                    // Sync | Patch
    public string? Cardinality { get; set; }           // One | ZeroOrOne | Many | OneOrMore

    public string? DXFilter { get; set; }              // only for DXUnit Sync Many
    public string? DXUnitContext { get; set; }         // for standalone DXElement
}

public sealed class DXUnitItem : IDXMigrationItem
{
    public DXMeta Meta { get; set; } = null!;

    // Shape depends on Cardinality
    public JToken Data { get; set; } = null!;
}

public sealed class DXElementItem : IDXMigrationItem
{
    public DXMeta Meta { get; set; } = null!;
    public JToken Data { get; set; } = null!;
}

public sealed class DXUnitObject
{
    public Guid ID { get; set; }
    public DateTime TimeStamp { get; set; }

    public string? Name { get; set; }
    public string? DisplayValue { get; set; }
    public int? UnitKind { get; set; }

    public Dictionary<string, DXEmbeddedElement>? DXElements { get; set; }
}

public sealed class DXEmbeddedElement
{
    public DXMeta Meta { get; set; } = null!;
    public JToken Data { get; set; } = null!;
}

public sealed class DXUnitBatch
{
    public List<DXUnitObject>? Upsert { get; set; }
    public List<DXUnitDeleteRef>? Delete { get; set; }
}

public sealed class DXUnitDeleteRef
{
    public Guid ID { get; set; }
    public DateTime TimeStamp { get; set; }
}


public sealed class DXElementBatch
{
    public List<DXElementObject>? Upsert { get; set; }
    public List<DXElementDeleteRef>? Delete { get; set; }
}

public sealed class DXElementObject
{
    public Guid ID { get; set; }
    public DateTime TimeStamp { get; set; }
    public Guid DXUnitID { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken>? Data { get; set; }
}

public sealed class DXElementDeleteRef
{
    public Guid ID { get; set; }
    public DateTime TimeStamp { get; set; }
    public Guid DXUnitID { get; set; }
}

public sealed class DXParsedItem
{
    public DXMeta Meta { get; init; } = null!;

    public DXUnitObject? DXUnitObject { get; init; }
    public DXUnitBatch? DXUnitBatch { get; init; }

    public DXElementObject? DXElementObject { get; init; }
    public DXElementBatch? DXElementBatch { get; init; }
}