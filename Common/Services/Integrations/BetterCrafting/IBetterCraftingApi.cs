namespace StardewMods.Common.Services.Integrations.BetterCrafting;
#nullable enable

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;
using StardewValley.Inventories;
using StardewValley.Network;


#if IS_BETTER_CRAFTING

using Leclair.Stardew.Common.Inventory;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.BetterCrafting.DynamicRules;
using Leclair.Stardew.BetterCrafting.Models;

namespace Leclair.Stardew.BetterCrafting;

#else

using StardewValley.Network;
using Newtonsoft.Json.Linq;

/// <summary>
/// The various currency types supported by <see cref="IBetterCrafting.CreateCurrencyIngredient(string, int)"/>
/// </summary>
public enum CurrencyType {
	/// <summary>
	/// The player's gold.
	/// </summary>
	Money,
	/// <summary>
	/// The player's earned points at the current festival. This should likely
	/// never actually be used, since players can't craft while they're at a
	/// festival in the first place.
	/// </summary>
	FestivalPoints,
	/// <summary>
	/// The player's casino points.
	/// </summary>
	ClubCoins,
	/// <summary>
	/// The player's Qi Gems.
	/// </summary>
	QiGems
};

/// <summary>
/// An <c>IInventoryProvider</c> is used by Better Crafting to discover and
/// interact with various item storages in the game.
/// </summary>
public interface IInventoryProvider {

	/// <summary>
	/// Check to see if this object is valid for inventory operations.
	///
	/// If location is null, it should not be considered when determining
	/// the validity of the object.
	/// 
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns>whether or not the object is valid</returns>
	bool IsValid(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// Check to see if items can be inserted into this object.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	bool CanInsertItems(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// Check to see if items can be extracted from this object.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	bool CanExtractItems(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// For objects larger than a single tile on the map, return the rectangle representing
	/// the object. For single tile objects, return null.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// Return the real position of the object. If the object has no position, returns null.
	/// For multi-tile objects, this should return the "main" object if there is one. 
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// Get the NetMutex that locks the object for multiplayer synchronization. This method must
	/// return a mutex. If null is returned, the object will be skipped.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// Whether or not a mutex is required for interacting with this object's inventory.
	/// You should always use a mutex to ensure items are handled safely with multiplayer,
	/// but in case you're doing something exceptional and Better Crafting should not
	/// worry about locking, you can explicitly disable mutex handling.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	bool IsMutexRequired(object obj, GameLocation? location, Farmer? who) => true;

	/// <summary>
	/// Get a list of items in the object's inventory, for modification or viewing. Assume that
	/// anything using this list will use GetMutex() to lock the inventory before modifying.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// Get a vanilla <c>IInventory</c> for an object, if one exists.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the object, or null if no player is involved</param>
	/// <returns></returns>
	IInventory? GetInventory(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// Check to see if a specific item is allowed to be stored in the object's inventory.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <param name="item">the item we're checking</param>
	bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item) => true;

	/// <summary>
	/// Clean the inventory of the object. This is for removing null entries, organizing, etc.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	void CleanInventory(object obj, GameLocation? location, Farmer? who);

	/// <summary>
	/// Get the actual inventory capacity of the object's inventory. New items may be added to the
	/// GetItems() list up until this count.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	int GetActualCapacity(object obj, GameLocation? location, Farmer? who);

}


/// <summary>
/// An <c>IBCInventory</c> represents an item storage that
/// Better Crafting is interacting with, whether by extracting
/// items or inserting them.
/// </summary>
public interface IBCInventory {

	/// <summary>
	/// Optional. If this inventory is associated with an object, that object.
	/// </summary>
	object Object { get; }

	/// <summary>
	/// If this inventory is associated with an object, where that object is located.
	/// </summary>
	GameLocation? Location { get; }

	/// <summary>
	/// If this inventory is associated with a player, the player.
	/// </summary>
	Farmer? Player { get; }

	/// <summary>
	/// If this inventory is managed by a NetMutex, or an object with one,
	/// which should be locked before manipulating the inventory, then
	/// provide it here.
	/// </summary>
	NetMutex? Mutex { get; }

	/// <summary>
	/// Get this inventory as a vanilla IInventory, if possible. May
	/// be null if the inventory is not a vanilla inventory.
	/// </summary>
	IInventory? Inventory { get; }

	/// <summary>
	/// Whether or not the inventory is locked and ready for read/write usage.
	/// </summary>
	bool IsLocked();

	/// <summary>
	/// Whether or not the inventory is a valid inventory.
	/// </summary>
	bool IsValid();

	/// <summary>
	/// Whether or not we can insert items into this inventory.
	/// </summary>
	bool CanInsertItems();

	/// <summary>
	/// Whether or not we can extract items from this inventory.
	/// </summary>
	bool CanExtractItems();

	/// <summary>
	/// For inventories associated with multiple tile regions in a location,
	/// such as a farm house kitchen, this is the region the inventory fills.
	/// Only rectangular shapes are supported. This is used for discovering
	/// connections to nearby inventories.
	/// </summary>
	Rectangle? GetMultiTileRegion();

	/// <summary>
	/// For inventories associated with a tile position in a location, such
	/// as a chest placed in the world.
	/// 
	/// For multi-tile inventories, this should be the primary tile if
	/// one exists.
	/// </summary>
	Vector2? GetTilePosition();

	/// <summary>
	/// Get this inventory as a list of items. May be null if
	/// there is an issue accessing the object's inventory.
	/// </summary>
	IList<Item?>? GetItems();

	/// <summary>
	/// Check to see if a specific item is allowed to be stored in
	/// this inventory.
	/// </summary>
	/// <param name="item">The item we're checking</param>
	bool IsItemValid(Item item);

	/// <summary>
	/// Attempt to clean the object's inventory. This should remove null
	/// entries, and run any other necessary logic.
	/// </summary>
	void CleanInventory();

	/// <summary>
	/// Get the number of item slots in the object's inventory. When adding
	/// items to the inventory, we will never extend the list beyond this
	/// number of entries.
	/// </summary>
	int GetActualCapacity();
}


/// <summary>
/// An <c>IIngredient</c> represents a single ingredient used when crafting a
/// recipe. An ingredient can be an item, a currency, or anything else.
///
/// The API provides methods for getting basic item and currency ingredients,
/// so you need not use this unless you're doing something fancy.
/// </summary>
public interface IIngredient {
	/// <summary>
	/// Whether or not this <c>IIngredient</c> supports quality control
	/// options, including using low quality first and limiting the maximum
	/// quality to use.
	/// </summary>
	bool SupportsQuality { get; }

	// Display
	/// <summary>
	/// The name of this ingredient to be displayed in the menu.
	/// </summary>
	string DisplayName { get; }

	/// <summary>
	/// The texture to use when drawing this ingredient in the menu.
	/// </summary>
	Texture2D Texture { get; }

	/// <summary>
	/// The source rectangle to use when drawing this ingredient in the menu.
	/// </summary>
	Rectangle SourceRectangle { get; }

#region Quantity

	/// <summary>
	/// The amount of this ingredient required to perform a craft.
	/// </summary>
	int Quantity { get; }

	/// <summary>
	/// Determine how much of this ingredient is available for crafting both
	/// in the player's inventory and in the other inventories.
	/// </summary>
	/// <param name="who">The farmer performing the craft</param>
	/// <param name="items">A list of all available <see cref="Item"/>s across
	/// all available <see cref="IBCInventory"/> instances. If you only support
	/// consuming ingredients from certain <c>IBCInventory</c> types, you should
	/// not use this value and instead iterate over the inventories. Please
	/// note that this does <b>not</b> include the player's inventory.</param>
	/// <param name="inventories">All the available inventories.</param>
	/// <param name="maxQuality">The maximum item quality we are allowed to
	/// count. This cannot be ignored unless <see cref="SupportsQuality"/>
	/// returns <c>false</c>.</param>
	int GetAvailableQuantity(Farmer who, IList<Item?>? items, IList<IBCInventory>? inventories, int maxQuality);

#endregion

#region Consumption

	/// <summary>
	/// Consume this ingredient out of the player's inventory and the other
	/// available inventories.
	/// </summary>
	/// <param name="who">The farmer performing the craft</param>
	/// <param name="inventories">All the available inventories.</param>
	/// <param name="maxQuality">The maximum item quality we are allowed to
	/// count. This cannot be ignored unless <see cref="SupportsQuality"/>
	/// returns <c>false</c>.</param>
	/// <param name="lowQualityFirst">Whether or not we should make an effort
	/// to consume lower quality ingredients before consuming higher quality
	/// ingredients.</param>
	void Consume(Farmer who, IList<IBCInventory>? inventories, int maxQuality, bool lowQualityFirst);

#endregion
}


/// <summary>
/// This event is dispatched by Better Crafting whenever a player performs a
/// craft, and may be fired multiple times in quick succession if a player is
/// performing bulk crafting.
/// </summary>
public interface IPerformCraftEvent {

	/// <summary>
	/// The player performing the craft.
	/// </summary>
	Farmer Player { get; }

	/// <summary>
	/// The item being crafted, may be null depending on the recipe.
	/// </summary>
	Item? Item { get; set; }

	/// <summary>
	/// The <c>BetterCraftingPage</c> menu instance that the player is
	/// crafting from.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// Cancel the craft, marking it as a failure. The ingredients will not
	/// be consumed and the player will not receive the item.
	/// </summary>
	void Cancel();

	/// <summary>
	/// Complete the craft, marking it as a success. The ingredients will be
	/// consumed and the player will receive the item, if there is one.
	/// </summary>
	void Complete();
}


/// <summary>
/// An <c>IRecipe</c> represents a single crafting recipe, though it need not
/// be associated with a vanilla <see cref="StardewValley.CraftingRecipe"/>.
/// Recipes usually produce <see cref="Item"/>s, but they are not required
/// to do so.
/// </summary>
public interface IRecipe {

#region Identity

	/// <summary>
	/// An additional sorting value to apply to recipes in the Better Crafting
	/// menu. Applied before other forms of sorting.
	/// </summary>
	string SortValue { get; }

	/// <summary>
	/// The internal name of the recipe. For standard recipes, this matches the
	/// name of the recipe used in the player's cookingRecipes / craftingRecipes
	/// dictionaries. For non-standard recipes, this can be anything as long as
	/// it's unique, and it's recommended to prefix the names with your mod's
	/// unique ID to ensure uniqueness.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// A name displayed to the user.
	/// </summary>
	string DisplayName { get; }

	/// <summary>
	/// An optional description of the recipe displayed on its tool-tip.
	/// </summary>
	string? Description { get; }

	/// <summary>
	/// Whether or not this recipe can be reversed with recycling.
	/// </summary>
	bool AllowRecycling { get; }

	/// <summary>
	/// Whether or not the player knows this recipe.
	/// </summary>
	/// <param name="who">The player we're asking about</param>
	bool HasRecipe(Farmer who);

	/// <summary>
	/// How many times the player has crafted this recipe. If advanced crafting
	/// information is enabled, and this value is non-zero, it will be
	/// displayed on recipe tool-tips.
	/// </summary>
	/// <param name="who">The player we're asking about.</param>
	int GetTimesCrafted(Farmer who);

	/// <summary>
	/// The vanilla <c>CraftingRecipe</c> instance for this recipe, if one
	/// exists. This may be used for interoperability with some other
	/// mods, but is not required.
	/// </summary>
	CraftingRecipe? CraftingRecipe { get; }

#endregion

#region Display

	/// <summary>
	/// The texture to use when drawing this recipe in the menu.
	/// </summary>
	Texture2D Texture { get; }

	/// <summary>
	/// The source rectangle to use when drawing this recipe in the menu.
	/// </summary>
	Rectangle SourceRectangle { get; }

	/// <summary>
	/// How tall this recipe should appear in the menu, in grid squares.
	/// </summary>
	int GridHeight { get; }

	/// <summary>
	/// How wide this recipe should appear in the menu, in grid squares.
	/// </summary>
	int GridWidth { get; }

#endregion

#region Cost and Quantity

	/// <summary>
	/// The quantity of item produced every time this recipe is crafted.
	/// </summary>
	int QuantityPerCraft { get; }

	/// <summary>
	/// The ingredients used by this recipe.
	/// </summary>
	IIngredient[]? Ingredients { get; }

#endregion

#region Creation

	/// <summary>
	/// Whether or not the item created by this recipe is stackable, and thus
	/// eligible for bulk crafting.
	/// </summary>
	bool Stackable { get; }

	/// <summary>
	/// Check to see if the given player can currently craft this recipe. This
	/// method is suitable for checking external conditions. For example, the
	/// add-on for crafting buildings from the crafting menu uses this to check
	/// that the current <see cref="GameLocation"/> allows building.
	/// </summary>
	/// <param name="who">The player we're asking about.</param>
	bool CanCraft(Farmer who);

	/// <summary>
	/// An optional, extra string to appear on item tool-tips. This can be used
	/// for displaying error messages to the user, or anything else that would
	/// be relevant. For example, the add-on for crafting buildings uses this
	/// to display error messages telling users why they are unable to craft
	/// a building, if they cannot.
	/// </summary>
	/// <param name="who">The player we're asking about.</param>
	string? GetTooltipExtra(Farmer who);

	/// <summary>
	/// Create an instance of the Item this recipe crafts, if this recipe
	/// crafts an item. Returning null is perfectly acceptable.
	/// </summary>
	Item? CreateItem();

	/// <summary>
	/// This method is called when performing a craft, and can be used to
	/// perform asynchronous actions or other additional logic as required.
	/// While crafting is taking place, Better Crafting will hold locks on
	/// every inventory involved. You should ideally do as little work
	/// here as possible.
	/// </summary>
	/// <param name="evt">Details about the event, and methods for telling
	/// Better Crafting when the craft has succeeded or failed.</param>
	void PerformCraft(IPerformCraftEvent evt) {
		evt.Complete();
	}

#endregion
}


/// <summary>
/// Better Crafting uses <c>IRecipeProvider</c> to discover crafting recipes
/// for display in the menu.
/// </summary>
public interface IRecipeProvider {
	/// <summary>
	/// The priority of this recipe provider, sort sorting purposes.
	/// When handling CraftingRecipe instances, the first provider
	/// to return a result is used.
	/// </summary>
	int RecipePriority { get; }

	/// <summary>
	/// Get an <see cref="IRecipe"/> wrapper for a <see cref="CraftingRecipe"/>.
	/// </summary>
	/// <param name="recipe">The vanilla <c>CraftingRecipe</c> to wrap</param>
	/// <returns>An IRecipe wrapper, or null if this provider does
	/// not handle this recipe.</returns>
	IRecipe? GetRecipe(CraftingRecipe recipe);

	/// <summary>
	/// Whether or not additional recipes from this provider should be
	/// cached. If the list should be updated every time the player
	/// opens the menu, this should return false.
	/// </summary>
	bool CacheAdditionalRecipes { get; }

	/// <summary>
	/// Get any additional recipes in IRecipe form. Additional recipes
	/// are those recipes not included in the <see cref="CraftingRecipe.cookingRecipes"/>
	/// and <see cref="CraftingRecipe.craftingRecipes"/> objects.
	/// </summary>
	/// <param name="cooking">Whether we want cooking recipes or crafting recipes.</param>
	/// <returns>An enumeration of this provider's additional recipes, or null.</returns>
	IEnumerable<IRecipe>? GetAdditionalRecipes(bool cooking);
}


/// <summary>
/// IDynamicRuleData instances represent all the configuration data associated
/// with dynamic rules that have been added to categories.
/// </summary>
public interface IDynamicRuleData {

	/// <summary>
	/// The ID of the dynamic rule this data is for. Please note that this is
	/// an absolute ID, rather than the mod-specific IDs that would be passed
	/// when registering a custom dynamic rule. You can use <see cref="GetAbsoluteRuleId(string)"/>
	/// to obtain an appropriate ID if necessary, or just create it yourself
	/// by combining your mod's unique ID with your custom rule's ID.
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// A dictionary of configuration data for this dynamic rule, as parsed
	/// by the game's JSON serializer.
	/// </summary>
	public IDictionary<string, JToken> Fields { get; }

}

/// <summary>
/// IDynamicRuleHandler instances handle the logic of determining whether or
/// not any given <see cref="IRecipe"/> matches a dynamic rule, and thus
/// whether the recipe should be displayed in a category using rules.
///
/// It also handles anything necessary for displaying a user interface to
/// the user for editing the rule's configuration.
/// </summary>
public interface IDynamicRuleHandler {

#region Display

	/// <summary>
	/// The name of the dynamic rule, to be displayed to the user when
	/// editing a category.
	/// </summary>
	string DisplayName { get; }

	/// <summary>
	/// A description of what the dynamic rule matches, to be displayed
	/// to the user when hovering over the rule in the interface to
	/// add a new rule.
	/// </summary>
	string Description { get; }

	/// <summary>
	/// The source texture for an icon to display alongside this dynamic rule.
	/// </summary>
	Texture2D Texture { get; }

	/// <summary>
	/// The source area for an icon to display alongside this dynamic rule.
	/// </summary>
	Rectangle Source { get; }

	/// <summary>
	/// Whether or not this dynamic rule should be allowed to be added to a
	/// category multiple times.
	/// </summary>
	bool AllowMultiple { get; }

#endregion

#region Editing

	/// <summary>
	/// Whether or not this dynamic rule has a custom editor.
	/// </summary>
	bool HasEditor { get; }

	/// <summary>
	/// WIP! This currently does not function. In the future, this will obtain
	/// a new editor child menu that will be rendered within the rule editor.
	/// </summary>
	/// <param name="parent">The rule editor</param>
	/// <param name="data">The data of the rule being edited</param>
	IClickableMenu? GetEditor(IClickableMenu parent, IDynamicRuleData data);

#endregion

#region Processing

	/// <summary>
	/// This method is called before a dynamic rule is executed, allowing the
	/// rule to parse its configuration into a state object that can be
	/// re-used when checking recipes against the rule.
	/// </summary>
	/// <param name="data">The data of the rule</param>
	/// <returns>A custom state object, or null if no state is required</returns>
	object? ParseState(IDynamicRuleData data);

	/// <summary>
	/// This method checks whether a recipe matches this dynamic rule or not.
	/// </summary>
	/// <param name="recipe">The recipe being checked.</param>
	/// <param name="item">The item output of the recipe being checked.</param>
	/// <param name="state">The state object returned from <see cref="ParseState(IDynamicRuleData)"/></param>
	bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, object? state);

#endregion
}

/// <summary>
/// ISimpleInputRuleHandler is an <see cref="IDynamicRuleHandler"/> that only
/// has a single text input for configuring it. This allows you to create
/// basic rules without needing to implement a configuration interface.
/// </summary>
public interface ISimpleInputRuleHandler : IDynamicRuleHandler {

	/// <summary>
	/// If set to a string, this string will be displayed alongside the
	/// text editor added to the rule editor for this rule.
	/// </summary>
	string? HelpText { get; }

}

#endif

/// <summary>
/// This class allows you to easily modify any part of an <see cref="IRecipe"/>'s
/// behavior, including its appearance, cost, and the item(s) it produces.
///
/// This is primarily meant for customizing how an existing <see cref="CraftingRecipe"/>
/// functions, but can be used for creating new recipes.
/// </summary>
public interface IRecipeBuilder {

	#region Identity

	/// <summary>
	/// An additional sorting value to apply to the recipe in the Better Crafting
	/// menu. This is applied before other forms of sorting. Setting this to
	/// <c>null</c> will restore the default behavior.
	/// </summary>
	/// <param name="value">The sorting value. "0" by default.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder SortValue(string? value);

	/// <summary>
	/// Set the recipe's display name. Setting this to <c>null</c> will
	/// restore the default display name.
	/// </summary>
	/// <param name="displayName">A method that returns a display name.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder DisplayName(Func<string>? displayName);

	/// <summary>
	/// Set the recipe's optional description. Setting this to <c>null</c>
	/// will restore the default description. The method returning <c>null</c>
	/// will result in no description being displayed.
	/// </summary>
	/// <param name="description">A method that returns a description.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder Description(Func<string?>? description);

	/// <summary>
	/// Set whether or not the recipe can be reversed with recycling. By
	/// default, this is true.
	/// </summary>
	/// <param name="allow">Can the recipe be reversed with recycling?</param>
	IRecipeBuilder AllowRecycling(bool allow);

	/// <summary>
	/// Check to see whether or not a given player knows this recipe.
	/// Setting this to <c>null</c> will restore the default behavior.
	/// </summary>
	/// <param name="hasRecipe">A method that checks if the player knows
	/// this recipe.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder HasRecipe(Func<Farmer, bool>? hasRecipe);

	/// <summary>
	/// Get how many times a given player has crafted this recipe. Setting this
	/// to <c>null</c> will restore the default behavior.
	/// </summary>
	/// <param name="timesCrafted">A method that returns the number of times crafted.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder GetTimesCrafted(Func<Farmer, int>? timesCrafted);

	#endregion

	#region Display

	/// <summary>
	/// The texture to use when drawing this recipe in UI. Setting this to
	/// <c>null</c> will restore the default texture.
	///
	/// The result of this function call will be cached as appropriate.
	///
	/// If a recipe has no texture, an error icon will be displayed instead
	/// for the recipe.
	/// </summary>
	/// <param name="texture">A method that returns a <see cref="Texture2D"/>.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder Texture(Func<Texture2D>? texture);

	/// <summary>
	/// The source rectangle to use when drawing this recipe in UI. Setting
	/// this to <c>null</c> will restore the default source rectangle. If
	/// this method returns <c>null</c>, the entire texture will be used.
	/// </summary>
	/// <param name="source">A method that returns a <see cref="Rectangle"/> or <c>null</c>.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder Source(Func<Rectangle?>? source);

	/// <summary>
	/// Set a size for the recipe to appear as within UI. By default, this will
	/// be calculated based on the aspect ratio of the source rectangle.
	///
	/// Please note that you should not use an entry larger than 4x4,
	/// and usually not more than 1x2 or 2x2, to ensure the recipe will
	/// fit in the user interface correctly.
	/// </summary>
	/// <param name="width">The size of the recipe in the grid, defaults to 1 or 2</param>
	/// <param name="height">The size of the recipe in the grid, defaults to 1 or 2</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder GridSize(int width, int height);

	/// <summary>
	/// Clear a grid size set with a previous call to <see cref="GridSize(int, int)"/>.
	/// </summary>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder ClearGridSize();

	#endregion

	#region Ingredients

	/// <summary>
	/// Clear the recipe's ingredients list. Optionally, a predicate can be
	/// provided to only clear ingredients from the list that match the predicate.
	/// </summary>
	/// <param name="predicate">An optional predicate for selecting which
	/// ingredients should be removed.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder ClearIngredients(Func<IIngredient, bool>? predicate = null);

	/// <summary>
	/// Add a new ingredient to the recipe's ingredients list.
	/// </summary>
	/// <param name="ingredient">The ingredient to add</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder AddIngredient(IIngredient ingredient);

	/// <summary>
	/// Add multiple new ingredients to the recipe's ingredients list.
	/// </summary>
	/// <param name="ingredients">The ingredients to add</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder AddIngredients(IEnumerable<IIngredient> ingredients);

	#endregion

	#region Can Craft

	/// <summary>
	/// Check to see if the given player can currently craft this recipe. See
	/// <see cref="IRecipe.CanCraft(Farmer)"/> for more details. Setting this
	/// to <c>null</c> will restore the default behavior.
	/// </summary>
	/// <param name="canCraft">A method to check if the given player can craft the recipe.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder CanCraft(Func<Farmer, bool>? canCraft);

	/// <summary>
	/// An optional, extra string to appear on recipe tool-tips. See
	/// <see cref="IRecipe.GetTooltipExtra(Farmer)"/> for more details.
	/// Setting this to <c>null</c> will restore the default behavior.
	/// </summary>
	/// <param name="tooltipExtra">A method returning an optional, extra string to display.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder GetTooltipExtra(Func<Farmer, string?>? tooltipExtra);

	#endregion

	#region Crafting and Output

	/// <summary>
	/// A method called when performing a craft, which can be used to perform
	/// asynchronous actions or other additional logic. See <see cref="IRecipe.PerformCraft(IPerformCraftEvent)"/>
	/// for more details. Setting this to <c>null</c> will restore the
	/// default behavior.
	/// </summary>
	/// <param name="action">A method called when performing a craft.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder OnPerformCraft(Action<IPerformCraftEvent>? action);

	/// <summary>
	/// Creates an instance of the <see cref="Item"/> this recipe crafts, if
	/// this recipe crafts an item. Return <c>null</c> if the recipe does not
	/// create an item (and use your logic in <see cref="OnPerformCraft(Action{IPerformCraftEvent}?)"/>).
	/// Setting this to <c>null</c> will restore the default behavior.
	/// </summary>
	/// <param name="createItem">A method that returns a created <see cref="Item"/>, or <c>null</c>.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder Item(Func<Item?>? createItem);

	/// <summary>
	/// The quantity of <see cref="Item"/> produced every time this recipe is
	/// crafted. This value overrides the stack size of the item returned from
	/// <see cref="Item(Func{Item?}?)"/>. Setting this to <c>null</c> will
	/// restore the default value, <c>1</c>.
	/// </summary>
	/// <param name="quantity">The quantity of item to produce per craft.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder Quantity(int? quantity);

	/// <summary>
	/// Whether or not the <see cref="Item"/> produced by this recipe should be
	/// considered stackable, which allows or disallows the use of the bulk
	/// crafting menu. Setting this to <c>null</c> will restore the default
	/// value, which checks the <see cref="Item.maximumStackSize"/> of this
	/// recipe's output item.
	///
	/// Please note that this does not make the resulting item unstackable, but
	/// only affects how it is handled in the UI.
	/// </summary>
	/// <param name="stackable">Whether or not the recipe's output is stackable.</param>
	/// <returns>The same <see cref="IRecipeBuilder"/> instance</returns>
	IRecipeBuilder Stackable(bool? stackable);

	#endregion

	#region Output

	/// <summary>
	/// Build this recipe and return it.
	/// </summary>
	/// <returns>The built <see cref="IRecipe"/></returns>
	IRecipe Build();

	#endregion

}

public interface ICraftingStation {

	/// <summary>
	/// The crafting station's unique Id.
	/// </summary>
	string Id { get; }

	/// <summary>
	/// The display name of this crafting station.
	/// </summary>
	string? DisplayName { get; }

	/// <summary>
	/// Whether this crafting station's recipes should be available outside
	/// of this crafting station or not.
	/// </summary>
	bool AreRecipesExclusive { get; }

	/// <summary>
	/// When this is true, this crafting station's recipes will always be
	/// available, even if the player hasn't learned the recipe yet.
	/// </summary>
	bool DisplayUnknownRecipes { get; }

	/// <summary>
	/// Whether or not this crafting station is for cooking.
	/// </summary>
	bool IsCooking { get; }

	/// <summary>
	/// A list of recipes included in this crafting station.
	/// </summary>
	string[] Recipes { get; }

}

/// <summary>
/// This interface contains a few basic properties on the Better Crafting
/// menu that may be useful for other mods.
/// </summary>
public interface IBetterCraftingMenu {

	/// <summary>
	/// The <see cref="IClickableMenu"/> instance for this menu. This is the
	/// same object, but included for convenience due to how API proxying works.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// Whether or not this menu is going to perform container discovery.
	/// </summary>
	bool DiscoverContainers { get; }

	/// <summary>
	/// Whether or not this menu is going to scan buildings as part of
	/// container discovery.
	/// </summary>
	bool DiscoverBuildings { get; }

	/// <summary>
	/// If this menu is associated with a specific crafting station, this
	/// is the crafting station.
	/// </summary>
	ICraftingStation? Station { get; }

	/// <summary>
	/// Whether or not this crafting menu is ready, meaning that it has
	/// finished initializing. Note that it can still be busy crafting, so
	/// you may need to check <see cref="Working"/> as well.
	/// </summary>
	bool IsReady { get; }

	/// <summary>
	/// Whether or not this crafting menu is for cooking. If this is
	/// false, then the menu is for crafting recipes.
	/// </summary>
	bool Cooking { get; }

	/// <summary>
	/// Whether or not this is a standalone menu. If this is false,
	/// this menu is likely contained in <see cref="GameMenu"/>.
	/// </summary>
	bool Standalone { get; }

	/// <summary>
	/// Whether or not the user is currently editing their categories.
	/// </summary>
	bool Editing { get; }

	/// <summary>
	/// Whether or not the menu is actively crafting something. This
	/// will only return true when a craft is happening, or when the
	/// menu is waiting for an asynchronous craft to return.
	/// </summary>
	bool Working { get; }

	/// <summary>
	/// Get the current recipe. This is normally the recipe that the
	/// player's cursor is hovering over, but when performing a craft
	/// or when the bulk crafting menu is open, it will return the
	/// relevant recipe.
	/// </summary>
	IRecipe? ActiveRecipe { get; }

	/// <summary>
	/// The location this menu was opened from, if it has an associated
	/// location. This may be null if the menu was not opened by
	/// interacting with something in the world, like a Workbench.
	/// </summary>
	GameLocation? Location { get; }

	/// <summary>
	/// The position this menu was opened from, if it has an associated
	/// position. This may be null if the menu was not opened by
	/// interacting with something in the world, like a kitchen.
	/// </summary>
	Vector2? Position { get; }

	/// <summary>
	/// The multi-tile area this menu was opened from, if it has an
	/// associated area. This is not set when working with single
	/// tile objects like a Workbench.
	/// </summary>
	Rectangle? Area { get; }

	/// <summary>
	/// Calling this method will toggle edit mode, as though the user
	/// clicked the button themselves.
	/// </summary>
	void ToggleEditMode();

	/// <summary>
	/// Get a list of specific recipes that are to be displayed in the
	/// crafting menu. If this list is <c>null</c>, all recipes will be
	/// displayed to the user.
	/// </summary>
	IReadOnlyList<string>? GetListedRecipes();

	/// <summary>
	/// Set a new list of specific recipes that are to be displayed in the
	/// crafting menu. Note: If the user does not know these recipes, they
	/// will not be displayed even if they're in this list.
	///
	/// Set the list to <c>null</c> to display all recipes.
	/// </summary>
	/// <param name="recipes">The list of recipes that should be displayed.</param>
	void UpdateListedRecipes(IEnumerable<string>? recipes);
}

/// <summary>
/// This event is emitted by <see cref="IBetterCrafting"/> whenever a new
/// Better Crafting menu is opened, and serves to allow other mods to add
/// or remove specific containers from a menu.
/// </summary>
public interface IPopulateContainersEvent {
	/// <summary>
	/// The relevant Better Crafting menu.
	/// </summary>
	IBetterCraftingMenu Menu { get; }

	/// <summary>
	/// A list of all the containers this menu should draw items from.
	/// </summary>
	IList<Tuple<object, GameLocation?>> Containers { get; }

	/// <summary>
	/// Set this to true to prevent Better Crafting from running its
	/// own container discovery logic, if you so desire.
	/// </summary>
	bool DisableDiscovery { get; set; }

}

public interface IBetterCrafting {

	#region GUI

	/// <summary>
	/// Try to open the Better Crafting menu. This may fail if there is another
	/// menu open that cannot be replaced.
	///
	/// If opening the menu from an object in the world, such as a workbench,
	/// its location and tile position can be provided for automatic detection
	/// of nearby chests.
	///
	/// Better Crafting has its own handling of mutexes, so please do not worry
	/// about locking Chests before handing them off to the menu.
	///
	/// When discovering additional containers, Better Crafting scans all tiles
	/// around each of its existing known containers. If a location and position
	/// for the menu source is provided, the tiles around that position will
	/// be scanned as well.
	///
	/// Discovery depends on the user's settings, though at a minimum a 3x3 area
	/// will be scanned to mimic the scanning radius of the vanilla workbench.
	/// </summary>
	/// <param name="cooking">If true, open the cooking menu. If false, open the crafting menu.</param>
	/// <param name="silent_open">If true, do not make a sound upon opening the menu.</param>
	/// <param name="location">The map the associated object is in, or null if there is no object</param>
	/// <param name="position">The tile position the associated object is at, or null if there is no object</param>
	/// <param name="area">The tile area the associated object covers, or null if there is no object or if the object only covers a single tile</param>
	/// <param name="discover_containers">If true, attempt to discover additional material containers.</param>
	/// <param name="containers">An optional list of containers to draw extra crafting materials from.</param>
	/// <param name="listed_recipes">An optional list of recipes by name. If provided, only these recipes will be listed in the crafting menu.</param>
	/// <param name="discover_buildings">If true, attempt to discover additional containers inside of adjacent buildings.</param>
	/// <returns>Whether or not the menu was opened successfully</returns>
	bool OpenCraftingMenu(
		bool cooking,
		bool silent_open = false,
		GameLocation? location = null,
		Vector2? position = null,
		Rectangle? area = null,
		bool discover_containers = true,
		IList<Tuple<object, GameLocation?>>? containers = null,
		IList<string>? listed_recipes = null,
		bool discover_buildings = false
	);

	/// <summary>
	/// Return the Better Crafting menu's type. In case you want to do
	/// spooky stuff to it, I guess.
	/// </summary>
	/// <returns>The BetterCraftingMenu type.</returns>
	Type GetMenuType();

	/// <summary>
	/// Get the currently open Better Crafting menu. This may be <c>null</c> if
	/// the menu is still opening.
	/// </summary>
	IBetterCraftingMenu? GetActiveMenu();

	/// <summary>
	/// This event is fired whenever a new Better Crafting menu is opened,
	/// allowing other mods to manipulate the list of containers.
	/// </summary>
	event Action<IPopulateContainersEvent>? MenuPopulateContainers;

	#endregion

	#region Recipes

	/// <summary>
	/// Return a list of all recipes that are exclusive to a specific
	/// crafting station. These recipes should not be listed in general
	/// crafting stations.
	/// </summary>
	/// <param name="cooking">If true, return cooking recipes. If false, return
	/// crafting recipes.</param>
	/// <returns>An enumeration of recipe names.</returns>
	IEnumerable<string> GetExclusiveRecipes(bool cooking);

	/// <summary>
	/// Register a recipe provider with Better Crafting. Calling this
	/// will also invalidate the recipe cache.
	///
	/// If the recipe provider was already registered, this does nothing.
	/// </summary>
	/// <param name="provider">The recipe provider to add</param>
	void AddRecipeProvider(IRecipeProvider provider);

	/// <summary>
	/// Unregister a recipe provider. Calling this will also invalidate
	/// the recipe cache.
	///
	/// If the recipe provider was not registered, this does nothing.
	/// </summary>
	/// <param name="provider">The recipe provider to remove</param>
	void RemoveRecipeProvider(IRecipeProvider provider);

	/// <summary>
	/// Invalidate the recipe cache. You should call this if your recipe
	/// provider ever adds new recipes after registering it.
	/// </summary>
	void InvalidateRecipeCache();

	/// <summary>
	/// Get all known recipes from all providers.
	/// </summary>
	/// <param name="cooking">If true, return cooking recipes. If false, return
	/// crafting recipes.</param>
	/// <returns>A collection of the recipes.</returns>
	IReadOnlyCollection<IRecipe> GetRecipes(bool cooking);

	/// <summary>
	/// Get a new <see cref="IRecipeBuilder"/> for customizing a recipe.
	/// </summary>
	/// <param name="recipe">The recipe to customize.</param>
	IRecipeBuilder RecipeBuilder(CraftingRecipe recipe);

	/// <summary>
	/// Get a new <see cref="IRecipeBuilder"/> for creating a new recipe, not
	/// based on an existing crafting recipe. If not replacing an existing
	/// <see cref="CraftingRecipe"/> then <paramref name="name"/> should be
	/// a new, unique string.
	/// </summary>
	/// <param name="name">The recipe's name.</param>
	IRecipeBuilder RecipeBuilder(string name);

	#endregion

	#region Ingredients

	/// <summary>
	/// Create a simple <see cref="IIngredient"/> that matches an item by ID
	/// and that consumes an exact quantity.
	/// </summary>
	/// <param name="item">The item ID to match.</param>
	/// <param name="quantity">The quantity to consume.</param>
	IIngredient CreateBaseIngredient(int item, int quantity);

	/// <summary>
	/// Create a simple <see cref="IIngredient"/> that matches items using a
	/// function and that consumes an exact quantity.
	/// </summary>
	/// <param name="matcher">The function to check items</param>
	/// <param name="quantity">The quantity to consume.</param>
	/// <param name="displayName">The name to display for the ingredient.</param>
	/// <param name="texture">The texture to display the ingredient with.</param>
	/// <param name="source">The source rectangle of the texture to display.</param>
	/// <param name="recycleTo">An optional item to return when recycling this
	/// ingredient. Providing a value here marks this ingredients as non-fuzzy
	/// for the purpose of recycling. If you want fuzzy behavior, just leave
	/// this as null and an appropriate item will be discovered.</param>
	IIngredient CreateMatcherIngredient(Func<Item, bool> matcher, int quantity, Func<string> displayName, Func<Texture2D> texture, Rectangle? source = null, Item? recycleTo = null);

	/// <summary>
	/// Create a simple <see cref="IIngredient"/> that matches a specific
	/// currency and consumes an exact quantity.
	/// </summary>
	/// <param name="type">The currency to match.</param>
	/// <param name="quantity">The quantity to consume.</param>
	IIngredient CreateCurrencyIngredient(CurrencyType type, int quantity);

	/// <summary>
	/// Create a simple <see cref="IIngredient"/> that does not match anything
	/// but requires a quantity of one, thus always preventing a recipe
	/// from being crafted. It displays as an error item in the
	/// ingredients list.
	/// </summary>
	IIngredient CreateErrorIngredient();

	#endregion

	#region Item Manipulation

	/// <summary>
	/// Lock the provided inventories and call a delegate with the locked
	/// <see cref="IBCInventory"/> instances, ready to be safely manipulated.
	///
	/// This is the same method used internally by the crafting menu to do
	/// just-in-time mutex locks when crafting.
	///
	/// The delegate's first argument is a list of locked inventories, and
	/// the second argument is an Action to call when you are done.
	/// </summary>
	/// <param name="inventories">The list of things with inventories you want
	/// to lock, the same as you'd pass into other API instances. Each one
	/// is handled using an <see cref="IInventoryProvider"/>.</param>
	/// <param name="who">The relevant farmer.</param>
	/// <param name="withLocks">A delegate to call when the locks are ready,
	/// which will not be immediate.</param>
	void WithInventories(
		IEnumerable<Tuple<object, GameLocation?>> inventories,
		Farmer? who,
		Action<IList<IBCInventory>, Action> withLocks
	);

	/// <summary>
	/// Consume matching items from a player, and also from a set of
	/// <see cref="IBCInventory"/> instances. This is a helper method for
	/// building custom <see cref="IIngredient"/>s.
	///
	/// This method is aware of the mod "Stack Quality" and handles merged
	/// stacks correctly.
	/// </summary>
	/// <param name="items">An enumeration of tuples where the function
	/// matches items, and the integer is the quantity to consume.</param>
	/// <param name="who">The player to consume items from, if any. Items
	/// are consumed from the player's inventory first.</param>
	/// <param name="inventories">An enumeration of <see cref="IBCInventory"/>
	/// instances to consume items from, such as the one passed to
	/// <see cref="IIngredient.Consume(Farmer, IList{IBCInventory}?, int, bool)"/>.</param>
	/// <param name="maxQuality">The maximum quality to consume.</param>
	/// <param name="lowQualityFirst">Whether or not to consume low quality
	/// items first.</param>
	void ConsumeItems(IEnumerable<(Func<Item, bool>, int)> items, Farmer? who, IEnumerable<IBCInventory>? inventories, int maxQuality = int.MaxValue, bool lowQualityFirst = false);

	/// <summary>
	/// Count the number of <see cref="Item"/>s available that match the given
	/// predicate, between the given player's inventory and the given enumeration
	/// of <see cref="Item"/>s.
	///
	/// This method is aware of the mod "Stack Quality" and handles merged
	/// stacks correctly.
	/// </summary>
	/// <param name="predicate">A method for checking whether a given <see cref="Item"/> should be counted.</param>
	/// <param name="who">An optional player, to include that player's inventory in the search.</param>
	/// <param name="items">An optional enumeration of <see cref="Item"/>s to include in the search.</param>
	/// <param name="maxQuality">The maximum quality of item to count.</param>
	/// <returns>The number of matching items.</returns>
	int CountItem(Func<Item, bool> predicate, Farmer? who, IEnumerable<Item?>? items, int maxQuality = int.MaxValue);

	#endregion

	#region Categories

	/// <summary>
	/// Create a new default category for recipes. Every player will receive
	/// this category, but they may delete it or alter it as they see fit.
	/// </summary>
	/// <param name="cooking">If true, this category is added to cooking.
	/// Otherwise, crafting.</param>
	/// <param name="categoryId">An internal ID for the category. Make sure
	/// this is unique.</param>
	/// <param name="Name">A method returning a human-readable name to be
	/// displayed in the menu.</param>
	/// <param name="recipeNames">An enumeration of recipe names for recipes to
	/// display in the category.</param>
	/// <param name="iconRecipe">The name of a recipe to use as the category's
	/// default icon.</param>
	///
	void CreateDefaultCategory(bool cooking, string categoryId, Func<string> Name, IEnumerable<string>? recipeNames = null, string? iconRecipe = null, bool useRules = false, IEnumerable<IDynamicRuleData>? rules = null);

	/// <summary>
	/// Add recipes to a default category. If a player has modified their
	/// category, this will not affect them.
	/// </summary>
	/// <param name="cooking">If true, we alter a cooking category.
	/// Otherwise, crafting.</param>
	/// <param name="categoryId">The ID of the category to alter.</param>
	/// <param name="recipeNames">An enumeration of recipe names for recipes to
	/// add to the category.</param>
	void AddRecipesToDefaultCategory(bool cooking, string categoryId, IEnumerable<string> recipeNames);

	/// <summary>
	/// Remove recipes from a default category. If a player has modified their
	/// category, this will not affect them.
	/// </summary>
	/// <param name="cooking">If true, we alter a cooking category.
	/// Otherwise, crafting.</param>
	/// <param name="categoryId">The ID of the category to alter.</param>
	/// <param name="recipeNames">An enumeration of recipe names for recipes to
	/// remove from the category.</param>
	void RemoveRecipesFromDefaultCategory(bool cooking, string categoryId, IEnumerable<string> recipeNames);

	#endregion

	#region Dynamic Rules

	/// <summary>
	/// Get the absolute rule ID of a rule added via <see cref="RegisterRuleHandler(string, IDynamicRuleHandler)"/>
	/// so that you can reference it when manipulating categories using the API.
	/// </summary>
	/// <param name="id">An ID for the rule handler. This should be unique
	/// within your mod, but can overlap with IDs from other mods as rule IDs
	/// are prefixed with your mod ID internally.</param>
	/// <returns>The prefixed rule ID.</returns>
	string GetAbsoluteRuleId(string id);

	/// <summary>
	/// Register a new dynamic rule handler for use with dynamic categories.
	/// </summary>
	/// <param name="id">An ID for the rule handler. This should be unique
	/// within your mod, but can overlap with IDs from other mods as rule IDs
	/// are prefixed with your mod ID internally.</param>
	/// <param name="handler">The rule handler instance.</param>
	/// <returns>Whether or not the handler was successfully registered.</returns>
	bool RegisterRuleHandler(string id, IDynamicRuleHandler handler);

	/// <summary>
	/// See <see cref="RegisterRuleHandler(string, IDynamicRuleHandler)"/>
	/// for details. This method exists to ensure the API translation layer functions
	/// as you would expect.
	/// </summary>
	bool RegisterRuleHandler(string id, ISimpleInputRuleHandler handler);

	/// <summary>
	/// Unregister a dynamic rule handler.
	/// </summary>
	/// <param name="id">The ID of the rule handler.</param>
	/// /// <returns>Whether or not the handler was successfully unregistered.</returns>
	bool UnregisterRuleHandler(string id);

	#endregion

	#region Inventories

	/// <summary>
	/// Register an inventory provider with Better Crafting. Inventory
	/// providers are used for interfacing with chests and other objects
	/// in the world that contain items.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="provider"></param>
	void RegisterInventoryProvider(Type type, IInventoryProvider provider);

	/// <summary>
	/// Unregister an inventory provider.
	/// </summary>
	/// <param name="type"></param>
	void UnregisterInventoryProvider(Type type);

	#endregion

}
