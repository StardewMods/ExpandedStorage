namespace StardewMods.ExpandedStorage.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;

/// <inheritdoc />
internal sealed class AssetHandler : BaseAssetHandler
{
    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    public AssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IModContentHelper modContentHelper)
        : base(eventManager, gameContentHelper, modContentHelper)
    {
        // Nothing
    }
}