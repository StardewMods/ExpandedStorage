# Toolbar Icons Change Log

## 2.8.1 (Unreleased)

### Fixed

* Fixed issues with UI scaling.

## 2.8.0 (November 5, 2024)

### Added

* Added icon for showing the calendar.
* Added hotkey for toggling all icons on/off.
* Added config option to enable/disable icon tooltips.
* Added config option to enable/disable icon sounds.

### Changed

* Updated for FauxCore 1.2.0.
* If config file is missing, it will attempt to restore from global data.
* Removed support for Dynamic Game Assets.
* Content pack format now requires icons to be registered with FauxCore.

### Fixed

* Updated for SDV 1.6.10 and SMAPI 4.1.3.
* Fixed issue with not being able to configure toolbar icons.

## 2.7.2 (April 12, 2024)

### Changed

* Initialize ToolbarIcons DI container on Entry.

## 2.7.1 (April 9, 2024)

### Changed

* Updated for FauxCore api changes.

## 2.7.0 (April 6, 2024)

### Added

* New default icon to toggle player collision.
* Improved GMCM integration.

### Fixed

* Toolbar icons should work correctly for all zoom levels.

## 2.6.2 (March 25, 2024)

### Fixed

* Fixed api integration with other mods.

## 2.6.1 (March 19, 2024)

### Changed

* Rebuild against final SDV 1.6 and SMAPI 4.0.0.

## 2.6.0 (March 19, 2024)

### Changed

* Updated for SDV 1.6 and .NET 6

## 2.5.0 (September 17, 2022)

### Added

* Added support for To-Dew.
* Added button for Daily Quests.
* Added button for Special Orders.

### Changed

* New icons created
  by [Tai](https://www.nexusmods.com/stardewvalley/users/92060238):
    * Always Scroll Map
    * To-Dew
    * Daily Quests
    * Special Orders

## 2.4.0 (September 6, 2022)

### Added

* Added support for opening custom menus.

## 2.3.3 (August 21, 2022)

### Fixed

* Fixed icons being drawn during cutscenes.

## 2.3.2 (August 19, 2022)

### Fixed

* Fixed icons displaying before all were loaded.

## 2.3.1 (July 15, 2022)

### Added

* Added additional logging.

### Fixed

* Fixed component misalignment.

### Changed

* Register config after all icons are loaded.

## 2.3.0 (July 14, 2022)

## Added

* Added support for method integration in content packs.
* Added support for Generic Mod Config Menu.

## Changed

* Moved some direct integration back into the content pack.
    * Chests Anywhere
    * Data Layers
    * Debug Mode
    * Horse Flute Anywhere
    * Instant Buildings
    * Lookup Anything
* Hide Toolbar Icons until they're all loaded.

## 2.2.2 (July 13, 2022)

### Added

* Added support for matching modded interface colors.

## 2.2.1 (July 12, 2022)

### Fixed

* Fixed disabled toolbar icons not taking effect.
* Fixed toolbar icons not moving with the toolbar.

## 2.2.0 (July 11, 2022)

### Added

* Added config menu for toolbar icons.
* Added support for Custom Toolbar vertical toolbar.

### Fixed

* Fixed errors from some mod integrations.
    * Data Layers
    * Debug Mode
    * Horse Flute Anywhere

## 2.1.0 (July 9, 2022)

### Added

* Added a default background to all icons.
* Added alert if FuryCore is installed.
* Added direct integration with some mods.
    * Always Scroll Map
    * Chests Anywhere
    * CJB Cheats Menu
    * CJB Item Spawner
    * Data Layers
    * Debug Mode
    * Dynamic Game Assets
    * Horse Flute Anywhere
    * Instant Buildings
    * Lookup Anything
    * Stardew Aquarium

## 2.0.0 (July 2, 2022)

* Bumped version number to avoid conflict on Toolbar Icons mod page.

## 1.0.0 (July 1, 2022)

* Initial Version