# DX Data Format (DX*Record)

This document describes the unified DX JSON envelope used for both metadata and runtime data.
The format transports DXUnit, DXElement, and DXEnum records using a single block model.

---

## 1) High-level shape

A file is a JSON array of independent blocks.

```json
[
  { "Meta": { ... }, "Data": { ... } },
  { "Meta": { ... }, "Data": { ... } }
]
```

- **Meta**: execution semantics (how to interpret/process)
- **Data**: payload (actual records)

Blocks may appear in any order unless your engine explicitly requires ordering.

---

## 2) Record shapes (C#)

```csharp
public sealed class DXDataBlock<TRecord>
{
    public DXMeta Meta { get; set; } = new DXMeta();
    public DXData<TRecord> Data { get; set; } = new DXData<TRecord>();
}

public sealed class DXMeta
{
    public string Kind { get; set; } = null!;  // "DXUnit" | "DXElement" | "DXEnum"
    public string? Type { get; set; }          // concrete type name (optional for DXEnum rows)

    public string? Op { get; set; }            // "Sync" | "Patch"
    public bool? IsMulti { get; set; }         // cardinality hint
    public bool? IsRequired { get; set; }      // cardinality hint

    public string? DXFilter { get; set; }      // selection filter (dxsql)
    public string? DXUnitContext { get; set; } // for standalone DXElements only
}

public sealed class DXData<TRecord>
{
    // JSON accepts a single object or an array for Items/Delete.
    public List<TRecord>? Items { get; set; }
    public List<DXDeleteRef>? Delete { get; set; }
}

public abstract class DXObjectRecord
{
    public Guid Id { get; set; }
    public DateTime TimeStamp { get; set; }
    public string? DXTitle { get; set; }   // computed display label — see section 11

    // All other properties (Name, Kind, etc.) are captured here.
    [JsonExtensionData]
    public IDictionary<string, JToken>? Fields { get; set; }
}

public sealed class DXUnitRecord : DXObjectRecord
{
    // Key = element type name, Value = element block
    public Dictionary<string, DXDataBlock<DXElementRecord>>? DXElements { get; set; }
}

public sealed class DXElementRecord : DXObjectRecord
{
    public Guid DXUnitId { get; set; }

    // Other DXElement columns (including system-managed ones like "DXUnitType") are captured in Fields.
}

public sealed class DXEnumRecord : DXObjectRecord
{
    public string? Type { get; set; } // used when Meta.Type is null
    public JToken? Key { get; set; }
    public JToken? Value { get; set; }
}

public sealed class DXDeleteRef
{
    public Guid Id { get; set; }

    // Optional extra fields (for disambiguation on delete).
    [JsonExtensionData]
    public IDictionary<string, JToken>? Fields { get; set; }
}
```

---

## 3) Meta fields

| Field          | Required                 | Applies to        | Meaning                                                               |
|----------------|--------------------------|-------------------|-----------------------------------------------------------------------|
| `Kind`         | Yes                      | all               | Object category (`DXUnit`, `DXElement`, `DXEnum`)                     |
| `Type`         | Usually                  | DXUnit/DXElement  | Concrete type name used by handlers                                   |
| `Type`         | Optional                 | DXEnum            | Can be omitted when each enum record has its own `Type`               |
| `Op`           | Optional                 | all               | `Sync` (full set semantics) or `Patch` (targeted changes)             |
| `IsMulti`      | Optional                 | all               | Cardinality hint for validation                                       |
| `IsRequired`   | Optional                 | all               | Cardinality hint for validation                                       |
| `DXFilter`     | Optional                 | usually DXUnit    | Selection filter for `Sync` operations                                |
| `DXUnitContext`| Required for standalone  | DXElement         | Execution context when a DXElement block is not nested inside a unit  |

> NOTE: `IsMulti` and `IsRequired` are hints for validation and defaults. JSON parsing accepts a single object or array for `Items` and `Delete`.

---

## 4) Cardinality mapping (IsMulti / IsRequired)

| IsMulti | IsRequired | Meaning                              |
|---------|------------|--------------------------------------|
| `false` | `true`     | Exactly one (`SingleMandatory`)      |
| `false` | `false`    | Zero or one (`SingleOptional`)       |
| `true`  | `true`     | One or more (`MultiMandatory`)       |
| `true`  | `false`    | Zero or more (`MultiOptional`)       |

This matches `DXElementInUnitTypeEnum` values in DXCore:
- `1` = SingleMandatory
- `2` = SingleOptional
- `3` = MultiMandatory
- `4` = MultiOptional

---

## 5) Op (processing mode)

### Patch
- apply `Items` items
- apply `Delete` items
- do **not** remove anything else implicitly

### Sync
- `Items` represents the desired final set **within the scope**
- objects missing from that set may be removed **within the scope**
- scope is typically defined by `Meta.DXFilter` (most commonly for “sync many” DXUnit blocks)

#### Sync scope rule (important)
Sync without an explicit scope is dangerous (it could imply “delete everything not present in `Items`”).

In this project’s implementation:
- implicit “delete-missing” is only performed when `Meta.Op = "Sync"` **and** `Meta.DXFilter` is provided (non-empty)
- if `Meta.DXFilter` is null/empty, Sync behaves like Patch (upsert `Items` + explicit `Delete` only)

`Meta.DXFilter` must be valid DXSQL (see `doc/DXSQL.md`). Expressions like `1=1` are not valid DXSQL; to match “everything” you’d use a valid DXSQL condition such as `Id IS NOT NULL` (be careful: that makes Sync operate over all rows of the type).

---

## 6) DXUnitRecord specifics

- A DXUnit record carries its columns as dynamic fields (e.g., `Name`, `DXTitleExpression`, `Kind`).
- Nested elements live in `DXElements` as a dictionary:
  - key = element type name
  - value = `DXDataBlock<DXElementRecord>`
- Nested element blocks usually **omit** `DXUnitContext` (context is the parent unit).
- Each nested `DXElementRecord` still includes `DXUnitId` (must match the parent unit Id).

### DerivedDXUnitType (system column on base unit tables)

When a DXUnit type is declared as a **derived type** (its `DXUnitDefinitionUnit` record has a `BaseDXUnit` reference), the physical table of the **base unit** gains a system-managed `DerivedDXUnitType` column.

- **Type**: `uuid` / `Guid`, NOT NULL.
- **Value**: the `DXUnitDefinitionUnit.Id` of the most-derived concrete type for that row.
  - A row that belongs to the base type itself (no further derivation) stores the **base type's own definition Id**.
  - A row that belongs to a derived type stores the **derived type's definition Id**.
- **Purpose**: allows polymorphic discrimination directly from the base table — you can tell whether a row is a pure base-type record or belongs to a more specific derived type without joining additional tables.
- **Relation**: the engine creates a `ManyToOne` relation from the base unit table to `DXUnitDefinitionUnit` under the relation name `DerivedDXUnitType`. The inverted (`OneToMany`) side lives on `DXUnitDefinitionUnit`.
- **Reads**: the field appears in `DXUnitRecord.Fields["DerivedDXUnitType"]` when reading base-unit records through `IDXUnitDataReader` or `IDXRawReader`.
- **Writers do not need to set this field** — the engine derives and writes it automatically on insert based on the concrete type being persisted.

---

## 7) DXElementRecord specifics

- `DXUnitId` is required.
- If the element is **standalone** (top-level block with `Kind = DXElement`), you must provide `Meta.DXUnitContext`.
- `Delete` references may include extra fields (e.g., `DXUnitId`) in `DXDeleteRef.Fields`.

### Common DXElements (`DXElementDefinitionUnit.IsCommon`)

Some DXElement types can be configured as **common** (by setting `IsCommon = true` on their `DXElementDefinitionUnit` record). This changes how containment is represented in storage to avoid creating a growing number of nullable `"<DXUnitTypeName>Id"` columns when the same DXElement type can belong to many different DXUnit types.

- When an element type is **common**, element rows use:
  - `DXUnitId` (the owning unit instance Id)
  - `DXUnitType` (the owning unit *definition* Id, i.e., `DXUnitDefinitionUnit.Id`)
- `DXUnitType` is treated as a system field and may appear inside `DXElementRecord.Fields` (and/or `DXDeleteRef.Fields`).
- Writers typically do **not** need to provide `DXUnitType` explicitly when nesting elements inside a DXUnit; the engine can derive it from the parent unit type.
- For standalone DXElement blocks, `Meta.DXUnitContext` remains required and provides the unit-type context used to infer the correct `DXUnitType` for common elements.

---

## 8) DXEnumRecord specifics

- `Key` and `Value` are the primary fields.
- If `Meta.Type` is omitted, each enum record **must** set its own `Type`.
- If `Meta.Type` is present, record-level `Type` is optional and may override.
- Other enum columns (if any) are captured in `Fields`.

---

## 9) Validation checklist

- `Meta.Kind` is present and is one of `DXUnit`, `DXElement`, `DXEnum`.
- `Meta.Type` is present for DXUnit/DXElement blocks; for DXEnum it can be omitted only if every record has `Type`.
- For standalone DXElement blocks, `Meta.DXUnitContext` is present.
- `Items`/`Delete` accept single object or array. If `IsMulti = true`, prefer array; if `IsMulti = false`, allow single object.
- If `IsRequired = true`, `Items` must exist (and must not be empty for multi).
- If `Meta.Op = Sync` and you expect “delete-missing”, ensure `Meta.DXFilter` is present to define the scope.
- DXUnit records:
  - `DXElements` keys are element type names.
  - Nested blocks must have `Meta.Kind = DXElement`.
  - Each nested element record has `DXUnitId` equal to the parent unit Id.
- DXElement records:
  - `DXUnitId` is required.
  - If present, `DXUnitType` (in `Fields`) is a GUID identifying the owning unit definition (used for common elements).
- DXEnum records:
  - `Key` and `Value` are required.
  - `Type` resolves from `Meta.Type` or record `Type` when Meta is missing.
- Delete refs:
  - `Id` is required.
  - Extra fields are allowed in `DXDeleteRef.Fields` for disambiguation.

---

## 10) Examples (from DXCore migration scripts)

### 10.1 DXUnit with nested DXElements (01_01_0002_DXCore_DXUnitDefinitionUnit.unit)

```json
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
        "Id": "018fa545-8876-7a5a-a72c-3fdaf537245d",
        "TimeStamp": "2021-10-02T00:00:00",
        "Name": "DXObjectDefinitionUnit",
        "DXTitleExpression": "Name",
        "Kind": 1,
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
                  "Id": "018fa545-8c5e-7ddf-8696-f3f0dfabd43a",
                  "TimeStamp": "2021-10-02T00:00:00",
                  "DXUnitId": "018fa545-8876-7a5a-a72c-3fdaf537245d",
                  "ColumnType": 3,
                  "Name": "Name",
                  "AllowNull": false,
                  "DefaultValue": null,
                  "Length": 100
                }
              ]
            }
          }
        }
      }
    ]
  }
}
```

### 10.2 DXEnum values with per-record Type (01_01_0005_DXCore_DXEnumDefinitionUnit.enum)

```json
{
  "Meta": {
    "Kind": "DXEnum",
    "Op": "Patch",
    "IsMulti": true,
    "IsRequired": false
  },
  "Data": {
    "Items": [
      {
        "Type": "DXElementInUnitTypeEnum",
        "Id": "018fa546-a1b6-78d8-b328-b2f2c17231de",
        "TimeStamp": "2021-10-02T00:00:00",
        "Key": 1,
        "Value": "SingleMandatory"
      },
      {
        "Type": "DXElementInUnitTypeEnum",
        "Id": "018fa546-a59e-728f-936a-e2ea0b18b5c6",
        "TimeStamp": "2021-10-02T00:00:00",
        "Key": 2,
        "Value": "SingleOptional"
      }
    ]
  }
}
```

### 10.3 Standalone DXElement block (01_01_0006_DXCore_DXElementToUnitElement.element)

```json
{
  "Meta": {
    "Kind": "DXElement",
    "Type": "DXElementToUnitRelationElement",
    "DXUnitContext": "DXElementDefinitionUnit",
    "Op": "Patch",
    "IsMulti": true,
    "IsRequired": false
  },
  "Data": {
    "Items": [
      {
        "Id": "018fa547-1ace-701a-9107-dbe3b4a261ce",
        "TimeStamp": "2021-10-02T00:00:00",
        "DXUnitId": "018fa545-78d6-7e8e-a30a-d48fbf08804a",
        "OwnRelationName": "RelatedDXUnits",
        "TargetRelationName": "TargetDXElement",
        "RelationType": 4,
        "TargetDXUnit": "018fa545-c30e-72aa-8c3d-eee1750d9731"
      }
    ]
  }
}
```

### 10.4 DXUnit inheritance init (01_01_0004_DXCore_DXInheritanceInitCore.unit)

```json
{
  "Meta": {
    "Kind": "DXUnit",
    "Type": "DXInheritanceInitCore",
    "Op": "Patch",
    "IsMulti": true,
    "IsRequired": false
  },
  "Data": {
    "Items": [
      {
        "Id": "018fa546-95fe-7e99-a7e7-3f1784a1dacd",
        "TimeStamp": "2021-10-02T00:00:00",
        "BaseDXUnit": "DXObjectDefinitionUnit",
        "ChildDXUnit": "DXUnitDefinitionUnit"
      }
    ]
  }
}
```

---

## 11) DXTitle — computed display label

Every `DXObjectRecord` carries a `DXTitle` property. It is a **read-only, system-managed** field that contains the human-readable label for that specific record instance.

### What it is

`DXTitle` is the evaluated result of `DXObjectDefinitionUnit.DXTitleExpression` — a DXExpression string configured per type (e.g. `"TUserMainElement.Name"` or `"Code"`). It is not stored in the database; it is computed at query time and injected into every full-load fetch.

On the C# domain model it is exposed as `DXUnit.DXTitle { get; }` (read-only from outside the framework).

### How to configure

Set `DXTitleExpression` on the `DXUnitDefinitionUnit` record for the type. The value is a DXExpression that the SQL builder evaluates as a column — element navigation is supported (e.g. `"TUserMainElement.Name"`).

```json
{
  "Id": "018fa54a-dbbe-79ce-9cdf-8fadedd7d372",
  "TimeStamp": "2021-10-02T00:00:00",
  "Name": "TUserUnit",
  "DXTitleExpression": "TUserMainElement.Name",
  "Kind": 3
}
```

If `DXTitleExpression` is empty or not set, the framework falls back to `"Id"` — the record's own Id becomes its title.

### When it is populated

`DXTitle` is populated on **full reads** (`DXLoadingType.Full`) via:
- `IDXUnitCoreRepository.GetItemRecord(string typeName, Guid id)`
- `IDXUnitGenericRepository.GetDXUnit<T>` / `GetDXUnits<T>` (all overloads)

It is **not** populated on base reads (Id-only queries) or when fetching elements nested inside a unit.

### JSON representation

`DXTitle` appears as a top-level field alongside `Id` and `TimeStamp` — it is **not** inside `Fields`:

```json
{
  "Id": "018fa54a-203e-7407-9bd0-cd287e850b03",
  "TimeStamp": "2021-10-02T00:00:00",
  "DXTitle": "Victor",
  "Name": "...",
  "Surname": "..."
}
```

`DXTitle` is a system-reserved name — user-defined type columns must not use this name.

---

## 12) ID generation

### Runtime operations — strict rules

ID generation at runtime is **unconditional** and **non-negotiable**: the application layer always controls the Id of any new record. The caller cannot supply or predict the Id of a record that does not yet exist in the database.

#### `InsertAsync` (typed, JObject, DXDataBlock overloads)

`AssignNewIds` is called unconditionally before the pipeline executes (when `DXMigrationContext.IsMigrating = false`):

- The unit `Id` is **always** replaced with a new UUID v7 (`Guid.CreateVersion7()`), regardless of what value the caller provided — including non-empty GUIDs.
- Every nested element record also gets a new UUID v7 assigned to its `Id`, and its `DXUnitId` is set to the newly assigned unit Id.

**There is no "if empty" condition on `InsertAsync`.** Any Id present in the incoming record is silently discarded.

#### `InsertOrUpdateAsync` (typed, JObject, DXDataBlock overloads)

Before deciding which path to take, the engine calls `IsItemExisting(typeName, record.Id)` against the database:

- **Record exists** (by Id) → routed to `UpdateAsync` → the existing Id is preserved, the record is updated.
- **Record does not exist** (regardless of whether the Id is `Guid.Empty` or any other non-empty GUID) → routed to `InsertAsync` → `AssignNewIds` runs → **the Id is always overwritten** with a new UUID v7.

**Consequence:** even if the caller provides a non-empty GUID for a new record, that GUID is discarded and replaced by the engine. The only way to know the actual assigned Id is to read the return value of the service call.

#### Caller contract

All insert and insert-or-update service methods return the assigned `Guid`. This is the **only source of truth** for the Id of a newly created record. Callers must capture and use this returned value — they must not assume their input Id was preserved.

UUID v7 is time-ordered (monotonically increasing), which gives good index locality.

### Migration scripts

Migration scripts run under `DXMigrationContext.IsMigrating = true`. In this mode `AssignNewIds` is **not called** — the engine uses the Id already set on the record exactly as provided.

**Migration scripts must always provide an explicit, non-empty, predefined Id for every record.** This is a hard requirement:

- If a migration record has `Id = Guid.Empty` (all zeros), the migration process **throws an exception** — it does not silently write a bad record.
- Ids must be stable across runs to ensure migrations are deterministic and repeatable, allowing safe upsert semantics (`Op: Patch`).

```json
{
  "Id": "018fa545-8876-7a5a-a72c-3fdaf537245d",
  "TimeStamp": "2021-10-02T00:00:00",
  "Name": "DXObjectDefinitionUnit"
}
```

---

End of document.

