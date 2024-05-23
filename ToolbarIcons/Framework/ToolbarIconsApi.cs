namespace StardewMods.ToolbarIcons.Framework;

using Microsoft.Xna.Framework;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewMods.ToolbarIcons.Framework.Services;

/// <inheritdoc />
public sealed class ToolbarIconsApi : IToolbarIconsApi
{
    private readonly BaseEventManager eventManager;
    private readonly IIconRegistry iconRegistry;
    private readonly Dictionary<string, string> ids = [];
    private readonly IModInfo modInfo;
    private readonly string prefix;
    private readonly ToolbarManager toolbarManager;

    private EventHandler<string>? toolbarIconPressed;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsApi" /> class.</summary>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="toolbarManager">Dependency used for adding or removing icons on the toolbar.</param>
    internal ToolbarIconsApi(
        IModInfo modInfo,
        IEventManager eventManager,
        IIconRegistry iconRegistry,
        ToolbarManager toolbarManager)
    {
        // Init
        this.modInfo = modInfo;
        this.iconRegistry = iconRegistry;
        this.toolbarManager = toolbarManager;
        this.prefix = this.modInfo.Manifest.UniqueID + "/";
        this.eventManager = new BaseEventManager();

        // Events
        eventManager.Subscribe<IIconPressedEventArgs>(this.OnIconPressed);
    }

    /// <inheritdoc />
    public event EventHandler<string> ToolbarIconPressed
    {
        add
        {
            Log.WarnOnce(
                "{0} uses deprecated code. {1} event is deprecated. Please subscribe to the {2} event instead.",
                this.modInfo.Manifest.Name,
                nameof(this.ToolbarIconPressed),
                nameof(IIconPressedEventArgs));

            this.toolbarIconPressed += value;
        }
        remove => this.toolbarIconPressed -= value;
    }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText)
    {
        var uniqueId = $"{this.prefix}{id}";
        this.ids.Add(uniqueId, id);
        this.iconRegistry.AddIcon(uniqueId, texturePath, sourceRect ?? new Rectangle(0, 0, 16, 16));
        this.toolbarManager.AddIcon(uniqueId, hoverText);
    }

    /// <inheritdoc />
    public void AddToolbarIcon(IIcon icon, string? hoverText)
    {
        this.ids.Add(icon.UniqueId, icon.Id);
        this.toolbarManager.AddIcon(icon.UniqueId, hoverText);
    }

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id)
    {
        var uniqueId = $"{this.prefix}{id}";
        this.ids.Remove(uniqueId);
        this.toolbarManager.RemoveIcon(uniqueId);
    }

    /// <inheritdoc />
    public void RemoveToolbarIcon(IIcon icon)
    {
        this.ids.Remove(icon.UniqueId);
        this.toolbarManager.RemoveIcon(icon.UniqueId);
    }

    /// <inheritdoc />
    public void Subscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc />
    public void Unsubscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (!this.ids.TryGetValue(e.Id, out var id))
        {
            return;
        }

        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(id, e.Button));

        if (this.toolbarIconPressed is null)
        {
            return;
        }

        foreach (var handler in this.toolbarIconPressed.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(this, id);
            }
            catch (Exception ex)
            {
                Log.Error(
                    "{0} failed in {1}: {2}",
                    this.modInfo.Manifest.Name,
                    nameof(this.ToolbarIconPressed),
                    ex.Message);
            }
        }
    }
}