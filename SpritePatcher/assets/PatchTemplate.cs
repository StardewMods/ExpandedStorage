namespace #REPLACE_namespace;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using IHaveModData = StardewValley.IHaveModData;
using SObject = StardewValley.Object;

public class Runner : global::StardewMods.SpritePatcher.Framework.BasePatchModel
{
    public Runner(PatchModelCtorArgs args) : base(args) { }

    public override bool Run(ISprite sprite)
    {
        this.BeforeRun(sprite);
        try
        {
            this.ActualRun(sprite.Entity);
        }
        catch
        {
            this.AfterRun(sprite);
            throw;
        }
        
        return this.AfterRun(sprite);
    }

    public void ActualRun(IHaveModData entity)
    {
#REPLACE_code
    }
}