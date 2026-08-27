# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

## [0.113.0] - 2026-08-27

### Added

- `IDXOwnershipReader` — which records of a type the current principal owns, as identifiers. Ownership was reachable only from inside the access gate, so an application could not ask "which of these are mine" at all: the ownership units are `Core`, which the type-level checker denies to every non-system caller, and `IDXUnitGenericRepository` is internal. Needed because a type declared `IsPublicRead`, or covered by a type-level `Read` grant, never reaches the gate's ownership narrowing — the decision comes back `Allowed` and every record is returned, so ownership has to be asked for explicitly;
- `IDXUnitQueryService<TResponse>.GetOwnedAsync`, inherited by `IDXUnitDtoService<TRequest, TResponse>` — the owned records mapped through the same read mapper as every other method on the service. This is what an "edit what I authored" screen reads;
- Both take a `DXUnitTypeAccessOperation`, defaulting to `Read`. Ownership rows are per-operation grants, so "records I see as their owner" and "records I may edit" are different answers once a record has co-owners holding narrower rows. No row ever covers `Create`;

### Changed

- `DXUnitTypeAccessOperation` moves from `IV.DX.Persistence.Contracts.Abstractions` to `IV.DX.Kernel.Enums`. A public application contract has to be able to name the operation, and `IV.DX.Application.Contracts` deliberately references `IV.DX.Kernel` only. The enum carries no persistence semantics, and `DXGrantEffectEnum` — its sibling in every ownership row — already lived in the kernel. Callers naming the type need their `using` updated;

## [0.112.0] - 2026-08-16

> 0.109.0 through 0.111.0 shipped without changelog entries. What they carried is folded in below,
> so a few items here reached a package before this release.

### Fixed

- A write that changes nothing no longer touches the row. `TimeStamp` was stamped unconditionally while a row was being filled, which marked every row dirty by construction, so a no-op update still issued an UPDATE and moved the timestamp. It now moves only when a column value actually differs from what was read. This also means a unit's `TimeStamp` no longer moves when only one of its elements changed - the unit's own row is not rewritten;
- `IDXElementDataService.InsertOrUpdateAsync` assigned a generated id to the first record of a block only, so every further record without an id was written with `Guid.Empty`;
- `DXSecurityService` clamps `UserAgent` and `DeviceId` to the widths `DXAuthSessionUnit` accepts before inserting a session. An oversized value failed the insert instead of being trimmed, so register, login and refresh all threw for callers that passed real client data through - a browser `User-Agent` runs 110-130 characters against a 100 character column, which made every browser login fail;

### Security

- Element writes are authorized. `IDXElementDataService` performed no access check at all, so anything reachable through it bypassed RBAC entirely. Access is now decided against the owning unit type: reading an element needs `Read` on the unit, and creating, changing or removing one needs `Update` on the specific unit that owns it. `Update` rather than `Delete` for removal, because removing an element is a change to a unit's contents, not the end of the unit - the same thing a whole-unit write with a modified element container already does, so the element path grants nothing new;
- Element reads narrow to what the caller may see, applying the same owned-only and public-record filtering as `IDXUnitDataReader`. Without it the element path returned rows belonging to units the unit path would have hidden;
- An element write resolves its owning unit from storage instead of from the request. Naming another unit's element alongside a unit of one's own previously passed the check against the caller's unit and then rewrote the other one, reparenting it in the process; a request whose declared owner disagrees with the stored one is now rejected;

### Added

- `IDXElementDataService` covers the element lifecycle: `GetItemAsync`, `GetItemsByUnitAsync`, `GetItemsByUnitFilterAsync`, `InsertAsync<T>`, `UpdateAsync<T>` and `InsertOrUpdateAsync<T>` for a typed element, `InsertOrUpdateAsync` for a block, and `DeleteAsync`. Editing one element of a unit no longer means reading the whole unit, changing a part of it and writing it all back;
- Owner-scoped overloads throughout the element path, for addresses that name the unit as well as the element: `IDXElementDataService.GetItemAsync<T>(unitType, dxUnitId, id)`, `UpdateAsync<T>(unitType, dxUnitId, element)` and `DeleteAsync<T>(unitType, dxUnitId, id)`, surfaced on the DTO services as `GetAsync(dxUnitId, id)`, `UpdateAsync(dxUnitId, id, dto)` and `DeleteAsync(dxUnitId, id)`. An element belonging to a different unit is reported as absent rather than served or written, so a nested route cannot be made to read or overwrite another unit's element by swapping the owner in the path;
- A DTO mapper layer for elements, mirroring the unit one: `DXElementMapper<TRequest, TResponse, TElement, TUnit>`, `DXElementReadMapper<TResponse, TElement, TUnit>`, `DXElementWriteMapper<TRequest, TElement, TUnit>`, the services they register (`IDXElementDtoService<TRequest, TResponse>`, `IDXElementQueryService<TResponse>`, `IDXElementCommandService<TRequest>`), and `AddDXElementMapper` / `AddDXElementReadMapper` / `AddDXElementWriteMapper`. The owning unit is a type argument because `IsCommon` lets one element type belong to several units, and `IDXElementCommandService.CreateAsync` takes the owner id as an argument so a nested route can supply it without every request DTO carrying it. Documented in `doc/DXUnitDtoMapper.md`;
- `DXObjectHelper.GetDeclaredDXUnitId` - the owning unit declared by an element record, read the same way by the access check and by the write that follows it;
- `DXAuthSessionUnit.UserAgentMaxLength` and `DXAuthSessionUnit.DeviceIdMaxLength` - the session column widths, declared on the unit that owns them so callers no longer have to discover them by trial;

### Changed

- A block is written in one transaction. Both `IDXUnitDataService` and `IDXElementDataService` opened a transaction per record, so a failure partway through a block left the earlier records committed;
- `IDXElementDataService.GetItemsAsync(dxUnitTypeName, dxFilter)` renamed to `GetItemsByUnitFilterAsync`. The filter is evaluated against the unit table, not the element table, and the old name said otherwise. The overloads taking ids take no filter string at all, which keeps them clear of the unparameterised filter path;
- `IDXElementDataService.InsertOrUpdateAsync(block)` returns every id written rather than the last one;
- `IDXElementCommandService.UpdateAsync` takes the element id as an argument (`UpdateAsync(id, dto)`) instead of reading it off the DTO. An element is addressed by two coordinates and a nested route supplies both, so request DTOs stay pure payload: no marker interface, and a mapper's `ToElementAsync` no longer has to remember to set `Id`. Whatever it writes into `Id` or `DXUnitId` is overwritten from the arguments;

## [0.108.0] - 2026-08-12

### Changed

- `IDXUnitCommandService<TRequest>` and `IDXUnitDtoService<TRequest, TResponse>` gain `CreateAsync` and `UpdateAsync` alongside `SaveAsync`, so a write call maps to exactly one access operation instead of resolving to `Create` or `Update` depending on whether the record happens to exist;
- `CreateAsync` always inserts and returns the server-assigned id; `UpdateAsync` returns `false` when no record with that id exists, rather than failing;
- `SaveAsync` keeps its upsert behaviour and is retained for import and synchronisation flows;

- `IDXUnitDataService.UpdateAsync<T>` returns `Guid.Empty` when no record with that id exists, instead of updating zero rows and reporting success; access is still checked before existence;

### Added

- `IDXUnitRequest` — contract for request DTOs exposing `Guid Id`, so REST controllers can bind the id from the route; the DTO services themselves place no constraint on the request type;

## [0.107.0] - 2026-08-11

### Changed

- `DXUnitTypeAccessOperation` splits `Write` into `Create` and `Update` — the enum is now `Read`, `Create`, `Update`, `Delete`;
- `DXUnitGrantElement.Write` replaced by `Create` and `Update`, so authoring a record and editing every record of a type are separate grants;
- `DXExecutionContext` — the twelve per-level allow-list properties and `ApplyGroupRestrictions` replaced by a single `Access` property of type `DXAccessScope`; level intersection now happens only in `IDXExecutionContextResolver`;
- `IDXUnitDataService.InsertOrUpdateAsync` (all overloads) resolves record existence before authorizing, so updating an owned record is no longer blocked by the create gate;
- `DXIdentityOwnershipUnit` and `DXGroupOwnershipUnit` are instance-level grants carrying `Read`, `Update`, `Delete` and `Effect`; co-owners of one record can hold different rights;
- Ownership never authorizes `Create` — it is a grant over a record that already exists;
- Ownership rows created automatically on insert grant `Read`, `Update` and `Delete` to the creator;
- An explicit `Deny` grant is a hard denial and no longer falls through to the ownership fallback;
- A `Deny` ownership row hides its record from reads even when `DXPublicAccessUnit` exposes it;

### Added

- `DXAccessScope` — operation-keyed type access carrying both granted and explicitly denied unit types; adding an operation to `DXUnitTypeAccessOperation` no longer changes its shape;
- `DXUnitTypeAllowSet` — allow set with explicit `Unrestricted` and `None` states, replacing the `__dx_deny__` marker convention;
- `DXUnitDefinitionUnit.AllowAuthenticatedCreate` — any caller with an identity may create instances of the type without holding a `Create` grant, for types whose records belong to whoever made them; an explicit `Deny` grant still overrides it;

### Fixed

- PostgreSQL schema migration emitted `RENAME COLUMN "X" TO "X"` for every unchanged column, which PostgreSQL rejects — every `ALTER TABLE` touching a table with an unchanged column failed;
- Type access checks no longer treat an empty allow list as "allow everything";
- `DXExecutionContextResolver` read role grants once per role per operation; grants are now read once per role and every operation derived from them;

## [0.106.0] - 2026-06-09

### Changed

- `DXUnitMapper<TDto, TUnit>` replaced by `DXUnitMapper<TRequest, TResponse, TUnit>` — request and response types are now distinct type parameters;
- `IDXUnitDtoService<TDto>` replaced by `IDXUnitDtoService<TRequest, TResponse>`, which extends `IDXUnitQueryService<TResponse>` and `IDXUnitCommandService<TRequest>`;
- `AddDXUnitMapper<TDto, TUnit>()` (convention mapper) now registers `IDXUnitDtoService<TDto, TDto>`;
- `DXConventionMapper<TDto, TUnit>` now inherits `DXUnitMapper<TDto, TDto, TUnit>`;

### Added

- `DXUnitReadMapper<TResponse, TUnit>` — abstract base for read-only mappers (`ToDtoAsync` only);
- `DXUnitWriteMapper<TRequest, TUnit>` — abstract base for write-only mappers (`ToUnitAsync` only);
- `IDXUnitQueryService<TResponse>` — read-only service interface (`GetAsync`, `GetAllAsync`);
- `IDXUnitCommandService<TRequest>` — write-only service interface (`SaveAsync`, `DeleteAsync`);
- `DXUnitQueryService<TResponse, TUnit, TReadMapper>` — internal implementation of `IDXUnitQueryService<TResponse>`;
- `DXUnitCommandService<TRequest, TUnit, TWriteMapper>` — internal implementation of `IDXUnitCommandService<TRequest>`;
- `AddDXUnitReadMapper<TMapper>()` — registers a read-only mapper and `IDXUnitQueryService<TResponse>`;
- `AddDXUnitWriteMapper<TMapper>()` — registers a write-only mapper and `IDXUnitCommandService<TRequest>`;

## [0.105.0] - 2026-05-22

### Changed

- `IDXUnitDataService.InsertAsync`, `UpdateAsync`, and `InsertOrUpdateAsync` (all overloads — typed, JObject, DXDataBlock) now return `Guid` (the assigned record Id) instead of the full model or block;
- `IDXElementDataService.InsertOrUpdateAsync` now returns `Task<Guid>` instead of the full block;
- `IDXPipelineExecutor.InsertAsync` and `UpdateAsync` (all overloads) now return `DXResult<Guid>` instead of the full model or block;
- After-handler DB reload in `DXPipelineExecutor` is now conditional — the reload only occurs when after-handlers are actually registered for the type, eliminating the unconditional round-trip for types with no after-handlers;

### Added

- UUID v7 (`Guid.CreateVersion7()`) auto-generation for new DXUnit records (all insert paths) and new DXElement records (`IDXElementDataService.InsertOrUpdateAsync`) when `Id == Guid.Empty`; auto-generation is skipped during migration (`DXMigrationContext.IsMigrating`);

## [0.104.0] - 2026-04-25

### Added

- `IDXUnitDtoService<TDto>` — application service for CRUD operations on a DTO backed by a `DXUnit`;
- `DXUnitMapper<TDto, TUnit>` — abstract base for custom DTO mappers (two methods: `ToDtoAsync`, `ToUnitAsync`);
- `DXConventionMapper<TDto, TUnit>` — internal reflection-based convention mapper; validates property compatibility at startup;
- `AddDXUnitMapper<TMapper>()` and `AddDXUnitMapper<TDto, TUnit>()` extension methods on `IServiceCollection`;

## [0.103.0] - 2026-04-24

### Changed

- Huge rework of IV.DX core project;

## [0.102.0] - 2025-10-18

### Added

- Method for DXUnit to convert it to JObject;

## [0.101.0] - 2025-10-17

### Changed

- IDXInitializer to init cache;

## [0.100.0] - 2025-10-07

### Changed

- Adapted solution to clean architecture;

## [0.36.0] - 2025-09-21

### Fixed

- CoreRepo to remove several rows for target mode;

## [0.35.0] - 2025-09-21

### Fixed

- Deleting DPEntityDescObject item and structure if the entity contains blocks;
- Deleting blocks from entity in target mode;

## [0.34.0] - 2025-09-18

### Changed

- Extended ESQLObjectHelper to create instances from JArray/json str;

## [0.33.0] - 2025-09-17

### Changed

- Extended IDataService to process JObject as income argument;

## [0.32.0] - 2025-09-17

### Changed

- IDataService, ICoreRepository, IGenericRepository, DataHandlers to return object Guid for Insert/Update method, bool for Delete method, etc;

## [0.31.0] - 2025-09-15

### Changed

- ESQLObjectHelper to public;

## [0.30.0] - 2025-09-15

### Fixed 

- Issue to correctly process Announced/Deleted columns for Target mode;

## [0.29.0] - 2025-09-15

### Changed

- Extended Core repo with new methods;

## [0.28.0] - 2025-03-16

### Fixed 

- Target mode logic to process DPColumnDescBlock items;
- Target mode logic to process DPBlockInEntityDescGenBlock items;
- Delete action for DPEntityDescObject;

## [0.27.0] - 2025-03-15

### Fixed

- DPEntityInheritanceBlock relation for DPEntityDescObject to single optional;

## [0.26.0] - 2025-01-12

### Changed

- ESQL where expression to support <> and != operators;

## [0.25.0] - 2025-01-12

### Changed

- Core repo to fullfill ESQLModel with null values;

## [0.24.0] - 2024-12-22

### Changed

-  Upgraded .NET Framework to 9;

## [0.23.0] - 2024-05-07

### Changed

- Unity DI to Microsoft DI;

## [0.22.0] - 2024-05-02

### Changed

- Upgraded .NET Framework to 8;
- Upgraded Newtonsoft.JSON to 13.0.3;

## [0.21.1] - 2023-09-17

### Changed

- Upgraded .NET Framework to 7;

## [0.21.0] - 2022-12-10

### Changed

- Names of properties for relation info object;

## [0.20.0] - 2022-12-06

### Changed

- Core repository contract;

### Fixed

- Data service to use correct data definition during loading data;

## [0.19.0] - 2022-09-25

### Fixed

- Issue in GetItem method for Core types;

## [0.18.0] - 2022-09-23

### Fixed

- Issue to get access to custom migration scripts;

## [0.17.0] - 2022-09-23

### Fixed

- Issue to get access to core migration scripts;

## [0.16.0] - 2022-09-23

### Added

- Pack migration scripts to nuget;

### Changed

- Enabled setting path to config file for Dependency registrator;

## [0.15.0] - 2022-04-25

### Fixed

- Bug in Get method for core repo when entity has hierarchy;

## [0.14.0] - 2022-04-24

### Fixed

- Bug in Get method for core repo when entity doesn't contain any blocks;

## [0.13.0] - 2022-04-17

### Changed

- Rework DataService and CoreRepo for Get methods;

## [0.12.0] - 2022-04-16

### Changed

- BuildModelDefinition method for ModelDefinition;

## [0.11.0] - 2022-04-16

### Added

- Implementation for GetItems for DataService;

## [0.0.1] - 2021-10-02

### Changed

- Init;
