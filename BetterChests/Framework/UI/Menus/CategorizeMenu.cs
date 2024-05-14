namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>A menu for assigning categories to a container.</summary>
internal sealed class CategorizeMenu : SearchMenu
{
    private readonly AssetHandler assetHandler;
    private readonly ClickableTextureComponent buttonCopy;
    private readonly ClickableTextureComponent buttonPaste;
    private readonly ClickableTextureComponent buttonSave;
    private readonly ClickableTextureComponent buttonStack;
    private readonly IStorageContainer container;
    private readonly Icon iconNoStack;

    /// <summary>Initializes a new instance of the <see cref="CategorizeMenu" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="container">The container to categorize.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="uiManager">Dependency used for managing ui.</param>
    public CategorizeMenu(
        AssetHandler assetHandler,
        IStorageContainer container,
        IExpressionHandler expressionHandler,
        UiManager uiManager)
        : base(expressionHandler, container.CategorizeChestSearchTerm, uiManager)
    {
        this.assetHandler = assetHandler;
        this.container = container;

        if (!assetHandler.Icons.TryGetValue("furyx639.BetterChests/Save", out var saveIcon))
        {
            throw new InvalidOperationException("The save icon is missing.");
        }

        this.buttonSave = new ClickableTextureComponent(
            "Save",
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + Game1.tileSize + 16,
                Game1.tileSize,
                Game1.tileSize),
            string.Empty,
            I18n.Button_SaveAsCategorization_Name(),
            assetHandler.UiTexture,
            saveIcon.Area,
            Game1.pixelZoom);

        this.allClickableComponents.Add(this.buttonSave);

        if (!assetHandler.Icons.TryGetValue("furyx639.BetterChests/NoStack", out var noStackIcon))
        {
            throw new InvalidOperationException("The no stack icon is missing.");
        }

        this.iconNoStack = noStackIcon;
        this.buttonStack = new ClickableTextureComponent(
            "Stack",
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 2),
                Game1.tileSize,
                Game1.tileSize),
            string.Empty,
            I18n.Button_IncludeExistingStacks_Name(),
            assetHandler.UiTexture,
            noStackIcon.Area,
            Game1.pixelZoom);

        if (this.container.CategorizeChestIncludeStacks is FeatureOption.Enabled)
        {
            this.buttonStack.texture = Game1.mouseCursors;
            this.buttonStack.sourceRect = new Rectangle(103, 469, 16, 16);
        }

        this.allClickableComponents.Add(this.buttonStack);

        if (!assetHandler.Icons.TryGetValue("furyx639.BetterChests/Copy", out var copyIcon))
        {
            throw new InvalidOperationException("The copy icon is missing.");
        }

        this.buttonCopy = new ClickableTextureComponent(
            "Copy",
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 3),
                Game1.tileSize,
                Game1.tileSize),
            string.Empty,
            I18n.Button_Copy_Name(),
            assetHandler.UiTexture,
            copyIcon.Area,
            Game1.pixelZoom);

        this.allClickableComponents.Add(this.buttonCopy);

        if (!assetHandler.Icons.TryGetValue("furyx639.BetterChests/Paste", out var pasteIcon))
        {
            throw new InvalidOperationException("The paste icon is missing.");
        }

        this.buttonPaste = new ClickableTextureComponent(
            "Paste",
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 4),
                Game1.tileSize,
                Game1.tileSize),
            string.Empty,
            I18n.Button_Paste_Name(),
            assetHandler.UiTexture,
            pasteIcon.Area,
            Game1.pixelZoom);

        this.allClickableComponents.Add(this.buttonPaste);
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        if (this.buttonSave.containsPoint(x, y) && this.readyToClose())
        {
            this.container.CategorizeChestSearchTerm = this.SearchText;
            this.container.CategorizeChestIncludeStacks = this.buttonStack.sourceRect.Equals(this.iconNoStack.Area)
                ? FeatureOption.Disabled
                : FeatureOption.Enabled;

            this.exitThisMenuNoSound();
            this.container.ShowMenu();
            return;
        }

        if (this.buttonStack.containsPoint(x, y))
        {
            if (this.buttonStack.sourceRect.Equals(this.iconNoStack.Area))
            {
                this.buttonStack.texture = Game1.mouseCursors;
                this.buttonStack.sourceRect = new Rectangle(103, 469, 16, 16);
                return;
            }

            this.buttonStack.texture = this.assetHandler.UiTexture;
            this.buttonStack.sourceRect = this.iconNoStack.Area;
            return;
        }

        if (this.buttonCopy.containsPoint(x, y))
        {
            DesktopClipboard.SetText(this.SearchText);
            return;
        }

        if (this.buttonPaste.containsPoint(x, y))
        {
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            this.SetSearchText(searchText, true);
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true) =>
        base.receiveRightClick(x, y, playSound);

    /// <inheritdoc />
    protected override List<Item> GetItems()
    {
        var items = base.GetItems();
        return items;
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) => true;
}