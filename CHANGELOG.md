# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [0.101.0] - 2025-10-17

### Changed

- IDXInitializer to init cache;

## [0.100.0] - 2025-10-07

### Chaned

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
