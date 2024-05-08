namespace StardewMods.BetterChests.Framework.Models.Containers;

using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc cref="IStorageContainer{TSource}" />
internal abstract class BaseContainer<TSource> : IStorageContainer<TSource>
    where TSource : class
{
    private readonly SortedList<StorageOption, Func<IStorageOptions>> storageOptions = [];

    private WeakReference<IStorageContainer?>? parent;

    /// <summary>Initializes a new instance of the <see cref="BaseContainer{TSource}" /> class.</summary>
    /// <param name="source">The source of the container.</param>
    protected BaseContainer(TSource source) => this.Source = new WeakReference<TSource>(source);

    /// <inheritdoc />
    public IStorageOptions ActualOptions =>
        this.storageOptions.TryGetValue(StorageOption.Individual, out var getOptions) ? getOptions() : this;

    /// <inheritdoc />
    public abstract int Capacity { get; }

    /// <inheritdoc />
    public virtual string Description
    {
        get
        {
            if (this.storageOptions.TryGetValue(StorageOption.Type, out var storageOption)
                || this.storageOptions.TryGetValue(StorageOption.Individual, out storageOption)
                || this.storageOptions.TryGetValue(StorageOption.Global, out storageOption))
            {
                return storageOption().Description;
            }

            return string.Empty;
        }
    }

    /// <inheritdoc />
    public virtual string DisplayName
    {
        get
        {
            if (this.storageOptions.TryGetValue(StorageOption.Type, out var storageOption)
                || this.storageOptions.TryGetValue(StorageOption.Individual, out storageOption)
                || this.storageOptions.TryGetValue(StorageOption.Global, out storageOption))
            {
                return storageOption().DisplayName;
            }

            return string.Empty;
        }
    }

    /// <inheritdoc />
    public abstract bool IsAlive { get; }

    /// <inheritdoc />
    public abstract IInventory Items { get; }

    /// <inheritdoc />
    public abstract GameLocation Location { get; }

    /// <inheritdoc />
    public abstract ModDataDictionary ModData { get; }

    /// <inheritdoc />
    public abstract NetMutex? Mutex { get; }

    /// <inheritdoc />
    public WeakReference<TSource> Source { get; }

    /// <inheritdoc />
    public abstract Vector2 TileLocation { get; }

    /// <inheritdoc />
    public RangeOption AccessChest
    {
        get => this.Get(options => options.AccessChest);
        set => this.Set(options => options.AccessChest, (options, newValue) => options.AccessChest = newValue, value);
    }

    /// <inheritdoc />
    public int AccessChestPriority
    {
        get => this.Get(options => options.AccessChestPriority);
        set =>
            this.Set(
                options => options.AccessChestPriority,
                (options, newValue) => options.AccessChestPriority = newValue,
                value);
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(options => options.AutoOrganize);
        set => this.Set(options => options.AutoOrganize, (options, newValue) => options.AutoOrganize = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(options => options.CarryChest);
        set => this.Set(options => options.CarryChest, (options, newValue) => options.CarryChest = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(options => options.CategorizeChest);
        set =>
            this.Set(
                options => options.CategorizeChest,
                (options, newValue) => options.CategorizeChest = newValue,
                value);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems
    {
        get => this.Get(options => options.CategorizeChestBlockItems);
        set =>
            this.Set(
                options => options.CategorizeChestBlockItems,
                (options, newValue) => options.CategorizeChestBlockItems = newValue,
                value);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks
    {
        get => this.Get(options => options.CategorizeChestIncludeStacks);
        set =>
            this.Set(
                options => options.CategorizeChestIncludeStacks,
                (options, newValue) => options.CategorizeChestIncludeStacks = newValue,
                value);
    }

    /// <inheritdoc />
    public string CategorizeChestSearchTerm
    {
        get => this.Get(options => options.CategorizeChestSearchTerm);
        set =>
            this.Set(
                options => options.CategorizeChestSearchTerm,
                (options, newValue) => options.CategorizeChestSearchTerm = newValue,
                value);
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(options => options.ChestFinder);
        set => this.Set(options => options.ChestFinder, (options, newValue) => options.ChestFinder = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(options => options.CollectItems);
        set => this.Set(options => options.CollectItems, (options, newValue) => options.CollectItems = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(options => options.ConfigureChest);
        set =>
            this.Set(
                options => options.ConfigureChest,
                (options, newValue) => options.ConfigureChest = newValue,
                value);
    }

    /// <inheritdoc />
    public RangeOption CookFromChest
    {
        get => this.Get(options => options.CookFromChest);
        set =>
            this.Set(options => options.CookFromChest, (options, newValue) => options.CookFromChest = newValue, value);
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(options => options.CraftFromChest);
        set =>
            this.Set(
                options => options.CraftFromChest,
                (options, newValue) => options.CraftFromChest = newValue,
                value);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.Get(options => options.CraftFromChestDistance);
        set =>
            this.Set(
                options => options.CraftFromChestDistance,
                (options, newValue) => options.CraftFromChestDistance = newValue,
                value);
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(options => options.HslColorPicker);
        set =>
            this.Set(
                options => options.HslColorPicker,
                (options, newValue) => options.HslColorPicker = newValue,
                value);
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(options => options.InventoryTabs);
        set =>
            this.Set(options => options.InventoryTabs, (options, newValue) => options.InventoryTabs = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(options => options.OpenHeldChest);
        set =>
            this.Set(options => options.OpenHeldChest, (options, newValue) => options.OpenHeldChest = newValue, value);
    }

    /// <inheritdoc />
    public IStorageContainer? Parent
    {
        get => this.parent?.TryGetTarget(out var target) == true ? target : null;
        set => this.parent = new WeakReference<IStorageContainer?>(value);
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get => this.Get(options => options.ResizeChest);
        set => this.Set(options => options.ResizeChest, (options, newValue) => options.ResizeChest = newValue, value);
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.Get(options => options.ResizeChestCapacity);
        set =>
            this.Set(
                options => options.ResizeChestCapacity,
                (options, newValue) => options.ResizeChestCapacity = newValue,
                value);
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(options => options.SearchItems);
        set => this.Set(options => options.SearchItems, (options, newValue) => options.SearchItems = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.Get(options => options.ShopFromChest);
        set =>
            this.Set(options => options.ShopFromChest, (options, newValue) => options.ShopFromChest = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption SortInventory
    {
        get => this.Get(options => options.SortInventory);
        set =>
            this.Set(options => options.SortInventory, (options, newValue) => options.SortInventory = newValue, value);
    }

    /// <inheritdoc />
    public string SortInventoryBy
    {
        get => this.Get(options => options.SortInventoryBy);
        set =>
            this.Set(
                options => options.SortInventoryBy,
                (options, newValue) => options.SortInventoryBy = newValue,
                value);
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(options => options.StashToChest);
        set => this.Set(options => options.StashToChest, (options, newValue) => options.StashToChest = newValue, value);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.Get(options => options.StashToChestDistance);
        set =>
            this.Set(
                options => options.StashToChestDistance,
                (options, newValue) => options.StashToChestDistance = newValue,
                value);
    }

    /// <inheritdoc />
    public StashPriority StashToChestPriority
    {
        get => this.Get(options => options.StashToChestPriority);
        set =>
            this.Set(
                options => options.StashToChestPriority,
                (options, newValue) => options.StashToChestPriority = newValue,
                value);
    }

    /// <inheritdoc />
    public string StorageIcon
    {
        get => this.Get(options => options.StorageIcon);
        set => this.Set(options => options.StorageIcon, (options, newValue) => options.StorageIcon = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption StorageInfo
    {
        get => this.Get(storage => storage.StorageInfo);
        set => this.Set(options => options.StorageInfo, (options, newValue) => options.StorageInfo = newValue, value);
    }

    /// <inheritdoc />
    public FeatureOption StorageInfoHover
    {
        get => this.Get(storage => storage.StorageInfoHover);
        set =>
            this.Set(
                options => options.StorageInfoHover,
                (options, newValue) => options.StorageInfoHover = newValue,
                value);
    }

    /// <inheritdoc />
    public string StorageName
    {
        get => this.Get(storage => storage.StorageName);
        set => this.Set(options => options.StorageName, (options, newValue) => options.StorageName = newValue, value);
    }

    /// <inheritdoc />
    public void AddOptions(StorageOption storageOption, Func<IStorageOptions> getOptions) =>
        this.storageOptions.Add(storageOption, getOptions);

    /// <inheritdoc />
    public void ForEachItem(Func<Item, bool> action)
    {
        for (var index = this.Items.Count - 1; index >= 0; --index)
        {
            if (this.Items[index] is null)
            {
                continue;
            }

            if (!action(this.Items[index]))
            {
                break;
            }
        }
    }

    /// <inheritdoc />
    public IStorageOptions GetParentOptions()
    {
        var currentOptions = new DefaultStorageOptions();
        foreach (var storageOption in StorageOptionExtensions.GetValues().Except(new[] { StorageOption.Individual }))
        {
            if (!this.storageOptions.TryGetValue(storageOption, out var getOptions))
            {
                continue;
            }

            var parentOptions = getOptions();
            parentOptions.ForEachOption(
                (name, option) =>
                {
                    switch (option)
                    {
                        case FeatureOption parentValue and not FeatureOption.Default
                            when !currentOptions.TryGetOption(name, out FeatureOption currentValue)
                            || currentValue is FeatureOption.Default:
                            currentOptions.SetOption(name, parentValue);
                            return;

                        case RangeOption parentValue and not RangeOption.Default
                            when !currentOptions.TryGetOption(name, out RangeOption currentValue)
                            || currentValue is RangeOption.Default:
                            currentOptions.SetOption(name, parentValue);
                            return;

                        case ChestMenuOption parentValue and not ChestMenuOption.Default
                            when !currentOptions.TryGetOption(name, out ChestMenuOption currentValue)
                            || currentValue is ChestMenuOption.Default:
                            currentOptions.SetOption(name, parentValue);
                            return;

                        case StashPriority parentValue and not StashPriority.Default
                            when !currentOptions.TryGetOption(name, out StashPriority currentValue)
                            || currentValue is StashPriority.Default:
                            currentOptions.SetOption(name, parentValue);
                            return;

                        case string parentValue when !string.IsNullOrWhiteSpace(parentValue)
                            && (!currentOptions.TryGetOption(name, out string currentValue)
                                || string.IsNullOrWhiteSpace(currentValue)):
                            currentOptions.SetOption(name, parentValue);
                            return;

                        case int parentValue and not 0 when !currentOptions.TryGetOption(name, out int currentValue)
                            || currentValue == 0:
                            currentOptions.SetOption(name, parentValue);
                            return;
                    }
                });
        }

        return currentOptions;
    }

    /// <inheritdoc />
    public virtual void GrabItemFromChest(Item? item, Farmer who)
    {
        if (item is null || !who.couldInventoryAcceptThisItem(item))
        {
            return;
        }

        this.Items.Remove(item);
        this.Items.RemoveEmptySlots();
        this.ShowMenu();
    }

    /// <inheritdoc />
    public virtual void GrabItemFromInventory(Item? item, Farmer who)
    {
        if (item is null)
        {
            return;
        }

        if (item.Stack == 0)
        {
            item.Stack = 1;
        }

        if (!this.TryAdd(item, out var remaining))
        {
            return;
        }

        if (remaining == null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            remaining = who.addItemToInventory(remaining);
        }

        this.ShowMenu();
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        itemGrabMenu.heldItem = remaining;
    }

    /// <inheritdoc />
    public virtual bool HighlightItems(Item? item) => InventoryMenu.highlightAllItems(item);

    /// <inheritdoc />
    public virtual void ShowMenu(bool playSound = false)
    {
        var oldID = Game1.activeClickableMenu?.currentlySnappedComponent?.myID ?? -1;
        Game1.activeClickableMenu = this.GetItemGrabMenu(
            playSound,
            context: this.Source.TryGetTarget(out var target) ? target : null);

        if (oldID == -1)
        {
            return;
        }

        Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
        Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(this.StorageName))
        {
            return this.StorageName.Trim();
        }

        var sb = new StringBuilder();

        sb.Append(this.DisplayName);
        sb.Append(" at ");
        sb.Append(this.Location?.DisplayName ?? "Unknown");
        sb.Append(CultureInfo.InvariantCulture, $"({this.TileLocation.X:n0}, {this.TileLocation.Y:n0})");
        return sb.ToString();
    }

    /// <inheritdoc />
    public abstract bool TryAdd(Item item, out Item? remaining);

    /// <inheritdoc />
    public abstract bool TryRemove(Item item);

    /// <summary>Opens an item grab menu for this container.</summary>
    /// <param name="playSound">Whether to play the container open sound.</param>
    /// <param name="reverseGrab">Indicates if an item can be held rather than placed back into the chest.</param>
    /// <param name="showReceivingMenu">Indicates whether the top menu is displayed.</param>
    /// <param name="snapToBottom">Indicates whether the menu will be moved to the bottom of the screen.</param>
    /// <param name="canBeExitedWithKey">Indicates whether the menu can be exited with the menu key.</param>
    /// <param name="playRightClickSound">Indicates whether sound can be played on right-click.</param>
    /// <param name="allowRightClick">Indicates whether right-click can be used for interactions other than tool attachments.</param>
    /// <param name="showOrganizeButton">Indicates whether the organize button will be shown.</param>
    /// <param name="source">
    /// Indicates the source of the <see cref="ItemGrabMenu" />. (0 - none, 1 - chest, 2 - gift, 3 -
    /// fishing chest, 4 - overflow).
    /// </param>
    /// <param name="sourceItem">The source item of the <see cref="ItemGrabMenu" />.</param>
    /// <param name="whichSpecialButton">Indicates whether the Junimo toggle button will be shown.</param>
    /// <param name="context">The context of the <see cref="ItemGrabMenu" />.</param>
    /// <returns>The <see cref="ItemGrabMenu" />.</returns>
    protected virtual ItemGrabMenu GetItemGrabMenu(
        bool playSound = false,
        bool reverseGrab = false,
        bool showReceivingMenu = true,
        bool snapToBottom = false,
        bool canBeExitedWithKey = true,
        bool playRightClickSound = true,
        bool allowRightClick = true,
        bool showOrganizeButton = true,
        int source = ItemGrabMenu.source_chest,
        Item? sourceItem = null,
        int whichSpecialButton = -1,
        object? context = null)
    {
        if (playSound)
        {
            Game1.player.currentLocation.localSound("openChest");
        }

        return new ItemGrabMenu(
            this.Items,
            reverseGrab,
            showReceivingMenu,
            this.HighlightItems,
            this.GrabItemFromInventory,
            null,
            this.GrabItemFromChest,
            snapToBottom,
            canBeExitedWithKey,
            playRightClickSound,
            allowRightClick,
            showOrganizeButton,
            source,
            sourceItem,
            whichSpecialButton,
            context ?? (this.Source.TryGetTarget(out var target) ? target : context));
    }

    /// <summary>Initialize the individual mod data storage options for this container.</summary>
    protected void InitOptions()
    {
        var modDataOptions = new ModDataStorageOptions(this.ModData);
        this.AddOptions(StorageOption.Individual, () => modDataOptions);

        // Initialize Storage Name
        if (string.IsNullOrWhiteSpace(this.StorageName)
            && this.ModData.TryGetValue("Pathoschild.ChestsAnywhere/Name", out var chestsAnywhereName)
            && !string.IsNullOrWhiteSpace(chestsAnywhereName)
            && this.ModData.TryGetValue("Pathoschild.ChestsAnywhere/Category", out var chestsAnywhereCatergory)
            && !string.IsNullOrWhiteSpace(chestsAnywhereCatergory))
        {
            this.StorageName = $"{chestsAnywhereCatergory} - {chestsAnywhereName}";
        }
    }

    private string Get(Func<IStorageOptions, string> getter)
    {
        foreach (var option in StorageOptionExtensions.GetValues())
        {
            if (!this.storageOptions.TryGetValue(option, out var getOptions))
            {
                continue;
            }

            var options = getOptions();
            var effectiveValue = getter(options);
            if (!string.IsNullOrWhiteSpace(effectiveValue))
            {
                return effectiveValue;
            }
        }

        return string.Empty;
    }

    private TOption Get<TOption>(Func<IStorageOptions, TOption> getter)
        where TOption : struct
    {
        foreach (var option in StorageOptionExtensions.GetValues())
        {
            if (!this.storageOptions.TryGetValue(option, out var getOptions))
            {
                continue;
            }

            var options = getOptions();
            var value = getter(options);
            if (!EqualityComparer<TOption>.Default.Equals(value, default(TOption)))
            {
                return value;
            }
        }

        return default(TOption);
    }

    private void Set(Func<IStorageOptions, string> getter, Action<IStorageOptions, string> setter, string value)
    {
        foreach (var option in StorageOptionExtensions.GetValues().Except(new[] { StorageOption.Individual }))
        {
            if (!this.storageOptions.TryGetValue(option, out var getOptions))
            {
                continue;
            }

            var options = getOptions();
            var effectiveValue = getter(options);
            if (string.IsNullOrWhiteSpace(effectiveValue))
            {
                continue;
            }

            if (value.Equals(effectiveValue, StringComparison.OrdinalIgnoreCase))
            {
                setter(this.ActualOptions, string.Empty);
                return;
            }

            break;
        }

        setter(this.ActualOptions, value);
    }

    private void Set<TOption>(
        Func<IStorageOptions, TOption> getter,
        Action<IStorageOptions, TOption> setter,
        TOption value)
        where TOption : struct
    {
        var defaultValue = default(TOption);
        foreach (var option in StorageOptionExtensions.GetValues().Except(new[] { StorageOption.Individual }))
        {
            if (!this.storageOptions.TryGetValue(option, out var getOptions))
            {
                continue;
            }

            var options = getOptions();
            var effectiveValue = getter(options);
            if (EqualityComparer<TOption>.Default.Equals(effectiveValue, defaultValue))
            {
                continue;
            }

            if (value.Equals(effectiveValue))
            {
                setter(this.ActualOptions, defaultValue);
                return;
            }

            break;
        }

        setter(this.ActualOptions, value);
    }
}