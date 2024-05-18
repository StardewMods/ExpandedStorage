namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>Popup menu for selecting an item from a list of values.</summary>
internal sealed class PopupSelect : BaseMenu
{
    private readonly Action<string> callback;
    private readonly ClickableTextureComponent cancelButton;
    private readonly ClickableTextureComponent okButton;
    private readonly SelectOption selectOption;
    private readonly TextField textField;

    private string currentText;

    /// <summary>Initializes a new instance of the <see cref="PopupSelect" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="items">The list of values to select from.</param>
    /// <param name="initialValue">The initial value of the text field.</param>
    /// <param name="callback">A method that is called when an option is selected.</param>
    /// <param name="maxItems">The maximum number of items to display.</param>
    public PopupSelect(
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        IReadOnlyCollection<KeyValuePair<string, string>> items,
        string? initialValue,
        Action<string> callback,
        int maxItems = int.MaxValue)
        : base(inputHelper, width: 400)
    {
        this.callback = callback;
        this.selectOption = new SelectOption(
            inputHelper,
            reflectionHelper,
            items,
            this.OnSelect,
            this.xPositionOnScreen,
            this.yPositionOnScreen + 48,
            400,
            400,
            maxItems);

        this.selectOption.AddHighlight(this.HighlightOption);
        this.selectOption.AddOperation(this.SortOptions);
        this.AddSubMenu(this.selectOption);
        this.Resize(this.selectOption.width + 16, this.selectOption.height + 16);
        this.MoveTo(
            ((Game1.uiViewport.Width - this.width) / 2) + IClickableMenu.borderWidth,
            ((Game1.uiViewport.Height - this.height) / 2) + IClickableMenu.borderWidth);

        this.currentText = initialValue ?? string.Empty;
        this.textField = new TextField(
            this.Input,
            this.xPositionOnScreen - 12,
            this.yPositionOnScreen,
            this.width,
            () => this.CurrentText,
            value => this.CurrentText = value)
        {
            Selected = true,
        };

        this.okButton = iconRegistry
            .RequireIcon(VanillaIcon.Ok)
            .GetComponent(
                IconStyle.Transparent,
                this.xPositionOnScreen + ((this.width - IClickableMenu.borderWidth) / 2) - Game1.tileSize,
                this.yPositionOnScreen + this.height + Game1.tileSize);

        this.cancelButton = iconRegistry
            .RequireIcon(VanillaIcon.Cancel)
            .GetComponent(
                IconStyle.Transparent,
                this.xPositionOnScreen + ((this.width + IClickableMenu.borderWidth) / 2),
                this.yPositionOnScreen + this.height + Game1.tileSize);

        this.allClickableComponents.Add(this.textField);
        this.allClickableComponents.Add(this.okButton);
        this.allClickableComponents.Add(this.cancelButton);
    }

    /// <summary>Gets or sets the current text.</summary>
    public string CurrentText
    {
        get => this.currentText;
        set
        {
            this.currentText = value;
            this.selectOption.RefreshOptions();
        }
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        switch (key)
        {
            case Keys.Escape when this.readyToClose():
                this.exitThisMenuNoSound();
                return;
            case Keys.Enter when this.readyToClose():
                this.callback(this.CurrentText);
                this.exitThisMenuNoSound();
                return;
            case Keys.Tab when this.textField.Selected
                && !string.IsNullOrWhiteSpace(this.CurrentText)
                && this.selectOption.Options.Any():
                this.CurrentText = this.selectOption.Options.First().Value;
                this.textField.Reset();
                break;
        }
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);

    /// <inheritdoc />
    protected override bool TryLeftClick(int x, int y)
    {
        if (this.okButton.containsPoint(x, y) && this.readyToClose())
        {
            this.callback(this.CurrentText);
            this.exitThisMenuNoSound();
            return true;
        }

        if (this.cancelButton.containsPoint(x, y) && this.readyToClose())
        {
            this.exitThisMenuNoSound();
            return true;
        }

        return false;
    }

    private bool HighlightOption(KeyValuePair<string, string> option) =>
        option.Value.Contains(this.CurrentText, StringComparison.OrdinalIgnoreCase);

    private void OnSelect(string? value)
    {
        this.CurrentText = value ?? string.Empty;
        this.textField.Reset();
    }

    private IEnumerable<KeyValuePair<string, string>> SortOptions(IEnumerable<KeyValuePair<string, string>> options) =>
        options.OrderByDescending(this.HighlightOption).ThenBy(option => option.Value);
}