# DXSQL (DXFilter / DXQuery DSL)

This document describes the DXSQL string DSL used by `SQLQueryBuilder` to build SQL SELECTs.
It is used in `DXFilter` (WHERE) and in column projections.

## 1) Purpose
- Express navigation across DXUnit / DXElement / DXProperty relations.
- Build SQL joins + WHERE conditions from a compact string.

## 2) Core navigation rules
DXSQL uses a dot-separated path:

```
segment.segment.segment.Property <op> <value>
```

Segments map to relations:

### 2.1 DXUnit -> DXUnit (equal relation)
```
U2U(RelationName)
```
Example:
```
U2U(User).ID = '...'
```

### 2.2 DXUnit -> DXElement (containment)
Containment (element is part of unit) uses the **element name with no prefix**.

Example:
```
TUserMainElement.Name = 'Svitlana'
```

### 2.3 DXElement -> DXUnit (containment, reverse)
Reverse containment uses:
```
E2UIn(UnitName)
```
Example:
```
TUserMainElement.E2UIn(TUserUnit).ID = '...'
```

### 2.4 DXUnit -> DXElement (equal relation)
Explicit Unit<->Element relations use:
```
U2E(RelationName)
```

### 2.5 DXElement -> DXUnit (equal relation)
Explicit reverse Unit<->Element relations use:
```
E2U(RelationName)
```

## 3) Filters (DXFilter)

### 3.1 Syntax
```
condition (AND|OR condition)*

condition := path '.' property <space> <operator+value>
path      := segment ('.' segment)*
segment   := U2U(...), U2E(...), E2U(...), E2UIn(...), or ElementName
```

### 3.2 Important parsing rules
- `AND` / `OR` are split by **literal tokens** with spaces:
  - " and ", " or ", " AND ", " OR ", " And ", " Or "
- There is **no parentheses support**.
- The property name is split from the operator/value at the **first space**.
  - Valid: `Name = 'x'`
  - Invalid: `Name='x'` (no space before operator)
- The operator/value part is copied as-is. DXSQL does not validate operators.

### 3.3 Examples
```
U2U(User).TUserMainElement.Name = 'Svitlana'
U2U(User).TUserMainElement.Name = 'Svitlana' AND U2U(User).TUserMainElement.Surname = 'Suvorova'
U2U(User).U2U(Position).TPositionMainElement.Name = 'Master'
```

## 4) Column projections (DXQuery columns)

Columns use the same path syntax, but end with **only a property name**:

```
columns["Alias"] = "U2U(User).TUserMainElement.Name"
columns["BookName"] = "TBookMainElement.Name"
```

If `columns` is empty, `SQLQueryBuilder` emits `"T_*".*` for the root unit.

## 5) Inheritance behavior
- Base/derived DXUnits are linked by ID internally.
- Property lookup can walk up the base chain when the property is not present on the current unit.
- No explicit DXSQL prefix is required for base/derived navigation.

## 6) Join behavior (implementation notes)
- Every traversed relation adds a `LEFT JOIN`.
- Containment joins use `DXUnitID` and unit `ID`.
- Equal relations use DXRelationDefinition rules (including many-to-many relation tables).

## 7) Quick reference
```
U2U(RelName)     Unit -> Unit (equal relation)
U2E(RelName)     Unit -> Element (equal relation)
E2U(RelName)     Element -> Unit (equal relation)
ElementName      Unit -> Element (containment)
E2UIn(UnitName)  Element -> Unit (containment reverse)
```

## 8) Limitations
- No parentheses or precedence rules.
- Requires spaces around operators in filters.
- The DSL is not parameterized; caller must ensure safe values.

### 8.1 “Always true” filters
DXSQL is **not** a raw SQL WHERE clause. The filter must be a DXSQL condition that starts with a valid property/path (e.g., `ID = '...'`, `TUserMainElement.Name = '...'`).

So expressions like `1=1` / `1 = 1` are **not** valid DXSQL.

If you need a filter that matches “everything”, use a valid DXSQL condition such as `ID IS NOT NULL` (and be careful: combined with Sync semantics, that can make a migration operate over the entire table/type).
