namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class Logger : ILog
{
    private readonly Lazy<ILog> log;

    /// <summary>Initializes a new instance of the <see cref="Logger"/> class.</summary>
    /// <param name="fauxCoreIntegration">Dependency used for FauxCore integration.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    public Logger(FauxCoreIntegration fauxCoreIntegration, IMonitor monitor) =>
        this.log = new Lazy<ILog>(() => fauxCoreIntegration.Api!.CreateLogService(monitor));

    /// <inheritdoc />
    public void Trace(string message, object?[]? args = null) => this.log.Value.Trace(message, args);

    /// <inheritdoc />
    public void TraceOnce(string message, params object?[]? args) => this.log.Value.TraceOnce(message, args);

    /// <inheritdoc />
    public void Debug(string message, object?[]? args = null) => this.log.Value.Debug(message, args);

    /// <inheritdoc />
    public void Info(string message, object?[]? args = null) => this.log.Value.Info(message, args);

    /// <inheritdoc />
    public void Warn(string message, object?[]? args = null) => this.log.Value.Warn(message, args);

    /// <inheritdoc />
    public void WarnOnce(string message, object?[]? args = null) => this.log.Value.WarnOnce(message, args);

    /// <inheritdoc />
    public void Error(string message, object?[]? args = null) => this.log.Value.Error(message, args);

    /// <inheritdoc />
    public void Alert(string message, object?[]? args = null) => this.log.Value.Alert(message, args);
}