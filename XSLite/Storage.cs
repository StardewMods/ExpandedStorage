using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace ImJustMatt.XSLite
{
    internal class Storage
    {
        #region ContentModel
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable CollectionNeverUpdated.Global
        public string SpecialChestType { get; set; } = "None";
        public string Image { get; set; }
        public int Frames { get; set; } = 5;
        public string Animation { get; set; } = "None";
        public int Delay { get; set; } = 5;
        public bool PlayerColor { get; set; } = true;
        public int Depth { get; set; } = 0;
        public IDictionary<string, string> ModData { get; set; } = new Dictionary<string, string>();
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore once UnusedAutoPropertyAccessor.Global
        // ReSharper restore CollectionNeverUpdated.Global
        #endregion

        internal string Name
        {
            get => _name;
            set
            {
                _name = value;
                _path = PathUtilities.NormalizePath($"Mods/furyx639.ExpandedStorage/SpriteSheets/{_name}");
            }
        }
        internal Texture2D Texture;
        internal int Id;
        internal int Width { get; private set; }
        internal int Height { get; private set; }
        internal int TileWidth { get; private set; }
        internal int TileHeight { get; private set; }
        internal float ScaleSize { get; private set; }

        private string _name;
        private string _path;
        private Texture2D _texture;
        
        internal void InvalidateCache(IContentHelper contentHelper)
        {
            _texture = Texture != null ? contentHelper.Load<Texture2D>(_path, ContentSource.GameContent) : null;
            if (_texture == null)
                return;
            
            Width = _texture.Width / Math.Max(1, Frames);
            Height = PlayerColor ? _texture.Height / 3 : _texture.Height;
            TileWidth = Width / 16;
            TileHeight = (Depth > 0 ? Depth : Height - 16) / 16;
            
            var tilesWide = Width / 16f;
            var tilesHigh = Height / 16f;
            ScaleSize = tilesWide switch
            {
                >= 7 => 0.5f,
                >= 6 => 0.66f,
                >= 5 => 0.75f,
                _ => tilesHigh switch
                {
                    >= 5 => 0.8f,
                    >= 3 => 1f,
                    _ => tilesWide switch
                    {
                        <= 2 => 2f,
                        <= 4 => 1f,
                        _ => 0.1f
                    }
                }
            };
        }
        
        internal void ForEachPos(int x, int y, Action<Vector2> doAction)
        {
            for (var i = 0; i < TileWidth; i++)
            {
                for (var j = 0; j < TileHeight; j++)
                {
                    var pos = new Vector2(x + i, y + j);
                    doAction.Invoke(pos);
                }
            }
        }
        
        internal bool Draw(SObject obj, int currentFrame, SpriteBatch spriteBatch, Vector2 pos, Vector2 origin, float alpha = 1f, float layerDepth = 0.89f, float scaleSize = 4f)
        {
            if (_texture == null)
                return false;
            
            var chest = obj as Chest;
            currentFrame -= chest?.startingLidFrame.Value ?? 0;
            var drawColored = PlayerColor && chest != null && !chest.playerChoiceColor.Value.Equals(Color.Black);
            var startLayer = drawColored && PlayerColor ? 1 : 0;
            var endLayer = startLayer == 0 ? 1 : 3;
            for (var layer = startLayer; layer < endLayer; layer++)
            {
                var color = Animation == "Color"
                    ? XSLite.ColorWheel.ToRgbColor()
                    : (layer % 2 == 0 || !drawColored) && chest != null
                        ? chest.Tint
                        : chest?.playerChoiceColor.Value ?? Color.White;
                
                spriteBatch.Draw(_texture,
                    pos + ShakeOffset(obj, -1, 2),
                    new Rectangle(Width * currentFrame, Height * layer, Width, Height),
                    color * alpha,
                    0f,
                    origin,
                    scaleSize,
                    SpriteEffects.None,
                    layerDepth + (1 + layer - startLayer) * 1E-05f);
            }
            return true;
        }
        
        internal void Replace(GameLocation location, Vector2 pos, SObject obj)
        {
            if (obj.modData.TryGetValue("furyx639.ExpandedStorage/X", out var xStr)
                && obj.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var yStr)
                && int.TryParse(xStr, out var xPos)
                && int.TryParse(yStr, out var yPos)
                && !pos.Equals(new Vector2(xPos, yPos)))
                return;

            if (obj is not Chest chest)
            {
                chest = new Chest(true, pos, Id);
                
                // Copy modData from original item
                foreach (var modData in obj.modData)
                    chest.modData.CopyFrom(modData);
                
                // Copy modData from config
                foreach (var modData in ModData)
                {
                    if (!chest.modData.ContainsKey(modData.Key))
                        chest.modData.Add(modData.Key, modData.Value);
                }
                
                // Replace object with chest
                location.Objects.Remove(pos);
                location.Objects.Add(pos, chest);
            }
            
            chest.Name = Name;
            chest.lidFrameCount.Value = Frames;
            chest.modData["furyx639.ExpandedStorage/Storage"] = Name;
            chest.modData["furyx639.ExpandedStorage/X"] = pos.X.ToString(CultureInfo.InvariantCulture);
            chest.modData["furyx639.ExpandedStorage/Y"] = pos.Y.ToString(CultureInfo.InvariantCulture);
            
            if (TileHeight == 1 && TileWidth == 1)
                return;
            
            // Add objects for extra Tile spaces
            ForEachPos((int) pos.X, (int) pos.Y, innerPos =>
            {
                if (innerPos.Equals(pos) || location.Objects.ContainsKey(innerPos))
                    return;
                
                var addObj = new SObject(Vector2.Zero, Id)
                {
                    name = Name
                };
                
                // Copy modData from original item
                foreach (var modData in chest.modData)
                    addObj.modData.CopyFrom(modData);
                
                location.Objects.Add(innerPos, addObj);
            });
        }
        
        internal void Remove(GameLocation location, Vector2 pos, SObject obj)
        {
            if (TileHeight == 1 && TileWidth == 1
                || !obj.modData.TryGetValue("furyx639.ExpandedStorage/X", out var xStr)
                || !obj.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var yStr)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos))
                return;
            
            ForEachPos(xPos, yPos, innerPos =>
            {
                if (innerPos.Equals(pos)
                    || !location.Objects.TryGetValue(innerPos, out var innerObj)
                    || !innerObj.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName)
                    || storageName != Name)
                    return;
                location.Objects.Remove(innerPos);
            });
        }
        
        private static Vector2 ShakeOffset(SObject instance, int minValue, int maxValue)
        {
            return instance.shakeTimer > 0
                ? new Vector2(Game1.random.Next(minValue, maxValue), 0)
                : Vector2.Zero;
        }
    }
}