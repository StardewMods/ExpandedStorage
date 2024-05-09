namespace StardewMods.CustomBush.Framework.Services;

using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.CustomBush;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.CustomBush.Framework.Models;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

/// <summary>Responsible for handling tea logic.</summary>
internal sealed class ModPatches : BaseService
{
    private static ModPatches instance = null!;

    private readonly AssetHandler assetHandler;
    private readonly MethodInfo checkItemPlantRules;
    private readonly IEventManager eventManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly string modDataId;
    private readonly string modDataItem;
    private readonly string modDataQuality;
    private readonly string modDataStack;
    private readonly string modDataTexture;
    private readonly IPatchManager patchManager;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="ModPatches" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into external code.</param>
    public ModPatches(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        IPatchManager patchManager,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        // Init
        ModPatches.instance = this;
        this.modDataId = this.ModId + "/Id";
        this.modDataItem = this.ModId + "/ShakeOff";
        this.modDataQuality = this.ModId + "/Quality";
        this.modDataStack = this.ModId + "/Stack";
        this.modDataTexture = this.ModId + "/Texture";
        this.assetHandler = assetHandler;
        this.eventManager = eventManager;
        this.gameContentHelper = gameContentHelper;
        this.patchManager = patchManager;
        this.reflectionHelper = reflectionHelper;
        this.checkItemPlantRules =
            typeof(GameLocation).GetMethod("CheckItemPlantRules", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new MethodAccessException("Unable to access CheckItemPlantRules");

        // Events
        this.eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <summary>Determines if the given Bush instance is a custom bush.</summary>
    /// <param name="bush">The bush instance to check.</param>
    /// <returns><c>true</c> if the bush is a custom bush; otherwise, <c>false</c>.</returns>
    public bool IsCustomBush(Bush bush) =>
        bush.modData.TryGetValue(this.modDataId, out var id) && this.assetHandler.Data.ContainsKey(id);

    /// <summary>Tries to get the custom bush model associated with the given bush.</summary>
    /// <param name="bush">The bush.</param>
    /// <param name="customBush">
    /// When this method returns, contains the custom bush associated with the given bush; otherwise,
    /// null.
    /// </param>
    /// <returns><c>true</c> if the custom bush associated with the given bush is found; otherwise, <c>false</c>.</returns>
    public bool TryGetCustomBush(Bush bush, [NotNullWhen(true)] out CustomBush? customBush)
    {
        customBush = null;
        return bush.modData.TryGetValue(this.modDataId, out var id)
            && this.assetHandler.Data.TryGetValue(id, out customBush);
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static Bush AddModData(Bush bush, SObject obj)
    {
        if (!ModPatches.instance.assetHandler.Data.ContainsKey(obj.QualifiedItemId))
        {
            return bush;
        }

        bush.modData[ModPatches.instance.modDataId] = obj.QualifiedItemId;
        bush.setUpSourceRect();
        return bush;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Automate_GetOutput_postfix(object __instance, ref object __result)
    {
        var bush = ModPatches.instance.reflectionHelper.GetProperty<Bush>(__instance, "Machine", false).GetValue();
        if (bush is null)
        {
            return;
        }

        var item = ModPatches.instance.reflectionHelper.GetField<Item>(__result, "Item");
        var count = ModPatches.instance.reflectionHelper.GetProperty<int>(__result, "Count");
        var oldItem = item.GetValue();
        var newItem = ModPatches.CreateItem(oldItem, bush);
        if (oldItem != newItem)
        {
            item.SetValue(newItem);
            count.SetValue(newItem.Stack);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Bush_draw_prefix(
        Bush __instance,
        SpriteBatch spriteBatch,
        float ___shakeRotation,
        NetRectangle ___sourceRect,
        float ___yDrawOffset)
    {
        if (!__instance.modData.TryGetValue(ModPatches.instance.modDataId, out var id)
            || !ModPatches.instance.assetHandler.Data.TryGetValue(id, out var bushModel))
        {
            return true;
        }

        var x = (__instance.Tile.X * 64) + 32;
        var y = (__instance.Tile.Y * 64) + 64 + ___yDrawOffset;
        if (__instance.drawShadow.Value)
        {
            spriteBatch.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 4)),
                Game1.shadowTexture.Bounds,
                Color.White,
                0,
                new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                4,
                SpriteEffects.None,
                1E-06f);
        }

        var path = !__instance.IsSheltered()
            ? bushModel.Texture
            : !string.IsNullOrWhiteSpace(bushModel.IndoorTexture)
                ? bushModel.IndoorTexture
                : bushModel.Texture;

        var texture = ModPatches.instance.gameContentHelper.Load<Texture2D>(path);

        spriteBatch.Draw(
            texture,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y)),
            ___sourceRect.Value,
            Color.White,
            ___shakeRotation,
            new Vector2(8, 32),
            4,
            __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            ((__instance.getBoundingBox().Center.Y + 48) / 10000f) - (__instance.Tile.X / 1000000f));

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Bush_GetShakeOffItem_postfix(Bush __instance, ref string __result)
    {
        if (__instance.modData.TryGetValue(ModPatches.instance.modDataItem, out var itemId)
            && !string.IsNullOrWhiteSpace(itemId))
        {
            __result = itemId;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Bush_inBloom_postfix(Bush __instance, ref bool __result)
    {
        if (__instance.modData.TryGetValue(ModPatches.instance.modDataItem, out var itemId)
            && !string.IsNullOrWhiteSpace(itemId))
        {
            __result = true;
            return;
        }

        if (!__instance.modData.TryGetValue(ModPatches.instance.modDataId, out var id)
            || !ModPatches.instance.assetHandler.Data.TryGetValue(id, out var bushModel))
        {
            return;
        }

        var season = __instance.Location.GetSeason();
        var dayOfMonth = Game1.dayOfMonth;
        var age = __instance.getAge();

        // Fails basic conditions
        if (age < bushModel.AgeToProduce || dayOfMonth < bushModel.DayToBeginProducing)
        {
            ModPatches.instance.Log.Trace(
                "{0} will not produce. Age: {1} < {2} , Day: {3} < {4}",
                id,
                age.ToString(CultureInfo.InvariantCulture),
                bushModel.AgeToProduce.ToString(CultureInfo.InvariantCulture),
                dayOfMonth.ToString(CultureInfo.InvariantCulture),
                bushModel.DayToBeginProducing.ToString(CultureInfo.InvariantCulture));

            __result = false;
            return;
        }

        ModPatches.instance.Log.Trace(
            "{0} passed basic conditions. Age: {1} >= {2} , Day: {3} >= {4}",
            id,
            age.ToString(CultureInfo.InvariantCulture),
            bushModel.AgeToProduce.ToString(CultureInfo.InvariantCulture),
            dayOfMonth.ToString(CultureInfo.InvariantCulture),
            bushModel.DayToBeginProducing.ToString(CultureInfo.InvariantCulture));

        // Fails default season conditions
        if (!bushModel.Seasons.Any() && season == Season.Winter && !__instance.IsSheltered())
        {
            ModPatches.instance.Log.Trace(
                "{0} will not produce. Season: {1} and plant is outdoors.",
                id,
                season.ToString());

            __result = false;
            return;
        }

        if (!bushModel.Seasons.Any())
        {
            ModPatches.instance.Log.Trace(
                "{0} passed default season condition. Season: {1} or plant is indoors.",
                id,
                season.ToString());
        }

        // Fails custom season conditions
        if (bushModel.Seasons.Any() && !bushModel.Seasons.Contains(season) && !__instance.IsSheltered())
        {
            ModPatches.instance.Log.Trace(
                "{0} will not produce. Season: {1} not in {2} and plant is outdoors.",
                id,
                season.ToString(),
                string.Join(',', bushModel.Seasons));

            __result = false;
            return;
        }

        if (bushModel.Seasons.Any())
        {
            ModPatches.instance.Log.Trace(
                "{0} passed custom season conditions. Season: {1} in {2} or plant is indoors.",
                id,
                season.ToString(),
                string.Join(',', bushModel.Seasons));
        }

        // Try to produce item
        ModPatches.instance.Log.Trace("{0} attempting to produce random item.", id);
        if (!ModPatches.instance.TryToProduceRandomItem(__instance, bushModel, out var item))
        {
            ModPatches.instance.Log.Trace("{0} will not produce. No item was produced.", id);
            __result = false;
            return;
        }

        ModPatches.instance.Log.Trace(
            "{0} selected {1} to grow with quality {2} and quantity {3}.",
            id,
            item.QualifiedItemId,
            item.Quality,
            item.Stack);

        __result = true;
        __instance.modData[ModPatches.instance.modDataItem] = item.QualifiedItemId;
        __instance.modData[ModPatches.instance.modDataQuality] = item.Quality.ToString(CultureInfo.InvariantCulture);
        __instance.modData[ModPatches.instance.modDataStack] = item.Stack.ToString(CultureInfo.InvariantCulture);
    }

    private static IEnumerable<CodeInstruction> Bush_performToolAction_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools
            .GetDeclaredMethods(typeof(ItemRegistry))
            .First(method => method.Name == nameof(ItemRegistry.Create) && !method.IsGenericMethod);

        return new CodeMatcher(instructions)
            .MatchStartForward(new CodeMatch(instruction => instruction.Calls(method)))
            .RemoveInstruction()
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.CreateBushItem)))
            .InstructionEnumeration();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Bush_setUpSourceRect_postfix(Bush __instance, NetRectangle ___sourceRect)
    {
        if (!__instance.modData.TryGetValue(ModPatches.instance.modDataId, out var id)
            || !ModPatches.instance.assetHandler.Data.TryGetValue(id, out var bushModel))
        {
            return;
        }

        __instance.modData[ModPatches.instance.modDataTexture] = !__instance.IsSheltered()
            ? bushModel.Texture
            : !string.IsNullOrWhiteSpace(bushModel.IndoorTexture)
                ? bushModel.IndoorTexture
                : bushModel.Texture;

        var age = __instance.getAge();
        var growthPercent = (float)age / bushModel.AgeToProduce;
        var x = (Math.Min(2, (int)(2 * growthPercent)) + __instance.tileSheetOffset.Value) * 16;
        var y = bushModel.TextureSpriteRow * 16;

        ___sourceRect.Value = new Rectangle(x, y, 16, 32);
    }

    private static IEnumerable<CodeInstruction> Bush_shake_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchStartForward(
                new CodeMatch(
                    instruction => instruction.Calls(
                        AccessTools.DeclaredMethod(
                            typeof(Game1),
                            nameof(Game1.createObjectDebris),
                            [
                                typeof(string),
                                typeof(int),
                                typeof(int),
                                typeof(int),
                                typeof(int),
                                typeof(float),
                                typeof(GameLocation),
                            ]))))
            .RemoveInstruction()
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.CreateObjectDebris)))
            .InstructionEnumeration();

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static Item CreateBushItem(string itemId, int amount, int quality, bool allowNull, Bush bush)
    {
        if (bush.modData.TryGetValue(ModPatches.instance.modDataId, out var bushId))
        {
            itemId = bushId;
        }

        return ItemRegistry.Create(itemId, amount, quality, allowNull);
    }

    private static Item CreateItem(Item i, Bush bush)
    {
        // Return cached item
        if (bush.modData.TryGetValue(ModPatches.instance.modDataItem, out var itemId)
            && !string.IsNullOrWhiteSpace(itemId))
        {
            if (!bush.modData.TryGetValue(ModPatches.instance.modDataQuality, out var quality)
                || !int.TryParse(quality, out var itemQuality))
            {
                itemQuality = 1;
            }

            if (!bush.modData.TryGetValue(ModPatches.instance.modDataStack, out var stack)
                || !int.TryParse(stack, out var itemStack))
            {
                itemStack = 1;
            }

            return ItemRegistry.Create(itemId, itemStack, itemQuality);
        }

        // Try to return random item
        if (bush.modData.TryGetValue(ModPatches.instance.modDataId, out var bushId)
            && ModPatches.instance.assetHandler.Data.TryGetValue(bushId, out var bushModel)
            && ModPatches.instance.TryToProduceRandomItem(bush, bushModel, out var item))
        {
            return item;
        }

        // Return vanilla instance
        return i;
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static void CreateObjectDebris(
        string id,
        int xTile,
        int yTile,
        int groundLevel,
        int itemQuality,
        float velocityMultiplier,
        GameLocation? location,
        Bush bush)
    {
        // Create cached item
        if (bush.modData.TryGetValue(ModPatches.instance.modDataItem, out var itemId)
            && !string.IsNullOrWhiteSpace(itemId))
        {
            if (!bush.modData.TryGetValue(ModPatches.instance.modDataQuality, out var quality)
                || !int.TryParse(quality, out itemQuality))
            {
                itemQuality = 1;
            }

            if (!bush.modData.TryGetValue(ModPatches.instance.modDataStack, out var stack)
                || !int.TryParse(stack, out var itemStack))
            {
                itemStack = 1;
            }

            for (var i = 0; i < itemStack; i++)
            {
                Game1.createObjectDebris(itemId, xTile, yTile, groundLevel, itemQuality, velocityMultiplier, location);
            }

            bush.modData.Remove(ModPatches.instance.modDataItem);
            bush.modData.Remove(ModPatches.instance.modDataQuality);
            bush.modData.Remove(ModPatches.instance.modDataStack);
            return;
        }

        bush.modData.Remove(ModPatches.instance.modDataItem);
        bush.modData.Remove(ModPatches.instance.modDataQuality);
        bush.modData.Remove(ModPatches.instance.modDataStack);

        // Try to create random item
        if (bush.modData.TryGetValue(ModPatches.instance.modDataId, out var bushId)
            && ModPatches.instance.assetHandler.Data.TryGetValue(bushId, out var bushModel)
            && ModPatches.instance.TryToProduceRandomItem(bush, bushModel, out var item))
        {
            Game1.createObjectDebris(
                item.QualifiedItemId,
                xTile,
                yTile,
                groundLevel,
                item.Quality,
                velocityMultiplier,
                location);

            return;
        }

        // Create vanilla item
        Game1.createObjectDebris(id, xTile, yTile, groundLevel, itemQuality, velocityMultiplier, location);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void GameLocation_CheckItemPlantRules_postfix(
        GameLocation __instance,
        ref bool __result,
        string itemId,
        bool isGardenPot,
        bool defaultAllowed,
        ref string deniedMessage)
    {
        var metadata = ItemRegistry.GetMetadata(itemId);
        if (metadata is null
            || !ModPatches.instance.assetHandler.Data.TryGetValue(metadata.QualifiedItemId, out var bushModel))
        {
            return;
        }

        var parameters = new object[] { bushModel.PlantableLocationRules, isGardenPot, defaultAllowed, null! };
        __result = (bool)ModPatches.instance.checkItemPlantRules.Invoke(__instance, parameters)!;
        deniedMessage = (string)parameters[3];
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void HoeDirt_canPlantThisSeedHere_postfix(string itemId, ref bool __result)
    {
        if (!__result || !ModPatches.instance.assetHandler.Data.ContainsKey($"(O){itemId}"))
        {
            return;
        }

        __result = false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void IndoorPot_performObjectDropInAction_postfix(
        IndoorPot __instance,
        Item dropInItem,
        bool probe,
        ref bool __result)
    {
        if (!ModPatches.instance.assetHandler.Data.ContainsKey(dropInItem.QualifiedItemId)
            || __instance.hoeDirt.Value.crop != null)
        {
            return;
        }

        if (!probe)
        {
            __instance.bush.Value = new Bush(__instance.TileLocation, 3, __instance.Location)
            {
                modData =
                {
                    [ModPatches.instance.modDataId] = dropInItem.QualifiedItemId,
                },
            };

            if (!__instance.Location.IsOutdoors)
            {
                __instance.bush.Value.inPot.Value = true;
                __instance.bush.Value.loadSprite();
                Game1.playSound("coin");
            }
        }

        __result = true;
    }

    private static IEnumerable<CodeInstruction>
        JunimoHarvester_update_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(
                new CodeMatch(OpCodes.Ldstr, "(O)815"),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ldc_I4_0))
            .Advance(2)
            .Insert(
                new CodeInstruction(OpCodes.Ldloc_S, (short)7),
                CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.CreateItem)))
            .InstructionEnumeration();

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_IsTeaSapling_postfix(SObject __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (ModPatches.instance.assetHandler.Data.ContainsKey(__instance.QualifiedItemId))
        {
            __result = true;
        }
    }

    private static IEnumerable<CodeInstruction>
        Object_placementAction_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(
                new CodeMatch(
                    OpCodes.Newobj,
                    AccessTools.Constructor(
                        typeof(Bush),
                        [typeof(Vector2), typeof(int), typeof(GameLocation), typeof(int)])))
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.AddModData)))
            .InstructionEnumeration();

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        // Patches
        this.patchManager.Add(
            this.ModId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.draw), [typeof(SpriteBatch)]),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Bush_draw_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.GetShakeOffItem)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Bush_GetShakeOffItem_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.inBloom)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Bush_inBloom_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.performToolAction)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Bush_performToolAction_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.setUpSourceRect)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Bush_setUpSourceRect_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.shake)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Bush_shake_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                typeof(GameLocation).GetMethod(
                    nameof(GameLocation.CheckItemPlantRules),
                    BindingFlags.Public | BindingFlags.Instance)
                ?? throw new MethodAccessException("Unable to access CheckItemPlantRules"),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.GameLocation_CheckItemPlantRules_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(HoeDirt), nameof(HoeDirt.canPlantThisSeedHere)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.HoeDirt_canPlantThisSeedHere_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(IndoorPot), nameof(IndoorPot.performObjectDropInAction)),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.IndoorPot_performObjectDropInAction_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(JunimoHarvester), nameof(JunimoHarvester.update)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.JunimoHarvester_update_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.IsTeaSapling)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Object_IsTeaSapling_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Object_placementAction_transpiler)),
                PatchType.Transpiler));

        var automateType = Type.GetType(
            "Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures.BushMachine, Automate");

        if (automateType is not null)
        {
            var methodGetOutput = AccessTools.DeclaredMethod(automateType, "GetOutput");
            if (methodGetOutput is not null)
            {
                this.patchManager.Add(
                    this.ModId,
                    new SavedPatch(
                        methodGetOutput,
                        AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Automate_GetOutput_postfix)),
                        PatchType.Postfix));
            }
        }

        this.patchManager.Patch(this.ModId);
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private Item? TryToProduceItem(Bush bush, ICustomBushDrop drop)
    {
        if (!Game1.random.NextBool(drop.Chance))
        {
            return null;
        }

        if (drop.Condition != null
            && !GameStateQuery.CheckConditions(
                drop.Condition,
                bush.Location,
                null,
                null,
                null,
                null,
                bush.Location.SeedsIgnoreSeasonsHere() ? GameStateQuery.SeasonQueryKeys : null))
        {
            ModPatches.instance.Log.Trace(
                "{0} did not select {1}. Failed: {2}",
                bush.modData[ModPatches.instance.modDataId],
                drop.Id,
                drop.Condition);

            return null;
        }

        if (drop.Season.HasValue
            && bush.Location.SeedsIgnoreSeasonsHere()
            && drop.Season != Game1.GetSeasonForLocation(bush.Location))
        {
            ModPatches.instance.Log.Trace(
                "{0} did not select {1}. Failed: {2}",
                bush.modData[ModPatches.instance.modDataId],
                drop.Id,
                drop.Season.ToString());

            return null;
        }

        var item = ItemQueryResolver.TryResolveRandomItem(
            drop,
            new ItemQueryContext(bush.Location, null, null),
            false,
            null,
            null,
            null,
            delegate(string query, string error)
            {
                this.Log.Error(
                    "{0} failed parsing item query {1} for item {2}: {3}",
                    bush.modData[ModPatches.instance.modDataId],
                    query,
                    drop.Id,
                    error);
            });

        return item;
    }

    private bool TryToProduceRandomItem(Bush bush, CustomBush customBush, [NotNullWhen(true)] out Item? item)
    {
        foreach (var drop in customBush.ItemsProduced)
        {
            item = this.TryToProduceItem(bush, drop);
            if (item is null)
            {
                continue;
            }

            return true;
        }

        item = null;
        return false;
    }
}