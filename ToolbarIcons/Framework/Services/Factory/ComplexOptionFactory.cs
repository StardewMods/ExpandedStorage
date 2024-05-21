namespace StardewMods.ToolbarIcons.Framework.Services.Factory;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework.UI;

/// <summary>Responsible for returning complex options.</summary>
internal sealed class ComplexOptionFactory
{
    private readonly IIconRegistry iconRegistry;
    private readonly Dictionary<string, string?> icons;
    private readonly IInputHelper inputHelper;

    /// <summary>Initializes a new instance of the <see cref="ComplexOptionFactory" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="icons">Dictionary containing all added icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    public ComplexOptionFactory(IIconRegistry iconRegistry, Dictionary<string, string?> icons, IInputHelper inputHelper)
    {
        this.iconRegistry = iconRegistry;
        this.icons = icons;
        this.inputHelper = inputHelper;
    }

    /// <summary>Attempt to retrieve a complex option.</summary>
    /// <param name="getCurrentId">Function which returns the current id.</param>
    /// <param name="getTooltip">Function which returns the tooltip.</param>
    /// <param name="getEnabled">Function which returns if icon is enabled.</param>
    /// <param name="setEnabled">Action which sets if the icon is enabled.</param>
    /// <param name="moveDown">Action to perform when down is pressed.</param>
    /// <param name="moveUp">Action to perform when up is pressed.</param>
    /// <param name="option">When this method returns, contains the option; otherwise, null.</param>
    /// <returns><c>true</c> if the option can be created; otherwise, <c>false</c>.</returns>
    public bool TryGetToolbarIconOption(
        Func<string> getCurrentId,
        Func<string> getTooltip,
        Func<bool> getEnabled,
        Action<bool> setEnabled,
        Action? moveDown,
        Action? moveUp,
        [NotNullWhen(true)] out BaseComplexOption? option)
    {
        try
        {
            option = new ToolbarIconOption(
                this.iconRegistry,
                this.inputHelper,
                getCurrentId,
                getTooltip,
                getEnabled,
                setEnabled,
                moveDown,
                moveUp);

            return true;
        }
        catch
        {
            option = null;
            return false;
        }
    }
}