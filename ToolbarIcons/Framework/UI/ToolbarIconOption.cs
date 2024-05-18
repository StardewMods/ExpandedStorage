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
    private readonly IIcon checkedIcon;
    private readonly IIcon downArrow;
    private readonly Func<string> getCurrentId;
    private readonly Func<bool> getEnabled;
    private readonly IIconRegistry iconRegistry;
    private readonly Dictionary<string, string?> icons;
    private readonly IInputHelper inputHelper;
    private readonly Action? moveDown;
    private readonly Action? moveUp;
    private readonly Action<bool> setEnabled;
    private readonly IIcon uncheckedIcon;
    private readonly IIcon upArrow;

    private string currentId;
    private IIcon icon;
    private string name;
    private string tooltip;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconOption" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="icons">Dictionary containing all added icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="getCurrentId">Function which returns the current id.</param>
    /// <param name="getEnabled">Function which returns if icon is enabled.</param>
    /// <param name="setEnabled">Action which sets if the icon is enabled.</param>
    /// <param name="moveDown">Action to perform when down is pressed.</param>
    /// <param name="moveUp">Action to perform when up is pressed.</param>
    public ToolbarIconOption(
        IIconRegistry iconRegistry,
        Dictionary<string, string?> icons,
        IInputHelper inputHelper,
        Func<string> getCurrentId,
        Func<bool> getEnabled,
        Action<bool> setEnabled,
        Action? moveDown,
        Action? moveUp)
    {
        this.Height = Game1.tileSize;
        this.iconRegistry = iconRegistry;
        this.icons = icons;
        this.inputHelper = inputHelper;
        this.getCurrentId = getCurrentId;
        this.getEnabled = getEnabled;
        this.setEnabled = setEnabled;
        this.moveDown = moveDown;
        this.moveUp = moveUp;
        this.downArrow = iconRegistry.RequireIcon(VanillaIcon.ArrowDown);
        this.upArrow = iconRegistry.RequireIcon(VanillaIcon.ArrowUp);
        this.checkedIcon = iconRegistry.RequireIcon(VanillaIcon.Checked);
        this.uncheckedIcon = iconRegistry.RequireIcon(VanillaIcon.Unchecked);
        this.UpdateId();
    }

    /// <inheritdoc />
    public override string Name => string.Empty;

    /// <inheritdoc />
    public override string Tooltip => this.tooltip;

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
        var clicked = (mouseLeft || mouseRight) && mouseY >= pos.Y && mouseY < pos.Y + this.Height;
        var hoverText = string.Empty;

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
        spriteBatch.Draw(
            checkbox.GetTexture(IconStyle.Transparent),
            pos + new Vector2(Game1.tileSize * 1, 0),
            checkbox.Area,
            Color.White,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            1f);

        if (mouseX >= pos.X + (Game1.tileSize * 1) && mouseX < pos.X + (Game1.tileSize * 2))
        {
            hoverText = I18n.Config_CheckBox_Tooltip();
            if (clicked)
            {
                this.Enabled = !this.Enabled;
            }
        }

        // Down Arrow
        spriteBatch.Draw(
            this.downArrow.GetTexture(IconStyle.Transparent),
            pos + new Vector2(Game1.tileSize * 2, 0),
            this.downArrow.Area,
            this.moveDown is not null ? Color.White : Color.Black * 0.35f,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            1f);

        if (mouseX >= pos.X + (Game1.tileSize * 2) && mouseX < pos.X + (Game1.tileSize * 3))
        {
            hoverText = I18n.Config_MoveDown_Tooltip();
            if (clicked)
            {
                this.moveDown?.Invoke();
            }
        }

        // Icon
        spriteBatch.Draw(
            this.icon.GetTexture(IconStyle.Transparent),
            pos + new Vector2((Game1.tileSize * 3) - 10, -12),
            this.icon.Area,
            Color.White,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            1f);

        // Up Arrow
        spriteBatch.Draw(
            this.upArrow.GetTexture(IconStyle.Transparent),
            pos + new Vector2(Game1.tileSize * 4, 0),
            this.upArrow.Area,
            this.moveUp is not null ? Color.White : Color.Black * 0.35f,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            1f);

        if (mouseX >= pos.X + (Game1.tileSize * 4) && mouseX < pos.X + (Game1.tileSize * 5))
        {
            hoverText = I18n.Config_MoveUp_Tooltip();
            if (clicked)
            {
                this.moveUp?.Invoke();
            }
        }

        if (!string.IsNullOrWhiteSpace(hoverText))
        {
            IClickableMenu.drawToolTip(spriteBatch, hoverText, string.Empty, null);
        }
    }

    [MemberNotNull(
        nameof(ToolbarIconOption.currentId),
        nameof(ToolbarIconOption.name),
        nameof(ToolbarIconOption.icon),
        nameof(ToolbarIconOption.tooltip))]
    private void UpdateId()
    {
        this.currentId = this.getCurrentId();
        this.name = this.currentId.Split('/')[^1];
        this.icon = this.iconRegistry.RequireIcon(this.currentId);
        this.tooltip = this.icons.TryGetValue(this.currentId, out var hoverText)
            ? hoverText ?? string.Empty
            : string.Empty;
    }
}