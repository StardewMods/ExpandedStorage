namespace StardewMods.ExpandedStorage.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ExpandedStorage.Framework.Interfaces;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;

/// <summary>Handles the config menu.</summary>
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly BetterChestsIntegration betterChestsIntegration;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IModHelper modHelper;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="betterChestsIntegration">Dependency for Better Chests integration.</param>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        BetterChestsIntegration betterChestsIntegration,
        ContentPatcherIntegration contentPatcherIntegration,
        IDataHelper dataHelper,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IModHelper modHelper)
        : base(contentPatcherIntegration, dataHelper, eventManager, modHelper)
    {
        this.betterChestsIntegration = betterChestsIntegration;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.modHelper = modHelper;

        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <inheritdoc />
    public Dictionary<string, DefaultStorageOptions> StorageOptions => this.Config.StorageOptions;

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(name => name.IsEquivalentTo("Data/BigCraftables")))
        {
            this.SetupModConfigMenu();
        }
    }

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupModConfigMenu();
        }
    }

    private void SetupModConfigMenu()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded || !this.betterChestsIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.modHelper.ReadConfig<DefaultConfig>();

        // Register mod configuration
        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

        var storages =
            Game1
                .bigCraftableData.Where(kvp => kvp.Value.CustomFields?.GetBool($"{Mod.Id}/Enabled") == true)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (var (id, data) in storages)
        {
            gmcm.AddPageLink(
                Mod.Manifest,
                id,
                () => TokenParser.ParseText(data.Name),
                () => TokenParser.ParseText(data.Description));

            config.StorageOptions.TryAdd(id, new DefaultStorageOptions());
        }

        config.StorageOptions.RemoveWhere(option => !storages.ContainsKey(option.Key));

        foreach (var (id, options) in config.StorageOptions)
        {
            if (!storages.TryGetValue(id, out var bigCraftableData))
            {
                continue;
            }

            this.betterChestsIntegration.Api.AddConfigOptions(
                Mod.Manifest,
                id,
                () => TokenParser.ParseText(bigCraftableData.DisplayName),
                options);
        }
    }
}