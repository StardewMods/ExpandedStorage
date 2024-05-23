#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewValley.Menus;
#endif

/// <summary>Menu for selecting an item from a list of values.</summary>
/// <typeparam name="TItem">The item type.</typeparam>
internal sealed class SelectOption<TItem> : FramedMenu
{
    private readonly List<ClickableComponent> components;
    private readonly Func<TItem, string> getValue;
    private readonly List<Highlight> highlights = [];
    private readonly List<TItem> items;
    private readonly List<Operation> operations = [];

    private int currentIndex = -1;
    private List<TItem>? options;
    private EventHandler<TItem?>? selectionChanged;

    /// <summary>Initializes a new instance of the <see cref="SelectOption{TItem}" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="items">The list of values to select from.</param>
    /// <param name="getValue">A function which returns a string from the item.</param>
    /// <param name="x">The x-position.</param>
    /// <param name="y">The y-position.</param>
    /// <param name="minWidth">The minimum width.</param>
    /// <param name="maxWidth">The maximum width.</param>
    /// <param name="maxItems">The maximum number of items to display.</param>
    public SelectOption(
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        IEnumerable<TItem> items,
        int x,
        int y,
        Func<TItem, string>? getValue = null,
        int minWidth = 0,
        int maxWidth = int.MaxValue,
        int maxItems = int.MaxValue)
        : base(inputHelper, reflectionHelper, x, y)
    {
        this.getValue = getValue ?? SelectOption<TItem>.GetDefaultValue;
        this.items = items
            .Where(
                item => this.GetValue(item).Trim().Length >= 3
                    && !this.GetValue(item).StartsWith("id_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var textBounds =
            this.items.Select(item => Game1.smallFont.MeasureString(this.GetValue(item)).ToPoint()).ToList();

        var textHeight = textBounds.Max(textBound => textBound.Y);

        this.Resize(
            Math.Max(minWidth, Math.Min(maxWidth, textBounds.Max(textBound => textBound.X) + 16)),
            textBounds.Take(maxItems).Sum(textBound => textBound.Y) + 16);

        this.components = this
            .items.Take(maxItems)
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

    /// <summary>Highlight an option.</summary>
    /// <param name="option">The option to highlight.</param>
    /// <returns><c>true</c> if the option should be highlighted; otherwise, <c>false</c>.</returns>
    public delegate bool Highlight(TItem option);

    /// <summary>An operation to perform on the options.</summary>
    /// <param name="options">The original options.</param>
    /// <returns>Returns a modified list of options.</returns>
    public delegate IEnumerable<TItem> Operation(IEnumerable<TItem> options);

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<TItem?> SelectionChanged
    {
        add => this.selectionChanged += value;
        remove => this.selectionChanged -= value;
    }

    /// <summary>Gets the currently selected option.</summary>
    public TItem? CurrentSelection => this.Options.ElementAtOrDefault(this.CurrentIndex);

    /// <summary>Gets the options.</summary>
    public List<TItem> Options =>
        this.options ??=
            this.operations.Aggregate(this.items.AsEnumerable(), (current, operation) => operation(current)).ToList();

    /// <summary>Gets the current index.</summary>
    public int CurrentIndex
    {
        get => this.currentIndex;
        private set
        {
            this.currentIndex = value;
            this.selectionChanged?.InvokeAll(this, this.CurrentSelection);
        }
    }

    /// <inheritdoc />
    protected override Rectangle Frame => new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);

    /// <summary>Add a highlight operation that will be applied to the items.</summary>
    /// <param name="highlight">The highlight operation.</param>
    public void AddHighlight(Highlight highlight) => this.highlights.Add(highlight);

    /// <summary>Add an operation that will be applied to the items.</summary>
    /// <param name="operation">The operation to perform.</param>
    public void AddOperation(Operation operation) => this.operations.Add(operation);

    /// <summary>Gets the text value for an item.</summary>
    /// <param name="item">The item to get the value from.</param>
    /// <returns>The text value of the item.</returns>
    public string GetValue(TItem item) => this.getValue(item);

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
                    if (item is null)
                    {
                        continue;
                    }

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
                        this.getValue(item),
                        new Vector2(component.bounds.X, component.bounds.Y),
                        this.HighlightOption(item) ? Game1.textColor : Game1.unselectedOptionColor);
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

    private static string GetDefaultValue(TItem item) =>
        item switch { string s => s, _ => item?.ToString() ?? string.Empty };

    private bool HighlightOption(TItem option) =>
        !this.highlights.Any() || this.highlights.All(highlight => highlight(option));
}