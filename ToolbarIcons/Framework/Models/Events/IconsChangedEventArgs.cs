namespace StardewMods.ToolbarIcons.Framework.Models.Events;

/// <summary>The event arguments after an icon is changed.</summary>
internal sealed class IconsChangedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="IconsChangedEventArgs" /> class.</summary>
    /// <param name="added">The added icon ids.</param>
    /// <param name="removed">The removed icon ids.</param>
    public IconsChangedEventArgs(IEnumerable<string> added, IEnumerable<string> removed)
    {
        this.Added = added;
        this.Removed = removed;
    }

    /// <summary>Gets the added icon ids.</summary>
    public IEnumerable<string> Added { get; }

    /// <summary>Gets the removed icon ids.</summary>
    public IEnumerable<string> Removed { get; }
}