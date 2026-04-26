# DXUnit DTO Mapper

The DTO mapper feature provides a thin application-layer bridge between the DX domain model (`DXUnit`/`DXElement`) and the DTOs your API or UI exposes. It handles the mechanical work of copying scalar properties and synchronizing element containers, leaving your code free of boilerplate reflection.

---

## Concepts

| Type | Role |
|---|---|
| `DXUnitMapper<TDto, TUnit>` | Abstract base. Implement this for full control. |
| `DXConventionMapper<TDto, TUnit>` | Built-in convention mapper. No code required. |
| `IDXUnitDtoService<TDto>` | Service interface. Inject this in controllers/services. |

Registration wires everything together: one call creates the mapper, wraps it in `IDXUnitDtoService<TDto>`, and registers both with the DI container.

---

## Registration

Call one of the two `AddDXUnitMapper` overloads during service registration.

### Convention mapper (no code required)

```csharp
builder.Services
    .AddDX(builder.Configuration)
    .AddCustomData("MigrationScripts/MyApp.json")
    ...

// After .AddDX():
builder.Services.AddDXUnitMapper<TBookDto, TBookUnit>();
```

`AddDXUnitMapper<TDto, TUnit>()` creates a `DXConventionMapper` and validates the mapping at startup — if a property cannot be mapped the application fails to start with a clear error message.

### Custom mapper

Derive from `DXUnitMapper<TDto, TUnit>` and implement the two abstract methods:

```csharp
public class TBookMapper : DXUnitMapper<TBookDto, TBookUnit>
{
    public override Task<TBookDto> ToDtoAsync(TBookUnit unit, CancellationToken ct = default)
    {
        return Task.FromResult(new TBookDto
        {
            Id    = unit.Id,
            Title = unit.TBookMainElement?.Name ?? string.Empty,
            // any custom transform
        });
    }

    public override Task<TBookUnit> ToUnitAsync(TBookDto dto, CancellationToken ct = default)
    {
        return Task.FromResult(new TBookUnit
        {
            Id = dto.Id,
            TBookMainElement = new TBookMainElement
            {
                Id       = Guid.NewGuid(),
                DXUnitId = dto.Id,
                Name     = dto.Title
            }
        });
    }
}
```

Register it:

```csharp
builder.Services.AddDXUnitMapper<TBookMapper>();
```

---

## Convention mapping rules

`DXConventionMapper` pairs DTO and unit properties **by name** (case-insensitive) and applies one of two mapping strategies:

| DTO property type | Unit property type | Strategy |
|---|---|---|
| `T` (any) | `T` (same type) | Direct value copy (scalar) |
| `List<TElement>` | `DXMultiElementsContainer<TElement>` | Container ↔ list conversion |

Any DTO property that has no name match in the unit, or whose type is incompatible with the rules above, causes **startup validation to fail** with an `InvalidOperationException` listing all mismatches. Use a custom mapper when the shapes differ.

### Container mapping detail

- **`ToDtoAsync`** — reads `container.Announced` and copies all elements into a new `List<TElement>`.
- **`ToUnitAsync`** — calls `container.AddToAnnounced(item)` for every element in the DTO list. The resulting unit contains only those elements. If you need full-replace (Sync) semantics, set `container.Mode = MultiElementsMode.Full` in a custom mapper's `ToUnitAsync`.

---

## Using `IDXUnitDtoService<TDto>`

Inject the service anywhere — it is registered as Scoped.

```csharp
public class BooksController(IDXUnitDtoService<TBookDto> books) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<TBookDto?> Get(Guid id, CancellationToken ct)
        => await books.GetAsync(id, ct);

    [HttpGet]
    public async Task<IEnumerable<TBookDto>> GetAll(CancellationToken ct)
        => await books.GetAllAsync(ct);

    [HttpGet("search")]
    public async Task<IEnumerable<TBookDto>> Search([FromQuery] string filter, CancellationToken ct)
        => await books.GetAsync(filter, ct);  // raw DXSQL filter string

    [HttpPost]
    public async Task Save(TBookDto dto, CancellationToken ct)
        => await books.SaveAsync(dto, ct);

    [HttpDelete("{id}")]
    public async Task Delete(Guid id, CancellationToken ct)
        => await books.DeleteAsync(id, ct);
}
```

### `IDXUnitDtoService<TDto>` contract

```csharp
public interface IDXUnitDtoService<TDto>
{
    Task<TDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<TDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<TDto>> GetAsync(string filter, CancellationToken ct = default);
    Task SaveAsync(TDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

`GetAsync(string filter)` accepts a raw DXSQL filter expression. See [DXSQL.md](DXSQL.md) for syntax.

`SaveAsync` calls `InsertOrUpdateAsync` on the underlying data service — no separate insert/update distinction is needed at the DTO layer.

---

## Startup validation

Both registration paths fail fast:

- **Convention mapper** — `DXConventionMapper.Validate()` runs during `AddDXUnitMapper<TDto, TUnit>()`. A bad mapping throws `InvalidOperationException` before the host starts.
- **Custom mapper** — type is checked to ensure it derives from `DXUnitMapper<TDto, TUnit>`. Passing a type that does not throws `InvalidOperationException` at registration.

---

## Full example

```csharp
// Domain model
[DXUnit("TBookUnit")]
public class TBookUnit : DXUnit
{
    public string Title { get; set; } = string.Empty;
    public DXMultiElementsContainer<TBookTagElement> Tags { get; set; } = new();
}

[DXElement("TBookTagElement")]
public class TBookTagElement : DXElement
{
    public string Name { get; set; } = string.Empty;
}

// DTO
public class TBookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<TBookTagElement> Tags { get; set; } = [];
}

// Registration — convention mapper handles both scalar and container properties
builder.Services.AddDXUnitMapper<TBookDto, TBookUnit>();

// Usage
public class BooksController(IDXUnitDtoService<TBookDto> books) : ControllerBase
{
    [HttpGet("{id}")]
    public Task<TBookDto?> Get(Guid id, CancellationToken ct) => books.GetAsync(id, ct);

    [HttpPost]
    public Task Save(TBookDto dto, CancellationToken ct) => books.SaveAsync(dto, ct);
}
```
