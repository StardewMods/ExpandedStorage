namespace StardewMods.CrystallineJunimoChests.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.CrystallineJunimoChests.Framework.Interfaces;
using StardewMods.CrystallineJunimoChests.Framework.Models;

/// <inheritdoc cref="StardewMods.CrystallineJunimoChests.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        ContentPatcherIntegration contentPatcherIntegration,
        IDataHelper dataHelper,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IModHelper modHelper)
        : base(contentPatcherIntegration, dataHelper, eventManager, modHelper)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <inheritdoc />
    public int GemCost => this.Config.GemCost;

    /// <inheritdoc />
    public string Sound => this.Config.Sound;

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

        gmcm.AddNumberOption(
            Mod.Manifest,
            () => config.GemCost,
            value => config.GemCost = value,
            I18n.Config_GemCost_Name,
            I18n.Config_GemCost_Tooltip);

        gmcm.AddTextOption(
            Mod.Manifest,
            () => config.Sound,
            value => config.Sound = value,
            I18n.Config_Sound_Name,
            I18n.Config_Sound_Tooltip);
    }
}