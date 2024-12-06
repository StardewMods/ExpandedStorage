#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Common.UI.Components;
using StardewValley.Menus;

#else

namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

#endif

/// <summary>Popup menu for selecting an item from a list of values.</summary>
/// <typeparam name="TItem">The item type.</typeparam>
internal sealed class PopupSelect<TItem> : BaseMenu
{
    private readonly ClickableTextureComponent cancelButton;
    private readonly ClickableTextureComponent okButton;
    private readonly SelectOption<TItem> selectOption;
    private readonly TextField textField;

    private string currentText;
    private EventHandler<TItem?>? optionSelected;

    /// <summary>Initializes a new instance of the <see cref="PopupSelect{TItem}" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="items">The list of values to select from.</param>
    /// <param name="initialValue">The initial value of the text field.</param>
    /// <param name="getValue">A function which returns a string from the item.</param>
    /// <param name="maxItems">The maximum number of items to display.</param>
    public PopupSelect(
        IIconRegistry iconRegistry,
        IEnumerable<TItem> items,
        string? initialValue,
        Func<TItem, string>? getValue = null,
        int maxItems = int.MaxValue)
        : base(width: 400)
    {
        this.selectOption = new SelectOption<TItem>(
            items,
            this.xPositionOnScreen,
            this.yPositionOnScreen + 48,
            getValue,
            400,
            400,
            maxItems);

        this.selectOption.SelectionChanged += this.OnSelectionChanged;
        this.selectOption.AddHighlight(this.HighlightOption);
        this.selectOption.AddOperation(this.SortOptions);
        this
            .AddSubMenu(this.selectOption)
            .ResizeTo(new Point(this.selectOption.width + 16, this.selectOption.height + 16))
            .MoveTo(
                new Point(
                    ((Game1.uiViewport.Width - this.width) / 2) + IClickableMenu.borderWidth,
                    ((Game1.uiViewport.Height - this.height) / 2) + IClickableMenu.borderWidth));

        this.currentText = initialValue ?? string.Empty;
        this.textField = new TextField(
            this,
            this.xPositionOnScreen - 12,
            this.yPositionOnScreen,
            this.width,
            () => this.CurrentText,
            value => this.CurrentText = value)
        {
            Selected = true,
        };

        this.okButton = iconRegistry
            .Icon(VanillaIcon.Ok)
            .Component(
                IconStyle.Transparent,
                this.xPositionOnScreen + ((this.width - IClickableMenu.borderWidth) / 2) - Game1.tileSize,
                this.yPositionOnScreen + this.height + Game1.tileSize);

        this.cancelButton = iconRegistry
            .Icon(VanillaIcon.Cancel)
            .Component(
                IconStyle.Transparent,
                this.xPositionOnScreen + ((this.width + IClickableMenu.borderWidth) / 2),
                this.yPositionOnScreen + this.height + Game1.tileSize);

        this.allClickableComponents.Add(this.textField);
        this.allClickableComponents.Add(this.okButton);
        this.allClickableComponents.Add(this.cancelButton);
    }

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<TItem?> OptionSelected
    {
        add => this.optionSelected += value;
        remove => this.optionSelected -= value;
    }

    /// <summary>Gets or sets the current text.</summary>
    public string CurrentText
    {
        get => this.currentText;
        set
        {
            if (this.currentText.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                this.currentText = value;
                return;
            }

            this.currentText = value;
            this.selectOption.RefreshOptions();
        }
    }

    /// <inheritdoc />
    public override void DrawUnder(SpriteBatch spriteBatch, Point cursor) =>
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        switch (key)
        {
            case Keys.Escape when this.readyToClose():
                this.exitThisMenuNoSound();
                return;

            case Keys.Enter when this.readyToClose():
                this.optionSelected?.InvokeAll(this, this.selectOption.CurrentSelection);
                this.exitThisMenuNoSound();
                return;

            case Keys.Tab when this.textField.Selected
                && !string.IsNullOrWhiteSpace(this.CurrentText)
                && this.selectOption.Options.Any():
                this.CurrentText = this.selectOption.GetValue(this.selectOption.Options[0]);
                this.textField.Reset();
                break;
        }
    }

    /// <inheritdoc />
    public override bool TryLeftClick(Point cursor)
    {
        if (this.okButton.bounds.Contains(cursor) && this.readyToClose())
        {
            this.optionSelected?.InvokeAll(this, this.selectOption.CurrentSelection);
            this.exitThisMenuNoSound();
            return true;
        }

        if (this.cancelButton.bounds.Contains(cursor) && this.readyToClose())
        {
            this.exitThisMenuNoSound();
            return true;
        }

        return false;
    }

    private bool HighlightOption(TItem option) =>
        this.selectOption.GetValue(option).Contains(this.CurrentText, StringComparison.OrdinalIgnoreCase);

    private void OnSelectionChanged(object? sender, TItem? item)
    {
        this.CurrentText = item is not null ? this.selectOption.GetValue(item) : string.Empty;
        this.textField.Reset();
    }

    private IEnumerable<TItem> SortOptions(IEnumerable<TItem> options) =>
        options.OrderByDescending(this.HighlightOption).ThenBy(this.selectOption.GetValue);
}