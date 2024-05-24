#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
#else
namespace StardewMods.Common.Services.Integrations.FauxCore;
#endif

using Microsoft.Xna.Framework;

/// <summary>A registry for icons.</summary>
public interface IIconRegistry
{
    /// <summary>Registers an icon.</summary>
    /// <param name="id">The icon unique identifier.</param>
    /// <param name="path">The icon texture path.</param>
    /// <param name="area">The icon source area.</param>
    public void AddIcon(string id, string path, Rectangle area);

    /// <summary>Gets the icons registered by any mod.</summary>
    /// <param name="modId">Restrict to icons from a particular mod.</param>
    /// <returns>An enumerable of icons.</returns>
    public IEnumerable<IIcon> GetIcons(string? modId = null);

    /// <summary>Retrieves an icon with the given id.</summary>
    /// <param name="id">The unique identifier of the icon.</param>
    /// <returns>Returns the icon.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no icon is found for the given id.</exception>
    public IIcon Icon(string id);

    /// <summary>Retrieves a vanilla icon.</summary>
    /// <param name="icon">The vanilla icon.</param>
    /// <returns>Returns the icon.</returns>
    public IIcon Icon(VanillaIcon icon);

    /// <summary>Attempt to retrieve a specific icon with the given id.</summary>
    /// <param name="id">The unique identifier of the icon.</param>
    /// <param name="icon">When this method returns, contains the icon; otherwise, null.</param>
    /// <returns><c>true</c> if the icon exists; otherwise, <c>false</c>.</returns>
    public bool TryGetIcon(string id, [NotNullWhen(true)] out IIcon? icon);
}