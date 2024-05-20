namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Show stats to the side of a chest.</summary>
internal sealed class StorageInfo : BaseFeature<StorageInfo>
{
    private const string AlphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private static readonly Lazy<int> LineHeight =
        new(() => (int)Game1.smallFont.MeasureString(StorageInfo.AlphaNumeric).Y);

    private readonly PerScreen<Dictionary<StorageInfoItem, Info>> cachedInfo = new(() => []);
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<IStorageContainer?> currentContainer = new();
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly MenuHandler menuHandler;
    private readonly PerScreen<bool> resetCache = new(() => true);

    /// <summary>Initializes a new instance of the <see cref="StorageInfo" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public StorageInfo(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        IModConfig modConfig)
        : base(eventManager, modConfig)
    {
        this.containerFactory = containerFactory;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.StorageInfo != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<InventoryChangedEventArgs>(this.OnInventoryChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<InventoryChangedEventArgs>(this.OnInventoryChanged);
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (this.resetCache.Value || !this.cachedInfo.Value.Any() || !this.Config.Controls.ToggleInfo.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleInfo);
        this.isActive.Value = !this.isActive.Value;
        Mod.Log.Info("{0}: Toggled chest info to {1}", this.Id, this.isActive.Value);
    }

    private void OnInventoryChanged(InventoryChangedEventArgs e) => this.resetCache.Value = true;

    private void OnMenuChanged(MenuChangedEventArgs e) => this.resetCache.Value = true;

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        var container = this.menuHandler.Top.Container;

        // Check if active
        if (!this.Config.StorageInfoMenuItems.Any()
            || !this.isActive.Value
            || this.menuHandler.CurrentMenu is null
            || container?.StorageInfo != FeatureOption.Enabled)
        {
            return;
        }

        // Check if info needs to be refreshed
        if (this.resetCache.Value)
        {
            this.RefreshInfo(container);
            this.resetCache.Value = false;
        }

        // Check if there is anything to display
        var items = StorageInfoItemExtensions
            .GetValues()
            .Where(
                infoItem => this.Config.StorageInfoMenuItems.Contains(infoItem)
                    && this.cachedInfo.Value.ContainsKey(infoItem))
            .Select(infoItem => this.cachedInfo.Value.GetValueOrDefault(infoItem))
            .ToList();

        if (!items.Any())
        {
            return;
        }

        var x = this.menuHandler.CurrentMenu.xPositionOnScreen - (IClickableMenu.borderWidth / 2) - 384;
        var y = this.menuHandler.CurrentMenu.yPositionOnScreen;

        // Draw background
        Game1.drawDialogueBox(
            x - IClickableMenu.borderWidth,
            y - (IClickableMenu.borderWidth / 2) - IClickableMenu.spaceToClearTopBorder,
            384,
            (StorageInfo.LineHeight.Value * items.Count)
            + IClickableMenu.spaceToClearTopBorder
            + (IClickableMenu.borderWidth * 2),
            false,
            true);

        // Draw info
        foreach (var info in items)
        {
            // Draw Name
            Utility.drawTextWithShadow(
                e.SpriteBatch,
                info.Name,
                Game1.smallFont,
                new Vector2(x, y),
                Game1.textColor,
                1f,
                0.1f);

            // Draw Value
            if (info.TotalBounds.X <= 384 - IClickableMenu.borderWidth)
            {
                e.SpriteBatch.DrawString(
                    Game1.smallFont,
                    info.Value,
                    new Vector2(x + info.NameBounds.X, y),
                    Game1.textColor);

                y += StorageInfo.LineHeight.Value;
                continue;
            }

            y += StorageInfo.LineHeight.Value;
            e.SpriteBatch.DrawString(Game1.smallFont, info.Value, new Vector2(x, y), Game1.textColor);

            y += StorageInfo.LineHeight.Value;
        }
    }

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        // Check if active
        if (!Context.IsPlayerFree
            || !Game1.displayHUD
            || !this.Config.StorageInfoHoverItems.Any()
            || !this.containerFactory.TryGetOne(
                Game1.currentLocation,
                this.inputHelper.GetCursorPosition().Tile,
                out var container)
            || container.StorageInfoHover != FeatureOption.Enabled)
        {
            this.currentContainer.Value = null;
            return;
        }

        // Check if info needs to be refreshed
        if (this.resetCache.Value || this.currentContainer.Value != container)
        {
            this.currentContainer.Value = container;
            this.RefreshInfo(container);
            this.resetCache.Value = false;
        }

        // Check if there is anything to display
        if (!this.cachedInfo.Value.Any())
        {
            return;
        }

        var sb = new StringBuilder();
        var valueWidth = 0;
        var yOffset = 0;
        Info? infoIcon = null;
        var iconPos = Vector2.Zero;
        var (mouseX, mouseY) = Game1.getMousePosition(true);

        foreach (var infoItem in this.Config.StorageInfoHoverItems)
        {
            if (!this.Config.StorageInfoHoverItems.Contains(infoItem)
                || !this.cachedInfo.Value.TryGetValue(infoItem, out var info))
            {
                continue;
            }

            if (valueWidth > 0 && (infoIcon is null || !iconPos.Equals(Vector2.Zero)))
            {
                sb.Append('\n');
                yOffset += info.TotalBounds.Y;
            }

            if (infoItem is StorageInfoItem.Icon)
            {
                infoIcon = info;
                continue;
            }

            if (infoIcon is not null && iconPos.Equals(Vector2.Zero))
            {
                sb.Append(CultureInfo.InvariantCulture, $"     {info.Value}");
                iconPos = new Vector2(mouseX + 32, mouseY + yOffset + 32);
                valueWidth = Math.Max(valueWidth, info.ValueBounds.X + 44);
                continue;
            }

            sb.Append(info.Value);
            valueWidth = Math.Max(valueWidth, info.ValueBounds.X + 4);
        }

        // Draw info
        IClickableMenu.drawHoverText(e.SpriteBatch, sb.ToString(), Game1.smallFont, boxWidthOverride: valueWidth + 32);

        // Draw icon
        if (infoIcon.HasValue && this.iconRegistry.TryGetIcon(infoIcon.Value.Value, out var icon))
        {
            e.SpriteBatch.Draw(
                Game1.content.Load<Texture2D>(icon.Path),
                iconPos,
                icon.Area,
                Color.White,
                0,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);
        }
    }

    private void RefreshInfo(IStorageContainer container)
    {
        this.cachedInfo.Value.Clear();

        // Add name
        if (!string.IsNullOrWhiteSpace(container.StorageName))
        {
            this.cachedInfo.Value.TryAdd(
                StorageInfoItem.Name,
                new Info(I18n.StorageInfo_Name(), container.StorageName));
        }

        // Add icon
        if (!string.IsNullOrWhiteSpace(container.StorageIcon)
            && this.iconRegistry.TryGetIcon(container.StorageIcon, out _))
        {
            this.cachedInfo.Value.TryAdd(
                StorageInfoItem.Icon,
                new Info(I18n.StorageInfo_Icon(), container.StorageIcon));
        }

        // Add type
        this.cachedInfo.Value.TryAdd(StorageInfoItem.Type, new Info(I18n.StorageInfo_Type(), container.DisplayName));

        // Add location
        this.cachedInfo.Value.TryAdd(
            StorageInfoItem.Location,
            new Info(I18n.StorageInfo_Location(), container.Location.Name));

        // Add position
        this.cachedInfo.Value.TryAdd(
            StorageInfoItem.Position,
            new Info(I18n.StorageInfo_Position(), $"{(int)container.TileLocation.X}, {(int)container.TileLocation.Y}"));

        // Add inventory
        if (container.Parent is FarmerContainer farmerStorage)
        {
            this.cachedInfo.Value.TryAdd(
                StorageInfoItem.Inventory,
                new Info(I18n.StorageInfo_Inventory(), farmerStorage.Farmer.Name));
        }

        var items = container.Items.Where(item => item is not null).ToList();

        // Capacity
        var capacity = container.ResizeChestCapacity switch
        {
            -1 => I18n.Capacity_Unlimited_Name(),
            _ when container.Capacity == int.MaxValue => I18n.Capacity_Unlimited_Name(),
            _ => $"{container.Items.CountItemStacks()} / {I18n.Capacity_Other_Name(container.Capacity)}",
        };

        this.cachedInfo.Value.TryAdd(StorageInfoItem.Capacity, new Info(I18n.StorageInfo_Capacity(), capacity));

        // Total items
        var totalItems = items.Sum(item => item.Stack);
        this.cachedInfo.Value.TryAdd(
            StorageInfoItem.TotalItems,
            new Info(I18n.StorageInfo_TotalItems(), $"{totalItems:n0}"));

        // Unique items
        var uniqueItems = items.Select(item => item.QualifiedItemId).Distinct().Count();
        this.cachedInfo.Value.TryAdd(
            StorageInfoItem.UniqueItems,
            new Info(I18n.StorageInfo_UniqueItems(), $"{uniqueItems:n0}"));

        // Total value
        var totalValue =
            items.Select(item => (long)item.sellToStorePrice(Game1.player.UniqueMultiplayerID) * item.Stack).Sum();

        this.cachedInfo.Value.TryAdd(
            StorageInfoItem.TotalValue,
            new Info(I18n.StorageInfo_TotalValue(), $"{totalValue:n0}"));
    }

    private readonly struct Info(string name, string value)
    {
        public string Name { get; } = name;

        public string Value { get; } = value;

        public Point NameBounds { get; } = Game1.smallFont.MeasureString($"{name} ").ToPoint();

        public Point ValueBounds { get; } = Game1.smallFont.MeasureString($"{value} ").ToPoint();

        public Point TotalBounds { get; } = Game1.smallFont.MeasureString($"{name} {value}").ToPoint();
    }
}