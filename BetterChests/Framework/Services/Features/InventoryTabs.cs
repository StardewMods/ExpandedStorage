namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Adds inventory tabs to the side of the <see cref="ItemGrabMenu" />.</summary>
internal sealed class InventoryTabs : BaseFeature<InventoryTabs>
{
    private readonly AssetHandler assetHandler;
    private readonly IExpressionHandler expressionHandler;
    private readonly MenuHandler menuHandler;
    private readonly PerScreen<List<TabIcon>> tabs = new(() => []);

    /// <summary>Initializes a new instance of the <see cref="InventoryTabs" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public InventoryTabs(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        ILog log,
        IManifest manifest,
        MenuHandler menuHandler,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.assetHandler = assetHandler;
        this.expressionHandler = expressionHandler;
        this.menuHandler = menuHandler;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.InventoryTabs != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() =>
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);

    /// <inheritdoc />
    protected override void Deactivate() =>
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);

    private void OnClicked(object? sender, TabData tabData)
    {
        this.Log.Trace("{0}: Switching tab to {1}.", this.Id, tabData.Label);
        this.expressionHandler.TryParseExpression(tabData.SearchTerm, out var expression);
        this.Events.Publish(new SearchChangedEventArgs(tabData.SearchTerm, expression));
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        var top = this.menuHandler.Top;
        this.tabs.Value.Clear();

        if (this.menuHandler.CurrentMenu is not ItemGrabMenu
            || top.InventoryMenu is null
            || container is not
            {
                InventoryTabs: FeatureOption.Enabled,
                SearchItems: FeatureOption.Enabled,
            })
        {
            return;
        }

        var x = Math.Min(
                top.InventoryMenu.xPositionOnScreen,
                this.menuHandler.Bottom.InventoryMenu?.xPositionOnScreen ?? int.MaxValue)
            - Game1.tileSize
            - IClickableMenu.borderWidth;

        var y = top.InventoryMenu.inventory[0].bounds.Y;

        foreach (var tabData in this.Config.InventoryTabList)
        {
            if (!this.assetHandler.Icons.TryGetValue(tabData.Icon, out var icon))
            {
                continue;
            }

            var tabIcon = new TabIcon(x, y, icon, tabData);
            tabIcon.Clicked += this.OnClicked;
            e.AddComponent(tabIcon);

            y += Game1.tileSize;
        }
    }
}