namespace StardewMods.FauxCore.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Models;

/// <summary>Handles the config menu.</summary>
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IManifest manifest,
        IModHelper modHelper)
        : base(eventManager, modHelper)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.manifest = manifest;

        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <inheritdoc />
#if DEBUG
    public SimpleLogLevel LogLevel => SimpleLogLevel.More;
#else
    public SimpleLogLevel LogLevel => this.Config.LogLevel;
#endif

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupModConfigMenu();
        }
    }

    private void SetupModConfigMenu()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();

        // Register mod configuration
        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

        // general options
        gmcm.AddSectionTitle(this.manifest, I18n.Config_Section_General_Title, I18n.Config_Section_General_Description);

        gmcm.AddTextOption(
            this.manifest,
            () => config.LogLevel.ToStringFast(),
            value => config.LogLevel = SimpleLogLevelExtensions.TryParse(value, out var logLevel)
                ? logLevel
                : SimpleLogLevel.Less,
            I18n.Config_LogLevel_Title,
            I18n.Config_LogLevel_Description,
            SimpleLogLevelExtensions.GetNames(),
            LocalizedTextManager.TryFormat);
    }
}