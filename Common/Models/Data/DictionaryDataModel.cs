#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Models.Data;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.Interfaces.Cache;
using StardewMods.FauxCore.Common.Interfaces.Data;
using StardewMods.FauxCore.Common.Models.Cache;

#else

namespace StardewMods.Common.Models.Data;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewMods.Common.Interfaces.Cache;
using StardewMods.Common.Interfaces.Data;
using StardewMods.Common.Models.Cache;

#endif

internal abstract class DictionaryDataModel
{
    private readonly Dictionary<string, ICachedValue> cachedValues = new();
    private readonly IDictionaryModel dictionaryModel;

    /// <summary>Initializes a new instance of the <see cref="DictionaryDataModel" /> class.</summary>
    /// <param name="dictionaryModel">The backing dictionary.</param>
    protected DictionaryDataModel(IDictionaryModel dictionaryModel) => this.dictionaryModel = dictionaryModel;

    /// <summary>Gets the key prefix.</summary>
    protected abstract string Prefix { get; }

    /// <summary>Check if the dictionary has a value for the given id.</summary>
    /// <param name="id">The id of the item.</param>
    /// <returns><c>true</c> if the dictionary contains a value; otherwise, <c>false</c>.</returns>
    public bool HasValue(string id) => this.dictionaryModel.ContainsKey(this.Prefix + id);

    /// <summary>Serialize a bool to a string.</summary>
    /// <param name="value">The bool value to serialize.</param>
    /// <returns>The string value of the bool if true; otherwise, an empty string.</returns>
    protected static string BoolToString(bool value) =>
        value ? value.ToString(CultureInfo.InvariantCulture) : string.Empty;

    /// <summary>Serialize a color to a string.</summary>
    /// <param name="value">The color value to serialize.</param>
    /// <returns>The string value of the color if true; otherwise, an empty string.</returns>
    protected static string ColorToString(Color value) =>
        value.Equals(Color.Black) ? string.Empty : value.PackedValue.ToString(CultureInfo.InvariantCulture);

    /// <summary>Serialize a dictionary of string key-value-pairs to a string.</summary>
    /// <param name="value">The dictionary value to serialize.</param>
    /// <returns>The string value of the dictionary if it is not empty; otherwise, an empty string.</returns>
    protected static string DictToString(Dictionary<string, string>? value) =>
        value?.Any() == true ? string.Join(',', value.Select(pair => $"{pair.Key}={pair.Value}")) : string.Empty;

    /// <summary>Serialize an int to a string.</summary>
    /// <param name="value">The int value to serialize.</param>
    /// <returns>The string value of the int if it is not zero; otherwise, an empty string.</returns>
    protected static string IntToString(int value) =>
        value == 0 ? string.Empty : value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Deserialize a string to a bool.</summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The bool value, or <c>false</c> if the value is not a valid bool.</returns>
    protected static bool StringToBool(string value) =>
        !string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out var boolValue) && boolValue;

    /// <summary>Deserialize a string to a color.</summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The color value, or Black if the value is not a valid color.</returns>
    protected static Color StringToColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !uint.TryParse(value, out var intValue))
        {
            return Color.Black;
        }

        var color = Color.Black;
        color.PackedValue = intValue;
        return color;
    }

    /// <summary>Deserialize a string to a dictionary of string key-value-pairs.</summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The dictionary value, or an empty dictionary if the value is not valid.</returns>
    protected static Dictionary<string, string> StringToDict(string value) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Split(',').Select(part => part.Split('=')).ToDictionary(part => part[0], part => part[1])
            : new Dictionary<string, string>();

    /// <summary>Deserialize a string to an int.</summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The integer value, or the default value if the value is not a valid integer.</returns>
    protected static int StringToInt(string value) =>
        !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var intValue) ? intValue : 0;

    /// <summary>Retrieves a value from the dictionary based on the provided id.</summary>
    /// <param name="id">The id of the item.</param>
    /// <returns>The value from the dictionary, or empty if the value is not found.</returns>
    protected string Get(string id)
    {
        var key = this.Prefix + id;
        return !this.dictionaryModel.TryGetValue(key, out var value) ? string.Empty : value;
    }

    /// <summary>Retrieves a value from the cache based on the provided id and serializer.</summary>
    /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
    /// <param name="id">The id of the cached item.</param>
    /// <param name="deserializer">A function used to deserialize the value from its serialized form.</param>
    /// <returns>The deserialized value from the cache, or null if the value is not found in the cache.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the cached value is of a different type than expected.</exception>
    protected TValue? Get<TValue>(string id, Func<string, TValue> deserializer)
    {
        var key = this.Prefix + id;
        if (!this.dictionaryModel.TryGetValue(key, out var value))
        {
            return default(TValue);
        }

        if (this.cachedValues.TryGetValue(id, out var cachedValue))
        {
            if (cachedValue is not CachedValue<TValue> cached)
            {
                throw new InvalidOperationException($"Cached value for key '{key}' is of the wrong type.");
            }

            if (cached.OriginalValue == value)
            {
                return cached.Value;
            }
        }

        var newValue = deserializer(value);
        this.cachedValues[id] = new CachedValue<TValue>(value, newValue);
        return newValue;
    }

    /// <summary>Sets the value of an item identified by the given id.</summary>
    /// <param name="id">The id of the item.</param>
    /// <param name="value">The value to store.</param>
    protected void Set(string id, string value)
    {
        var key = this.Prefix + id;
        this.dictionaryModel.SetValue(key, value);
    }

    /// <summary>Sets the value of a cached item identified by the given id.</summary>
    /// <typeparam name="TValue">The type of the value to store.</typeparam>
    /// <param name="id">The id of the cached item.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="serializer">A function that converts the value to a string representation.</param>
    protected void Set<TValue>(string id, TValue value, Func<TValue, string> serializer)
    {
        var key = this.Prefix + id;
        var stringValue = serializer(value);
        this.cachedValues[id] = new CachedValue<TValue>(stringValue, value);
        this.dictionaryModel.SetValue(key, stringValue);
    }
}