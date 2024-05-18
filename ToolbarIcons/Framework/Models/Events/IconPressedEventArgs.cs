namespace StardewMods.ToolbarIcons.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <inheritdoc cref="IIconPressedEventArgs" />
internal sealed class IconPressedEventArgs : EventArgs, IIconPressedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="IconPressedEventArgs" /> class.</summary>
    /// <param name="id">The icon id.</param>
    /// <param name="button">The button.</param>
    public IconPressedEventArgs(string id, SButton button)
    {
        this.Button = button;
        this.Id = id;
    }

    /// <inheritdoc />
    public SButton Button { get; }

    /// <inheritdoc />
    public string Id { get; }
}