using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace XSLite
{
    public class XSLiteAPI : IXSLiteAPI
    {
        private static readonly HashSet<string> VanillaNames = new()
        {
            "Chest",
            "Stone Chest",
            "Junimo Chest",
            "Mini-Shipping Bin",
            "Mini-Fridge",
            "Auto-Grabber"
        };
        
        private readonly XSLite _mod;
        internal XSLiteAPI(XSLite mod)
        {
            _mod = mod;
        }
        public bool LoadContentPack(string path)
        {
            var temp = _mod.Helper.ContentPacks.CreateFake(path);
            var info = temp.ReadJsonFile<ContentPack>("content-pack.json");
            if (info == null)
            {
                _mod.Monitor.Log($"Cannot read content-pack.json from {path}", LogLevel.Warn);
                return false;
            }
            
            var contentPack = _mod.Helper.ContentPacks.CreateTemporary(
                path,
                info.UniqueID,
                info.Name,
                info.Description,
                info.Author,
                new SemanticVersion(info.Version));
            return LoadContentPack(contentPack);
        }
        
        public bool LoadContentPack(IContentPack contentPack)
        {
            _mod.Monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
            
            var storages = contentPack.ReadJsonFile<IDictionary<string, Storage>>("expanded-storage.json");
            if (storages == null)
            {
                _mod.Monitor.Log($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                return false;
            }
            
            // Load expanded storages
            foreach (var storage in storages)
            {
                // Skip duplicate storages
                if (_mod.Storages.ContainsKey(storage.Key))
                {
                    _mod.Monitor.Log($"Duplicate storage {storage.Key} in {contentPack.Manifest.UniqueID}.", LogLevel.Warn);
                    continue;
                }
                
                // Skip vanilla storages
                if (VanillaNames.Contains(storage.Key))
                {
                    _mod.Monitor.LogOnce("XSLite does not load vanilla storages.", LogLevel.Warn);
                    continue;
                }
                
                storage.Value.Name = storage.Key;
                
                if (!string.IsNullOrWhiteSpace(storage.Value.Image) && contentPack.HasFile($"assets/{storage.Value.Image}"))
                {
                    storage.Value.Texture = contentPack.LoadAsset<Texture2D>($"assets/{storage.Value.Image}");
                }
                else
                {
                    // Load placeholder texture
                    storage.Value.Texture = _mod.Helper.Content.Load<Texture2D>("assets/texture.png");
                }
                
                _mod.Storages.Add(storage.Key, storage.Value);
            }
            
            // Generate content-pack.json for Json Assets
            contentPack.WriteJsonFile("content-pack.json", new ContentPack
            {
                Author = contentPack.Manifest.Author,
                Description = contentPack.Manifest.Description,
                Name = contentPack.Manifest.Name,
                UniqueID = contentPack.Manifest.UniqueID,
                Version = contentPack.Manifest.Version.ToString()
            });
            
            _mod.JsonAssets.API.LoadAssets(contentPack.DirectoryPath);
            return true;
        }

        public bool AcceptsItem(Chest chest, Item item)
        {
            return true;
        }
    }
}