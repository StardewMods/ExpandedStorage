namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Objects;

/// <summary>Unload a held chest's contents into another chest.</summary>
internal sealed class UnloadChest : BaseFeature<UnloadChest>
{
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;
    private readonly ProxyChestFactory proxyChestFactory;

    /// <summary>Initializes a new instance of the <see cref="UnloadChest" /> class.</summary>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    public UnloadChest(
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModEvents modEvents,
        ProxyChestFactory proxyChestFactory)
        : base(log, manifest, configManager)
    {
        this.containerFactory = containerFactory;
        this.containerHandler = containerHandler;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
        this.proxyChestFactory = proxyChestFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.UnloadChest != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate() => this.modEvents.Input.ButtonPressed += this.OnButtonPressed;

    /// <inheritdoc />
    protected override void Deactivate() => this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;

    [EventPriority(EventPriority.Normal + 10)]
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsUseToolButton()
            || this.inputHelper.IsSuppressed(e.Button)
            || !this.containerFactory.TryGetOneFromPlayer(Game1.player, out var containerFrom)
            || containerFrom.Options.UnloadChest != Option.Enabled)
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
            || !this.containerFactory.TryGetOneFromLocation(Game1.currentLocation, pos, out var containerTo)
            || containerTo.Options.UnloadChest != Option.Enabled)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);

        if (this.Config.UnloadChestSwap)
        {
            if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
                || obj is not Chest chest
                || !this.proxyChestFactory.TryCreateRequest(chest, out var request))
            {
                return;
            }

            Game1.currentLocation.Objects.Remove(pos);
            if (!Utility.tryToPlaceItem(
                Game1.currentLocation,
                Game1.player.ActiveObject,
                (int)pos.X * Game1.tileSize,
                (int)pos.Y * Game1.tileSize))
            {
                Game1.currentLocation.Objects.Add(pos, obj);
                request.Cancel();
                return;
            }

            Game1.player.ActiveObject = request.Item;
            request.Confirm();

            // Swap container from and to
            if (!this.containerFactory.TryGetOneFromLocation(Game1.currentLocation, pos, out containerTo)
                || !this.containerFactory.TryGetOneFromPlayer(Game1.player, out containerFrom))
            {
                return;
            }
        }

        if (!this.containerHandler.Transfer(containerFrom, containerTo, out var amounts, true))
        {
            return;
        }

        foreach (var (name, amount) in amounts)
        {
            if (amount > 0)
            {
                this.Log.Trace(
                    "{0}: {{ Item: {1}, Quantity: {2}, From: {3}, To: {4} }}",
                    [this.Id, name, amount, containerFrom, containerTo]);
            }
        }
    }
}