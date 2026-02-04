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
    public Guid ID { get; set; }
    public DateTime TimeStamp { get; set; }

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
    public Guid DXUnitID { get; set; }
}

public sealed class DXEnumRecord : DXObjectRecord
{
    public string? Type { get; set; } // used when Meta.Type is null
    public JToken? Key { get; set; }
    public JToken? Value { get; set; }
}

public sealed class DXDeleteRef
{
    public Guid ID { get; set; }

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
- scope may be defined by `DXFilter` (typically for DXUnit sync-many)

---

## 6) DXUnitRecord specifics

- A DXUnit record carries its columns as dynamic fields (e.g., `Name`, `DisplayValue`, `Kind`).
- Nested elements live in `DXElements` as a dictionary:
  - key = element type name
  - value = `DXDataBlock<DXElementRecord>`
- Nested element blocks usually **omit** `DXUnitContext` (context is the parent unit).
- Each nested `DXElementRecord` still includes `DXUnitID` (must match the parent unit ID).

---

## 7) DXElementRecord specifics

- `DXUnitID` is required.
- If the element is **standalone** (top-level block with `Kind = DXElement`), you must provide `Meta.DXUnitContext`.
- `Delete` references may include extra fields (e.g., `DXUnitID`) in `DXDeleteRef.Fields`.

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
- DXUnit records:
  - `DXElements` keys are element type names.
  - Nested blocks must have `Meta.Kind = DXElement`.
  - Each nested element record has `DXUnitID` equal to the parent unit ID.
- DXElement records:
  - `DXUnitID` is required.
- DXEnum records:
  - `Key` and `Value` are required.
  - `Type` resolves from `Meta.Type` or record `Type` when Meta is missing.
- Delete refs:
  - `ID` is required.
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
        "ID": "2a30fc41-144d-45a8-b74a-e4ca528fc81c",
        "TimeStamp": "2021-10-02T00:00:00",
        "Name": "DXObjectDefinitionUnit",
        "DisplayValue": "Name",
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
                  "ID": "2a8e6b99-37ec-45dd-8dd1-c6163e56fb36",
                  "TimeStamp": "2021-10-02T00:00:00",
                  "DXUnitID": "2a30fc41-144d-45a8-b74a-e4ca528fc81c",
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
        "ID": "56cfe59b-069a-4bc6-ac44-59cac46d7153",
        "TimeStamp": "2021-10-02T00:00:00",
        "Key": 1,
        "Value": "SingleMandatory"
      },
      {
        "Type": "DXElementInUnitTypeEnum",
        "ID": "08e793f0-07c9-4fc5-818f-515d74731b65",
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
        "ID": "72f6f3f3-e55f-4a24-915d-893b69932f67",
        "TimeStamp": "2021-10-02T00:00:00",
        "DXUnitID": "65bd9684-6709-409a-a46b-7c605dcb715b",
        "OwnRelationName": "RelatedDXUnits",
        "TargetRelationName": "TargetDXElement",
        "RelationType": 4,
        "TargetDXUnit": "cee041ff-53d1-46cc-b2ae-d9cb4db0e577"
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
        "ID": "ddb4f6d1-af51-47b1-860a-bdaae6a67555",
        "TimeStamp": "2021-10-02T00:00:00",
        "BaseDXUnit": "DXObjectDefinitionUnit",
        "ChildDXUnit": "DXUnitDefinitionUnit"
      }
    ]
  }
}
```

---

End of document.

