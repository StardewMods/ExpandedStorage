﻿namespace StardewMods.BetterChests.Models;

using System.Collections.Generic;
using Common.Enums;
using StardewMods.BetterChests.Enums;

/// <inheritdoc />
internal class StorageData : IStorageData
{
    /// <inheritdoc />
    public FeatureOption AutoOrganize { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CarryChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public HashSet<string>? ChestMenuTabSet { get; set; }

    /// <inheritdoc />
    public FeatureOption CollectItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest { get; set; } = FeatureOptionRange.Default;

    /// <inheritdoc />
    public HashSet<string>? CraftFromChestDisableLocations { get; set; }

    /// <inheritdoc />
    public int CraftFromChestDistance { get; set; }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption FilterItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public HashSet<string>? FilterItemsList { get; set; }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption OrganizeChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy { get; set; } = GroupBy.Default;

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy { get; set; } = SortBy.Default;

    /// <inheritdoc />
    public FeatureOption ResizeChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public int ResizeChestCapacity { get; set; }

    /// <inheritdoc />
    public FeatureOption ResizeChestMenu { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public int ResizeChestMenuRows { get; set; }

    /// <inheritdoc />
    public FeatureOption SearchItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOptionRange StashToChest { get; set; } = FeatureOptionRange.Default;

    /// <inheritdoc />
    public HashSet<string>? StashToChestDisableLocations { get; set; }

    /// <inheritdoc />
    public int StashToChestDistance { get; set; }

    /// <inheritdoc />
    public int StashToChestPriority { get; set; }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption UnloadChest { get; set; } = FeatureOption.Default;
}