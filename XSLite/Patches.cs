using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace ImJustMatt.XSLite
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming convention defined by Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    internal class Patches
    {
        private static XSLite _mod;
        
        public Patches(XSLite mod, Harmony harmony)
        {
            _mod = mod;
            
            // Chest Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)}),
                prefix: new HarmonyMethod(GetType(), nameof(Chest_draw_prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool)}),
                prefix: new HarmonyMethod(GetType(), nameof(Chest_drawLocal_prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.drawInMenu), new[] {typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool)}),
                prefix: new HarmonyMethod(GetType(), nameof(Chest_drawInMenu_prefix))
            );

            // Object Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                prefix: new HarmonyMethod(GetType(), nameof(Object_checkForAction_prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)}),
                prefix: new HarmonyMethod(GetType(), nameof(Object_draw_prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
                prefix: new HarmonyMethod(GetType(), nameof(Object_drawWhenHeld_prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.drawPlacementBounds)),
                prefix: new HarmonyMethod(GetType(), nameof(Object_drawPlacementBounds_prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.performToolAction)),
                prefix: new HarmonyMethod(GetType(), nameof(Object_performToolAction_prefix))
            );
            
            // Utility Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
                postfix: new HarmonyMethod(GetType(), nameof(Utility_playerCanPlaceItemHere_postfix))
            );
        }
        
        #region ChestPatches
        private static bool Chest_draw_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/X", out var xStr)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var yStr)
                || !_mod.Storages.TryGetValue(storageName, out var storage)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos))
                return true;
            
            // Only draw from origin sprite for bigger expanded storages)
            if ((storage.TileHeight > 1 || storage.TileWidth > 1) && (x != xPos || y != yPos))
                return false;
            
            var draw_x = (float) x;
            var draw_y = (float) y;
            if (__instance.localKickStartTile.HasValue)
            {
                draw_x = Utility.Lerp(__instance.localKickStartTile.Value.X, draw_x, __instance.kickProgress);
                draw_y = Utility.Lerp(__instance.localKickStartTile.Value.Y, draw_y, __instance.kickProgress);
            }
            
            var globalPosition = new Vector2(draw_x, (int) (draw_y - storage.Depth / 16f - 1f));
            var layerDepth = Math.Max(0.0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
            
            return !storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                Game1.GlobalToLocal(Game1.viewport, globalPosition * 64),
                Vector2.Zero,
                alpha,
                layerDepth
            );
        }
        
        /// <summary>Draw chest with playerChoiceColor and animation when held.</summary>
        private static bool Chest_drawLocal_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, int x, int y, float alpha, bool local)
        {
            if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName) 
                || !_mod.Storages.TryGetValue(storageName, out var storage)
                || storage.Texture == null)
                return true;
            
            storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                new Vector2(x, y - 64),
                Vector2.Zero,
                alpha
            );
            return false;
        }
        
        /// <summary>Draw chest with playerChoiceColor and animation in menu.</summary>
        private static bool Chest_drawInMenu_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName) 
                || !_mod.Storages.TryGetValue(storageName, out var storage))
                return true;
            
            var origin = new Vector2(storage.Width / 2f, storage.Height / 2f);
            var drawScaleSize = scaleSize * storage.ScaleSize;
            var draw = storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                location + new Vector2(32, 32),
                origin,
                transparency,
                layerDepth,
                drawScaleSize
            );
            if (!draw)
                return true;
            
            // Draw Stack
            if (__instance.Stack > 1)
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize) - 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            
            // Draw Held Items
            var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
            if (items > 0)
                Utility.drawTinyDigits(items, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - 3f * scaleSize, 2f * scaleSize), 3f * scaleSize, 1f, color);
            return false;
        }
        #endregion
        
        #region ObjectPatches
        private static bool Object_checkForAction_prefix(SObject __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {
            if (justCheckingForActivity
                || !Game1.didPlayerJustRightClick(true)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out _)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/X", out var xStr)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var yStr)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos)
                || !Game1.currentLocation.Objects.TryGetValue(new Vector2(xPos, yPos), out var obj)
                || obj is not Chest chest) return true;
            __result = chest.checkForAction(who);
            return false;
        }
        
        private static bool Object_draw_prefix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out _)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/X", out var xStr)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var yStr)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos))
                return true;
            return xPos == x && yPos == y;
        }
        
        private static bool Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName) 
                || !_mod.Storages.TryGetValue(storageName, out var storage))
                return true;
            
            objectPosition.X -= storage.Width * 2f - 32;
            objectPosition.Y -= storage.Height * 2f - 64;
            
            return !storage.Draw(__instance, 0, spriteBatch, objectPosition, Vector2.Zero);
        }
        
        private static bool Object_drawPlacementBounds_prefix(SObject __instance, SpriteBatch spriteBatch, GameLocation location)
        {
            
            if (!_mod.Storages.TryGetValue(__instance.Name, out var storage))
                return true;
            
            var tile = 64 * Game1.GetPlacementGrabTile();
            var x = (int) tile.X;
            var y = (int) tile.Y;
            
            Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
            if (Game1.isCheckingNonMousePlacement)
            {
                var pos = Utility.GetNearbyValidPlacementPosition(Game1.player, location, __instance, x, y);
                x = (int) pos.X;
                y = (int) pos.Y;
            }
            
            var canPlaceHere = Utility.playerCanPlaceItemHere(location, __instance, x, y, Game1.player)
                               || Utility.isThereAnObjectHereWhichAcceptsThisItem(location, __instance, x, y)
                               && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player);
            
            Game1.isCheckingNonMousePlacement = false;
            
            storage.ForEachPos(x / 64, y / 64, delegate(Vector2 pos)
            {
                spriteBatch.Draw(Game1.mouseCursors,
                    pos * 64 - new Vector2(Game1.viewport.X, Game1.viewport.Y),
                    new Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    4f,
                    SpriteEffects.None,
                    0.01f);
            });
            
            var globalPosition = new Vector2((int) (x / 64f), (int) (y / 64f - storage.Depth / 16f - 1f));
            return !storage.Draw(__instance, 0, spriteBatch, Game1.GlobalToLocal(Game1.viewport, globalPosition * 64), Vector2.Zero, 0.5f);
        }
        
        private static bool Object_performToolAction_prefix(SObject __instance, Tool t, GameLocation location)
        {
            if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/X", out var xStr)
                || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var yStr)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos)
                || !location.Objects.TryGetValue(new Vector2(xPos, yPos), out var obj)
                || obj == __instance
                || obj is not Chest chest)
                return true;
            return chest.performToolAction(t, location);
        }
        #endregion
        
        #region UtilityPatches
        private static void Utility_playerCanPlaceItemHere_postfix(ref bool __result, GameLocation location, Item item, int x, int y, Farmer f)
        {
            if (!_mod.Storages.TryGetValue(item.Name, out var storage))
                return;
            
            x = 64 * (x / 64);
            y = 64 * (y / 64);
            
            if (Utility.isPlacementForbiddenHere(location) || Game1.eventUp || f.bathingClothes.Value || f.onBridge.Value)
            {
                __result = false;
                return;
            }
            
            // Is Within Tile With Leeway
            if (!Utility.withinRadiusOfPlayer(x, y, Math.Max(storage.TileWidth, storage.TileHeight), f))
            {
                __result = false;
                return;
            }
            
            // Position intersects with farmer
            var rect = new Rectangle(x, y, storage.TileWidth * 64, storage.TileHeight * 64);
            if (location.farmers.Any(farmer => farmer.GetBoundingBox().Intersects(rect)))
            {
                __result = false;
                return;
            }
            
            // Is Close Enough to Farmer
            rect.Inflate(32, 32);
            if (!rect.Intersects(f.GetBoundingBox()))
            {
                __result = false;
                return;
            }
            
            for (var i = 0; i < storage.TileWidth; i++)
            {
                for (var j = 0; j < storage.TileHeight; j++)
                {
                    var tileLocation = new Vector2(x / 64 + i, y / 64 + j);
                    if (item.canBePlacedHere(location, tileLocation)
                        && location.getObjectAtTile((int) tileLocation.X, (int) tileLocation.Y) == null
                        && location.isTilePlaceable(tileLocation, item))
                        continue;
                    
                    // Item cannot be placed here
                    __result = false;
                    return;
                }
            }
            
            __result = true;
        }
        #endregion
    }
}