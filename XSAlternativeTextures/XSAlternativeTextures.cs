using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlternativeTextures.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace XSAlternativeTextures
{
    public class XSAlternativeTextures : Mod, IAssetEditor
    {
        private static readonly string AlternativeTexturesPath = PathUtilities.NormalizePath("AlternativeTextures/Textures");
        private static readonly string ExpandedStoragePath = PathUtilities.NormalizePath("Mods/furyx639.ExpandedStorage/SpriteSheets");
        private static readonly IList<string> Storages = new List<string>();
        
        private static IAlternativeTexturesAPI _alternativeTexturesAPI;
        private static IExpandedStorageAPI _expandedStorageAPI;
        
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }
        
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _alternativeTexturesAPI = Helper.ModRegistry.GetApi<IAlternativeTexturesAPI>("PeacefulEnd.AlternativeTextures");
            _expandedStorageAPI = Helper.ModRegistry.GetApi<IExpandedStorageAPI>("furyx639.ExpandedStorage");
            
            var texturePath = Path.Combine(Helper.DirectoryPath, "assets", "texture.png");
            var model = new AlternativeTextureModel
            {
                ItemName = "Chest",
                Type = "Craftable",
                TextureWidth = 16,
                TextureHeight = 32,
                Variations = 1,
                Seasons = new List<string>()
            };
            foreach (var storageName in _expandedStorageAPI.GetAllStorages())
            {
                Storages.Add(storageName);
                model.Keywords = new List<string> { storageName };
                _alternativeTexturesAPI.AddAlternativeTexture(model, "furyx639", texturePath);
            }
        }
        
        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            var assetName = PathUtilities.NormalizePath(asset.AssetName);
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAtOrDefault(2);
            if (!assetName.StartsWith(AlternativeTexturesPath, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(storageName)
                || !Storages.Contains(storageName))
                return false;
            var texture = Helper.Content.Load<Texture2D>(Path.Combine(ExpandedStoragePath, storageName), ContentSource.GameContent);
            return texture.Width == 80 && texture.Height == 96;
        }
        
        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAtOrDefault(2);
            if (storageName == null)
                return;
            var editor = asset.AsImage();
            var texture = Helper.Content.Load<Texture2D>(Path.Combine(ExpandedStoragePath, storageName), ContentSource.GameContent);
            editor.PatchImage(texture, new Rectangle(0, 0, 16, 32), new Rectangle(0,  0, 16, 32));
            editor.PatchImage(texture, new Rectangle(0, 0, 80, 32), new Rectangle(16,  0, 80, 32));
        }
    }
}