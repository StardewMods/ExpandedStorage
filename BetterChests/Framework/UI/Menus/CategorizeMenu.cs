namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
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
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public CategorizeMenu(
        IStorageContainer container,
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper)
        : base(expressionHandler, iconRegistry, inputHelper, reflectionHelper, container.CategorizeChestSearchTerm)
    {
        this.container = container;

        this.saveComponent = iconRegistry
            .RequireIcon(InternalIcon.Save)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + Game1.tileSize + 16);

        this.saveComponent.hoverText = I18n.Button_SaveAsCategorization_Name();

        this.iconNoStack = iconRegistry.RequireIcon(InternalIcon.NoStack);
        this.stackComponent = this.iconNoStack.GetComponent(
            IconStyle.Button,
            this.xPositionOnScreen + this.width + 4,
            this.yPositionOnScreen + ((Game1.tileSize + 16) * 2));

        this.stackComponent.hoverText = I18n.Button_IncludeExistingStacks_Name();

        if (this.container.CategorizeChestIncludeStacks is FeatureOption.Enabled)
        {
            this.stackComponent.texture = Game1.mouseCursors;
            this.stackComponent.sourceRect = new Rectangle(103, 469, 16, 16);
        }

        this.copyComponent = iconRegistry
            .RequireIcon(InternalIcon.NoStack)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 3));

        this.copyComponent.hoverText = I18n.Button_Copy_Name();

        this.pasteComponent = iconRegistry
            .RequireIcon(InternalIcon.Paste)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 4));

        this.pasteComponent.hoverText = I18n.Button_Paste_Name();

        this.allClickableComponents.Add(this.saveComponent);
        this.allClickableComponents.Add(this.stackComponent);
        this.allClickableComponents.Add(this.copyComponent);
        this.allClickableComponents.Add(this.pasteComponent);
    }

    /// <inheritdoc />
    protected override List<Item> GetItems()
    {
        var items = base.GetItems();
        return items;
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) => true;

    /// <inheritdoc />
    protected override bool TryLeftClick(int x, int y)
    {
        if (this.saveComponent.containsPoint(x, y) && this.readyToClose())
        {
            this.container.CategorizeChestSearchTerm = this.SearchText;
            this.container.CategorizeChestIncludeStacks = this.stackComponent.texture.Equals(Game1.mouseCursors)
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;

            this.exitThisMenuNoSound();
            this.container.ShowMenu();
            return true;
        }

        if (this.stackComponent.containsPoint(x, y))
        {
            if (this.stackComponent.texture.Equals(Game1.mouseCursors))
            {
                this.stackComponent.texture = this.iconNoStack.GetTexture(IconStyle.Button);
                this.stackComponent.sourceRect = new Rectangle(0, 0, 16, 16);
                return true;
            }

            this.stackComponent.texture = Game1.mouseCursors;
            this.stackComponent.sourceRect = new Rectangle(103, 469, 16, 16);
            return true;
        }

        if (this.copyComponent.containsPoint(x, y))
        {
            DesktopClipboard.SetText(this.SearchText);
            return true;
        }

        if (this.pasteComponent.containsPoint(x, y))
        {
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            this.SetSearchText(searchText, true);
            return true;
        }

        return false;
    }
}