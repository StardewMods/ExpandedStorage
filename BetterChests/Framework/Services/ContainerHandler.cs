namespace StardewMods.BetterChests.Framework.Services;

using HarmonyLib;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Responsible for handling containers.</summary>
internal sealed class ContainerHandler : GenericBaseService<ContainerHandler>
{
    private static ContainerHandler instance = null!;

    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly IEventManager eventManager;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly LocalizedTextManager localizedTextManager;
    private readonly IManifest manifest;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="ContainerHandler" /> class.</summary>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="localizedTextManager">Dependency used for formatting and translating text.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into external code.</param>
    public ContainerHandler(
        ConfigManager configManager,
        ContainerFactory containerFactory,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        LocalizedTextManager localizedTextManager,
        ILog log,
        IManifest manifest,
        IPatchManager patchManager,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        ContainerHandler.instance = this;
        this.configManager = configManager;
        this.containerFactory = containerFactory;
        this.eventManager = eventManager;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.localizedTextManager = localizedTextManager;
        this.manifest = manifest;
        this.reflectionHelper = reflectionHelper;

        // Patches
        patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
                AccessTools.DeclaredMethod(typeof(ContainerHandler), nameof(ContainerHandler.Chest_addItem_prefix)),
                PatchType.Prefix));

        var automateType = Type.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer, Automate");
        if (automateType is not null)
        {
            var methodStore = AccessTools.DeclaredMethod(automateType, "Store");
            if (methodStore is not null)
            {
                patchManager.Add(
                    this.UniqueId,
                    new SavedPatch(
                        methodStore,
                        AccessTools.DeclaredMethod(
                            typeof(ContainerHandler),
                            nameof(ContainerHandler.Automate_Store_prefix)),
                        PatchType.Prefix));
            }
        }

        patchManager.Patch(this.UniqueId);
    }

    /// <summary>Checks if an item is allowed to be added to a container.</summary>
    /// <param name="to">The container to add the item to.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="allowByDefault">Indicates whether it should be allowed by default.</param>
    /// <param name="force">Indicates whether it should be a forced attempt.</param>
    /// <returns>true if the item can be added; otherwise, false.</returns>
    public bool CanAddItem(IStorageContainer to, Item item, bool allowByDefault = false, bool force = false)
    {
        var hasItem = to.Items.ContainsId(item.QualifiedItemId);

        // Stop iterating if destination container is already at capacity
        if (to.Items.CountItemStacks() >= to.Capacity && !hasItem)
        {
            return false;
        }

        var itemTransferringEventArgs = new ItemTransferringEventArgs(to, item);
        if (allowByDefault)
        {
            itemTransferringEventArgs.AllowTransfer();
        }

        this.eventManager.Publish(itemTransferringEventArgs);

        // Automatically block if prevented
        if (itemTransferringEventArgs.IsPrevented)
        {
            return false;
        }

        // Return true if forced or allowed
        return force || itemTransferringEventArgs.IsAllowed;
    }

    /// <summary>Configure the container.</summary>
    /// <param name="container">The container to configure.</param>
    public void Configure(IStorageContainer container)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        this.Log.Info("{0}: Configuring {1}", this.Id, container);

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var options = new DefaultStorageOptions();
        container.ActualOptions.CopyTo(options);
        var parentOptions = container.GetParentOptions();
        this.genericModConfigMenuIntegration.Register(() => new DefaultStorageOptions().CopyTo(options), Save);

        gmcm.AddSectionTitle(this.manifest, () => container.DisplayName, container.ToString);

        gmcm.AddTextOption(
            this.manifest,
            () => options.StorageName,
            value => options.StorageName = value,
            I18n.Config_StorageName_Name,
            I18n.Config_StorageName_Tooltip);

        gmcm.AddTextOption(
            this.manifest,
            () => options.StorageIcon,
            value => options.StorageIcon = value,
            I18n.Config_StorageIcon_Name,
            I18n.Config_StorageIcon_Tooltip);

        // Access Chest Priority
        if (container.AccessChest is not RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                this.manifest,
                () => options.AccessChestPriority,
                value => options.AccessChestPriority = value,
                I18n.Config_AccessChestPriority_Name,
                I18n.Config_AccessChestPriority_Tooltip);
        }

        // Stash to Chest Priority
        if (container.StashToChest is not RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                this.manifest,
                () => (int)options.StashToChestPriority,
                value => options.StashToChestPriority = (StashPriority)value,
                I18n.Config_StashToChestPriority_Name,
                I18n.Config_StashToChestPriority_Tooltip,
                -3,
                3,
                1,
                this.localizedTextManager.FormatStashPriority);
        }

        // Categorize Chest
        if (container.CategorizeChest is not FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.CategorizeChestSearchTerm,
                value => options.CategorizeChestSearchTerm = value,
                I18n.Config_CategorizeChestSearchTerm_Name,
                I18n.Config_CategorizeChestSearchTerm_Tooltip);

            gmcm.AddTextOption(
                this.manifest,
                () => options.CategorizeChestIncludeStacks.ToStringFast(),
                value => options.CategorizeChestIncludeStacks = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CategorizeChestIncludeStacks_Name,
                I18n.Config_CategorizeChestIncludeStacks_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption(parentOptions?.CategorizeChestIncludeStacks));
        }

        gmcm.AddPageLink(this.manifest, "Main", I18n.Section_Main_Name, I18n.Section_Main_Description);
        this.configManager.AddMainOption("Main", I18n.Section_Main_Name, options, parentOptions: parentOptions);
        gmcm.OpenModMenu(this.manifest);
        return;

        void Save()
        {
            this.Log.Trace("Config changed: {0}\n{1}", container, options);
            options.CopyTo(container);

            if (container is ChestContainer chestContainer)
            {
                chestContainer.Chest.fridge.Value = container.CookFromChest is not RangeOption.Disabled;
            }
        }
    }

    /// <summary>Sort a a container.</summary>
    /// <param name="container">The container to sort.</param>
    /// <param name="reverse">Indicates whether to reverse the sort order.</param>
    public void Sort(IStorageContainer container, bool reverse = false)
    {
        var containerSortingEventArgs = new ContainerSortingEventArgs(container);
        ItemGrabMenu.organizeItemsInList(container.Items);
        this.eventManager.Publish(containerSortingEventArgs);

        if (!reverse)
        {
            return;
        }

        var copy = container.Items.Reverse().ToList();
        container.Items.OverwriteWith(copy);
    }

    /// <summary>Transfers items from one container to another.</summary>
    /// <param name="from">The container to transfer items from.</param>
    /// <param name="to">The container to transfer items to.</param>
    /// <param name="amounts">Output parameter that contains the transferred item amounts.</param>
    /// <param name="force">Indicates whether to attempt to force the transfer.</param>
    /// <param name="existingOnly">Indicates whether to only transfer to existing stacks.</param>
    /// <returns>true if the transfer was successful; otherwise, false.</returns>
    public bool Transfer(
        IStorageContainer from,
        IStorageContainer to,
        [NotNullWhen(true)] out Dictionary<string, int>? amounts,
        bool force = false,
        bool existingOnly = false)
    {
        var items = new Dictionary<string, int>();
        from.ForEachItem(
            item =>
            {
                var hasItem = to.Items.ContainsId(item.QualifiedItemId);

                // Stop iterating if destination container is already at capacity
                if (to.Items.CountItemStacks() >= to.Capacity && !hasItem)
                {
                    return false;
                }

                var itemTransferringEventArgs = new ItemTransferringEventArgs(to, item);
                if (existingOnly && hasItem)
                {
                    itemTransferringEventArgs.AllowTransfer();
                }

                this.eventManager.Publish(itemTransferringEventArgs);

                // Automatically block if prevented
                if (itemTransferringEventArgs.IsPrevented)
                {
                    return true;
                }

                // Block if existing only and item is not in destination container
                if (existingOnly && !hasItem)
                {
                    return true;
                }

                // Block if not forced or allowed
                if (!force && !itemTransferringEventArgs.IsAllowed)
                {
                    return true;
                }

                var stack = item.Stack;
                items.TryAdd(item.QualifiedItemId, 0);
                if (!to.TryAdd(item, out var remaining) || !from.TryRemove(item))
                {
                    return true;
                }

                var amount = stack - (remaining?.Stack ?? 0);
                items[item.QualifiedItemId] += amount;
                return true;
            });

        if (items.Any())
        {
            amounts = items;
            return true;
        }

        amounts = null;
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = ContainerHandler.instance.reflectionHelper.GetProperty<Item>(stack, "Sample").GetValue();
        return !ContainerHandler.instance.containerFactory.TryGetOne(___Chest, out var container)
            || ContainerHandler.instance.CanAddItem(container, item, true);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!ContainerHandler.instance.containerFactory.TryGetOne(__instance, out var container)
            || ContainerHandler.instance.CanAddItem(container, item, true, true))
        {
            return true;
        }

        __result = item;
        return false;
    }
}