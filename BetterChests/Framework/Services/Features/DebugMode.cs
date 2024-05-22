namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI.Menus;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <summary>Feature used for debugging purposes.</summary>
internal sealed class DebugMode : BaseFeature<DebugMode>
{
    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly IExpressionHandler expressionHandler;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly IReflectionHelper reflectionHelper;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="DebugMode" /> class.</summary>
    /// <param name="commandHelper">Dependency used for handling console commands.</param>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations by containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public DebugMode(
        ICommandHelper commandHelper,
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(eventManager, configManager)
    {
        // Init
        this.configManager = configManager;
        this.containerFactory = containerFactory;
        this.containerHandler = containerHandler;
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.reflectionHelper = reflectionHelper;
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
                this.Configure(args);
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
        if (!this.toolbarIconsIntegration.IsLoaded || !this.iconRegistry.TryGetIcon(InternalIcon.Debug, out var icon))
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(this.Id, icon.Path, icon.Area, I18n.Button_Debug_Name());
        this.toolbarIconsIntegration.Api.Subscribe(this.OnIconPressed);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.Id);
        this.toolbarIconsIntegration.Api.Unsubscribe(this.OnIconPressed);
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (e.Id != this.Id || Game1.activeClickableMenu?.readyToClose() == false)
        {
            return;
        }

        Game1.activeClickableMenu?.exitThisMenu();
        Game1.activeClickableMenu = new DebugMenu(this, this.inputHelper);
    }

    private void ResetAll()
    {
        var defaultOptions = new DefaultStorageOptions();
        foreach (var container in this.containerFactory.GetAll())
        {
            defaultOptions.CopyTo(container);
        }
    }

    private void Configure(IReadOnlyList<string> args)
    {
        if (args.Count != 1)
        {
            return;
        }

        switch (args[0])
        {
            case "backpack":
                if (!this.containerFactory.TryGetOne(Game1.player, out var container))
                {
                    return;
                }

                this.containerHandler.Configure(container);
                return;

            default: return;
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
                Game1.activeClickableMenu = new ConfigMenu(this.inputHelper);
                return;
            case "layout":
                Game1.activeClickableMenu = new LayoutMenu(this.inputHelper);
                return;
            case "search":
                Game1.activeClickableMenu = new SearchMenu(
                    this.expressionHandler,
                    this.iconRegistry,
                    this.inputHelper,
                    this.reflectionHelper,
                    "({category}~\"fish\" !{tags}~\"ocean\" [{quality}~iridium {quality}~gold])");

                return;
            case "tab":
                Game1.activeClickableMenu = new TabMenu(
                    this.configManager,
                    this.expressionHandler,
                    this.iconRegistry,
                    this.inputHelper,
                    this.reflectionHelper);

                return;
        }
    }
}