namespace StardewMods.ExpandedStorage.Framework.Services;

using System.Runtime.CompilerServices;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Data;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework.Models;

/// <summary>Managed storage data for all chests.</summary>
internal sealed class StorageDataFactory
{
    private readonly ConditionalWeakTable<Item, IStorageData?> cachedChests = new();
    private readonly Dictionary<string, StorageData?> cachedTypes = new();

    /// <summary>Attempts to retrieve storage data for an item type based on the item id.</summary>
    /// <param name="itemId">The id for the item from which to retrieve the storage data.</param>
    /// <param name="storageData">When this method returns, contains the storage data; otherwise, null.</param>
    /// <returns><c>true</c> if the storage data was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetData(string itemId, [NotNullWhen(true)] out StorageData? storageData)
    {
        storageData = null;
        if (!this.cachedTypes.TryGetValue(itemId, out var typeData))
        {
            var customFields = GetCustomFields();
            if (customFields?.GetBool($"{Mod.Id}/Enabled") != true)
            {
                this.cachedTypes[itemId] = null;
                return false;
            }

            var typeModel = new DictionaryModel(GetCustomFields);
            typeData = new StorageData(typeModel);
            this.cachedTypes.Add(itemId, typeData);
        }

        storageData = typeData;
        return storageData is not null;

        Dictionary<string, string>? GetCustomFields() =>
            Game1.bigCraftableData.TryGetValue(itemId, out var bigCraftableData) ? bigCraftableData.CustomFields : null;
    }

    /// <summary>Attempts to retrieve storage data for an item.</summary>
    /// <param name="item">The item for which to retrieve the storage data.</param>
    /// <param name="storageData">When this method returns, contains the storage data; otherwise, null.</param>
    /// <returns><c>true</c> if the storage data was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData)
    {
        if (this.cachedChests.TryGetValue(item, out storageData))
        {
            return storageData is not null;
        }

        if (!this.TryGetData(item.ItemId, out var typeData))
        {
            return false;
        }

        var chestModel = new ModDataModel(item.modData);
        var chestData = new StorageData(chestModel);
        storageData = new Storage(typeData, chestData);
        this.cachedChests.Add(item, storageData);
        return true;
    }
}