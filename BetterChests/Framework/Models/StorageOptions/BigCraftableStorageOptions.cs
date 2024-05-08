namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewValley.GameData.BigCraftables;
using StardewValley.TokenizableStrings;

/// <inheritdoc />
internal sealed class BigCraftableStorageOptions : CustomFieldsStorageOptions
{
    private readonly string itemId;

    /// <summary>Initializes a new instance of the <see cref="BigCraftableStorageOptions" /> class.</summary>
    /// <param name="itemId">he big craftable object id.</param>
    public BigCraftableStorageOptions(string itemId)
        : base(BigCraftableStorageOptions.GetCustomFields(itemId)) =>
        this.itemId = itemId;

    /// <summary>Gets the big craftable data.</summary>
    public BigCraftableData Data =>
        Game1.bigCraftableData.TryGetValue(this.itemId, out var bigCraftableData)
            ? bigCraftableData
            : new BigCraftableData();

    /// <inheritdoc />
    public override string Description => TokenParser.ParseText(this.Data.Description);

    /// <inheritdoc />
    public override string DisplayName => TokenParser.ParseText(this.Data.DisplayName);

    private static Func<Dictionary<string, string>?> GetCustomFields(string itemId) =>
        () => Game1.bigCraftableData.TryGetValue(itemId, out var bigCraftableData)
            ? bigCraftableData.CustomFields
            : null;
}