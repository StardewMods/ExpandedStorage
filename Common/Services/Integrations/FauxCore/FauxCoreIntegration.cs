namespace StardewMods.Common.Services.Integrations.FauxCore;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.ModIntegration{T}" />
[SuppressMessage("StyleCop", "SA1124", Justification = "Reviewed")]
internal sealed class FauxCoreIntegration
    : ModIntegration<IFauxCoreApi>, IExpressionHandler, IIconRegistry, ILog, IPatchManager, IThemeHelper
{
    private const string ModUniqueId = "furyx639.FauxCore";
    private readonly Queue<Action> deferred = [];
    private readonly Lazy<IExpressionHandler>? expressionHandler;
    private readonly Lazy<IIconRegistry>? iconRegistry;
    private readonly Lazy<ILog>? log;
    private readonly IMonitor monitor;
    private readonly Lazy<IPatchManager>? patchManager;
    private readonly Lazy<IThemeHelper>? themeHelper;

    private bool initialized;

    /// <summary>Initializes a new instance of the <see cref="FauxCoreIntegration" /> class.</summary>
    /// <param name="modEvents">Dependency used for managing access to SMAPI events.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    public FauxCoreIntegration(IModEvents modEvents, IModRegistry modRegistry, IMonitor monitor)
        : base(modRegistry, FauxCoreIntegration.ModUniqueId)
    {
        this.monitor = monitor;
        if (!this.IsLoaded)
        {
            return;
        }

        this.expressionHandler = new Lazy<IExpressionHandler>(() => this.Api.ExpressionHandler);
        this.iconRegistry = new Lazy<IIconRegistry>(() => this.Api.IconRegistry);
        this.log = new Lazy<ILog>(() => this.Api.Log);
        this.patchManager = new Lazy<IPatchManager>(() => this.Api.PatchManager);
        this.themeHelper = new Lazy<IThemeHelper>(() => this.Api.ThemeHelper);

        modEvents.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public void Add(string id, params ISavedPatch[] patches)
    {
        if (this.initialized)
        {
            this.patchManager?.Value.Add(id, patches);
            return;
        }

        this.deferred.Enqueue(() => this.patchManager?.Value.Add(id, patches));
    }

    /// <inheritdoc />
    public void AddAsset(string path, IRawTextureData data)
    {
        if (this.initialized)
        {
            this.themeHelper?.Value.AddAsset(path, data);
            return;
        }

        this.deferred.Enqueue(() => this.themeHelper?.Value.AddAsset(path, data));
    }

    /// <inheritdoc />
    public void AddIcon(string id, string path, Rectangle area)
    {
        if (this.initialized)
        {
            this.iconRegistry?.Value.AddIcon(id, path, area);
            return;
        }

        this.deferred.Enqueue(() => this.iconRegistry?.Value.AddIcon(id, path, area));
    }

    /// <inheritdoc />
    public void Alert(string message, object?[]? args = null) => this.log?.Value.Alert(message, args);

    /// <inheritdoc />
    public void Debug(string message, object?[]? args = null) => this.log?.Value.Debug(message, args);

    /// <inheritdoc />
    public void Error(string message, object?[]? args = null) => this.log?.Value.Error(message, args);

    /// <inheritdoc />
    public IEnumerable<IIcon> GetIcons() => this.iconRegistry?.Value.GetIcons() ?? Enumerable.Empty<IIcon>();

    /// <inheritdoc />
    public void Info(string message, object?[]? args = null) => this.log?.Value.Info(message, args);

    /// <inheritdoc />
    public void Patch(string id)
    {
        if (this.initialized)
        {
            this.patchManager?.Value.Patch(id);
            return;
        }

        this.deferred.Enqueue(() => this.patchManager?.Value.Patch(id));
    }

    /// <inheritdoc />
    public IManagedTexture RequireAsset(string path) =>
        this.themeHelper?.Value.RequireAsset(path)
        ?? throw new InvalidOperationException($"Failed to load asset: {path}.");

    /// <inheritdoc />
    public IIcon RequireIcon(string id) =>
        this.iconRegistry?.Value.RequireIcon(id) ?? throw new InvalidOperationException($"Failed to load icon: {id}.");

    /// <inheritdoc/>
    public IIcon RequireIcon(VanillaIcon icon) =>
        this.iconRegistry?.Value.RequireIcon(icon)
        ?? throw new InvalidOperationException($"Failed to load icon: {icon.ToStringFast()}.");

    /// <inheritdoc />
    public void Trace(string message, object?[]? args = null) => this.log?.Value.Trace(message, args);

    /// <inheritdoc />
    public void TraceOnce(string message, params object?[]? args) => this.log?.Value.TraceOnce(message, args);

    /// <inheritdoc />
    public bool TryCreateExpression(
        ExpressionType expressionType,
        [NotNullWhen(true)] out IExpression? expression,
        string? term = null,
        params IExpression[]? expressions)
    {
        expression = null;
        return this.expressionHandler?.Value.TryCreateExpression(expressionType, out expression, term, expressions)
            ?? false;
    }

    /// <inheritdoc />
    public bool TryGetAsset(string path, [NotNullWhen(true)] out IManagedTexture? texture)
    {
        if (this.themeHelper?.Value.TryGetAsset(path, out texture) == true)
        {
            return true;
        }

        texture = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetIcon(string id, [NotNullWhen(true)] out IIcon? icon)
    {
        icon = null;
        return this.iconRegistry?.Value.TryGetIcon(id, out icon) ?? false;
    }

    /// <inheritdoc />
    public bool TryGetRawTextureData(string path, [NotNullWhen(true)] out IRawTextureData? rawTextureData)
    {
        if (this.themeHelper?.Value.TryGetRawTextureData(path, out rawTextureData) == true)
        {
            return true;
        }

        rawTextureData = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryParseExpression(string expression, [NotNullWhen(true)] out IExpression? parsedExpression)
    {
        parsedExpression = null;
        return this.expressionHandler?.Value.TryParseExpression(expression, out parsedExpression) ?? false;
    }

    /// <inheritdoc />
    public void Unpatch(string id)
    {
        if (this.initialized)
        {
            this.patchManager?.Value.Unpatch(id);
            return;
        }

        this.deferred.Enqueue(() => this.patchManager?.Value.Unpatch(id));
    }

    /// <inheritdoc />
    public void Warn(string message, object?[]? args = null) => this.log?.Value.Warn(message, args);

    /// <inheritdoc />
    public void WarnOnce(string message, object?[]? args = null) => this.log?.Value.WarnOnce(message, args);

    [EventPriority((EventPriority)int.MaxValue)]
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs? e)
    {
        if (!this.IsLoaded)
        {
            return;
        }

        this.initialized = true;
        this.Api.Monitor = this.monitor;

        while (this.deferred.TryDequeue(out var delayedPatch))
        {
            delayedPatch.Invoke();
        }
    }
}