# Garbage Day Change Log

## 3.1.9 (November 5, 2024)

### Changed

* Updated for FauxCore 1.2.0.
* Load Garbage Can texture from internal asset.
* If config file is missing, it will attempt to restore from global data.

### Fixed

* Updated for SDV 1.6.10 and SMAPI 4.1.3.

## 3.1.8 (April 15, 2024)

### Changed

* Toolbar icon forces the trash can lid event on the nearby garbage can.
* Provide default options for Better Chests.

### Fixed

* Fixed garbage cans dropping infinite special items.

## 3.1.7 (April 12, 2024)

### Changed

* Initialize GarbageDay DI container on Entry.

## 3.1.6 (April 9, 2024)

### Changed

* Updated for FauxCore api changes.

## 3.1.5 (April 6, 2024)

### Fixed

* Add toolbar icon integration on game launched.

## 3.1.4 (April 4, 2024)

### Fixed

* Garbage cans can now be loaded by default when the config option is set.

## 3.1.3 (April 2, 2024)

### Changed

* Added logging for debugging purposes.

## 3.1.2 (March 25, 2024)

### Fixed

* Fixed error that occurs when custom maps are installed.
* Fixed api integration with Toolbar Icons.

## 3.1.1 (March 19, 2024)

### Changed

* Rebuild against final SDV 1.6 and SMAPI 4.0.0.

## 3.1.0 (March 19, 2024)

### Changed

* Updated for SDV 1.6 and .NET 6

## 3.0.1 (August 21, 2022)

### Changed

* Open the Garbage Can lid when a farmer is nearby.

## 3.0.0 ( August 20, 2022)

### Added

* Added console command:
    * `garbage_hat` - Next Garbage Can will drop a hat.

### Changed

* Updated to SMAPI 3.15.
* No longer depends on any other mods.

## 2.1.0-beta.5

### Changed

* Added support for more types of custom loot
* Fixed RNG being too low

## 2.1.0-beta.1

### Changed

* Updated from Expanded Storage (Legacy) to XSLite+XSPlus

## 2.0.1

### Changed

* Allow adding Garbage Cans using "Garbage": "ID" tile property
* Update to SMAPI 3.12

## 2.0.0

### Changed

* Preliminary 64-bit support
* Simplified map integrations

## 1.0.3

### Added

* Added config option to hide Garbage Cans from Chests Anywhere

## 1.0.2

### Added

* Added console commands:
    * `garbage_fill` - Adds loot to all garbage cans.
    * `garbage_kill` - Removes all garbage cans.
* Added Open when Farmer Nearby
* Added color based on items

### Changed

* Fixed MacOS support

## 1.0.1

### Added

* Added Content Pack Format
* Added API

## 1.0.0

* Initial Release