# DX DTO Mapper

The DTO mapper feature provides a thin application-layer bridge between the DX domain model and the DTOs your API or UI exposes. It handles the mechanical work of copying scalar properties and synchronizing element containers, leaving your code free of boilerplate reflection.

Request and response shapes are first-class: the mapper hierarchy supports symmetric (same DTO in/out), read-only, write-only, and fully asymmetric (different request/response) configurations.

There are two parallel families. **Unit mappers** address a whole `DXUnit` and are what most endpoints use. **Element mappers** address a single `DXElement` without loading the unit that owns it — see [Element mappers](#element-mappers).

---

## Concepts

| Type | Role |
|---|---|
| `DXUnitMapper<TRequest, TResponse, TUnit>` | Abstract base for full CRUD mappers. Implement both directions. |
| `DXUnitReadMapper<TResponse, TUnit>` | Abstract base for read-only mappers. Implement `ToDtoAsync` only. |
| `DXUnitWriteMapper<TRequest, TUnit>` | Abstract base for write-only mappers. Implement `ToUnitAsync` only. |
| `DXConventionMapper<TDto, TUnit>` | Built-in symmetric convention mapper. No code required. |
| `IDXUnitDtoService<TRequest, TResponse>` | Full CRUD service. Inject in controllers/services. |
| `IDXUnitQueryService<TResponse>` | Read-only service. Inject when writes are not needed. |
| `IDXUnitCommandService<TRequest>` | Write-only service. Inject when reads are not needed. |

`IDXUnitDtoService<TRequest, TResponse>` extends both `IDXUnitQueryService<TResponse>` and `IDXUnitCommandService<TRequest>`.

Registration wires everything together: one call creates the mapper, wraps it in the appropriate service interface, and registers both with the DI container.

---

## Registration

### Convention mapper — symmetric, no code required

Use when request and response shapes are identical and all properties match by name.

```csharp
builder.Services.AddDXUnitMapper<TBookDto, TBookUnit>();
```

Registers `IDXUnitDtoService<TBookDto, TBookDto>`. Validates the mapping at startup — a property mismatch throws `InvalidOperationException` before the host starts.

### Full CRUD custom mapper

Use when request and response have different shapes, or when custom logic is needed in either direction.

Derive from `DXUnitMapper<TRequest, TResponse, TUnit>`:

```csharp
public class BookMapper : DXUnitMapper<BookRequest, BookResponse, BookUnit>
{
    private readonly IEnumService _enums;

    public BookMapper(IEnumService enums) => _enums = enums;

    public override async Task<BookResponse> ToDtoAsync(BookUnit unit, CancellationToken ct = default)
    {
        return new BookResponse
        {
            Id        = unit.Id,
            Title     = unit.TBookMainElement?.Name ?? string.Empty,
            Genre     = await _enums.ResolveAsync(unit.Genre, ct),  // int → { key, value }
            UpdatedAt = unit.TimeStamp
        };
    }

    public override Task<BookUnit> ToUnitAsync(BookRequest dto, CancellationToken ct = default)
    {
        return Task.FromResult(new BookUnit
        {
            Id    = dto.Id,
            Genre = dto.Genre,  // int key only
            TBookMainElement = new TBookMainElement { Name = dto.Title }
        });
    }
}
```

Register it:

```csharp
builder.Services.AddDXUnitMapper<BookMapper>();
```

Registers `IDXUnitDtoService<BookRequest, BookResponse>`.

### Read-only custom mapper

Use when an endpoint only reads — no writes, no `SaveAsync`/`DeleteAsync` needed.

Derive from `DXUnitReadMapper<TResponse, TUnit>`:

```csharp
public class BookReadMapper : DXUnitReadMapper<BookResponse, BookUnit>
{
    public override Task<BookResponse> ToDtoAsync(BookUnit unit, CancellationToken ct = default)
        => Task.FromResult(new BookResponse { Id = unit.Id, Title = unit.TBookMainElement?.Name });
}
```

Register it:

```csharp
builder.Services.AddDXUnitReadMapper<BookReadMapper>();
```

Registers `IDXUnitQueryService<BookResponse>`.

### Write-only custom mapper

Use when an endpoint only writes — no reads, no `GetAsync`/`GetAllAsync` needed.

Derive from `DXUnitWriteMapper<TRequest, TUnit>`:

```csharp
public class BookWriteMapper : DXUnitWriteMapper<BookRequest, BookUnit>
{
    public override Task<BookUnit> ToUnitAsync(BookRequest dto, CancellationToken ct = default)
        => Task.FromResult(new BookUnit { Id = dto.Id, Genre = dto.Genre });
}
```

Register it:

```csharp
builder.Services.AddDXUnitWriteMapper<BookWriteMapper>();
```

Registers `IDXUnitCommandService<BookRequest>`.

---

## Element mappers

Use these when an endpoint addresses one element of a unit — a chapter of a book, a line of an order — rather than the unit as a whole. The element's own row is read and written directly, so the unit is never loaded, never rewritten, and its `TimeStamp` does not move.

| Type | Role |
|---|---|
| `DXElementMapper<TRequest, TResponse, TElement, TUnit>` | Abstract base for full CRUD element mappers. |
| `DXElementReadMapper<TResponse, TElement, TUnit>` | Read-only. Implement `ToDtoAsync` only. |
| `DXElementWriteMapper<TRequest, TElement, TUnit>` | Write-only. Implement `ToElementAsync` only. |
| `IDXElementDtoService<TRequest, TResponse>` | Full CRUD service. |
| `IDXElementQueryService<TResponse>` | Read-only service. |
| `IDXElementCommandService<TRequest>` | Write-only service. |

`TUnit` is a type argument, not something inferred from `TElement`: an element declared `IsCommon` can belong to several unit types, and both the access rules and the storage layout depend on which one is meant.

```csharp
public class ChapterMapper : DXElementMapper<ChapterRequest, ChapterResponse, BookChapterElement, BookUnit>
{
    public override Task<ChapterResponse> ToDtoAsync(BookChapterElement element, CancellationToken ct = default)
        => Task.FromResult(new ChapterResponse
        {
            Id     = element.Id,
            BookId = element.DXUnitId,
            Number = element.Number,
            Title  = element.Title
        });

    public override Task<BookChapterElement> ToElementAsync(ChapterRequest dto, CancellationToken ct = default)
        => Task.FromResult(new BookChapterElement
        {
            Id     = dto.Id,
            Number = dto.Number,
            Title  = dto.Title
        });
}
```

Register it:

```csharp
builder.Services.AddDXElementMapper<ChapterMapper>();       // IDXElementDtoService<ChapterRequest, ChapterResponse>
builder.Services.AddDXElementReadMapper<ChapterReadMapper>();   // IDXElementQueryService<ChapterResponse>
builder.Services.AddDXElementWriteMapper<ChapterWriteMapper>(); // IDXElementCommandService<ChapterRequest>
```

### Service interfaces

```csharp
public interface IDXElementQueryService<TResponse>
{
    Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<TResponse>> GetByUnitAsync(Guid dxUnitId, CancellationToken ct = default);
}

public interface IDXElementCommandService<TRequest>
{
    Task<Guid> CreateAsync(Guid dxUnitId, TRequest dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(TRequest dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
```

`CreateAsync` takes the owner as an argument rather than reading it off the DTO, so the caller decides where it came from — a route segment for a nested resource (`/books/{bookId}/chapters`), the body for a flat one — without every request shape having to carry it. An id on the DTO is ignored on create; the server assigns one.

`ToElementAsync` must set `Id` for updates to work — that is how the service knows which element is meant. It does **not** need to set `DXUnitId`: on create the owner comes from the argument, and on update it is read from storage.

### Access

Element access is decided against the **owning unit type**, since an element has no grants of its own:

| Operation | Requires |
|---|---|
| `GetAsync`, `GetByUnitAsync` | `Read` on the unit type, narrowed to units the caller may see |
| `CreateAsync`, `UpdateAsync`, `DeleteAsync` | `Update` on the specific unit that owns the element, or ownership of it |

Writes need `Update` rather than `Create` or `Delete` because none of them brings a unit into being or ends it — they change a unit's contents, which is what a whole-unit write with a modified element container already does.

An update resolves the owning unit from storage, so an element cannot be moved to another unit through this path: a request naming an element of one unit alongside a different owner is rejected.

### Limitations

- **No handler pipeline.** Unlike the unit services, no before/after handlers run. A handler registered for a unit does not see an element written this way.
- **No convention mapper yet.** There is no element equivalent of `DXConventionMapper`; write the mapper by hand.
- **No controller bases.** `IV.DX.WebApi` has `DXUnitQueryControllerBase` and `DXUnitCommandControllerBase` but no element counterparts.

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

## Service interfaces

### `IDXUnitDtoService<TRequest, TResponse>` — full CRUD

```csharp
public interface IDXUnitDtoService<TRequest, TResponse>
    : IDXUnitQueryService<TResponse>
    , IDXUnitCommandService<TRequest>
{ }
```

### `IDXUnitQueryService<TResponse>` — reads only

```csharp
public interface IDXUnitQueryService<TResponse>
{
    Task<TResponse?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<TResponse>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<TResponse>> GetAsync(string filter, CancellationToken ct = default);
}
```

### `IDXUnitCommandService<TRequest>` — writes only

```csharp
public interface IDXUnitCommandService<TRequest>
{
    Task<Guid> SaveAsync(TRequest dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

`GetAsync(string filter)` accepts a raw DXSQL filter expression. See [DXSQL.md](DXSQL.md) for syntax.

`SaveAsync` calls `InsertOrUpdateAsync` on the underlying data service — no separate insert/update distinction is needed at the DTO layer.

---

## Startup validation

All registration paths fail fast:

- **Convention mapper** — `DXConventionMapper.Validate()` runs during `AddDXUnitMapper<TDto, TUnit>()`. A bad mapping throws `InvalidOperationException` before the host starts.
- **Custom mapper** — type is checked to ensure it derives from the expected base class (`DXUnitMapper<,,>`, `DXUnitReadMapper<,>`, or `DXUnitWriteMapper<,>`). Passing an incompatible type throws `InvalidOperationException` at registration.

---

## Full example — asymmetric request/response

```csharp
// Domain model
[DXUnit("BookUnit")]
public class BookUnit : DXUnit
{
    public int Genre { get; set; }
    public DXMultiElementsContainer<BookTagElement> Tags { get; set; } = new();
}

// Request DTO — client sends int key for enum fields, no timestamp
public class BookRequest
{
    public Guid Id    { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Genre  { get; set; }
}

// Response DTO — server returns resolved enum object and audit fields
public class BookResponse
{
    public Guid Id        { get; set; }
    public string Title   { get; set; } = string.Empty;
    public GenreDto Genre { get; set; } = null!;  // { key, value }
    public DateTime UpdatedAt { get; set; }
}

public record GenreDto(int Key, string Value);

// Custom mapper
public class BookMapper : DXUnitMapper<BookRequest, BookResponse, BookUnit>
{
    private readonly IEnumService _enums;
    public BookMapper(IEnumService enums) => _enums = enums;

    public override async Task<BookResponse> ToDtoAsync(BookUnit unit, CancellationToken ct = default)
        => new BookResponse
        {
            Id        = unit.Id,
            Title     = unit.TBookMainElement?.Name ?? string.Empty,
            Genre     = await _enums.ResolveAsync(unit.Genre, ct),
            UpdatedAt = unit.TimeStamp
        };

    public override Task<BookUnit> ToUnitAsync(BookRequest dto, CancellationToken ct = default)
        => Task.FromResult(new BookUnit
        {
            Id    = dto.Id,
            Genre = dto.Genre,
            TBookMainElement = new TBookMainElement { Name = dto.Title }
        });
}

// Registration
builder.Services.AddDXUnitMapper<BookMapper>();

// Controller
public class BooksController(IDXUnitDtoService<BookRequest, BookResponse> books) : ControllerBase
{
    [HttpGet("{id}")]
    public Task<BookResponse?> Get(Guid id, CancellationToken ct)
        => books.GetAsync(id, ct);

    [HttpGet]
    public Task<IEnumerable<BookResponse>> GetAll(CancellationToken ct)
        => books.GetAllAsync(ct);

    [HttpPost]
    public Task<Guid> Save(BookRequest dto, CancellationToken ct)
        => books.SaveAsync(dto, ct);

    [HttpDelete("{id}")]
    public Task Delete(Guid id, CancellationToken ct)
        => books.DeleteAsync(id, ct);
}
```

## Full example — symmetric convention mapper

When request and response shapes are identical, no mapper class is needed:

```csharp
// Single DTO used for both directions
public class TBookDto
{
    public Guid Id        { get; set; }
    public DateTime TimeStamp { get; set; }
}

// Registration
builder.Services.AddDXUnitMapper<TBookDto, TBookUnit>();

// Controller
public class BooksController(IDXUnitDtoService<TBookDto, TBookDto> books) : ControllerBase
{
    [HttpGet("{id}")]
    public Task<TBookDto?> Get(Guid id, CancellationToken ct) => books.GetAsync(id, ct);

    [HttpPost]
    public Task<Guid> Save(TBookDto dto, CancellationToken ct) => books.SaveAsync(dto, ct);
}
```
