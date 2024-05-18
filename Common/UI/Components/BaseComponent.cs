namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;

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
    public virtual ClickableComponent Component => this;

    /// <inheritdoc />
    public virtual string? HoverText => null;

    /// <inheritdoc />
    public virtual bool Visible
    {
        get => this.Component.visible;
        set => this.Component.visible = value;
    }

    /// <summary>Gets the input helper.</summary>
    protected IInputHelper Input { get; }

    /// <inheritdoc />
    public virtual bool Contains(Vector2 position) => this.Component.containsPoint((int)position.X, (int)position.Y);

    /// <inheritdoc />
    public abstract void Draw(SpriteBatch spriteBatch);

    /// <inheritdoc />
    public virtual void MoveTo(int x, int y)
    {
        this.bounds.X = x;
        this.bounds.Y = y;
    }

    /// <inheritdoc />
    public virtual void Resize(int width, int height)
    {
        this.bounds.Width = width;
        this.bounds.Height = height;
    }

    /// <inheritdoc />
    public virtual bool TryLeftClick(int x, int y) => false;

    /// <inheritdoc />
    public virtual bool TryRightClick(int x, int y) => false;

    /// <inheritdoc />
    public virtual bool TryScroll(int direction) => false;

    /// <inheritdoc />
    public virtual void Update(int x, int y) { }
}