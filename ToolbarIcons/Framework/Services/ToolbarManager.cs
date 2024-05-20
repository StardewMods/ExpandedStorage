namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewValley.Menus;

/// <summary>Service for handling the toolbar icons on the screen.</summary>
internal sealed class ToolbarManager
{
    private readonly ConfigManager configManager;
    private readonly IEventManager eventManager;
    private readonly IIconRegistry iconRegistry;
    private readonly Dictionary<string, string?> icons;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<ClickableComponent?> lastButton = new();
    private readonly PerScreen<Toolbar?> lastToolbar = new();
    private readonly IReflectionHelper reflectionHelper;

    private Vector2 lastOrigin = Vector2.Zero;

    /// <summary>Initializes a new instance of the <see cref="ToolbarManager" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="icons">Dictionary containing all added icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public ToolbarManager(
        ConfigManager configManager,
        IEventManager eventManager,
        IIconRegistry iconRegistry,
        Dictionary<string, string?> icons,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper)
    {
        // Init
        this.configManager = configManager;
        this.eventManager = eventManager;
        this.iconRegistry = iconRegistry;
        this.icons = icons;
        this.inputHelper = inputHelper;
        this.reflectionHelper = reflectionHelper;

        // Events
        eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        eventManager.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
        eventManager.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        eventManager.Subscribe<RenderingHudEventArgs>(this.OnRenderingHud);
        eventManager.Subscribe<ReturnedToTitleEventArgs>(this.OnReturnedToTitle);
        eventManager.Subscribe<SaveLoadedEventArgs>(this.OnSaveLoaded);
    }

    private ClickableTextureComponent? ActiveComponent =>
        this
            .Toolbar?.allClickableComponents?.OfType<ClickableTextureComponent>()
            .FirstOrDefault(
                component => component.bounds.Contains(this.inputHelper.GetCursorPosition().GetScaledScreenPixels()));

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
        this.configManager.Visible
        && Context.IsPlayerFree
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
        if (!this.icons.TryAdd(id, hoverText))
        {
            return;
        }

        Log.Trace("Adding icon: {0}", id);
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

        Log.Trace("Removing icon: {0}", id);
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

        if (this.ActiveComponent is null)
        {
            return;
        }

        Game1.playSound("drumkit6");
        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(
            new IconPressedEventArgs(this.ActiveComponent.name, e.Button));

        this.inputHelper.Suppress(e.Button);
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!this.configManager.ToggleKey.JustPressed())
        {
            return;
        }

        var config = this.configManager.GetNew();
        config.Visible = !config.Visible;
        this.configManager.Save(config);
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e) => this.RefreshComponents(true);

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (!this.ShowToolbar)
        {
            return;
        }

        if (this.ActiveComponent is not null && !string.IsNullOrWhiteSpace(this.ActiveComponent.hoverText))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, this.ActiveComponent.hoverText, Game1.smallFont);
        }
    }

    private void OnRenderingHud(RenderingHudEventArgs e)
    {
        if (this.Toolbar is null)
        {
            return;
        }

        this.RefreshComponents();
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

        this.RefreshComponents(true);
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
        var origin = this.Toolbar.width > this.Toolbar.height
            ? new Vector2(
                this.Button.bounds.Left,
                yAlign ? this.Button.bounds.Bottom + 20 : this.Button.bounds.Top - 52)
            : new Vector2(
                xAlign ? this.Button.bounds.Right + 20 : this.Button.bounds.Left - 52,
                this.Button.bounds.Top);

        if (!force && this.lastOrigin.Equals(origin))
        {
            return;
        }

        this.lastOrigin = new Vector2(origin.X, origin.Y);
        var delta = this.Toolbar.width > this.Toolbar.height ? new Vector2(36, 0) : new Vector2(0, 36);
        this.Toolbar.allClickableComponents = [];
        foreach (var id in this.configManager.Icons.Where(icon => icon.Enabled).Select(icon => icon.Id).Distinct())
        {
            if (!this.icons.TryGetValue(id, out var hoverText) || !this.iconRegistry.TryGetIcon(id, out var icon))
            {
                continue;
            }

            var component = icon.GetComponent(IconStyle.Button, (int)origin.X, (int)origin.Y, 2f);
            component.name = id;
            component.hoverText = hoverText;
            this.Toolbar.allClickableComponents.Add(component);
            origin += delta;
        }
    }
}