namespace StardewMods.Common.Services;

using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Static wrapper for logging service.</summary>
internal sealed class Log
{
    private static Log instance = null!;

    private readonly ISimpleLogging simpleLogging;

    /// <summary>Initializes a new instance of the <see cref="Log" /> class.</summary>
    /// <param name="simpleLogging">Dependency used for logging information to the console.</param>
    public Log(ISimpleLogging simpleLogging)
    {
        Log.instance = this;
        this.simpleLogging = simpleLogging;
    }

    /// <summary>Logs an alert message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Alert(string message, params object?[]? args) => Log.instance.simpleLogging.Alert(message, args);

    /// <summary>Logs a debug message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Debug(string message, params object?[]? args) => Log.instance.simpleLogging.Debug(message, args);

    /// <summary>Logs an error message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    [SuppressMessage("Naming", "CA1716", Justification = "Reviewed")]
    public static void Error(string message, params object?[]? args) => Log.instance.simpleLogging.Error(message, args);

    /// <summary>Logs an info message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Info(string message, params object?[]? args) => Log.instance.simpleLogging.Info(message, args);

    /// <summary>Logs a trace message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Trace(string message, params object?[]? args) => Log.instance.simpleLogging.Trace(message, args);

    /// <summary>Logs a trace message to the console only once.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void TraceOnce(string message, params object?[]? args) =>
        Log.instance.simpleLogging.TraceOnce(message, args);

    /// <summary>Logs a warn message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Warn(string message, params object?[]? args) => Log.instance.simpleLogging.Warn(message, args);

    /// <summary>Logs a warn message to the console only once.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void WarnOnce(string message, params object?[]? args) =>
        Log.instance.simpleLogging.WarnOnce(message, args);
}