namespace StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Represents a lock that is currently being held.</summary>
internal interface IServiceLock
{
    /// <summary>Release the lock.</summary>
    public void Release();
}