namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using System.Text;
using StardewMods.Common.Enums;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Helper methods to convert between different text formats.</summary>
internal sealed class Localized : BaseService
{
    private readonly ITranslationHelper translations;

    /// <summary>Initializes a new instance of the <see cref="Localized" /> class.</summary>
    /// <param name="translations">Dependency used for accessing translations.</param>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public Localized(ILog log, IManifest manifest, ITranslationHelper translations)
        : base(log, manifest) =>
        this.translations = translations;

    /// <summary>Formats item attribute using localized text when available.</summary>
    /// <param name="value">The value for item attribute to format.</param>
    /// <returns>Localized text for the item attribute.</returns>
    public static string Attribute(string? value) =>
        ItemAttributeExtensions.TryParse(value, out var attribute)
            ? attribute switch
            {
                ItemAttribute.Category => I18n.Attribute_Category_Name(),
                ItemAttribute.Name => I18n.Attribute_Name_Name(),
                ItemAttribute.Quantity => I18n.Attribute_Quantity_Name(),
                ItemAttribute.Quality => I18n.Attribute_Quality_Name(),
                ItemAttribute.Tags => I18n.Attribute_Tags_Name(),
                _ => I18n.Attribute_Any_Name(),
            }
            : I18n.Attribute_Any_Name();

    /// <summary>Formats border using localized text when available.</summary>
    /// <param name="value">The value for border to format.</param>
    /// <returns>Localized text for the border.</returns>
    public static string Border(int value) =>
        value switch
        {
            (int)InventoryMenu.BorderSide.Left => I18n.Border_Left(),
            (int)InventoryMenu.BorderSide.Right => I18n.Border_Right(),
            (int)InventoryMenu.BorderSide.Top => I18n.Border_Top(),
            (int)InventoryMenu.BorderSide.Bottom => I18n.Border_Bottom(),
            _ => I18n.Border_Left(),
        };

    /// <summary>Formats carry chest limit using localized text when available.</summary>
    /// <param name="value">The value for carry chest limit to format.</param>
    /// <returns>Localized text for the carry chest limit distance.</returns>
    public static string CarryChestLimit(int value) =>
        value switch
        {
            1 => I18n.Config_CarryChestLimit_ValueOne(),
            > 1 => I18n.Config_CarryChestLimit_ValueMany(value),
            _ => I18n.Config_CarryChestLimit_ValueUnlimited(),
        };

    /// <summary>Formats range distance using localized text when available.</summary>
    /// <param name="value">The value for range distance to format.</param>
    /// <returns>Localized text for the range distance.</returns>
    public static string Distance(int value) =>
        value switch
        {
            (int)RangeOption.Default => I18n.Option_Default_Name(),
            (int)RangeOption.Disabled => I18n.Option_Disabled_Name(),
            (int)RangeOption.Inventory => I18n.Option_Inventory_Name(),
            (int)RangeOption.World - 1 => I18n.Range_Distance_Unlimited(),
            (int)RangeOption.World => I18n.Option_World_Name(),
            >= (int)RangeOption.Location => I18n.Range_Distance_Many(
                Math.Pow(2, 1 + value - (int)RangeOption.Location).ToString(CultureInfo.InvariantCulture)),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a capacity value using localized text when available.</summary>
    /// <param name="parentCapacity">The capacity this value inherits from.</param>
    /// <param name="getSize">Get method for the current menu size.</param>
    /// <returns>Localized text for the method value.</returns>
    public static Func<int, string> FormatCapacity(int parentCapacity, Func<int> getSize) =>
        value =>
        {
            var size = getSize();
            var actualCapacity = value switch
            {
                5 => -1, > 0 when size > 1 => value * size, 1 => 9, 2 => 36, 3 => 70, _ => 70 * value,
            };

            var sb = new StringBuilder();
            if (actualCapacity == parentCapacity)
            {
                sb.Append(I18n.Config_DefaultOption_Indicator());
            }

            sb.Append(
                actualCapacity switch
                {
                    -1 => I18n.Capacity_Unlimited_Name(),
                    0 => I18n.Option_Default_Name(),
                    _ => I18n.Capacity_Other_Name(actualCapacity),
                });

            return sb.ToString();
        };

    /// <summary>Formats range distance using localized text when available.</summary>
    /// <param name="parentOption">The range this value inherits from.</param>
    /// <param name="parentValue">The distance this value inherits from.</param>
    /// <returns>Localized text for the range distance.</returns>
    public static Func<int, string> FormatDistance(RangeOption? parentOption = null, int parentValue = 0) =>
        value =>
        {
            var actualValue = value switch
            {
                (int)RangeOption.Default => I18n.Option_Default_Name(),
                (int)RangeOption.Disabled => I18n.Option_Disabled_Name(),
                (int)RangeOption.Inventory => I18n.Option_Inventory_Name(),
                (int)RangeOption.World - 1 => I18n.Range_Distance_Unlimited(),
                (int)RangeOption.World => I18n.Option_World_Name(),
                >= (int)RangeOption.Location => I18n.Range_Distance_Many(
                    Math.Pow(2, 1 + value - (int)RangeOption.Location).ToString(CultureInfo.InvariantCulture)),
                _ => I18n.Option_Default_Name(),
            };

            if (parentOption == null)
            {
                return actualValue;
            }

            var sb = new StringBuilder();
            switch (value)
            {
                case (int)RangeOption.Default when parentOption == RangeOption.Default:
                case (int)RangeOption.Disabled when parentOption == RangeOption.Disabled:
                case (int)RangeOption.Inventory when parentOption == RangeOption.Inventory:
                case (int)RangeOption.World - 1 when parentOption == RangeOption.Location && parentValue == -1:
                case (int)RangeOption.World when parentOption == RangeOption.World:
                case >= (int)RangeOption.Location when parentOption == RangeOption.Location
                    && parentValue == Math.Pow(2, 1 + value - (int)RangeOption.Location):
                case
                    { } when parentOption == RangeOption.Default:
                    sb.Append(I18n.Config_DefaultOption_Indicator());
                    break;
            }

            sb.Append(actualValue);
            return sb.ToString();
        };

    /// <summary>Formats menu size using localized text when available.</summary>
    /// <param name="parentOption">The menu size this value inherits from.</param>
    /// <returns>Localized text for the menu size.</returns>
    public static Func<string, string> FormatMenuSize(ChestMenuOption? parentOption = null) =>
        value =>
        {
            var actualOption = ChestMenuOptionExtensions.TryParse(value, out var option)
                ? option
                : ChestMenuOption.Default;

            var sb = new StringBuilder();
            if (parentOption?.Equals(actualOption) == true)
            {
                sb.Append(I18n.Config_DefaultOption_Indicator());
            }

            sb.Append(Localized.FormatMenuSize(actualOption));
            return sb.ToString();
        };

    /// <summary>Formats capacity option using localized text when available.</summary>
    /// <param name="option">The value for capacity to format.</param>
    /// <returns>Localized text for the capacity.</returns>
    public static string FormatMenuSize(ChestMenuOption option) =>
        option switch
        {
            ChestMenuOption.Disabled => I18n.Option_Disabled_Name(),
            ChestMenuOption.Default => I18n.Option_Default_Name(),
            _ => I18n.Capacity_Other_Name((int)option),
        };

    /// <summary>Formats a method value using localized text when available.</summary>
    /// <param name="parentMethod">The method this value inherits from.</param>
    /// <returns>Localized text for the method value.</returns>
    public static Func<string, string> FormatMethod(FilterMethod? parentMethod = null) =>
        value =>
        {
            var actualMethod = FilterMethodExtensions.TryParse(value, out var method) ? method : FilterMethod.Default;

            var sb = new StringBuilder();
            if (parentMethod?.Equals(actualMethod) == true)
            {
                sb.Append(I18n.Config_DefaultOption_Indicator());
            }

            sb.Append(Localized.FormatMethod(actualMethod));
            return sb.ToString();
        };

    /// <summary>Formats an option value using localized text when available.</summary>
    /// <param name="parentOption">The option this value inherits from.</param>
    /// <returns>Localized text for the option value.</returns>
    public static Func<string, string> FormatOption(FeatureOption? parentOption = null) =>
        value =>
        {
            var actualOption = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default;

            var sb = new StringBuilder();
            if (parentOption?.Equals(actualOption) == true)
            {
                sb.Append(I18n.Config_DefaultOption_Indicator());
            }

            sb.Append(Localized.FormatOption(actualOption));
            return sb.ToString();
        };

    /// <summary>Formats an option value using localized text when available.</summary>
    /// <param name="option">The option value to format.</param>
    /// <returns>Localized text for the option value.</returns>
    public static string FormatOption(FeatureOption option) =>
        option switch
        {
            FeatureOption.Disabled => I18n.Option_Disabled_Name(),
            FeatureOption.Enabled => I18n.Option_Enabled_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a range value using localized text when available.</summary>
    /// <param name="parentRange">The range this value inherits from.</param>
    /// <returns>Localized text for the range value.</returns>
    public static Func<string, string> FormatRange(RangeOption? parentRange = null) =>
        value =>
        {
            var actualRange = RangeOptionExtensions.TryParse(value, out var rangeOption)
                ? rangeOption
                : RangeOption.Default;

            var sb = new StringBuilder();
            if (parentRange?.Equals(actualRange) == true)
            {
                sb.Append(I18n.Config_DefaultOption_Indicator());
            }

            sb.Append(Localized.FormatRange(actualRange));
            return sb.ToString();
        };

    /// <summary>Formats a range value using localized text when available.</summary>
    /// <param name="range">The range value to format.</param>
    /// <returns>Localized text for the range value.</returns>
    public static string FormatRange(RangeOption range) =>
        range switch
        {
            RangeOption.Disabled => I18n.Option_Disabled_Name(),
            RangeOption.Inventory => I18n.Option_Inventory_Name(),
            RangeOption.Location => I18n.Option_Location_Name(),
            RangeOption.World => I18n.Option_World_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a priority value using localized text when available.</summary>
    /// <param name="value">The priority value to format.</param>
    /// <returns>Localized text for the priority value.</returns>
    public static string FormatStashPriority(int value) =>
        value switch
        {
            -3 => I18n.Priority_Lowest_Name(),
            -2 => I18n.Priority_Lower_Name(),
            -1 => I18n.Priority_Low_Name(),
            0 => I18n.Priority_Default_Name(),
            1 => I18n.Priority_High_Name(),
            2 => I18n.Priority_Higher_Name(),
            3 => I18n.Priority_Highest_Name(),
            _ => I18n.Priority_Default_Name(),
        };

    /// <summary>Formats a method value using localized text when available.</summary>
    /// <param name="method">The method value to format.</param>
    /// <returns>Localized text for the method value.</returns>
    private static string FormatMethod(FilterMethod method) =>
        method switch
        {
            FilterMethod.Sorted => I18n.Method_Sorted_Name(),
            FilterMethod.GrayedOut => I18n.Method_GrayedOut_Name(),
            FilterMethod.Hidden => I18n.Method_Hidden_Name(),
            _ => I18n.Option_Default_Name(),
        };
}