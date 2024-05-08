# Better Chests Change Log

## 2.19.0 (Planned)

### Added

* Added visual editors for configuring storage options.
* Added support for searching by item attributes.

### Changed

* If config file is missing, it will attempt to restore from global data.

### Fixed

* Fixed AutoOrganize not grabbing items from chests after they fail too insert
  into the first chest.

## 2.18.3 (May 7, 2024)

### Added

* Added configuration for Mini-Fridge storage type.
* Added a toggleable feature to aid in debugging.

### Changed

* Removed `and` and `or` from search expressions.
* Added `(` and `)` for grouping search expressions where all conditions must be
  met.
* Added `[` and `]` for grouping search expressions where any conditions must be
  met.
* Merged icons into a single asset.

### Fixed

* Fixed chest menu launching without community center or color picker buttons.

## 2.18.2 (May 5, 2024)

### Changed

* Overhauled the hierarchy system for storage options.

### Fixed

* Fixed cursor jumping to top-left corner when grabbing items.

## 2.18.1 (May 4, 2024)

### Changed

* Added config button for dressers and fish tanks.

### Fixed

* Fixed storages showing the inventory of the chest next to it.

## 2.18.0 (May 3, 2024)

### Added

* Added Access Chest Priority to configure sort/access order.
* Added configurable icons that can be assigned to storages.
* Added Name and Icon as info that can be displayed for storages.
* Added support for storage furniture such as dressers and fish tanks.
* Added flavored and quality items to categorize chest search.

### Changed

* When a storage is given a unique name, the dropdown will no longer show the
  location/tile.
* Group storage type config options into separate pages for each storage type.

### Fixed

* Updated menu relaunch method to avoid breaking Chests Anywhere detection.

## 2.17.1 (May 2, 2024)

### Added

* Initialize storage name from latent mod data.
* Added support for copy+paste into search bar.
* Added support for copy and paste into search bar.

### Changed

* Remove textbox limit for search.

## 2.17.0 (May 1, 2024)

### Added

* Added support for Horse Overhaul Saddle Bags.
* Added console command for resetting all storage options back to default.
* Added hotkey for clearing the search bar.
* Added inventory tabs for saving search texts.
* Added info tooltip when hovering over a storage.
* Added customizable sort options for storages.

### Changed

* Access Chests now includes container names and location when searching.
* Love of Cooking will now use chests in the current location with Cook From
  Chest enabled.
* Multiple search terms separated by space will return items where any of the
  terms match.
* Default options now must be set to an enabled option or disabled.
* If a default option is set to default, it will be overridden.

### Fixed

* Fixed compatibility issue with Love of Cooking.

## 2.16.3 (April 19, 2024)

### Changed

* Backpack Storage options will not inherit from the global storage options.
* Temporarily hiding backpack config behind the console
  command `bc_player_config` until it is fully implemented.

### Fixed

* Fixed backpack config button showing up in other game menus.
* Fixed backpack items not being placeable in any slot.

## 2.16.2 (April 19, 2024)

### Added

* Added lock indicator to backpack inventory menu.
* Added experimental support for resizing the backpack as a storage.

### Changed

* Adjusted the Access Chest overlay so that the dropdown does not go over the
  arrows.
* When doing a reverse transfer, allow any items to go into the backpack.
* Improved the the precision of logging storage activity and their location.

### Fixed

* Search bar is not selected by default when opening chests.
* Prevent suppressing modifier keys while the search bar is selected.

## 2.16.1 (April 18, 2024)

### Fixed

* Hotfix for search bar losing focus.

## 2.16.0 (April 18, 2024)

### Added

* Overhauled searching and categorizing items.
* Added config option to adjust the overburdened speed debuff.
* Added config option to enable or disable the access chest arrows.
* Added controls for transferring items up and down.
* Added icon for saving search as the categorization.
* Added icon for toggling existing stacks.
* Added icon for toggling rejecting uncategorized items.
* When searching, all categorized items will display in the menu.
* Added storage options for allowing existing items to be stashed.
* Added storage options for blocking items not belonging to the category.
* Added cache table for saving search expressions.
* Added events for items being displayed and items being highlighted.
* Farmer and Farmhands can now inherit options from the FarmHouse and Cabin
  respectively.
* Added config option to choose whether the color picker is on the left or right
  side.

### Changed

* Inventory Tabs have been removed.
* Customize icons for the HSL color picker.
* Moved Access Chest arrows and dropdown to the top of the screen.
* Moved Access Chest arrows and index to below the label.
* Moved input events to be handled by the inventory menu manager.
* Allow transfer logic to replicate transfer existing stacks by default.

### Fixed

* Fixed config options for inventory tab method and search items method.
* Fixed grabbing items from chests leaving empty slots until organized.
* Fixed held containers having all of their features disabled.
* Fixed shipping bin leaving empty slots.
* Fixed shipping bin as a chest menu unable to be disabled.

## 2.15.2 (April 16, 2024)

### Fixed

* Prevent trying to stash into the backpack.

## 2.15.1 (April 16, 2024)

### Changed

* Shipping bins accessed through the dropdown will be loaded in the chest menu.
* Improved the method for detecting storages.

### Fixed

* Fixed all Automate items were being rejected.
* Fixed last chest in the Access Menu dropdown was not being shown.
* Prevent crashing when an item does not have a name.
* Prevent loading non playerChests as containers.

## 2.15.0 (April 15, 2024)

### Added

* Adjust the position of Chests Anywhere UI to not overlap with Better Chests
  UI.
* Added feature to remotely access chests.
* Added feature to include chests when cooking.

### Changed

* Indicate the parent value in config options.
* Craft from Workbench settings is now configurable under Workbench as the
  storage type.
* Relocated the HSL Color Picker to the right side of the menu.
* Improved controller support for the menu components.
* Chest types in the config options menu are sorted by name.
* Simplified stash priority options in the config menu.

### Fixed

* GMCM was not loading if the config.json file was incompatible.
* AutoOrganize skipped any chest that was full even if additional items could be
  stacked.
* Added null-safe access for inventory tabs.

## 2.14.0 (April 12, 2024)

### Added

* Added support for configuring chest types in the config menu.

### Changed

* Ensure the minimum capacity is at least the same as the menu size.
* Display capacity in GMCM as increments of menu size.
* Play lid animation when hovering a chest in the color picker.

### Fixed

* Chests can now inherit capacity from parent options.

## 2.13.1 (April 12, 2024)

### Fixed

* Fixed crash when generating a new tab.json file.

## 2.13.0 (April 12, 2024)

### Added

* Allow chest menu to be resized independently of capacity.
* Added ShopFromChest options to generic mod config menu.
* Added ResizeChestCapacity options to generic mod config menu.

### Changed

* Initialize BetterChests DI container on Entry.
* Update transpilers to use CodeMatchers.

### Fixed

* Fixed carried Junimo Chests losing their inventory.

## 2.12.0 (April 10, 2024)

### Added

* Added controller support for the HSL Color Picker.

### Changed

### Fixed

* Fixed color picker forgetting colors.
* Fixed shipping bin not showing last item.

## 2.11.0 (April 9, 2024)

### Added

* Added support for Shipping Bins.

### Changed

* Added more logging of configuration changes.
* When configuring chests, it will show the chest being configured.
* Updated for FauxCore api changes.

### Fixed

* Config options will now be reflected without restarting the game.
* Disabled config options will show on the main menu.
* Workbenches were not using the Workbench range.
* Prevent swapping chest with carried chests.
* Fixed Satchels not opening after exiting from menu.

## 2.10.1 (April 6, 2024)

### Changed

* If categorize is disabled, stash to existing stacks only.

### Fixed

* Categorize menu should work correctly for all zoom levels.
* Updated to support Better Crafting 2.2.0 Api changes.

## 2.10.0 (April 4, 2024)

### Added

* Added back categorize menu to remove categories.

### Changed

* Crafting from chests now requires Better Crafting.

### Fixed

* Locked items can now be toggled again, and they won't stash into inventories.
* Only categorized items will be stashed.

## 2.9.3 (April 2, 2024)

### Added

* Added ShopFromChest feature.

### Changed

* Apply stashing rules more strictly to prevent unwanted stashing.
* Suppress playing chest sound when returning to the chest from the config menu.

### Fixed

* Full chests can be stashed into for existing items.
* If location does not have CustomFields, use empty dictionary instead.

## 2.9.2 (March 25, 2024)

### Changed

* Updated default stash distance to 10 tiles to prevent unintentional stashing.

### Fixed

* Crafting now works for chests outside of the current location.
* Stashing is now able to find eligible chests.
* Backpack items are no longer double counted towards crafting materials.
* Prevent chest being lost when added to a carried chest.
* Chest menu background is no longer black.
* Cursor should be drawn above the color picker.
* Chest color is no longer lost when clicking in the chest menu.
* Loading incompatible data will now generate new data (tabs and config).
* Toolbar icons can now be used for stashing/crafting.

## 2.9.1 (March 19, 2024)

### Changed

* Rebuild against final SDV 1.6 and SMAPI 4.0.0.

### Fixed

* Fixed items being stashed into a random chest and ignoring categorization.

## 2.9.0 (March 19, 2024)

### Changed

* Updated for SDV 1.6 and .NET 6

## 2.8.0 (September 19, 2022)

### Added

* Added modifier key to scroll inventory one page at a time.
* Added support for the Auto-Grabber.

### Changed

* Updated API for Expanded Storage Integration.

### Fixed

* Fixed locked items getting stashed when using the FillOutStacks button.

## 2.7.2 (September 7, 2022)

### Fixed

* Fixed StashToChest Default was not being overridden by a default setting.

## 2.7.1 (September 6, 2022)

### Fixed

* Fixed CraftFromChest not working on Crafting tab.

## 2.7.0 (September 5, 2022)

### Added

* Added Chest Info feature.

### Changed

* ChestFinder can now open the menu to found chests.
* ChestFinder automatically adds arrows after a short delay.

## 2.6.3 (September 4, 2022)

### Changed

* Added support for Better Crafting from the inventory menu.

## 2.6.1 (September 2, 2022)

### Added

* Added Craft From Workbench feature.
* Added extra logging for Storages types and individual storages.

### Fixed

* Fixed ingredients getting multiplied every time Crafting Page is reopened.
* Fixed SearchBar not spanning full width of ItemSelectionMenu.

### Changed

* Changed LockSlot key to a KeybindList.
* Prevent CustomColorPicker from showing up on unsupported Storage Types.

## 2.6.0 (September 1, 2022)

### Added

* Added configurable SaddleBag type when Horse Overhaul is installed.

### Fixed

* Completely disable Shipping Bin features when a conflicting mod is installed.

### Changed

* Disallow any Chest from being automatically stashed into another chest.
* Craft From Chest will only lock chests when crafting items.
* Allow stacking of empty chests.
* When opening a held chest, gray out that chest in inventory.
* UnloadChest only activates when the target chest is clicked on.

## 2.5.5 (August 27, 2022)

### Fixed

* Fixed Unlimited Chests freezing the game.

## 2.5.4 (August 27, 2022)

### Changed

* Moved menu ChestLabel to not overlap with the vanilla Color Picker.

### Fixed

* Fixed ResizeChest and ResizeChestMenu not being Disabled.
* Fixed ChestLabel not supporting Zoom levels.
* Fixed TransferItems not saving config from GMCM menu.
* Fixed LabelChest not saving config from GMCM menu.
* Fixed config options not applying to Junimo Chests.

## 2.5.3 (August 21, 2022)

### Changed

* Added sound on picking up chests.
* Added sound on combining chests.
* Chests picked up from broken can be opened while held.

### Fixed

* Fixed vanilla color picker rendering under menu.
* Fixed AutoGrabber inventory menu overlapping with backpack inventory.

## 2.5.2 (August 16, 2022)

### Fixed

* Fixed CustomColorPicker being drawn over other menus.
* Fixed StashToChest not working for currently opened chest.

## 2.5.1 (August 15, 2022)

### Changed

* Assign actual values to default config options.

## 2.5.0 (August 14, 2022)

### Added

* Added multiple options for the Configure Menu.
    * Simple (Default) only shows Chest Label, Categorize, and Stash to Chest
      Priority/Stacks.
    * Categorize will load the Categorize Menu directly.
    * Full will show all the config options.
    * Advanced is full, and some options will be open text fields.

### Fixed

* Fixed CustomColorPicker drawing even when disabled.

### Changed

* Disabled features are now hidden from the configure menu.

## 2.4.0 (August 13, 2022)

### Added

* Added back context tag extensions.
    * category_artifact
    * category_furniture
    * donate_bundle
    * donate_museum
* UnloadChestCombine adds held chest capacity to target chest.

### Fixed

* Fixed features not being disabled when BetterShippingBin is disabled.
* Fixed tabs using non-localized hover text.
* Fixed CarryChestSlow debuff activating for empty chests.

### Changed

* Default Storage options are now part of the main config.
* The following features are now configurable individually or by type:
    * Configurator
    * HideItems
    * LabelChest
    * TransferItems
* Improved controller support.

## 2.3.1 (August 11, 2022)

### Added

* Added Chest preview to Color Picker.

### Fixed

* Fixed graphical glitches in menus.

## 2.3.0 (August 7, 2022)

### Added

* Added TransferItems feature to transfer items in/out of a chest.
* Added option to hide items instead of fading them out.
* Added negate search for categorizing items (hold shift).

### Fixed

* Fixed tabs being drawn over hover text.

### Changed

* Improvements to the Color Picker for reliability.
* Added localization support for storage names/tooltips.
* Require GMCM for Configurator feature.
* Optimizations for performance.

## 2.2.0 (July 15, 2022)

### Added

* Added config options for each vanilla chest type.

## 2.1.3 (July 13, 2022)

### Added

* Added support for matching modded interface colors.

### Changed

* CarryChest will not activate if holding a tool.

## 2.1.2 (July 12, 2022)

### Added

* Added an extra indicator for when you're in a tab.

### Fixed

* Fixed color changing everytime items are moved to/from a chest.

### Changed

* Updated tabs/search so they only fade items instead of hide them.
* Moved config icon back to top of side buttons.

## 2.1.1 (July 11, 2022)

### Fixed

* Fixed ChestMenuTabs appearing over some hover elements.
* Fixed Configure button not having background.
* Fixed Island Shipping Bin not launching correctly.
* Fixed cursor moving around on the Shipping Bin.
* Fixed AutoOrganize crash before saving.

## 2.1.0 (July 9, 2022)

### Added

* Added Chest Finder.
* Added localized tags for search and categorization.

### Fixed

* Fixed Shipping Bin not detected when launched by Chests Anywhere.
* Fixed items not updating when you add or remove items from a chest.
* Fixed AutoOrganize ignoring priority when moving items.

### Changed

* Configure chest now happens from the chest inventory menu.
* Improved integration with BetterCrafting for multiplayer.

## 2.0.3 (July 5, 2022)

### Added

* Added alert if FuryCore is installed.
* Added additional logging of config.
* Added additional logging of AutoOrganize.

### Fixed

* Fixed displayed items turning invisible in chests.

## 2.0.2 (July 2, 2022)

### Fixed

* Fixed CraftFromChest and StashToChest not working with Sheds/Fridges.
* Fixed wall of red text occurring when leaving farm.
* Fixed items not being in sync after organizing.
* Fixed error when opening up chests from ChestsAnywhere hotkey.

## 2.0.1 (July 1, 2022)

### Fixed

* Fixed Configure button being placed over community center.

## 2.0.0 (July 1, 2022)

### Added

* Added LabelChest feature.

### Fixed

* Fixed integration with BetterCrafting.
* Fixed crash when installed with BetterShippingBin.

### Changed

* Updated to SMAPI 3.15.0.
* Removed dependency on FuryCore.

### Changed

* Added Chinese Translation
* Added support for ModManifestBuilder.

### Fixed

* Updated BetterCrafting integration for v1.1.0+.

## 1.7.3 (March 25, 2022)

### Changed

* CarryChest will only work when no item is currently being held.
* FilterItems now handles negative only filters slightly differently.
    * Stashing requires at least one non-negative item to work.
    * Negative-only filters will continue to block disallowed items.

## 1.7.2 (February 26, 2022)

### Fixed

* Further optimized code to prevent looping over all items on each click.

## 1.7.1 (February 26, 2022)

* Updated to FuryCore 1.6.1

### Fixed

* Fixed lag associated with UnloadChests feature.
* Fixed FilterItems not patching Automate correctly.

## 1.7.0 (February 25, 2022)

* Updated to FuryCore 1.6.0

### Changed

* If BetterCrafting is loaded, use BetterCrafting API to open crafting page.

### Fixed

* Fixed disabled features being enabled by individual settings.
    * Disabled is intended to take precedence from default and/or chest types.

## 1.6.0 (February 23, 2022)

### Added

* Added integration
  with [Better Crafting](https://www.nexusmods.com/stardewvalley/mods/11115).

### Fixed

* Fixed controller getting stuck when switching tabs.

## 1.5.2 (February 23, 2022)

### Fixed

* Fixed Chests locking after activating Craft from Chest.

## 1.5.1 (February 22, 2022)

### Fixed

* Fixed AutoOrganize not working for any chest.

## 1.5.0 (February 22, 2022)

### Added

* Added AutoOrganize feature.
* Shipping Bin on Island is now recognized by Better Chests and uses Shipping
  Bin as it's chest type.
* Integrate Configurator using new FuryCore service.

### Changed

* Allow ModData override for storage names in locations and buildings
* Chests are now sorted in GMCM.
* More detailed logging when items are being stashed.

### Fixed

* Fixed StashToChest using CraftFromChest config for Default Chest.
* Fixed Fridge sometimes using default chest config.
* StashToChest will once again use StashToChestPriority.

## 1.4.2 (February 16, 2022)

### Fixed

* Fixed an error resulting from Chests that had an exact capacity of 72.

## 1.4.1 (February 15, 2022)

### Fixed

* Fixed CollectItems error.

## 1.4.0 (February 15, 2022)

### Added

* Added Organize Chest feature.
* Added Toolbar icons for Stash to Chest and Craft from Chest.
* Added Chest Menu for Shipping Bins.

### Changed

* Purge inaccessible cached objects.
* Optimized CollectItems code.

## 1.3.0 (February 12, 2022)

### Added

* Added support for Auto-Grabber.
* Added support for Junimo Hut.
* Added support for Shipping Bin.
* Added manual compatibility for XSLite chests.
    * Custom chest types must be defined from chests folder.

### Changed

* SlotLock Keybind is now a modifier key.
    * Must be held and left-click to lock a slot.
* LockedSlots are now attached to the item.
* Refactor to handle different types of storages.
* Refactor enumerating game objects into FuryCore service.

## 1.2.1 (February 6, 2022)

### Fixed

* Quick hotfix for ModIntegration error.

## 1.2.0 (February 6, 2022)

### Added

* Added CarryChestLimit option to limit the number of chests that can be carried
  at once.
* Added red text alerts to certain features.
    * When CarryChestLimit is reached and attempting to carry another chest.
    * When attempting to Craft from Chests and no eligible chests were found.
    * When attempting to Stash to Chests and no eligible chests were found.
* Added StashToChestPriority to Chest Data.
* Added more logging on `better_chests_info` command.
    * List out eligible chests for CraftFromChest.
    * List out eligible chests for StashToChest.
* Added CarryChestSlow for speed debuff while carrying a chest.
* Added integration for HorseOverhaul mod.

### Changed

* The ItemSelectionMenu is now affected by Better Chest features.
    * ResizeChestMenu will expand the ItemSelectionMenu.
    * ChestMenuTabs will filter the ItemSelectionMenu.
    * SearchItems will add the search bar to the ItemSelectionMenu.
* Locked Slots now holds items in place when shifting the toolbar.

## 1.1.0 (February 5, 2022)

### Fixed

* Updated method for keeping Chests in sync for multiplayer.

## 1.0.0 (February 3, 2022)

* Initial Version
