namespace StardewMods.CrystallineJunimoChests.Framework.Services;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.CrystallineJunimoChests.Framework.Interfaces;
using StardewMods.CrystallineJunimoChests.Framework.Models;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Responsible for handling interactions with chests.</summary>
internal sealed class ChestHandler
{
    private readonly AssetHandler assetHandler;
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;
    private readonly IModContentHelper modContentHelper;
    private readonly ITranslationHelper translationHelper;

    /// <summary>Initializes a new instance of the <see cref="ChestHandler" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="translationHelper">Dependency used for accessing mod translations.</param>
    public ChestHandler(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IInputHelper inputHelper,
        IModConfig modConfig,
        IModContentHelper modContentHelper,
        ITranslationHelper translationHelper)
    {
        this.assetHandler = assetHandler;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;
        this.modContentHelper = modContentHelper;
        this.translationHelper = translationHelper;

        // Events
        eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        eventManager.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!e.Button.IsUseToolButton()
            || Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest
                {
                    QualifiedItemId: "(BC)256",
                } chest,
                chestColorPicker:
                {
                    itemToDrawColored: Chest colorPickerChest,
                } chestColorPicker,
            })
        {
            return;
        }

        var (mouseX, mouseY) = Utility.ModifyCoordinatesForUIScale(e.Cursor.GetScaledScreenPixels()).ToPoint();
        var currentColor = chest.playerChoiceColor.Value;
        var currentSelection = chestColorPicker.colorSelection;
        chestColorPicker.receiveLeftClick(mouseX, mouseY);
        if (currentSelection == chestColorPicker.colorSelection)
        {
            return;
        }

        if (chestColorPicker.colorSelection == 0)
        {
            chest.GlobalInventoryId = "JunimoChests";
            chest.modData.Remove("furyx639.BetterChests/StorageName");
            chest.modData.Remove("furyx639.ExpandedStorage/TextureOverride");
            chest.modData.Remove("furyx639.ExpandedStorage/TintOverride");
            colorPickerChest.modData.Remove("furyx639.ExpandedStorage/TextureOverride");
            colorPickerChest.modData.Remove("furyx639.ExpandedStorage/TintOverride");
            return;
        }

        this.inputHelper.Suppress(e.Button);
        var data = this.assetHandler.Data[chestColorPicker.colorSelection - 1];
        chest.playerChoiceColor.Value = currentColor;

        // Cost is disabled
        if (this.modConfig.GemCost < 1)
        {
            Game1.activeClickableMenu.exitThisMenuNoSound();
            this.UpdateChest(chest, data, chestColorPicker.colorSelection);
            return;
        }

        // Player has item
        var item = ItemRegistry.GetDataOrErrorItem(this.assetHandler.Data[chestColorPicker.colorSelection - 1].Item);
        if (Game1.player.Items.ContainsId(item.QualifiedItemId, this.modConfig.GemCost))
        {
            var responses = Game1.currentLocation.createYesNoResponses();
            Game1.currentLocation.createQuestionDialogue(
                I18n.Message_Confirm(
                    this.modConfig.GemCost,
                    item.DisplayName,
                    chest.DisplayName,
                    this.translationHelper.Get(
                        $"color.{this.assetHandler.Data[chestColorPicker.colorSelection - 1].Name}")),
                responses,
                (_, whichAnswer) =>
                {
                    if (whichAnswer != "Yes")
                    {
                        return;
                    }

                    Game1.player.Items.ReduceId(item.QualifiedItemId, this.modConfig.GemCost);
                    this.UpdateChest(chest, data, chestColorPicker.colorSelection);
                });

            return;
        }

        // Player does not have item
        Game1.drawObjectDialogue(
            I18n.Message_Alert(
                this.modConfig.GemCost,
                item.DisplayName,
                chest.DisplayName,
                this.translationHelper.Get(
                    $"color.{this.assetHandler.Data[chestColorPicker.colorSelection - 1].Name}")));
    }

    private void OnMenuChanged(MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu
            {
                context: Chest
                {
                    QualifiedItemId: "(BC)256",
                } currentChest,
                chestColorPicker.itemToDrawColored: Chest colorPickerChest,
            })
        {
            return;
        }

        var currentSelection = DiscreteColorPicker.getSelectionFromColor(currentChest.playerChoiceColor.Value);
        if (currentSelection == 0)
        {
            currentChest.modData.Remove("furyx639.ExpandedStorage/TextureOverride");
            currentChest.modData.Remove("furyx639.ExpandedStorage/TintOverride");
            return;
        }

        currentChest.modData["furyx639.ExpandedStorage/TintOverride"] = this
            .assetHandler.Data[currentSelection]
            .Color.PackedValue.ToString(CultureInfo.InvariantCulture);

        currentChest.modData["furyx639.ExpandedStorage/TextureOverride"] = this.modContentHelper
            .GetInternalAssetName($"assets/{this.assetHandler.Data[currentSelection].Name}.png")
            .Name;

        colorPickerChest.modData["furyx639.ExpandedStorage/TextureOverride"] = this.modContentHelper
            .GetInternalAssetName($"assets/{this.assetHandler.Data[currentSelection].Name}.png")
            .Name;

        colorPickerChest.modData["furyx639.ExpandedStorage/TintOverride"] = this
            .assetHandler.Data[currentSelection]
            .Color.PackedValue.ToString(CultureInfo.InvariantCulture);
    }

    private void UpdateChest(Chest chest, ColorData data, int selection)
    {
        Game1.playSound(this.modConfig.Sound);

        chest.modData["furyx639.BetterChests/StorageName"] =
            I18n.Chest_Name(this.translationHelper.Get($"color.{data.Name}"));

        chest.modData["furyx639.ExpandedStorage/TextureOverride"] = this.modContentHelper
            .GetInternalAssetName($"assets/{data.Name}.png")
            .Name;

        chest.modData["furyx639.ExpandedStorage/TintOverride"] =
            data.Color.PackedValue.ToString(CultureInfo.InvariantCulture);

        chest.GlobalInventoryId = $"{Mod.Id}-{data.Name}";
        chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(selection);

        chest.Location.temporarySprites.Add(
            new TemporaryAnimatedSprite(5, (chest.TileLocation * Game1.tileSize) - new Vector2(0, 32), data.Color)
            {
                layerDepth = 1f,
            });
    }
}