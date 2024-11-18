#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.ToolbarIcons;
#else

namespace StardewMods.Common.Services.Integrations.ToolbarIcons;
#endif

#pragma warning disable CA1711

/// <summary>Represents the event arguments for a toolbar icon being pressed.</summary>
public interface IIconPressedEventArgs
{
    /// <summary>Gets the button that was pressed.</summary>
    SButton Button { get; }

    /// <summary>Gets the id of the icon that was pressed.</summary>
    string Id { get; }
}