namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>A menu for assigning categories to a container.</summary>
internal sealed class CategorizeMenu : SearchMenu
{
    private readonly ClickableTextureComponent buttonCopy;
    private readonly ClickableTextureComponent buttonPaste;
    private readonly ClickableTextureComponent buttonSave;
    private readonly ClickableTextureComponent buttonStack;
    private readonly IStorageContainer container;
    private readonly IIcon iconNoStack;

    /// <summary>Initializes a new instance of the <see cref="CategorizeMenu" /> class.</summary>
    /// <param name="container">The container to categorize.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="uiManager">Dependency used for managing ui.</param>
    public CategorizeMenu(
        IStorageContainer container,
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        UiManager uiManager)
        : base(expressionHandler, container.CategorizeChestSearchTerm, uiManager)
    {
        this.container = container;

        if (!iconRegistry.TryGetIcon("Save", out var saveIcon))
        {
            throw new InvalidOperationException("The save icon is missing.");
        }

        if (!iconRegistry.TryGetIcon("NoStack", out var noStackIcon))
        {
            throw new InvalidOperationException("The save icon is missing.");
        }

        if (!iconRegistry.TryGetIcon("Copy", out var copyIcon))
        {
            throw new InvalidOperationException("The save icon is missing.");
        }

        if (!iconRegistry.TryGetIcon("Paste", out var pasteIcon))
        {
            throw new InvalidOperationException("The save icon is missing.");
        }

        this.buttonSave = new ClickableTextureComponent(
            saveIcon.Id,
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + Game1.tileSize + 16,
                Game1.tileSize,
                Game1.tileSize),
            string.Empty,
            I18n.Button_SaveAsCategorization_Name(),
            saveIcon.Texture,
            saveIcon.Area,
            Game1.pixelZoom);

        this.iconNoStack = noStackIcon;
        this.buttonStack = new ClickableTextureComponent(
            noStackIcon.Id,
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 2),
                Game1.tileSize,
                Game1.tileSize),
            string.Empty,
            I18n.Button_IncludeExistingStacks_Name(),
            noStackIcon.Texture,
            noStackIcon.Area,
            Game1.pixelZoom);

        if (this.container.CategorizeChestIncludeStacks is FeatureOption.Enabled)
        {
            this.buttonStack.texture = Game1.mouseCursors;
            this.buttonStack.sourceRect = new Rectangle(103, 469, 16, 16);
        }

        this.buttonCopy = new ClickableTextureComponent(
            copyIcon.Id,
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 3),
                Game1.tileSize,
                Game1.tileSize),
            string.Empty,
            I18n.Button_Copy_Name(),
            copyIcon.Texture,
            copyIcon.Area,
            Game1.pixelZoom);

        this.buttonPaste = new ClickableTextureComponent(
            pasteIcon.Id,
            new Rectangle(
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 4),
                Game1.tileSize,
                Game1.tileSize),
            string.Empty,
            I18n.Button_Paste_Name(),
            pasteIcon.Texture,
            pasteIcon.Area,
            Game1.pixelZoom);

        this.allClickableComponents.Add(this.buttonSave);
        this.allClickableComponents.Add(this.buttonStack);
        this.allClickableComponents.Add(this.buttonCopy);
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

            this.buttonStack.texture = this.iconNoStack.Texture;
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
    protected override List<Item> GetItems()
    {
        var items = base.GetItems();
        return items;
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) => true;
}