namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewValley.GameData.Locations;

/// <inheritdoc />
internal sealed class LocationStorageOptions : ChildStorageOptions
{
    private readonly Func<LocationData> getData;

    /// <summary>Initializes a new instance of the <see cref="LocationStorageOptions" /> class.</summary>
    /// <param name="getDefault">Get the default storage options.</param>
    /// <param name="getData">Get the location data.</param>
    public LocationStorageOptions(Func<IStorageOptions> getDefault, Func<LocationData> getData)
        : base(getDefault, new CustomFieldsStorageOptions(LocationStorageOptions.GetCustomFields(getData))) =>
        this.getData = getData;

    /// <summary>Gets the location data.</summary>
    public LocationData? Data => this.getData();

    /// <inheritdoc />
    public override string GetDescription() => I18n.Storage_Fridge_Tooltip();

    /// <inheritdoc />
    public override string GetDisplayName() => I18n.Storage_Fridge_Name();

    private static Func<bool, Dictionary<string, string>> GetCustomFields(Func<LocationData> getData) =>
        init =>
        {
            if (init)
            {
                getData().CustomFields ??= [];
            }

            return getData().CustomFields ?? [];
        };
}