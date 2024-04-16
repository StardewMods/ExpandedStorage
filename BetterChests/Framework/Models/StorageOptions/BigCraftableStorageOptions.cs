namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewValley.GameData.BigCraftables;
using StardewValley.TokenizableStrings;

/// <inheritdoc />
internal sealed class BigCraftableStorageOptions : ChildStorageOptions
{
    private readonly Func<BigCraftableData> getData;

    /// <summary>Initializes a new instance of the <see cref="BigCraftableStorageOptions" /> class.</summary>
    /// <param name="getDefault">Get the default storage options.</param>
    /// <param name="getData">Get the big craftable data.</param>
    public BigCraftableStorageOptions(Func<IStorageOptions> getDefault, Func<BigCraftableData> getData)
        : base(getDefault, new CustomFieldsStorageOptions(BigCraftableStorageOptions.GetCustomFields(getData))) =>
        this.getData = getData;

    /// <summary>Gets the big craftable data.</summary>
    public BigCraftableData Data => this.getData();

    /// <inheritdoc />
    public override string GetDescription() => TokenParser.ParseText(this.Data.Description);

    /// <inheritdoc />
    public override string GetDisplayName() => TokenParser.ParseText(this.Data.DisplayName);

    private static Func<bool, Dictionary<string, string>> GetCustomFields(Func<BigCraftableData> getData) =>
        init =>
        {
            if (init)
            {
                getData().CustomFields ??= [];
            }

            return getData().CustomFields ?? [];
        };
}