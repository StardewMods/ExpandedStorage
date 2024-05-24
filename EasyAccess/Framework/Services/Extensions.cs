namespace StardewMods.EasyAccess.Framework.Services;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.EasyAccess.Framework.Enums;

/// <summary>Extension methods for Easy Access.</summary>
internal static class Extensions
{
    /// <summary>Retrieves an internal icon.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="internalIcon">The internal icon.</param>
    /// <returns>Returns the icon.</returns>
    public static IIcon RequireIcon(this IIconRegistry iconRegistry, InternalIcon internalIcon) =>
        iconRegistry.Icon(internalIcon.ToStringFast());

    /// <summary>Attempt to retrieve a specific internal icon.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="internalIcon">The internal icon.</param>
    /// <param name="icon">When this method returns, contains the icon; otherwise, null.</param>
    /// <returns><c>true</c> if the icon exists; otherwise, <c>false</c>.</returns>
    public static bool TryGetIcon(
        this IIconRegistry iconRegistry,
        InternalIcon internalIcon,
        [NotNullWhen(true)] out IIcon? icon) =>
        iconRegistry.TryGetIcon(internalIcon.ToStringFast(), out icon);
}