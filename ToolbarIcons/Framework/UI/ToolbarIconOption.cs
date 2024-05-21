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

    private readonly ClickableTextureComponent checkedIcon;
    private readonly ClickableTextureComponent downArrow;
    private readonly Func<string> getCurrentId;
    private readonly Func<bool> getEnabled;
    private readonly Func<string> getTooltip;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly Action? moveDown;
    private readonly Action? moveUp;
    private readonly Action<bool> setEnabled;
    private readonly ClickableTextureComponent uncheckedIcon;
    private readonly ClickableTextureComponent upArrow;

    private string currentId;
    private ClickableTextureComponent icon;
    private string name;

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
        this.downArrow = iconRegistry.RequireIcon(VanillaIcon.ArrowDown).GetComponent(IconStyle.Transparent);
        this.upArrow = iconRegistry.RequireIcon(VanillaIcon.ArrowUp).GetComponent(IconStyle.Transparent);
        this.checkedIcon = iconRegistry.RequireIcon(VanillaIcon.Checked).GetComponent(IconStyle.Transparent);
        this.uncheckedIcon = iconRegistry.RequireIcon(VanillaIcon.Unchecked).GetComponent(IconStyle.Transparent);

        this.downArrow.hoverText = I18n.Config_MoveDown_Tooltip();
        this.upArrow.hoverText = I18n.Config_MoveUp_Tooltip();
        this.checkedIcon.hoverText = I18n.Config_CheckBox_Tooltip();
        this.uncheckedIcon.hoverText = I18n.Config_CheckBox_Tooltip();

        this.UpdateId();
    }

    /// <inheritdoc />
    public override string Name => string.Empty;

    /// <inheritdoc />
    public override string Tooltip => string.Empty;

    /// <inheritdoc />
    public override int Height { get; protected set; }

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
            this.name,
            Game1.dialogueFont,
            pos - new Vector2(540, 0),
            SpriteText.color_Gray);

        // Checkbox
        var checkbox = this.Enabled ? this.checkedIcon : this.uncheckedIcon;
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
        this.upArrow.bounds.X = (int)pos.X + (Game1.tileSize * 2);
        this.upArrow.bounds.Y = (int)pos.Y;

        if (this.moveUp is not null && this.upArrow.containsPoint(mouseX, mouseY))
        {
            this.upArrow.tryHover(mouseX, mouseY);
            ToolbarIconOption.hoverText = this.upArrow.hoverText;
            if (clicked)
            {
                this.upArrow.scale = 3.5f;
                this.moveUp();
                Game1.playSound("shwip");
            }
        }
        else if ((mouseX < this.upArrow.bounds.Left || mouseX > this.upArrow.bounds.Right)
            && ToolbarIconOption.hoverText == this.upArrow.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.upArrow.draw(spriteBatch, this.moveUp is not null ? Color.White : Color.Black * 0.35f, 1f);

        // Icon
        this.icon.bounds.X = (int)pos.X + (Game1.tileSize * 3);
        this.icon.bounds.Y = (int)pos.Y;

        if (this.Enabled && this.icon.containsPoint(mouseX, mouseY))
        {
            this.icon.tryHover(mouseX, mouseY);
            ToolbarIconOption.hoverText = this.icon.hoverText;
        }
        else if (ToolbarIconOption.hoverText == this.icon.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.icon.draw(spriteBatch, this.Enabled ? Color.White : Color.Black * 0.35f, 1f);

        // Down Arrow
        this.downArrow.bounds.X = (int)pos.X + (Game1.tileSize * 4);
        this.downArrow.bounds.Y = (int)pos.Y;

        if (this.moveDown is not null && this.downArrow.containsPoint(mouseX, mouseY))
        {
            this.downArrow.tryHover(mouseX, mouseY);
            ToolbarIconOption.hoverText = this.downArrow.hoverText;
            if (clicked)
            {
                this.downArrow.scale = 3.5f;
                this.moveDown();
                Game1.playSound("shwip");
            }
        }
        else if ((mouseX < this.downArrow.bounds.Left || mouseX > this.downArrow.bounds.Right)
            && ToolbarIconOption.hoverText == this.downArrow.hoverText)
        {
            ToolbarIconOption.hoverText = null;
        }

        this.downArrow.draw(spriteBatch, this.moveDown is not null ? Color.White : Color.Black * 0.35f, 1f);

        if (!string.IsNullOrWhiteSpace(ToolbarIconOption.hoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, ToolbarIconOption.hoverText, string.Empty, null);
        }
    }

    [MemberNotNull(nameof(ToolbarIconOption.currentId), nameof(ToolbarIconOption.name), nameof(ToolbarIconOption.icon))]
    private void UpdateId()
    {
        this.currentId = this.getCurrentId();
        this.name = this.currentId.Split('/')[^1];
        this.icon = this.iconRegistry.RequireIcon(this.currentId).GetComponent(IconStyle.Button, scale: 3f);

        this.icon.hoverText = this.getTooltip();
    }
}