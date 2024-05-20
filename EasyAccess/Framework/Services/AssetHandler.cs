namespace StardewMods.EasyAccess.Framework.Services;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.EasyAccess.Framework.Enums;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler
{
    private static readonly InternalIcon[] Icons = [InternalIcon.Collect, InternalIcon.Dispense];

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(IIconRegistry iconRegistry, IModContentHelper modContentHelper, IThemeHelper themeHelper)
    {
        themeHelper.AddAsset($"{Mod.Id}/Icons", modContentHelper.Load<IRawTextureData>("assets/icons.png"));

        for (var index = 0; index < AssetHandler.Icons.Length; index++)
        {
            iconRegistry.AddIcon(
                AssetHandler.Icons[index].ToStringFast(),
                $"{Mod.Id}/Icons",
                new Rectangle(16 * (index % 5), 16 * (int)(index / 5f), 16, 16));
        }
    }
}