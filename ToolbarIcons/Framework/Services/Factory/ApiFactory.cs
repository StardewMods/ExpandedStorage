namespace StardewMods.ToolbarIcons.Framework.Services.Factory;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <inheritdoc />
internal sealed class ApiFactory : IApiFactory<IToolbarIconsApi>
{
    private readonly IEventManager eventManager;
    private readonly IIconRegistry iconRegistry;
    private readonly ToolbarManager toolbarManager;

    /// <summary>Initializes a new instance of the <see cref="ApiFactory" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="toolbarManager">Dependency used for adding or removing icons on the toolbar.</param>
    public ApiFactory(IEventManager eventManager, IIconRegistry iconRegistry, ToolbarManager toolbarManager)
    {
        this.eventManager = eventManager;
        this.iconRegistry = iconRegistry;
        this.toolbarManager = toolbarManager;
    }

    /// <inheritdoc />
    public IToolbarIconsApi CreateApi(IModInfo modInfo) =>
        new ToolbarIconsApi(modInfo, this.eventManager, this.iconRegistry, this.toolbarManager);
}