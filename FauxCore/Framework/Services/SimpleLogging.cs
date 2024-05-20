namespace StardewMods.FauxCore.Framework.Services;

using System.Globalization;
using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;

/// <inheritdoc />
internal sealed class SimpleLogging : ISimpleLogging
{
    private readonly IModConfig modConfig;
    private readonly IMonitor monitor;

    private string lastMessage = string.Empty;

    /// <summary>Initializes a new instance of the <see cref="SimpleLogging" /> class.</summary>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    public SimpleLogging(IModConfig modConfig, IMonitor monitor)
    {
        this.modConfig = modConfig;
        this.monitor = monitor;
    }

    /// <inheritdoc />
    public void Alert(string message, object?[]? args = null) => this.Raise(message, LogLevel.Alert, false, args);

    /// <inheritdoc />
    public void Debug(string message, object?[]? args = null) => this.Raise(message, LogLevel.Debug, false, args);

    /// <inheritdoc />
    public void Error(string message, object?[]? args = null) => this.Raise(message, LogLevel.Error, false, args);

    /// <inheritdoc />
    public void Info(string message, object?[]? args = null) => this.Raise(message, LogLevel.Info, false, args);

    /// <inheritdoc />
    public void Trace(string message, object?[]? args = null) => this.Raise(message, LogLevel.Trace, false, args);

    /// <inheritdoc />
    public void TraceOnce(string message, params object?[]? args) => this.Raise(message, LogLevel.Trace, true, args);

    /// <inheritdoc />
    public void Warn(string message, object?[]? args = null) => this.Raise(message, LogLevel.Warn, false, args);

    /// <inheritdoc />
    public void WarnOnce(string message, object?[]? args = null) => this.Raise(message, LogLevel.Warn, true, args);

    private void Raise(string message, LogLevel level, bool once, object?[]? args = null)
    {
        if (args != null)
        {
            message = string.Format(CultureInfo.InvariantCulture, message, args);
        }

        // Prevent consecutive duplicate messages
        if (message == this.lastMessage)
        {
            return;
        }

        this.lastMessage = message;
        switch (level)
        {
            case LogLevel.Trace when this.modConfig.LogLevel == SimpleLogLevel.More:
            case LogLevel.Debug when this.modConfig.LogLevel == SimpleLogLevel.More:
            case LogLevel.Info when this.modConfig.LogLevel >= SimpleLogLevel.Less:
            case LogLevel.Warn when this.modConfig.LogLevel >= SimpleLogLevel.Less:
            case LogLevel.Error:
            case LogLevel.Alert:
                if (once)
                {
                    this.monitor.LogOnce(message, level);
                    break;
                }

                this.monitor.Log(message, level);
                break;
            default:
                // Suppress log from console
                this.monitor.Log(message);
                return;
        }

        if (level == LogLevel.Alert && this.modConfig.LogLevel == SimpleLogLevel.More)
        {
            Game1.showRedMessage(message);
        }
    }
}