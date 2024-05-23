#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Helpers;
#else
namespace StardewMods.Common.Helpers;
#endif

/// <inheritdoc />
internal sealed class ReverseComparer<T> : Comparer<T>
    where T : IComparable<T>
{
    /// <inheritdoc />
    public override int Compare(T? x, T? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null)
        {
            return 1;
        }

        if (y == null)
        {
            return -1;
        }

        return y.CompareTo(x);
    }
}