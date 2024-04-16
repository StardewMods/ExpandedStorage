namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

/// <inheritdoc />
internal sealed class BuildingStorageOptions : ChildStorageOptions
{
    private readonly Func<BuildingData> getData;

    /// <summary>Initializes a new instance of the <see cref="BuildingStorageOptions" /> class.</summary>
    /// <param name="getDefault">Get the default storage options.</param>
    /// <param name="getData">Get the building data.</param>
    public BuildingStorageOptions(Func<IStorageOptions> getDefault, Func<BuildingData> getData)
        : base(getDefault, new CustomFieldsStorageOptions(BuildingStorageOptions.GetCustomFields(getData))) =>
        this.getData = getData;

    /// <summary>Gets the building data.</summary>
    public BuildingData Data => this.getData();

    /// <inheritdoc />
    public override string GetDescription() => TokenParser.ParseText(this.Data.Description);

    /// <inheritdoc />
    public override string GetDisplayName() => TokenParser.ParseText(this.Data.Name);

    private static Func<bool, Dictionary<string, string>> GetCustomFields(Func<BuildingData> getData) =>
        init =>
        {
            if (init)
            {
                getData().CustomFields ??= [];
            }

            return getData().CustomFields ?? [];
        };
}