namespace StardewMods.EasyAccess.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.EasyAccess.Framework.Interfaces;

/// <summary>Handles dispensing items.</summary>
internal sealed class DispenseService : BaseService<DispenseService>
{
    private readonly AssetHandler assetHandler;
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="DispenseService" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public DispenseService(
        AssetHandler assetHandler,
        IEventSubscriber eventSubscriber,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(log, manifest)
    {
        // Init
        this.assetHandler = assetHandler;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;
        this.toolbarIconsIntegration = toolbarIconsIntegration;

        // Events
        eventSubscriber.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
        eventSubscriber.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
    }

    private void OnGameLaunched(GameLaunchedEventArgs obj)
    {
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.UniqueId,
            this.assetHandler.IconTexture.Name.BaseName,
            new Rectangle(16, 0, 16, 16),
            I18n.Button_DispenseInputs_Name());

        this.toolbarIconsIntegration.Api.Subscribe(this.OnIconPressed);
    }

    private void DispenseItems()
    {
        if (Game1.player.CurrentItem is null)
        {
            return;
        }

        foreach (var pos in Game1.player.Tile.Box(this.modConfig.DispenseInputDistance))
        {
            if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
                || !obj.HasContextTag("machine_input")
                || !obj.AttemptAutoLoad(Game1.player.Items, Game1.player))
            {
                continue;
            }

            this.Log.Info("Dispensed {0} into producer {1}.", Game1.player.CurrentItem.DisplayName, obj.DisplayName);
        }
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.modConfig.ControlScheme.DispenseItems.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.modConfig.ControlScheme.DispenseItems);
        this.DispenseItems();
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (e.Id == this.UniqueId)
        {
            this.DispenseItems();
        }
    }
}