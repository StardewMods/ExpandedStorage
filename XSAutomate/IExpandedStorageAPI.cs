using StardewValley;
using StardewValley.Objects;

namespace ImJustMatt.XSAutomate
{
    public interface IExpandedStorageAPI
    {
        /// <summary>Checks whether an item is allowed to be added to a chest.</summary>
        /// <param name="chest">The chest to add to.</param>
        /// <param name="item">The item to be added.</param>
        /// <returns>True if chest accepts the item.</returns>
        bool AcceptsItem(Chest chest, Item item);
    }
}