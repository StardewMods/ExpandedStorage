namespace StardewMods.Common.Models.Data;

using StardewMods.Common.Interfaces.Data;

internal sealed class DictionaryModel : IDictionaryModel
{
    private readonly Func<Dictionary<string, string>?> getData;

    /// <summary>Initializes a new instance of the <see cref="DictionaryModel" /> class.</summary>
    /// <param name="getData">Get the custom field data.</param>
    public DictionaryModel(Func<Dictionary<string, string>?> getData) => this.getData = getData;

    private Dictionary<string, string>? Data => this.getData();

    /// <inheritdoc />
    public bool ContainsKey(string key) => this.Data?.ContainsKey(key) == true;

    /// <inheritdoc />
    public void SetValue(string key, string value)
    {
        if (this.Data is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            this.Data.Remove(key);
            return;
        }

        this.Data[key] = value;
    }

    /// <inheritdoc />
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        if (this.Data is not null)
        {
            return this.Data.TryGetValue(key, out value);
        }

        value = null;
        return false;
    }
}