namespace StardewMods.CustomBush.Framework;

using StardewMods.Common.Services.Integrations.CustomBush;
using StardewMods.CustomBush.Framework.Services;
using StardewValley.TerrainFeatures;

/// <inheritdoc />
public sealed class CustomBushApi : ICustomBushApi
{
    private readonly AssetHandler assetHandler;
    private readonly IModInfo modInfo;
    private readonly ModPatches modPatches;

    /// <summary>Initializes a new instance of the <see cref="CustomBushApi" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="modPatches">Dependency for managing custom bushes.</param>
    internal CustomBushApi(AssetHandler assetHandler, IModInfo modInfo, ModPatches modPatches)
    {
        this.assetHandler = assetHandler;
        this.modInfo = modInfo;
        this.modPatches = modPatches;
    }

    /// <inheritdoc />
    public IEnumerable<(string Id, ICustomBush Data)> GetData() =>
        this.assetHandler.Data.Select(pair => (pair.Key, (ICustomBush)pair.Value));

    /// <inheritdoc />
    public bool IsCustomBush(Bush bush) => this.modPatches.IsCustomBush(bush);

    /// <inheritdoc />
    public bool TryGetCustomBush(Bush bush, out ICustomBush? customBush) =>
        this.TryGetCustomBush(bush, out customBush, out _);

    /// <inheritdoc />
    public bool TryGetCustomBush(Bush bush, out ICustomBush? customBush, out string? id)
    {
        if (this.modPatches.TryGetCustomBush(bush, out var customBushInstance))
        {
            // Replace blank name with default
            if (string.IsNullOrWhiteSpace(customBushInstance.DisplayName))
            {
                customBushInstance.DisplayName = I18n.Default_Name();
            }

            // Replace blank description with default
            if (string.IsNullOrWhiteSpace(customBushInstance.Description))
            {
                customBushInstance.Description = I18n.Default_Description();
            }

            customBush = customBushInstance;
            id = customBushInstance.Id;
            return true;
        }

        customBush = null;
        id = null;
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