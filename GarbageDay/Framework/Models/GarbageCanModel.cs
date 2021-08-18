using Microsoft.Xna.Framework;
using StardewValley;

namespace GarbageDay.Framework.Models
{
    internal class GarbageCanModel
    {
        internal string MapName { get; set; }
        internal string MapLoot { get; set; }
        internal GameLocation Location { get; set; }
        internal Vector2 Tile { get; set; }
        internal string WhichCan { get; set; }
    }
}