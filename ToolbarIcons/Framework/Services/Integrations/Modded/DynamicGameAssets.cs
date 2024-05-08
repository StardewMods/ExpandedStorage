namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DynamicGameAssets : IMethodIntegration
{
    /// <inheritdoc />
    public object?[] Arguments => ["dga_store", Array.Empty<string>()];

    /// <inheritdoc />
    public string HoverText => I18n.Button_DynamicGameAssets();

    /// <inheritdoc />
    public int Index => 3;

    /// <inheritdoc />
    public string MethodName => "OnStoreCommand";

    /// <inheritdoc />
    public string ModId => "spacechase0.DynamicGameAssets";
}