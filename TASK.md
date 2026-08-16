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

## Investigate: editing part of a unit

There is no first-class way to change one element of a unit. Everything writes
whole units, so editing a single element means read the unit, mutate it, write it
all back. Books/chapters hit this; anything with a `DXMultiElementsContainer`
will hit it too.

What that costs today:

- Read access is required on top of Update, because the caller has to load the
  unit before it can touch a part of it - a plain whole-unit write does not.
- The read-modify-write window has no protection, which is the concurrency gap
  above seen from a second angle. It is worse here: the caller never intended to
  write the untouched fields at all.
- Going through the DTO services makes it worse again - unit to response to
  request to unit only carries what the DTOs model, so unmapped fields are reset
  by an edit that never asked to touch them. `BookController` writes the unit
  directly to dodge this, but every caller has to know to do that.
- Deleting an element is easy to get wrong. `DXMultiElementsContainer` defaults
  to Target mode, so leaving an element out of `Announced` does not remove it -
  it has to go into `Deleted`. Nothing in the type makes that obvious.

`IDXElementDataService.InsertOrUpdateAsync(DXDataBlock<DXElementRecord>)` already
writes a single element block, so the persistence side may already be able to do
this. Worth checking whether an element-scoped write path can be exposed above it
- how it should check access, and what it means for the unit's `TimeStamp`.
