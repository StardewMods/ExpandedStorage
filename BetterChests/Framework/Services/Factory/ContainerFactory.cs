namespace StardewMods.BetterChests.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Models.Data;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

/// <summary>Provides access to all known storages for other services.</summary>
internal sealed class ContainerFactory
{
    private readonly ConditionalWeakTable<object, IStorageContainer> cachedContainers = new();
    private readonly IModConfig modConfig;
    private readonly ProxyChestFactory proxyChestFactory;
    private readonly Dictionary<string, IStorageOptions> storageOptions = new();

    /// <summary>Initializes a new instance of the <see cref="ContainerFactory" /> class.</summary>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    public ContainerFactory(IModConfig modConfig, ProxyChestFactory proxyChestFactory)
    {
        this.modConfig = modConfig;
        this.proxyChestFactory = proxyChestFactory;
    }

    /// <summary>Retrieves all containers that match the optional predicate.</summary>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAll(Func<IStorageContainer, bool>? predicate = default)
    {
        var foundContainers = new HashSet<IStorageContainer>();
        var containerQueue = new Queue<IStorageContainer>();

        foreach (var container in this.GetAllFromPlayers(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }

        foreach (var container in this.GetAllFromLocations(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }

        foreach (var container in this.GetAllFromContainers(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }
    }

    /// <summary>Retrieves all containers from the specified container that match the optional predicate.</summary>
    /// <param name="parentContainer">The container where the container items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAll(
        IStorageContainer parentContainer,
        Func<IStorageContainer, bool>? predicate = default)
    {
        foreach (var item in parentContainer.Items)
        {
            if (item is not SObject @object || !this.TryGetAny(@object, out var childContainer))
            {
                continue;
            }

            childContainer.Parent = parentContainer;
            if (predicate is null || predicate(childContainer))
            {
                yield return childContainer;
            }
        }
    }

    /// <summary>Retrieves all containers from the specified game location that match the optional predicate.</summary>
    /// <param name="location">The game location where the container will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAll(
        GameLocation location,
        Func<IStorageContainer, bool>? predicate = default)
    {
        // Get container for fridge in location
        if (this.TryGetOne(location, out var container))
        {
            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from buildings
        foreach (var building in location.buildings)
        {
            if (!this.TryGetOne(building, out container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from NPCs
        foreach (var npc in location.characters.OfType<NPC>())
        {
            if (!this.TryGetOne(npc, out container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from placed objects
        foreach (var (pos, obj) in location.Objects.Pairs)
        {
            if (pos.X <= 0 || pos.Y <= 0 || !this.TryGetAny(obj, out container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from placed furniture
        foreach (var furniture in location.furniture.OfType<StorageFurniture>())
        {
            if (!this.TryGetAny(furniture, out container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }
    }

    /// <summary>Retrieves all container items from the specified player matching the optional predicate.</summary>
    /// <param name="farmer">The player whose container items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAll(Farmer farmer, Func<IStorageContainer, bool>? predicate = default)
    {
        // Get container from farmer backpack
        if (!this.TryGetOne(farmer, out var farmerContainer))
        {
            yield break;
        }

        if (predicate is not null && predicate(farmerContainer))
        {
            yield return farmerContainer;
        }

        // Search for containers from farmer inventory
        foreach (var item in farmer.Items)
        {
            if (item is not SObject @object || !this.TryGetAny(@object, out var childContainer))
            {
                continue;
            }

            childContainer.Parent = farmerContainer;
            if (predicate is null || predicate(childContainer))
            {
                yield return childContainer;
            }
        }
    }

    /// <summary>Tries to retrieve a container from the active menu.</summary>
    /// <param name="menu">The menu to retrieve a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(IClickableMenu? menu, [NotNullWhen(true)] out IStorageContainer? container)
    {
        // Get the actual menu
        var actualMenu = menu switch
        {
            { } menuWithChild when menuWithChild.GetChildMenu() is
                { } childMenu => childMenu,
            GameMenu gameMenu => gameMenu.GetCurrentPage(),
            not null => menu,
            _ => Game1.activeClickableMenu,
        };

        // Get a default actual inventory for the menu
        var actualInventory = actualMenu switch
        {
            InventoryMenu inventoryMenu => inventoryMenu.actualInventory,
            ItemGrabMenu
            {
                ItemsToGrabMenu:
                { } itemsToGrabMenu,
                showReceivingMenu: true,
            } => itemsToGrabMenu.actualInventory,
            InventoryPage
            {
                inventory:
                { } inventory,
            } => inventory.actualInventory,
            ShopMenu
            {
                source: StorageFurniture furniture,
            } => furniture.heldItems,
            ShopMenu
            {
                inventory:
                { } inventory,
            } => inventory.actualInventory,
            _ => null,
        };

        // Get the context for the menu
        var context = Game1.activeClickableMenu switch
        {
            ItemGrabMenu itemGrabMenu => itemGrabMenu.context,
            ShopMenu shopMenu => shopMenu.source,
            GameMenu => Game1.player,
            _ => null,
        };

        // Find container based on the context
        container = context switch
        {
            // The same storage container
            IStorageContainer contextContainer when actualInventory is null
                || actualInventory.Equals(contextContainer.Items) => contextContainer,

            // Farmer backpack container
            not null when actualInventory is not null
                && actualInventory.Equals(Game1.player.Items)
                && this.TryGetOne(Game1.player, out var farmerContainer) => farmerContainer,

            // Placed chest container
            Chest chest when actualInventory is not null
                && actualInventory.Equals(chest.GetItemsForPlayer())
                && chest.Location is not null
                && this.TryGetOne(chest.Location, chest.TileLocation, out var placedContainer) => placedContainer,

            // Held chest container
            Chest chest when chest == Game1.player.ActiveObject
                && actualInventory is not null
                && this.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var heldContainer)
                && actualInventory.Equals(heldContainer.Items) => heldContainer,

            // Placed object container
            SObject
                {
                    heldObject.Value: Chest heldChest,
                } @object when actualInventory is not null
                && actualInventory.Equals(heldChest.GetItemsForPlayer())
                && @object.Location is not null
                && this.TryGetOne(@object.Location, @object.TileLocation, out var objectContainer) => objectContainer,

            // Proxy container
            SObject @object when @object == Game1.player.ActiveObject
                && actualInventory is not null
                && this.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var proxyContainer)
                && actualInventory.Equals(proxyContainer.Items) => proxyContainer,

            // Building container
            Building building when this.TryGetOne(building, out var buildingContainer)
                && (actualInventory is null || actualInventory.Equals(buildingContainer.Items)) => buildingContainer,

            // Storage furniture container
            StorageFurniture furniture when actualInventory is not null
                && furniture.heldItems.Equals(actualInventory)
                && this.TryGetOne(furniture, out var furnitureContainer) => furnitureContainer,

            // Return the shipping bin (Chests Anywhere)
            Farm farm when this.TryGetOne(farm.getBuildingByType("Shipping Bin"), out var shippingContainer)
                && (actualInventory is null || actualInventory.Equals(shippingContainer.Items)) => shippingContainer,

            // Return the saddle bag (Horse Overhaul)
            Stable stable when actualInventory is not null
                && this.TryGetOne(stable.getStableHorse(), out var stableContainer)
                && actualInventory.Equals(stableContainer.Items) => stableContainer,
            Horse horse when actualInventory is not null
                && this.TryGetOne(horse, out var horseContainer)
                && actualInventory.Equals(horseContainer.Items) => horseContainer,

            // Unknown or unsupported
            _ => null,
        };

        return container is not null;
    }

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The player whose container will be retrieved.</param>
    /// <param name="index">The index of the player's inventory. Defaults to the active item.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Farmer farmer, int index, [NotNullWhen(true)] out IStorageContainer? container)
    {
        var item = farmer.Items.ElementAtOrDefault(index);
        if (item is null)
        {
            container = null;
            return false;
        }

        if (this.cachedContainers.TryGetValue(item, out container))
        {
            return true;
        }

        if (item is not SObject @object
            || !this.TryGetAny(@object, out container)
            || !this.TryGetOne(farmer, out var farmerContainer))
        {
            container = null;
            return false;
        }

        container.Parent = farmerContainer;
        this.cachedContainers.AddOrUpdate(item, container);
        return true;
    }

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The farmer to get a container from.</param>
    /// <param name="farmerContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? farmerContainer)
    {
        if (this.cachedContainers.TryGetValue(farmer, out farmerContainer))
        {
            return true;
        }

        // Create the farmer container
        farmerContainer = new FarmerContainer(farmer);

        // Add type options for the backpack
        if (this.modConfig.StorageOptions.GetValueOrDefault("Characters")?.GetValueOrDefault("Farmer") is not null)
        {
            var backpackOptions = new ConfigStorageOptions(
                () => this.modConfig.StorageOptions["Characters"]["Farmer"],
                I18n.Storage_Backpack_Tooltip,
                I18n.Storage_Backpack_Name);

            farmerContainer.AddOptions(StorageOption.Type, backpackOptions);
        }

        // Add mod options for the farmer
        var farmerModel = new ModDataModel(farmer.modData);
        var farmerOptions = new DictionaryStorageOptions(farmerModel);
        farmerContainer.AddOptions(StorageOption.Individual, farmerOptions);

        // Cache container to instance
        this.cachedContainers.AddOrUpdate(farmer, farmerContainer);
        return true;
    }

    /// <summary>Tries to get a container from the specified building.</summary>
    /// <param name="building">The building to get a container from.</param>
    /// <param name="buildingContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Building building, [NotNullWhen(true)] out IStorageContainer? buildingContainer)
    {
        if (this.cachedContainers.TryGetValue(building, out buildingContainer))
        {
            return true;
        }

        // Create the building container
        switch (building)
        {
            case ShippingBin shippingBin:
                buildingContainer = new BuildingContainer(shippingBin);
                break;

            case not null when building.GetBuildingChest("Output") is
                { } chest:
                // Check if output chest has already been cached
                if (this.cachedContainers.TryGetValue(chest, out buildingContainer))
                {
                    this.cachedContainers.AddOrUpdate(building, buildingContainer);
                    return true;
                }

                buildingContainer = new BuildingContainer(building, chest);

                // Cache building chest to the building storage container
                this.cachedContainers.AddOrUpdate(chest, buildingContainer);
                break;

            default: return false;
        }

        // Add global options to the building container
        buildingContainer.AddOptions(StorageOption.Global, this.modConfig.DefaultOptions);

        // Add mod options to the building container
        var buildingModel = new ModDataModel(building.modData);
        var buildingOptions = new DictionaryStorageOptions(buildingModel);
        buildingContainer.AddOptions(StorageOption.Individual, buildingOptions);

        // Add type options to the building container
        if (!this.storageOptions.TryGetValue($"(B){building.buildingType.Value}", out var typeOptions))
        {
            var typeModel = new DictionaryModel(GetCustomFields);
            typeOptions = new DictionaryStorageOptions(typeModel, GetDescription, GetDisplayName);
            this.storageOptions.Add($"(B){building.buildingType.Value}", typeOptions);
        }

        buildingContainer.AddOptions(StorageOption.Type, typeOptions);

        // Cache container to instance
        this.cachedContainers.AddOrUpdate(building, buildingContainer);
        return true;

        Dictionary<string, string>? GetCustomFields() =>
            Game1.buildingData.TryGetValue(building.buildingType.Value, out var buildingData)
                ? buildingData.CustomFields
                : null;

        string GetDescription() =>
            Game1.buildingData.TryGetValue(building.buildingType.Value, out var buildingData)
                ? TokenParser.ParseText(buildingData.Description)
                : I18n.Storage_Other_Name();

        string GetDisplayName() =>
            Game1.buildingData.TryGetValue(building.buildingType.Value, out var buildingData)
                ? TokenParser.ParseText(buildingData.Name)
                : I18n.Storage_Other_Tooltip();
    }

    /// <summary>Tries to get a container from the specified NPC.</summary>
    /// <param name="npc">The NPC to get a container from.</param>
    /// <param name="npcContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(NPC npc, [NotNullWhen(true)] out IStorageContainer? npcContainer)
    {
        if (this.cachedContainers.TryGetValue(npc, out npcContainer))
        {
            return true;
        }

        switch (npc)
        {
            // Horse Overhaul
            case Horse horse:
                var stable = horse.TryFindStable();
                if (stable is null
                    || !stable.modData.TryGetValue("Goldenrevolver.HorseOverhaul/stableID", out var stableData)
                    || string.IsNullOrWhiteSpace(stableData)
                    || !int.TryParse(stableData, out var stableId)
                    || !Game1.getFarm().Objects.TryGetValue(new Vector2(stableId, 0), out var saddleBag)
                    || saddleBag is not Chest saddleBagChest)
                {
                    return false;
                }

                // Check if saddle bag chest has already been cached
                if (this.cachedContainers.TryGetValue(saddleBagChest, out npcContainer))
                {
                    this.cachedContainers.AddOrUpdate(npc, npcContainer);
                    return true;
                }

                // Create the horse container
                npcContainer = new NpcContainer(horse, saddleBagChest);

                // Add global options to the horse container
                npcContainer.AddOptions(StorageOption.Global, this.modConfig.DefaultOptions);

                // Add mod options to the horse container
                var npcModel = new ModDataModel(horse.modData);
                var npcOptions = new DictionaryStorageOptions(npcModel);
                npcContainer.AddOptions(StorageOption.Individual, npcOptions);

                // Add type options to the horse container
                if (!this.storageOptions.TryGetValue($"(B){stable.buildingType.Value}", out var typeOptions))
                {
                    var typeModel = new DictionaryModel(GetCustomFields);
                    typeOptions = new DictionaryStorageOptions(
                        typeModel,
                        I18n.Storage_Saddlebag_Tooltip,
                        I18n.Storage_Saddlebag_Name);

                    this.storageOptions.Add($"(B){stable.buildingType.Value}", typeOptions);
                }

                npcContainer.AddOptions(StorageOption.Type, typeOptions);

                // Cache container to instance
                this.cachedContainers.AddOrUpdate(npc, npcContainer);
                this.cachedContainers.AddOrUpdate(saddleBagChest, npcContainer);
                return true;

                Dictionary<string, string>? GetCustomFields() =>
                    Game1.buildingData.TryGetValue(stable.buildingType.Value, out var buildingData)
                        ? buildingData.CustomFields
                        : null;

            default: return false;
        }
    }

    /// <summary>Tries to get a container from the specified location and position.</summary>
    /// <param name="location">The location to get a container from.</param>
    /// <param name="pos">The position to get a the container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(GameLocation location, Vector2 pos, [NotNullWhen(true)] out IStorageContainer? container)
    {
        // Try getting fridge container
        if (pos.Equals(Vector2.Zero) && this.TryGetOne(location, out container))
        {
            return true;
        }

        // Create intersection tile
        var bounds = new Rectangle(
            (int)(pos.X * Game1.tileSize),
            (int)(pos.Y * Game1.tileSize),
            Game1.tileSize,
            Game1.tileSize);

        // Get container if it's a placed object
        foreach (var @object in location.Objects.Values)
        {
            if (@object.GetBoundingBox().Intersects(bounds) && this.TryGetAny(@object, out container))
            {
                return true;
            }
        }

        // Get container if it's a building
        foreach (var building in location.buildings)
        {
            if (building.GetBoundingBox().Intersects(bounds) && this.TryGetOne(building, out container))
            {
                return true;
            }
        }

        // Get container if it's a storage furniture
        foreach (var furniture in location.furniture.OfType<StorageFurniture>())
        {
            if (furniture.IntersectsForCollision(bounds) && this.TryGetAny(furniture, out container))
            {
                return true;
            }
        }

        // Get container if it's an npc
        foreach (var npc in location.characters.OfType<NPC>())
        {
            if (npc.GetBoundingBox().Intersects(bounds) && this.TryGetOne(npc, out container))
            {
                return true;
            }
        }

        container = null;
        return false;
    }

    /// <summary>Tries to get a container from the specified location.</summary>
    /// <param name="location">The location to get a container from.</param>
    /// <param name="locationContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(GameLocation location, [NotNullWhen(true)] out IStorageContainer? locationContainer)
    {
        if (this.cachedContainers.TryGetValue(location, out locationContainer))
        {
            return true;
        }

        // Only return containers for locations that have a fridge
        if (location.GetFridge() is not
            { } fridge)
        {
            return false;
        }

        // Check if fridge has already been cached
        if (this.cachedContainers.TryGetValue(fridge, out locationContainer))
        {
            this.cachedContainers.AddOrUpdate(location, locationContainer);
            return true;
        }

        // Create the fridge container
        locationContainer = new FridgeContainer(location, fridge);

        // Add global options to the fridge container
        locationContainer.AddOptions(StorageOption.Global, this.modConfig.DefaultOptions);

        // Add mod options to the fridge container
        var buildingModel = new ModDataModel(location.modData);
        var buildingOptions = new DictionaryStorageOptions(buildingModel);
        locationContainer.AddOptions(StorageOption.Individual, buildingOptions);

        // Add type options to the fridge container
        if (!this.storageOptions.TryGetValue($"(L){location.Name}", out var typeOptions))
        {
            var typeModel = new DictionaryModel(GetCustomFields);
            typeOptions = new DictionaryStorageOptions(
                typeModel,
                I18n.Storage_Fridge_Tooltip,
                I18n.Storage_Fridge_Name);

            this.storageOptions.Add($"(L){location.Name}", typeOptions);
        }

        locationContainer.AddOptions(StorageOption.Type, typeOptions);

        // Cache container to instance
        this.cachedContainers.AddOrUpdate(fridge, locationContainer);
        this.cachedContainers.AddOrUpdate(location, locationContainer);
        return true;

        Dictionary<string, string>? GetCustomFields() =>
            DataLoader.Locations(Game1.content).GetValueOrDefault(location.Name)?.CustomFields;
    }

    /// <summary>Tries to get a container from the specified object.</summary>
    /// <param name="item">The item to get a container from.</param>
    /// <param name="itemContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Item item, [NotNullWhen(true)] out IStorageContainer? itemContainer)
    {
        if (this.cachedContainers.TryGetValue(item, out itemContainer))
        {
            return true;
        }

        switch (item)
        {
            // Try finding container based on location
            case SObject
            {
                Location: not null,
            } @object when this.TryGetOne(@object.Location, @object.TileLocation, out itemContainer):
                this.cachedContainers.AddOrUpdate(item, itemContainer);
                return true;

            // Do an exhaustive search for the container
            case not null when this.GetAll(Predicate).FirstOrDefault() is
                { } foundContainer:
                itemContainer = foundContainer;
                return true;

            default: return false;
        }

        bool Predicate(IStorageContainer container) =>
            container switch
            {
                ChestContainer chestContainer => chestContainer.Chest == item,
                ObjectContainer objectContainer => objectContainer.Object.heldObject.Value == item,
                _ => false,
            };
    }

    private IEnumerable<IStorageContainer> GetAllFromContainers(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        while (containerQueue.TryDequeue(out var container))
        {
            foreach (var childContainer in this.GetAll(container, predicate))
            {
                if (!foundContainers.Add(childContainer))
                {
                    continue;
                }

                containerQueue.Enqueue(childContainer);
                yield return childContainer;
            }
        }
    }

    private IEnumerable<IStorageContainer> GetAllFromLocations(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        var foundLocations = new HashSet<GameLocation>();
        var locationQueue = new Queue<GameLocation>();

        locationQueue.Enqueue(Game1.currentLocation);

        foreach (var location in Game1.locations)
        {
            if (!location.Equals(Game1.currentLocation))
            {
                locationQueue.Enqueue(location);
            }
        }

        while (locationQueue.TryDequeue(out var location))
        {
            if (!foundLocations.Add(location))
            {
                continue;
            }

            foreach (var container in this.GetAll(location, predicate))
            {
                if (!foundContainers.Add(container))
                {
                    continue;
                }

                containerQueue.Enqueue(container);
                yield return container;
            }

            foreach (var building in location.buildings)
            {
                if (building.GetIndoorsType() == IndoorsType.Instanced)
                {
                    locationQueue.Enqueue(building.GetIndoors());
                }
            }
        }
    }

    private IEnumerable<IStorageContainer> GetAllFromPlayers(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        foreach (var farmer in Game1.getAllFarmers())
        {
            foreach (var container in this.GetAll(farmer, predicate))
            {
                if (!foundContainers.Add(container))
                {
                    continue;
                }

                containerQueue.Enqueue(container);
                yield return container;
            }
        }
    }

    private bool TryGetAny(SObject @object, [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (this.cachedContainers.TryGetValue(@object, out container))
        {
            return true;
        }

        switch (@object)
        {
            case Chest
            {
                playerChest.Value: true,
            } chest:
                // Create the chest container
                container = new ChestContainer(chest);
                break;

            case
            {
                heldObject.Value: Chest chestObject,
            }:
                // Check if object chest has already been cached
                if (this.cachedContainers.TryGetValue(chestObject, out container))
                {
                    this.cachedContainers.AddOrUpdate(@object, container);
                    return true;
                }

                // Create the object container
                container = new ObjectContainer(@object, chestObject);

                // Cache chest object to the container
                this.cachedContainers.AddOrUpdate(@object, container);
                break;

            case not null when this.proxyChestFactory.TryGetProxy(@object, out var proxyChest):
                // Check if proxy chest has already been cached
                if (this.cachedContainers.TryGetValue(proxyChest, out container))
                {
                    this.cachedContainers.AddOrUpdate(@object, container);
                    return true;
                }

                // Create the chest container
                container = new ChestContainer(proxyChest);

                // Cache proxy chest to the container
                this.cachedContainers.AddOrUpdate(proxyChest, container);
                break;

            case StorageFurniture furniture:
                // Create the furniture container
                container = new FurnitureContainer(furniture);

                // Add global options to the furniture container
                container.AddOptions(StorageOption.Global, this.modConfig.DefaultOptions);

                // Add mod options to the furniture container
                var furnitureModel = new ModDataModel(furniture.modData);
                var furnitureOptions = new DictionaryStorageOptions(furnitureModel);
                container.AddOptions(StorageOption.Individual, furnitureOptions);

                // Add type options if furniture exists in config
                if (this.modConfig.StorageOptions.GetValueOrDefault("Furniture")?.GetValueOrDefault(@object.ItemId) is
                    not null)
                {
                    var furnitureTypeOptions = new ConfigStorageOptions(
                        () => this.modConfig.StorageOptions["Furniture"][@object.ItemId],
                        () => TokenParser.ParseText(
                            ItemRegistry.RequireTypeDefinition("(F)").GetData(@object.ItemId).Description),
                        () => TokenParser.ParseText(
                            ItemRegistry.RequireTypeDefinition("(F)").GetData(@object.ItemId).DisplayName));

                    container.AddOptions(StorageOption.Type, furnitureTypeOptions);
                }

                // Cache container to instance
                this.cachedContainers.AddOrUpdate(@object, container);
                return true;

            default: return false;
        }

        // Add global options to the object container
        container.AddOptions(StorageOption.Global, this.modConfig.DefaultOptions);

        // Add mod options to the object container
        var objectModel = new ModDataModel(@object.modData);
        var objectOptions = new DictionaryStorageOptions(objectModel);
        container.AddOptions(StorageOption.Individual, objectOptions);

        // Add type options to the object container
        if (!this.storageOptions.TryGetValue(@object.QualifiedItemId, out var typeOptions))
        {
            var typeModel = new DictionaryModel(GetCustomFields);
            typeOptions = new DictionaryStorageOptions(typeModel, GetDescription, GetDisplayName);
            this.storageOptions.Add(@object.QualifiedItemId, typeOptions);
        }

        container.AddOptions(StorageOption.Type, typeOptions);

        // Cache container to instance
        this.cachedContainers.AddOrUpdate(@object, container);

        // Initialize fridge if cook from chest is enabled
        if (@object is Chest storageChest)
        {
            storageChest.fridge.Value = typeOptions.CookFromChest is not RangeOption.Disabled;
        }

        return true;

        Dictionary<string, string>? GetCustomFields() =>
            Game1.bigCraftableData.TryGetValue(@object.ItemId, out var bigCraftableData)
                ? bigCraftableData.CustomFields
                : null;

        string GetDescription() =>
            Game1.bigCraftableData.TryGetValue(@object.ItemId, out var bigCraftableData)
                ? TokenParser.ParseText(bigCraftableData.Description)
                : I18n.Storage_Other_Name();

        string GetDisplayName() =>
            Game1.bigCraftableData.TryGetValue(@object.ItemId, out var bigCraftableData)
                ? TokenParser.ParseText(bigCraftableData.Name)
                : I18n.Storage_Other_Tooltip();
    }
}