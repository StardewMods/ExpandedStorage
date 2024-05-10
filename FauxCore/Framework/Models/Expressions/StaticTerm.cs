namespace StardewMods.FauxCore.Framework.Models.Expressions;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents a basic term.</summary>
internal sealed class StaticTerm : IExpression
{
    private readonly bool exact;

    /// <summary>Initializes a new instance of the <see cref="StaticTerm" /> class.</summary>
    /// <param name="term">The search value.</param>
    /// <param name="exact">Indicates whether exact matching should be used.</param>
    public StaticTerm(string term, bool exact)
    {
        this.exact = exact;
        this.Term = term;
    }

    /// <inheritdoc />
    public IEnumerable<IExpression> Expressions => Array.Empty<IExpression>();

    /// <inheritdoc />
    public ExpressionType ExpressionType => ExpressionType.Static;

    /// <inheritdoc />
    public string Term { get; }

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) => (this.Equals(x) ? -1 : 1).CompareTo(this.Equals(y) ? -1 : 1);

    /// <inheritdoc />
    public bool Equals(Item? other) =>
        other is not null
        && ((other.DisplayName is not null && this.Equals(other.DisplayName))
            || other.GetContextTags().Any(this.Equals));

    /// <inheritdoc />
    public bool Equals(IInventory? other) => other is not null && other.Any(this.Equals);

    /// <inheritdoc />
    public bool Equals(string? other) =>
        !string.IsNullOrWhiteSpace(other)
        && (this.exact
            ? string.Equals(other, this.Term, StringComparison.OrdinalIgnoreCase)
            : other.Contains(this.Term, StringComparison.OrdinalIgnoreCase));

    // public bool Equals(IInventory other) =>
    //     this.Equals(container.ToString()!) || other.Any(this.Equals);

    /// <inheritdoc />
    public override string ToString() => this.Term;
}