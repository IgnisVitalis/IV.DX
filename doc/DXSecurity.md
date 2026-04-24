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
- runtime access decisions per unit type (read/write);
- ownership fallback when full type access is not granted.

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
- `DXUnitGrantElement`: grant rule from role to target unit type, with effect.
- `DXIdentityOwnershipUnit`: ownership mapping from identity to concrete record.
- `DXGroupOwnershipUnit`: ownership mapping from group to concrete record.

Enums:

- `DXIdentityProviderTypeEnum`: `Local`, `Telegram`, `Microsoft`, `Facebook`.
- `DXGrantEffectEnum`: `Allow`, `Deny`.

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

Authorization is enforced by `IDXUnitTypeAccessChecker` (`DXContextualUnitTypeAccessChecker`) and consumed by data services/readers.

### Access decisions

`DXAccessDecision`:

- `Allowed`
- `AllowedOwnedOnly`
- `Denied`

### Decision rules

1. If security disabled -> `Allowed`.
2. If no execution context -> `Denied`.
3. If context is system -> `Allowed`.
4. Core unit types are denied for non-system context.
5. Evaluate hierarchical allow-lists:
- tenant restrictions;
- membership restrictions;
- group restrictions (only when `ApplyGroupRestrictions = true`).
6. If global allowed list contains type -> `Allowed`.
7. Otherwise -> ownership fallback:
- with identity in context -> `AllowedOwnedOnly`;
- without identity -> `Denied`.

## Public access model

Public access is **read-only** and is intentionally explicit. There is no implicit anonymous access.

### Goals

1. Keep existing RBAC and ownership rules unchanged for authenticated users.
2. Allow selected data to be consumed without session context.
3. Prevent accidental widening from "private by default".

### Level 1: type-level public read

`DXUnitDefinitionUnit.IsPublicRead` marks a unit type as publicly readable.

- When `IsPublicRead = true`, `Read` for that type is `Allowed` even without execution context.
- `Write` is not affected.
- Core unit types remain non-public for non-system callers.

Typical use: catalog/reference-like units where all entries are safe to expose.

### Level 2: entry-level public read for private type

For private types (`IsPublicRead = false`), selected entries can still be public via `DXPublicAccessUnit`.

`DXPublicAccessUnit` fields:

- `DXUnitDefinition` -> target unit definition id.
- `PublicDXUnitID` -> concrete public record id for that type.

Runtime behavior for anonymous read:

- `GetItem(type, id)` returns item only if mapping exists for `(type, id)`.
- `GetItems(type)` returns only mapped ids for that type.
- `GetItems(type, ids)` intersects requested ids with mapped ids.
- `GetItems(type, filter)` applies `ID IN (mapped ids)` and original filter together.

Typical use: mostly private dataset with curated public subset.

### Precedence and invariants

1. System context bypass remains strongest rule.
2. Type-level public (`IsPublicRead`) overrides the need for per-entry mapping.
3. Entry-level public does not grant write/update/delete.
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

### Hierarchical RBAC resolution

`DXExecutionContextResolver` resolves context from `identityLoginId + sessionId`:

1. Validate session and login.
2. Load memberships of identity.
3. Collect role ids from:
- membership;
- tenant;
- group memberships.
4. Resolve grants per role and operation (`Read`/`Write`/`Delete`):
- deny overrides allow per target unit.
5. Build per-level allow sets:
- tenant read/write/delete;
- membership read/write/delete;
- group read/write/delete.
6. Compute final sets by intersection:
- `Tenant ∩ Membership ∩ Group` (group only if enabled for context).

If a provided set resolves empty, internal deny marker is used to prevent accidental allow.

## Ownership fallback behavior

Ownership applies only for unit definitions with `SupportsOwnership = true`.

### Read path

When decision is `AllowedOwnedOnly`:

- single record read: returned only if owned by identity or active group;
- list/filter reads: query is restricted to owned ids.

### Write/Delete path

- insert requires full `Allowed` (owned-only is not enough to create).
- update/delete with `AllowedOwnedOnly` require ownership of target record.

### Ownership creation and cleanup

On successful insert (non-system context with identity), service auto-creates `DXIdentityOwnershipUnit`.

On delete, identity/group ownership rows for that record are deleted.

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

1. Query/raw-reader APIs enforce full type access only.
- They use `EnsureAccess` and do not apply owned-only filtering semantics like `IDXUnitDataReader` does.

## Testing status summary

Covered:

- security on/off initialization behavior;
- migration system bypass mode;
- auth/session flows (register/login/refresh/logout/logout-all);
- hierarchical access checker scenarios (tenant/membership/group restrictions, deny precedence);
- end-to-end resolver tests from real role/grant data to final execution context sets;
- ownership fallback: insert/update/delete with and without ownership, AllowedOwnedOnly returning empty on reads;
- delete-authorization behavior (full allow required; AllowedOwnedOnly with ownership also checked);
- query provider ownership scoping (AllowedOwnedOnly returns empty when no owned records).

Recommended additional tests:

- consumer-facing integration sample for JWT → execution context binding.

## Suggested evolution path

1. Add consumer-facing integration sample for JWT -> execution context binding.
