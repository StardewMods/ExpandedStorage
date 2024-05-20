namespace StardewMods.CrystallineJunimoChests.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.CrystallineJunimoChests.Framework.Interfaces;
using StardewMods.CrystallineJunimoChests.Framework.Models;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Responsible for handling interactions with chests.</summary>
internal sealed class ChestHandler
{
    private readonly AssetHandler assetHandler;
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;
    private readonly ITranslationHelper translationHelper;

    /// <summary>Initializes a new instance of the <see cref="ChestHandler" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="translationHelper">Dependency used for accessing mod translations.</param>
    public ChestHandler(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IInputHelper inputHelper,
        IModConfig modConfig,
        ITranslationHelper translationHelper)
    {
        this.assetHandler = assetHandler;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;
        this.translationHelper = translationHelper;

        // Events
        eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        eventManager.Subscribe<ObjectListChangedEventArgs>(this.OnObjectListChanged);
    }

    private GameLocation.afterQuestionBehavior GetAfterDialogueBehavior(
        Chest chest,
        DataModel data,
        ParsedItemData item,
        int selection) =>
        (who, whichAnswer) =>
        {
            if (whichAnswer != "Yes")
            {
                return;
            }

            Game1.playSound(data.Sound);
            who.Items.ReduceId(item.QualifiedItemId, this.modConfig.GemCost);
            chest.GlobalInventoryId = $"{Mod.Mod.Id}-{data.Colors[selection - 1].Name}";
            chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(selection);
            chest.Location.temporarySprites.Add(
                new TemporaryAnimatedSprite(
                    5,
                    (chest.TileLocation * Game1.tileSize) - new Vector2(0, 32),
                    DiscreteColorPicker.getColorFromSelection(selection))
                {
                    layerDepth = 1f,
                });
        };

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
                { } chestColorPicker,
            })
        {
            return;
        }

        var (x, y) = e.Cursor.GetScaledScreenPixels();
        var area = new Rectangle(
            chestColorPicker.xPositionOnScreen + (IClickableMenu.borderWidth / 2),
            chestColorPicker.yPositionOnScreen + (IClickableMenu.borderWidth / 2),
            36 * DiscreteColorPicker.totalColors,
            28);

        if (!area.Contains(x, y))
        {
            return;
        }

        var selection = ((int)x - area.X) / 36;
        if (selection < 0 || selection >= DiscreteColorPicker.totalColors)
        {
            return;
        }

        var currentSelection = DiscreteColorPicker.getSelectionFromColor(chest.playerChoiceColor.Value);
        if (selection == currentSelection)
        {
            return;
        }

        if (selection == 0)
        {
            chest.GlobalInventoryId = "JunimoChests";
            return;
        }

        this.inputHelper.Suppress(e.Button);
        var data = this.assetHandler.Data;

        // Cost is disabled
        if (this.modConfig.GemCost < 1)
        {
            Game1.activeClickableMenu.exitThisMenuNoSound();
            Game1.playSound(data.Sound);
            chest.GlobalInventoryId = $"{Mod.Mod.Id}-{data.Colors[selection - 1].Name}";
            chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(selection);
            chest.Location.temporarySprites.Add(
                new TemporaryAnimatedSprite(
                    5,
                    (chest.TileLocation * Game1.tileSize) - new Vector2(0, 32),
                    DiscreteColorPicker.getColorFromSelection(selection))
                {
                    layerDepth = 1f,
                });

            return;
        }

        // Player has item
        var item = ItemRegistry.GetDataOrErrorItem(data.Colors[selection - 1].Item);
        if (Game1.player.Items.ContainsId(item.QualifiedItemId, this.modConfig.GemCost))
        {
            var responses = Game1.currentLocation.createYesNoResponses();
            Game1.currentLocation.createQuestionDialogue(
                I18n.Message_Confirm(
                    this.modConfig.GemCost,
                    item.DisplayName,
                    chest.DisplayName,
                    this.translationHelper.Get($"color.{data.Colors[selection - 1].Name}")),
                responses,
                this.GetAfterDialogueBehavior(chest, data, item, selection));

            return;
        }

        Game1.drawObjectDialogue(
            I18n.Message_Alert(
                this.modConfig.GemCost,
                item.DisplayName,
                chest.DisplayName,
                this.translationHelper.Get($"color.{data.Colors[selection - 1].Name}")));
    }

    private void OnObjectListChanged(ObjectListChangedEventArgs e)
    {
        foreach (var (_, obj) in e.Added)
        {
            if (obj is not Chest
                {
                    SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
                } chest)
            {
                return;
            }

            // Change special chest type to none so that it can be colorable
            chest.SpecialChestType = Chest.SpecialChestTypes.None;
            chest.GlobalInventoryId = "JunimoChests";
            chest.lidFrameCount.Value = 5;
        }
    }
}