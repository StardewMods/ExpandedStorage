namespace StardewMods.ToolbarIcons.Framework.Services;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewValley.Menus;

// TODO: Center Toolbar Icons

/// <summary>Service for handling the toolbar icons on the screen.</summary>
internal sealed class ToolbarManager : BaseService
{
    private readonly ConfigManager configManager;
    private readonly IEventManager eventManager;
    private readonly IIconRegistry iconRegistry;
    private readonly Dictionary<string, string?> icons;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<ComponentArea> lastArea = new(() => ComponentArea.Custom);
    private readonly PerScreen<ClickableComponent?> lastButton = new();
    private readonly PerScreen<Toolbar?> lastToolbar = new();
    private readonly ILog log;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="ToolbarManager" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="icons">Dictionary containing all added icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public ToolbarManager(
        ConfigManager configManager,
        IEventManager eventManager,
        IIconRegistry iconRegistry,
        Dictionary<string, string?> icons,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        // Init
        this.configManager = configManager;
        this.eventManager = eventManager;
        this.iconRegistry = iconRegistry;
        this.icons = icons;
        this.inputHelper = inputHelper;
        this.log = log;
        this.reflectionHelper = reflectionHelper;

        // Events
        eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
        eventManager.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        eventManager.Subscribe<RenderingHudEventArgs>(this.OnRenderingHud);
        eventManager.Subscribe<ReturnedToTitleEventArgs>(this.OnReturnedToTitle);
        eventManager.Subscribe<SaveLoadedEventArgs>(this.OnSaveLoaded);
    }

    private ClickableComponent? Button
    {
        get
        {
            if (this.lastButton.Value is not null)
            {
                return this.lastButton.Value;
            }

            if (this.Toolbar is null)
            {
                return null;
            }

            this.lastButton.Value =
                this.reflectionHelper.GetField<List<ClickableComponent>>(this.Toolbar, "buttons").GetValue().First();

            return this.lastButton.Value;
        }
    }

    [MemberNotNullWhen(true, nameof(ToolbarManager.Toolbar))]
    private bool ShowToolbar =>
        Context.IsPlayerFree
        && !Game1.eventUp
        && Game1.farmEvent == null
        && Game1.displayHUD
        && Game1.activeClickableMenu is null
        && this.Toolbar is not null;

    private Toolbar? Toolbar => this.lastToolbar.Value ??= Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();

    /// <summary>Adds an icon next to the <see cref="Toolbar" />.</summary>
    /// <param name="id">A unique identifier for the icon.</param>
    /// <param name="hoverText">Text to appear when hovering over the icon.</param>
    public void AddIcon(string id, string? hoverText)
    {
        this.log.Trace("Adding icon: {0}", id);
        this.icons.Add(id, hoverText);
        this.eventManager.Publish(new IconsChangedEventArgs([id], []));
        this.RefreshComponents(true);
    }

    /// <summary>Removes an icon.</summary>
    /// <param name="id">A unique identifier for the icon.</param>
    public void RemoveIcon(string id)
    {
        if (!this.icons.ContainsKey(id))
        {
            return;
        }

        this.log.Trace("Removing icon: {0}", id);
        this.icons.Remove(id);
        this.eventManager.Publish(new IconsChangedEventArgs([], [id]));
        this.RefreshComponents(true);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!this.ShowToolbar || this.inputHelper.IsSuppressed(e.Button))
        {
            return;
        }

        if (!e.Button.IsActionButton() && !e.Button.IsUseToolButton())
        {
            return;
        }

        var cursorPos = e.Cursor.GetScaledScreenPixels().ToPoint();
        var component =
            this.Toolbar.allClickableComponents.FirstOrDefault(
                component => component.containsPoint(cursorPos.X, cursorPos.Y));

        if (component is null)
        {
            return;
        }

        Game1.playSound("drumkit6");
        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(
            new IconPressedEventArgs(component.name, e.Button));

        this.inputHelper.Suppress(e.Button);
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e) => this.RefreshComponents(true);

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (!this.ShowToolbar)
        {
            return;
        }

        var cursorPos = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        var component =
            this
                .Toolbar.allClickableComponents.OfType<ClickableTextureComponent>()
                .FirstOrDefault(component => component.containsPoint(cursorPos.X, cursorPos.Y));

        if (component is not null && !string.IsNullOrWhiteSpace(component.hoverText))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, component.hoverText, Game1.smallFont);
        }
    }

    private void OnRenderingHud(RenderingHudEventArgs e)
    {
        if (!this.ShowToolbar)
        {
            return;
        }

        var cursorPos = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        foreach (var component in this.Toolbar.allClickableComponents.OfType<ClickableTextureComponent>())
        {
            component.tryHover(cursorPos.X, cursorPos.Y);
            component.draw(e.SpriteBatch);
        }
    }

    private void OnReturnedToTitle(ReturnedToTitleEventArgs e)
    {
        this.lastButton.Value = null;
        this.lastToolbar.Value = null;
    }

    private void OnSaveLoaded(SaveLoadedEventArgs e)
    {
        if (this.Toolbar is null)
        {
            return;
        }

        this.Toolbar.allClickableComponents ??= [];
        this.RefreshComponents();
    }

    private void RefreshComponents(bool force = false)
    {
        if (this.Button is null || this.Toolbar is null)
        {
            return;
        }

        // Calculate top-left
        var xAlign = this.Button.bounds.X * (1f / Game1.options.zoomLevel) < Game1.viewport.Width / 2f;
        var yAlign = this.Button.bounds.Y * (1f / Game1.options.zoomLevel) < Game1.viewport.Height / 2f;
        ComponentArea area;
        int x;
        int y;
        if (this.Toolbar.width > this.Toolbar.height)
        {
            x = this.Button.bounds.Left;
            if (yAlign)
            {
                area = ComponentArea.Top;
                y = this.Button.bounds.Bottom + 20;
            }
            else
            {
                area = ComponentArea.Bottom;
                y = this.Button.bounds.Top - 52;
            }
        }
        else
        {
            y = this.Button.bounds.Top;
            if (xAlign)
            {
                area = ComponentArea.Left;
                x = this.Button.bounds.Right + 20;
            }
            else
            {
                area = ComponentArea.Right;
                x = this.Button.bounds.Left - 52;
            }
        }

        if (!force && this.lastArea.Value == area)
        {
            return;
        }

        this.lastArea.Value = area;
        this.Toolbar.allClickableComponents ??= [];
        this.Toolbar.allClickableComponents.Clear();
        foreach (var toolbarIcon in this.configManager.Icons)
        {
            if (!this.icons.TryGetValue(toolbarIcon.Id, out var hoverText)
                || !this.iconRegistry.TryGetIcon(toolbarIcon.Id, out var icon))
            {
                continue;
            }

            var component = icon.GetComponent(IconStyle.Button, x, y, 2f);
            component.name = toolbarIcon.Id;
            component.hoverText = hoverText;
            component.visible = toolbarIcon.Enabled;
            this.Toolbar.allClickableComponents.Add(component);

            switch (area)
            {
                case ComponentArea.Top:
                case ComponentArea.Bottom:
                    x += component.bounds.Width + 4;
                    break;
                case ComponentArea.Right:
                case ComponentArea.Left:
                    y += component.bounds.Height + 4;
                    break;
            }
        }
    }
}