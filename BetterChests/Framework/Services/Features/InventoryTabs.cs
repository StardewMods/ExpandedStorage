namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
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
    private readonly ExpressionHandler expressionHandler;
    private readonly IInputHelper inputHelper;
    private readonly MenuHandler menuHandler;
    private readonly PerScreen<List<TabIcon>> tabs = new(() => []);

    /// <summary>Initializes a new instance of the <see cref="InventoryTabs" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public InventoryTabs(
        AssetHandler assetHandler,
        IEventManager eventManager,
        ExpressionHandler expressionHandler,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        MenuHandler menuHandler,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.assetHandler = assetHandler;
        this.expressionHandler = expressionHandler;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.InventoryTabs != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        if (container is not
            {
                InventoryTabs: FeatureOption.Enabled,
                SearchItems: FeatureOption.Enabled,
            }
            || !this.tabs.Value.Any()
            || this.menuHandler.CurrentMenu is not ItemGrabMenu
            || !this.menuHandler.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA:
                if (this.tabs.Value.Any(tab => tab.LeftClick(mouseX, mouseY)))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                return;

            case SButton.MouseRight or SButton.ControllerB:
                if (this.tabs.Value.Any(tab => tab.RightClick(mouseX, mouseY)))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                return;
        }
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        var top = this.menuHandler.Top;
        this.tabs.Value.Clear();

        if (this.menuHandler.CurrentMenu is not ItemGrabMenu itemGrabMenu
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

        foreach (var inventoryTab in this.Config.InventoryTabList)
        {
            if (!this.assetHandler.Icons.TryGetValue(inventoryTab.Icon, out var icon))
            {
                continue;
            }

            this.tabs.Value.Add(
                new TabIcon(
                    x,
                    y,
                    icon,
                    inventoryTab,
                    () =>
                    {
                        this.Log.Trace("{0}: Switching tab to {1}.", this.Id, inventoryTab.Label);
                        this.expressionHandler.SearchText = inventoryTab.SearchTerm;
                        this.expressionHandler.SearchExpression =
                            this.expressionHandler.TryParseExpression(inventoryTab.SearchTerm, out var expression)
                                ? expression
                                : null;

                        this.Events.Publish(new SearchChangedEventArgs(this.expressionHandler.SearchExpression));
                    }));

            y += Game1.tileSize;
        }
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (!this.tabs.Value.Any() || this.menuHandler.CurrentMenu is not ItemGrabMenu)
        {
            return;
        }

        foreach (var tab in this.tabs.Value)
        {
            tab.Draw(e.SpriteBatch);
        }
    }

    private void OnRenderingActiveMenu(RenderingActiveMenuEventArgs e)
    {
        if (!this.tabs.Value.Any() || this.menuHandler.CurrentMenu is not ItemGrabMenu)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        foreach (var tab in this.tabs.Value)
        {
            tab.Update(mouseX, mouseY);
        }
    }
}