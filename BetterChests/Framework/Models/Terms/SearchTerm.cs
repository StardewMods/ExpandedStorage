namespace StardewMods.BetterChests.Framework.Models.Terms;

using StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Represents a search term.</summary>
internal sealed class SearchTerm : ISearchExpression
{
    /// <summary>Initializes a new instance of the <see cref="SearchTerm" /> class.</summary>
    /// <param name="term">The search value.</param>
    public SearchTerm(string term) => this.Term = term;

    /// <summary>Gets the value.</summary>
    public string Term { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item? item) =>
        item is not null
        && ((item.Name is not null && item.Name.Equals(this.Term, StringComparison.OrdinalIgnoreCase))
            || (item.DisplayName is not null && item.DisplayName.Equals(this.Term, StringComparison.OrdinalIgnoreCase))
            || item.GetContextTags().Any(tag => tag.Equals(this.Term, StringComparison.OrdinalIgnoreCase)));

    /// <inheritdoc />
    public bool PartialMatch(Item? item) =>
        item is not null
        && ((item.Name is not null && item.Name.Contains(this.Term, StringComparison.OrdinalIgnoreCase))
            || (item.DisplayName is not null
                && item.DisplayName.Contains(this.Term, StringComparison.OrdinalIgnoreCase))
            || item.GetContextTags().Any(tag => tag.Contains(this.Term, StringComparison.OrdinalIgnoreCase)));
}