namespace StardewMods.Common.Services.Integrations.FauxCore;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;

/// <inheritdoc />
internal sealed class Patcher : IPatchManager
{
    private readonly Lazy<IPatchManager> patchManager;
    private readonly Queue<Action> queue = [];

    private bool initialized;

    /// <summary>Initializes a new instance of the <see cref="Patcher"/> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="fauxCoreIntegration">Dependency used for FauxCore integration.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    public Patcher(IEventSubscriber eventSubscriber, FauxCoreIntegration fauxCoreIntegration, ILog log)
    {
        this.patchManager = new Lazy<IPatchManager>(() => fauxCoreIntegration.Api!.CreatePatchService(log));
        eventSubscriber.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <inheritdoc />
    public void Add(string id, params ISavedPatch[] patches)
    {
        if (this.initialized)
        {
            this.patchManager.Value.Add(id, patches);
            return;
        }

        this.queue.Enqueue(() => this.patchManager.Value.Add(id, patches));
    }

    /// <inheritdoc />
    public void Patch(string id)
    {
        if (this.initialized)
        {
            this.patchManager.Value.Patch(id);
            return;
        }

        this.queue.Enqueue(() => this.patchManager.Value.Patch(id));
    }

    /// <inheritdoc />
    public void Unpatch(string id)
    {
        if (this.initialized)
        {
            this.patchManager.Value.Unpatch(id);
            return;
        }

        this.queue.Enqueue(() => this.patchManager.Value.Unpatch(id));
    }

    private void OnGameLaunched(GameLaunchedEventArgs obj)
    {
        this.initialized = true;
        while (this.queue.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }
}