using System.Collections.Generic;
using ExpandedStorage.API;

namespace ExpandedStorage.Framework.Models
{
    public class TabModel : ITab
    {
        public string TabImage { get; set; }
        public HashSet<string> AllowList { get; set; } = new();
        public HashSet<string> BlockList { get; set; } = new();
    }
}