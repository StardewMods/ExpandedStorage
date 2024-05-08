# Better Chests

Adds enhanced chest features to the game.

## Contents

* [Features](#features)
    * [Access Chests](#access-chests)
    * [Auto Organize](#auto-organize)
    * [Carry Chest](#carry-chest)
    * [Categorize Chest](#categorize-chest)
    * [Chest Finder](#chest-finder)
    * [Collect Items](#collect-items)
    * [Configure Chest](#configure-chest)
    * [Craft From Chest](#craft-from-chest)
    * [HSL Color Picker](#hsl-color-picker)
    * [Inventory Tabs](#inventory-tabs)
    * [Lock Item](#lock-item)
    * [Open Held Chest](#open-held-chest)
    * [Resize Chest](#resize-chest)
    * [Search Items](#search-items)
    * [Shop From Chest](#shop-from-chest)
    * [Sort Inventory](#sort-inventory)
    * [Stash To Chest](#stash-to-chest)
    * [Storage Info](#storage-info)
* [Usage](#usage)
    * [Using Search](#using-search)
* [Configurations](#configurations)
    * [Config Inheritance](#config-inheritance)
    * [Sort By Values](#group-by-values)
    * [Info Values](#in-game-menus)
    * [Option Values](#option-values)
    * [Range Values](#range-values)
* [Mod Integrations](#mod-integrations)
    * [Automate](#automate)
    * [Better Crafting](#better-crafting)
    * [Horse Overhaul](#horse-overhaul)
* [Customization](#customization)
    * [Other Assets](#other-assets)
* [Translations](#translations)

## Features

### Access Chests

Remotely access storages in the game from anywhere.

| Config                | Description                                           | Default Value  | Other Value(s)                                                   |
|:----------------------|:------------------------------------------------------|:---------------|:-----------------------------------------------------------------|
| AccessChest           | Enables the Access Chest feature.                     | `"Location"`   | `"Disabled"`, `"Default"`, `"Inventory"`, `"World"` <sup>1</sup> |
| AccessChestShowArrows | Displays previous/next arrows for navigating chests.  | `true`         | `true`, `false`                                                  |
| AccessPreviousChest   | Assigns the keybind for accessing the previous chest. | `LeftTrigger`  | Any valid button code.<sup>2</sup>                               |
| AccessNextChest       | Assigns the keybind for accessing the next chest.     | `RightTrigger` | Any valid button code.<sup>2</sup>                               |

1. See [Range Values](#range-values).
2.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Auto Organize

Every night, after going to bed, your chests will automatically organize items
based on the same rules as [Categorize Chest](#categorize-chest)
and  [Stash to Chest](#stash-to-chest). Items will only move between chests with
the feature enabled, and only from a lower priority chest to a higher priority
chest.

It also applies your [Sort Inventory](#sort-inventory) rules at the end.

| Config       | Description                         | Default Value | Other Value(s)                        |
|:-------------|:------------------------------------|:--------------|:--------------------------------------|
| AutoOrganize | Enables the  Auto Organize feature. | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |

1. See [Option Values](#option-values).

### Carry Chest

With Carry Chest enabled, you can hit the Use Tool button to pick up chests into
your inventory even if it has items.

| Config               | Description                                            | Default Value | Other Value(s)                                                       |
|:---------------------|:-------------------------------------------------------|:--------------|:---------------------------------------------------------------------|
| CarryChest           | Enables the Carry Chest feature.                       | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup>                                |
| CarryChestLimit      | Limits how many chests can be carried.                 | `3`           | Any positive integer.<sup>2</sup>                                    |
| CarryChestSlow       | Enables the Carry Chest Slow feature.                  | `Enabled`     | `"Disabled"`, `"Default"`<sup>1</sup>                                |
| CarryChestSlowAmount | The speed penalty for carrying chests above the limit. | `-1`          | Positive integer from `0` (normal speed) to `4` (slime speed debuff) |
| CarryChestSlowLimit  | The limit in which the slowness effect is applied.     | `1`           | Any positive integer.<sup>2</sup>                                    |

1. See [Option Values](#option-values).
2. Use `0` for unlimited chests.

### Categorize Chest

Assigns items to a chest based on a search term to be used
for [Auto Organize](#auto-organize) and [Stash to Chest](#stash-to-chest).

See [Using Search](#using-search) for more information.

| Config                       | Description                                          | Default Value | Other Value(s)                        |
|:-----------------------------|:-----------------------------------------------------|:--------------|:--------------------------------------|
| CategorizeChest              | Enables the Categorize Chest feature.                | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| CategorizeChestBlockItems    | Determines if non-categorized items will be blocked. | `"Disabled"`  | `"Disabled"`, `"Default"`<sup>1</sup> |
| CategorizeChestIncludeStacks | Determines if existing stacks will be included.      | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |

1. See [Option Values](#option-values).

### Chest Finder

Search for which chest(s) have the item you're looking for.

See [Using Search](#using-search) for more information.

| Config       | Description                                   | Default Value                         | Other Value(s)                        |
|:-------------|:----------------------------------------------|---------------------------------------|:--------------------------------------|
| ChestFinder  | Enables the Chest Finder feature.             | `"Enabled"`                           | `"Disabled"`, `"Default"`<sup>1</sup> |
| ToggleSearch | Assigns a keybind to toggle the chest finder. | `"LeftControl + F, RightControl + F"` | Any valid button code.<sup>1</sup>    |

1. See [Option Values](#option-values).
2.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Collect Items

While you are carrying a chest in your inventory, any items you pick up may be
collected directly into the chest,
bypassing your inventory.<sup>1</sup>

| Config       | Description                        | Default Value | Other Value(s)                        |
|:-------------|:-----------------------------------|:--------------|:--------------------------------------|
| CollectItems | Enables the Collect Items feature. | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |

1. See [Option Values](#option-values).

### Configure Chest

Set individual configuration options for storages.

| Config         | Description                                  | Default Value | Other Value(s)                        |
|:---------------|:---------------------------------------------|:--------------|:--------------------------------------|
| ConfigureChest | Enables the Configure Chest feature.         | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| ConfigureChest | Assigns the keybind for configuring a chest. | `"End"`       | Any valid button code.<sup>2</sup>    |

1. See [Option Values](#option-values).
2.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Craft From Chest

Hit a configurable key to bring up a crafting menu that uses items stored in
nearby chests. This feature requires Better Crafting to be installed.

| Config                         | Description                                               | Default Value | Other Value(s)                                                                 |
|:-------------------------------|:----------------------------------------------------------|:--------------|:-------------------------------------------------------------------------------|
| CraftFromChest                 | Enables the Craft From Chest feature.                     | `"Location"`  | `"Disabled"`, `"Default"`, `"Inventory"`, `"Location"`, `"World"` <sup>1</sup> |
| OpenCrafting                   | Assigns the keybind for opening the crafting menu.        | `"K"`         | Any valid button code.<sup>2</sup>                                             |
| CraftFromChestDisableLocations | A list of locations that crafting will not be allowed in. | `[]`          | The locations to block.<sup>3</sup>                                            |
| CraftFromChestDistance         | Limits the distance that a chest can be crafted from.     | -1            | Any positive integer or `-1`.<sup>4</sup>                                      |

1. See [Range Values](#range-values).
2.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

3. Add `"UndergroundMine"` to the list to disable in Mine and Skull Cavern.
4. Measured in tiles away from the player. Use `-1` for "unlimited" distance.

### HSL Color Picker

Replaces the Chest Color Picker with a more precise version that lets you pick a
color with sliders for hue, saturation,
and lightness.

| Config                        | Description                                       | Default Value | Other Value(s)                        |
|:------------------------------|:--------------------------------------------------|:--------------|:--------------------------------------|
| HSLColorPicker                | Enables the HSL Color Picker feature.             | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| HSLColorPickerHueSteps        | How many intervals of hue can be selected.        | `29`          | Any positive integer.                 |
| HSLColorPickerSaturationSteps | How many intervals of saturation can be selected. | `16`          | Any positive integer.`                |
| HSLColorPickerLightnessSteps  | How many intervals of lightness can be selected.  | `16`          | Any positive integer.                 |
| HSLColorPickerPlacement       | Which side will the Color Picker be added to.     | `"Right"`     | `"Left"`                              |

1. See [Option Values](#option-values).

### Inventory Tabs

Adds inventory tabs to the side of the menu.

| Config           | Description                                 | Default Value | Other Value(s)                        |
|:-----------------|:--------------------------------------------|:--------------|:--------------------------------------|
| InventoryTabs    | Enables the Inventory Tabs feature.         | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| InventoryTabList | Determines what inventory tabs will appear. |               |                                       |

### Lock Item

Lock an item in its storage so to prevent it from being moved or sold.

| Config       | Description                                     | Default Value | Other Value(s)                        |
|:-------------|:------------------------------------------------|:--------------|:--------------------------------------|
| LockItem     | Enables the Lock Item feature.                  | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| LockItemHold | Hold the LockSlot key and click to lock a slot. | `true`        | `false`                               |
| LockSlot     | Assigns the keybind for locking a slot.         | `"A"`         | Any valid button code.<sup>3</sup>    |

1. See [Option Values](#option-values).
2.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Open Held Chest

With a chest as your active item, you can hit the action button to bring up the
chest menu and access the chests
contents.

| Config        | Description                          | Default Value | Other Value(s)                        |
|:--------------|:-------------------------------------|:--------------|:--------------------------------------|
| OpenHeldChest | Enables the Open Held Chest feature. | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |

1. See [Option Values](#option-values).

### Resize Chest

The default storage for a chest is 36 items. With Resize Chest enabled you can
increase storage space up to virtually
unlimited storage.<sup>1</sup>

| Config              | Description                                             | Default Value             | Other Value(s)                                                          |
|:--------------------|:--------------------------------------------------------|:--------------------------|:------------------------------------------------------------------------|
| ResizeChest         | Enables the Resize Chest feature.                       | `"Large"`                 | `"Disabled"`, `"Default"`, `"Small"`, `"Medium"`, `"Large"`<sup>2</sup> |
| ResizeChestCapacity | The number of items the chest can store.                | `70`                      | Any positive integer or `-1`.<sup>3</sup>                               |
| ScrollDown          | Assigns a keybind to scroll down.                       | `"DPadDown"`              | Any valid button code.<sup>4</sup>                                      |
| ScrollUp            | Assigns a keybind to scroll up.                         | `"DPadUp"`                | Any valid button code.<sup>4</sup>                                      |
| ScrollPage          | Assigns a keybind to hold to scroll one page at a time. | `"LeftShift, RightShift"` | Any valid button code.<sup>4</sup>                                      |

1. If the number of items exceeds the menu space, you can scroll to access the
   overflow.
2. See [Option Values](#option-values).
3. Use `-1` for "unlimited" items or a positive multiple of 12 for limited
   storage.
4.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Search Items

Adds a search bar to the top of the Chest Menu to only display items that meet a
search criteria.

See [Using Search](#using-search) for more information.

| Config            | Description                                | Default Value | Other Value(s)                                     |
|:------------------|:-------------------------------------------|:--------------|:---------------------------------------------------|
| SearchItems       | Enables the Search Items feature.          | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup>              |
| SearchItemsMethod | Determines how the search will be applied. | `"GrayedOut"` | `"Default"`, `"Sorted"`, `"GrayedOut"`, `"Hidden"` |

1. See [Option Values](#option-values).

### Shop From Chest

Allows shops to access items in chests for purchases.

| Config        | Description                          | Default Value | Other Value(s)                                                                 |
|:--------------|:-------------------------------------|:--------------|:-------------------------------------------------------------------------------|
| ShopFromChest | Enables the Shop From Chest feature. | `"Location"`  | `"Disabled"`, `"Default"`, `"Inventory"`, `"Location"`, `"World"` <sup>1</sup> |

1. See [Range Values](#range-values).

### Sort Inventory

Organize Chest allows you to group and sort items by a configurable property of
those items.

| Config Option   | Description                                  | Default Value | Other Value(s)                         |
|:----------------|:---------------------------------------------|:--------------|:---------------------------------------|
| SortInventory   | Enables the Sort Inventory feature.          | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup>  |
| SortInventoryBy | The fields that inventory will be sorted by. | `""`          | See [Sort By Values](#sort-by-values). |

1. See [Option Values](#option-values).

### Stash To Chest

Hit a configurable key to instantly stash items from your inventory into nearby
chests.<sup>1</sup>

| Config Option                | Description                                               | Default Value             | Other Value(s)                                                                |
|:-----------------------------|:----------------------------------------------------------|:--------------------------|:------------------------------------------------------------------------------|
| StashToChest                 | Enables the Stash To Chest feature.                       | `"Location"`              | `"Disabled"`, `"Default"`, `"Inventory"`, `"Location"`, `"World"`<sup>2</sup> |
| StashItems                   | Assigns the keybind for stashing items.                   | `"Z"`                     | Any valid button code.<sup>3</sup>                                            |
| StashToChestDisableLocations | A list of locations that stashing will not be allowed in. | `[]`                      | The locations to block.<sup>4</sup>                                           |
| StashToChestDistance         | Limits the distance that a chest can be stashed into.     | -1                        | Any positive integer or `-1`.<sup>5</sup>                                     |
| StashToChestPriority         | Prioritize certain chests over others.                    | 0                         | Any integer value.                                                            |
| TransferItems                | Assigns the keybind to hold for transferring items up.    | `"LeftShift, RightShift"` | Any valid button code.<sup>3</sup>                                            |
| TransferItemsReverse         | Assigns the keybind to hold for transferring items down.  | `"LeftAlt, RightAlt"`     | Any valid button code.<sup>3</sup>                                            |

1. Included chests are determined by config options.
2. See [Range Values](#range-values).
3.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

4. Add `"UndergroundMine"` to the list to disable in Mine and Skull Cavern.
5. Measured in tiles away from the player. Use `-1` for "unlimited" distance.

### Storage Info

Display stats about the storage in the chest menu and when hovering the cursor
over a storage.

| Config                | Description                                | Default Value                                                                      | Other Value(s)                        |
|:----------------------|:-------------------------------------------|------------------------------------------------------------------------------------|:--------------------------------------|
| StorageInfo           | Enables the Chest Info feature.            | `"Enabled"`                                                                        | `"Disabled"`, `"Default"`<sup>1</sup> |
| StorageInfoHoverItems | The info to show on hover.                 | `"Icon","Name","Type","Capacity","TotalValue"`                                     | Any valid button code.<sup>2</sup>    |
| StorageInfoMenuItems  | The info to show in menu.                  | `"Type","Location","Position","Inventory","TotalItems","UniqueItems","TotalValue"` | See [Info Values](#info-values).      |
| ToggleInfo            | Assigns a keybind to show/hide chest info. | `"LeftShift + OemQuestion, RightShift + OemQuestion"`                              | See [Info Values](#info-values).      |

1. See [Option Values](#option-values).
2.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

## Usage

### Using Search

Searching can be applied to an item's name, but it's often more useful to use
tags which are shared by multiple items.

The game adds
various [Context Tags](https://stardewvalleywiki.com/Modding:Items#Context_tags)
to each item which are used throughout this mod.

There are a few ways to see what context tags each item contains:

* Enter the console command `debug listtags` to show all tags for the currently
  held item.
* Refer to
  the [Modding Docs](https://stardewvalleywiki.com/Modding:Items#Context_tags)
  for some tags.
* Install [Lookup Anything](https://www.nexusmods.com/stardewvalley/mods/541),
  enable [ShowDataMiningField](https://github.com/Pathoschild/StardewMods/tree/stable/LookupAnything#configure)
  in its
  config and hit F1 while hovering over any item.

Here are examples of some useful tags:

| Description | Tags                                                        |
|-------------|-------------------------------------------------------------|
| Category    | `category_clothing`, `category_boots`, `category_hats`, ... |
| Color       | `color_red`, `color_blue`, ...                              |
| Name        | `item_sap`, `item_wood`, ...                                |
| Type        | `wood_item`, `trash_item`, ...                              |
| Quality     | `quality_none`, `quality_gold`, ...                         |
| Season      | `season_spring`, `season_fall`, ...                         |
| Index       | `id_o_709`, `id_r_529`, ...                                 |

## Configurations

For ease of use, it is recommended to set config options
from [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).

### Config Inheritance

For some config options, the same options can be applied to multiple levels. The
lowest level config will always take
precedence.

* **Default Chest** - Every chest inherits from the Default Chest options.
* **Chest Type** - Applies to a particular type of chest.
    * Chest
    * Stone Chest
    * Junimo Chest
    * Auto-Grabber
    * Mini-Fridge
    * Mini-Shipping Bin
    * Fridge
    * Custom chests added by other mods.<sup>1</sup>
* **Individual Chest** - A single instance of a chest can be configured
  individually.

### Sort By Values

Group by is a text property of the item that organize will order by first.

* **Category** - Sort by the category.
* **Name** - Sort by the name.
* **Quantity** - Sort by the quantity.
* **Quality** - Sort by the quality.
* **Type** - Sort by the item type.

## Info Values

The types of info that can be displayed.

* **Name** - The storage name.
* **Icon** - The storage icon.
* **Type** - The type of storage.
* **Location** - The location of the storage.
* **Position** - The position of the storage.
* **Inventory** - The farmer whose inventory contains the storage.
* **Capacity** - The number of item stacks and slots in the storage.
* **TotalItems** - The total items in the storage.
* **UniqueItems** - The number of unique items in the storage.
* **TotalValue** - The total value of all items in the storage.

### Option Values

The option value determines whether a feature will be enabled or disabled for a
chest.

* **Default** - The value will be inherited from a parent config.<sup>1</sup>
* **Disabled** - The feature will be disabled.
* **Enabled** - The feature will be enabled.

1. If parent value is unspecified, Enabled will be the default value.

### Range Values

The Range value limits which chests will be selected for a feature relative to
the player.

* **Default** - The value will be inherited from a parent config.<sup>1</sup>
* **Disabled** - The feature will be disabled.
* **Inventory** - Only chests in the player inventory.
* **Location** - Only chests in the players current location.
* **World** - Any chest accessible to the player in the world.

1. If parent value is unspecified, Location will be the default value.

## Mod Integrations

### Automate

When [Filter Items](#filter-items) is enabled, then any categorizations that a
chest has will be applied to
[Automate](https://www.nexusmods.com/stardewvalley/mods/1063). This means that
Automate will be blocked from adding
items into the chest if the Filter list does not allow it.

### Better Crafting

Craft from Chest will launch a Better Crafting Page and it will correctly
include all chests with the feature enabled.

### Horse Overhaul

Better Chests automatically integrates
with [Horse Overhaul](https://www.nexusmods.com/stardewvalley/mods/7911)
saddlebags. The distance to the player's Horse will be considered for features
such
as [Craft from Chest](#craft-from-chest) and
[Stash to Chest](#stash-to-chest).

The SaddleBag can have its own Better Chest config by adding an entry for a
chest named `"SaddleBag"` to the
`BetterChests/assets/chests.json` file.

## Customization

### API

Register your chest using
the [Better Chests API](../Common/Integrations/BetterChests/IBetterChestsApi.cs).

### Other Assets

#### Icons

Replace any or all of the icons for the Configure, Craft from Chest, and Stash
to Chest buttons by editing the
image<sup>1</sup>:

`furyx639.BetterChests/Icons`.

Sample `content.json`:

```jsonc
{
  "Format": "1.24.0",
  "Changes": [
    {
      "Action": "EditImage",
      "Target": "furyx639.BetterChests/Icons",
      "FromFile": "assets/MyConfigureButton.png",
      "FromArea": {"X": 0, "Y": 0, "Width": 16, "Height": 16},
      "ToArea": {"X": 0, "Y": 0, "Width": 16, "Height": 16}
    },
  ]
}
```

1. See
   the [Edit Image](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editimage.md)
   docs for Content Patcher.

#### Tab Texture

Replace any or all of the default tab textures by editing the image<sup>1</sup>:

`furyx639.BetterChests\\Tabs\\Textures`

Sample `content.json`:

```jsonc
{
  "Format": "1.24.0",
  "Changes": [
    // Replace texture for mining icon
    {
      "Action": "EditImage",
      "Target": "furyx639.BetterChests\\Tabs\\Texture",
      "FromFile": "assets/mining-icon.png",
      "FromArea": {"X": 0, "Y": 0, "Width": 16, "Height": 16},
      "ToArea" {"X": 48, "Y": 0, "Width": 16, "Height": 16}
    }
  ]
}
```

1. See
   the [Edit Image](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editimage.md)
   docs for Content Patcher.

## Translations

See [here](i18n/default.json) for the base translation file.

| Language                   | Status            | Credits              |
|:---------------------------|:------------------|:---------------------|
| [Chinese](i18n/zh.json)    | ✔️ Complete       | Andyc66              |
| [French](i18n/fr.json)     | ❔ Incomplete      | Ayatus               |
| [German](i18n/de.json)     | ❔ Incomplete      | Loni4ever            |
| [Hungarian](i18n/hu.json)  | ❔ Incomplete      | martin66789          |
| [Italian](i18n/it.json)    | ✔️ Complete       | zomboide             |
| Japanese                   | ❌️ Not Translated |                      |
| [Korean](i18n/ko.json)     | ✔️ Complete       | wally232             |
| [Portuguese](i18n/pt.json) | ✔️ Complete       | Aulberon             |
| [Russian](i18n/ru.json)    | ❔ Incomplete      | Newrotd              |
| [Spanish](i18n/es.json)    | ✔️ Complete       | Querbis              |
| [Turkish](i18n/tr.json)    | ✔️ Complete       | KawaiFoxHappyClaws76 |
