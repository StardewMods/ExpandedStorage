#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.ContentPatcher;

using StardewModdingAPI.Events;
using StardewMods.FauxCore.Common.Interfaces;
#else
namespace StardewMods.Common.Services.Integrations.ContentPatcher;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
#endif

/// <inheritdoc />
internal sealed class ContentPatcherIntegration : ModIntegration<IContentPatcherApi>
{
    private readonly IEventManager eventManager;

    private int countDown = 10;

    /// <summary>Initializes a new instance of the <see cref="ContentPatcherIntegration" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ContentPatcherIntegration(IEventManager eventManager, IModRegistry modRegistry)
        : base(modRegistry)
    {
        this.eventManager = eventManager;
        if (this.IsLoaded)
        {
            this.eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
        }
    }

    /// <inheritdoc />
    public override string UniqueId => "Pathoschild.ContentPatcher";

    /// <inheritdoc />
    public override ISemanticVersion Version { get; } = new SemanticVersion(2, 0, 0);

    private void OnGameLaunched(GameLaunchedEventArgs e) =>
        this.eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);

    private void OnUpdateTicked(UpdateTickedEventArgs e)
    {
        if (--this.countDown == 0)
        {
            this.eventManager.Unsubscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        }

        if (!this.IsLoaded || !this.Api.IsConditionsApiReady)
        {
            return;
        }

        this.eventManager.Unsubscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        this.eventManager.Publish(new ConditionsApiReadyEventArgs());
    }
}