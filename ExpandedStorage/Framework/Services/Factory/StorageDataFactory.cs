namespace StardewMods.ExpandedStorage.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Data;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework.Models;

/// <summary>Managed storage data for all chests.</summary>
internal sealed class StorageDataFactory
{
    private readonly ConditionalWeakTable<Item, IStorageData?> cachedChests = new();
    private readonly Dictionary<string, DictionaryStorageData?> cachedTypes = new();

    /// <summary>Attempts to retrieve storage data for an item.</summary>
    /// <param name="item">The item for which to retrieve the storage data.</param>
    /// <param name="storageData">
    /// When this method returns, contains the storage data for the item, if it exists; otherwise,
    /// contains null. This parameter is passed uninitialized.
    /// </param>
    /// <returns><c>true</c> if the storage data for the item is successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData)
    {
        if (this.cachedChests.TryGetValue(item, out storageData))
        {
            return storageData is not null;
        }

        if (!this.cachedTypes.TryGetValue(item.ItemId, out var typeData))
        {
            var customFields = GetCustomFields();
            if (customFields?.GetBool(Mod.Mod.Id + "/Enabled") != true)
            {
                this.cachedTypes[item.ItemId] = null;
                return false;
            }

            var typeModel = new DictionaryModel(GetCustomFields);
            typeData = new DictionaryStorageData(typeModel);
            this.cachedTypes.Add(item.ItemId, typeData);
        }

        if (typeData is null)
        {
            return false;
        }

        var chestModel = new ModDataModel(item.modData);
        var chestData = new DictionaryStorageData(chestModel);
        storageData = new StorageData(typeData, chestData);
        this.cachedChests.Add(item, storageData);
        return true;

        Dictionary<string, string>? GetCustomFields() =>
            Game1.bigCraftableData.TryGetValue(item.ItemId, out var bigCraftableData)
                ? bigCraftableData.CustomFields
                : null;
    }
}