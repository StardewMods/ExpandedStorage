namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

/// <inheritdoc />
internal class BuildingStorageOptions : CustomFieldsStorageOptions
{
    private readonly string buildingType;

    /// <summary>Initializes a new instance of the <see cref="BuildingStorageOptions" /> class.</summary>
    /// <param name="buildingType">The building type.</param>
    public BuildingStorageOptions(string buildingType)
        : base(BuildingStorageOptions.GetCustomFields(buildingType)) =>
        this.buildingType = buildingType;

    /// <summary>Gets the building data.</summary>
    public BuildingData Data =>
        Game1.buildingData.TryGetValue(this.buildingType, out var buildingData) ? buildingData : new BuildingData();

    /// <inheritdoc />
    public override string Description =>
        this.buildingType switch
        {
            "Stable" => I18n.Storage_Saddlebag_Tooltip(), _ => TokenParser.ParseText(this.Data.Description),
        };

    /// <inheritdoc />
    public override string DisplayName =>
        this.buildingType switch
        {
            "Stable" => I18n.Storage_Saddlebag_Name(), _ => TokenParser.ParseText(this.Data.Name),
        };

    private static Func<Dictionary<string, string>?> GetCustomFields(string buildingType) =>
        () => Game1.buildingData.TryGetValue(buildingType, out var buildingData) ? buildingData.CustomFields : null;
}