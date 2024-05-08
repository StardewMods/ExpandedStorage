namespace StardewMods.BetterChests.Framework.Models;

using System.Globalization;
using System.Text;
using StardewModdingAPI.Utilities;

/// <summary>Controls config data.</summary>
internal sealed class Controls
{
    /// <summary>Gets or sets controls to access chests.</summary>
    public KeybindList AccessChests { get; set; } = new(SButton.B);

    /// <summary>Gets or sets a value to access the previous chest.</summary>
    public KeybindList AccessPreviousChest { get; set; } = new(SButton.LeftTrigger);

    /// <summary>Gets or sets a value to access the next chest.</summary>
    public KeybindList AccessNextChest { get; set; } = new(SButton.RightTrigger);

    /// <summary>Gets or sets a value to clear the current search.</summary>
    public KeybindList ClearSearch { get; set; } = new(SButton.Escape);

    /// <summary>Gets or sets controls to configure currently held object.</summary>
    public KeybindList ConfigureChest { get; set; } = new(SButton.End);

    /// <summary>Gets or sets controls for copying.</summary>
    public KeybindList Copy { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.C),
        new Keybind(SButton.RightControl, SButton.C));

    /// <summary>Gets or sets controls to lock an item slot.</summary>
    public KeybindList LockSlot { get; set; } = new(SButton.LeftAlt);

    /// <summary>Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage" />.</summary>
    public KeybindList OpenCrafting { get; set; } = new(SButton.K);

    /// <summary>Gets or sets controls to open the found chest.</summary>
    public KeybindList OpenFoundChest { get; set; } = new(
        new Keybind(SButton.LeftShift, SButton.Enter),
        new Keybind(SButton.RightShift, SButton.Enter));

    /// <summary>Gets or sets controls for pasting.</summary>
    public KeybindList Paste { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.V),
        new Keybind(SButton.RightControl, SButton.V));

    /// <summary>Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> down.</summary>
    public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

    /// <summary>Gets or sets controls to scroll by page instead of row.</summary>
    public KeybindList ScrollPage { get; set; } = new(new Keybind(SButton.LeftShift), new Keybind(SButton.RightShift));

    /// <summary>Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> up.</summary>
    public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

    /// <summary>Gets or sets controls to stash player items into storages.</summary>
    public KeybindList StashItems { get; set; } = new(SButton.Z);

    /// <summary>Gets or sets controls to toggle item collection on or off.</summary>
    public KeybindList ToggleCollectItems { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.Space),
        new Keybind(SButton.RightControl, SButton.Space));

    /// <summary>Gets or sets controls to toggle chest info on or off.</summary>
    public KeybindList ToggleInfo { get; set; } = new(
        new Keybind(SButton.LeftShift, SButton.OemQuestion),
        new Keybind(SButton.RightShift, SButton.OemQuestion));

    /// <summary>Gets or sets controls to toggle search bar on or off.</summary>
    public KeybindList ToggleSearch { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.F),
        new Keybind(SButton.RightControl, SButton.F));

    /// <summary>Gets or sets controls to lock an item slot.</summary>
    public KeybindList TransferItems { get; set; } = new(
        new Keybind(SButton.LeftShift),
        new Keybind(SButton.RightShift));

    /// <summary>Gets or sets controls to lock an item slot.</summary>
    public KeybindList TransferItemsReverse { get; set; } = new(
        new Keybind(SButton.LeftAlt),
        new Keybind(SButton.RightAlt));

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.AccessChests)}: {this.AccessChests}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.AccessNextChest)}: {this.AccessNextChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.AccessPreviousChest)}: {this.AccessPreviousChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ClearSearch)}: {this.ClearSearch}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ConfigureChest)}: {this.ConfigureChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.LockSlot)}: {this.LockSlot}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.OpenCrafting)}: {this.OpenCrafting}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.OpenFoundChest)}: {this.OpenFoundChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ScrollDown)}: {this.ScrollDown}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ScrollPage)}: {this.ScrollPage}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ScrollUp)}: {this.ScrollUp}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.StashItems)}: {this.StashItems}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ToggleCollectItems)}: {this.ToggleCollectItems}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ToggleInfo)}: {this.ToggleInfo}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.ToggleSearch)}: {this.ToggleSearch}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.TransferItems)}: {this.TransferItems}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.TransferItemsReverse)}: {this.TransferItemsReverse}");

        return sb.ToString();
    }
}