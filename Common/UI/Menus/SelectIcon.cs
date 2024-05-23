#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

/// <summary>Menu for selecting an icon.</summary>
internal class SelectIcon : FramedMenu
{
    private readonly IEnumerable<IIcon> allIcons;
    private readonly int columns;
    private readonly GetHoverText getHoverText;
    private readonly List<Highlight> highlights = [];
    private readonly int length;
    private readonly List<Operation> operations = [];
    private readonly int spacing;

    private List<ClickableTextureComponent>? components;
    private int currentIndex = -1;
    private List<IIcon>? icons;
    private EventHandler<IIcon?>? selectionChanged;

    /// <summary>Initializes a new instance of the <see cref="SelectIcon" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="allIcons">The icons to pick from.</param>
    /// <param name="rows">This rows of icons to display.</param>
    /// <param name="columns">The columns of icons to display.</param>
    /// <param name="getHoverText">A function which returns the hover text for an icon.</param>
    /// <param name="scale">The icon scale.</param>
    /// <param name="spacing">The spacing between icons.</param>
    /// <param name="x">The x-position.</param>
    /// <param name="y">The y-position.</param>
    public SelectIcon(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        IEnumerable<IIcon> allIcons,
        int rows,
        int columns,
        GetHoverText? getHoverText = null,
        float scale = Game1.pixelZoom,
        int spacing = 16,
        int? x = null,
        int? y = null)
        : base(
            inputHelper,
            reflectionHelper,
            x,
            y,
            (columns * ((int)(scale * 16) + spacing)) + spacing,
            (rows * ((int)(scale * 16) + spacing)) + spacing)
    {
        this.allIcons = allIcons;
        this.columns = columns;
        this.getHoverText = getHoverText ?? SelectIcon.GetUniqueId;
        this.spacing = spacing;
        this.length = (int)Math.Floor(scale * 16);
    }

    /// <summary>Get the hover text for an icon.</summary>
    /// <param name="icon">The icon.</param>
    /// <returns>The hover text.</returns>
    public delegate string GetHoverText(IIcon icon);

    /// <summary>Highlight an icon.</summary>
    /// <param name="icon">The icon to highlight.</param>
    /// <returns><c>true</c> if the icon should be highlighted; otherwise, <c>false</c>.</returns>
    public delegate bool Highlight(IIcon icon);

    /// <summary>An operation to perform on the icons.</summary>
    /// <param name="icons">The original icons.</param>
    /// <returns>Returns a modified list of icons.</returns>
    public delegate IEnumerable<IIcon> Operation(IEnumerable<IIcon> icons);

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<IIcon?> SelectionChanged
    {
        add => this.selectionChanged += value;
        remove => this.selectionChanged -= value;
    }

    /// <summary>Gets the currently selected icon.</summary>
    public IIcon? CurrentSelection => this.Icons.ElementAtOrDefault(this.CurrentIndex);

    /// <summary>Gets the icons.</summary>
    public virtual List<IIcon> Icons =>
        this.icons ??= this.operations.Aggregate(this.allIcons, (current, operation) => operation(current)).ToList();

    /// <summary>Gets or sets the current index.</summary>
    public int CurrentIndex
    {
        get => this.currentIndex;
        protected set
        {
            this.currentIndex = value;
            this.selectionChanged?.InvokeAll(this, this.CurrentSelection);
        }
    }

    /// <inheritdoc />
    protected override Rectangle Frame =>
        new(this.xPositionOnScreen + 4, this.yPositionOnScreen + 4, this.width - 8, this.height - 8);

    /// <inheritdoc />
    protected override int StepSize => 32;

    private IEnumerable<ClickableTextureComponent> Components
    {
        get
        {
            if (this.components is not null)
            {
                return this.components;
            }

            this.components = this
                .Icons.Select(
                    (icon, index) =>
                    {
                        var component = icon.GetComponent(IconStyle.Transparent);

                        component.baseScale = component.scale = (float)Math.Floor(
                            (float)this.length / Math.Max(component.sourceRect.Width, component.sourceRect.Height));

                        component.bounds = new Rectangle(
                            this.xPositionOnScreen
                            + (index % this.columns * (this.length + this.spacing))
                            + (int)((this.length - (component.sourceRect.Width * component.scale)) / 2f)
                            + this.spacing,
                            this.yPositionOnScreen
                            + (index / this.columns * (this.length + this.spacing))
                            + (int)((this.length - (component.sourceRect.Height * component.scale)) / 2f)
                            + this.spacing,
                            this.length,
                            this.length);

                        component.hoverText = this.getHoverText(icon);
                        component.name = index.ToString(CultureInfo.InvariantCulture);

                        return component;
                    })
                .ToList();

            this.MaxOffset = this.components.Last().bounds.Bottom - this.yPositionOnScreen - this.height + this.spacing;
            if (this.MaxOffset <= 0)
            {
                this.MaxOffset = -1;
            }

            return this.components;
        }
    }

    /// <summary>Add a highlight operation that will be applied to the items.</summary>
    /// <param name="highlight">The highlight operation.</param>
    public void AddHighlight(Highlight highlight) => this.highlights.Add(highlight);

    /// <summary>Add an operation that will be applied to the icons.</summary>
    /// <param name="operation">The operation to perform.</param>
    public void AddOperation(Operation operation) => this.operations.Add(operation);

    /// <inheritdoc />
    public override void MoveTo(int x, int y)
    {
        base.MoveTo(x, y);
        this.RefreshIcons();
    }

    /// <summary>Refreshes the icons by applying the operations to them.</summary>
    public void RefreshIcons()
    {
        this.components = null;
        this.icons = null;
    }

    /// <inheritdoc />
    protected override void Draw(SpriteBatch spriteBatch)
    {
        // Draw items
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.DrawInFrame(
            spriteBatch,
            SpriteSortMode.Deferred,
            () =>
            {
                foreach (var component in this.Components)
                {
                    var index = int.Parse(component.name, CultureInfo.InvariantCulture);
                    var icon = this.Icons[index];
                    component.tryHover(mouseX, mouseY + this.Offset, 0.2f);

                    if (index == this.CurrentIndex)
                    {
                        spriteBatch.Draw(
                            Game1.mouseCursors,
                            new Rectangle(
                                this.xPositionOnScreen
                                + (index % this.columns * (this.length + this.spacing))
                                + this.spacing,
                                this.yPositionOnScreen
                                + (index / this.columns * (this.length + this.spacing))
                                - this.Offset
                                + this.spacing,
                                this.length,
                                this.length),
                            new Rectangle(194, 388, 16, 16),
                            Color.White,
                            0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            0.975f);
                    }

                    component.draw(
                        spriteBatch,
                        this.HighlightIcon(icon) ? Color.White : Color.White * 0.25f,
                        1f,
                        0,
                        0,
                        -this.Offset);

                    if (component.containsPoint(mouseX, mouseY + this.Offset))
                    {
                        this.HoverText ??= component.hoverText;
                    }
                }
            });
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch b) =>
        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            OptionsDropDown.dropDownBGSource,
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            this.height,
            Color.White,
            Game1.pixelZoom,
            false,
            0.97f);

    /// <inheritdoc />
    protected override bool TryLeftClick(int x, int y)
    {
        var component = this.Components.FirstOrDefault(i => i.bounds.Contains(x, y + this.Offset));
        if (component is null)
        {
            return false;
        }

        this.CurrentIndex = int.Parse(component.name, CultureInfo.InvariantCulture);
        return true;
    }

    private static string GetUniqueId(IIcon icon) => icon.UniqueId;

    private bool HighlightIcon(IIcon icon) =>
        !this.highlights.Any() || this.highlights.All(highlight => highlight(icon));
}