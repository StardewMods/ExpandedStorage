namespace StardewMods.GarbageDay.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.GarbageDay.Framework.Interfaces;
using StardewMods.GarbageDay.Framework.Models;

/// <inheritdoc cref="StardewMods.GarbageDay.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : Mod.ConfigManager<DefaultConfig>, IModConfig
{
    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        ContentPatcherIntegration contentPatcherIntegration,
        IDataHelper dataHelper,
        IEventManager eventManager,
        IModHelper modHelper)
        : base(contentPatcherIntegration, dataHelper, eventManager, modHelper) { }

    /// <inheritdoc />
    public DayOfWeek GarbageDay => this.Config.GarbageDay;

    /// <inheritdoc />
    public bool OnByDefault => this.Config.OnByDefault;
}