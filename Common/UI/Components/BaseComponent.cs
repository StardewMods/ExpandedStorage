#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Models.Events;
using StardewMods.FauxCore.Common.Services;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

/// <summary>Base custom component.</summary>
internal abstract class BaseComponent : ClickableComponent, ICustomComponent
{
    private EventHandler<IClicked>? clicked;

    /// <summary>Initializes a new instance of the <see cref="BaseComponent" /> class.</summary>
    /// <param name="parent">The parent menu.</param>
    /// <param name="x">The component x-coordinate.</param>
    /// <param name="y">The component y-coordinate.</param>
    /// <param name="width">The component width.</param>
    /// <param name="height">The component height.</param>
    /// <param name="name">The component name.</param>
    protected BaseComponent(ICustomMenu? parent, int x, int y, int width, int height, string name)
        : base(new Rectangle(x, y, width, height), name)
    {
        this.Parent = parent;
        this.Color = Color.White;
    }

    /// <inheritdoc />
    public event EventHandler<IClicked>? Clicked
    {
        add => this.clicked += value;
        remove => this.clicked -= value;
    }

    /// <inheritdoc />
    public Rectangle Bounds => this.bounds;

    /// <inheritdoc />
    public Point Location => this.bounds.Location;

    /// <inheritdoc />
    public ICustomMenu? Parent { get; }

    /// <inheritdoc />
    public Point Size => this.bounds.Size;

    /// <inheritdoc />
    public Color Color { get; private set; }

    /// <inheritdoc />
    public string? HoverText { get; private set; }

    /// <inheritdoc />
    public virtual void Draw(SpriteBatch spriteBatch, Point cursor, Point offset) =>
        UiToolkit.DrawInFrame(
            spriteBatch,
            new Rectangle(this.bounds.X + offset.X, this.bounds.Y + offset.Y, this.bounds.Width, this.bounds.Height),
            sb => this.DrawInFrame(sb, cursor, offset));

    /// <summary>Draws the component in a framed area.</summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    /// <param name="cursor">The mouse position.</param>
    /// <param name="offset">The offset.</param>
    public virtual void DrawInFrame(SpriteBatch spriteBatch, Point cursor, Point offset)
    {
        // Do nothing
    }

    /// <inheritdoc />
    public virtual ICustomComponent MoveTo(Point location)
    {
        this.bounds.Location = location;
        return this;
    }

    /// <inheritdoc />
    public virtual ICustomComponent ResizeTo(Point size)
    {
        this.bounds.Size = size;
        return this;
    }

    /// <inheritdoc />
    public ICustomComponent SetColor(Color value)
    {
        this.Color = value;
        return this;
    }

    /// <inheritdoc />
    public ICustomComponent SetHoverText(string? value)
    {
        this.HoverText = value;
        return this;
    }

    /// <inheritdoc />
    public virtual bool TryLeftClick(Point cursor)
    {
        if (!this.visible || !this.bounds.Contains(cursor))
        {
            return false;
        }

        this.clicked.InvokeAll(this, new ClickedEventArgs(SButton.MouseLeft, cursor));
        return true;
    }

    /// <inheritdoc />
    public virtual bool TryRightClick(Point cursor)
    {
        if (!this.visible || !this.bounds.Contains(cursor))
        {
            return false;
        }

        this.clicked.InvokeAll(this, new ClickedEventArgs(SButton.Right, cursor));
        return true;
    }

    /// <inheritdoc />
    public virtual bool TryScroll(int direction) => false;

    /// <inheritdoc />
    public virtual void Update(Point cursor) { }
}