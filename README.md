# IV.DX

**A schema-first, database-driven data framework for .NET 9.**

IV.DX lets you describe a domain as *definitions stored in the database* rather than as a hand-written
schema. Types, columns, relations, constraints, queries and access rules all live as data (`DXUnit` /
`DXElement` records), and the engine derives the physical tables, the SQL and the runtime services from
them. Applications get typed CRUD services, a filter DSL, a handler pipeline, a DTO mapping layer, RBAC
and transparent column encryption on top of that model.

Current version: **0.112.0** — see [CHANGELOG.md](CHANGELOG.md).

---

## Packages

| Package            | Contents                                                                                  |
|--------------------|-------------------------------------------------------------------------------------------|
| `IV.DX`            | Everything storage-agnostic: domain kernel, persistence, application services, hosting/DI. Carries no database driver. |
| `IV.DX.PostgreSQL` | The PostgreSQL provider (Npgsql): dialect, schema helper, migration lock. Depends on `IV.DX`. |

```xml
<PackageReference Include="IV.DX" Version="x.x.x" />
<PackageReference Include="IV.DX.PostgreSQL" Version="x.x.x" />
```

The provider is chosen **in code**, not in configuration — the core knows nothing about which databases
exist. PostgreSQL is currently the only provider; another one can be added without touching `IV.DX`.

---

## Quick start

**1. Secrets** — always via environment variables (`Secrets__Key` maps to `Secrets:Key`):

| Variable                            | Required                     | Meaning                                             |
|-------------------------------------|------------------------------|-----------------------------------------------------|
| `Secrets__DatabaseConnectionString` | yes                          | Database connection string                          |
| `Secrets__EncryptionKey`            | yes                          | Base64 32-byte AES key (`openssl rand -base64 32`)  |
| `Secrets__JwtSigningKey`            | when `.AddSecurity()` is used | JWT signing key, min. 32 chars                     |

**2. Registration** — one builder chain; migrations and initialization run automatically:

```csharp
using IV.DX.Hosting;
using IV.DX.PostgreSQL;

builder.Services
    .AddDX(builder.Configuration)
    .UsePostgreSQL()                              // database provider
    .AddHandlers(typeof(MyHandler).Assembly)      // pipeline handlers
    .AddSecurity()                                // optional: RBAC + auth
    .AddCustomData("MigrationScripts/MyApp.json") // your schema
    .RegisterHostedService();                     // bootstrap on startup
```

For console, desktop or test hosts, end the chain with `.Build()` and call `await root.StartDXAsync()`.

**3. Model:**

```csharp
[DXUnit("TBookUnit")]
public class TBookUnit : DXUnit
{
    public TBookMainElement TBookMainElement { get; set; }                      // single mandatory
    public DXMultiElementsContainer<TBookChapterElement> TBookChapterElement { get; set; } // multi
}

[DXElement("TBookMainElement")]
public class TBookMainElement : DXElement
{
    [DXColumn("Name")] public string Name { get; set; }
}
```

**4. Use it:**

```csharp
await dataService.InsertAsync(book);
var one   = await dataReader.GetItemAsync<TBookUnit>(id);
var found = await dataReader.GetItemsAsync<TBookUnit>("TBookMainElement.Name = 'My Book'");
```

Full walkthrough: [doc/getting-started.md](doc/getting-started.md).

---

## Core concepts

**DXUnit** — the root persistent entity, one physical table. **DXElement** — an owned data block belonging
to a unit, related to it as single/multi and mandatory/optional (`DXElementInUnitTypeEnum`). An element type
marked `IsCommon` may be reused by several unit types.

**Definitions as data.** `DXUnitDefinitionUnit`, `DXElementDefinitionUnit`, `DXColumnDefinitionElement`,
`DXUniqueColumnsElement`, relation and enum definition types describe the schema. The engine reads them,
diffs against the live database and applies the DDL.

**Migration scripts.** Schema and seed data ship as `.dx` files — JSON arrays of `DXDataBlock` in a single
envelope shared by units, elements and enums, with `Patch` and `Sync` processing modes. A `.json` config
file lists the `.dx` files in order. Migration is idempotent, so re-running against an existing database is
safe, and a distributed lock keeps concurrent instances from migrating at once. Format reference:
[doc/DXDataFormat.md](doc/DXDataFormat.md).

**Inheritance.** Unit types support single-table inheritance via `BaseDXUnit`; the shared base table gains a
system-managed `DerivedDXUnitType` column holding the definition id of the most-derived concrete type, so
rows can be discriminated without extra joins.

**Unique constraints.** `DXUniqueColumnsElement` produces `UC_{table}_{cols…}` constraints with columns
normalized and sorted, so column order never creates a duplicate; `Target` mode adds/removes, `Full` mode
declares the complete desired set.

---

## Features

### DXSQL — filter and projection DSL
A compact dot-separated path language over unit/element/relation navigation, compiled to SQL joins and
`WHERE` clauses by `SQLQueryBuilder`. Used both for `DXFilter` and for query column projections.
→ [doc/DXSQL.md](doc/DXSQL.md)

### DXQuery — persisted named queries
Query definitions live in the database (source type, projected columns, optional filter expression) and are
executed through `IDXQueryResultProvider`, returning column schema plus rows as `JObject`. Access control
applies to results. → [doc/DXQuery.md](doc/DXQuery.md)

### Handler pipeline
Per-unit-type hooks around every operation: `IDXBeforeInsertHandler<T>` / `IDXAfterInsertHandler<T>` and the
update, delete, get and existence equivalents, ordered by `Order`. A handler returns a `DXResult<T>` whose
`Flow` (`Continue`, `SkipProcess`, `Stop`) drives the rest of the pipeline — validation, mutation and side
effects all live here rather than in the caller.

### DTO mappers
`IDXUnitDtoService<TRequest, TResponse>` is ready-made CRUD between your DTOs and the domain model, with
read-only and write-only variants. Either use the convention mapper (property-name matching, no mapper class,
validated at startup) via `AddDXUnitMapper<TDto, TUnit>()`, or write a `DXUnitMapper<TDto, TUnit>` for full
control. Element mappers (`AddDXElementMapper<…>`) address a single element **without loading its unit**, so
editing one chapter of a book is one row write instead of a read-modify-write of the whole unit; every
operation has an owner-scoped overload for nested routes. → [doc/DXUnitDtoMapper.md](doc/DXUnitDtoMapper.md)

### Security — authentication and RBAC
Optional, enabled by `.AddSecurity()`. Local registration/login, JWT access tokens and rotating refresh
sessions. Authorization is **not** encoded in the JWT: the token identifies login and session, and grants are
resolved from persisted data at runtime. Roles grant per-operation access (`Read`, `Create`, `Update`,
`Delete`) to unit types with `Allow`/`Deny` effects; instance-level `DXIdentityOwnershipUnit` /
`DXGroupOwnershipUnit` rows are themselves grants, so co-owners of one record can hold different rights.
Type flags (`SupportsOwnership`, `IsPublicRead`, `AllowAuthenticatedCreate`) tune the model per type, and
public read has both type-level and entry-level forms. Calls are authorized inside an execution-context
scope; without one, non-system operations are denied. → [doc/DXSecurity.md](doc/DXSecurity.md)

### Column encryption with key rotation
`EncryptedString` columns are encrypted at rest, each value tagged with the key id that wrote it. Changing
`Secrets__EncryptionKey` and restarting is enough: both keys are loaded, reads decrypt with whichever key
matches, and `DXEncryptionRotationService` re-encrypts in the background while the app stays available. For
multi-instance deployments, implement `IDXEncryptionKeyProvider` against a shared key store and drive
`IDXEncryptionMigrationService` yourself.

### DX Action
A command framework: actions are identified by `Module + Key`, their parameter metadata is stored as
`DXActionDefinitionUnit` records, implementations are DI-injected C# classes with attributed In/Out
properties, and `IDXActionExecutor` runs any of them by name — callers never reference concrete action
classes. Actions can be replaced or extended without editing the original.
→ [doc/DXAction.md](doc/DXAction.md)

---

## Architecture

```
IV.DX.Kernel                 domain models, attributes, enums, DX data format, embedded core migrations
  ├─ IV.DX.Persistence.Contracts   repository ports, SQL/provider seam, execution context, access checker
  │    └─ IV.DX.Persistence        repositories, SQLQueryBuilder, structure cache, migration lock
  └─ IV.DX.Application.Contracts   service interfaces, mappers, handler contracts, runtime types
       └─ IV.DX.Application        data/DTO/query/security/encryption services, pipeline, actions
            └─ IV.DX.Hosting       DI builder, initializer, hosted service, key + context providers
                 └─ IV.DX.PostgreSQL   Npgsql provider: dialect, schema helper, migration lock
```

Layers below `IV.DX.Hosting` are referenced with `PrivateAssets="all"` and folded into the single `IV.DX`
package; the provider seam sits at the repository port, so a new database means a new provider package only.

Repository layout: sources in [src/IV.DX/](src/IV.DX/), tests in [src/IV.DX/Tests/](src/IV.DX/Tests/),
documentation in [doc/](doc/), model diagrams (drawio + mermaid) in [diagrams/](diagrams/), packaging
scripts in [scripts/](scripts/).

---

## Build, test, pack

Requires the .NET SDK pinned in [global.json](global.json) (9.0.306, latest patch) and a PostgreSQL instance
for the integration tests.

```bash
dotnet build src/IV.DX/IV.DX.sln
dotnet test  src/IV.DX/Tests/IntTests/IV.DX.Persistence.IntTests/IV.DX.Persistence.IntTests.csproj
dotnet test  src/IV.DX/Tests/IntTests/IV.DX.Application.IntTests/IV.DX.Application.IntTests.csproj
```

Unit test projects cover the kernel, contracts, hosting and shared helpers. `scripts/pack.ps1` (and the
per-package wrappers) build NuGet packages into a local feed; `.github/workflows/publish-nuget.yml` packs and
publishes both packages on a `v*` tag, gated by a minimum coverage threshold.

---

## Known gaps

Tracked in [TASK.md](TASK.md): no optimistic concurrency token on unit/element writes (a full `PUT` is
last-writer-wins), no convention mapper for elements, the handler pipeline does not yet run on the
element-scoped path, and generated SQL table aliases are numbered by physical row order.
