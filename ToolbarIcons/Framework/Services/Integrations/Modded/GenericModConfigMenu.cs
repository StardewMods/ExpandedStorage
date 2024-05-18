namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class GenericModConfigMenu : IMethodIntegration
{
    /// <inheritdoc />
    public object?[] Arguments => [0];

    /// <inheritdoc />
    public string HoverText => I18n.Button_GenericModConfigMenu();

    /// <inheritdoc />
    public string Icon => InternalIcon.GenericModConfigMenu.ToStringFast();

    /// <inheritdoc />
    public string MethodName => "OpenListMenu";

    /// <inheritdoc />
    public string ModId => "spacechase0.GenericModConfigMenu";
}