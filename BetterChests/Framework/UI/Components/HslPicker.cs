namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Represents a component that allows the player to select a color using HSL sliders.</summary>
internal sealed class HslPicker
{
    private static readonly HslColor Transparent = new(0, 0, 0);

    private readonly Chest chest;
    private readonly ClickableComponent chestComponent;
    private readonly DiscreteColorPicker colorPicker;
    private readonly ClickableTextureComponent copyComponent;
    private readonly IReflectedField<int> currentLidFrame;
    private readonly Rectangle defaultColorArea;
    private readonly ClickableTextureComponent defaultColorComponent;
    private readonly Func<Color> getColor;
    private readonly Slider hue;
    private readonly IInputHelper inputHelper;
    private readonly Slider lightness;
    private readonly ItemGrabMenu menu;
    private readonly IModConfig modConfig;
    private readonly Slider saturation;
    private readonly Action<Color> setColor;
    private readonly int xPosition;
    private readonly int yPosition;

    private Slider? holding;
    private bool hoverChest;
    private int lidFrameCount;

    /// <summary>Initializes a new instance of the <see cref="HslPicker" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="colorPicker">The vanilla color picker component.</param>
    /// <param name="menu">The menu that the color picker is attached to.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="getColor">Get method for the current color.</param>
    /// <param name="setColor">Set method for the current color.</param>
    public HslPicker(
        AssetHandler assetHandler,
        DiscreteColorPicker colorPicker,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        ItemGrabMenu menu,
        IReflectionHelper reflectionHelper,
        IModConfig modConfig,
        Func<Color> getColor,
        Action<Color> setColor)
    {
        this.colorPicker = colorPicker;
        this.inputHelper = inputHelper;
        this.menu = menu;
        this.modConfig = modConfig;
        this.getColor = getColor;
        this.setColor = setColor;
        this.chest = (Chest)colorPicker.itemToDrawColored!;
        this.currentLidFrame = reflectionHelper.GetField<int>(this.chest, "currentLidFrame");
        this.currentLidFrame.SetValue(this.chest.startingLidFrame.Value);

        this.xPosition = this.modConfig.HslColorPickerPlacement switch
        {
            InventoryMenu.BorderSide.Left => this.menu.xPositionOnScreen
                - (2 * Game1.tileSize)
                - (IClickableMenu.borderWidth / 2),
            _ => Math.Max(this.menu.colorPickerToggleButton.bounds.Right, this.menu.okButton.bounds.Right)
                + Game1.tileSize,
        };

        this.yPosition = this.menu.yPositionOnScreen - 56 + (IClickableMenu.borderWidth / 2);

        var playerChoiceColor = getColor();
        this.CurrentColor = HslPicker.Transparent;
        if (!playerChoiceColor.Equals(Color.Black))
        {
            this.CurrentColor = HslColor.FromColor(playerChoiceColor);
            colorPicker.colorSelection =
                (playerChoiceColor.R << 0) | (playerChoiceColor.G << 8) | (playerChoiceColor.B << 16);

            this.chest.playerChoiceColor.Value = playerChoiceColor;
        }

        this.chestComponent = new ClickableComponent(
            new Rectangle(
                this.xPosition,
                this.yPosition - Game1.tileSize - (IClickableMenu.borderWidth / 2),
                Game1.tileSize,
                Game1.tileSize),
            this.chest)
        {
            myID = (int)Math.Pow(this.yPosition - Game1.tileSize - (IClickableMenu.borderWidth / 2f), 2)
                + this.xPosition,
        };

        this.copyComponent = iconRegistry.RequireIcon(InternalIcon.Copy).GetComponent(IconStyle.Transparent);
        this.copyComponent.bounds = new Rectangle(this.xPosition + 30, this.yPosition - 4, 36, 36);
        this.copyComponent.hoverText = I18n.Button_Copy_Name();
        this.copyComponent.scale = 3f;
        this.copyComponent.myID = (int)Math.Pow(this.yPosition + 2, 2) + this.xPosition + 34;

        this.defaultColorArea = new Rectangle(this.xPosition - 6, this.yPosition - 4, 36, 36);
        this.defaultColorComponent = new ClickableTextureComponent(
            new Rectangle(this.xPosition - 2, this.yPosition, 7, 7),
            Game1.mouseCursors,
            new Rectangle(295, 503, 7, 7),
            Game1.pixelZoom)
        {
            myID = (int)Math.Pow(this.yPosition, 2) + this.xPosition,
        };

        this.hue = new Slider(
            assetHandler.HslTexture,
            () => this.CurrentColor.H,
            value =>
            {
                this.CurrentColor = this.colorPicker.colorSelection == 0
                    ? assetHandler.HslColors[(int)(value * assetHandler.HslColors.Length)]
                    : this.CurrentColor with { H = value };

                this.UpdateColor();
            },
            new Rectangle(this.xPosition, this.yPosition + 36, 23, 522),
            modConfig.HslColorPickerHueSteps);

        this.saturation = new Slider(
            value => (this.CurrentColor with
            {
                S = this.colorPicker.colorSelection == 0 ? 0 : value,
                L = Math.Max(0.01f, this.colorPicker.colorSelection == 0 ? value : this.CurrentColor.L),
            }).ToRgbColor(),
            () => this.CurrentColor.S,
            value =>
            {
                this.CurrentColor = this.colorPicker.colorSelection == 0
                    ? new HslColor(0, value, 0.5f)
                    : this.CurrentColor with
                    {
                        S = value,
                        L = Math.Max(0.01f, this.CurrentColor.L),
                    };

                this.UpdateColor();
            },
            new Rectangle(this.xPosition + 32, this.yPosition + 300, 23, 256),
            modConfig.HslColorPickerSaturationSteps);

        this.lightness = new Slider(
            value => (this.CurrentColor with
            {
                S = this.colorPicker.colorSelection == 0 ? 0 : this.CurrentColor.S,
                L = value,
            }).ToRgbColor(),
            () => this.CurrentColor.L,
            value =>
            {
                this.CurrentColor = this.CurrentColor with { L = value };
                this.UpdateColor();
            },
            new Rectangle(this.xPosition + 32, this.yPosition + 36, 23, 256),
            modConfig.HslColorPickerLightnessSteps);

        var ids = new HashSet<int>();
        foreach (var cc in this.menu.discreteColorPickerCC)
        {
            ids.Add(cc.myID);
            this.menu.allClickableComponents.Remove(cc);
        }

        foreach (var cc in this.menu.allClickableComponents.Where(cc => ids.Contains(cc.upNeighborID)))
        {
            cc.upNeighborID = -1;
        }

        this.menu.allClickableComponents.Add(this.chestComponent);
        this.menu.allClickableComponents.Add(this.copyComponent);
        this.menu.allClickableComponents.Add(this.defaultColorComponent);
        this.menu.allClickableComponents.AddRange(this.hue.Bars);
        this.menu.allClickableComponents.AddRange(this.lightness.Bars);
        this.menu.allClickableComponents.AddRange(this.saturation.Bars);
        this.SetupBorderNeighbors();
    }

    /// <summary>Gets the current hsl color.</summary>
    public HslColor CurrentColor { get; private set; }

    /// <summary>Draw the color picker.</summary>
    /// <param name="spriteBatch">The sprite batch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        // Get Input states
        var (mouseX, mouseY) = Game1.getMousePosition(true);

        // Update components
        this.hue.Update(mouseX, mouseY);
        this.saturation.Update(mouseX, mouseY);
        this.lightness.Update(mouseX, mouseY);

        // Background
        IClickableMenu.drawTextureBox(
            spriteBatch,
            this.xPosition - (IClickableMenu.borderWidth / 2),
            this.yPosition - (IClickableMenu.borderWidth / 2),
            58 + IClickableMenu.borderWidth,
            558 + IClickableMenu.borderWidth,
            Color.LightGray);

        // Default color component
        this.defaultColorComponent.draw(spriteBatch);

        // Copy component
        this.copyComponent.draw(spriteBatch);

        // Hue slider
        this.hue.Draw(spriteBatch);

        // Saturation slider
        this.saturation.Draw(spriteBatch);

        // Lightness slider
        this.lightness.Draw(spriteBatch);

        // Default color selected
        if (this.colorPicker.colorSelection == 0)
        {
            IClickableMenu.drawTextureBox(
                spriteBatch,
                Game1.mouseCursors,
                new Rectangle(375, 357, 3, 3),
                this.defaultColorArea.X,
                this.defaultColorArea.Y,
                36,
                36,
                Color.Black,
                Game1.pixelZoom,
                false);
        }

        // Chest
        var hovering = this.chestComponent.containsPoint(mouseX, mouseY);
        if (hovering != this.hoverChest)
        {
            this.lidFrameCount = 0;
            this.hoverChest = hovering;
        }
        else if (++this.lidFrameCount < 5)
        {
            // Do nothing
        }
        else if (this.hoverChest)
        {
            this.lidFrameCount = 0;
            var nextFrame = Math.Min(this.chest.getLastLidFrame(), this.currentLidFrame.GetValue() + 1);
            this.currentLidFrame.SetValue(nextFrame);
        }
        else
        {
            this.lidFrameCount = 0;
            var nextFrame = Math.Max(this.chest.startingLidFrame.Value, this.currentLidFrame.GetValue() - 1);
            this.currentLidFrame.SetValue(nextFrame);
        }

        this.chest.draw(spriteBatch, this.chestComponent.bounds.X, this.chestComponent.bounds.Y, local: true);

        var isDown = this.inputHelper.IsDown(SButton.MouseLeft) || this.inputHelper.IsSuppressed(SButton.MouseLeft);
        if (!isDown)
        {
            if (this.holding is not null)
            {
                this.holding.Holding = false;
                this.holding = null;
            }
        }
    }

    /// <summary>Performs a left-click action based on the given mouse coordinates.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse.</param>
    /// <param name="mouseY">The y-coordinate of the mouse.</param>
    /// <returns><c>true</c> if the left-click action was successfully performed; otherwise, <c>false</c>.</returns>
    public bool LeftClick(int mouseX, int mouseY)
    {
        if (this.holding is not null)
        {
            return false;
        }

        if (this.defaultColorArea.Contains(mouseX, mouseY))
        {
            this.CurrentColor = HslPicker.Transparent;
            this.colorPicker.colorSelection = 0;
            this.UpdateColor();
            return true;
        }

        if (this.copyComponent.bounds.Contains(mouseX, mouseY))
        {
            DesktopClipboard.SetText(this.colorPicker.colorSelection.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        if (this.hue.LeftClick(mouseX, mouseY))
        {
            this.holding = this.hue;
            return true;
        }

        if (this.saturation.LeftClick(mouseX, mouseY))
        {
            this.holding = this.saturation;
            return true;
        }

        if (this.lightness.LeftClick(mouseX, mouseY))
        {
            this.holding = this.lightness;
            return true;
        }

        return false;
    }

    /// <summary>Performs a right-click action based on the given mouse coordinates.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse.</param>
    /// <param name="mouseY">The y-coordinate of the mouse.</param>
    /// <returns><c>true</c> if the right-click action was successfully performed; otherwise, <c>false</c>.</returns>
    public bool RightClick(int mouseX, int mouseY)
    {
        if (this.holding is not null)
        {
            return false;
        }

        if (this.copyComponent.bounds.Contains(mouseX, mouseY))
        {
            var textColor = string.Empty;
            DesktopClipboard.GetText(ref textColor);
            if (!int.TryParse(textColor, out var selection))
            {
                return false;
            }

            var playerChoiceColor = DiscreteColorPicker.getColorFromSelection(selection);
            this.CurrentColor = HslColor.FromColor(playerChoiceColor);
            this.colorPicker.colorSelection = selection;
            this.UpdateColor();
            return true;
        }

        return false;
    }

    /// <summary>Sets up the border neighbors for the color picker.</summary>
    public void SetupBorderNeighbors()
    {
        Action<ClickableComponent, int> setBorderNeighbor;
        Action<ClickableComponent, int> setLocalNeighbor;
        var borderNeighbors = new List<ClickableComponent>();
        var localNeighbors = new List<ClickableComponent>();

        if (this.modConfig.HslColorPickerPlacement == InventoryMenu.BorderSide.Left)
        {
            setBorderNeighbor = (c, i) => c.leftNeighborID = i;
            setLocalNeighbor = (c, i) => c.rightNeighborID = i;
            borderNeighbors.AddRange(this.menu.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Left));
            localNeighbors.Add(this.chestComponent);
            localNeighbors.Add(this.copyComponent);
            localNeighbors.AddRange(this.saturation.Bars);
            localNeighbors.AddRange(this.lightness.Bars);
        }
        else
        {
            setBorderNeighbor = (c, i) => c.rightNeighborID = i;
            setLocalNeighbor = (c, i) => c.leftNeighborID = i;
            borderNeighbors.AddRange(
                this.menu.allClickableComponents.Where(
                    c => c.bounds.Left == this.menu.colorPickerToggleButton.bounds.Left || c == this.menu.trashCan));

            localNeighbors.Add(this.chestComponent);
            localNeighbors.Add(this.defaultColorComponent);
            localNeighbors.AddRange(this.hue.Bars);
        }

        if (!Game1.player.showChestColorPicker)
        {
            foreach (var button in borderNeighbors)
            {
                setBorderNeighbor(button, -1);
            }

            return;
        }

        // Set local up-down neighbors
        this.chestComponent.downNeighborID = this.defaultColorComponent.myID;
        this.defaultColorComponent.upNeighborID = this.chestComponent.myID;
        this.copyComponent.upNeighborID = this.chestComponent.myID;

        this.defaultColorComponent.downNeighborID = this.hue.Bars[0].myID;
        this.hue.Bars[0].upNeighborID = this.defaultColorComponent.myID;

        this.copyComponent.downNeighborID = this.lightness.Bars[0].myID;
        this.lightness.Bars[0].upNeighborID = this.copyComponent.myID;

        this.lightness.Bars[^1].downNeighborID = this.saturation.Bars[0].myID;
        this.saturation.Bars[0].upNeighborID = this.lightness.Bars[^1].myID;

        // Set local left-right neighbors
        this.defaultColorComponent.rightNeighborID = this.copyComponent.myID;
        this.copyComponent.leftNeighborID = this.defaultColorComponent.myID;

        var saturationAndLightnessBars = this.saturation.Bars.Concat(this.lightness.Bars).ToList();

        // Assign hue components to saturation and lightness
        foreach (var hueBar in this.hue.Bars)
        {
            var saturationAndLightnessBar =
                saturationAndLightnessBars.MinBy(c => Math.Abs(c.bounds.Center.Y - hueBar.bounds.Center.Y));

            hueBar.rightNeighborID = saturationAndLightnessBar?.myID ?? -1;
        }

        // Assign saturation components to hue
        foreach (var saturationAndLightnessBar in saturationAndLightnessBars)
        {
            var hueBar = this.hue.Bars.MinBy(
                c => Math.Abs(c.bounds.Center.Y - saturationAndLightnessBar.bounds.Center.Y));

            saturationAndLightnessBar.leftNeighborID = hueBar?.myID ?? -1;
        }

        // Assign a local neighbor to each border neighbor
        foreach (var borderNeighbor in borderNeighbors)
        {
            var localNeighbor = localNeighbors.MinBy(c => Math.Abs(c.bounds.Center.Y - borderNeighbor.bounds.Center.Y));
            setBorderNeighbor(borderNeighbor, localNeighbor?.myID ?? -1);
        }

        // Assign a border neighbor to each local neighbor
        foreach (var localNeighbor in localNeighbors)
        {
            var borderNeighbor =
                borderNeighbors.MinBy(c => Math.Abs(c.bounds.Center.Y - localNeighbor.bounds.Center.Y));

            setLocalNeighbor(localNeighbor, borderNeighbor?.myID ?? -1);
        }
    }

    private void UpdateColor()
    {
        var c = this.CurrentColor.ToRgbColor();
        this.setColor(c);
        this.colorPicker.colorSelection = (c.R << 0) | (c.G << 8) | (c.B << 16);
        this.chest.playerChoiceColor.Value = this.colorPicker.colorSelection == 0 ? Color.Black : c;
    }
}