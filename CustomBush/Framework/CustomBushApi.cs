namespace StardewMods.CustomBush.Framework;

using StardewMods.Common.Services.Integrations.CustomBush;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.CustomBush.Framework.Services;
using StardewValley.TerrainFeatures;

/// <inheritdoc />
public sealed class CustomBushApi : ICustomBushApi
{
    private readonly AssetHandler assetHandler;
    private readonly ILog log;
    private readonly IModInfo modInfo;
    private readonly ModPatches modPatches;

    /// <summary>Initializes a new instance of the <see cref="CustomBushApi" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="modPatches">Dependency for managing custom bushes.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    internal CustomBushApi(AssetHandler assetHandler, ModPatches modPatches, IModInfo modInfo, ILog log)
    {
        this.assetHandler = assetHandler;
        this.modPatches = modPatches;
        this.modInfo = modInfo;
        this.log = log;
    }

    /// <inheritdoc />
    public IEnumerable<(string Id, ICustomBush Data)> GetData() =>
        this.assetHandler.Data.Select(pair => (pair.Key, (ICustomBush)pair.Value));

    /// <inheritdoc />
    public bool IsCustomBush(Bush bush) => this.modPatches.IsCustomBush(bush);

    /// <inheritdoc />
    public bool TryGetCustomBush(Bush bush, out ICustomBush? customBush)
    {
        if (this.modPatches.TryGetCustomBush(bush, out var customBushInstance))
        {
            customBush = customBushInstance;
            return true;
        }

        customBush = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetDrops(string id, out IList<ICustomBushDrop>? drops)
    {
        drops = null;
        if (!this.assetHandler.Data.TryGetValue(id, out var customBush))
        {
            return false;
        }

        drops = customBush.ItemsProduced.Select(drop => (ICustomBushDrop)drop).ToList();
        return true;
    }
}