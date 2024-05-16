namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Api for shared functionality between mods.</summary>
public interface IFauxCoreApi
{
    /// <summary>Gets an instance of the expression handler service.</summary>
    public IExpressionHandler ExpressionHandler { get; }

    /// <summary>Gets an instance of the icon registry service.</summary>
    public IIconRegistry IconRegistry { get; }

    /// <summary>Gets or sets the <see cref="IMonitor" /> instance.</summary>
    public IMonitor? Monitor { get; set; }

    /// <summary>Gets an instance of the log service.</summary>
    public ILog Log { get; }

    /// <summary>Gets an instance of the patch manager service.</summary>
    public IPatchManager PatchManager { get; }

    /// <summary>Gets an instance of the theme helper service.</summary>
    public IThemeHelper ThemeHelper { get; }
}