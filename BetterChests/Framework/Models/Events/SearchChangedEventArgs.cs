namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Represents the event arguments for when the search term changes.</summary>
internal sealed class SearchChangedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="SearchChangedEventArgs" /> class.</summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="searchExpression">The search expression.</param>
    public SearchChangedEventArgs(string searchTerm, IExpression? searchExpression)
    {
        this.SearchTerm = searchTerm;
        this.SearchExpression = searchExpression;
    }

    /// <summary>Gets the search expression.</summary>
    public IExpression? SearchExpression { get; }

    /// <summary>Gets the search term.</summary>
    public string SearchTerm { get; }
}