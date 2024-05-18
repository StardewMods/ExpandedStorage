namespace StardewMods.ExpandedStorage.Framework;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewMods.ExpandedStorage.Framework.Services;

/// <inheritdoc />
public sealed class ExpandedStorageApi : IExpandedStorageApi
{
    private readonly AssetHandler assetHandler;
    private readonly BaseEventManager eventManager;
    private readonly IModInfo modInfo;

    /// <summary>Initializes a new instance of the <see cref="ExpandedStorageApi" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="assetHandler">Dependency for managing expanded storage chests.</param>
    internal ExpandedStorageApi(IEventManager eventManager, IModInfo modInfo, AssetHandler assetHandler)
    {
        // Init
        this.modInfo = modInfo;
        this.assetHandler = assetHandler;
        this.eventManager = new BaseEventManager(modInfo.Manifest);

        // Events
        eventManager.Subscribe<ChestCreatedEventArgs>(this.OnChestCreated);
    }

    /// <inheritdoc />
    public void Subscribe<TEventArgs>(Action<TEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc />
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData) =>
        this.assetHandler.TryGetData(item, out storageData);

    /// <inheritdoc />
    public void Unsubscribe<TEventArgs>(Action<TEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnChestCreated(ChestCreatedEventArgs e) =>
        this.eventManager.Publish<IChestCreated, ChestCreatedEventArgs>(e);
}