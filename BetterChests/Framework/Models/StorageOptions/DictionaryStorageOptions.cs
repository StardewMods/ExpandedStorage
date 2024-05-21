namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Interfaces.Data;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.BetterChests.IStorageOptions" />
internal sealed class DictionaryStorageOptions : StorageOptions
{
    private readonly Func<string> getDescription;
    private readonly Func<string> getDisplayName;

    /// <summary>Initializes a new instance of the <see cref="DictionaryStorageOptions" /> class.</summary>
    /// <param name="dictionaryModel">The backing dictionary.</param>
    /// <param name="getDescription">Get method for the description.</param>
    /// <param name="getDisplayName">Get method for the display name.</param>
    public DictionaryStorageOptions(
        IDictionaryModel dictionaryModel,
        Func<string>? getDescription = null,
        Func<string>? getDisplayName = null)
        : base(dictionaryModel)
    {
        this.getDescription = getDescription ?? I18n.Storage_Other_Tooltip;
        this.getDisplayName = getDisplayName ?? I18n.Storage_Other_Name;
    }

    /// <inheritdoc />
    public override string Description => this.getDescription();

    /// <inheritdoc />
    public override string DisplayName => this.getDisplayName();
}