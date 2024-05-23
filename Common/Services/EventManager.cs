#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services;
#else
namespace StardewMods.Common.Services;
#endif

using StardewModdingAPI.Events;

/// <inheritdoc />
internal sealed class EventManager : BaseEventManager
{
    private readonly IModEvents? modEvents;

    /// <summary>Initializes a new instance of the <see cref="EventManager" /> class.</summary>
    /// <param name="modEvents">Dependency used for managing access to SMAPI events.</param>
    public EventManager(IModEvents modEvents) => this.modEvents = modEvents;

    /// <inheritdoc />
    public override void Subscribe<TEventArgs>(Action<TEventArgs> handler)
    {
        var eventType = typeof(TEventArgs);
        if (!this.Subscribers.ContainsKey(eventType))
        {
            this.AddSmapiEvent(eventType.Name);
        }

        base.Subscribe(handler);
    }

    /// <inheritdoc />
    public override void Unsubscribe<TEventArgs>(Action<TEventArgs> handler)
    {
        base.Unsubscribe(handler);
        var eventType = typeof(TEventArgs);
        if (!this.Subscribers.ContainsKey(eventType))
        {
            this.RemoveSmapiEvent(eventType.Name);
        }
    }

    private void AddSmapiEvent(string eventName)
    {
        if (this.modEvents is null)
        {
            return;
        }

        switch (eventName)
        {
            // Content
            case nameof(AssetRequestedEventArgs):
                this.modEvents.Content.AssetRequested += this.PublishEventLate;
                return;
            case nameof(AssetsInvalidatedEventArgs):
                this.modEvents.Content.AssetsInvalidated += this.PublishEventLate;
                return;
            case nameof(AssetReadyEventArgs):
                this.modEvents.Content.AssetReady += this.PublishEvent;
                return;
            case nameof(LocaleChangedEventArgs):
                this.modEvents.Content.LocaleChanged += this.PublishEventLate;
                return;

            // Display
            case nameof(MenuChangedEventArgs):
                this.modEvents.Display.MenuChanged += this.PublishEventLate;
                return;
            case nameof(RenderingEventArgs):
                this.modEvents.Display.Rendering += this.PublishEventEarly;
                return;
            case nameof(RenderedEventArgs):
                this.modEvents.Display.Rendered += this.PublishEventLate;
                return;
            case nameof(RenderingWorldEventArgs):
                this.modEvents.Display.RenderingWorld += this.PublishEventEarly;
                return;
            case nameof(RenderedWorldEventArgs):
                this.modEvents.Display.RenderedWorld += this.PublishEventLate;
                return;
            case nameof(RenderingActiveMenuEventArgs):
                this.modEvents.Display.RenderingActiveMenu += this.PublishEventEarly;
                return;
            case nameof(RenderedActiveMenuEventArgs):
                this.modEvents.Display.RenderedActiveMenu += this.PublishEventLate;
                return;
            case nameof(RenderingHudEventArgs):
                this.modEvents.Display.RenderingHud += this.PublishEventEarly;
                return;
            case nameof(RenderedHudEventArgs):
                this.modEvents.Display.RenderedHud += this.PublishEventLate;
                return;
            case nameof(WindowResizedEventArgs):
                this.modEvents.Display.WindowResized += this.PublishEventLate;
                return;

            // Game loop
            case nameof(GameLaunchedEventArgs):
                this.modEvents.GameLoop.GameLaunched += this.PublishEventLate;
                return;
            case nameof(UpdateTickedEventArgs):
                this.modEvents.GameLoop.UpdateTicked += this.PublishEventLate;
                return;
            case nameof(UpdateTickingEventArgs):
                this.modEvents.GameLoop.UpdateTicking += this.PublishEventEarly;
                return;
            case nameof(OneSecondUpdateTickedEventArgs):
                this.modEvents.GameLoop.OneSecondUpdateTicked += this.PublishEventLate;
                return;
            case nameof(OneSecondUpdateTickingEventArgs):
                this.modEvents.GameLoop.OneSecondUpdateTicking += this.PublishEventEarly;
                return;
            case nameof(SaveCreatedEventArgs):
                this.modEvents.GameLoop.SaveCreated += this.PublishEventLate;
                return;
            case nameof(SaveCreatingEventArgs):
                this.modEvents.GameLoop.SaveCreating += this.PublishEventEarly;
                return;
            case nameof(SavedEventArgs):
                this.modEvents.GameLoop.Saved += this.PublishEventLate;
                return;
            case nameof(SavingEventArgs):
                this.modEvents.GameLoop.Saving += this.PublishEventEarly;
                return;
            case nameof(SaveLoadedEventArgs):
                this.modEvents.GameLoop.SaveLoaded += this.PublishEventLate;
                return;
            case nameof(DayStartedEventArgs):
                this.modEvents.GameLoop.DayStarted += this.PublishEventLate;
                return;
            case nameof(DayEndingEventArgs):
                this.modEvents.GameLoop.DayEnding += this.PublishEventEarly;
                return;
            case nameof(TimeChangedEventArgs):
                this.modEvents.GameLoop.TimeChanged += this.PublishEventLate;
                return;
            case nameof(ReturnedToTitleEventArgs):
                this.modEvents.GameLoop.ReturnedToTitle += this.PublishEventLate;
                return;

            // Input
            case nameof(ButtonsChangedEventArgs):
                this.modEvents.Input.ButtonsChanged += this.PublishEventEarly;
                return;
            case nameof(ButtonPressedEventArgs):
                this.modEvents.Input.ButtonPressed += this.PublishEventEarly;
                return;
            case nameof(ButtonReleasedEventArgs):
                this.modEvents.Input.ButtonReleased += this.PublishEventEarly;
                return;
            case nameof(CursorMovedEventArgs):
                this.modEvents.Input.CursorMoved += this.PublishEventEarly;
                return;
            case nameof(MouseWheelScrolledEventArgs):
                this.modEvents.Input.MouseWheelScrolled += this.PublishEventEarly;
                return;

            // Multiplayer
            case nameof(PeerContextReceivedEventArgs):
                this.modEvents.Multiplayer.PeerContextReceived += this.PublishEventLate;
                return;
            case nameof(PeerConnectedEventArgs):
                this.modEvents.Multiplayer.PeerConnected += this.PublishEventLate;
                return;
            case nameof(ModMessageReceivedEventArgs):
                this.modEvents.Multiplayer.ModMessageReceived += this.PublishEventLate;
                return;
            case nameof(PeerDisconnectedEventArgs):
                this.modEvents.Multiplayer.PeerDisconnected += this.PublishEventLate;
                return;

            // Player
            case nameof(InventoryChangedEventArgs):
                this.modEvents.Player.InventoryChanged += this.PublishEventLate;
                return;
            case nameof(LevelChangedEventArgs):
                this.modEvents.Player.LevelChanged += this.PublishEventLate;
                return;
            case nameof(WarpedEventArgs):
                this.modEvents.Player.Warped += this.PublishEventLate;
                return;

            // World
            case nameof(LocationListChangedEventArgs):
                this.modEvents.World.LocationListChanged += this.PublishEventLate;
                return;
            case nameof(BuildingListChangedEventArgs):
                this.modEvents.World.BuildingListChanged += this.PublishEventLate;
                return;
            case nameof(ChestInventoryChangedEventArgs):
                this.modEvents.World.ChestInventoryChanged += this.PublishEventLate;
                return;
            case nameof(DebrisListChangedEventArgs):
                this.modEvents.World.DebrisListChanged += this.PublishEventLate;
                return;
            case nameof(FurnitureListChangedEventArgs):
                this.modEvents.World.FurnitureListChanged += this.PublishEventLate;
                return;
            case nameof(LargeTerrainFeatureListChangedEventArgs):
                this.modEvents.World.LargeTerrainFeatureListChanged += this.PublishEventLate;
                return;
            case nameof(NpcListChangedEventArgs):
                this.modEvents.World.NpcListChanged += this.PublishEventLate;
                return;
            case nameof(ObjectListChangedEventArgs):
                this.modEvents.World.ObjectListChanged += this.PublishEventLate;
                return;
            case nameof(TerrainFeatureListChangedEventArgs):
                this.modEvents.World.TerrainFeatureListChanged += this.PublishEventLate;
                return;
        }
    }

    private void PublishEvent<TEventArgs>(object? sender, TEventArgs eventArgs)
        where TEventArgs : EventArgs =>
        this.Publish(eventArgs);

    [EventPriority(EventPriority.High)]
    private void PublishEventEarly<TEventArgs>(object? sender, TEventArgs eventArgs)
        where TEventArgs : EventArgs =>
        this.Publish(eventArgs);

    [EventPriority(EventPriority.Low)]
    private void PublishEventLate<TEventArgs>(object? sender, TEventArgs eventArgs)
        where TEventArgs : EventArgs =>
        this.Publish(eventArgs);

    private void RemoveSmapiEvent(string eventName)
    {
        if (this.modEvents is null)
        {
            return;
        }

        switch (eventName)
        {
            // Content
            case nameof(AssetRequestedEventArgs):
                this.modEvents.Content.AssetRequested -= this.PublishEventLate;
                return;
            case nameof(AssetsInvalidatedEventArgs):
                this.modEvents.Content.AssetsInvalidated -= this.PublishEventLate;
                return;
            case nameof(AssetReadyEventArgs):
                this.modEvents.Content.AssetReady -= this.PublishEvent;
                return;
            case nameof(LocaleChangedEventArgs):
                this.modEvents.Content.LocaleChanged -= this.PublishEventLate;
                return;

            // Display
            case nameof(MenuChangedEventArgs):
                this.modEvents.Display.MenuChanged -= this.PublishEventLate;
                return;
            case nameof(RenderingEventArgs):
                this.modEvents.Display.Rendering -= this.PublishEventEarly;
                return;
            case nameof(RenderedEventArgs):
                this.modEvents.Display.Rendered -= this.PublishEventLate;
                return;
            case nameof(RenderingWorldEventArgs):
                this.modEvents.Display.RenderingWorld -= this.PublishEventEarly;
                return;
            case nameof(RenderedWorldEventArgs):
                this.modEvents.Display.RenderedWorld -= this.PublishEventLate;
                return;
            case nameof(RenderingActiveMenuEventArgs):
                this.modEvents.Display.RenderingActiveMenu -= this.PublishEventEarly;
                return;
            case nameof(RenderedActiveMenuEventArgs):
                this.modEvents.Display.RenderedActiveMenu -= this.PublishEventLate;
                return;
            case nameof(RenderingHudEventArgs):
                this.modEvents.Display.RenderingHud -= this.PublishEventEarly;
                return;
            case nameof(RenderedHudEventArgs):
                this.modEvents.Display.RenderedHud -= this.PublishEventLate;
                return;
            case nameof(WindowResizedEventArgs):
                this.modEvents.Display.WindowResized -= this.PublishEventLate;
                return;

            // Game loop
            case nameof(GameLaunchedEventArgs):
                this.modEvents.GameLoop.GameLaunched -= this.PublishEventLate;
                return;
            case nameof(UpdateTickedEventArgs):
                this.modEvents.GameLoop.UpdateTicked -= this.PublishEventLate;
                return;
            case nameof(UpdateTickingEventArgs):
                this.modEvents.GameLoop.UpdateTicking -= this.PublishEventEarly;
                return;
            case nameof(OneSecondUpdateTickedEventArgs):
                this.modEvents.GameLoop.OneSecondUpdateTicked -= this.PublishEventLate;
                return;
            case nameof(OneSecondUpdateTickingEventArgs):
                this.modEvents.GameLoop.OneSecondUpdateTicking -= this.PublishEventEarly;
                return;
            case nameof(SaveCreatedEventArgs):
                this.modEvents.GameLoop.SaveCreated -= this.PublishEventLate;
                return;
            case nameof(SaveCreatingEventArgs):
                this.modEvents.GameLoop.SaveCreating -= this.PublishEventEarly;
                return;
            case nameof(SavedEventArgs):
                this.modEvents.GameLoop.Saved -= this.PublishEventLate;
                return;
            case nameof(SavingEventArgs):
                this.modEvents.GameLoop.Saving -= this.PublishEventEarly;
                return;
            case nameof(SaveLoadedEventArgs):
                this.modEvents.GameLoop.SaveLoaded -= this.PublishEventLate;
                return;
            case nameof(DayStartedEventArgs):
                this.modEvents.GameLoop.DayStarted -= this.PublishEventLate;
                return;
            case nameof(DayEndingEventArgs):
                this.modEvents.GameLoop.DayEnding -= this.PublishEventEarly;
                return;
            case nameof(TimeChangedEventArgs):
                this.modEvents.GameLoop.TimeChanged -= this.PublishEventLate;
                return;
            case nameof(ReturnedToTitleEventArgs):
                this.modEvents.GameLoop.ReturnedToTitle -= this.PublishEventLate;
                return;

            // Input
            case nameof(ButtonsChangedEventArgs):
                this.modEvents.Input.ButtonsChanged -= this.PublishEventEarly;
                return;
            case nameof(ButtonPressedEventArgs):
                this.modEvents.Input.ButtonPressed -= this.PublishEventEarly;
                return;
            case nameof(ButtonReleasedEventArgs):
                this.modEvents.Input.ButtonReleased -= this.PublishEventEarly;
                return;
            case nameof(CursorMovedEventArgs):
                this.modEvents.Input.CursorMoved -= this.PublishEventEarly;
                return;
            case nameof(MouseWheelScrolledEventArgs):
                this.modEvents.Input.MouseWheelScrolled -= this.PublishEventEarly;
                return;

            // Multiplayer
            case nameof(PeerContextReceivedEventArgs):
                this.modEvents.Multiplayer.PeerContextReceived -= this.PublishEventLate;
                return;
            case nameof(PeerConnectedEventArgs):
                this.modEvents.Multiplayer.PeerConnected -= this.PublishEventLate;
                return;
            case nameof(ModMessageReceivedEventArgs):
                this.modEvents.Multiplayer.ModMessageReceived -= this.PublishEventLate;
                return;
            case nameof(PeerDisconnectedEventArgs):
                this.modEvents.Multiplayer.PeerDisconnected -= this.PublishEventLate;
                return;

            // Player
            case nameof(InventoryChangedEventArgs):
                this.modEvents.Player.InventoryChanged -= this.PublishEventLate;
                return;
            case nameof(LevelChangedEventArgs):
                this.modEvents.Player.LevelChanged -= this.PublishEventLate;
                return;
            case nameof(WarpedEventArgs):
                this.modEvents.Player.Warped -= this.PublishEventLate;
                return;

            // World
            case nameof(LocationListChangedEventArgs):
                this.modEvents.World.LocationListChanged -= this.PublishEventLate;
                return;
            case nameof(BuildingListChangedEventArgs):
                this.modEvents.World.BuildingListChanged -= this.PublishEventLate;
                return;
            case nameof(ChestInventoryChangedEventArgs):
                this.modEvents.World.ChestInventoryChanged -= this.PublishEventLate;
                return;
            case nameof(DebrisListChangedEventArgs):
                this.modEvents.World.DebrisListChanged -= this.PublishEventLate;
                return;
            case nameof(FurnitureListChangedEventArgs):
                this.modEvents.World.FurnitureListChanged -= this.PublishEventLate;
                return;
            case nameof(LargeTerrainFeatureListChangedEventArgs):
                this.modEvents.World.LargeTerrainFeatureListChanged -= this.PublishEventLate;
                return;
            case nameof(NpcListChangedEventArgs):
                this.modEvents.World.NpcListChanged -= this.PublishEventLate;
                return;
            case nameof(ObjectListChangedEventArgs):
                this.modEvents.World.ObjectListChanged -= this.PublishEventLate;
                return;
            case nameof(TerrainFeatureListChangedEventArgs):
                this.modEvents.World.TerrainFeatureListChanged -= this.PublishEventLate;
                return;
        }
    }
}