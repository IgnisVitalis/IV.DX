# Getting Started with IV.DX

IV.DX is a DB-driven framework for defining, persisting, and querying structured data using a schema-first model (DXUnits and DXElements), with optional RBAC, pipeline hooks, and query support.

---

## 1) Installation

Add the NuGet package to your project:

```xml
<PackageReference Include="IV.DX" Version="x.x.x" />
```

The package bundles all layers: domain, persistence, application services, and hosting extensions.

---

## 2) Configuration

IV.DX requires a PostgreSQL database (MySQL is also supported) and reads its configuration from `IConfiguration`. Non-secret settings go in `appsettings.json`; secrets are always provided via environment variables.

### 2.1 appsettings.json — non-secret settings only

```json
{
  "Security": {
    "JwtIssuer": "MyApp",
    "JwtAudience": "MyApp.Client",
    "AccessTokenLifetimeMinutes": 15,
    "RefreshTokenLifetimeDays": 30
  }
}
```

`Security` is required only when you call `.AddSecurity()` on the builder.

### 2.2 Secrets — environment variables

Never store secrets in `appsettings.json`. All secrets live under a single `Secrets` section and are provided via environment variables using .NET's double-underscore convention (`Secrets__Key` maps to `Secrets:Key` in `IConfiguration`):

| Environment Variable                | Required                       | Description                                                         |
|-------------------------------------|--------------------------------|---------------------------------------------------------------------|
| `Secrets__DatabaseConnectionString` | Yes                            | Database connection string.                                         |
| `Secrets__DatabaseType`             | Yes                            | Database type: `PostgreSQL`, `MySQL`.                               |
| `Secrets__EncryptionKey`            | Yes                            | Base64-encoded 32-byte AES key. Generate: `openssl rand -base64 32` |
| `Secrets__JwtSigningKey`            | Yes, if using `.AddSecurity()` | JWT signing key, min 32 chars.                                      |

Environment variables override any value from `appsettings.json`. See [section 2.3](#23-per-environment-examples) for per-environment examples.

### 2.3 Encryption key rotation

IV.DX supports **zero-downtime encryption key rotation** for single-instance apps and provides a pluggable `IDXEncryptionKeyProvider` interface for advanced scenarios such as microservice deployments.

#### How it works (single instance)

IV.DX stores the active encryption key in a state file (`encryption-key-state.json` next to the application binary). On startup, `DXEncryptionRotationService` (an `IHostedService` registered automatically by `RegisterHostedService()`) compares the key in the environment variable against the state file:

| Condition                           | Action                                                                                                |
|-------------------------------------|-------------------------------------------------------------------------------------------------------|
| State file missing (first startup)  | Write current key to state file — no migration needed.                                                |
| State file matches current key      | No-op.                                                                                                |
| State file differs from current key | Load **both** keys into memory; run background re-encryption migration; update state file on success. |

During migration the app is fully operational:
- New writes use the current key.
- Reads transparently decrypt using either the current or the previous key based on the `keyId` stored with each value.
- If migration fails (partial), the state file is **not** updated and the previous key remains available — simply restart to retry.

#### Step-by-step rotation (single instance)

1. Generate a new key:
   ```bash
   openssl rand -base64 32
   ```

2. Update the environment variable — just change the key value:
   ```
   Secrets__EncryptionKey=<new-base64>
   ```
   That's it. No other variables need to change.

3. Restart the app. On startup:
   - `DXConfiguredEncryptionKeyProvider` derives a stable Id from each key's bytes, so the new and old keys automatically get different IDs without any manual configuration.
   - It loads both keys into memory: current from env, previous from the state file.
   - `DXEncryptionRotationService` detects the key change and starts background re-encryption.
   - The app is immediately available — reads transparently decrypt using whichever key matches the `kid=` in each stored value.

4. Monitor logs for the completion message:
   ```
   Encryption key rotation complete. N record(s) re-encrypted.
   ```

5. The state file is updated automatically on success. No further action needed.

> **State file security**: `encryption-key-state.json` contains the previous key in plaintext Base64. Protect it with OS file permissions (e.g., `chmod 600` on Linux). This is the same security level as the environment variable itself. Never commit the state file to source control — add it to `.gitignore`.

#### Custom `IDXEncryptionKeyProvider` (microservices)

For microservice deployments where multiple instances share the same database, the state-file approach does not coordinate across instances. Implement `IDXEncryptionKeyProvider` with a shared key store (Consul, Kubernetes secrets, AWS Secrets Manager, etc.) and register it instead of the default:

```csharp
// Register your custom provider — must be done before calling AddDX.
services.AddSingleton<IDXEncryptionKeyProvider, MySharedKeyProvider>();

// Then add DX as normal.
builder.Services
    .AddDX(builder.Configuration)
    .RegisterHostedService();
```

> When a custom provider is detected, `DXEncryptionRotationService` is a **no-op** — your provider is solely responsible for rotation logic. You can still call `IDXEncryptionMigrationService.MigrateAsync()` manually or from a single designated coordinator instance.

`IDXEncryptionKeyProvider` contract:

```csharp
public interface IDXEncryptionKeyProvider
{
    /// <summary>Returns the key used to encrypt new data.</summary>
    DXEncryptionKey GetCurrent();

    /// <summary>Tries to find a key by its Id (used when decrypting existing data).</summary>
    bool TryGet(string keyId, out DXEncryptionKey key);
}
```

`DXEncryptionKey` holds a `KeyId` (string) and `KeyBytes` (byte[32]).

#### Triggering re-encryption manually

`IDXEncryptionMigrationService` is publicly available from DI. Call it from your own management endpoint, background job, or CLI tool:

```csharp
var migrationService = scope.ServiceProvider.GetRequiredService<IDXEncryptionMigrationService>();
var result = await migrationService.MigrateAsync();

Console.WriteLine($"Re-encrypted: {result.Reencrypted}, Failed: {result.Failed}");
// result.IsComplete == true when Failed == 0
```

The service finds all unit types that have at least one `EncryptedString` column, reads every record, and re-saves it — which triggers encryption with the current key.

### 2.4 Per-environment examples

**Development — `launchSettings.json`** (not committed to git):

```json
{
  "profiles": {
    "MyApp": {
      "commandName": "Project",
      "environmentVariables": {
        "Secrets__DatabaseConnectionString": "Host=localhost;Database=myapp;Username=postgres;Password=secret",
        "Secrets__DatabaseType": "PostgreSQL",
        "Secrets__EncryptionKey": "<base64-32-bytes>",
        "Secrets__JwtSigningKey": "dev-signing-key-at-least-32-characters"
      }
    }
  }
}
```

Alternatively use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "Secrets:DatabaseConnectionString" "Host=localhost;Database=myapp;Username=postgres;Password=secret"
dotnet user-secrets set "Secrets:DatabaseType" "PostgreSQL"
dotnet user-secrets set "Secrets:EncryptionKey" "<base64-32-bytes>"
dotnet user-secrets set "Secrets:JwtSigningKey" "dev-signing-key-at-least-32-characters"
```

**Production — Linux (systemd)**:

```ini
# /etc/myapp/secrets  (chmod 600, chown myapp:myapp)
Secrets__DatabaseConnectionString=Host=db.internal;Database=myapp;Username=app;Password=...
Secrets__DatabaseType=PostgreSQL
Secrets__EncryptionKey=...
Secrets__JwtSigningKey=...
```

```ini
# /etc/systemd/system/myapp.service
[Service]
User=myapp
EnvironmentFile=/etc/myapp/secrets
ExecStart=/usr/bin/dotnet /opt/myapp/MyApp.dll
```

**Production — Windows (Windows Service)**:

```powershell
# Run as Administrator — scoped to the service user account
[System.Environment]::SetEnvironmentVariable("Secrets__DatabaseConnectionString", "Host=db;Database=myapp;Username=app;Password=...", "Machine")
[System.Environment]::SetEnvironmentVariable("Secrets__DatabaseType", "PostgreSQL", "Machine")
[System.Environment]::SetEnvironmentVariable("Secrets__EncryptionKey", "...", "Machine")
[System.Environment]::SetEnvironmentVariable("Secrets__JwtSigningKey", "...", "Machine")
```

> For stricter isolation set variables directly on the service via the Windows Service registry key `HKLM\SYSTEM\CurrentControlSet\Services\<name>\Environment` instead of machine-level.

**Docker**:

```bash
docker run \
  -e Secrets__DatabaseConnectionString="Host=db;Database=myapp;Username=app;Password=secret" \
  -e Secrets__DatabaseType="PostgreSQL" \
  -e Secrets__EncryptionKey="..." \
  -e Secrets__JwtSigningKey="..." \
  myapp:latest
```

```yaml
# docker-compose.yml
services:
  myapp:
    image: myapp:latest
    environment:
      Secrets__DatabaseConnectionString: "Host=db;Database=myapp;Username=app;Password=secret"
      Secrets__DatabaseType: "PostgreSQL"
      Secrets__EncryptionKey: "..."
      Secrets__JwtSigningKey: "..."
```

For production use Docker secrets or an external secrets manager (AWS Secrets Manager, Azure Key Vault, HashiCorp Vault) that injects values as environment variables.

---

## 3) Service registration and initialization

IV.DX uses a builder API to register services and declare which optional modules are active. All schema migrations and startup logic run automatically.

### ASP.NET Core / Generic Host (recommended)

```csharp
using IV.DX.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDX(builder.Configuration)
    .AddHandlers(typeof(MyHandler).Assembly)  // your domain handlers
    .AddSecurity()                            // optional: RBAC and auth
    .AddCustomData("MigrationScripts/MyApp.json") // your schema
    .RegisterHostedService();                 // runs automatically on startup
```

`RegisterHostedService()` registers an `IHostedService` that runs schema bootstrap and handler initialization before the app starts accepting requests. You do not need to call anything else.

### Non-hosted apps (console, desktop, tests)

```csharp
var services = new ServiceCollection();

services
    .AddDX(configuration)
    .AddHandlers(typeof(MyHandler).Assembly)
    .AddSecurity()                            // optional
    .AddCustomData("MigrationScripts/MyApp.json")
    .Build();

var root = services.BuildServiceProvider();

await root.StartDXAsync(); // run schema bootstrap and handler initialization
```

### What gets initialized

| Always                                     | Optional                                           |
|--------------------------------------------|----------------------------------------------------|
| DXCore schema (tables, system definitions) | `.AddSecurity()` — RBAC, roles, identity schema    |
| DXQuery schema (named query definitions)   | `.AddCustomData(path)` — your own migration script |

Custom data migration is idempotent — re-running on an existing database is safe.

---

## 5) Defining your domain model

### 5.1 DXUnit (root entity)

A DXUnit is the top-level persistent object. It must be decorated with `[DXUnit]` and extend `DXUnit`.

```csharp
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

[DXUnit("TBookUnit")]
public class TBookUnit : DXUnit
{
    // Single mandatory element
    public TBookMainElement TBookMainElement { get; set; }

    // Multi-optional elements
    public DXMultiElementsContainer<TBookChapterElement> TBookChapterElement { get; set; }
}
```

#### DXUnit inheritance and DerivedDXUnitType

DXUnit types support single-table inheritance. A derived type declares its base type via the `BaseDXUnit` field in its `DXUnitDefinitionUnit` migration record. The physical table of the **base unit** is shared across all types in the hierarchy.

When at least one derived type exists, the base unit table gains a system-managed `DerivedDXUnitType` column (`uuid NOT NULL`). This column stores the `DXUnitDefinitionUnit.Id` of the most-derived concrete type for each row:

| Row belongs to                    | DerivedDXUnitType value       |
|-----------------------------------|-------------------------------|
| Base type (no further derivation) | Base type's own definition Id |
| Derived type                      | Derived type's definition Id  |

This enables polymorphic discrimination directly from the base table — you can identify the concrete type of any row without joining additional tables.

The column is **set automatically by the engine on insert** based on the concrete type being persisted. It is available in `DXUnitRecord.Fields["DerivedDXUnitType"]` when reading via `IDXUnitDataReader` or `IDXRawReader`.

Example migration for a derived type:

```json
{
  "Id": "020357c3-bfb2-4583-b285-3ed31e0e24f7",
  "TimeStamp": "2021-10-02T00:00:00",
  "Name": "TComputerUnit",
  "Kind": 3,
  "BaseDXUnit": "018fa549-e1be-70ce-81f7-a6f7554ffdde"
}
```

Here `BaseDXUnit` references the `DXUnitDefinitionUnit.Id` of `TDeviceUnit`. After migration, every row in the `TDeviceUnit` table carries a `DerivedDXUnitType` value — `7f8501fd…` for pure device rows and `020357c3…` for computer rows.

### 5.2 DXElement (owned data block)

A DXElement belongs to a DXUnit. It must be decorated with `[DXElement]` and extend `DXElement`.

```csharp
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;

[DXElement("TBookMainElement")]
public class TBookMainElement : DXElement
{
    [DXColumn("Name")]
    public string Name { get; set; }
}

[DXElement("TBookChapterElement")]
public class TBookChapterElement : DXElement
{
    [DXColumn("Number")]
    public int Number { get; set; }

    [DXColumn("Text")]
    public string Text { get; set; }
}
```

### 5.3 DXColumn attribute

`[DXColumn(name, dxExpression, loadingType)]`

| Parameter       | Meaning                                                          |
|-----------------|------------------------------------------------------------------|
| `name`          | Column name in the database table                                |
| `dxExpression`  | DXSQL expression used for queries (defaults to `name` if omitted)|
| `loadingType`   | `DXLoadingType.Full` (default) or `DXLoadingType.Base`           |

`DXLoadingType.Base` columns are always loaded. `Full` columns load only when full loading is requested.

---

## 6) Schema migration scripts

IV.DX uses `.dx` files (JSON arrays of `DXDataBlock`) to define schema and seed data. A migration config file (`.json`) is an ordered list of `.dx` file paths relative to itself.

### 6.1 Migration config (MyApp.json)

```json
[
  "MyApp/01_01_0000_TBookMainElement.dx",
  "MyApp/01_01_0001_TBookChapterElement.dx",
  "MyApp/01_01_0002_TBookUnit.dx"
]
```

### 6.2 Defining an element type (.dx)

This creates the `TBookMainElement` element type with a `Name` column:

```json
[
  {
    "Meta": {
      "Kind": "DXUnit",
      "Type": "DXElementDefinitionUnit",
      "Op": "Patch",
      "IsMulti": true,
      "IsRequired": false
    },
    "Data": {
      "Items": [
        {
          "Id": "018fa549-aef6-778f-bad1-f12283c79aaa",
          "TimeStamp": "2021-10-02T00:00:00",
          "Name": "TBookMainElement",
          "Kind": 3,
          "DXElements": {
            "DXColumnDefinitionElement": {
              "Meta": {
                "Kind": "DXElement",
                "Type": "DXColumnDefinitionElement",
                "Op": "Patch",
                "IsMulti": true,
                "IsRequired": false
              },
              "Data": {
                "Items": [
                  {
                    "Id": "018fa549-b2de-7741-8193-83e882ced388",
                    "TimeStamp": "2021-10-02T00:00:00",
                    "DXUnitId": "018fa549-aef6-778f-bad1-f12283c79aaa",
                    "ColumnType": 3,
                    "Name": "Name",
                    "Length": 100,
                    "AllowNull": true,
                    "DefaultValue": null
                  }
                ]
              }
            }
          }
        }
      ]
    }
  }
]
```

### 6.3 Defining a unit type (.dx)

This creates the `TBookUnit` unit type and links its elements:

```json
[
  {
    "Meta": {
      "Kind": "DXUnit",
      "Type": "DXUnitDefinitionUnit",
      "Op": "Patch",
      "IsMulti": true,
      "IsRequired": false
    },
    "Data": {
      "Items": [
        {
          "Id": "018fa549-e98e-7d20-a34a-26a03317a998",
          "TimeStamp": "2021-10-02T00:00:00",
          "Name": "TBookUnit",
          "Kind": 3,
          "DXElements": {
            "DXElementInUnitDefinitionElement": {
              "Meta": {
                "Kind": "DXElement",
                "Type": "DXElementInUnitDefinitionElement",
                "Op": "Patch",
                "IsMulti": true,
                "IsRequired": false
              },
              "Data": {
                "Items": [
                  {
                    "Id": "018fa549-ed76-7778-9aa9-8b30594bda4e",
                    "TimeStamp": "2021-10-02T00:00:00",
                    "DXUnitId": "018fa549-e98e-7d20-a34a-26a03317a998",
                    "RelationType": 1,
                    "DXElementDefinitionUnit": "018fa549-aef6-778f-bad1-f12283c79aaa"
                  }
                ]
              }
            }
          }
        }
      ]
    }
  }
]
```

`RelationType` values map to `DXElementInUnitTypeEnum`:
- `1` = SingleMandatory
- `2` = SingleOptional
- `3` = MultiMandatory
- `4` = MultiOptional

See [DXDataFormat.md](DXDataFormat.md) for the full data format reference.

### 6.4 Unique constraints

`DXUniqueColumnsElement` adds a `UNIQUE` database constraint across one or more columns of the element or unit table. Multiple entries may coexist on the same type, each describing a different column combination.

#### Naming convention

Constraints are named automatically as:

```
UC_{tableName}_{col1}_{col2}...
```

Where the column names are **sorted alphabetically** before joining with `_`. For example, a constraint on `name` and `surname` for table `TBookMainElement` is always named `UC_TBookMainElement_name_surname`, regardless of the order the columns appear in the definition.

#### Column order independence and deduplication

The engine normalizes column sets before persisting or comparing them:

- `"name,surname"` and `"surname,name"` are treated as identical — they produce the same constraint.
- If duplicate column sets appear in `Announced` (including reversed-order duplicates), only one record is kept.
- When removing a constraint in TargetMode, the column order in the `Deleted` entry does not need to match what is stored in the database.

#### TargetMode vs FullMode semantics

`DXUniqueColumnsElement` participates in the standard `DXMultiElementsContainer` modes:

**`Target`** — explicit add/remove:
- `Announced` — add these constraints (skipped if already present).
- `Deleted` — remove these constraints.
- All other existing constraints are untouched.

**`Full`** — declared desired state:
- `Announced` is the complete set that should exist after the operation.
- Constraints in the DB but absent from `Announced` are dropped.
- Constraints in `Announced` but absent from the DB are added.
- Matching records are preserved as-is (no delete + re-insert).

#### Using relation-generated columns in unique constraints

Unique constraints are processed **after** relation columns are created. This means a column that is introduced by a relation definition (e.g., a foreign-key column added automatically when linking a DXElement to a DXUnit) can appear in a `DXUniqueColumnsElement.Columns` value and the constraint will be applied correctly.

#### C# API

```csharp
// Insert a type with a unique constraint on (name, surname)
var element = new DXElementDefinitionUnit
{
    Id = Guid.NewGuid(),
    Name = "TPersonMainElement",
    // ... columns ...
    DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
    {
        Mode = MultiElementsMode.Target,
        Announced = new HashSet<DXUniqueColumnsElement>
        {
            new DXUniqueColumnsElement
            {
                Id = Guid.NewGuid(),
                DXUnitId = element.Id,
                Columns = "name,surname"   // column order does not matter
            }
        }
    }
};

await dataService.InsertAsync(element);

// FullMode update — replace the constraint with a new one
element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
{
    Mode = MultiElementsMode.Full,
    Announced = new HashSet<DXUniqueColumnsElement>
    {
        new DXUniqueColumnsElement { Id = Guid.NewGuid(), DXUnitId = element.Id, Columns = "email" }
    }
};

await dataService.UpdateAsync(element);
```

#### Migration script (.dx)

```json
{
  "Id": "018fa549-aef6-778f-bad1-f12283c79aaa",
  "TimeStamp": "2024-01-01T00:00:00",
  "Name": "TPersonMainElement",
  "Kind": 3,
  "DXElements": {
    "DXColumnDefinitionElement": {
      "Meta": { "Kind": "DXElement", "Type": "DXColumnDefinitionElement", "Op": "Patch", "IsMulti": true, "IsRequired": false },
      "Data": {
        "Items": [
          { "Id": "aaaa0001-0000-0000-0000-000000000001", "DXUnitId": "018fa549-aef6-778f-bad1-f12283c79aaa", "Name": "name",    "ColumnType": 3, "Length": 100, "DefaultValue": "''" },
          { "Id": "aaaa0001-0000-0000-0000-000000000002", "DXUnitId": "018fa549-aef6-778f-bad1-f12283c79aaa", "Name": "surname", "ColumnType": 3, "Length": 100, "DefaultValue": "''" },
          { "Id": "aaaa0001-0000-0000-0000-000000000003", "DXUnitId": "018fa549-aef6-778f-bad1-f12283c79aaa", "Name": "email",   "ColumnType": 3, "Length": 200, "DefaultValue": "''" }
        ]
      }
    },
    "DXUniqueColumnsElement": {
      "Meta": { "Kind": "DXElement", "Type": "DXUniqueColumnsElement", "Op": "Patch", "IsMulti": true, "IsRequired": false },
      "Data": {
        "Items": [
          {
            "Id": "bbbb0001-0000-0000-0000-000000000001",
            "DXUnitId": "018fa549-aef6-778f-bad1-f12283c79aaa",
            "Columns": "name,surname"
          },
          {
            "Id": "bbbb0001-0000-0000-0000-000000000002",
            "DXUnitId": "018fa549-aef6-778f-bad1-f12283c79aaa",
            "Columns": "email"
          }
        ]
      }
    }
  }
}
```

This creates two constraints: `UC_TPersonMainElement_name_surname` and `UC_TPersonMainElement_email`.

---

## 7) Reading and writing data

Inject `IDXUnitDataService` (writes) and `IDXUnitDataReader` (reads) from DI.

### 7.1 Insert

```csharp
var book = new TBookUnit
{
    Id = Guid.NewGuid(),
    TBookMainElement = new TBookMainElement
    {
        Id = Guid.NewGuid(),
        DXUnitId = /* same as TBookUnit.Id */ book.Id,
        Name = "My Book"
    }
};

var inserted = await dataService.InsertAsync(book);
```

### 7.2 Update

```csharp
book.TBookMainElement.Name = "Updated Title";
var updated = await dataService.UpdateAsync(book);
```

### 7.3 Delete

```csharp
await dataService.DeleteAsync(book);
```

### 7.4 Read by Id

```csharp
var book = await dataReader.GetItemAsync<TBookUnit>(id);
```

### 7.5 Read all

```csharp
var books = await dataReader.GetItemsAsync<TBookUnit>();
```

### 7.6 Read with DXSQL filter

```csharp
var books = await dataReader.GetItemsAsync<TBookUnit>(
    "TBookMainElement.Name = 'My Book'");
```

See [DXSQL.md](DXSQL.md) for the full filter syntax reference.

---

## 8) Pipeline handlers

Handlers let you run logic before or after CRUD operations on a specific unit type. Implement one or more handler interfaces and scan the assembly during registration.

```csharp
using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;

public class TBookUnitHandler :
    IDXBeforeInsertHandler<TBookUnit>,
    IDXAfterInsertHandler<TBookUnit>
{
    public int Order => 0;

    public async Task<DXResult<TBookUnit>> BeforeInsertAsync(
        TBookUnit unit, DXHandlerBaseContext ctx, CancellationToken ct)
    {
        // Validate or mutate unit before insert
        if (string.IsNullOrEmpty(unit.TBookMainElement?.Name))
            return DXResult<TBookUnit>.Fail("Name is required.");

        return DXResult<TBookUnit>.OkContinue(unit);
    }

    public async Task<DXResult<TBookUnit>> AfterInsertAsync(
        TBookUnit unit, DXHandlerBaseContext ctx, CancellationToken ct)
    {
        // Post-insert side effects
        return DXResult<TBookUnit>.OkContinue(unit);
    }
}
```

Register the handler assembly after building the container:

```csharp
app.Services.InitializeDXHandlers(typeof(TBookUnitHandler).Assembly);
```

Available handler interfaces:

| Interface                          | When called                      |
|------------------------------------|----------------------------------|
| `IDXBeforeInsertHandler<T>`        | Before insert persistence        |
| `IDXAfterInsertHandler<T>`         | After insert persistence         |
| `IDXBeforeUpdateHandler<T>`        | Before update persistence        |
| `IDXAfterUpdateHandler<T>`         | After update persistence         |
| `IDXBeforeDeleteHandler<T>`        | Before delete persistence        |
| `IDXAfterDeleteHandler<T>`         | After delete persistence         |
| `IDXBeforeGetHandler<T>`           | Before get/read                  |
| `IDXAfterGetHandler<T>`            | After get/read                   |
| `IDXIsItemExistingHandler<T>`      | Custom existence check           |

`DXResult.Flow` controls pipeline continuation:
- `DXFlow.Continue` — proceed to next handler / persistence
- `DXFlow.SkipProcess` — skip persistence, continue post-handlers
- `DXFlow.Stop` — halt the pipeline

---

## 9) Security and authentication

Security is optional. Enable it by calling `InitDXSecurityDataAsync` during bootstrap.

### 9.1 Register a user

```csharp
var security = scope.ServiceProvider.GetRequiredService<IDXSecurityService>();

var result = await security.RegisterLocalAsync(new DXRegisterLocalRequest
{
    Username = "alice",
    Password = "s3cret"
});

// result.AccessToken, result.RefreshToken
```

### 9.2 Login

```csharp
var result = await security.LoginLocalAsync(new DXLoginLocalRequest
{
    Username = "alice",
    Password = "s3cret"
});
```

### 9.3 Refresh token

```csharp
var result = await security.RefreshAsync(new DXRefreshRequest
{
    IdentityLoginId = result.IdentityLoginId,
    SessionId = result.SessionId,
    RefreshToken = result.RefreshToken
});
```

### 9.4 Binding JWT to execution context

The JWT contains claims `dx_login_id` and `sid`. After validating the token in your middleware, bind the execution context:

```csharp
var resolver = scope.ServiceProvider.GetRequiredService<IDXExecutionContextResolver>();
var accessor = scope.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();

var context = await resolver.ResolveAsync(loginId, sessionId);

using (accessor.BeginScope(context))
{
    // All DX service calls inside this scope are subject to RBAC
    var books = await dataReader.GetItemsAsync<TBookUnit>();
}
```

Without an execution context scope, security is evaluated as `Denied` for non-system operations.

See [DXSecurity.md](DXSecurity.md) for the full RBAC model reference.

---

## 10) Query provider

`IDXQueryResultProvider` executes pre-defined DXQuery configurations stored in the database and returns structured `JObject` results.

```csharp
var queryProvider = scope.ServiceProvider.GetRequiredService<IDXQueryResultProvider>();

// Execute a query by its stored Id
var result = await queryProvider.GetAsync(dxQueryId, dxFilterId: null);

// Get display values (Name/Id pairs) for a type
var DXTitleExpressions = await queryProvider.GetDXTitleExpressionsAsync("TBookUnit");
```

DXQuery is always initialized as part of the standard startup — no extra configuration needed.

---

## 12) DTO mapper

`IDXUnitDtoService<TDto>` is a ready-made CRUD service that bridges your DTOs to the DX domain model. Register it with one call — either using the built-in convention mapper (property-name matching, validated at startup) or a hand-written `DXUnitMapper<TDto, TUnit>`:

```csharp
// Convention mapper — no mapper code required
builder.Services.AddDXUnitMapper<TBookDto, TBookUnit>();

// Custom mapper — full control over the transform
builder.Services.AddDXUnitMapper<TBookMapper>();
```

Inject `IDXUnitDtoService<TDto>` wherever you need it:

```csharp
public class BooksController(IDXUnitDtoService<TBookDto> books) : ControllerBase
{
    [HttpGet("{id}")]
    public Task<TBookDto?> Get(Guid id, CancellationToken ct) => books.GetAsync(id, ct);

    [HttpPost]
    public Task Save(TBookDto dto, CancellationToken ct) => books.SaveAsync(dto, ct);
}
```

See [DXUnitDtoMapper.md](DXUnitDtoMapper.md) for the full reference including convention mapping rules, container semantics, startup validation, and examples.

---

## 11) Startup sequence summary

```csharp
// ASP.NET Core / Generic Host
builder.Services
    .AddDX(builder.Configuration)
    .AddHandlers(typeof(MyHandler).Assembly)
    .AddSecurity()                                   // optional
    .AddCustomData("MigrationScripts/MyApp.json")
    .RegisterHostedService();                        // everything runs automatically

var app = builder.Build();
```

```csharp
// Non-hosted (console, desktop, tests)
services
    .AddDX(configuration)
    .AddHandlers(typeof(MyHandler).Assembly)
    .AddSecurity()                                   // optional
    .AddCustomData("MigrationScripts/MyApp.json")
    .Build();

var root = services.BuildServiceProvider();
await root.StartDXAsync();
```
