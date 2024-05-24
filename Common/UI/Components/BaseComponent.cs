#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Interfaces.UI;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces.UI;
using StardewValley.Menus;
#endif

/// <summary>Base custom component.</summary>
internal abstract class BaseComponent : ClickableComponent, ICustomComponent
{
    /// <summary>Initializes a new instance of the <see cref="BaseComponent" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <param name="name">The component name.</param>
    protected BaseComponent(IInputHelper inputHelper, int x, int y, int width, int height, string name)
        : base(new Rectangle(x, y, width, height), name) =>
        this.Input = inputHelper;

    /// <inheritdoc />
    public virtual string? HoverText => null;

    /// <inheritdoc />
    public Point Position => this.bounds.Location;

    /// <summary>Gets the input helper.</summary>
    protected IInputHelper Input { get; }

    /// <inheritdoc />
    public abstract void Draw(SpriteBatch spriteBatch, Point cursor, Point offset);

    /// <inheritdoc />
    public virtual void MoveTo(Point cursor) => this.bounds.Location = cursor;

    /// <inheritdoc />
    public virtual void Resize(Point dimensions) => this.bounds.Size = dimensions;

    /// <inheritdoc />
    public virtual bool TryLeftClick(Point cursor) => false;

    /// <inheritdoc />
    public virtual bool TryRightClick(Point cursor) => false;

    /// <inheritdoc />
    public virtual bool TryScroll(int direction) => false;

    /// <inheritdoc />
    public virtual void Update(Point cursor) { }
}