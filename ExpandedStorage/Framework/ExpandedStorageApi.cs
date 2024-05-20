namespace StardewMods.ExpandedStorage.Framework;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewMods.ExpandedStorage.Framework.Services.Factory;

/// <inheritdoc />
public sealed class ExpandedStorageApi : IExpandedStorageApi
{
    private readonly Mod.BaseEventManager eventManager;
    private readonly IModInfo modInfo;
    private readonly StorageDataFactory storageDataFactory;

    /// <summary>Initializes a new instance of the <see cref="ExpandedStorageApi" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="storageDataFactory">Dependency used for managing storage data.</param>
    internal ExpandedStorageApi(IEventManager eventManager, IModInfo modInfo, StorageDataFactory storageDataFactory)
    {
        // Init
        this.modInfo = modInfo;
        this.storageDataFactory = storageDataFactory;
        this.eventManager = new Mod.BaseEventManager();

        // Events
        eventManager.Subscribe<ChestCreatedEventArgs>(this.OnChestCreated);
    }

    /// <inheritdoc />
    public void Subscribe<TEventArgs>(Action<TEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc />
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData) =>
        this.storageDataFactory.TryGetData(item, out storageData);

    /// <inheritdoc />
    public void Unsubscribe<TEventArgs>(Action<TEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnChestCreated(ChestCreatedEventArgs e) =>
        this.eventManager.Publish<IChestCreated, ChestCreatedEventArgs>(e);
}