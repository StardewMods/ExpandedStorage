#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;

#else

namespace StardewMods.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;

#endif

/// <summary>The event arguments when a component is clicked.</summary>
public interface IClicked
{
    /// <summary>Gets the button pressed.</summary>
    SButton Button { get; }

    /// <summary>Gets the cursor position.</summary>
    Point Cursor { get; }
}