namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>The event arguments before an item is transferred into a container.</summary>
internal sealed class ItemTransferringEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ItemTransferringEventArgs" /> class.</summary>
    /// <param name="into">The container being transferred into.</param>
    /// <param name="item">The item being transferred.</param>
    public ItemTransferringEventArgs(IStorageContainer into, Item item)
    {
        this.Into = into;
        this.Item = item;
    }

    /// <summary>Gets the destination container to which the item was sent.</summary>
    public IStorageContainer Into { get; }

    /// <summary>Gets the item that was transferred.</summary>
    public Item Item { get; }

    /// <summary>Gets a value indicating whether the the transfer is allowed.</summary>
    public bool IsAllowed { get; private set; }

    /// <summary>Gets a value indicating whether the the transfer is prevented.</summary>
    public bool IsPrevented { get; private set; }

    /// <summary>Allow the transfer.</summary>
    public void AllowTransfer() => this.IsAllowed = true;

    /// <summary>Prevent the transfer.</summary>
    public void PreventTransfer() => this.IsPrevented = true;
}