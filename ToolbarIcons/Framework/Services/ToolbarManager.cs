namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewValley.Menus;

// TODO: Center Toolbar Icons

/// <summary>Service for handling the toolbar icons on the screen.</summary>
internal sealed class ToolbarManager : BaseService
{
    private readonly Queue<Action> actionQueue = new();
    private readonly AssetHandler assetHandler;
    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly ConfigManager configManager;
    private readonly ContentPatcherIntegration contentPatcherIntegration;
    private readonly PerScreen<string> currentHoverText = new();
    private readonly IEventManager eventManager;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<ComponentArea> lastArea = new(() => ComponentArea.Custom);
    private readonly PerScreen<ClickableComponent> lastButton = new();
    private readonly PerScreen<Toolbar> lastToolbar = new();
    private readonly ILog log;
    private readonly IReflectionHelper reflectionHelper;

    private int ticks;

    /// <summary>Initializes a new instance of the <see cref="ToolbarManager" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="components">Dependency used for the toolbar icon components.</param>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="contentPatcherIntegration">Dependency used for integration with Content Patcher.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into external code.</param>
    public ToolbarManager(
        AssetHandler assetHandler,
        Dictionary<string, ClickableTextureComponent> components,
        ConfigManager configManager,
        ContentPatcherIntegration contentPatcherIntegration,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        // Init
        this.assetHandler = assetHandler;
        this.components = components;
        this.configManager = configManager;
        this.contentPatcherIntegration = contentPatcherIntegration;
        this.eventManager = eventManager;
        this.inputHelper = inputHelper;
        this.log = log;
        this.reflectionHelper = reflectionHelper;

        // Events
        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
        eventManager.Subscribe<ToolbarIconsLoadedEventArgs>(this.OnToolbarIconsLoaded);

        if (!contentPatcherIntegration.IsLoaded)
        {
            eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
            return;
        }

        eventManager.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
    }

    private static bool ShowToolbar =>
        Context.IsPlayerFree
        && !Game1.eventUp
        && Game1.farmEvent == null
        && Game1.displayHUD
        && Game1.activeClickableMenu is null
        && Game1.onScreenMenus.OfType<Toolbar>().Any();

    /// <summary>Adds an icon next to the <see cref="Toolbar" />.</summary>
    /// <param name="id">A unique identifier for the icon.</param>
    /// <param name="getTexture">Get method for the texture.</param>
    /// <param name="sourceRect">The source rectangle of the icon.</param>
    /// <param name="hoverText">Text to appear when hovering over the icon.</param>
    public void AddToolbarIcon(string id, Func<Texture2D> getTexture, Rectangle? sourceRect, string? hoverText) =>
        this.actionQueue.Enqueue(
            () =>
            {
                var icon = this.configManager.Icons.FirstOrDefault(
                    icon => icon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                if (icon is null)
                {
                    icon = new ToolbarIcon(id);
                    var config = this.configManager.GetNew();
                    config.Icons.Add(icon);
                    this.configManager.Save(config);
                }

                if (this.components.ContainsKey(id))
                {
                    return;
                }

                this.log.Trace("Adding icon: {0}", id);
                this.components.Add(
                    id,
                    new ClickableTextureComponent(
                        new Rectangle(0, 0, 32, 32),
                        getTexture(),
                        sourceRect ?? new Rectangle(0, 0, 16, 16),
                        2f)
                    {
                        hoverText = hoverText,
                        name = id,
                        visible = icon.Enabled,
                    });
            });

    /// <summary>Removes an icon.</summary>
    /// <param name="id">A unique identifier for the icon.</param>
    public void RemoveToolbarIcon(string id) =>
        this.actionQueue.Enqueue(
            () =>
            {
                var toolbarIcon = this.configManager.Icons.FirstOrDefault(
                    toolbarIcon => toolbarIcon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                if (toolbarIcon is null)
                {
                    return;
                }

                this.log.Trace("Removing icon: {0}", id);
                var config = this.configManager.GetNew();
                config.Icons.Remove(toolbarIcon);
                this.configManager.Save(config);
                this.components.Remove(id);
            });

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!ToolbarManager.ShowToolbar || this.inputHelper.IsSuppressed(e.Button))
        {
            return;
        }

        if (e.Button is not (SButton.MouseLeft or SButton.MouseRight)
            && !(e.Button.IsActionButton() || e.Button.IsUseToolButton()))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var component =
            this.components.Values.FirstOrDefault(component => component.visible && component.containsPoint(x, y));

        if (component is null)
        {
            return;
        }

        Game1.playSound("drumkit6");
        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(
            new IconPressedEventArgs(component.name, e.Button));

        this.inputHelper.Suppress(e.Button);
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs e) =>
        this.eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e)
    {
        foreach (var icon in this.configManager.Icons)
        {
            if (this.components.TryGetValue(icon.Id, out var component))
            {
                component.visible = icon.Enabled;
            }
        }

        this.ReorientComponents();
    }

    private void OnCursorMoved(CursorMovedEventArgs e)
    {
        if (!ToolbarManager.ShowToolbar)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.currentHoverText.Value = string.Empty;
        foreach (var component in this.components.Values.Where(component => component.visible))
        {
            component.tryHover(x, y);
            if (component.bounds.Contains(x, y))
            {
                this.currentHoverText.Value = component.hoverText;
            }
        }
    }

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (!ToolbarManager.ShowToolbar)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(this.currentHoverText.Value))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, this.currentHoverText.Value, Game1.smallFont);
        }
    }

    private void OnRenderingHud(RenderingHudEventArgs e)
    {
        if (!ToolbarManager.ShowToolbar)
        {
            return;
        }

        this.ReorientComponents();

        foreach (var component in this.components.Values.Where(component => component.visible))
        {
            e.SpriteBatch.Draw(
                this.assetHandler.Icons.Value,
                new Vector2(component.bounds.X, component.bounds.Y),
                new Rectangle(0, 0, 16, 16),
                Color.White,
                0f,
                Vector2.Zero,
                2f,
                SpriteEffects.None,
                1f);

            component.draw(e.SpriteBatch);
        }
    }

    private void OnToolbarIconsLoaded(ToolbarIconsLoadedEventArgs e)
    {
        // Init
        this.ReorientComponents();

        // Events
        this.eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.eventManager.Subscribe<CursorMovedEventArgs>(this.OnCursorMoved);
        this.eventManager.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.eventManager.Subscribe<RenderingHudEventArgs>(this.OnRenderingHud);
    }

    private void OnUpdateTicked(UpdateTickedEventArgs e)
    {
        if (!this.contentPatcherIntegration.IsLoaded && ++this.ticks < 2)
        {
            return;
        }

        while (this.actionQueue.TryDequeue(out var action))
        {
            action();
        }
    }

    private void ReorientComponents()
    {
        if (!this.TryGetButton(out var button) || this.components.Values.All(component => !component.visible))
        {
            return;
        }

        var xAlign = button.bounds.X * (1f / Game1.options.zoomLevel) < Game1.viewport.Width / 2;
        var yAlign = button.bounds.Y * (1f / Game1.options.zoomLevel) < Game1.viewport.Height / 2;
        ComponentArea area;
        int x;
        int y;
        if (this.lastToolbar.Value.width > this.lastToolbar.Value.height)
        {
            x = button.bounds.Left;
            if (yAlign)
            {
                area = ComponentArea.Top;
                y = button.bounds.Bottom + 20;
            }
            else
            {
                area = ComponentArea.Bottom;
                y = button.bounds.Top - 52;
            }
        }
        else
        {
            y = button.bounds.Top;
            if (xAlign)
            {
                area = ComponentArea.Left;
                x = button.bounds.Right + 20;
            }
            else
            {
                area = ComponentArea.Right;
                x = button.bounds.Left - 52;
            }
        }

        this.ReorientComponents(area, x, y);
    }

    private void ReorientComponents(ComponentArea area, int x, int y)
    {
        this.lastArea.Value = area;
        foreach (var icon in this.configManager.Icons)
        {
            if (this.components.TryGetValue(icon.Id, out var component))
            {
                if (!icon.Enabled)
                {
                    component.visible = false;
                    continue;
                }

                component.visible = true;
                component.bounds.X = x;
                component.bounds.Y = y;
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
                    case ComponentArea.Custom:
                    default:
                        break;
                }
            }
        }
    }

    private bool TryGetButton([NotNullWhen(true)] out ClickableComponent? button)
    {
        var activeToolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
        if (this.lastToolbar.IsActiveForScreen() && activeToolbar == this.lastToolbar.Value)
        {
            button = this.lastButton.Value;
            return true;
        }

        if (activeToolbar is null)
        {
            button = null;
            return false;
        }

        this.lastToolbar.Value = activeToolbar;
        var buttons = this.reflectionHelper.GetField<List<ClickableComponent>>(activeToolbar, "buttons").GetValue();
        button = this.lastButton.Value = buttons.First();
        return true;
    }
}