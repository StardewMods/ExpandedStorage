namespace StardewMods.CrystallineJunimoChests.Framework.Services;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Harmony Patches for Enhanced Junimo Chests.</summary>
internal sealed class ModPatches
{
    private static AssetHandler assetHandler = null!;

    private readonly IPatchManager patchManager;

    /// <summary>Initializes a new instance of the <see cref="ModPatches" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public ModPatches(AssetHandler assetHandler, IEventManager eventManager, IPatchManager patchManager)
    {
        // Init
        ModPatches.assetHandler = assetHandler;
        this.patchManager = patchManager;

        // Events
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);

        // Patches
        this.patchManager.Add(
            Mod.Id,
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(Chest),
                    nameof(Chest.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_draw_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(Chest),
                    nameof(Chest.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool)]),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_drawLocal_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_GetActualCapacity_postfix)),
                PatchType.Postfix));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_draw_prefix(
        Chest __instance,
        ref int ___currentLidFrame,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha)
    {
        if (!__instance.playerChest.Value || __instance.QualifiedItemId != "(BC)256")
        {
            return true;
        }

        var drawX = (float)x;
        var drawY = (float)y;
        if (__instance.localKickStartTile.HasValue)
        {
            drawX = Utility.Lerp(__instance.localKickStartTile.Value.X, drawX, __instance.kickProgress);
            drawY = Utility.Lerp(__instance.localKickStartTile.Value.Y, drawY, __instance.kickProgress);
        }

        var baseSortOrder = Math.Max(0f, (((drawY + 1f) * 64f) - 24f) / 10000f) + (drawX * 1E-05f);
        if (__instance.localKickStartTile.HasValue)
        {
            spriteBatch.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2((drawX + 0.5f) * 64f, (drawY + 0.5f) * 64f)),
                Game1.shadowTexture.Bounds,
                Color.Black * 0.5f,
                0f,
                new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                4f,
                SpriteEffects.None,
                0.0001f);

            drawY -= (float)Math.Sin(__instance.kickProgress * Math.PI) * 0.5f;
        }

        var pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX, drawY - 1f) * Game1.tileSize);
        var startingLidFrame = __instance.startingLidFrame.Value;
        var lastLidFrame = __instance.getLastLidFrame();

        var frame = new Rectangle(
            Math.Min(lastLidFrame - startingLidFrame + 1, Math.Max(0, ___currentLidFrame - startingLidFrame)) * 16,
            0,
            16,
            32);

        if (__instance.playerChoiceColor.Value.Equals(Color.Black))
        {
            spriteBatch.Draw(
                ModPatches.assetHandler.Texture,
                pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
                frame,
                Color.White * alpha,
                0f,
                Vector2.Zero,
                4f,
                SpriteEffects.None,
                baseSortOrder);

            return false;
        }

        spriteBatch.Draw(
            ModPatches.assetHandler.Texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = 32 },
            Color.White * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder);

        var selection = DiscreteColorPicker.getSelectionFromColor(__instance.playerChoiceColor.Value) - 1;
        var data = ModPatches.assetHandler.Data;
        var color = __instance.playerChoiceColor.Value;
        if (selection >= 0 && selection < data.Colors.Length)
        {
            color = Utility.StringToColor(data.Colors[selection].Color) ?? __instance.playerChoiceColor.Value;
        }

        spriteBatch.Draw(
            ModPatches.assetHandler.Texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = 64 },
            color * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 1E-05f);

        if (selection < 0)
        {
            return false;
        }

        spriteBatch.Draw(
            ModPatches.assetHandler.Texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = (selection * 32) + 96 },
            Color.White * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 2E-05f);

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_drawLocal_prefix(
        Chest __instance,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha,
        bool local)
    {
        if (!__instance.playerChest.Value || __instance.QualifiedItemId != "(BC)256")
        {
            return true;
        }

        var pos = local
            ? new Vector2(x, y - 64)
            : Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize);

        var baseSortOrder = local ? 0.89f : ((y * 64) + 4) / 10000f;
        var frame = new Rectangle(0, 0, 16, 32);

        if (__instance.playerChoiceColor.Value.Equals(Color.Black))
        {
            spriteBatch.Draw(
                ModPatches.assetHandler.Texture,
                pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
                frame,
                Color.White * alpha,
                0f,
                Vector2.Zero,
                4f,
                SpriteEffects.None,
                baseSortOrder);

            return false;
        }

        spriteBatch.Draw(
            ModPatches.assetHandler.Texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = 32 },
            Color.White * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder);

        spriteBatch.Draw(
            ModPatches.assetHandler.Texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = 64 },
            __instance.playerChoiceColor.Value * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 1E-05f);

        var selection = DiscreteColorPicker.getSelectionFromColor(__instance.playerChoiceColor.Value) - 1;
        if (selection < 0)
        {
            return false;
        }

        spriteBatch.Draw(
            ModPatches.assetHandler.Texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = (32 * selection) + 96 },
            Color.White * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 2E-05f);

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (!__instance.playerChest.Value || __instance.QualifiedItemId != "(BC)256")
        {
            return;
        }

        __result = 9;
    }

    private void OnGameLaunched(GameLaunchedEventArgs e) => this.patchManager.Patch(Mod.Id);
}