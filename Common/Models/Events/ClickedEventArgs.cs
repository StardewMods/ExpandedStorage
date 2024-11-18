#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Models.Events;

using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;

#else

namespace StardewMods.Common.Models.Events;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.FauxCore;

#endif

/// <inheritdoc />
internal sealed class ClickedEventArgs : IClicked
{
    /// <summary></summary>
    /// <param name="button">The button pressed.</param>
    /// <param name="cursor">The cursor position.</param>
    public ClickedEventArgs(SButton button, Point cursor)
    {
        this.Button = button;
        this.Cursor = cursor;
    }

    public SButton Button { get; }

    /// <inheritdoc />
    public Point Cursor { get; }
}