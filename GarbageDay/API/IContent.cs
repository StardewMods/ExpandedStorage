using System.Collections.Generic;

namespace GarbageDay.API
{
    public interface IContent
    {
        /// <summary>Items to add to a Loot table</summary>
        IDictionary<string, IDictionary<string, double>> Loot { get; set; }
    }
}