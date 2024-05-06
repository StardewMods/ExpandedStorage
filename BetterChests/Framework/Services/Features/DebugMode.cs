namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.UI.Menus;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Feature used for debugging purposes.</summary>
internal sealed class DebugMode : BaseFeature<DebugMode>
{
    private readonly SearchHandler searchHandler;

    /// <summary>Initializes a new instance of the <see cref="DebugMode" /> class.</summary>
    /// <param name="commandHelper">Dependency used for handling console commands.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    public DebugMode(
        ICommandHelper commandHelper,
        IEventManager eventManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        SearchHandler searchHandler)
        : base(eventManager, log, manifest, modConfig)
    {
        // Init
        this.searchHandler = searchHandler;

        // Commands
        commandHelper.Add("bc_menu", I18n.Command_Menu(), this.ShowMenu);
    }

    /// <inheritdoc/>
#if DEBUG
    public override bool ShouldBeActive => true;
#else
    public override bool ShouldBeActive => this.Config.DebugMode;
#endif

    /// <inheritdoc />
    protected override void Activate() { }

    /// <inheritdoc />
    protected override void Deactivate() { }

    private void ShowMenu(string arg1, string[] arg2)
    {
        if (arg2.Length != 1 || Game1.activeClickableMenu?.readyToClose() == false)
        {
            return;
        }

        Game1.activeClickableMenu?.exitThisMenu();
        switch (arg2[0].Trim().ToLower(CultureInfo.InvariantCulture))
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