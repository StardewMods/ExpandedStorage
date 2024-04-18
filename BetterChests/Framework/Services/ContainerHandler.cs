namespace StardewMods.BetterChests.Framework.Services;

using HarmonyLib;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Objects;

/// <summary>Responsible for handling containers.</summary>
internal sealed class ContainerHandler : BaseService<ContainerHandler>
{
    private static ContainerHandler instance = null!;

    private readonly ContainerFactory containerFactory;
    private readonly IEventPublisher eventPublisher;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="ContainerHandler" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into external code.</param>
    public ContainerHandler(
        ContainerFactory containerFactory,
        IEventPublisher eventPublisher,
        ILog log,
        IManifest manifest,
        IPatchManager patchManager,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        ContainerHandler.instance = this;
        this.containerFactory = containerFactory;
        this.eventPublisher = eventPublisher;
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

        this.eventPublisher.Publish(itemTransferringEventArgs);

        // Automatically block if prevented
        if (itemTransferringEventArgs.IsPrevented)
        {
            return false;
        }

        // Return true if forced or allowed
        return force || itemTransferringEventArgs.IsAllowed;
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

                this.eventPublisher.Publish(itemTransferringEventArgs);

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