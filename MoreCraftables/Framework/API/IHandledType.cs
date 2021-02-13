﻿using System.Collections.Generic;
using StardewValley;

// ReSharper disable UnusedMember.Global

namespace MoreCraftables.Framework.API
{
    public interface IHandledType
    {
        string Type { get; set; }

        IDictionary<string, object> Properties { get; set; }

        bool IsHandledItem(Item item);

        bool CanStackWith(Item item, Item otherItem);
    }
}