namespace StardewMods.ToolbarIcons.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

/// <summary>Represents a complex menu option for arranging toolbar icons.</summary>
internal sealed class ToolbarIconOption : BaseComplexOption
{
    private static string? hoverText;

    private readonly IIcon checkedIcon;
    private readonly IIcon downArrowIcon;
    private readonly Func<string> getCurrentId;
    private readonly Func<bool> getEnabled;
    private readonly Func<string> getTooltip;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly Action? moveDown;
    private readonly Action? moveUp;
    private readonly Action<bool> setEnabled;
    private readonly IIcon uncheckedIcon;
    private readonly IIcon upArrowIcon;
    private ClickableTextureComponent? checkedComponent;

    private string currentId;
    private ClickableTextureComponent? downArrow;
    private ClickableTextureComponent icon;
    private string name;
    private ClickableTextureComponent? uncheckedComponent;
    private ClickableTextureComponent? upArrow;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconOption" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="getCurrentId">Function which returns the current id.</param>
    /// <param name="getTooltip">Function which returns the tooltip.</param>
    /// <param name="getEnabled">Function which returns if icon is enabled.</param>
    /// <param name="setEnabled">Action which sets if the icon is enabled.</param>
    /// <param name="moveDown">Action to perform when down is pressed.</param>
    /// <param name="moveUp">Action to perform when up is pressed.</param>
    public ToolbarIconOption(
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        Func<string> getCurrentId,
        Func<string> getTooltip,
        Func<bool> getEnabled,
        Action<bool> setEnabled,
        Action? moveDown,
        Action? moveUp)
    {
        this.Height = Game1.tileSize;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.getCurrentId = getCurrentId;
        this.getTooltip = getTooltip;
        this.getEnabled = getEnabled;
        this.setEnabled = setEnabled;
        this.moveDown = moveDown;
        this.moveUp = moveUp;
        this.downArrowIcon = iconRegistry.Icon(VanillaIcon.ArrowDown);
        this.upArrowIcon = iconRegistry.Icon(VanillaIcon.ArrowUp);
        this.checkedIcon = iconRegistry.Icon(VanillaIcon.Checked);
        this.uncheckedIcon = iconRegistry.Icon(VanillaIcon.Unchecked);
        this.UpdateId();
    }

    /// <inheritdoc />
    public override string Name => string.Empty;

    /// <inheritdoc />
    public override string Tooltip => string.Empty;

    /// <inheritdoc />
    public override int Height { get; protected set; }

    private ClickableTextureComponent CheckedIcon
    {
        get
        {
            if (this.checkedComponent is not null)
            {
                return this.checkedComponent;
            }

            this.checkedComponent = this.checkedIcon.Component(IconStyle.Transparent);
            this.checkedComponent.hoverText = I18n.Config_CheckBox_Tooltip();
            return this.checkedComponent;
        }
    }

    private ClickableTextureComponent DownArrow
    {
        get
        {
            if (this.downArrow is not null)
            {
                return this.downArrow;
            }

            this.downArrow = this.downArrowIcon.Component(IconStyle.Transparent);
            this.downArrow.hoverText = I18n.Config_MoveDown_Tooltip();
            return this.downArrow;
        }
    }

    private ClickableTextureComponent UncheckedIcon
    {
        get
        {
            if (this.uncheckedComponent is not null)
            {
                return this.uncheckedComponent;
            }

            this.uncheckedComponent = this.uncheckedIcon.Component(IconStyle.Transparent);
            this.uncheckedComponent.hoverText = I18n.Config_CheckBox_Tooltip();
            return this.uncheckedComponent;
        }
    }

    private ClickableTextureComponent UpArrow
    {
        get
        {
            if (this.upArrow is not null)
            {
                return this.upArrow;
            }

            this.upArrow = this.upArrowIcon.Component(IconStyle.Transparent);
            this.upArrow.hoverText = I18n.Config_MoveUp_Tooltip();
            return this.upArrow;
        }
    }

    private bool Enabled
    {
        get => this.getEnabled();
        set => this.setEnabled(value);
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        var (mouseX, mouseY) = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        var mouseLeft = this.inputHelper.GetState(SButton.MouseLeft) == SButtonState.Pressed;
        var mouseRight = this.inputHelper.GetState(SButton.MouseRight) == SButtonState.Pressed;
        var hoverY = mouseY >= pos.Y && mouseY < pos.Y + this.Height;
        var clicked = (mouseLeft || mouseRight) && hoverY;

        if (this.currentId != this.getCurrentId())
        {
            this.UpdateId();
        }

        Utility.drawTextWithShadow(
            spriteBatch,
            this.icon.hoverText,
            Game1.dialogueFont,
            pos - new Vector2(540, 0),
            SpriteText.color_Gray);

        // Checkbox
        var checkbox = this.Enabled ? this.CheckedIcon : this.UncheckedIcon;
        checkbox.bounds.X = (int)pos.X + Game1.tileSize;
        checkbox.bounds.Y = (int)pos.Y;
        checkbox.tryHover(mouseX, mouseY);
        checkbox.draw(spriteBatch);

        if (checkbox.containsPoint(mouseX, mouseY))
        {
            ToolbarIconOption.hoverText = checkbox.hoverText;
            if (clicked)
            {
                checkbox.scale = 3.5f;
                this.Enabled = !this.Enabled;
                Game1.playSound("drumkit6");
            }
        }
        else if ((mouseX < checkbox.bounds.Left || mouseX > checkbox.bounds.Right)
            && ToolbarIconOption.hoverText == checkbox.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        // Up Arrow
        this.UpArrow.bounds.X = (int)pos.X + (Game1.tileSize * 2);
        this.UpArrow.bounds.Y = (int)pos.Y;

        if (this.moveUp is not null && this.UpArrow.containsPoint(mouseX, mouseY))
        {
            this.UpArrow.tryHover(mouseX, mouseY);
            ToolbarIconOption.hoverText = this.UpArrow.hoverText;
            if (clicked)
            {
                this.UpArrow.scale = 3.5f;
                this.moveUp();
                Game1.playSound("shwip");
            }
        }
        else if ((mouseX < this.UpArrow.bounds.Left || mouseX > this.UpArrow.bounds.Right)
            && ToolbarIconOption.hoverText == this.UpArrow.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.UpArrow.draw(spriteBatch, this.moveUp is not null ? Color.White : Color.Black * 0.35f, 1f);

        // Icon
        this.icon.bounds.X = (int)pos.X + (Game1.tileSize * 3);
        this.icon.bounds.Y = (int)pos.Y;

        if (this.Enabled && this.icon.containsPoint(mouseX, mouseY))
        {
            this.icon.tryHover(mouseX, mouseY);
            ToolbarIconOption.hoverText = this.name;
        }
        else if (ToolbarIconOption.hoverText == this.name)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.icon.draw(spriteBatch, this.Enabled ? Color.White : Color.Black * 0.35f, 1f);

        // Down Arrow
        this.DownArrow.bounds.X = (int)pos.X + (Game1.tileSize * 4);
        this.DownArrow.bounds.Y = (int)pos.Y;

        if (this.moveDown is not null && this.DownArrow.containsPoint(mouseX, mouseY))
        {
            this.DownArrow.tryHover(mouseX, mouseY);
            ToolbarIconOption.hoverText = this.DownArrow.hoverText;
            if (clicked)
            {
                this.DownArrow.scale = 3.5f;
                this.moveDown();
                Game1.playSound("shwip");
            }
        }
        else if ((mouseX < this.DownArrow.bounds.Left || mouseX > this.DownArrow.bounds.Right)
            && ToolbarIconOption.hoverText == this.DownArrow.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.DownArrow.draw(spriteBatch, this.moveDown is not null ? Color.White : Color.Black * 0.35f, 1f);

        if (!string.IsNullOrWhiteSpace(ToolbarIconOption.hoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, ToolbarIconOption.hoverText, null, null);
        }
    }

    [MemberNotNull(nameof(ToolbarIconOption.currentId), nameof(ToolbarIconOption.name), nameof(ToolbarIconOption.icon))]
    private void UpdateId()
    {
        this.currentId = this.getCurrentId();
        this.name = this.currentId.Split('/')[^1];
        this.icon = this.iconRegistry.Icon(this.currentId).Component(IconStyle.Button, scale: 3f);
        this.icon.hoverText = this.getTooltip();
    }
}