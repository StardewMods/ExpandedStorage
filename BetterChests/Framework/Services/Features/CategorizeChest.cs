namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Runtime.CompilerServices;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class CategorizeChest : BaseFeature<CategorizeChest>
{
    private readonly ConditionalWeakTable<IStorageContainer, ItemMatcher> cachedItemMatchers = new();
    private readonly ContainerFactory containerFactory;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly ItemMatcherFactory itemMatcherFactory;

    /// <summary>Initializes a new instance of the <see cref="CategorizeChest" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public CategorizeChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        ItemGrabMenuManager itemGrabMenuManager,
        ItemMatcherFactory itemMatcherFactory,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.containerFactory = containerFactory;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.itemMatcherFactory = itemMatcherFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CategorizeChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ChestInventoryChangedEventArgs>(this.OnChestInventoryChanged);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Subscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ChestInventoryChangedEventArgs>(this.OnChestInventoryChanged);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Unsubscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
    }

    private static Func<IEnumerable<Item>, IEnumerable<Item>> FilterByCategory(
        IStorageContainer container,
        IItemFilter itemMatcher)
    {
        return InternalFilterMethod;

        IEnumerable<Item> InternalFilterMethod(IEnumerable<Item> items) =>
            container.Options.CategorizeChestMethod switch
            {
                FilterMethod.Sorted or FilterMethod.GrayedOut => items.OrderByDescending(itemMatcher.MatchesFilter),
                FilterMethod.Hidden => items.Where(itemMatcher.MatchesFilter),
                _ => items,
            };
    }

    private void OnChestInventoryChanged(ChestInventoryChangedEventArgs e)
    {
        if (!this.containerFactory.TryGetOneFromLocation(e.Location, e.Chest.TileLocation, out var container)
            || container.Options.CategorizeChestAutomatically != FeatureOption.Enabled)
        {
            return;
        }

        var tags = new HashSet<string>(container.Options.CategorizeChestTags);
        foreach (var item in e.Added)
        {
            var tag = item
                .GetContextTags()
                .Where(tag => tag.StartsWith("id_", StringComparison.OrdinalIgnoreCase))
                .MinBy(tag => tag.Contains('('));

            if (tag is not null)
            {
                tags.Add(tag);
            }
        }

        if (!tags.SetEquals(container.Options.CategorizeChestTags))
        {
            container.Options.CategorizeChestTags = [..tags];
        }
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        var top = this.itemGrabMenuManager.Top.Container;
        if (top?.Options.CategorizeChest == FeatureOption.Enabled)
        {
            var itemMatcher = this.GetOrCreateItemMatcher(top);
            if (top.Options.CategorizeChestMethod is FilterMethod.GrayedOut)
            {
                this.itemGrabMenuManager.Bottom.AddHighlightMethod(itemMatcher.MatchesFilter);
            }

            this.itemGrabMenuManager.Bottom.AddOperation(CategorizeChest.FilterByCategory(top, itemMatcher));
        }

        var bottom = this.itemGrabMenuManager.Bottom.Container;
        if (bottom?.Options.CategorizeChest == FeatureOption.Enabled)
        {
            var itemMatcher = this.GetOrCreateItemMatcher(bottom);
            if (bottom.Options.CategorizeChestMethod is FilterMethod.GrayedOut)
            {
                this.itemGrabMenuManager.Top.AddHighlightMethod(itemMatcher.MatchesFilter);
            }

            this.itemGrabMenuManager.Top.AddOperation(CategorizeChest.FilterByCategory(bottom, itemMatcher));
        }
    }

    private void OnItemTransferring(ItemTransferringEventArgs e)
    {
        // Only test if categorize is enabled
        if (e.Into.Options.CategorizeChest != FeatureOption.Enabled)
        {
            return;
        }

        // Allow transfer if item does matches category
        var itemMatcher = this.GetOrCreateItemMatcher(e.Into);
        if (!itemMatcher.IsEmpty && itemMatcher.MatchesFilter(e.Item))
        {
            e.AllowTransfer();
        }
    }

    private ItemMatcher GetOrCreateItemMatcher(IStorageContainer container)
    {
        if (!this.cachedItemMatchers.TryGetValue(container, out var itemMatcher))
        {
            itemMatcher = this.itemMatcherFactory.GetDefault();
        }

        itemMatcher.SearchText = container.Options.CategorizeChestTags.Any()
            ? string.Join(' ', container.Options.CategorizeChestTags)
            : string.Empty;

        this.cachedItemMatchers.AddOrUpdate(container, itemMatcher);
        return itemMatcher;
    }
}