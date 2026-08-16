# DXSecurity in IV.DX

## Purpose

This document describes the current DXSecurity design in `IV.DX`, including:

- authentication and session handling;
- role-based access control (RBAC);
- ownership fallback rules;
- initialization modes;
- known limitations and risks.

The goal is to capture how DXSecurity works today in code so it can be safely used and evolved.

## High-level architecture

DXSecurity is split into two layers:

1. Authentication layer
- local registration/login;
- JWT access token issuance;
- refresh token session lifecycle.

2. Authorization layer
- DB-driven RBAC via DX units/elements;
- runtime access decisions per unit type (read/create/update/delete);
- instance-level ownership grants when full type access is not granted.

Important: RBAC is not encoded as role claims in JWT. JWT is used to identify login/session, and RBAC is resolved from persisted data at runtime.

## Core security data model

Security schema is delivered as embedded DX migrations (`Data/DXSecurity.json` and related `.dx` files).

Main units/elements:

- `DXIdentityUnit`: principal identity.
- `DXIdentityLoginUnit`: login credentials/provider for identity.
- `DXAuthSessionUnit`: refresh-session state (session id, token hash, expiry, revoke metadata).
- `DXSecurityMemberUnit`: base type that can hold roles.
- `DXTenantUnit`: tenant-level security member.
- `DXMembershipUnit`: identity membership inside tenant.
- `DXGroupUnit`: group-level security member (inside tenant).
- `DXGroupMembershipUnit`: membership-to-group bridge.
- `DXRoleUnit`: role definition.
- `DXRoleElement`: assigns a role to a security member.
- `DXUnitGrantElement`: grant rule from role to target unit type. Carries one flag per operation (`Read`, `Create`, `Update`, `Delete`) and an effect.
- `DXIdentityOwnershipUnit`: instance-level grant from identity to a concrete record, with `Read`/`Update`/`Delete` flags and an effect.
- `DXGroupOwnershipUnit`: same, from a group to a concrete record.

Ownership rows are grants, not mere mappings. Two identities can own one record with different
rights — an author holding `Update + Delete` and a collaborator holding `Update` only.

Enums:

- `DXIdentityProviderTypeEnum`: `Local`, `Telegram`, `Microsoft`, `Facebook`.
- `DXGrantEffectEnum`: `Allow`, `Deny`.

Unit-definition flags that affect security:

- `SupportsOwnership`: ownership rows are consulted for this type at all.
- `IsPublicRead`: the type is readable without an execution context.
- `AllowAuthenticatedCreate`: any caller with an identity may create instances without a `Create` grant.

## Security enablement model

Security is considered enabled when `DXRoleUnit` exists in the structure cache.

- Before security migration, access checker allows operations (core-only mode).
- After `InitDXSecurityDataAsync`, security is explicitly enabled.

This allows phased startup:

1. `InitDXCoreDataAsync`
2. optional `InitDXQueryDataAsync`
3. optional `InitDXSecurityDataAsync`

## Authentication and session lifecycle

`IDXSecurityService` exposes:

- `RegisterLocalAsync`
- `LoginLocalAsync`
- `RefreshAsync`
- `LogoutAsync`
- `LogoutAllAsync`

### Register/Login

- Register creates `DXIdentityUnit` and `DXIdentityLoginUnit`.
- Login finds local login and verifies password hash.
- Both create a new `DXAuthSessionUnit`.

### Tokens

Access token (JWT) includes:

- `sub` -> identity id
- `dx_login_id` -> identity login id
- `sid` -> session id

Refresh token:

- generated randomly;
- stored as hash in `DXAuthSessionUnit.RefreshTokenHash`;
- rotated on refresh.

### Refresh rotation

`RefreshAsync` enforces:

- session exists;
- session not revoked;
- session not expired;
- refresh token hash matches.

On success:

- creates a new session;
- revokes current session;
- links previous session to replacement (`ReplacedBySession`).

On refresh token mismatch:

- current session is revoked immediately.

## Authorization model

Type-level decisions are made by `IDXUnitTypeAccessChecker` (`DXContextualUnitTypeAccessChecker`).
Data services do not consume it directly: they go through `IDXUnitAccessGate`, which combines it
with the instance-level ownership check and the narrowing a restricted read needs. See
[Element access](#element-access) for why that gate is shared rather than per-service.

### Access decisions

`DXAccessDecision`:

- `Allowed`
- `AllowedOwnedOnly`
- `Denied`

### Operations

`DXUnitTypeAccessOperation` is `Read`, `Create`, `Update`, `Delete`.

`Create` and `Update` are separate on purpose. One "write" permission cannot express an
append-only type (create but never modify), a role that edits records it may not author, or
records provisioned by a system and edited by users.

### Decision rules

Evaluated in order by `DXContextualUnitTypeAccessChecker`:

1. Security disabled -> `Allowed`.
2. Context is system -> `Allowed`.
3. Core unit type -> `Denied` (non-system callers).
4. `Read` on a type with `IsPublicRead` -> `Allowed`, with or without a context.
5. No execution context -> `Denied`.
6. Type explicitly denied for this operation -> `Denied`. An explicit `Deny` outranks every
   other route to access, including ownership.
7. Type granted for this operation -> `Allowed`.
8. `Create` on a type with `AllowAuthenticatedCreate`, and the context has an identity -> `Allowed`.
9. Otherwise -> ownership fallback:
- with identity in context -> `AllowedOwnedOnly`;
- without identity -> `Denied`.

`AllowedOwnedOnly` is not access; it is the signal to consult ownership rows for the concrete
record. The checker works at type level and cannot see instances.

### Type-level access

`DXExecutionContext.Access` is a `DXAccessScope`: for each operation it holds the granted unit
types and the explicitly denied ones. Both are `DXUnitTypeAllowSet`, which distinguishes
`Unrestricted` (no restriction imposed) from `None` (nothing allowed) — an empty restriction
never widens access.

"Denied" and "never granted" are different states. Only an explicit denial overrides access
granted elsewhere.

### Hierarchical RBAC resolution

`DXExecutionContextResolver` resolves context from `identityLoginId + sessionId`:

1. Validate session and login.
2. Load memberships of identity.
3. Collect role ids from membership, tenant and group memberships.
4. Read each role's grants once, and derive every operation from them:
- deny overrides allow per target unit within a level.
5. Build a `DXAccessScope` per level (tenant, membership, group), each carrying granted and
   denied types per operation.
6. Combine the levels into `DXExecutionContext.Access`:
- grants are intersected — a level that imposes no restriction is skipped;
- denials are accumulated — a denial at any level stands.

A level with no roles imposes no restriction, which is not the same as granting nothing. When
no level restricted anything, nothing was granted anywhere and the result allows nothing.

Level composition happens only here. The checker consumes the combined result, so there is no
second, redundant walk over tenant/membership/group at access-check time.

## Public access model

Public access is **read-only** and is intentionally explicit. There is no implicit anonymous access.

### Goals

1. Keep existing RBAC and ownership rules unchanged for authenticated users.
2. Allow selected data to be consumed without session context.
3. Prevent accidental widening from "private by default".

### Level 1: type-level public read

`DXUnitDefinitionUnit.IsPublicRead` marks a unit type as publicly readable.

- When `IsPublicRead = true`, `Read` for that type is `Allowed` even without execution context.
- `Create`, `Update` and `Delete` are not affected.
- Core unit types remain non-public for non-system callers.

Typical use: catalog/reference-like units where all entries are safe to expose.

### Level 2: entry-level public read for private type

For private types (`IsPublicRead = false`), selected entries can still be public via `DXPublicAccessUnit`.

`DXPublicAccessUnit` fields:

- `DXUnitDefinition` -> target unit definition id.
- `PublicDXUnitId` -> concrete public record id for that type.

Runtime behavior for anonymous read:

- `GetItem(type, id)` returns item only if mapping exists for `(type, id)`.
- `GetItems(type)` returns only mapped ids for that type.
- `GetItems(type, ids)` intersects requested ids with mapped ids.
- `GetItems(type, filter)` applies `Id IN (mapped ids)` and original filter together.

Typical use: mostly private dataset with curated public subset.

### Precedence and invariants

1. System context bypass remains strongest rule.
2. Type-level public (`IsPublicRead`) overrides the need for per-entry mapping.
3. Entry-level public does not grant create/update/delete.
4. Authenticated callers keep normal RBAC path; public mapping is additive for read visibility.
5. If no public mappings exist for private type, anonymous reads return empty/null (or denied for non-reader APIs), never full dataset.

### Security boundaries

- Public access is only evaluated in `IDXUnitDataReader` read methods.
- Raw/query APIs that enforce strict type access are not automatically public-aware.
- Public exposure is data-configurable and auditable from `DXPublicAccessUnit`.

### Recommended usage

1. Use `IsPublicRead` only when entire type is safe.
2. For mixed-sensitivity types, keep type private and expose only whitelisted records via `DXPublicAccessUnit`.
3. Keep public exposure lifecycle explicit: add mapping on publish, remove mapping on unpublish/delete.

## Ownership behavior

Ownership applies only for unit definitions with `SupportsOwnership = true`.

An ownership row is an instance-level grant. It states which operations its owner may perform
on one record, and carries an effect:

- a row is consulted only when it covers the requested operation;
- a `Deny` row covering the operation refuses access outright;
- access requires an `Allow` row that covers the operation.

### Read path

When the decision is `AllowedOwnedOnly`:

- single record read: returned only if a row with `Read` grants it to the identity or an active group;
- list/filter reads: the query is restricted to ids granted that way.

A `Deny` row removes its record from results even when `DXPublicAccessUnit` exposes it publicly.

### Create path

Ownership never authorizes `Create`. It is a grant over a record that already exists, so creation
always requires either a `Create` grant or `AllowAuthenticatedCreate` on the type. This is what
keeps `AllowedOwnedOnly` — which every authenticated caller falls into for un-granted types —
from becoming a way to create records.

### Update/Delete path

Full `Allowed` passes. `AllowedOwnedOnly` requires an ownership row granting that specific
operation, so a collaborator can hold `Update` without `Delete`.

### Ownership creation and cleanup

On successful insert (non-system context with identity), the service auto-creates a
`DXIdentityOwnershipUnit` granting the creator `Read`, `Update` and `Delete`. Types that only
need "whoever made it owns it" therefore require no ownership rows to be managed by hand.

On delete, identity and group ownership rows for that record are deleted.

## Element access

A `DXElement` has no grants and no ownership rows of its own. Everything about it is decided
against the **unit type that owns it**, named by `dxUnitTypeName` on the read calls and by
`Meta.DXUnitContext` on a block write.

| Operation on an element | Requires |
|---|---|
| read | `Read` on the owning unit type, narrowed to the units the caller may see |
| create, update, delete | `Update` on the specific unit that owns it, or ownership granting `Update` |

Writes map to `Update` rather than to `Create` and `Delete`. Adding or removing an element does
not bring a unit into being or end it — it changes a unit's contents, which is exactly what a
whole-unit write carrying a modified element container already does. Requiring `Delete` to drop
one element would mean handing out the right to delete the whole unit; requiring `Create` to add
one would let a caller holding a `Create` grant append to units they do not own, while locking out
an owner who holds only `Update`. Either variant would make the element path grant something the
unit path does not; as specified it grants nothing new.

Reads apply the same narrowing as `IDXUnitDataReader`: under `AllowedOwnedOnly`, or for an
anonymous caller falling back to `DXPublicAccessUnit`, the result is restricted to elements whose
owning unit is visible. An element of a unit the caller cannot see reads as absent rather than
denied, so its existence does not leak.

### Resolving the owner

For an element that already exists the owning unit is read from storage, never taken from the
request. A request naming another unit's element alongside a unit of the caller's own would
otherwise pass the check against their unit and then rewrite the other one — moving it across in
the process.

A declared owner that disagrees with the stored one is always refused; how depends on where it
came from:

- from a request body (`IDXElementDataService.UpdateAsync(unitType, element)`) it is a caller
  error and throws;
- from an address (`UpdateAsync(unitType, dxUnitId, element)`, and the DTO services'
  `GetAsync(dxUnitId, id)` / `UpdateAsync(dxUnitId, id, dto)` / `DeleteAsync(dxUnitId, id)`) it is
  reported as absent, so a nested route answers `404` rather than serving or overwriting an
  element that lives under a different unit.

Nothing on the element path reparents an element.

### Where the rules live

`IDXUnitAccessGate` holds the decisions — the type-level check, the owned-only and public-record
narrowing, and the instance-level ownership check — and both the unit services and the element
service go through it. One component on purpose: two copies of these rules would drift, and the
newer path would quietly become a way around the older one.

### Not covered

The element path runs no handler pipeline. Before/after handlers registered for a unit do not see
an element written through `IDXElementDataService`.

## Sensitive data handling

DX persistence layer enforces sensitive handling by column type:

- `HashedString`: values are hashed (used for password/refresh token hashes).
- `EncryptedString`: values are encrypted/decrypted via configured protector.

Reader/query services mask sensitive values in output for hashed/encrypted columns.

## System bypass

Migration flow executes under system execution context:

- `SubjectId = "system:migration"`
- `IsSystem = true`

This is required so schema/data migrations can run even when security is enabled and no user context exists.

## End-to-end runtime expectations

For secure application behavior, host application should:

1. validate JWT signature/lifetime;
2. extract `dx_login_id` and `sid` claims;
3. call `IDXExecutionContextResolver.ResolveAsync(...)`;
4. open execution context scope via `IDXExecutionContextAccessor.BeginScope(...)`;
5. execute application operations inside that scope.

Without this integration, RBAC cannot be evaluated against current principal/session.

## Current limitations and risks

1. `DXExecutionContext` is resolved from the database on every request — session, memberships,
   group memberships and role grants. There is no caching or invalidation.
2. The system execution context created for service tokens carries no `SubjectId`, so writes
   made through it are not attributable to an operator.
3. A hand-built `DXExecutionContext` with no `Access` set is `Unrestricted` and allows
   everything. Contexts produced by `IDXExecutionContextResolver` are unaffected.
4. `AllowAuthenticatedCreate` has no quota or rate limit — throttling is the host application's
   responsibility.

## Testing status summary

Covered:

- security on/off initialization behavior;
- migration system bypass mode;
- auth/session flows (register/login/refresh/logout/logout-all);
- access checker scenarios over the combined access scope, including deny precedence;
- end-to-end resolver tests from real role/grant data to the resolved execution context;
- ownership: insert/update/delete with and without ownership, `AllowedOwnedOnly` returning empty on reads;
- delete-authorization behavior;
- query provider ownership scoping;
- `Create` and `Update` as separate grants: create-without-update, update-without-create, and an
  append-only type where the author cannot revise what they added;
- `AllowAuthenticatedCreate`: granted with no roles, refused without an identity, and overridden
  by an explicit `Deny` grant;
- ownership rows granting a subset of operations — a co-owner holding `Update` but not `Delete`;
- ownership `Effect = Deny`, including a denied record staying hidden despite `DXPublicAccessUnit`.

Not yet covered:

- consumer-facing integration sample for JWT → execution context binding.

## Suggested evolution path

1. Cache the resolved execution context per session, invalidated on role, grant or membership change.
2. Give service tokens a real subject so system writes are attributable.
3. Add a consumer-facing integration sample for JWT -> execution context binding.
