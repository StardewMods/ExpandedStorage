using System.Collections.Generic;
using GarbageDay.API;

namespace GarbageDay.Framework.Models
{
    internal class ContentModel : IContent
    {
        public IDictionary<string, IDictionary<string, double>> Loot { get; set; } = new Dictionary<string, IDictionary<string, double>>();
    }
}