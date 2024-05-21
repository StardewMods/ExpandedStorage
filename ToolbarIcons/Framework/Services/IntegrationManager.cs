namespace StardewMods.ToolbarIcons.Framework.Services;

using System.Reflection;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewValley.Menus;

/// <summary>Base class for adding toolbar icons for integrated mods.</summary>
internal sealed class IntegrationManager
{
    private readonly Dictionary<string, Action> actions = new();
    private readonly IEnumerable<ICustomIntegration> customIntegrations;
    private readonly IModRegistry modRegistry;
    private readonly MethodInfo overrideButtonReflected;
    private readonly IReflectionHelper reflectionHelper;
    private readonly ToolbarManager toolbarManager;

    /// <summary>Initializes a new instance of the <see cref="IntegrationManager" /> class.</summary>
    /// <param name="customIntegrations">Integrations directly supported by the mod.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="modRegistry">Dependency for fetching metadata about loaded mods.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    /// <param name="toolbarManager">Dependency used for adding or removing icons on the toolbar.</param>
    public IntegrationManager(
        IEnumerable<ICustomIntegration> customIntegrations,
        IEventManager eventManager,
        IModRegistry modRegistry,
        IReflectionHelper reflectionHelper,
        ToolbarManager toolbarManager)
    {
        // Init
        this.customIntegrations = customIntegrations;
        this.modRegistry = modRegistry;
        this.reflectionHelper = reflectionHelper;
        this.toolbarManager = toolbarManager;
        this.overrideButtonReflected = Game1.input.GetType().GetMethod("OverrideButton")
            ?? throw new MethodAccessException("Unable to access OverrideButton");

        // Events
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
        eventManager.Subscribe<IIconPressedEventArgs>(this.OnIconPressed);
    }

    /// <summary>Adds an icon from the data model.</summary>
    /// <param name="id">The icon id.</param>
    /// <param name="data">The icon action.</param>
    public void AddIcon(string id, IntegrationData data)
    {
        Action? action;
        switch (data.Type)
        {
            case IntegrationType.Menu when this.TryGetMenuAction(data.ModId, data.ExtraData, out var integrationAction):
                action = integrationAction;
                break;
            case IntegrationType.Method when this.TryGetMethod(data.ModId, data.ExtraData, out var integrationAction):
                action = integrationAction;
                break;
            case IntegrationType.Keybind when this.TryGetKeybindAction(
                data.ModId,
                data.ExtraData,
                out var integrationAction):
                action = integrationAction;
                break;
            default: return;
        }

        if (!this.actions.TryAdd(id, action))
        {
            return;
        }

        Log.Trace(
            "Adding icon: {{ id: {0}, mod: {1}, type: {2}, description: {3} }}.",
            id,
            data.ModId,
            data.Type.ToStringFast(),
            data.HoverText);

        this.toolbarManager.AddIcon(id, data.HoverText);
    }

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        // Load Custom Integrations
        foreach (var customIntegration in this.customIntegrations)
        {
            Action? action;
            switch (customIntegration)
            {
                case IActionIntegration integration when this.TryGetCustomAction(
                    integration.ModId,
                    integration.GetAction,
                    out var integrationAction):
                    action = integrationAction;
                    Log.Trace(
                        "Adding icon: {{ id: {0}, mod: {1}, description: {2} }}.",
                        integration.Icon,
                        integration.ModId,
                        integration.HoverText);

                    break;
                case IMethodIntegration integration when this.TryGetMethodWithParams(
                    integration.ModId,
                    integration.MethodName,
                    integration.Arguments,
                    out var integrationAction):
                    action = integrationAction;
                    Log.Trace(
                        "Adding icon {{ id: {0}, mod: {1}, description: {2}, method: {3} }}.",
                        integration.Icon,
                        integration.ModId,
                        integration.HoverText,
                        integration.MethodName);

                    break;
                case IVanillaIntegration integration:
                    action = integration.DoAction;
                    Log.Trace(
                        "Adding icon {{ id: {0}, mod: vanilla, description: {1} }}.",
                        integration.Icon,
                        integration.HoverText);

                    break;
                default: continue;
            }

            if (!this.actions.TryAdd(customIntegration.Icon, action))
            {
                continue;
            }

            this.toolbarManager.AddIcon(customIntegration.Icon, customIntegration.HoverText);
        }
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (this.actions.TryGetValue(e.Id, out var action))
        {
            action.Invoke();
        }
    }

    private void OverrideButton(SButton button, bool inputState) =>
        this.overrideButtonReflected.Invoke(Game1.input, [button, inputState]);

    /// <summary>Attempt to retrieve a custom action.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="getAction">Function which returns the action to perform.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    private bool TryGetCustomAction(string modId, Func<IMod, Action?> getAction, [NotNullWhen(true)] out Action? action)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            action = null;
            return false;
        }

        action = getAction(mod);
        return action is not null;
    }

    /// <summary>Attempt to retrieve a keybind action.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="keybinds">The method to run.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    private bool TryGetKeybindAction(string modId, string keybinds, [NotNullWhen(true)] out Action? action)
    {
        action = null;
        if (!this.modRegistry.IsLoaded(modId))
        {
            return false;
        }

        var keys = keybinds.Trim().Split(' ');
        IList<SButton> buttons = new List<SButton>();
        foreach (var key in keys)
        {
            if (Enum.TryParse(key, out SButton button))
            {
                buttons.Add(button);
            }
        }

        action = () =>
        {
            foreach (var button in buttons)
            {
                this.OverrideButton(button, true);
            }
        };

        return true;
    }

    /// <summary>Attempt to retrieve a menu action.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="fullName">The full name to the menu class.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    private bool TryGetMenuAction(string modId, string fullName, [NotNullWhen(true)] out Action? action)
    {
        action = null;
        if (!this.TryGetMod(modId, out var mod))
        {
            return false;
        }

        var type = Type.GetType(fullName);
        var constructor = type?.GetConstructor(Array.Empty<Type>());
        if (constructor is null)
        {
            return false;
        }

        action = () =>
        {
            var menu = constructor.Invoke(Array.Empty<object>());
            Game1.activeClickableMenu = (IClickableMenu)menu;
        };

        return true;
    }

    /// <summary>Attempt to retrieve a method.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    private bool TryGetMethod(string modId, string method, [NotNullWhen(true)] out Action? action)
    {
        action = null;
        if (!this.TryGetMod(modId, out var mod))
        {
            return false;
        }

        var reflectedMethod = this.reflectionHelper.GetMethod(mod, method, false);
        if (reflectedMethod is null)
        {
            return false;
        }

        action = () => reflectedMethod.Invoke();
        return true;
    }

    /// <summary>Attempt to retrieve a method with parameters.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="arguments">The arguments to pass to the method.</param>
    /// <param name="action">When this method returns, contains the action; otherwise, null.</param>
    /// <returns><c>true</c> if the integration was added; otherwise, <c>false</c>.</returns>
    private bool TryGetMethodWithParams(
        string modId,
        string method,
        object?[] arguments,
        [NotNullWhen(true)] out Action? action)
    {
        action = null;
        if (!this.TryGetMod(modId, out var mod))
        {
            return false;
        }

        var reflectedMethod = this.reflectionHelper.GetMethod(mod, method, false);
        if (reflectedMethod is null)
        {
            return false;
        }

        action = () => reflectedMethod.Invoke(arguments);
        return true;
    }

    /// <summary>Tries to get the instance of a mod based on the mod id.</summary>
    /// <param name="modId">The unique id of the mod.</param>
    /// <param name="mod">The mod instance.</param>
    /// <returns><c>true</c> if the mod instance could be obtained; otherwise, <c>false</c>.</returns>
    private bool TryGetMod(string modId, [NotNullWhen(true)] out IMod? mod)
    {
        if (!this.modRegistry.IsLoaded(modId))
        {
            mod = null;
            return false;
        }

        var modInfo = this.modRegistry.Get(modId);
        mod = (IMod?)modInfo?.GetType().GetProperty("Mod")?.GetValue(modInfo);
        return mod is not null;
    }
}