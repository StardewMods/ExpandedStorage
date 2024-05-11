namespace StardewMods.BetterChests.Framework;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.Menus;

/// <inheritdoc />
public sealed class BetterChestsApi : IBetterChestsApi
{
    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly IModInfo modInfo;

    /// <summary>Initializes a new instance of the <see cref="BetterChestsApi" /> class.</summary>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="containerHandler">Dependency used for handling operations by containers.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    internal BetterChestsApi(
        IModInfo modInfo,
        ConfigManager configManager,
        ContainerHandler containerHandler,
        ContainerFactory containerFactory)
    {
        this.modInfo = modInfo;
        this.configManager = configManager;
        this.containerHandler = containerHandler;
        this.containerFactory = containerFactory;
    }

    /// <inheritdoc />
    public void AddConfigOptions(IManifest manifest, string? pageId, Func<string>? getTitle, IStorageOptions options) =>
        this.configManager.AddMainOption(manifest, pageId, getTitle, options);

    /// <inheritdoc />
    public void Sort(IStorageContainer container, bool reverse = false) =>
        this.containerHandler.Sort(container, reverse);

    /// <inheritdoc />
    public IEnumerable<(string ItemId, int Amount, bool Prevented)> Transfer(
        IStorageContainer containerFrom,
        IStorageContainer containerTo)
    {
        if (!this.containerHandler.Transfer(containerFrom, containerTo, out var amounts))
        {
            yield break;
        }

        foreach (var (itemId, amount) in amounts)
        {
            yield return (itemId, amount, false);
        }
    }

    /// <inheritdoc />
    public bool TryGetOne(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOne(farmer, out container);

    /// <inheritdoc />
    public bool TryGetOne(Item item, [NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOne(item, out container);

    /// <inheritdoc />
    public bool TryGetOne(GameLocation location, Vector2 pos, [NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOne(location, pos, out container);

    /// <inheritdoc />
    public bool TryGetOne(IClickableMenu menu, [NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOne(menu, out container);

    /// <inheritdoc />
    public bool TryGetOne(Farmer farmer, int index, [NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOne(farmer, index, out container);
}