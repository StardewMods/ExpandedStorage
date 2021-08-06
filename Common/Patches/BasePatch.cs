using HarmonyLib;
using StardewModdingAPI;

namespace ImJustMatt.Common.Patches
{
    internal abstract class BasePatch<T> where T : IMod
    {
        private protected static T Mod;

        internal BasePatch(IMod mod, Harmony harmony)
        {
            Mod = (T) mod;
        }

        private protected static IMonitor Monitor => Mod.Monitor;
    }
}