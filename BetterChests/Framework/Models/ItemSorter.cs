namespace StardewMods.BetterChests.Framework.Models;

using StardewMods.BetterChests.Framework.Enums;

/// <summary>Compares two items based on a sort expression.</summary>
internal sealed class ItemSorter : IComparer<Item>
{
    private readonly List<IComparer<Item>> sortables;

    /// <summary>Initializes a new instance of the <see cref="ItemSorter" /> class.</summary>
    /// <param name="sortExpression">The expression to sort items by.</param>
    public ItemSorter(string sortExpression)
    {
        this.sortables = new List<IComparer<Item>>();
        this.ParseAttributes(sortExpression);
    }

    /// <inheritdoc />
    public int Compare(Item? x, Item? y) =>
        this.sortables.Select(sortable => sortable.Compare(x, y)).FirstOrDefault(result => result != 0);

    private void ParseAttributes(string sortBy)
    {
        // Split sort key into entries
        var entries = sortBy.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        // Parse each part into attributes
        foreach (var entry in entries)
        {
            this.ParseAttribute(entry);
        }
    }

    private void ParseAttribute(string entry)
    {
        var parts = entry.Split('~', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        // Modifiers
        var ascending = true;
        var negative = false;

        while (true)
        {
            switch (parts[0][0])
            {
                // Ascending
                case '<':
                    ascending = true;
                    parts[0] = parts[0][1..];
                    continue;

                // Descending
                case '>':
                    ascending = false;
                    parts[0] = parts[0][1..];
                    continue;

                case '!':
                    negative = !negative;
                    parts[0] = parts[0][1..];
                    continue;
            }

            break;
        }

        // Comparer
        var compareTo = string.Empty;
        if (parts.Length == 2)
        {
            compareTo = parts[1];
        }

        // Attribute
        if (!SortByExtensions.TryParse(parts[0], out var sortBy, true))
        {
            return;
        }

        switch (sortBy)
        {
            case SortBy.Category:
                this.sortables.Add(
                    new LiteralAttribute(ascending, negative, compareTo, item => item.getCategoryName()));

                return;

            case SortBy.Name:
                this.sortables.Add(new LiteralAttribute(ascending, negative, compareTo, item => item.DisplayName));
                return;

            case SortBy.Quantity:
                this.sortables.Add(new NumericalAttribute(ascending, negative, compareTo, item => item.Stack));
                return;

            case SortBy.Quality:
                this.sortables.Add(new NumericalAttribute(ascending, negative, compareTo, item => item.Quality));
                return;

            case SortBy.Type:
                this.sortables.Add(new LiteralAttribute(ascending, negative, compareTo, item => item.TypeDefinitionId));
                return;

            default: throw new ArgumentOutOfRangeException(nameof(entry));
        }
    }

    private class LiteralAttribute : IComparer<Item>
    {
        private readonly Func<Item, string> accessor;
        private readonly int ascending;
        private readonly string? compareTo;
        private readonly int negative;

        public LiteralAttribute(bool ascending, bool negative, string compareTo, Func<Item, string> accessor)
        {
            this.ascending = ascending ? 1 : -1;
            this.negative = negative ? -1 : 1;
            this.compareTo = string.IsNullOrWhiteSpace(compareTo) ? null : compareTo;
            this.accessor = accessor;
        }

        public int Compare(Item? x, Item? y)
        {
            if (x is null && y is null)
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            if (this.compareTo is null)
            {
                return this.ascending
                    * string.Compare(this.accessor(x), this.accessor(y), StringComparison.OrdinalIgnoreCase);
            }

            var a = this.accessor(x).Contains(this.compareTo, StringComparison.OrdinalIgnoreCase)
                ? -this.negative
                : this.negative;

            var b = this.accessor(y).Contains(this.compareTo, StringComparison.OrdinalIgnoreCase)
                ? -this.negative
                : this.negative;

            return this.ascending * a.CompareTo(b);
        }
    }

    private class NumericalAttribute : IComparer<Item>
    {
        private readonly Func<Item, int> accessor;
        private readonly int ascending;
        private readonly int? compareTo;
        private readonly int negative;

        public NumericalAttribute(bool ascending, bool negative, string compareTo, Func<Item, int> accessor)
        {
            this.ascending = ascending ? 1 : -1;
            this.negative = negative ? -1 : 1;
            this.accessor = accessor;
            this.compareTo = string.IsNullOrWhiteSpace(compareTo) || int.TryParse(compareTo, out var intCompareTo)
                ? null
                : intCompareTo;
        }

        public int Compare(Item? x, Item? y)
        {
            if (x is null && y is null)
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            if (this.compareTo is null)
            {
                return this.ascending * this.accessor(x).CompareTo(this.accessor(y));
            }

            var a = this.accessor(x) == this.compareTo ? -this.negative : this.negative;
            var b = this.accessor(y) == this.compareTo ? -this.negative : this.negative;
            return this.ascending * a.CompareTo(b);
        }
    }
}