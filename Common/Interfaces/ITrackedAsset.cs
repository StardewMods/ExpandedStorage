namespace StardewMods.Common.Interfaces;

using StardewModdingAPI.Events;

/// <summary>Represents a tracked asset.</summary>
internal interface ITrackedAsset
{
    /// <summary>Provide the asset.</summary>
    /// <param name="e">The asset requested event args.</param>
    void ProvideAsset(AssetRequestedEventArgs e);
}