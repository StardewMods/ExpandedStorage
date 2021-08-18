using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Common.Helpers;
using Common.Integrations.JsonAssets;
using SObject = StardewValley.Object;

namespace XSLite
{
    public class XSLite : Mod, IAssetLoader, IAssetEditor
    {
        private const int PlayerColorOffset = 6;
        private const int BraceOffset = 12;
        
        private static readonly string ExpandedStoragePath = PathUtilities.NormalizePath("Mods/furyx639.ExpandedStorage/SpriteSheets");
        internal static readonly HSLColor ColorWheel = new();
        internal JsonAssetsIntegration JsonAssets;
        internal readonly IDictionary<string, Storage> Storages = new Dictionary<string, Storage>();
        
        private XSLiteAPI _api;
        private readonly HashSet<int> _objectIds = new();
        
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            JsonAssets = new JsonAssetsIntegration(Helper.ModRegistry);
            
            _api = new XSLiteAPI(this);
            var patches = new Patches(this, new Harmony(ModManifest.UniqueID));
            
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.World.ObjectListChanged += OnObjectListChanged;
        }
        
        /// <inheritdoc />
        public override object GetApi()
        {
            return _api;
        }
        
        /// <summary>Load Expanded Storage content packs</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets.API.IdsAssigned += OnIdsLoaded;
            
            Monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (var contentPack in Helper.ContentPacks.GetOwned())
            {
                _api.LoadContentPack(contentPack);
            }
        }
        
        /// <summary>Invalidate sprite cache for storages each in-game day</summary>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (var storage in Storages.Values)
            {
                storage.InvalidateCache(Helper.Content);
            }
        }
        
        /// <summary>Replace Expanded Storages with modded Chest</summary>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.IsCurrentLocation)
                return;
            
            foreach (var (pos, obj) in e.Added)
            {
                if (TryGetStorage(obj, out var storage))
                    storage.Replace(e.Location, pos, obj);
            }
            
            foreach (var (pos, obj) in e.Removed)
            {
                if (TryGetStorage(obj, out var storage))
                    storage.Remove(e.Location, pos, obj);
            }
        }
        
        /// <summary>Load Json Asset IDs for Expanded Storage objects</summary>
        private void OnIdsLoaded(object sender, EventArgs e)
        {
            _objectIds.Clear();
            foreach (var (storageName, storage) in Storages)
            {
                var objectId = JsonAssets.API.GetBigCraftableId(storageName);
                if (objectId != -1) _objectIds.Add(objectId);
                storage.Id = objectId;
            }
        }
        
        internal bool TryGetStorage(SObject obj, out Storage storage)
        {
            if (!obj.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName))
                storageName = obj.Name;
            return Storages.TryGetValue(storageName, out storage) && obj.bigCraftable.Value;
        }
        
        /// <inheritdoc />
        public bool CanLoad<T>(IAssetInfo asset)
        {
            var assetName = PathUtilities.NormalizePath(asset.AssetName);
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAtOrDefault(3);
            return assetName.StartsWith(ExpandedStoragePath)
                   && storageName != null
                   && Storages.ContainsKey(storageName);
        }
        
        /// <inheritdoc />
        public T Load<T>(IAssetInfo asset)
        {
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAtOrDefault(3);
            if (!string.IsNullOrWhiteSpace(storageName) && Storages.TryGetValue(storageName, out var storage))
                return (T) (object) storage.Texture;
            return (T) (object) null;
        }
        
        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            var assetName = PathUtilities.NormalizePath(asset.AssetName);
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAtOrDefault(3);
            return assetName.StartsWith(ExpandedStoragePath)
                   && storageName != null
                   && Storages.TryGetValue(storageName, out var storage)
                   && string.IsNullOrWhiteSpace(storage.Image);
        }
        
        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAtOrDefault(3);
            if (storageName == null || !Storages.TryGetValue(storageName, out var storage))
                return;
            var editor = asset.AsImage();
            for (var frame = 0; frame < 5; frame++)
            {
                for (var layer = 0; layer < 3; layer++)
                {
                    // Base Layer
                    var sourceArea = Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, storage.Id + layer * 6, 16, 32);
                    var targetArea = new Rectangle(frame * 16, layer * 32, 16, 32);
                    editor.PatchImage(Game1.bigCraftableSpriteSheet, sourceArea, targetArea);
                    
                    // Lid Layer
                    sourceArea = Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, storage.Id + layer * 6 + frame + 1, 16, 32);
                    sourceArea.Height = 21;
                    targetArea.Height = 21;
                    editor.PatchImage(Game1.bigCraftableSpriteSheet, sourceArea, targetArea);
                }
            }
        }
    }
}