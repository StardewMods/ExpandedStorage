namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <summary>The event arguments before an item is transferred into a container.</summary>
internal sealed class ItemTransferringEventArgs(IStorageContainer into, Item item, bool force) : EventArgs,
    IItemTransferring
{
    private bool isAllowed;

    private bool isPrevented;

    /// <inheritdoc />
    public bool IsForced { get; } = force;

    /// <inheritdoc />
    public IStorageContainer Into { get; } = into;

    /// <inheritdoc />
    public Item Item { get; } = item;

    /// <inheritdoc />
    public bool IsAllowed => this.isAllowed && !this.isPrevented;

    /// <inheritdoc />
    public void AllowTransfer() => this.isAllowed = true;

    /// <inheritdoc />
    public void PreventTransfer() => this.isPrevented = true;
}