# Expanded Storage Change Log

## 3.1.0 (November 5, 2024)

### Added

* Added support for better chest config options.
* Added support for global inventory id.
* Added texture and tint override to data model.

### Changed

* Updated for FauxCore 1.2.0.
* If config file is missing, it will attempt to restore from global data.

### Fixed

* Updated for SDV 1.6.10 and SMAPI 4.1.3.

## 3.0.5 (April 12, 2024)

### Changed

* Draw based on current lid frame in local.

## 3.0.4 (April 12, 2024)

### Changed

* Initialize ExpandedStorage DI container on Entry.
* Update transpilers to use CodeMatchers.

## 3.0.3 (April 9, 2024)

### Changed

* Updated for FauxCore api changes.

## 3.0.2 (April 2, 2024)

### Changed

* Added logging for debugging purposes.

## 3.0.1 (March 19, 2024)

### Changed

* Rebuild against final SDV 1.6 and SMAPI 4.0.0.

## 3.0.0 (March 19, 2024)

### Changed

* Updated for SDV 1.6 and .NET 6

# 2.0.3 (September 21, 2022)

### Fixed

* Fixed an issue where storage were unplaceable by default.

# 2.0.2 (September 21, 2022)

### Fixed

* Fixed shop items having double the intended price.

# 2.0.1 (September 21, 2022)

### Added

* Added config options for each Expanded Storage chest type.
* Added support for loading Expanded Storage v1 content via Api.

### Fixed

* Fixed recipes not being added to crafting tab after being purchased.
* Fixed recipes not appearing in the Qi Gems shop.
* Fixed compatibility with Better Crafting.

## 2.0.0 (September 19, 2022)

* Initial version (of the rewrite)
