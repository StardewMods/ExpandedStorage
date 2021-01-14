﻿namespace ExpandedStorage.Framework
{
    public class ModConfig
    {
        /// <summary>Whether to allow modded storage to have capacity other than 36 slots.</summary>
        public bool AllowModdedCapacity { get; set; } = true;

        /// <summary>Whether to allow chests to be carried and moved.</summary>
        public bool AllowCarryingChests { get; set; } = true;

        /// <summary>Adds three extra rows to the Inventory Menu.</summary>
        public bool ExpandInventoryMenu { get; set; } = true;
        
        /// <summary>Adds clickable arrows to indicate when there are more items in the chest.</summary>
        public bool ShowOverlayArrows { get; set; } = true;

        /// <summary>Allows filtering Inventory Menu by searching for the the item name.</summary>
        public bool ShowSearchBar { get; set; } = true;

        /// <summary>Control scheme for Expanded Storage features.</summary>
        public ModConfigControlsRaw ControlsRaw { get; set; } = new ModConfigControlsRaw();
    }
}