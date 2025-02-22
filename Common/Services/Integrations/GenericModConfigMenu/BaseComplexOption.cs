#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.GenericModConfigMenu;
#else

namespace StardewMods.Common.Services.Integrations.GenericModConfigMenu;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <inheritdoc />
internal abstract class BaseComplexOption : IComplexOption
{
    /// <inheritdoc />
    public virtual string Name => string.Empty;

    /// <inheritdoc />
    public virtual string Tooltip => string.Empty;

    /// <inheritdoc />
    public abstract int Height { get; protected set; }

    /// <inheritdoc />
    public virtual void AfterReset()
    { }

    /// <inheritdoc />
    public virtual void AfterSave()
    { }

    /// <inheritdoc />
    public virtual void BeforeMenuClosed()
    { }

    /// <inheritdoc />
    public virtual void BeforeMenuOpened()
    { }

    /// <inheritdoc />
    public virtual void BeforeReset()
    { }

    /// <inheritdoc />
    public virtual void BeforeSave()
    { }

    /// <inheritdoc />
    public abstract void Draw(SpriteBatch spriteBatch, Vector2 pos);
}