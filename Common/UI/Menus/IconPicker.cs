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

/// <summary>Popup menu for selecting an icon.</summary>
internal sealed class IconPicker : BaseMenu
{
    private readonly ClickableTextureComponent cancelButton;
    private readonly ClickableTextureComponent dropdown;
    private readonly IInputHelper inputHelper;
    private readonly ClickableTextureComponent okButton;
    private readonly IReflectionHelper reflectionHelper;
    private readonly SelectIcon selectIcon;
    private readonly List<string> sources;
    private readonly TextField textField;

    private string currentText;
    private EventHandler<IIcon?>? iconSelected;

    /// <summary>Initializes a new instance of the <see cref="IconPicker" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public IconPicker(IIconRegistry iconRegistry, IInputHelper inputHelper, IReflectionHelper reflectionHelper)
        : base(inputHelper, width: 400)
    {
        this.inputHelper = inputHelper;
        this.reflectionHelper = reflectionHelper;
        var icons = iconRegistry.GetIcons().ToList();
        this.sources = icons.Select(icon => icon.Source).Distinct().ToList();
        this.sources.Sort();

        this.selectIcon = new SelectIcon(
            inputHelper,
            reflectionHelper,
            icons,
            5,
            5,
            x: this.xPositionOnScreen,
            y: this.yPositionOnScreen + 48);

        this.selectIcon.AddOperation(this.FilterIcons);
        this.AddSubMenu(this.selectIcon);
        this.Resize(this.selectIcon.width + 16, this.selectIcon.height + 16);
        this.MoveTo(
            ((Game1.uiViewport.Width - this.width) / 2) + IClickableMenu.borderWidth,
            ((Game1.uiViewport.Height - this.height) / 2) + IClickableMenu.borderWidth);

        this.currentText = string.Empty;
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

        this.dropdown = iconRegistry
            .RequireIcon(VanillaIcon.Dropdown)
            .GetComponent(IconStyle.Transparent, this.xPositionOnScreen + this.width - 16, this.yPositionOnScreen);

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
        this.allClickableComponents.Add(this.dropdown);
        this.allClickableComponents.Add(this.okButton);
        this.allClickableComponents.Add(this.cancelButton);
    }

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<IIcon?> IconSelected
    {
        add => this.iconSelected += value;
        remove => this.iconSelected -= value;
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
            this.selectIcon.RefreshIcons();
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
            case Keys.Enter when this.readyToClose() && this.selectIcon.CurrentSelection is not null:
                this.iconSelected?.InvokeAll(this, this.selectIcon.CurrentSelection);
                this.exitThisMenuNoSound();
                return;
            case Keys.Tab when this.textField.Selected && !string.IsNullOrWhiteSpace(this.CurrentText):
                this.CurrentText =
                    this.sources.FirstOrDefault(
                        source => source.Contains(this.CurrentText, StringComparison.OrdinalIgnoreCase))
                    ?? this.CurrentText;

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
            if (this.selectIcon.CurrentSelection is not null)
            {
                this.iconSelected?.InvokeAll(this, this.selectIcon.CurrentSelection);
            }

            this.exitThisMenuNoSound();
            return true;
        }

        if (this.cancelButton.containsPoint(x, y) && this.readyToClose())
        {
            this.exitThisMenuNoSound();
            return true;
        }

        if (this.dropdown.containsPoint(x, y))
        {
            var sourceDropdown = new Dropdown<string>(
                this.inputHelper,
                this.reflectionHelper,
                this.textField,
                this.sources,
                minWidth: this.width,
                maxItems: 10);

            sourceDropdown.OptionSelected += (_, value) =>
            {
                this.CurrentText = value ?? this.CurrentText;
                this.textField.Reset();
            };

            this.SetChildMenu(sourceDropdown);
            return true;
        }

        return false;
    }

    private IEnumerable<IIcon> FilterIcons(IEnumerable<IIcon> icons) =>
        string.IsNullOrWhiteSpace(this.CurrentText)
            ? icons
            : icons.Where(icon => icon.Source.Contains(this.CurrentText, StringComparison.OrdinalIgnoreCase));
}