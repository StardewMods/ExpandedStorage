using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ImJustMatt.Common.Integrations.JsonAssets;
using ImJustMatt.Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace ImJustMatt.XSLite
{
    public class XSLite : Mod, IAssetLoader
    {
        internal static readonly HSLColor ColorWheel = new();
        internal JsonAssetsIntegration JsonAssets;
        internal readonly IDictionary<string, Storage> Storages = new Dictionary<string, Storage>();
        
        private XSLiteAPI _api;
        private readonly HashSet<int> _objectIds = new();
        
        public override void Entry(IModHelper helper)
        {
            JsonAssets = new JsonAssetsIntegration(Helper.ModRegistry);
            
            _api = new XSLiteAPI(this);
            var patches = new Patches(this, new Harmony(ModManifest.UniqueID));
            
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.World.ObjectListChanged += OnObjectListChanged;
        }
        
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
            
            var added = e.Added.LastOrDefault(obj => obj.Value.bigCraftable.Value && _objectIds.Contains(obj.Value.ParentSheetIndex) || obj.Value.modData.ContainsKey("furyx639.ExpandedStorage/Storage"));
            if (added.Value != null && Storages.TryGetValue(added.Value.name, out var storage))
            {
                storage.Replace(e.Location, added.Key, added.Value);
            }
            
            var removed = e.Removed.LastOrDefault(obj => obj.Value.modData.ContainsKey("furyx639.ExpandedStorage/Storage"));
            if (removed.Value != null
                && removed.Value.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName)
                && Storages.TryGetValue(storageName, out storage))
            {
                storage.Remove(e.Location, removed.Key, removed.Value);
            }
        }
        
        /// <summary>Load Json Asset IDs for Expanded Storage objects</summary>
        private void OnIdsLoaded(object sender, EventArgs e)
        {
            _objectIds.Clear();
            foreach (var storage in Storages)
            {
                var objectId = JsonAssets.API.GetBigCraftableId(storage.Key);
                if (objectId != -1) _objectIds.Add(objectId);
                storage.Value.Id = objectId;
            }
        }
        
        public bool CanLoad<T>(IAssetInfo asset)
        {
            var assetName = PathUtilities.NormalizePath(asset.AssetName);
            var modPath = PathUtilities.NormalizePath("Mods/furyx639.ExpandedStorage/SpriteSheets");
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAtOrDefault(3);
            return assetName.StartsWith(modPath)
                   && storageName != null
                   && Storages.TryGetValue(storageName, out var storage)
                   && storage.Texture != null;
        }
        
        public T Load<T>(IAssetInfo asset)
        {
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAtOrDefault(3);
            if (!string.IsNullOrWhiteSpace(storageName) && Storages.TryGetValue(storageName, out var storage))
                return (T) (object) storage.Texture;
            return (T) (object) null;
        }
    }
}