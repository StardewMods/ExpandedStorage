namespace StardewMods.Common.UI.Menus;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>Menu for selecting an item from a list of values.</summary>
internal class SelectOption : FramedMenu
{
    private readonly Action<string?> callback;
    private readonly List<ClickableComponent> components;
    private readonly List<Func<KeyValuePair<string, string>, bool>> highlights = [];
    private readonly List<KeyValuePair<string, string>> items;

    private readonly List<Func<IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>>>
        operations = [];

    private int currentIndex = -1;
    private List<KeyValuePair<string, string>>? options;

    /// <summary>Initializes a new instance of the <see cref="SelectOption" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="items">The list of values to select from.</param>
    /// <param name="callback">A method that is called when an option is selected.</param>
    /// <param name="x">The x-position.</param>
    /// <param name="y">The y-position.</param>
    /// <param name="minWidth">The minimum width.</param>
    /// <param name="maxWidth">The maximum width.</param>
    /// <param name="maxItems">The maximum number of items to display.</param>
    public SelectOption(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        IReadOnlyCollection<KeyValuePair<string, string>> items,
        Action<string?> callback,
        int x,
        int y,
        int minWidth = 0,
        int maxWidth = int.MaxValue,
        int maxItems = int.MaxValue)
        : base(inputHelper, reflectionHelper, x, y)
    {
        this.callback = callback;
        this.items = items
            .Where(
                item => item.Value.Trim().Length >= 3
                    && !item.Value.StartsWith("id_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var textBounds = items.Select(item => Game1.smallFont.MeasureString(item.Value).ToPoint()).ToList();
        var textHeight = textBounds.Max(textBound => textBound.Y);

        this.Resize(
            Math.Max(minWidth, Math.Min(maxWidth, textBounds.Max(textBound => textBound.X) + 16)),
            textBounds.Take(maxItems).Sum(textBound => textBound.Y) + 16);

        this.components = items
            .Take(maxItems)
            .Select(
                (_, index) => new ClickableComponent(
                    new Rectangle(
                        this.xPositionOnScreen + 8,
                        this.yPositionOnScreen + (textHeight * index) + 8,
                        this.width,
                        textHeight),
                    index.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        this.allClickableComponents.AddRange(this.components);

        this.MaxOffset = this.items.Count - this.components.Count;
        if (this.MaxOffset <= 0)
        {
            this.MaxOffset = -1;
        }
    }

    /// <summary>Gets the currently selected option.</summary>
    public string? CurrentSelection => this.Options.ElementAtOrDefault(this.CurrentIndex).Key;

    /// <summary>Gets the options.</summary>
    public virtual IEnumerable<KeyValuePair<string, string>> Options =>
        this.options ??=
            this.operations.Aggregate(this.items.AsEnumerable(), (current, operation) => operation(current)).ToList();

    /// <summary>Gets or sets the current index.</summary>
    public int CurrentIndex
    {
        get => this.currentIndex;
        protected set
        {
            this.currentIndex = value;
            this.callback(this.CurrentSelection);
        }
    }

    /// <inheritdoc />
    protected override Rectangle Frame => new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);

    /// <summary>Add a highlight operation that will be applied to the items.</summary>
    /// <param name="highlight">The highlight operation.</param>
    public void AddHighlight(Func<KeyValuePair<string, string>, bool> highlight) => this.highlights.Add(highlight);

    /// <summary>Add an operation that will be applied to the items.</summary>
    /// <param name="operation">The operation to perform.</param>
    public void AddOperation(
        Func<IEnumerable<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>> operation) =>
        this.operations.Add(operation);

    /// <summary>Refreshes the items by applying the operations to them.</summary>
    public void RefreshOptions() => this.options = null;

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
                foreach (var component in this.components)
                {
                    var index = this.Offset + int.Parse(component.name, CultureInfo.InvariantCulture);
                    var item = this.Options.ElementAtOrDefault(index);
                    if (component.bounds.Contains(mouseX, mouseY))
                    {
                        spriteBatch.Draw(
                            Game1.staminaRect,
                            component.bounds with { Width = component.bounds.Width - 16 },
                            new Rectangle(0, 0, 1, 1),
                            Color.Wheat,
                            0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            0.975f);
                    }

                    spriteBatch.DrawString(
                        Game1.smallFont,
                        item.Value,
                        new Vector2(component.bounds.X, component.bounds.Y),
                        !this.highlights.Any() || this.highlights.Any(highlight => highlight(item))
                            ? Game1.textColor
                            : Game1.unselectedOptionColor);
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
        var component = this.components.FirstOrDefault(i => i.bounds.Contains(x, y));
        if (component is null)
        {
            return false;
        }

        this.CurrentIndex = this.Offset + int.Parse(component.name, CultureInfo.InvariantCulture);
        return true;
    }
}