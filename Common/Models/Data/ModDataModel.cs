namespace StardewMods.Common.Models.Data;

using StardewMods.Common.Interfaces.Data;
using StardewValley.Mods;

/// <inheritdoc />
internal sealed class ModDataModel : IDictionaryModel
{
    private readonly ModDataDictionary modData;

    /// <summary>Initializes a new instance of the <see cref="ModDataModel" /> class.</summary>
    /// <param name="modData">The mod data dictionary.</param>
    public ModDataModel(ModDataDictionary modData) => this.modData = modData;

    /// <inheritdoc />
    public bool ContainsKey(string key) => this.modData.ContainsKey(key);

    /// <inheritdoc />
    public void SetValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            this.modData.Remove(key);
            return;
        }

        this.modData[key] = value;
    }

    /// <inheritdoc />
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
        this.modData.TryGetValue(key, out value);
}