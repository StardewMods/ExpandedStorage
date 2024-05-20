namespace StardewMods.EasyAccess.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.EasyAccess.Framework.Enums;
using StardewMods.EasyAccess.Framework.Interfaces;

/// <summary>Handles collecting items.</summary>
internal sealed class CollectService : Mod.BaseService<CollectService>
{
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="CollectService" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public CollectService(
        IEventManager eventManager,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IModConfig modConfig,
        ToolbarIconsIntegration toolbarIconsIntegration)
    {
        // Init
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;
        this.toolbarIconsIntegration = toolbarIconsIntegration;

        // Events
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
        eventManager.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
    }

    private void CollectItems()
    {
        foreach (var pos in Game1.player.Tile.Box(this.modConfig.CollectOutputDistance))
        {
            if (Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
            {
                // Dig Spot
                if (this.modConfig.DoDigSpots && obj.ParentSheetIndex == 590)
                {
                    Game1.currentLocation.digUpArtifactSpot((int)pos.X, (int)pos.Y, Game1.player);
                    if (!Game1.currentLocation.terrainFeatures.ContainsKey(pos))
                    {
                        Game1.currentLocation.makeHoeDirt(pos, true);
                    }

                    Game1.currentLocation.Objects.Remove(pos);
                    continue;
                }

                // Big Craftables
                if (this.modConfig.DoForage && obj.IsSpawnedObject && obj.isForage())
                {
                    // Vanilla Logic
                    var r = new Random(
                        ((int)Game1.uniqueIDForThisGame / 2)
                        + (int)Game1.stats.DaysPlayed
                        + (int)pos.X
                        + ((int)pos.Y * 777));

                    if (Game1.player.professions.Contains(16))
                    {
                        obj.Quality = 4;
                    }
                    else if (r.NextDouble() < Game1.player.ForagingLevel / 30f)
                    {
                        obj.Quality = 2;
                    }
                    else if (r.NextDouble() < Game1.player.ForagingLevel / 15f)
                    {
                        obj.Quality = 1;
                    }

                    ++Game1.stats.ItemsForaged;
                    if (Game1.currentLocation.isFarmBuildingInterior())
                    {
                        Game1.player.gainExperience(0, 5);
                    }
                    else
                    {
                        Game1.player.gainExperience(2, 7);
                    }

                    var direction = pos.Y < Game1.player.Tile.Y
                        ? 0
                        : pos.X > Game1.player.Tile.X
                            ? 1
                            : pos.Y > Game1.player.Tile.Y
                                ? 2
                                : pos.X < Game1.player.Tile.X
                                    ? 3
                                    : -1;

                    Game1.createItemDebris(obj, Game1.tileSize * pos, direction, Game1.currentLocation);
                    Game1.currentLocation.Objects.Remove(pos);
                    Mod.Log.Info("Dropped {0} from forage.", obj.DisplayName);
                    continue;
                }

                if (this.modConfig.DoMachines)
                {
                    var item = obj.heldObject.Value;
                    if (item is not null && obj.checkForAction(Game1.player))
                    {
                        Mod.Log.Info("Collected {0} from producer {1}.", item.DisplayName, obj.DisplayName);
                    }
                }
            }

            if (!this.modConfig.DoTerrain)
            {
                continue;
            }

            // Terrain Features
            if (Game1.currentLocation.terrainFeatures.TryGetValue(pos, out var terrainFeature))
            {
                terrainFeature.performUseAction(pos);
            }

            // Large Terrain Features
            terrainFeature = Game1.currentLocation.getLargeTerrainFeatureAt((int)pos.X, (int)pos.Y);
            terrainFeature?.performUseAction(pos);
        }
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.modConfig.ControlScheme.CollectItems.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.modConfig.ControlScheme.CollectItems);
        this.CollectItems();
    }

    private void OnGameLaunched(GameLaunchedEventArgs obj)
    {
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.iconRegistry.RequireIcon(InternalIcon.Collect),
            I18n.Button_CollectOutputs_Name());

        this.toolbarIconsIntegration.Api.Subscribe(this.OnIconPressed);
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (e.Id == this.iconRegistry.RequireIcon(InternalIcon.Collect).Id)
        {
            this.CollectItems();
        }
    }
}