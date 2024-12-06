namespace StardewMods.ToolbarIcons.Framework.Services;

using StardewModdingAPI.Utilities;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewMods.ToolbarIcons.Framework.Services.Factory;

/// <summary>Handles generic mod config menu.</summary>
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly ComplexOptionFactory complexOptionFactory;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly Dictionary<string, string?> icons;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="complexOptionFactory">Dependency used for creating complex options.</param>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="icons">Dictionary containing all added icons.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        ComplexOptionFactory complexOptionFactory,
        ContentPatcherIntegration contentPatcherIntegration,
        IDataHelper dataHelper,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        Dictionary<string, string?> icons,
        IModHelper modHelper)
        : base(contentPatcherIntegration, dataHelper, eventManager, modHelper)
    {
        this.complexOptionFactory = complexOptionFactory;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.icons = icons;

        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
        eventManager.Subscribe<IconsChangedEventArgs>(this.OnIconsChanged);
        eventManager.Subscribe<ToolbarIconsLoadedEventArgs>(this.OnToolbarIconsLoaded);
    }

    /// <inheritdoc />
    public List<ToolbarIcon> Icons => this.Config.Icons;

    /// <inheritdoc />
    public bool PlaySound => this.Config.PlaySound;

    /// <inheritdoc />
    public float Scale => this.Config.Scale;

    /// <inheritdoc />
    public bool ShowTooltip => this.Config.ShowTooltip;

    /// <inheritdoc />
    public KeybindList ToggleKey => this.Config.ToggleKey;

    /// <inheritdoc />
    public bool Visible => this.Config.Visible;

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e) => this.ReloadConfig();

    private void OnIconsChanged(IconsChangedEventArgs e)
    {
        this.Config.Icons.RemoveAll(icon => e.Removed.Contains(icon.Id));
        this.Config.Icons.AddRange(e.Added.Select(id => new ToolbarIcon(id)));
    }

    private void OnToolbarIconsLoaded(ToolbarIconsLoadedEventArgs e) => this.ReloadConfig();

    private void ReloadConfig()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();
        config.Icons.RemoveAll(icon => !this.icons.ContainsKey(icon.Id));
        config.Icons.AddRange(
            this.icons.Keys.Where(id => config.Icons.All(icon => icon.Id != id)).Select(id => new ToolbarIcon(id)));

        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

        gmcm.AddKeybindList(
            Mod.Manifest,
            () => config.ToggleKey,
            value => config.ToggleKey = value,
            I18n.Config_ToggleKey_Name,
            I18n.Config_ToggleKey_Tooltip);

        gmcm.AddBoolOption(
            Mod.Manifest,
            () => config.PlaySound,
            value => config.PlaySound = value,
            I18n.Config_PlaySound_Name,
            I18n.Config_PlaySound_Tooltip);

        gmcm.AddBoolOption(
            Mod.Manifest,
            () => config.ShowTooltip,
            value => config.ShowTooltip = value,
            I18n.Config_ShowTooltip_Name,
            I18n.Config_ShowTooltip_Tooltip);

        gmcm.AddBoolOption(
            Mod.Manifest,
            () => config.Visible,
            value => config.Visible = value,
            I18n.Config_Visible_Name,
            I18n.Config_Visible_Tooltip);

        gmcm.AddSectionTitle(Mod.Manifest, I18n.Config_CustomizeToolbar_Name, I18n.Config_CustomizeToolbar_Tooltip);
        var total = config.Icons.Count;

        for (var index = 0; index < config.Icons.Count; index++)
        {
            if (!this.complexOptionFactory.TryGetToolbarIconOption(
                GetCurrentId(index),
                GetTooltip(index),
                GetEnabled(index),
                SetEnabled(index),
                GetMoveDown(index),
                GetMoveUp(index),
                out var option))
            {
                total--;
                continue;
            }

            this.genericModConfigMenuIntegration.AddComplexOption(option);
        }

        return;

        Func<string> GetCurrentId(int i) => () => config.Icons[i].Id;

        Func<bool> GetEnabled(int i) => () => config.Icons[i].Enabled;

        Action<bool> SetEnabled(int i) => enabled => config.Icons[i].Enabled = enabled;

        Func<string> GetTooltip(int i) =>
            () => this.icons.TryGetValue(config.Icons[i].Id, out var hoverText)
                ? hoverText ?? string.Empty
                : string.Empty;

        Action? GetMoveDown(int i)
        {
            if (i == total - 1)
            {
                return null;
            }

            return () =>
            {
                (config.Icons[i], config.Icons[i + 1]) = (config.Icons[i + 1], config.Icons[i]);
            };
        }

        Action? GetMoveUp(int i)
        {
            if (i == 0)
            {
                return null;
            }

            return () =>
            {
                (config.Icons[i], config.Icons[i - 1]) = (config.Icons[i - 1], config.Icons[i]);
            };
        }
    }
}