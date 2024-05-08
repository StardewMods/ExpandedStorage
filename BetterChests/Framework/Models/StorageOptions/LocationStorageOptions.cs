namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewValley.GameData.Locations;

/// <inheritdoc />
internal sealed class LocationStorageOptions : CustomFieldsStorageOptions
{
    private readonly string locationName;

    /// <summary>Initializes a new instance of the <see cref="LocationStorageOptions" /> class.</summary>
    /// <param name="locationName">The location name.</param>
    public LocationStorageOptions(string locationName)
        : base(LocationStorageOptions.GetCustomFields(locationName)) =>
        this.locationName = locationName;

    /// <inheritdoc />
    public override string Description => I18n.Storage_Fridge_Tooltip();

    /// <inheritdoc />
    public override string DisplayName => I18n.Storage_Fridge_Name();

    /// <summary>Gets the location data.</summary>
    public LocationData Data =>
        DataLoader.Locations(Game1.content).GetValueOrDefault(this.locationName) ?? new LocationData();

    private static Func<Dictionary<string, string>?> GetCustomFields(string locationName) =>
        () => DataLoader.Locations(Game1.content).GetValueOrDefault(locationName)?.CustomFields;
}