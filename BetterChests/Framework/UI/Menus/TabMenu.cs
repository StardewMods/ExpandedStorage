namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Menu for customizing tabs.</summary>
internal sealed class TabMenu : SearchMenu
{
    private readonly ClickableTextureComponent addButton;
    private readonly IModConfig config;
    private readonly ConfigManager configManager;
    private readonly ClickableTextureComponent copyButton;
    private readonly ClickableTextureComponent editButton;
    private readonly IIconRegistry iconRegistry;
    private readonly ClickableTextureComponent okButton;
    private readonly ClickableTextureComponent pasteButton;
    private readonly ClickableTextureComponent removeButton;
    private readonly ClickableTextureComponent saveButton;

    private TabEditor? activeTab;

    /// <summary>Initializes a new instance of the <see cref="TabMenu" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public TabMenu(
        ConfigManager configManager,
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper)
        : base(expressionHandler, iconRegistry, inputHelper, reflectionHelper, string.Empty)
    {
        this.configManager = configManager;
        this.iconRegistry = iconRegistry;
        this.config = this.configManager.GetNew();

        this.saveButton = iconRegistry
            .RequireIcon(InternalIcon.Save)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + Game1.tileSize + 16);

        this.saveButton.hoverText = I18n.Ui_Save_Name();

        this.copyButton = iconRegistry
            .RequireIcon(InternalIcon.Copy)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 2));

        this.copyButton.hoverText = I18n.Ui_Copy_Tooltip();

        this.pasteButton = iconRegistry
            .RequireIcon(InternalIcon.Paste)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 3));

        this.pasteButton.hoverText = I18n.Ui_Paste_Tooltip();

        this.editButton = iconRegistry
            .RequireIcon(VanillaIcon.ColorPicker)
            .GetComponent(
                IconStyle.Transparent,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 4));

        this.editButton.hoverText = I18n.Ui_Edit_Tooltip();

        this.addButton = iconRegistry
            .RequireIcon(VanillaIcon.Plus)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 5));

        this.addButton.hoverText = I18n.Ui_Add_Tooltip();

        this.removeButton = iconRegistry
            .RequireIcon(VanillaIcon.Trash)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 6));

        this.removeButton.hoverText = I18n.Ui_Remove_Tooltip();

        this.okButton = iconRegistry
            .RequireIcon(VanillaIcon.Ok)
            .GetComponent(
                IconStyle.Transparent,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + this.height - Game1.tileSize - (IClickableMenu.borderWidth / 2));

        this.allClickableComponents.Add(this.saveButton);
        this.allClickableComponents.Add(this.copyButton);
        this.allClickableComponents.Add(this.pasteButton);
        this.allClickableComponents.Add(this.editButton);
        this.allClickableComponents.Add(this.addButton);
        this.allClickableComponents.Add(this.removeButton);
        this.allClickableComponents.Add(this.okButton);

        for (var i = this.config.InventoryTabList.Count - 1; i >= 0; i--)
        {
            if (!this.iconRegistry.TryGetIcon(this.config.InventoryTabList[i].Icon, out _))
            {
                this.config.InventoryTabList.RemoveAt(i);
            }
        }

        for (var i = 0; i < this.config.InventoryTabList.Count; i++)
        {
            var tabData = this.config.InventoryTabList[i];
            var icon = this.iconRegistry.RequireIcon(tabData.Icon);
            var tabIcon = new TabEditor(
                this.iconRegistry,
                this.Input,
                this.xPositionOnScreen - (Game1.tileSize * 2) - 256,
                this.yPositionOnScreen + (Game1.tileSize * (i + 1)) + 16,
                (Game1.tileSize * 2) + 256,
                icon,
                tabData)
            {
                Active = i == 0,
                Index = i,
            };

            tabIcon.Clicked += this.OnClicked;
            tabIcon.MoveDown += this.OnMoveDown;
            tabIcon.MoveUp += this.OnMoveUp;

            this.allClickableComponents.Add(tabIcon);

            if (i != 0)
            {
                continue;
            }

            this.activeTab = tabIcon;
            this.SetSearchText(tabData.SearchTerm, true);
        }
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) => true;

    /// <inheritdoc />
    protected override bool TryLeftClick(Point cursor)
    {
        if (this.saveButton.bounds.Contains(cursor) && this.readyToClose())
        {
            Game1.playSound("drumkit6");
            if (this.activeTab is not null)
            {
                this.config.InventoryTabList[this.activeTab.Index].SearchTerm = this.SearchText;
            }

            return true;
        }

        if (this.copyButton.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            DesktopClipboard.SetText(this.SearchText);
            return true;
        }

        if (this.pasteButton.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            this.SetSearchText(searchText, true);
            return true;
        }

        if (this.addButton.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            var tabData = new TabData
            {
                Icon = VanillaIcon.Plus.ToStringFast(),
                Label = I18n.Ui_NewTab_Name(),
            };

            var icon = this.iconRegistry.RequireIcon(VanillaIcon.Plus);
            var tabIcon = new TabEditor(
                this.iconRegistry,
                this.Input,
                this.xPositionOnScreen - (Game1.tileSize * 2) - 256,
                this.yPositionOnScreen + (Game1.tileSize * (this.config.InventoryTabList.Count + 1)) + 16,
                (Game1.tileSize * 2) + 256,
                icon,
                tabData)
            {
                Active = false,
                Index = this.config.InventoryTabList.Count,
            };

            tabIcon.Clicked += this.OnClicked;
            tabIcon.MoveDown += this.OnMoveDown;
            tabIcon.MoveUp += this.OnMoveUp;

            this.allClickableComponents.Add(tabIcon);
            this.config.InventoryTabList.Add(tabData);
            return true;
        }

        if (this.removeButton.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            if (this.activeTab is not null)
            {
                this.config.InventoryTabList.RemoveAt(this.activeTab.Index);
                for (var index = this.allClickableComponents.IndexOf(this.activeTab);
                    index < this.allClickableComponents.Count - 1;
                    index++)
                {
                    var current = (TabEditor)this.allClickableComponents[index];
                    var next = (TabEditor)this.allClickableComponents[index + 1];
                    (current.Index, next.Index) = (next.Index, current.Index);

                    var currentY = current.bounds.Y;
                    var nextY = next.bounds.Y;

                    current.MoveTo(new Point(current.bounds.X, nextY));
                    next.MoveTo(new Point(next.bounds.X, currentY));

                    (this.allClickableComponents[index], this.allClickableComponents[index + 1]) = (
                        this.allClickableComponents[index + 1], this.allClickableComponents[index]);
                }

                this.allClickableComponents.RemoveAt(this.allClickableComponents.Count - 1);
                this.activeTab = null;
            }

            return true;
        }

        if (this.okButton.bounds.Contains(cursor))
        {
            Game1.playSound("bigDeSelect");
            this.exitThisMenuNoSound();
            return true;
        }

        return false;
    }

    private void OnClicked(object? sender, TabClickedEventArgs e)
    {
        Game1.playSound("drumkit6");
        this.SetSearchText(e.Data.SearchTerm, true);
        if (this.activeTab is not null)
        {
            this.activeTab.Active = false;
        }

        this.activeTab = (TabEditor?)sender;
        if (this.activeTab is not null)
        {
            this.activeTab.Active = true;
        }
    }

    private void OnMoveDown(object? sender, TabClickedEventArgs e)
    {
        Game1.playSound("drumkit6");
        if (sender is not TabEditor tabEditor || tabEditor.Index >= this.config.InventoryTabList.Count - 1)
        {
            return;
        }

        (this.config.InventoryTabList[tabEditor.Index], this.config.InventoryTabList[tabEditor.Index + 1]) = (
            this.config.InventoryTabList[tabEditor.Index + 1], this.config.InventoryTabList[tabEditor.Index]);

        var index = this.allClickableComponents.IndexOf(tabEditor);
        var current = (TabEditor)this.allClickableComponents[index];
        var next = (TabEditor)this.allClickableComponents[index + 1];
        (current.Index, next.Index) = (next.Index, current.Index);

        var currentY = current.bounds.Y;
        var nextY = next.bounds.Y;

        current.MoveTo(new Point(current.bounds.X, nextY));
        next.MoveTo(new Point(next.bounds.X, currentY));

        (this.allClickableComponents[index], this.allClickableComponents[index + 1]) = (
            this.allClickableComponents[index + 1], this.allClickableComponents[index]);
    }

    private void OnMoveUp(object? sender, TabClickedEventArgs e)
    {
        Game1.playSound("drumkit6");
        if (sender is not TabEditor
            {
                Index: > 0,
            } tabEditor)
        {
            return;
        }

        (this.config.InventoryTabList[tabEditor.Index], this.config.InventoryTabList[tabEditor.Index - 1]) = (
            this.config.InventoryTabList[tabEditor.Index - 1], this.config.InventoryTabList[tabEditor.Index]);

        var index = this.allClickableComponents.IndexOf(tabEditor);
        var current = (TabEditor)this.allClickableComponents[index];
        var previous = (TabEditor)this.allClickableComponents[index - 1];
        (current.Index, previous.Index) = (previous.Index, current.Index);

        var currentY = current.bounds.Y;
        var previousY = previous.bounds.Y;

        current.MoveTo(new Point(current.bounds.X, previousY));
        previous.MoveTo(new Point(previous.bounds.X, currentY));

        (this.allClickableComponents[index], this.allClickableComponents[index - 1]) = (
            this.allClickableComponents[index - 1], this.allClickableComponents[index]);
    }
}