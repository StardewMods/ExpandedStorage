# FauxCore Change Log

## 1.2.1 (Unreleased)

### Changed

* Added French translation.

## 1.2.0 (November 5, 2024)

### Added

* Added ExpressionHandler service for parsing expressions.
* Added IconRegistry service for managing icons.

### Changed

* Redesigned FauxCoreApi services into property getters.
* Updated FauxCoreIntegration as a proxy to FauxCoreApi services.
* If config file is missing, it will attempt to restore from global data.

### Fixed

* Updated for SDV 1.6.10 and SMAPI 4.1.3.

### 1.1.1 (April 12, 2024)

### Changed

* Initialize FauxCore DI container on Entry.

## 1.1.0 (April 9, 2024)

### Changed

* Updated Api for Theme Helper.
* Collect multiple samples for palette swaps.

## 1.0.2 (April 2, 2024)

### Changed

* Only enable in-game alerts if log level is More.

### Fixed

* Fixed all logs being suppressed.

## 1.0.1 (March 19, 2024)

### Changed

* Rebuild against final SDV 1.6 and SMAPI 4.0.0.

## 1.0.0 (March 19, 2024)

* Initial Version
