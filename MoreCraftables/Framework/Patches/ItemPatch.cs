﻿using System.Collections.Generic;
using System.Linq;
using Harmony;
using MoreCraftables.Framework.Models;
using Common.PatternPatches;
using StardewModdingAPI;
using StardewValley;

// ReSharper disable InconsistentNaming

namespace MoreCraftables.Framework.Patches
{
    public class ItemPatch : Patch<ModConfig>
    {
        private static IList<HandledTypeWrapper> _handledTypes;

        public ItemPatch(IMonitor monitor, ModConfig config, IList<HandledTypeWrapper> handledTypes)
            : base(monitor, config)
        {
            _handledTypes = handledTypes;
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
                prefix: new HarmonyMethod(GetType(), nameof(CanStackWithPrefix))
            );
        }

        public static bool CanStackWithPrefix(Item __instance, ref bool __result, ISalable other)
        {
            // Verify this is a handled item type
            var handledType = _handledTypes.FirstOrDefault(t => t.HandledType.IsHandledItem(__instance));
            if (handledType == null)
                return true;

            // Verify instance is Object
            if (__instance is not Object obj)
                return true;
            
            // Verify other item is Object
            if (other is not Object otherObj)
                return true;

            // Yield return to handled type
            __result = __instance.maximumStackSize() > 1
                       && other.maximumStackSize() > 1
                       && obj.ParentSheetIndex == otherObj.ParentSheetIndex
                       && obj.bigCraftable.Value == otherObj.bigCraftable.Value
                       && obj.Quality == otherObj.Quality
                       && handledType.HandledType.CanStackWith(__instance, (Item) other);
            return false;
        }
    }
}