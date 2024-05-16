namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>A menu for assigning categories to a container.</summary>
internal sealed class CategorizeMenu : SearchMenu
{
    private readonly IStorageContainer container;
    private readonly ClickableTextureComponent copyComponent;
    private readonly IIcon iconNoStack;
    private readonly ClickableTextureComponent pasteComponent;
    private readonly ClickableTextureComponent saveComponent;
    private readonly ClickableTextureComponent stackComponent;

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

        this.saveComponent = iconRegistry.RequireIcon("Save").GetComponent(IconStyle.Button);
        this.saveComponent.hoverText = I18n.Button_SaveAsCategorization_Name();
        this.saveComponent.bounds = new Rectangle(
            this.xPositionOnScreen + this.width + 4,
            this.yPositionOnScreen + Game1.tileSize + 16,
            Game1.tileSize,
            Game1.tileSize);

        this.iconNoStack = iconRegistry.RequireIcon("NoStack");
        this.stackComponent = this.iconNoStack.GetComponent(IconStyle.Button);
        this.stackComponent.hoverText = I18n.Button_IncludeExistingStacks_Name();
        this.stackComponent.bounds = new Rectangle(
            this.xPositionOnScreen + this.width + 4,
            this.yPositionOnScreen + ((Game1.tileSize + 16) * 2),
            Game1.tileSize,
            Game1.tileSize);

        if (this.container.CategorizeChestIncludeStacks is FeatureOption.Enabled)
        {
            this.stackComponent.texture = Game1.mouseCursors;
            this.stackComponent.sourceRect = new Rectangle(103, 469, 16, 16);
        }

        this.copyComponent = iconRegistry.RequireIcon("Copy").GetComponent(IconStyle.Button);
        this.copyComponent.hoverText = I18n.Button_Copy_Name();
        this.copyComponent.bounds = new Rectangle(
            this.xPositionOnScreen + this.width + 4,
            this.yPositionOnScreen + ((Game1.tileSize + 16) * 3),
            Game1.tileSize,
            Game1.tileSize);

        this.pasteComponent = iconRegistry.RequireIcon("Paste").GetComponent(IconStyle.Button);
        this.pasteComponent.hoverText = I18n.Button_Paste_Name();
        this.pasteComponent.bounds = new Rectangle(
            this.xPositionOnScreen + this.width + 4,
            this.yPositionOnScreen + ((Game1.tileSize + 16) * 4),
            Game1.tileSize,
            Game1.tileSize);

        this.allClickableComponents.Add(this.saveComponent);
        this.allClickableComponents.Add(this.stackComponent);
        this.allClickableComponents.Add(this.copyComponent);
        this.allClickableComponents.Add(this.pasteComponent);
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        if (this.saveComponent.containsPoint(x, y) && this.readyToClose())
        {
            this.container.CategorizeChestSearchTerm = this.SearchText;
            this.container.CategorizeChestIncludeStacks = this.stackComponent.sourceRect.Equals(this.iconNoStack.Area)
                ? FeatureOption.Disabled
                : FeatureOption.Enabled;

            this.exitThisMenuNoSound();
            this.container.ShowMenu();
            return;
        }

        if (this.stackComponent.containsPoint(x, y))
        {
            if (this.stackComponent.sourceRect.Equals(this.iconNoStack.Area))
            {
                this.stackComponent.texture = Game1.mouseCursors;
                this.stackComponent.sourceRect = new Rectangle(103, 469, 16, 16);
                return;
            }

            this.stackComponent.texture = this.iconNoStack.GetTexture(IconStyle.Button);
            this.stackComponent.sourceRect = this.iconNoStack.Area;
            return;
        }

        if (this.copyComponent.containsPoint(x, y))
        {
            DesktopClipboard.SetText(this.SearchText);
            return;
        }

        if (this.pasteComponent.containsPoint(x, y))
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