# DX Data Format (Metadata & Data)

This document describes the unified DX JSON envelope format used for **both metadata and runtime data**.
It is designed to transport, synchronize, patch, and initialize **DXUnit**, **DXElement**, and **DXEnum** objects consistently.

---

## 1) High-level shape

A file is a JSON array of **independent blocks**.

```json
[
  { "Meta": { ... }, "Data": { ... } },
  { "Meta": { ... }, "Data": { ... } }
]
```

- **Meta**: execution semantics (how to interpret/process)
- **Data**: payload (actual object fields)

No batching requirements: blocks may appear in any order unless your engine explicitly requires ordering.

---

## 2) Kinds

### DXUnit
A unit object (definition or runtime record depending on `Type`).

### DXElement
A unit-owned object (definition or runtime record). When processed standalone, it requires a **DXUnitContext**.

### DXEnum
Enum values (`Key`/`Value`) and/or enum-related records.

---

## 3) Meta

### 3.1 Meta schema

```json
"Meta": {
  "Kind": "DXUnit | DXElement | DXEnum",
  "Type": "ConcreteTypeName",
  "Op": "Sync | Patch",
  "IsMulti": true,
  "IsRequired": false,
  "DXFilter": "dxsql expression",
  "DXUnitContext": "DXUnitTypeName"
}
```

### 3.2 Meta fields

| Field | Required | Applies to | Meaning |
|------|----------|------------|---------|
| `Kind` | ✔ | all | Object category (`DXUnit`, `DXElement`, `DXEnum`) |
| `Type` | ✔ | all | Concrete type name used by handlers |
| `Op` | ⭕ | all | `Sync` (full set semantics) or `Patch` (targeted changes) |
| `IsMulti` | ⭕ | all | If `true` the payload shape is array-based; if `false` single-object |
| `IsRequired` | ⭕ | all | If `true`, the payload must be present and non-empty (see rules) |
| `DXFilter` | ⭕ | typically DXUnit | Selection filter for `Sync` operations (dxsql) |
| `DXUnitContext` | ⭕ | DXElement | Execution context for standalone DXElements |

> Note: `IsMulti` + `IsRequired` replaces the old `Cardinality` string.

---

## 4) Cardinality via `IsMulti` / `IsRequired`

These two flags fully describe the expected “cardinality”:

| IsMulti | IsRequired | Meaning |
|--------|------------|---------|
| `false` | `true` | Exactly one (`One`) |
| `false` | `false` | Zero or one (`ZeroOrOne`) |
| `true` | `true` | One or more (`OneOrMore`) |
| `true` | `false` | Zero or more (`Many`) |

### Validation guideline
- If `IsMulti = false`, `Data.Upsert` may be an object (single) **or** an array of size 1 (choose one convention and keep it consistent).
- If `IsMulti = true`, `Data.Upsert` is an array.
- If `IsRequired = true`:
  - single: `Upsert` must exist
  - multi: `Upsert` must exist and have at least one item

---

## 5) Op (processing modes)

### Patch
Targeted operation:
- apply `Upsert` items
- apply `Delete` items
- **do not** remove anything else implicitly

### Sync
Full-set operation:
- `Upsert` represents the desired final set within the selection scope
- objects missing from that set may be removed **within the scope**

> Scope may be:
> - explicit list only (common)
> - selection by `DXFilter` (optional, typically for DXUnit sync-many)

---

## 6) Data

`Data` contains **only payload**. No execution instructions.

### 6.1 Common pattern

```json
"Data": {
  "Upsert": [ ... ],
  "Delete": [ ... ]
}
```

- `Upsert`: objects to create or update
- `Delete`: references to remove (shape depends on your engine; usually `{ "ID": "..." }` or a dedicated `...DeleteRef`)

---

## 7) Examples

### 7.1 DXElement — Patch, Many, not required

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
    "Upsert": [
      {
        "ID": "72f6f3f3-e55f-4a24-915d-893b69932f67",
        "TimeStamp": "2021-10-02T00:00:00",
        "DXUnitID": "65bd9684-6709-409a-a46b-7c605dcb715b",
        "OwnRelationName": "RelatedDXUnits",
        "TargetRelationName": "TargetDXElement",
        "RelationType": 4,
        "TargetDXUnit": "cee041ff-53d1-46cc-b2ae-d9cb4db0e577"
      }
    ],
    "Delete": [
      { "ID": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" }
    ]
  }
}
```

### 7.2 DXUnit — Sync, Many, required (full set)

```json
{
  "Meta": {
    "Kind": "DXUnit",
    "Type": "DXUnitDefinitionUnit",
    "Op": "Sync",
    "IsMulti": true,
    "IsRequired": true
  },
  "Data": {
    "Upsert": [
      {
        "ID": "2a30fc41-144d-45a8-b74a-e4ca528fc81c",
        "TimeStamp": "2021-10-02T00:00:00",
        "Name": "DXObjectDefinitionUnit",
        "DisplayValue": "Name",
        "Kind": 1
      }
    ]
  }
}
```

### 7.3 DXUnit — Sync, Many with DXFilter (scoped sync)

```json
{
  "Meta": {
    "Kind": "DXUnit",
    "Type": "DXUnitDefinitionUnit",
    "Op": "Sync",
    "IsMulti": true,
    "IsRequired": true,
    "DXFilter": "Kind = 1 AND Name LIKE 'DX%DefinitionUnit'"
  },
  "Data": {
    "Upsert": [
      { "ID": "...", "TimeStamp": "...", "Name": "DXUnitDefinitionUnit", "Kind": 1 }
    ]
  }
}
```

### 7.4 DXEnum — Patch, Many, required

```json
{
  "Meta": {
    "Kind": "DXEnum",
    "Type": "DXColumnTypeEnum",
    "Op": "Patch",
    "IsMulti": true,
    "IsRequired": true
  },
  "Data": {
    "Upsert": [
      {
        "ID": "b1477a09-7d88-4e77-9b57-98a8e31eab27",
        "TimeStamp": "2021-10-02T00:00:00",
        "Key": 3,
        "Value": "String"
      }
    ]
  }
}
```

### 7.5 Single object forms

#### Exactly one (single required)

```json
{
  "Meta": {
    "Kind": "DXUnit",
    "Type": "DXInheritanceInitCore",
    "Op": "Patch",
    "IsMulti": false,
    "IsRequired": true
  },
  "Data": {
    "Upsert": {
      "ID": "ddb4f6d1-af51-47b1-860a-bdaae6a67555",
      "TimeStamp": "2021-10-02T00:00:00",
      "BaseDXUnit": "DXObjectDefinitionUnit",
      "ChildDXUnit": "DXUnitDefinitionUnit"
    }
  }
}
```

#### Optional single (zero-or-one)

```json
{
  "Meta": {
    "Kind": "DXElement",
    "Type": "DXUniqueColumnsElement",
    "DXUnitContext": "DXRelationDefinitionUnit",
    "Op": "Patch",
    "IsMulti": false,
    "IsRequired": false
  },
  "Data": {
    "Upsert": null
  }
}
```

---

## 8) Rules & invariants

- `DXUnitContext` is **required** when `Kind = DXElement` and the element is processed standalone.
- `Meta` must not contain business fields.
- `Data` must not contain execution semantics.
- Object payloads should include `ID` and `TimeStamp` (and `DXUnitID` for DXElements) according to your domain rules.

---

## 9) Minimal C# Meta model

```csharp
public sealed class DXMeta
{
    public string Kind { get; set; } = null!;          // "DXUnit" | "DXElement" | "DXEnum"
    public string Type { get; set; } = null!;          // concrete type name

    public string? Op { get; set; }                    // "Sync" | "Patch"
    public bool? IsMulti { get; set; }                 // replaces Cardinality
    public bool? IsRequired { get; set; }              // replaces Cardinality

    public string? DXFilter { get; set; }              // selection filter (dxsql)
    public string? DXUnitContext { get; set; }         // for standalone DXElement
}
```

---

End of document.