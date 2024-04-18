namespace StardewMods.BetterChests.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Buildings;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Provides access to all known storages for other services.</summary>
internal sealed class ContainerFactory : BaseService
{
    private readonly ConditionalWeakTable<object, IStorageContainer> cachedContainers = new();
    private readonly IModConfig modConfig;
    private readonly ProxyChestFactory proxyChestFactory;
    private readonly Dictionary<string, IStorageOptions> storageOptions = new();

    /// <summary>Initializes a new instance of the <see cref="ContainerFactory" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    public ContainerFactory(ILog log, IManifest manifest, IModConfig modConfig, ProxyChestFactory proxyChestFactory)
        : base(log, manifest)
    {
        this.modConfig = modConfig;
        this.proxyChestFactory = proxyChestFactory;
    }

    /// <summary>Retrieves the storage options for a given item.</summary>
    /// <param name="item">The item for which to retrieve the storage options.</param>
    /// <returns>The storage options for the given item.</returns>
    public IStorageOptions GetStorageOptions(Item item)
    {
        if (this.storageOptions.TryGetValue(item.QualifiedItemId, out var storageOption))
        {
            return storageOption;
        }

        storageOption = new BigCraftableStorageOptions(() => this.modConfig.DefaultOptions, GetData);
        this.storageOptions.Add(item.QualifiedItemId, storageOption);
        return storageOption;

        BigCraftableData GetData() =>
            Game1.bigCraftableData.TryGetValue(item.ItemId, out var data) ? data : new BigCraftableData();
    }

    /// <summary>Retrieves the storage options for a given location.</summary>
    /// <param name="location">The location for which to retrieve the storage options.</param>
    /// <returns>The storage options for the given item.</returns>
    public IStorageOptions GetStorageOptions(GameLocation location)
    {
        if (this.storageOptions.TryGetValue($"(L){location.Name}", out var storageOption))
        {
            return storageOption;
        }

        storageOption = new LocationStorageOptions(() => this.modConfig.DefaultOptions, GetData);
        this.storageOptions.Add($"(L){location.Name}", storageOption);
        return storageOption;

        LocationData GetData() =>
            DataLoader.Locations(Game1.content).TryGetValue(location.Name, out var data) ? data : new LocationData();
    }

    /// <summary>Retrieves the storage options for a given building.</summary>
    /// <param name="building">The building for which to retrieve the storage options.</param>
    /// <returns>The storage options for the given item.</returns>
    public IStorageOptions GetStorageOptions(Building building)
    {
        if (this.storageOptions.TryGetValue($"(B){building.buildingType.Value}", out var storageOption))
        {
            return storageOption;
        }

        storageOption = new BuildingStorageOptions(() => this.modConfig.DefaultOptions, GetData);
        this.storageOptions.Add($"(B){building.buildingType.Value}", storageOption);
        return storageOption;

        BuildingData GetData() =>
            Game1.buildingData.TryGetValue(building.buildingType.Value, out var data) ? data : new BuildingData();
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
            if (item is null || !this.TryGetAny(item, out var childContainer))
            {
                continue;
            }

            var container = new ChildContainer(parentContainer, childContainer);
            if (predicate is null || predicate(container))
            {
                yield return container;
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

        // Search for containers from placed objects
        foreach (var obj in location.Objects.Values)
        {
            if (!this.TryGetAny(obj, out container))
            {
                continue;
            }

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
            if (item is null || !this.TryGetAny(item, out var childContainer))
            {
                continue;
            }

            var container = new ChildContainer(farmerContainer, childContainer);
            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }
    }

    /// <summary>Tries to retrieve a container from the active menu.</summary>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne([NotNullWhen(true)] out IStorageContainer? container)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            container = null;
            return false;
        }

        switch (itemGrabMenu.context)
        {
            case Chest chest:
                if (chest == Game1.player.ActiveObject
                    && this.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out container))
                {
                    return true;
                }

                if (chest.Location is not null && this.TryGetOne(chest.Location, chest.TileLocation, out container))
                {
                    return true;
                }

                break;

            case SObject
            {
                heldObject.Value: Chest,
            } obj:
                if (obj.Location is not null && this.TryGetOne(obj.Location, obj.TileLocation, out container))
                {
                    return true;
                }

                break;

            case SObject obj:
                if (obj == Game1.player.ActiveObject
                    && this.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out container))
                {
                    return true;
                }

                break;

            case Building building:
                if (this.TryGetOne(building, out container))
                {
                    return true;
                }

                break;

            // Chests Anywhere
            case Farm farm:
                var shippingBin = farm.getBuildingByType("Shipping Bin");
                if (shippingBin is not null && this.TryGetOne(shippingBin, out container))
                {
                    return true;
                }

                break;
        }

        container = null;
        return false;
    }

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The player whose container will be retrieved.</param>
    /// <param name="index">The index of the player's inventory. Defaults to the active item.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
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

        if (!this.TryGetAny(item, out var childContainer) || !this.TryGetOne(farmer, out var farmerContainer))
        {
            container = null;
            return false;
        }

        container = new ChildContainer(farmerContainer, childContainer);
        this.cachedContainers.AddOrUpdate(item, container);
        return true;
    }

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The farmer to get a container from.</param>
    /// <param name="farmerContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? farmerContainer)
    {
        if (this.cachedContainers.TryGetValue(farmer, out farmerContainer))
        {
            return true;
        }

        var farmHouse = Utility.getHomeOfFarmer(farmer);
        var storageType = this.GetStorageOptions(farmHouse);
        farmerContainer = new FarmerContainer(storageType, farmer);
        this.cachedContainers.AddOrUpdate(farmer, farmerContainer);
        return true;
    }

    /// <summary>Tries to get a container from the specified building.</summary>
    /// <param name="building">The building to get a container from.</param>
    /// <param name="buildingContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Building building, [NotNullWhen(true)] out IStorageContainer? buildingContainer)
    {
        if (this.cachedContainers.TryGetValue(building, out buildingContainer))
        {
            return true;
        }

        var storageType = this.GetStorageOptions(building);
        if (building is ShippingBin shippingBin)
        {
            buildingContainer = new BuildingContainer(storageType, shippingBin);
            this.cachedContainers.AddOrUpdate(shippingBin, buildingContainer);
            return true;
        }

        var chest = building.GetBuildingChest("Output");
        if (chest is null)
        {
            return false;
        }

        if (this.cachedContainers.TryGetValue(chest, out buildingContainer))
        {
            return true;
        }

        buildingContainer = new BuildingContainer(storageType, building, chest);
        this.cachedContainers.AddOrUpdate(building, buildingContainer);
        this.cachedContainers.AddOrUpdate(chest, buildingContainer);
        return true;
    }

    /// <summary>Tries to get a container from the specified location and position.</summary>
    /// <param name="location">The location to get a container from.</param>
    /// <param name="pos">The position to get a the container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(GameLocation location, Vector2 pos, [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (pos.Equals(Vector2.Zero) && this.TryGetOne(location, out container))
        {
            return true;
        }

        // Container is a placed object
        if (location.Objects.TryGetValue(pos, out var obj) && this.TryGetAny(obj, out container))
        {
            return true;
        }

        container = null;
        return false;
    }

    /// <summary>Tries to get a container from the specified location.</summary>
    /// <param name="location">The location to get a container from.</param>
    /// <param name="locationContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(GameLocation location, [NotNullWhen(true)] out IStorageContainer? locationContainer)
    {
        if (this.cachedContainers.TryGetValue(location, out locationContainer))
        {
            return true;
        }

        if (location.GetFridge() is not
            { } fridge)
        {
            return false;
        }

        if (this.cachedContainers.TryGetValue(fridge, out locationContainer))
        {
            return true;
        }

        var storageType = this.GetStorageOptions(location);
        locationContainer = new FridgeContainer(storageType, location, fridge);
        this.cachedContainers.AddOrUpdate(fridge, locationContainer);
        this.cachedContainers.AddOrUpdate(location, locationContainer);
        return true;
    }

    /// <summary>Tries to get a container from the specified object.</summary>
    /// <param name="item">The item to get a container from.</param>
    /// <param name="itemContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Item item, [NotNullWhen(true)] out IStorageContainer? itemContainer)
    {
        if (this.cachedContainers.TryGetValue(item, out itemContainer))
        {
            return true;
        }

        itemContainer = this.GetAll(Predicate).FirstOrDefault();
        return itemContainer is not null;

        bool Predicate(IStorageContainer container) =>
            container switch
            {
                ChestContainer chestContainer => chestContainer.Chest == item,
                ObjectContainer objectContainer => objectContainer.Object.heldObject.Value == item,
                _ => false,
            };
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

    private IEnumerable<IStorageContainer> GetAllFromLocations(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        var foundLocations = new HashSet<GameLocation>();
        var locationQueue = new Queue<GameLocation>();

        foreach (var location in Game1.locations)
        {
            locationQueue.Enqueue(location);
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

    private bool TryGetAny(Item item, [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (this.cachedContainers.TryGetValue(item, out container))
        {
            return true;
        }

        if (item is Chest chest)
        {
            if (!chest.playerChest.Value)
            {
                container = null;
                return false;
            }
        }
        else if (item is SObject obj && obj.heldObject.Value is Chest objChest)
        {
            chest = objChest;
        }
        else if (this.proxyChestFactory.TryGetProxy(item, out var proxyChest))
        {
            chest = proxyChest;
        }
        else
        {
            container = null;
            return false;
        }

        var storageOption = this.GetStorageOptions(item);
        container = item switch
        {
            Chest => new ChestContainer(storageOption, chest),
            SObject obj => new ObjectContainer(storageOption, obj, chest),
            _ => new ChestContainer(storageOption, chest),
        };

        this.cachedContainers.AddOrUpdate(item, container);
        this.cachedContainers.AddOrUpdate(chest, container);
        return true;
    }
}