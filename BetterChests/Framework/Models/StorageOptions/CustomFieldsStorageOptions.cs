namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

/// <inheritdoc />
internal class CustomFieldsStorageOptions : DictionaryStorageOptions
{
    private readonly Func<Dictionary<string, string>?> getData;

    /// <summary>Initializes a new instance of the <see cref="CustomFieldsStorageOptions" /> class.</summary>
    /// <param name="getData">Get the custom field data.</param>
    public CustomFieldsStorageOptions(Func<Dictionary<string, string>?> getData) => this.getData = getData;

    private Dictionary<string, string> Data => this.getData() ?? [];

    /// <inheritdoc />
    protected override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
        this.Data.TryGetValue(key, out value);

    /// <inheritdoc />
    protected override void SetValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            this.Data.Remove(key);
            return;
        }

        this.Data[key] = value;
    }
}