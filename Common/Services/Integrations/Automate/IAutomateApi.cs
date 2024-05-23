#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.Automate;
#else
namespace StardewMods.Common.Services.Integrations.Automate;
#endif

using Microsoft.Xna.Framework;

// ReSharper disable All
#pragma warning disable

/// <summary>The API which lets other mods interact with Automate.</summary>
public interface IAutomateApi
{
    /// <summary>Add an automation factory.</summary>
    /// <param name="factory">An automation factory which construct machines, containers, and connectors.</param>
    void AddFactory(IAutomationFactory factory);

    /// <summary>Get the status of machines in a tile area. This is a specialized API for Data Layers and similar mods.</summary>
    /// <param name="location">The location for which to display data.</param>
    /// <param name="tileArea">The tile area for which to display data.</param>
    IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea);
}