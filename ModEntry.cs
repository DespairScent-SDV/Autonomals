using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Locations;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace DespairScent.Autonomals
{
    internal sealed class ModEntry : Mod
    {

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.updateWhenNotCurrentLocation)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(PatchFarmAnimalsUpdate)));
        }

        private static bool PatchFarmAnimalsUpdate(FarmAnimal __instance, Building currentBuilding, GameTime time, GameLocation environment)
        {
            if (environment == Game1.getFarm() && currentBuilding == null)
            {
                __instance.updateWhenCurrentLocation(time, environment);
                return false;
            }
            return true;
        }

    }
}
