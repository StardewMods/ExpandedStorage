namespace StardewMods.BetterChests.Framework.Models.Events;

/// <summary>The event arguments after a tab is clicked.</summary>
internal sealed class TabClickedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="TabClickedEventArgs" /> class.</summary>
    /// <param name="button">The button that was pressed.</param>
    /// <param name="data">The tab data.</param>
    public TabClickedEventArgs(SButton button, TabData data)
    {
        this.Button = button;
        this.Data = data;
    }

    /// <summary>Gets the data of the clicked tab.</summary>
    public TabData Data { get; }

    /// <summary>Gets the button that was pressed.</summary>
    private SButton Button { get; }
}