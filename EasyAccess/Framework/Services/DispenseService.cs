namespace StardewMods.EasyAccess.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
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
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public DispenseService(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IInputHelper inputHelper,
        IModConfig modConfig,
        ToolbarIconsIntegration toolbarIconsIntegration)
    {
        // Init
        this.assetHandler = assetHandler;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;
        this.toolbarIconsIntegration = toolbarIconsIntegration;

        // Events
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
        eventManager.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
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

            Log.Info("Dispensed {0} into producer {1}.", Game1.player.CurrentItem.DisplayName, obj.DisplayName);
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

    private void OnGameLaunched(GameLaunchedEventArgs obj)
    {
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.UniqueId,
            this.assetHandler.Icon.Name.BaseName,
            new Rectangle(16, 0, 16, 16),
            I18n.Button_DispenseInputs_Name());

        this.toolbarIconsIntegration.Api.Subscribe(this.OnIconPressed);
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (e.Id == this.UniqueId)
        {
            this.DispenseItems();
        }
    }
}