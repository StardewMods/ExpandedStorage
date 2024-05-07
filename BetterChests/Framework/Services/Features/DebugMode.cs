namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI.Menus;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <summary>Feature used for debugging purposes.</summary>
internal sealed class DebugMode : BaseFeature<DebugMode>
{
    private readonly AssetHandler assetHandler;
    private readonly ConfigureChest configureChest;
    private readonly ContainerFactory containerFactory;
    private readonly SearchHandler searchHandler;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="DebugMode" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="commandHelper">Dependency used for handling console commands.</param>
    /// <param name="configureChest">Dependency used for configuring storages.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public DebugMode(
        AssetHandler assetHandler,
        ICommandHelper commandHelper,
        ConfigureChest configureChest,
        ContainerFactory containerFactory,
        IEventManager eventManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        SearchHandler searchHandler,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(eventManager, log, manifest, modConfig)
    {
        // Init
        this.assetHandler = assetHandler;
        this.configureChest = configureChest;
        this.containerFactory = containerFactory;
        this.searchHandler = searchHandler;
        this.toolbarIconsIntegration = toolbarIconsIntegration;

        // Commands
        commandHelper.Add("bc_config", I18n.Command_PlayerConfig(), this.Command);
        commandHelper.Add("bc_reset", I18n.Command_ResetAll(), this.Command);
        commandHelper.Add("bc_menu", I18n.Command_Menu(), this.Command);
    }

    /// <inheritdoc />
#if DEBUG
    public override bool ShouldBeActive => true;
#else
    public override bool ShouldBeActive => this.Config.DebugMode;
#endif

    /// <summary>Executes a command based on the provided command string and arguments.</summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="args">The arguments for the command.</param>
    public void Command(string command, string[] args)
    {
        switch (command)
        {
            case "bc_config":
                this.configureChest.Command("bc_config", args);
                return;
            case "bc_reset":
                this.ResetAll();
                return;
            case "bc_menu":
                this.ShowMenu(args);
                return;
        }
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        if (this.toolbarIconsIntegration.IsLoaded
            && this.assetHandler.Icons.TryGetValue(this.ModId + "/Debug", out var icon))
        {
            this.toolbarIconsIntegration.Api.AddToolbarIcon(this.Id, icon.Path, icon.Area, I18n.Button_Debug_Name());

            this.toolbarIconsIntegration.Api.Subscribe(this.OnIconPressed);
        }
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        if (this.toolbarIconsIntegration.IsLoaded)
        {
            this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.Id);
            this.toolbarIconsIntegration.Api.Unsubscribe(this.OnIconPressed);
        }
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (Game1.activeClickableMenu?.readyToClose() == false)
        {
            return;
        }

        Game1.activeClickableMenu?.exitThisMenu();
        Game1.activeClickableMenu = new DebugMenu(this);
    }

    private void ResetAll()
    {
        var defaultOptions = new DefaultStorageOptions();
        foreach (var container in this.containerFactory.GetAll())
        {
            defaultOptions.CopyTo(container);
        }
    }

    private void ShowMenu(IReadOnlyList<string> args)
    {
        if (args.Count != 1 || Game1.activeClickableMenu?.readyToClose() == false)
        {
            return;
        }

        Game1.activeClickableMenu?.exitThisMenu();
        switch (args[0].Trim().ToLower(CultureInfo.InvariantCulture))
        {
            case "config":
                Game1.activeClickableMenu = new ConfigMenu();
                return;
            case "layout":
                Game1.activeClickableMenu = new LayoutMenu();
                return;
            case "search":
                Game1.activeClickableMenu = new SearchMenu(this.searchHandler);
                return;
            case "sort":
                Game1.activeClickableMenu = new SortMenu();
                return;
            case "tab":
                Game1.activeClickableMenu = new TabMenu();
                return;
        }
    }
}