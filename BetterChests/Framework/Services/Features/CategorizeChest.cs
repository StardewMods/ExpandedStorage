namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class CategorizeChest : BaseFeature<CategorizeChest>
{
    private readonly PerScreen<List<Item>> cachedItems = new(() => []);
    private readonly GenericCacheTable<ISearchExpression?> cachedSearches;
    private readonly MenuHandler menuHandler;
    private readonly SearchHandler searchHandler;

    /// <summary>Initializes a new instance of the <see cref="CategorizeChest" /> class.</summary>
    /// <param name="cacheManager">Dependency used for managing cache tables.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    public CategorizeChest(
        CacheManager cacheManager,
        IEventManager eventManager,
        MenuHandler menuHandler,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        SearchHandler searchHandler)
        : base(eventManager, log, manifest, modConfig)
    {
        this.cachedSearches = cacheManager.GetCacheTable<ISearchExpression?>();
        this.menuHandler = menuHandler;
        this.searchHandler = searchHandler;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CategorizeChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Subscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Subscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Unsubscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
        this.Events.Unsubscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    private bool CanAcceptItem(IStorageContainer container, Item item, out bool accepted)
    {
        accepted = false;
        var includeStacks = container.CategorizeChestIncludeStacks == FeatureOption.Enabled;
        var hasStacks = container.Items.ContainsId(item.QualifiedItemId);
        if (includeStacks && hasStacks)
        {
            accepted = true;
            return true;
        }

        // Cannot handle if there is no search term
        if (string.IsNullOrWhiteSpace(container.CategorizeChestSearchTerm))
        {
            return false;
        }

        // Retrieve search expression from cache or generate a new one
        if (!this.cachedSearches.TryGetValue(container.CategorizeChestSearchTerm, out var searchExpression))
        {
            this.cachedSearches.AddOrUpdate(
                container.CategorizeChestSearchTerm,
                this.searchHandler.TryParseExpression(container.CategorizeChestSearchTerm, out searchExpression)
                    ? searchExpression
                    : null);
        }

        // Cannot handle if search term is invalid
        if (searchExpression is null)
        {
            return false;
        }

        // Check if item matches search expressions
        accepted = searchExpression.PartialMatch(item);
        return true;
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        var top = this.menuHandler.Top.Container;
        if (e.Container == this.menuHandler.Bottom.Container
            && top is
            {
                CategorizeChest: FeatureOption.Enabled,
                CategorizeChestBlockItems: FeatureOption.Enabled,
            })
        {
            if (this.CanAcceptItem(top, e.Item, out var accepted) && !accepted)
            {
                e.UnHighlight();
            }

            return;
        }

        // Unhighlight items not actually in container
        if (e.Container == top && !e.Container.Items.Contains(e.Item))
        {
            e.UnHighlight();
        }
    }

    [Priority(int.MinValue + 1)]
    private void OnItemsDisplaying(ItemsDisplayingEventArgs e)
    {
        // Append searched items to the end of the list
        if (e.Container == this.menuHandler.Top.Container
            && e.Container.CategorizeChest is FeatureOption.Enabled
            && this.cachedItems.Value.Any())
        {
            e.Edit(items => items.Concat(this.cachedItems.Value.Except(e.Container.Items)));
        }
    }

    [Priority(int.MinValue)]
    private void OnItemTransferring(ItemTransferringEventArgs e)
    {
        // Only test if categorize is enabled
        if (e.Into.CategorizeChest != FeatureOption.Enabled || !this.CanAcceptItem(e.Into, e.Item, out var accepted))
        {
            return;
        }

        if (accepted)
        {
            e.AllowTransfer();
        }
        else if (e.Into.CategorizeChestBlockItems == FeatureOption.Enabled)
        {
            e.PreventTransfer();
        }
    }

    private void OnSearchChanged(SearchChangedEventArgs e)
    {
        if (e.SearchExpression is null)
        {
            this.cachedItems.Value = [];
            return;
        }

        this.cachedItems.Value = [..ItemRepository.GetItems(e.SearchExpression.PartialMatch)];
    }
}