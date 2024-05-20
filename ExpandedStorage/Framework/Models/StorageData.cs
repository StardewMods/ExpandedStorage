namespace StardewMods.ExpandedStorage.Framework.Models;

using StardewMods.Common.Services.Integrations.ExpandedStorage;

/// <inheritdoc />
internal sealed class StorageData : IStorageData
{
    private readonly DictionaryStorageData chestData;
    private readonly DictionaryStorageData typeData;

    /// <summary>Initializes a new instance of the <see cref="StorageData" /> class.</summary>
    /// <param name="typeData">The default data for all chests of the given type.</param>
    /// <param name="chestData">The individual data for the chest.</param>
    public StorageData(DictionaryStorageData typeData, DictionaryStorageData chestData)
    {
        this.typeData = typeData;
        this.chestData = chestData;
    }

    /// <inheritdoc />
    public string CloseNearbySound
    {
        get => this.Get(nameof(this.CloseNearbySound), data => data.CloseNearbySound, "doorCreakReverse");
        set => this.chestData.CloseNearbySound = value;
    }

    /// <inheritdoc />
    public int Frames
    {
        get => this.Get(nameof(this.Frames), data => data.Frames, 1);
        set => this.chestData.Frames = value;
    }

    /// <inheritdoc />
    public string? GlobalInventoryId
    {
        get => this.Get(nameof(this.GlobalInventoryId), data => data.GlobalInventoryId, null);
        set => this.chestData.GlobalInventoryId = value;
    }

    /// <inheritdoc />
    public bool IsFridge
    {
        get => this.Get(nameof(this.IsFridge), data => data.IsFridge, false);
        set => this.chestData.IsFridge = value;
    }

    /// <inheritdoc />
    public bool OpenNearby
    {
        get => this.Get(nameof(this.OpenNearby), data => data.OpenNearby, false);
        set => this.chestData.OpenNearby = value;
    }

    /// <inheritdoc />
    public string OpenNearbySound
    {
        get => this.Get(nameof(this.OpenNearbySound), data => data.OpenNearbySound, "doorCreak");
        set => this.chestData.OpenNearbySound = value;
    }

    /// <inheritdoc />
    public string OpenSound
    {
        get => this.Get(nameof(this.OpenSound), data => data.OpenSound, "openChest");
        set => this.chestData.OpenSound = value;
    }

    /// <inheritdoc />
    public string PlaceSound
    {
        get => this.Get(nameof(this.PlaceSound), data => data.PlaceSound, "axe");
        set => this.chestData.PlaceSound = value;
    }

    /// <inheritdoc />
    public bool PlayerColor
    {
        get => this.Get(nameof(this.PlayerColor), data => data.PlayerColor, false);
        set => this.chestData.PlayerColor = value;
    }

    private TValue Get<TValue>(string id, Func<IStorageData, TValue> getMethod, TValue defaultValue) =>
        this.chestData.HasValue(id)
            ? getMethod(this.chestData)
            : this.typeData.HasValue(id)
                ? getMethod(this.typeData)
                : defaultValue;
}