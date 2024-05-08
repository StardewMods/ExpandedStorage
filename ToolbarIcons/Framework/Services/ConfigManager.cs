namespace StardewMods.ToolbarIcons.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewMods.ToolbarIcons.Framework.UI;
using StardewValley.Menus;

/// <summary>Handles generic mod config menu.</summary>
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly Func<ToolbarIconOption> getToolbarIconsOption;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="components">Dependency used for the toolbar icon components.</param>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="getToolbarIconsOption">Gets a new instance of <see cref="ToolbarIconOption" />.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        Dictionary<string, ClickableTextureComponent> components,
        IDataHelper dataHelper,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        Func<ToolbarIconOption> getToolbarIconsOption,
        IManifest manifest,
        IModHelper modHelper)
        : base(dataHelper, eventManager, modHelper)
    {
        this.manifest = manifest;
        this.components = components;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.getToolbarIconsOption = getToolbarIconsOption;

        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
        eventManager.Subscribe<ToolbarIconsLoadedEventArgs>(this.OnToolbarIconsLoaded);
    }

    /// <inheritdoc />
    public List<ToolbarIcon> Icons => this.Config.Icons;

    /// <inheritdoc />
    public float Scale => this.Config.Scale;

    /// <inheritdoc />
    public override DefaultConfig GetDefault()
    {
        var defaultConfig = base.GetDefault();

        // Add icons to config with default sorting
        defaultConfig.Icons.Sort((i1, i2) => string.Compare(i1.Id, i2.Id, StringComparison.OrdinalIgnoreCase));

        return defaultConfig;
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e) => this.ReloadConfig();

    private void OnToolbarIconsLoaded(ToolbarIconsLoadedEventArgs e) => this.ReloadConfig();

    private void ReloadConfig()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();
        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

        gmcm.AddSectionTitle(this.manifest, I18n.Config_CustomizeToolbar_Name, I18n.Config_CustomizeToolbar_Tooltip);

        var index = 0;
        while (index < config.Icons.Count)
        {
            var icon = config.Icons[index];
            if (!this.components.ContainsKey(icon.Id))
            {
                config.Icons.RemoveAt(index);
                continue;
            }

            var toolbarIconsOption = this.getToolbarIconsOption();
            toolbarIconsOption.Init(index);
            this.genericModConfigMenuIntegration.AddComplexOption(toolbarIconsOption);
            index++;
        }
    }
}