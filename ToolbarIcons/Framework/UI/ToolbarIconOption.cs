namespace StardewMods.ToolbarIcons.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Services;
using StardewValley.Menus;

/// <summary>Represents a complex menu option for arranging toolbar icons.</summary>
internal sealed class ToolbarIconOption : BaseComplexOption
{
    private readonly AssetHandler assetHandler;
    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;

    private ClickableTextureComponent? component;
    private ToolbarIcon? icon;
    private int index;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconOption" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="components">Dependency used for the toolbar icon components.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public ToolbarIconOption(
        AssetHandler assetHandler,
        Dictionary<string, ClickableTextureComponent> components,
        IInputHelper inputHelper,
        IModConfig modConfig)
    {
        this.assetHandler = assetHandler;
        this.components = components;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;
    }

    /// <inheritdoc />
    public override string Name => this.component?.name ?? string.Empty;

    /// <inheritdoc />
    public override string Tooltip => this.component?.hoverText ?? string.Empty;

    /// <inheritdoc />
    public override int Height { get; protected set; }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        var (mouseX, mouseY) = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        var mouseLeft = this.inputHelper.GetState(SButton.MouseLeft) == SButtonState.Pressed;
        var mouseRight = this.inputHelper.GetState(SButton.MouseRight) == SButtonState.Pressed;

        if (this.component is not null && this.icon is not null)
        {
            if (this.icon != this.modConfig.Icons[this.index])
            {
                this.Update();
            }

            // Arrows
            spriteBatch.Draw(
                this.assetHandler.Arrows.Value,
                pos + new Vector2(0, 0),
                new Rectangle(0, 0, 8, 8),
                Color.White * (this.index > 0 ? 1f : 0.5f),
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);

            spriteBatch.Draw(
                this.assetHandler.Arrows.Value,
                pos + new Vector2(96, 0),
                new Rectangle(8, 0, 8, 8),
                Color.White * (this.index < this.components.Count - 1 ? 1f : 0.5f),
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);

            // Checkbox
            spriteBatch.Draw(
                Game1.mouseCursors,
                pos + new Vector2(160, 0),
                this.icon.Enabled ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked,
                Color.White,
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                0.4f);

            // Icon
            this.component.bounds.X = (int)pos.X + 40;
            this.component.bounds.Y = (int)pos.Y - 12;
            this.component.draw(spriteBatch);
        }

        if (!(mouseLeft || mouseRight) || mouseY < pos.Y || mouseY > pos.Y + this.Height)
        {
            return;
        }

        // Move up
        if (this.index > 0 && mouseX >= pos.X && mouseX <= pos.X + 32)
        {
            (this.modConfig.Icons[this.index - 1], this.modConfig.Icons[this.index]) = (
                this.modConfig.Icons[this.index], this.modConfig.Icons[this.index - 1]);

            return;
        }

        // Move down
        if (this.index < this.components.Count - 1 && mouseX >= pos.X + 96 && mouseX <= pos.X + 128)
        {
            (this.modConfig.Icons[this.index + 1], this.modConfig.Icons[this.index]) = (
                this.modConfig.Icons[this.index], this.modConfig.Icons[this.index + 1]);

            return;
        }

        // Toggle
        if (this.icon is not null && mouseX >= pos.X + 160 && mouseX <= pos.X + 192)
        {
            this.icon.Enabled = !this.icon.Enabled;
        }
    }

    /// <summary>Initializes the toolbar icons.</summary>
    /// <param name="initIndex">The index.</param>
    public void Init(int initIndex)
    {
        this.index = initIndex;
        this.icon = this.modConfig.Icons[initIndex];

        if (!this.components.TryGetValue(this.icon.Id, out var initComponent))
        {
            return;
        }

        this.component = new ClickableTextureComponent(
            new Rectangle(0, 0, 32, 32),
            initComponent.texture,
            initComponent.sourceRect,
            3)
        {
            hoverText = initComponent.hoverText,
            name = initComponent.name,
        };

        var textBounds = Game1.dialogueFont.MeasureString(initComponent.hoverText);
        this.Height = (int)textBounds.Y;
    }

    private void Update()
    {
        this.icon = this.modConfig.Icons[this.index];
        if (this.component is null || !this.components.TryGetValue(this.icon.Id, out var updateComponent))
        {
            return;
        }

        this.component.texture = updateComponent.texture;
        this.component.sourceRect = updateComponent.sourceRect;
        this.component.hoverText = updateComponent.hoverText;
        this.component.name = updateComponent.name;
    }
}