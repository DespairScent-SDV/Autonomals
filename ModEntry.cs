using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewModdingAPI.Events;

namespace DespairScent.Autonomals
{
    internal sealed class ModEntry : Mod
    {

        private static ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.updateWhenNotCurrentLocation)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(PatchFarmAnimalsUpdate)));

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                return;
            }

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Allowed locations",
                tooltip: () => "Locations on which the mod will have effect. Change only if really necessary.",
                allowedValues: new string[] {
                    ModConfig.LOCATIONS_ANY,
                    ModConfig.LOCATIONS_BUILDABLE_ONLY,
                    ModConfig.LOCATIONS_FARM_ONLY
                },
                getValue: () => Config.AllowedLocations,
                setValue: value => Config.AllowedLocations = value
            );
        }

        private static bool PatchFarmAnimalsUpdate(FarmAnimal __instance, Building currentBuilding, GameTime time, GameLocation environment)
        {
            if (currentBuilding == null && Config.AllowedLocations switch
            {
                ModConfig.LOCATIONS_BUILDABLE_ONLY => environment.IsBuildableLocation(),
                ModConfig.LOCATIONS_FARM_ONLY => environment == Game1.getFarm(),
                _ => true
            })
            {
                __instance.updateWhenCurrentLocation(time, environment);
                return false;
            }
            return true;
        }

    }
}
