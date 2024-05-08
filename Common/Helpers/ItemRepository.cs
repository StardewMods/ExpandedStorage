namespace StardewMods.Common.Helpers;

using StardewValley.ItemTypeDefinitions;

/// <summary>Provides methods for retrieving items based on a predicate.</summary>
internal static class ItemRepository
{
    /// <summary>Retrieves all items from the item registry.</summary>
    /// <param name="flavored">Indicates whether flavored items should be included.</param>
    /// <param name="identifiers">Identifiers of specific items to retrieve. If null or empty, retrieves all items.</param>
    /// <returns>An enumerable collection of Item objects.</returns>
    public static IEnumerable<Item> GetAll(bool flavored = true, params string[]? identifiers)
    {
        foreach (var itemType in ItemRegistry.ItemTypes)
        {
            if (identifiers is not null && identifiers.Any() && !identifiers.Contains(itemType.Identifier))
            {
                continue;
            }

            var definition = ItemRegistry.GetTypeDefinition(itemType.Identifier);
            foreach (var itemId in itemType.GetAllIds())
            {
                var item = ItemRegistry.Create(itemType.Identifier + itemId);
                if (!flavored)
                {
                    yield return item;

                    continue;
                }

                switch (definition)
                {
                    case ObjectDataDefinition objectDataDefinition:
                        if (item.QualifiedItemId == "(O)340")
                        {
                            yield return objectDataDefinition.CreateFlavoredHoney(null);

                            continue;
                        }

                        var ingredient = item as SObject;
                        switch (item.Category)
                        {
                            case SObject.FruitsCategory:
                                yield return objectDataDefinition.CreateFlavoredWine(ingredient);
                                yield return objectDataDefinition.CreateFlavoredJelly(ingredient);
                                yield return objectDataDefinition.CreateFlavoredDriedFruit(ingredient);

                                break;

                            case SObject.VegetableCategory:
                                yield return objectDataDefinition.CreateFlavoredJuice(ingredient);
                                yield return objectDataDefinition.CreateFlavoredPickle(ingredient);

                                break;

                            case SObject.flowersCategory:
                                yield return objectDataDefinition.CreateFlavoredHoney(ingredient);

                                break;

                            case SObject.FishCategory:
                                yield return objectDataDefinition.CreateFlavoredBait(ingredient);
                                yield return objectDataDefinition.CreateFlavoredSmokedFish(ingredient);

                                break;

                            case SObject.sellAtFishShopCategory when item.QualifiedItemId == "(O)812":
                                foreach (var fishPondData in DataLoader.FishPondData(Game1.content))
                                {
                                    if (fishPondData.ProducedItems.All(
                                        producedItem => producedItem.ItemId != item.QualifiedItemId))
                                    {
                                        continue;
                                    }

                                    foreach (var fishPondItem in ItemRepository.GetAll(false, "(O)"))
                                    {
                                        if (fishPondItem is SObject fishPondObject
                                            && fishPondData.RequiredTags.All(fishPondItem.HasContextTag))
                                        {
                                            yield return objectDataDefinition.CreateFlavoredRoe(fishPondObject);
                                            yield return objectDataDefinition.CreateFlavoredAgedRoe(fishPondObject);
                                        }
                                    }
                                }

                                break;
                        }

                        if (item.HasContextTag("edible_mushroom"))
                        {
                            yield return objectDataDefinition.CreateFlavoredDriedMushroom(item as SObject);
                        }

                        yield return item;

                        break;

                    default:
                        yield return item;

                        break;
                }
            }
        }
    }

    // TODO: Method for caching all context tags

    /// <summary>Retrieves items based on the provided predicate.</summary>
    /// <param name="predicate">The predicate used to filter the items. If null, all items are returned.</param>
    /// <returns>An enumerable collection of Item objects.</returns>
    public static IEnumerable<Item> GetItems(Func<Item, bool>? predicate)
    {
        foreach (var item in ItemRepository.GetAll())
        {
            if (predicate is null || predicate(item))
            {
                yield return item;
            }

            if (item is not SObject obj
                || obj.bigCraftable.Value
                || item.QualifiedItemId == "(O)447"
                || item.QualifiedItemId == "(O)812")
            {
                continue;
            }

            // Add silver quality item
            obj = (SObject)item.getOne();
            obj.Quality = SObject.medQuality;
            if (predicate is null || predicate(obj))
            {
                yield return obj;
            }

            // Add gold quality item
            obj = (SObject)item.getOne();
            obj.Quality = SObject.highQuality;
            if (predicate is null || predicate(obj))
            {
                yield return obj;
            }

            // Add iridium quality item
            obj = (SObject)item.getOne();
            obj.Quality = SObject.bestQuality;
            if (predicate is null || predicate(obj))
            {
                yield return obj;
            }
        }
    }
}