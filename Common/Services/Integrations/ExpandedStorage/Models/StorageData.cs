#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.ExpandedStorage;

using StardewMods.FauxCore.Common.Interfaces.Data;
using StardewMods.FauxCore.Common.Models.Data;
#else
namespace StardewMods.Common.Services.Integrations.ExpandedStorage;

using StardewMods.Common.Interfaces.Data;
using StardewMods.Common.Models.Data;
#endif

using Microsoft.Xna.Framework;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.ExpandedStorage.IStorageData" />
internal class StorageData : DictionaryDataModel, IStorageData
{
    /// <summary>Initializes a new instance of the <see cref="StorageData" /> class.</summary>
    /// <param name="dictionaryModel">The backing dictionary.</param>
    public StorageData(IDictionaryModel dictionaryModel)
        : base(dictionaryModel) { }

    /// <inheritdoc />
    public string CloseNearbySound
    {
        get => this.Get(nameof(this.CloseNearbySound));
        set => this.Set(nameof(this.CloseNearbySound), value);
    }

    /// <inheritdoc />
    public int Frames
    {
        get => this.Get(nameof(this.Frames), DictionaryDataModel.StringToInt);
        set => this.Set(nameof(this.Frames), value, DictionaryDataModel.IntToString);
    }

    /// <inheritdoc />
    public string? GlobalInventoryId
    {
        get => this.Get(nameof(this.GlobalInventoryId));
        set => this.Set(nameof(this.GlobalInventoryId), value ?? string.Empty);
    }

    /// <inheritdoc />
    public bool IsFridge
    {
        get => this.Get(nameof(this.IsFridge), DictionaryDataModel.StringToBool);
        set => this.Set(nameof(this.IsFridge), value, DictionaryDataModel.BoolToString);
    }

    /// <inheritdoc />
    public Dictionary<string, string>? ModData
    {
        get => this.Get(nameof(this.ModData), DictionaryDataModel.StringToDict);
        set => this.Set(nameof(this.ModData), value, DictionaryDataModel.DictToString);
    }

    /// <inheritdoc />
    public bool OpenNearby
    {
        get => this.Get(nameof(this.OpenNearby), DictionaryDataModel.StringToBool);
        set => this.Set(nameof(this.OpenNearby), value, DictionaryDataModel.BoolToString);
    }

    /// <inheritdoc />
    public string OpenNearbySound
    {
        get => this.Get(nameof(this.OpenNearbySound));
        set => this.Set(nameof(this.OpenNearbySound), value);
    }

    /// <inheritdoc />
    public string OpenSound
    {
        get => this.Get(nameof(this.OpenSound));
        set => this.Set(nameof(this.OpenSound), value);
    }

    /// <inheritdoc />
    public string PlaceSound
    {
        get => this.Get(nameof(this.PlaceSound));
        set => this.Set(nameof(this.PlaceSound), value);
    }

    /// <inheritdoc />
    public bool PlayerColor
    {
        get => this.Get(nameof(this.PlayerColor), DictionaryDataModel.StringToBool);
        set => this.Set(nameof(this.PlayerColor), value, DictionaryDataModel.BoolToString);
    }

    /// <inheritdoc />
    public string TextureOverride
    {
        get => this.Get(nameof(this.TextureOverride));
        set => this.Set(nameof(this.TextureOverride), value);
    }

    /// <inheritdoc />
    public Color TintOverride
    {
        get => this.Get(nameof(this.TintOverride), DictionaryDataModel.StringToColor);
        set => this.Set(nameof(this.TintOverride), value, DictionaryDataModel.ColorToString);
    }

    /// <inheritdoc />
    protected override string Prefix => "furyx639.ExpandedStorage/";
}