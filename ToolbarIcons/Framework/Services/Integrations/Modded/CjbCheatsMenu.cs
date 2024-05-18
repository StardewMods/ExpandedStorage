namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class CjbCheatsMenu : IMethodIntegration
{
    /// <inheritdoc />
    public object?[] Arguments => [0, true];

    /// <inheritdoc />
    public string HoverText => I18n.Button_CheatsMenu();

    /// <inheritdoc />
    public string Icon => VanillaIcon.QualityIridium.ToStringFast();

    /// <inheritdoc />
    public string MethodName => "OpenCheatsMenu";

    /// <inheritdoc />
    public string ModId => "CJBok.CheatsMenu";
}