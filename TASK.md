# TASK

## No optimistic concurrency on unit writes

`DXUnit` and `DXElement` both carry a `TimeStamp`, but nothing treats it as a
concurrency token - no `RowVersion`, no `ETag` / `If-Match` handling anywhere in
Kernel, WebApi or Application, and DTOs do not surface it. So a full-unit `PUT`
is last-writer-wins with no detection.

It bites hardest where a unit owns a collection of elements: two callers editing
different elements of the same unit each send the whole unit, and the second
write silently drops the first one's changes. Seen on books/chapters, but the
cause is in the kernel, not in that app.

Fix: add a concurrency token to `DXUnit` / `DXElement`, expose it through the DTO
layer, and reject a stale write (409) instead of applying it.

`TimeStamp` is at least a usable version marker now: it moves only when a row's
values actually change, so it no longer reports an edit that did not happen. The
detection half is still missing.

## No convention mapper for elements

Units get `DXConventionMapper<TDto, TUnit>` and `AddDXUnitMapper<TDto, TUnit>` -
name-matched property copying, no mapper class, validated at startup. Elements
have no equivalent, so even a DTO that matches an element property for property
needs a hand-written mapper.

Elements are the easier case: no containers, no nested elements, so only the
scalar half of `DXConventionMapper` is needed. Worth factoring that half out of
the existing mapper rather than writing a second copy of it.

## Handlers do not run on the element-scoped path

`IDXElementDataService` now covers the full element lifecycle, but it does not
execute the handler pipeline - unlike the unit services, which run before and
after handlers around every operation. A handler registered for a unit does not
see an element written through the element service.

Two questions to settle before adding them:

- What a handler for an element write receives. The unit contexts carry the whole
  unit; an element write has no unit loaded, and loading one to satisfy a handler
  would give back the read-modify-write cost the element path exists to avoid.
- How unit handlers and element handlers stay consistent. A unit write today
  writes its elements too, so both sets would fire on that path and only one on
  the element path.

## Table aliases in generated SQL follow physical row order

`SQLQueryBuilder` numbers table aliases (`T_<n>_0`) by the position of each
definition in the structure snapshot, and the snapshot is read with no
`ORDER BY` - so the numbering follows Postgres heap order of
`DXUnitDefinitionUnit`. Anything that changes how those rows are written
reshuffles it: removing the redundant no-op UPDATE from unit writes was enough to
swap two units and move every alias after them.

Nothing is functionally wrong - aliases are internal - but `SQLQueryHelperTests`
pins generated SQL to those numbers and has to be edited whenever they move.

Fix: order the structure load deterministically (by `Id` is enough, and Guid v7
makes that creation order). Note that renumbering will move every alias constant
in `SQLQueryHelperTests` once, on purpose.
