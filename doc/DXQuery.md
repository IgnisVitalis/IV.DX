# DXQuery

## Purpose

DXQuery is a persisted, named query definition stored in the DX database. It specifies:

- which DXUnit type to read from (`DXUnitDefinition`);
- which columns to project (`DXQueryColumnElement`);
- an optional filter expression to narrow results (`FilterExpression`).

Queries are executed via `IDXQueryResultProvider.GetAsync` and return a structured result containing both the column schema and the data rows.

---

## Data model

### DXQueryUnit

Top-level query definition unit (`IV.DX.Kernel.Models.DXQueryUnit`).

| Column             | Type        | Required | Description                                                     |
|--------------------|-------------|----------|-----------------------------------------------------------------|
| `Name`             | string(50)  | Yes      | Human-readable query name                                       |
| `Description`      | text        | No       | Optional description                                            |
| `DXUnitDefinition` | Guid        | Yes      | FK to `DXUnitDefinitionUnit` — the target unit type to query    |
| `FilterExpression` | text        | No       | DXSQL WHERE expression applied when executing the query         |

### DXQueryColumnElement

Defines a projected column. A `DXQueryUnit` can have zero or more of these.

| Column       | Type       | Required | Description                                                        |
|--------------|------------|----------|--------------------------------------------------------------------|
| `Name`       | string(50) | Yes      | Output alias (field name in the result row)                        |
| `Expression` | text       | Yes      | DXSQL expression evaluated to produce the column value             |
| `Order`      | int        | Yes      | Sort order for consistent column ordering in the result definition |

The `ID` column is always included automatically regardless of what columns are defined.

---

## FilterExpression

`FilterExpression` is an optional DXSQL WHERE clause (see `doc/DXSQL.md`). When set, only rows that satisfy the expression are returned.

```
TUserMainElement.Name = 'Victor'
TUserMainElement.Birth > '2000-01-01' AND TUserMainElement.Name = 'Victor'
```

Rules:
- Uses DXSQL syntax — element navigation, U2U joins, and compound AND/OR are supported.
- No parentheses.
- Spaces required around operators.
- `null` or empty means no filter — all accessible rows are returned.

---

## Executing a query

```csharp
IDXQueryResultProvider provider = ...; // resolved from DI

JObject result = await provider.GetAsync(dxQueryId);
```

### Result shape

```json
{
  "QueryDefinition": [
    { "Name": "ID",   "Expression": "ID",   "Order": -1 },
    { "Name": "Name", "Expression": "TUserMainElement.Name", "Order": 0 }
  ],
  "Content": {
    "Meta": { "Kind": "DXUnit", "Type": "TUserUnit", "Op": "Sync", "IsMulti": true },
    "Data": {
      "Items": [
        { "ID": "...", "TimeStamp": "...", "Name": "Victor" }
      ]
    }
  }
}
```

- **`QueryDefinition`** — ordered array of column descriptors. The `ID` entry (Order -1) is always prepended. `ID`, `DXUnitID`, and `TimeStamp` fields are stripped from each descriptor since they are system columns.
- **`Content`** — a standard `DXDataBlock<DXUnitRecord>` (see `doc/DXDataFormat.md`). Each item contains only the projected columns plus `ID` and `TimeStamp`.

### Access control

`GetAsync` respects the active `DXExecutionContext`:

- **No context** — the query definition is read as system; content uses anonymous public fallback (only records exposed via `DXPublicAccessUnit` are returned for non-public types).
- **System context** (`IsSystem = true`) — full access to all rows.
- **User context** — access decisions follow the standard RBAC rules for the target unit type (`AllowedReadUnitTypes`, `AllowedOwnedOnly`, ownership, public access).

Sensitive columns (`EncryptedString`, `HashedString`) are masked to empty string in the result regardless of the caller's access level.

---

## Creating a query (C#)

```csharp
var query = new DXQueryUnit
{
    ID = Guid.NewGuid(),
    TimeStamp = DateTime.UtcNow,
    Name = "Active users",
    DXUnitDefinition = userUnitDefinitionId,
    FilterExpression = "TUserMainElement.Name = 'Victor'",
    DXQueryColumnElement = new DXMultiElementsContainer<DXQueryColumnElement>
    {
        Announced = new HashSet<DXQueryColumnElement>
        {
            new DXQueryColumnElement
            {
                ID = Guid.NewGuid(),
                DXUnitID = query.ID,
                TimeStamp = DateTime.UtcNow,
                Name = "Name",
                Expression = "TUserMainElement.Name",
                Order = 0
            }
        }
    }
};

await dataService.InsertOrUpdateAsync(query);
```

`DXUnitDefinition` must be the `DXUnitDefinitionUnit.ID` for the target type. To resolve it:

```csharp
var unitDef = structureCache.GetDXUnit("TUserUnit"); // by type name
Guid unitDefinitionId = unitDef.ID;
```

---

## Migration

`DXQueryUnit` and `DXQueryColumnElement` are registered via:

```
IV.DX.Kernel/Migration/DXQuery/01_01_0000_DXQuery_DXQueryColumnElement.dx
IV.DX.Kernel/Migration/DXQuery/01_01_0001_DXQuery_DXQueryUnit.dx
```

`DXQueryUnit` has a `DXUnitToUnitRelationElement` back to `DXUnitDefinitionUnit`:
- `OwnRelationName`: `DXQueries`
- `TargetRelationName`: `DXUnitDefinition`
- `RelationType`: 4 (MultiOptional)
