namespace StardewMods.CrystallineJunimoChests;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.CrystallineJunimoChests.Models;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private ModConfig config = null!;
    private Texture2D texture = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        this.config = this.Helper.ReadConfig<ModConfig>();
        this.texture = this.Helper.ModContent.Load<Texture2D>("assets/texture.png");
        _ = new ModPatches(this.Helper.ModContent, this.ModManifest, this.texture);

        // Events
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
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

        this.Helper.Input.Suppress(e.Button);
        var data = this.Helper.ModContent.Load<DataModel>("assets/data.json");

        // Cost is disabled
        if (this.config.GemCost < 1)
        {
            Game1.activeClickableMenu.exitThisMenuNoSound();
            Game1.playSound(data.Sound);
            chest.GlobalInventoryId = $"{this.ModManifest.UniqueID}-{data.Colors[selection - 1].Name}";
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
        if (Game1.player.Items.ContainsId(item.QualifiedItemId, this.config.GemCost))
        {
            var responses = Game1.currentLocation.createYesNoResponses();
            Game1.currentLocation.createQuestionDialogue(
                I18n.Message_Confirm(
                    this.config.GemCost,
                    item.DisplayName,
                    chest.DisplayName,
                    this.Helper.Translation.Get($"color.{data.Colors[selection - 1].Name}")),
                responses,
                (who, whichAnswer) =>
                {
                    if (whichAnswer != "Yes")
                    {
                        return;
                    }

                    Game1.playSound(data.Sound);
                    who.Items.ReduceId(item.QualifiedItemId, this.config.GemCost);
                    chest.GlobalInventoryId = $"{this.ModManifest.UniqueID}-{data.Colors[selection - 1].Name}";
                    chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(selection);
                    chest.Location.temporarySprites.Add(
                        new TemporaryAnimatedSprite(
                            5,
                            (chest.TileLocation * Game1.tileSize) - new Vector2(0, 32),
                            DiscreteColorPicker.getColorFromSelection(selection))
                        {
                            layerDepth = 1f,
                        });
                });

            return;
        }

        Game1.drawObjectDialogue(
            I18n.Message_Alert(
                this.config.GemCost,
                item.DisplayName,
                chest.DisplayName,
                this.Helper.Translation.Get($"color.{data.Colors[selection - 1].Name}")));
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (!this.Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
        {
            return;
        }

        var api = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (api is null)
        {
            return;
        }

        api.Register(
            this.ModManifest,
            () => this.config = this.Helper.ReadConfig<ModConfig>(),
            () => this.Helper.WriteConfig(new ModConfig()));

        api.AddNumberOption(
            this.ModManifest,
            () => this.config.GemCost,
            value => this.config.GemCost = value,
            I18n.Config_GemCost_Name,
            I18n.Config_GemCost_Tooltip);
    }
}