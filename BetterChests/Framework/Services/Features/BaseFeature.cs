namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IFeature" />
internal abstract class BaseFeature<TFeature> : Mod.BaseService<TFeature>, IFeature
    where TFeature : class, IFeature
{
    private bool isActivated;

    /// <summary>Initializes a new instance of the <see cref="BaseFeature{TFeature}" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    protected BaseFeature(IEventManager eventManager, IModConfig modConfig)
    {
        this.Config = modConfig;
        this.Events = eventManager;
        this.Events.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
    }

    /// <inheritdoc />
    public abstract bool ShouldBeActive { get; }

    /// <summary>Gets the Dependency used for managing config data.</summary>
    protected IModConfig Config { get; }

    /// <summary>Gets the dependency used for managing events.</summary>
    protected IEventManager Events { get; }

    /// <summary>Activate this feature.</summary>
    protected abstract void Activate();

    /// <summary>Deactivate this feature.</summary>
    protected abstract void Deactivate();

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e)
    {
        if (this.isActivated == this.ShouldBeActive)
        {
            return;
        }

        this.isActivated = this.ShouldBeActive;
        if (this.isActivated)
        {
            Mod.Log.Trace("Activating feature {0}", this.Id);
            this.Activate();
            return;
        }

        Mod.Log.Trace("Deactivating feature {0}", this.Id);
        this.Deactivate();
    }
}