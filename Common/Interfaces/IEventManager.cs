#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Interfaces;
#else
namespace StardewMods.Common.Interfaces;
#endif

/// <summary>Represents a service for publishing and subscribing to events.</summary>
public interface IEventManager
{
    /// <summary>Publishes an event with the given event arguments.</summary>
    /// <typeparam name="TEventArgs">The event argument implementation.</typeparam>
    /// <param name="eventArgs">The event arguments to publish.</param>
    /// <remarks>
    /// This method is used to raise an event with the provided event arguments. It can be used to notify subscribers
    /// of an event.
    /// </remarks>
    public void Publish<TEventArgs>(TEventArgs eventArgs)
        where TEventArgs : EventArgs;

    /// <summary>Publishes an event with the given event arguments.</summary>
    /// <typeparam name="TEventType">The type of the event arguments.</typeparam>
    /// <typeparam name="TEventArgs">The event argument implementation.</typeparam>
    /// <param name="eventArgs">The event arguments to publish.</param>
    /// <remarks>
    /// This method is used to raise an event with the provided event arguments. It can be used to notify subscribers
    /// of an event.
    /// </remarks>
    public void Publish<TEventType, TEventArgs>(TEventArgs eventArgs)
        where TEventArgs : EventArgs, TEventType;

    /// <summary>Subscribes to an event handler.</summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="handler">The event handler to subscribe.</param>
    void Subscribe<TEventArgs>(Action<TEventArgs> handler);

    /// <summary>Unsubscribes an event handler from an event.</summary>
    /// <param name="handler">The event handler to unsubscribe.</param>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    void Unsubscribe<TEventArgs>(Action<TEventArgs> handler);
}