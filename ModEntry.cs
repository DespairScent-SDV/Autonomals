using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;

namespace DespairScent.Autonomals
{
    internal sealed class ModEntry : Mod
    {
        private static ModEntry Mod;

        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            Mod = this;

            Mod.Config = helper.ReadConfig<ModConfig>();

            var harmony = new Harmony(Mod.ModManifest.UniqueID);

            harmony.Patch(AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.updateWhenNotCurrentLocation)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(PatchUpdateWhenNotCurrentLocation)));

            harmony.Patch(AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.updateWhenCurrentLocation)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(PatchUpdateWhenCurrentLocation)));

            helper.Events.GameLoop.GameLaunched += Mod.OnGameLaunched;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu == null)
            {
                return;
            }

            configMenu.Register(
                mod: Mod.ModManifest,
                reset: () => Mod.Config = new ModConfig(),
                save: () => Helper.WriteConfig(Mod.Config)
            );

            configMenu.AddBoolOption(
                mod: Mod.ModManifest,
                name: () => "Enabled",
                tooltip: () => "Toggles mod functionality",
                getValue: () => Mod.Config.Enabled,
                setValue: value => Mod.Config.Enabled = value
            );
            configMenu.AddTextOption(
                mod: Mod.ModManifest,
                name: () => "Allowed locations",
                tooltip: () => "Locations on which the mod will have effect. Change only if really necessary.",
                allowedValues: new string[] {
                    ModConfig.LOCATIONS_ANY,
                    ModConfig.LOCATIONS_BUILDABLE_ONLY,
                    ModConfig.LOCATIONS_FARM_ONLY
                },
                getValue: () => Mod.Config.AllowedLocations,
                setValue: value => Mod.Config.AllowedLocations = value
            );
        }


        private static HashSet<long> _updatedAnimals = new();
        private static TimeSpan _updatedAnimalsAt;

        private static bool PatchUpdateWhenNotCurrentLocation(FarmAnimal __instance, Building currentBuilding, GameTime time, GameLocation environment)
        {
            if (currentBuilding == null && Mod.Config.Enabled && Mod.Config.AllowedLocations switch
            {
                ModConfig.LOCATIONS_BUILDABLE_ONLY => environment.IsBuildableLocation(),
                ModConfig.LOCATIONS_FARM_ONLY => environment == Game1.getFarm(),
                _ => true
            })
            {
                if (!(time.TotalGameTime.Equals(_updatedAnimalsAt) && _updatedAnimals.Contains(__instance.myID.Value)))
                {
                    __instance.updateWhenCurrentLocation(time, environment);
                    return false;
                }
            }
            return true;
        }

        private static bool PatchUpdateWhenCurrentLocation(FarmAnimal __instance, GameTime time, GameLocation location)
        {
            if (!time.TotalGameTime.Equals(_updatedAnimalsAt))
            {
                _updatedAnimalsAt = time.TotalGameTime;
                _updatedAnimals.Clear();
            }
            _updatedAnimals.Add(__instance.myID.Value);
            return true;
        }

    }
}
