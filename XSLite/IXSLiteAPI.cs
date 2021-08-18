using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace XSLite
{
    public interface IXSLiteAPI
    {
        /// <summary>Load a directory as an Expanded Storage content pack.</summary>
        /// <param name="path">Path containing expandedStorage.json file.</param>
        /// <returns>True if content was loaded successfully.</returns>
        bool LoadContentPack(string path);

        /// <summary>Load an Expanded Storage content pack.</summary>
        /// <param name="contentPack">The content pack to load.</param>
        /// <returns>True if content was loaded successfully.</returns>
        bool LoadContentPack(IContentPack contentPack);
        
        /// <summary>Checks whether an item is allowed to be added to a chest.</summary>
        /// <param name="chest">The chest to add to.</param>
        /// <param name="item">The item to be added.</param>
        /// <returns>True if chest accepts the item.</returns>
        bool AcceptsItem(Chest chest, Item item);
    }
}